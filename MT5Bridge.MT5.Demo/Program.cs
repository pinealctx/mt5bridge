using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using CommandLine;
using MetaQuotes.MT5CommonAPI;
using Microsoft.Extensions.Configuration;
using MT5Bridge.Core.Logging;
using MT5Bridge.Logging.NLog;
using MT5Bridge.MT5.Core;
using MT5Bridge.MT5.Core.Managers;
using NLog;

namespace MT5Bridge.MT5.Demo;

public class Program
{
    [ModuleInitializer]
    public static void RegisterAssemblyResolver()
    {
        // Ensure current directory is the same as the executable directory
        // This helps in finding config files and DLLs when run from different locations
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        if (Directory.GetCurrentDirectory() != baseDirectory)
        {
            Directory.SetCurrentDirectory(baseDirectory);
        }

        // Register assembly resolver for MT5 SDK DLLs
        // Since they are marked as Private=false in csproj, they are not in deps.json
        // and need manual resolution when published as a single file.
        AssemblyLoadContext.Default.Resolving += OnAssemblyResolve;
    }

    private static Assembly? OnAssemblyResolve(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        if (assemblyName.Name != null && assemblyName.Name.StartsWith("MetaQuotes.MT5"))
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{assemblyName.Name}.dll");
            if (File.Exists(path))
            {
                return context.LoadFromAssemblyPath(path);
            }
        }
        return null;
    }

    public class Options
    {
        [Option('s', "server", Required = false, HelpText = "MT5 server address (e.g., localhost:443)")]
        public string? Server { get; set; }

        [Option('l', "login", Required = false, HelpText = "Manager login")]
        public ulong? Login { get; set; }

        [Option('p', "password", Required = false, HelpText = "Manager password")]
        public string? Password { get; set; }

        [Option('t', "test", Required = false, HelpText = "Test to run: groups, user, account, deposit, deals, listen")]
        public string? Test { get; set; }

        [Option('u', "user-login", HelpText = "User login for user/account/deposit/deals tests")]
        public ulong? UserLogin { get; set; }
    }

    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== MT5Bridge Test Console ===\n");

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        // Configure logging
        var loggerFactory = new NLogLoggerFactory();
        var logger = loggerFactory.CreateLogger("MT5TestConsole");

        return await Parser.Default.ParseArguments<Options>(args)
            .MapResult(
                async (Options opts) => await RunTestAsync(opts, logger, configuration),
                errs => Task.FromResult(1));
    }

    private static async Task<int> RunTestAsync(Options options, MT5Bridge.Core.Logging.ILogger logger, IConfiguration configuration)
    {
        IMT5Manager? manager = null;

        try
        {
            // Merge configuration: command line > appsettings.json
            var server = options.Server ?? configuration["MT5Connection:Server"];
            var login = options.Login ?? ulong.Parse(configuration["MT5Connection:Login"] ?? "0");
            var password = options.Password ?? configuration["MT5Connection:Password"];
            var test = options.Test ?? configuration["Demo:DefaultTest"] ?? "groups";
            var userLogin = options.UserLogin ?? (ulong.TryParse(configuration["Demo:DefaultUserLogin"], out var ul) ? ul : null);
            var timeoutMs = int.Parse(configuration["MT5Connection:TimeoutMs"] ?? "30000");

            // Validate required parameters
            if (string.IsNullOrEmpty(server) || login == 0 || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("❌ Error: Server, Login, and Password are required.");
                Console.WriteLine("   Specify them via:");
                Console.WriteLine("   1. Command line: --server <addr> --login <id> --password <pwd>");
                Console.WriteLine("   2. appsettings.json: MT5Connection section");
                return 1;
            }

            // Create connection settings
            var settings = new MT5ConnectionSettings
            {
                Server = server,
                Login = login,
                Password = password,
                TimeoutMs = (uint)timeoutMs,
                PumpMode = PumpMode.Full
            };

            // Update options with merged values for test execution
            options.Test = test;
            options.UserLogin = userLogin;

            // Create manager
            manager = new MT5Manager(logger);

            // Subscribe to connection state changes
            manager.ConnectionStateChanged += (sender, e) =>
            {
                Console.WriteLine($"Connection: {e.OldState} -> {e.NewState}");
                if (e.Message != null)
                {
                    Console.WriteLine($"  Message: {e.Message}");
                }
            };

            // Connect
            Console.WriteLine($"Connecting to {server}...");
            var connectResult = await manager.ConnectAsync(settings);

            if (!connectResult.IsSuccess)
            {
                Console.WriteLine($"❌ Connection failed: {connectResult.Message}");
                return 1;
            }

            Console.WriteLine("✅ Connected successfully\n");

            // Run test
            return await RunSpecificTestAsync(manager, options, logger);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            logger.Error("Test error", ex);
            return 1;
        }
        finally
        {
            if (manager != null)
            {
                await manager.DisconnectAsync();
                manager.Dispose();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }

    private static async Task<int> RunSpecificTestAsync(IMT5Manager manager, Options options, MT5Bridge.Core.Logging.ILogger logger)
    {
        switch (options.Test?.ToLower() ?? "groups")
        {
            case "groups":
                return await TestGroupsAsync(manager);

            case "user":
                if (!options.UserLogin.HasValue)
                {
                    Console.WriteLine("❌ --user-login is required for user test");
                    return 1;
                }
                return await TestUserAsync(manager, options.UserLogin.Value);

            case "account":
                if (!options.UserLogin.HasValue)
                {
                    Console.WriteLine("❌ --user-login is required for account test");
                    return 1;
                }
                return await TestAccountAsync(manager, options.UserLogin.Value);

            case "deposit":
                if (!options.UserLogin.HasValue)
                {
                    Console.WriteLine("❌ --user-login is required for deposit test");
                    return 1;
                }
                return await TestDepositAsync(manager, options.UserLogin.Value);

            case "deals":
                if (!options.UserLogin.HasValue)
                {
                    Console.WriteLine("❌ --user-login is required for deals test");
                    return 1;
                }
                return await TestDealsAsync(manager, options.UserLogin.Value);

            case "listen":
                return await TestListenAsync(manager);

            default:
                Console.WriteLine($"❌ Unknown test: {options.Test}");
                Console.WriteLine("Available tests: groups, user, account, deposit, deals, listen");
                return 1;
        }
    }

    private static async Task<int> TestListenAsync(IMT5Manager manager)
    {
        Console.WriteLine("\n=== Test: Listen to Trade Events ===\n");
        Console.WriteLine("Listening for deals, orders, and positions... (Press any key to stop)\n");

        // Subscribe to events
        manager.DealAdded += (s, deal) => Console.WriteLine($"[DEAL ADDED] {deal.Print()}");
        manager.DealUpdated += (s, deal) => Console.WriteLine($"[DEAL UPDATED] {deal.Print()}");
        manager.DealDeleted += (s, deal) => Console.WriteLine($"[DEAL DELETED] {deal.Print()}");

        manager.OrderAdded += (s, order) => Console.WriteLine($"[ORDER ADDED] {order.Print()}");
        manager.OrderUpdated += (s, order) => Console.WriteLine($"[ORDER UPDATED] {order.Print()}");
        manager.OrderDeleted += (s, order) => Console.WriteLine($"[ORDER DELETED] {order.Print()}");

        manager.PositionAdded += (s, pos) => Console.WriteLine($"[POSITION ADDED] {pos.Print()}");
        manager.PositionUpdated += (s, pos) => Console.WriteLine($"[POSITION UPDATED] {pos.Print()}");
        manager.PositionDeleted += (s, pos) => Console.WriteLine($"[POSITION DELETED] {pos.Print()}");

        // Wait for key press
        while (!Console.KeyAvailable)
        {
            await Task.Delay(100);
        }

        Console.ReadKey(true);
        Console.WriteLine("\nStopped listening.");
        return 0;
    }

    private static async Task<int> TestGroupsAsync(IMT5Manager manager)
    {
        Console.WriteLine("=== Test: Get Groups ===\n");

        var result = await manager.GetGroupsAsync();

        if (!result.IsSuccess)
        {
            Console.WriteLine($"❌ Failed: {result.Message}");
            return 1;
        }

        var groups = result.Data!;
        Console.WriteLine($"✅ Found {groups.Total()} groups:\n");

        for (uint i = 0; i < groups.Total(); i++)
        {
            var group = groups.Next(i);
            if (group != null)
            {
                Console.WriteLine($"{i + 1}. {group.Group()}");
                Console.WriteLine($"   Currency: {group.Currency()}");
                Console.WriteLine($"   Leverage: 1:{group.TradeFlags()}");
                Console.WriteLine();
            }
        }

        return 0;
    }

    private static async Task<int> TestUserAsync(IMT5Manager manager, ulong login)
    {
        Console.WriteLine($"=== Test: Get User {login} ===\n");

        var result = await manager.GetUserAsync(login);

        if (!result.IsSuccess)
        {
            Console.WriteLine($"❌ Failed: {result.Message}");
            return 1;
        }

        var user = result.Data!;
        Console.WriteLine($"✅ User information:");
        Console.WriteLine($"   Login: {user.Login()}");
        Console.WriteLine($"   Name: {user.Name()}");
        Console.WriteLine($"   Group: {user.Group()}");
        Console.WriteLine($"   Email: {user.EMail()}");
        Console.WriteLine($"   Registration: {SMTTime.ToDateTime(user.Registration())}");
        Console.WriteLine($"   Last Access: {SMTTime.ToDateTime(user.LastAccess())}");

        return 0;
    }

    private static async Task<int> TestAccountAsync(IMT5Manager manager, ulong login)
    {
        Console.WriteLine($"=== Test: Get Account {login} ===\n");

        var result = await manager.GetAccountAsync(login);

        if (!result.IsSuccess)
        {
            Console.WriteLine($"❌ Failed: {result.Message}");
            return 1;
        }

        var account = result.Data!;
        Console.WriteLine($"✅ Account information:");
        Console.WriteLine($"   Login: {account.Login()}");
        Console.WriteLine($"   Balance: {account.Balance():F2}");
        Console.WriteLine($"   Credit: {account.Credit():F2}");
        Console.WriteLine($"   Equity: {account.Equity():F2}");
        Console.WriteLine($"   Margin: {account.Margin():F2}");
        Console.WriteLine($"   Free Margin: {account.MarginFree():F2}");
        Console.WriteLine($"   Profit: {account.Profit():F2}");

        return 0;
    }

    private static async Task<int> TestDepositAsync(IMT5Manager manager, ulong login)
    {
        Console.WriteLine($"=== Test: Deposit to {login} ===\n");

        // Get balance before
        var accountBefore = await manager.GetAccountAsync(login);
        if (!accountBefore.IsSuccess)
        {
            Console.WriteLine($"❌ Failed to get account: {accountBefore.Message}");
            return 1;
        }

        var balanceBefore = accountBefore.Data!.Balance();
        Console.WriteLine($"Balance before: {balanceBefore:F2}");

        // Deposit
        decimal amount = 100.00m;
        Console.WriteLine($"\nDepositing {amount:F2}...");

        var depositResult = await manager.DepositAsync(login, amount, "Test deposit");
        if (!depositResult.IsSuccess)
        {
            Console.WriteLine($"❌ Deposit failed: {depositResult.Message}");
            return 1;
        }

        Console.WriteLine("✅ Deposit successful");

        // Get balance after
        await Task.Delay(1000); // Wait for update
        var accountAfter = await manager.GetAccountAsync(login);
        if (accountAfter.IsSuccess)
        {
            var balanceAfter = accountAfter.Data!.Balance();
            Console.WriteLine($"Balance after: {balanceAfter:F2}");
            Console.WriteLine($"Difference: {balanceAfter - balanceBefore:F2}");
        }

        return 0;
    }

    private static async Task<int> TestDealsAsync(IMT5Manager manager, ulong login)
    {
        Console.WriteLine($"=== Test: Get Deals for {login} ===\n");

        var from = DateTime.Now.AddDays(-7);
        var to = DateTime.Now;

        Console.WriteLine($"Period: {from:yyyy-MM-dd} to {to:yyyy-MM-dd}\n");

        var result = await manager.GetDealsAsync(login, from, to);

        if (!result.IsSuccess)
        {
            Console.WriteLine($"❌ Failed: {result.Message}");
            return 1;
        }

        var deals = result.Data!;
        Console.WriteLine($"✅ Found {deals.Total()} deals:\n");

        for (uint i = 0; i < deals.Total() && i < 20; i++) // Show max 20
        {
            var deal = deals.Next(i);
            if (deal != null)
            {
                Console.WriteLine($"{i + 1}. Ticket: {deal.Deal()}");
                Console.WriteLine($"   Time: {SMTTime.ToDateTime(deal.Time())}");
                Console.WriteLine($"   Action: {deal.Action()}");
                Console.WriteLine($"   Profit: {deal.Profit():F2}");
                Console.WriteLine($"   Comment: {deal.Comment()}");
                Console.WriteLine();
            }
        }

        if (deals.Total() > 20)
        {
            Console.WriteLine($"... and {deals.Total() - 20} more deals");
        }

        return 0;
    }
}
