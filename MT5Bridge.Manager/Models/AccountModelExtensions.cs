using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.Manager.Models;

/// <summary>
/// Extension methods for converting CIMTAccount to AccountModel
/// </summary>
public static class AccountModelExtensions
{
    /// <summary>
    /// Convert CIMTAccount COM object to AccountModel POCO
    /// </summary>
    public static AccountModel ToModel(this CIMTAccount account)
    {
        return new AccountModel
        {
            // Core Identity
            Login = account.Login(),
            CurrencyDigits = account.CurrencyDigits(),

            // Balance & Credit
            Balance = account.Balance(),
            Credit = account.Credit(),

            // Margin
            Margin = account.Margin(),
            MarginFree = account.MarginFree(),
            MarginLevel = account.MarginLevel(),
            MarginLeverage = account.MarginLeverage(),
            MarginInitial = account.MarginInitial(),
            MarginMaintenance = account.MarginMaintenance(),

            // Profit & Loss
            Profit = account.Profit(),
            Storage = account.Storage(),
#pragma warning disable CS0618 // Type or member is obsolete
            Commission = 0, // Deprecated field, set to 0
#pragma warning restore CS0618
            Floating = account.Floating(),
            Equity = account.Equity(),

            // Stop Out Information
            SOActivation = account.SOActivation(),
            SOTime = (long)account.SOTime(),
            SOLevel = account.SOLevel(),
            SOEquity = account.SOEquity(),
            SOMargin = account.SOMargin(),

            // Blocked Amounts
            BlockedCommission = account.BlockedCommission(),
            BlockedProfit = account.BlockedProfit(),

            // Assets & Liabilities
            Assets = account.Assets(),
            Liabilities = account.Liabilities()
        };
    }
}
