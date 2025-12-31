using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.Manager.Models.Proto;

public static class AccountProtoExtensions
{
    /// <summary>
    /// Converts CIMTAccount COM object to Proto AccountModel
    /// </summary>
    public static AccountModel ToProto(this CIMTAccount account)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

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
            Commission = 0, // Deprecated - no longer used
#pragma warning restore CS0618
            Floating = account.Floating(),
            Equity = account.Equity(),

            // Stop Out Information
            SoActivation = (uint)account.SOActivation(),
            SoTime = (long)account.SOTime(),
            SoLevel = account.SOLevel(),
            SoEquity = account.SOEquity(),
            SoMargin = account.SOMargin(),

            // Blocked Amounts
            BlockedCommission = account.BlockedCommission(),
            BlockedProfit = account.BlockedProfit(),

            // Assets & Liabilities
            Assets = account.Assets(),
            Liabilities = account.Liabilities()
        };
    }
}
