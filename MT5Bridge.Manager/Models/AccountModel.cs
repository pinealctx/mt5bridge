namespace MT5Bridge.Manager.Models;

/// <summary>
/// Account model (POCO representation of CIMTAccount)
/// Complete mapping of all IMTAccount interface properties
/// </summary>
public class AccountModel
{
    // ========== Core Identity ==========

    /// <summary>
    /// Get or set the login of the client, to whom the trading account belongs
    /// </summary>
    public ulong Login { get; set; }

    /// <summary>
    /// Get or set the number of decimal places in the account deposit currency
    /// </summary>
    public uint CurrencyDigits { get; set; }

    // ========== Balance & Credit ==========

    /// <summary>
    /// Get or set the balance of a trading account
    /// </summary>
    public double Balance { get; set; }

    /// <summary>
    /// Get or set the current amount of credit given to an account
    /// </summary>
    public double Credit { get; set; }

    // ========== Margin ==========

    /// <summary>
    /// Get or set the current value of the account margin
    /// </summary>
    public double Margin { get; set; }

    /// <summary>
    /// Get or set the free margin of an account
    /// </summary>
    public double MarginFree { get; set; }

    /// <summary>
    /// Get or set the margin level as a percentage
    /// </summary>
    public double MarginLevel { get; set; }

    /// <summary>
    /// Get or set the margin leverage
    /// </summary>
    public double MarginLeverage { get; set; }

    /// <summary>
    /// Get or set the current size of the initial margin of positions on a trading account
    /// </summary>
    public double MarginInitial { get; set; }

    /// <summary>
    /// Get or set the current size of the maintenance margin of positions on a trading account
    /// </summary>
    public double MarginMaintenance { get; set; }

    // ========== Profit & Loss ==========

    /// <summary>
    /// Get or set the size of the current profit for all open positions
    /// </summary>
    public double Profit { get; set; }

    /// <summary>
    /// Get or set the current size of swaps charged for open positions on the account
    /// </summary>
    public double Storage { get; set; }

    /// <summary>
    /// Get or set the size of commissions charged for all transactions on the account (deprecated, no longer used)
    /// </summary>
    [Obsolete("This field is deprecated and is no longer used")]
    public double Commission { get; set; }

    /// <summary>
    /// Get or set the size of floating profit/loss of open positions on the account
    /// </summary>
    public double Floating { get; set; }

    /// <summary>
    /// Get or set account equity
    /// </summary>
    public double Equity { get; set; }

    // ========== Stop Out Information ==========

    /// <summary>
    /// Get or set the account status as per the minimum amount of funds on the account required to maintain trading positions
    /// Values: ACTIVATION_NONE = 0, ACTIVATION_MARGIN_CALL = 1, ACTIVATION_STOP_OUT = 2
    /// </summary>
    public uint SOActivation { get; set; }

    /// <summary>
    /// Get or set the time when the Margin Call or Stop Out level was reached (SMT time format)
    /// </summary>
    public long SOTime { get; set; }

    /// <summary>
    /// Get or set the margin level of an account at the time of reaching the Stop Out level
    /// </summary>
    public double SOLevel { get; set; }

    /// <summary>
    /// Get or set the account equity at the time of reaching the Stop Out level
    /// </summary>
    public double SOEquity { get; set; }

    /// <summary>
    /// Get or set the margin amount on an account at the time of reaching the Stop Out level
    /// </summary>
    public double SOMargin { get; set; }

    // ========== Blocked Amounts ==========

    /// <summary>
    /// Get or set the amount of the standard commission locked on the account, which has been accumulated during the day/month
    /// </summary>
    public double BlockedCommission { get; set; }

    /// <summary>
    /// Get or set the amount of intraday profit locked on the account
    /// </summary>
    public double BlockedProfit { get; set; }

    // ========== Assets & Liabilities ==========

    /// <summary>
    /// Get or set the current total amount of assets on a trading account
    /// </summary>
    public double Assets { get; set; }

    /// <summary>
    /// Get or set the current total amount of liabilities on a trading account
    /// </summary>
    public double Liabilities { get; set; }

    /// <summary>
    /// Serialize to readable JSON with indentation
    /// </summary>
    public override string ToString()
    {
        return MT5JsonSerializer.ToReadableJson(this);
    }
}
