namespace MT5Bridge.Manager.Models;

/// <summary>
/// Group model (POCO representation of CIMTConGroup)
/// Mapping of core IMTConGroup interface properties
/// Note: Commission and Symbol configurations are complex nested structures and may need separate models
/// </summary>
public class GroupModel
{
    // ========== Core Identity ==========

    /// <summary>
    /// Get or set the name of a group, including a path to it in accordance with the hierarchy
    /// </summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the ID of the trade server, to which the group is linked
    /// </summary>
    public ulong Server { get; set; }

    // ========== Permissions ==========

    /// <summary>
    /// Get or set permission flags for the group (EnPermissionsFlags)
    /// </summary>
    public ulong PermissionsFlags { get; set; }

    /// <summary>
    /// Get or set the authorization mode for accounts in the group (EnAuthMode)
    /// </summary>
    public uint AuthMode { get; set; }

    /// <summary>
    /// Get or set authentication mode using one-time passwords (EnAuthOTPMode)
    /// </summary>
    public uint AuthOTPMode { get; set; }

    /// <summary>
    /// Get or set the minimum password length for accounts in the group
    /// </summary>
    public uint AuthPasswordMin { get; set; }

    // ========== Company Information ==========

    /// <summary>
    /// Get or set the name of the company that services the group
    /// </summary>
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the website address of the company that services the group
    /// </summary>
    public string CompanyPage { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the email address of the company that services the group
    /// </summary>
    public string CompanyEmail { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the website address of the technical support of the company that services the group
    /// </summary>
    public string CompanySupportPage { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the technical support email address of the company that services the group
    /// </summary>
    public string CompanySupportEmail { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the name of the subdirectory that stores the templates of reports, emails, etc.
    /// </summary>
    public string CompanyCatalog { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the deposit page URL for a group of accounts
    /// </summary>
    public string CompanyDepositPage { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the withdrawal page URL for a group of accounts
    /// </summary>
    public string CompanyWithdrawalPage { get; set; } = string.Empty;

    // ========== Currency ==========

    /// <summary>
    /// Get or set the deposit currency of the group
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the number of digits after the decimal point in the group deposit currency
    /// </summary>
    public uint CurrencyDigits { get; set; }

    // ========== Reports ==========

    /// <summary>
    /// Get or set the mode of report generation (EnReportsMode)
    /// </summary>
    public uint ReportsMode { get; set; }

    /// <summary>
    /// Get or set the options for sending reports (EnReportsFlags)
    /// </summary>
    public uint ReportsFlags { get; set; }

    /// <summary>
    /// Get or set the mail server which is used for sending reports to clients in the group
    /// </summary>
    public string ReportsEmail { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the address of the SMTP server for sending reports (obsolete)
    /// </summary>
    [Obsolete("This method is obsolete and is no longer used")]
    public string ReportsSMTP { get; set; } = string.Empty;

    /// <summary>
    /// Get or set a login for the authorization on the SMTP server (obsolete)
    /// </summary>
    [Obsolete("This method is obsolete and is no longer used")]
    public string ReportsSMTPLogin { get; set; } = string.Empty;

    /// <summary>
    /// Get or set a password for the authorization on the SMTP server (obsolete)
    /// </summary>
    [Obsolete("This method is obsolete and is no longer used")]
    public string ReportsSMTPPass { get; set; } = string.Empty;

    // ========== News ==========

    /// <summary>
    /// Get or set the mode of news sending to the clients from the group (EnNewsMode)
    /// </summary>
    public uint NewsMode { get; set; }

    /// <summary>
    /// Get or set the categories of news received by the group
    /// </summary>
    public string NewsCategory { get; set; } = string.Empty;

    // ========== Mail ==========

    /// <summary>
    /// Get or set the mode of operation of the internal mail system for the group (EnMailMode)
    /// </summary>
    public uint MailMode { get; set; }

    // ========== Trading ==========

    /// <summary>
    /// Get or set trade options of a group (EnTradeFlags)
    /// </summary>
    public uint TradeFlags { get; set; }

    /// <summary>
    /// Get or set the mode of money transfer between accounts (EnTransferMode)
    /// </summary>
    public uint TradeTransferMode { get; set; }

    /// <summary>
    /// Get or set the annual interest rate on deposits of the group accounts
    /// </summary>
    public double TradeInterestrate { get; set; }

    /// <summary>
    /// Get or set the amount of additional funds that a brokerage company can provide to a client
    /// for opening a position with a volume larger than allowed by the client's current funds
    /// </summary>
    public double TradeVirtualCredit { get; set; }

    // ========== Margin ==========

    /// <summary>
    /// Get or set the mode of including floating profit/loss into free margin calculation (EnFreeMarginMode)
    /// </summary>
    public uint MarginFreeMode { get; set; }

    /// <summary>
    /// Get or set the mode of checking the levels of Stop Out and Margin Call (EnStopOutMode)
    /// </summary>
    public uint MarginSOMode { get; set; }

    /// <summary>
    /// Get or set the Margin Call level
    /// </summary>
    public double MarginCall { get; set; }

    /// <summary>
    /// Get or set the Stop Out level
    /// </summary>
    public double MarginStopOut { get; set; }

    /// <summary>
    /// Get or set the mode of use of the profit/loss recorded during a trading day
    /// in free margin calculation (EnMarginFreeProfitFlags)
    /// </summary>
    public uint MarginFreeProfitMode { get; set; }

    /// <summary>
    /// Get or set the risk management mode applied for the group
    /// </summary>
    public uint MarginMode { get; set; }

    /// <summary>
    /// Get or set margin calculation flags
    /// </summary>
    public uint MarginFlags { get; set; }

    /// <summary>
    /// Get the floating margin profile applied to the group
    /// </summary>
    public string MarginFloatingLeverage { get; set; } = string.Empty;

    // ========== Demo Account Settings ==========

    /// <summary>
    /// Get or set the default credit leverage for demo accounts opened in the group
    /// </summary>
    public uint DemoLeverage { get; set; }

    /// <summary>
    /// Get or set the default amount of deposit for demo accounts opened in the group
    /// </summary>
    public double DemoDeposit { get; set; }

    /// <summary>
    /// Get or set demo account inactivity period, after which open orders and positions
    /// from these accounts will be deleted from the platform databases (in days)
    /// </summary>
    public uint DemoInactivityPeriod { get; set; }

    // ========== Limits ==========

    /// <summary>
    /// Get or set the maximum number of days, for which the group can request data on conducted trade operations
    /// </summary>
    public uint LimitHistory { get; set; }

    /// <summary>
    /// Get or set the maximum number of orders that can be simultaneously placed by an account from this group
    /// </summary>
    public uint LimitOrders { get; set; }

    /// <summary>
    /// Get or set the maximum number of symbols, for which an account can simultaneously receive quotes
    /// </summary>
    public uint LimitSymbols { get; set; }

    /// <summary>
    /// Get or set the maximum number of open positions that can be present simultaneously on a client account from this group
    /// </summary>
    public uint LimitPositions { get; set; }

    /// <summary>
    /// Serialize to readable JSON with indentation
    /// </summary>
    public override string ToString()
    {
        return MT5JsonSerializer.ToReadableJson(this);
    }
}
