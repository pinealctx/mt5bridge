using Microsoft.Extensions.Configuration;
using Serilog;

namespace MT5Bridge.Manager.Demo.Commands;

/// <summary>
/// Trade commands: deposit, withdraw
/// </summary>
public class TradeCommand : BaseCommand
{
    public TradeCommand(ILogger logger, IConfiguration configuration)
        : base(logger, configuration)
    {
    }

    public async Task<int> DepositAsync(string? server, ulong? login, string? password, ulong userLogin, double amount, string? comment)
    {
        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Console.WriteLine($"=== Deposit to User {userLogin} ===\n");
        Console.WriteLine($"Amount: {amount:F2}");
        Console.WriteLine($"Comment: {comment ?? "Deposit via MT5Bridge"}\n");

        var result = await manager.DepositAsync(userLogin, (decimal)amount, comment ?? "Deposit via MT5Bridge");

        if (!result.IsSuccess)
        {
            Console.WriteLine($"Failed: {result.Message}");
            return 1;
        }

        Console.WriteLine($"Success! {result.Message}");

        // Query account after deposit
        var accountResult = await manager.GetAccountAsync(userLogin);
        if (accountResult.IsSuccess)
        {
            Console.WriteLine($"\nNew Balance: {accountResult.Data!.Balance:F2}");
        }

        return 0;
    }

    public async Task<int> WithdrawAsync(string? server, ulong? login, string? password, ulong userLogin, double amount, string? comment)
    {
        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Console.WriteLine($"=== Withdraw from User {userLogin} ===\n");
        Console.WriteLine($"Amount: {amount:F2}");
        Console.WriteLine($"Comment: {comment ?? "Withdrawal via MT5Bridge"}\n");

        var result = await manager.WithdrawAsync(userLogin, (decimal)amount, comment ?? "Withdrawal via MT5Bridge");

        if (!result.IsSuccess)
        {
            Console.WriteLine($"Failed: {result.Message}");
            return 1;
        }

        Console.WriteLine($"Success! {result.Message}");

        // Query account after withdrawal
        var accountResult = await manager.GetAccountAsync(userLogin);
        if (accountResult.IsSuccess)
        {
            Console.WriteLine($"\nNew Balance: {accountResult.Data!.Balance:F2}");
        }

        return 0;
    }
}
