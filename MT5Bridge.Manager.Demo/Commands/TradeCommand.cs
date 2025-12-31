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
        // Validate input
        if (amount <= 0)
        {
            Logger.Error("Deposit amount must be greater than zero");
            Console.WriteLine("Error: Deposit amount must be greater than zero");
            return 1;
        }

        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Logger.Information("=== Deposit to User {UserLogin} ===", userLogin);
        Logger.Information("Amount: {Amount:F2}", amount);
        Logger.Information("Comment: {Comment}", comment ?? "Deposit via MT5Bridge");

        var result = await manager.DepositAsync(userLogin, (decimal)amount, comment ?? "Deposit via MT5Bridge");

        if (!result.IsSuccess)
        {
            Logger.Error("Deposit failed: {Message}", result.Message);
            return 1;
        }

        Logger.Information("Deposit success: {Message}", result.Message);

        // Query account after deposit
        var accountResult = await manager.GetAccountAsync(userLogin);
        if (accountResult.IsSuccess)
        {
            Logger.Information("New Balance: {Balance:F2}", accountResult.Data!.Balance);
        }

        return 0;
    }

    public async Task<int> WithdrawAsync(string? server, ulong? login, string? password, ulong userLogin, double amount, string? comment)
    {
        // Validate input
        if (amount <= 0)
        {
            Logger.Error("Withdrawal amount must be greater than zero");
            Console.WriteLine("Error: Withdrawal amount must be greater than zero");
            return 1;
        }

        var settings = CreateConnectionSettings(server, login, password);
        using var manager = await ConnectAsync(settings);

        Logger.Information("=== Withdraw from User {UserLogin} ===", userLogin);
        Logger.Information("Amount: {Amount:F2}", amount);
        Logger.Information("Comment: {Comment}", comment ?? "Withdrawal via MT5Bridge");

        var result = await manager.WithdrawAsync(userLogin, (decimal)amount, comment ?? "Withdrawal via MT5Bridge");

        if (!result.IsSuccess)
        {
            Logger.Error("Withdrawal failed: {Message}", result.Message);
            return 1;
        }

        Logger.Information("Withdrawal success: {Message}", result.Message);

        // Query account after withdrawal
        var accountResult = await manager.GetAccountAsync(userLogin);
        if (accountResult.IsSuccess)
        {
            Logger.Information("New Balance: {Balance:F2}", accountResult.Data!.Balance);
        }

        return 0;
    }
}
