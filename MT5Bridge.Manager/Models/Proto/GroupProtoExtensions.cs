using MetaQuotes.MT5CommonAPI;

namespace MT5Bridge.Manager.Models.Proto;

public static class GroupProtoExtensions
{
    /// <summary>
    /// Converts CIMTConGroup COM object to Proto GroupModel
    /// </summary>
    public static GroupModel ToProto(this CIMTConGroup group)
    {
        if (group == null)
            throw new ArgumentNullException(nameof(group));

        return new GroupModel
        {
            // Core Identity
            Group = group.Group() ?? string.Empty,
            Server = group.Server(),

            // Permissions
            PermissionsFlags = (ulong)group.PermissionsFlags(),
            AuthMode = (uint)group.AuthMode(),
            AuthOtpMode = (uint)group.AuthOTPMode(),
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
            ReportsSmtp = group.ReportsSMTP() ?? string.Empty,
            ReportsSmtpLogin = group.ReportsSMTPLogin() ?? string.Empty,
            ReportsSmtpPass = group.ReportsSMTPPass() ?? string.Empty,
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
            MarginSoMode = (uint)group.MarginSOMode(),
            MarginCall = group.MarginCall(),
            MarginStopOut = group.MarginStopOut(),
            MarginFreeProfitMode = (uint)group.MarginFreeProfitMode(),
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
