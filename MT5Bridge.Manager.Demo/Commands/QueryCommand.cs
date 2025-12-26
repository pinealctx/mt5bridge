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
        Console.WriteLine($"Found {groups.Length} groups:\n");

        foreach (var group in groups)
        {
            Console.WriteLine($"  {group.Group}");
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
        Console.WriteLine($"Login:       {user.Login}");

#pragma warning disable CS0618 // Type or member is obsolete
        Console.WriteLine($"Name:        {user.Name}");
#pragma warning restore CS0618

        Console.WriteLine($"First Name:  {user.FirstName}");
        Console.WriteLine($"Middle Name: {user.MiddleName}");
        Console.WriteLine($"Last Name:   {user.LastName}");
        Console.WriteLine($"Email:       {user.Email}");
        Console.WriteLine($"Group:       {user.Group}");
        Console.WriteLine($"Status:      {user.Status}");
        Console.WriteLine($"Leverage:    {user.Leverage}");
        Console.WriteLine($"Agent:       {user.Agent}");
        Console.WriteLine($"Registration: {user.Registration}");

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
        Console.WriteLine($"Login:      {account.Login}");
        Console.WriteLine($"Balance:    {account.Balance:F2}");
        Console.WriteLine($"Credit:     {account.Credit:F2}");
        Console.WriteLine($"Equity:     {account.Equity:F2}");
        Console.WriteLine($"Margin:     {account.Margin:F2}");
        Console.WriteLine($"Free Margin: {account.MarginFree:F2}");
        Console.WriteLine($"Margin Level: {account.MarginLevel:F2}%");
        Console.WriteLine($"Profit:     {account.Profit:F2}");

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
        Console.WriteLine($"Found {deals.Length} deals:\n");

        for (int i = 0; i < Math.Min(deals.Length, 20); i++) // Show first 20
        {
            var deal = deals[i];
            Console.WriteLine($"  Deal #{deal.Deal}: {deal.Symbol}, " +
                            $"Action: {deal.Action}, " +
                            $"Price: {deal.Price:F5}, " +
                            $"Volume: {deal.Volume}, " +
                            $"Profit: {deal.Profit:F2}");
        }

        if (deals.Length > 20)
        {
            Console.WriteLine($"  ... and {deals.Length - 20} more");
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

        Console.WriteLine($"{result.Message}\n");

        var deals = result.Data!;
        double totalChange = 0;
        int balanceCount = 0;

        Console.WriteLine("Balance Operations:");
        Console.WriteLine("---------------------------------------------------");

        foreach (var deal in deals)
        {
            // Filter only balance operations (already filtered by manager, but double check if needed)
            // Actually GetBalanceHistoryAsync already filters for DEAL_BALANCE
            balanceCount++;
            var profit = deal.Profit;
            totalChange += profit;

            var time = deal.Time;
            var operation = profit > 0 ? "Deposit" : "Withdrawal";

            Console.WriteLine($"{time:yyyy-MM-dd HH:mm:ss} | {operation,10} | {profit,12:F2} | {deal.Comment}");
        }

        Console.WriteLine("---------------------------------------------------");
        Console.WriteLine($"Total Balance Operations: {balanceCount}");
        Console.WriteLine($"Net Balance Change: {totalChange:F2}");

        return 0;
    }
}
