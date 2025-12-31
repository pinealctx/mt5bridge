using Microsoft.Extensions.Configuration;
using Serilog;

namespace MT5Bridge.Manager.Demo.Commands;

/// <summary>
/// Query commands: groups, user, account, deals, balance-history
/// </summary>
public class QueryCommand : BaseCommand
{
    public QueryCommand(ILogger logger, IConfiguration configuration)
        : base(logger, configuration)
    {
    }

    public async Task<int> GroupsAsync(string? server, ulong? login, string? password)
    {
        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Console.WriteLine("=== Query Groups ===\n");

        var result = await manager.GetGroupsAsync();
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Failed: {result.Message}");
            return 1;
        }

        var groups = result.Data!;
        Logger.Information("Found {GroupCount} groups", groups.Length);
        foreach (var group in groups)
        {
            Logger.Information("  Group: {Group}", group.Group);
        }

        return 0;
    }

    public async Task<int> UserAsync(string? server, ulong? login, string? password, ulong userLogin)
    {
        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Console.WriteLine($"=== Query User {userLogin} ===\n");

        var result = await manager.GetUserAsync(userLogin);
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Failed: {result.Message}");
            return 1;
        }

        var user = result.Data!;
        Logger.Information("User Login: {Login}", user.Login);
#pragma warning disable CS0618 // Type or member is obsolete
        Logger.Information("Name: {Name}", user.Name);
#pragma warning restore CS0618
        Logger.Information("First Name: {FirstName}", user.FirstName);
        Logger.Information("Middle Name: {MiddleName}", user.MiddleName);
        Logger.Information("Last Name: {LastName}", user.LastName);
        Logger.Information("Email: {Email}", user.Email);
        Logger.Information("Group: {Group}", user.Group);
        Logger.Information("Status: {Status}", user.Status);
        Logger.Information("Leverage: {Leverage}", user.Leverage);
        Logger.Information("Agent: {Agent}", user.Agent);
        Logger.Information("Registration: {Registration}", user.Registration);

        return 0;
    }

    public async Task<int> AccountAsync(string? server, ulong? login, string? password, ulong userLogin)
    {
        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Console.WriteLine($"=== Query Account {userLogin} ===\n");

        var result = await manager.GetAccountAsync(userLogin);
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Failed: {result.Message}");
            return 1;
        }

        var account = result.Data!;
        Logger.Information("Account Login: {Login}", account.Login);
        Logger.Information("Balance: {Balance:F2}", account.Balance);
        Logger.Information("Credit: {Credit:F2}", account.Credit);
        Logger.Information("Equity: {Equity:F2}", account.Equity);
        Logger.Information("Margin: {Margin:F2}", account.Margin);
        Logger.Information("Free Margin: {MarginFree:F2}", account.MarginFree);
        Logger.Information("Margin Level: {MarginLevel:F2}%", account.MarginLevel);
        Logger.Information("Profit: {Profit:F2}", account.Profit);

        return 0;
    }

    public async Task<int> DealsAsync(string? server, ulong? login, string? password, ulong userLogin, int days = 7)
    {
        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Console.WriteLine($"=== Query Deals for User {userLogin} (last {days} days) ===\n");

        var from = DateTime.UtcNow.AddDays(-days);
        var to = DateTime.UtcNow;

        var result = await manager.GetDealsAsync(userLogin, from, to);
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Failed: {result.Message}");
            return 1;
        }

        var deals = result.Data!;
        Logger.Information("Found {DealCount} deals", deals.Length);

        for (int i = 0; i < Math.Min(deals.Length, 20); i++) // Show first 20
        {
            var deal = deals[i];
            Logger.Information(
                "  Deal #{Deal}: {Symbol}, Action: {Action}, Price: {Price:F5}, Volume: {Volume}, Profit: {Profit:F2}",
                deal.Deal, deal.Symbol, deal.Action, deal.Price, deal.Volume, deal.Profit);
        }

        if (deals.Length > 20)
        {
            Logger.Information("  ... and {MoreCount} more", deals.Length - 20);
        }

        return 0;
    }

    public async Task<int> BalanceHistoryAsync(string? server, ulong? login, string? password, ulong userLogin, int days = 30)
    {
        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Console.WriteLine($"=== Balance History for User {userLogin} (last {days} days) ===\n");

        var from = DateTime.UtcNow.AddDays(-days);
        var to = DateTime.UtcNow;

        var result = await manager.GetBalanceHistoryAsync(userLogin, from, to);
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Failed: {result.Message}");
            return 1;
        }

        Logger.Information("{ResultMessage}", result.Message);

        var deals = result.Data!;
        double totalChange = 0;
        int balanceCount = 0;

        Logger.Information("Balance Operations:");

        foreach (var deal in deals)
        {
            // Filter only balance operations (already filtered by manager, but double check if needed)
            // Actually GetBalanceHistoryAsync already filters for DEAL_BALANCE
            balanceCount++;
            var profit = deal.Profit;
            totalChange += profit;

            var time = deal.Time;
            var operation = profit > 0 ? "Deposit" : "Withdrawal";

            Logger.Information(
                "{Time:yyyy-MM-dd HH:mm:ss} | {Operation,10} | {Profit,12:F2} | {Comment}",
                time, operation, profit, deal.Comment);
        }

        Logger.Information("Total Balance Operations: {Count}", balanceCount);
        Logger.Information("Net Balance Change: {TotalChange:F2}", totalChange);

        return 0;
    }
}
