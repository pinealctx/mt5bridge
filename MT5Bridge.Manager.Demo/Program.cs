using CommandLine;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using Serilog;
using MT5Bridge.Serilog;
using MT5Bridge.Manager.Demo.Commands;
using MT5Bridge.Manager.Demo.Options;

namespace MT5Bridge.Manager.Demo;

public static class Program
{
    [ModuleInitializer]
    internal static void RegisterAssemblyResolver()
    {
        // Ensure current directory is the same as the executable directory
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        if (Directory.GetCurrentDirectory() != baseDirectory)
        {
            Directory.SetCurrentDirectory(baseDirectory);
        }

        // Register assembly resolver for MT5 SDK DLLs
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

    public static async Task<int> Main(string[] args)
    {
        Console.WriteLine("=== MT5Bridge CLI ===\n");

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        // Configure Serilog
        var serilogConfig = new SerilogConfig();
        configuration.GetSection("Logging").Bind(serilogConfig);

        // Default to console if not configured
        if (!serilogConfig.Console.Enabled && !serilogConfig.File.Enabled && !serilogConfig.CloudWatch.Enabled)
        {
            serilogConfig.Console.Enabled = true;
        }

        var logger = SerilogBootstrapper.CreateLogger(serilogConfig);

        // Initialize command handlers
        var queryCmd = new QueryCommand(logger, configuration);
        var listenCmd = new ListenCommand(logger, configuration);
        var tradeCmd = new TradeCommand(logger, configuration);

        try
        {
            return await Parser.Default.ParseArguments<
                QueryGroupsOptions,
                QueryUserOptions,
                QueryAccountOptions,
                QueryDealsOptions,
                QueryBalanceOptions,
                ListenOptions,
                DepositOptions,
                WithdrawOptions>(args)
                .MapResult(
                    (QueryGroupsOptions opts) => queryCmd.GroupsAsync(opts.Server, opts.Login, opts.Password),
                    (QueryUserOptions opts) => queryCmd.UserAsync(opts.Server, opts.Login, opts.Password, opts.UserLogin),
                    (QueryAccountOptions opts) => queryCmd.AccountAsync(opts.Server, opts.Login, opts.Password, opts.UserLogin),
                    (QueryDealsOptions opts) => queryCmd.DealsAsync(opts.Server, opts.Login, opts.Password, opts.UserLogin, opts.Days),
                    (QueryBalanceOptions opts) => queryCmd.BalanceHistoryAsync(opts.Server, opts.Login, opts.Password, opts.UserLogin, opts.Days),
                    (ListenOptions opts) => listenCmd.RunAsync(opts.Server, opts.Login, opts.Password, opts.Types, opts.Mode),
                    (DepositOptions opts) => tradeCmd.DepositAsync(opts.Server, opts.Login, opts.Password, opts.UserLogin, opts.Amount, opts.Comment),
                    (WithdrawOptions opts) => tradeCmd.WithdrawAsync(opts.Server, opts.Login, opts.Password, opts.UserLogin, opts.Amount, opts.Comment),
                    errs => Task.FromResult(1));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            logger.Error(ex, "CLI error");
            return 1;
        }
        finally
        {
            if (logger is IDisposable disposable)
            {
                disposable.Dispose();
            }
            await Log.CloseAndFlushAsync();
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}

