using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.Manager.Models;

/// <summary>
/// Extension methods for converting CIMTUser to UserModel
/// </summary>
public static class UserModelExtensions
{
    /// <summary>
    /// Convert CIMTUser COM object to UserModel POCO
    /// </summary>
    public static UserModel ToModel(this CIMTUser user)
    {
        return new UserModel
        {
            // Core Identity
            Login = user.Login(),
            Group = user.Group() ?? string.Empty,
            CertSerialNumber = user.CertSerialNumber(),
            Rights = (uint)user.Rights(),

            // Registration & Access
            Registration = (long)user.Registration(),
            LastAccess = (long)user.LastAccess(),
            LastIP = user.LastIP() ?? string.Empty,

            // Personal Information
#pragma warning disable CS0618 // Type or member is obsolete
            Name = user.Name() ?? string.Empty,
#pragma warning restore CS0618
            FirstName = user.FirstName() ?? string.Empty,
            LastName = user.LastName() ?? string.Empty,
            MiddleName = user.MiddleName() ?? string.Empty,
            Company = user.Company() ?? string.Empty,
            Account = user.Account() ?? string.Empty,

            // Contact Information
            Country = user.Country() ?? string.Empty,
            Language = user.Language(),
            City = user.City() ?? string.Empty,
            State = user.State() ?? string.Empty,
            ZIPCode = user.ZIPCode() ?? string.Empty,
            Address = user.Address() ?? string.Empty,
            Phone = user.Phone() ?? string.Empty,
            Email = user.EMail() ?? string.Empty,
            ID = user.ID() ?? string.Empty,

            // Account Identifiers
            MQID = user.MQID() ?? string.Empty,
            ClientID = user.ClientID(),
            Status = user.Status() ?? string.Empty,
            Comment = user.Comment() ?? string.Empty,
            Color = user.Color(),

            // Security
            PhonePassword = user.PhonePassword() ?? string.Empty,
            LastPassChange = (long)user.LastPassChange(),
            PasswordHash = string.Empty, // PasswordHash requires type parameter, skip for now
            OTPSecret = user.OTPSecret() ?? string.Empty,

            // Trading Settings
            Leverage = user.Leverage(),
            Agent = user.Agent(),
            LimitOrders = user.LimitOrders(),
            LimitPositionsValue = (ulong)user.LimitPositionsValue(),

            // Marketing
            LeadSource = user.LeadSource() ?? string.Empty,
            LeadCampaign = user.LeadCampaign() ?? string.Empty,

            // Financial Information
            Balance = user.Balance(),
            BalancePrevDay = user.BalancePrevDay(),
            BalancePrevMonth = user.BalancePrevMonth(),
            EquityPrevDay = user.EquityPrevDay(),
            EquityPrevMonth = user.EquityPrevMonth(),
            Credit = user.Credit(),

            // Commission & Interest
            InterestRate = user.InterestRate(),
            CommissionDaily = user.CommissionDaily(),
            CommissionMonthly = user.CommissionMonthly(),
            CommissionAgentDaily = user.CommissionAgentDaily(),
            CommissionAgentMonthly = user.CommissionAgentMonthly()
        };
    }

    /// <summary>
    /// Update CIMTUser COM object from UserModel POCO
    /// </summary>
    public static void UpdateFromModel(this CIMTUser user, UserModel model)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (model == null) throw new ArgumentNullException(nameof(model));

        user.Login(model.Login);
        user.Group(model.Group);
        // CertSerialNumber is read-only
        user.Rights((CIMTUser.EnUsersRights)model.Rights);

        // Personal Information
#pragma warning disable CS0618 // Type or member is obsolete
        user.Name(model.Name);
#pragma warning restore CS0618
        user.FirstName(model.FirstName);
        user.LastName(model.LastName);
        user.MiddleName(model.MiddleName);
        user.Company(model.Company);
        user.Account(model.Account);

        // Contact Information
        user.Country(model.Country);
        user.Language(model.Language);
        user.City(model.City);
        user.State(model.State);
        user.ZIPCode(model.ZIPCode);
        user.Address(model.Address);
        user.Phone(model.Phone);
        user.EMail(model.Email);
        user.ID(model.ID);

        // Account Identifiers
        // MQID is read-only
        user.ClientID(model.ClientID);
        user.Status(model.Status);
        user.Comment(model.Comment);
        user.Color(model.Color);

        // Security
        user.PhonePassword(model.PhonePassword);

        // Trading Settings
        user.Leverage(model.Leverage);
        user.Agent(model.Agent);
        user.LimitOrders(model.LimitOrders);
        user.LimitPositionsValue(model.LimitPositionsValue);

        // Marketing
        user.LeadSource(model.LeadSource);
        user.LeadCampaign(model.LeadCampaign);

        // Note: Financial fields (Balance, Credit, etc.) are read-only in CIMTUser
        // and are updated via deals/manager operations.
    }
}
