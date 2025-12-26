using System.Text.Json.Serialization;

namespace MT5Bridge.Manager.Models;

/// <summary>
/// User model (POCO representation of CIMTUser)
/// Complete mapping of all IMTUser interface properties
/// </summary>
public class UserModel
{
    // ========== Core Identity ==========

    /// <summary>
    /// Get or set the login of a user
    /// </summary>
    public ulong Login { get; set; }

    /// <summary>
    /// Get or set the group to which the user is included
    /// </summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// Get the number of the certificate that was used last by the client for authorization
    /// </summary>
    public ulong CertSerialNumber { get; set; }

    /// <summary>
    /// Get or set user permissions (EnUsersRights flags)
    /// </summary>
    public uint Rights { get; set; }

    // ========== Registration & Access ==========

    /// <summary>
    /// Get or set the client record creation date (SMT time format)
    /// </summary>
    public long Registration { get; set; }

    /// <summary>
    /// Get the date of the last connection using the account (SMT time format)
    /// </summary>
    public long LastAccess { get; set; }

    /// <summary>
    /// Get the IP address from which the user last connected to the server
    /// </summary>
    public string LastIP { get; set; } = string.Empty;

    // ========== Personal Information ==========

    /// <summary>
    /// Get or set the client's name (obsolete, use FirstName instead)
    /// </summary>
    [Obsolete("Use FirstName instead")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the client's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the client's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the client's middle name
    /// </summary>
    public string MiddleName { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the name of a client's company
    /// </summary>
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the number of a client's account in an external trading system
    /// </summary>
    public string Account { get; set; } = string.Empty;

    // ========== Contact Information ==========

    /// <summary>
    /// Get or set a client's country of residence
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the language of a user
    /// </summary>
    public uint Language { get; set; }

    /// <summary>
    /// Get or set a client's city of residence
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Get or set a client's state (region) of residence
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Get or set a client's zip code
    /// </summary>
    public string ZIPCode { get; set; } = string.Empty;

    /// <summary>
    /// Get or set a client's address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Get or set a client's phone number
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Get or set a client's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the number of a client's identity document
    /// </summary>
    public string ID { get; set; } = string.Empty;

    // ========== Account Identifiers ==========

    /// <summary>
    /// Get the client's MetaQuotes ID
    /// </summary>
    public string MQID { get; set; } = string.Empty;

    /// <summary>
    /// Get or set the client ID with which the trading account is associated
    /// </summary>
    public ulong ClientID { get; set; }

    /// <summary>
    /// Get or set a client's status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Get or set a comment to a client
    /// </summary>
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Get or set a client's color (for visual identification)
    /// </summary>
    public uint Color { get; set; }

    // ========== Security ==========

    /// <summary>
    /// Get or set a client's phone password
    /// </summary>
    public string PhonePassword { get; set; } = string.Empty;

    /// <summary>
    /// Get the date of the last change of the user's password (SMT time format)
    /// </summary>
    public long LastPassChange { get; set; }

    /// <summary>
    /// Get the password hash of a client record (used only in MetaTrader 5 Server API)
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Get or set a secret key which links a trading account and a one-time password generator
    /// </summary>
    public string OTPSecret { get; set; } = string.Empty;

    // ========== Trading Settings ==========

    /// <summary>
    /// Get or set the size of a client's leverage
    /// </summary>
    public uint Leverage { get; set; }

    /// <summary>
    /// Get or set the number of a client's agent account
    /// </summary>
    public ulong Agent { get; set; }

    /// <summary>
    /// Get or set the maximum number of active (placed) pending orders allowed on the account
    /// </summary>
    public uint LimitOrders { get; set; }

    /// <summary>
    /// Get or set the maximum value of open positions allowed on the account
    /// </summary>
    public ulong LimitPositionsValue { get; set; }

    // ========== Marketing ==========

    /// <summary>
    /// Get or set a lead source : a website a client has come from
    /// </summary>
    public string LeadSource { get; set; } = string.Empty;

    /// <summary>
    /// Get or set a lead campaign : name of a marketing campaign a client was attracted by
    /// </summary>
    public string LeadCampaign { get; set; } = string.Empty;

    // ========== Financial Information (Read-only) ==========

    /// <summary>
    /// Get the current balance of a client
    /// </summary>
    public double Balance { get; set; }

    /// <summary>
    /// Get the value of a client's balance as of the end of the previous day
    /// </summary>
    public double BalancePrevDay { get; set; }

    /// <summary>
    /// Get the value of a client's balance as of the end of the previous trading month
    /// </summary>
    public double BalancePrevMonth { get; set; }

    /// <summary>
    /// Get the value of a client's equity as of the end of the previous day
    /// </summary>
    public double EquityPrevDay { get; set; }

    /// <summary>
    /// Get the value of a client's equity as of the end of the previous trading month
    /// </summary>
    public double EquityPrevMonth { get; set; }

    /// <summary>
    /// Get the current amount of funds credited to a client
    /// </summary>
    public double Credit { get; set; }

    // ========== Commission & Interest (Read-only) ==========

    /// <summary>
    /// Get the amount accrued for the current month calculated based on the annual interest rate
    /// </summary>
    public double InterestRate { get; set; }

    /// <summary>
    /// Get the amount of commissions from a client for a day
    /// </summary>
    public double CommissionDaily { get; set; }

    /// <summary>
    /// Get the total amount of commissions charged from a client for the current month
    /// </summary>
    public double CommissionMonthly { get; set; }

    /// <summary>
    /// Get the size of agent commissions charged from a client's trade operations for a day
    /// </summary>
    public double CommissionAgentDaily { get; set; }

    /// <summary>
    /// Get the amount of agent commission charged for a client's trade operations for the current month
    /// </summary>
    public double CommissionAgentMonthly { get; set; }

    /// <summary>
    /// Serialize to readable JSON with indentation
    /// </summary>
    public override string ToString()
    {
        return MT5JsonSerializer.ToReadableJson(this);
    }
}
