using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.Manager.Models;

/// <summary>
/// Extension methods for converting CIMTConGroup to GroupModel
/// </summary>
public static class GroupModelExtensions
{
    /// <summary>
    /// Convert CIMTConGroup COM object to GroupModel POCO
    /// </summary>
    public static GroupModel ToModel(this CIMTConGroup group)
    {
        return new GroupModel
        {
            // Core Identity
            Group = group.Group() ?? string.Empty,
            Server = group.Server(),

            // Permissions
            PermissionsFlags = (ulong)group.PermissionsFlags(),
            AuthMode = (uint)group.AuthMode(),
            AuthOTPMode = (uint)group.AuthOTPMode(),
            AuthPasswordMin = group.AuthPasswordMin(),

            // Company Information
            Company = group.Company() ?? string.Empty,
            CompanyPage = group.CompanyPage() ?? string.Empty,
            CompanyEmail = group.CompanyEmail() ?? string.Empty,
            CompanySupportPage = group.CompanySupportPage() ?? string.Empty,
            CompanySupportEmail = group.CompanySupportEmail() ?? string.Empty,
            CompanyCatalog = group.CompanyCatalog() ?? string.Empty,
            CompanyDepositPage = group.CompanyDepositPage() ?? string.Empty,
            CompanyWithdrawalPage = group.CompanyWithdrawalPage() ?? string.Empty,

            // Currency
            Currency = group.Currency() ?? string.Empty,
            CurrencyDigits = group.CurrencyDigits(),

            // Reports
            ReportsMode = (uint)group.ReportsMode(),
            ReportsFlags = (uint)group.ReportsFlags(),
            ReportsEmail = group.ReportsEmail() ?? string.Empty,
#pragma warning disable CS0618 // Type or member is obsolete
            ReportsSMTP = group.ReportsSMTP() ?? string.Empty,
            ReportsSMTPLogin = group.ReportsSMTPLogin() ?? string.Empty,
            ReportsSMTPPass = group.ReportsSMTPPass() ?? string.Empty,
#pragma warning restore CS0618

            // News
            NewsMode = (uint)group.NewsMode(),
            NewsCategory = group.NewsCategory() ?? string.Empty,

            // Mail
            MailMode = (uint)group.MailMode(),

            // Trading
            TradeFlags = (uint)group.TradeFlags(),
            TradeTransferMode = (uint)group.TradeTransferMode(),
            TradeInterestrate = group.TradeInterestrate(),
            TradeVirtualCredit = group.TradeVirtualCredit(),

            // Margin
            MarginFreeMode = (uint)group.MarginFreeMode(),
            MarginSOMode = (uint)group.MarginSOMode(),
            MarginCall = group.MarginCall(),
            MarginStopOut = group.MarginStopOut(),
            MarginFreeProfitMode = group.MarginFreeProfitMode(),
            MarginMode = (uint)group.MarginMode(),
            MarginFlags = (uint)group.MarginFlags(),
            MarginFloatingLeverage = group.MarginFloatingLeverage() ?? string.Empty,

            // Demo Account Settings
            DemoLeverage = group.DemoLeverage(),
            DemoDeposit = group.DemoDeposit(),
            DemoInactivityPeriod = group.DemoInactivityPeriod(),

            // Limits
            LimitHistory = (uint)group.LimitHistory(),
            LimitOrders = group.LimitOrders(),
            LimitSymbols = group.LimitSymbols(),
            LimitPositions = group.LimitPositions()
        };
    }
}
