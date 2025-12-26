using MetaQuotes.MT5CommonAPI;
using MetaQuotes.MT5ManagerAPI;

namespace MT5Bridge.Manager.Models.Proto;

public static class UserProtoExtensions
{
    /// <summary>
    /// Converts CIMTUser COM object to Proto UserModel
    /// </summary>
    public static UserModel ToProto(this CIMTUser user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

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
            LastIp = user.LastIP() ?? string.Empty,

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
            ZipCode = user.ZIPCode() ?? string.Empty,
            Address = user.Address() ?? string.Empty,
            Phone = user.Phone() ?? string.Empty,
            Email = user.EMail() ?? string.Empty,
            Id = user.ID() ?? string.Empty,

            // Account Identifiers
            Mqid = user.MQID() ?? string.Empty,
            ClientId = user.ClientID(),
            Status = user.Status() ?? string.Empty,
            Comment = user.Comment() ?? string.Empty,
            Color = user.Color(),

            // Security
            PhonePassword = user.PhonePassword() ?? string.Empty,
            LastPassChange = (long)user.LastPassChange(),
            PasswordHash = string.Empty, // Cannot retrieve without type parameter
            OtpSecret = user.OTPSecret() ?? string.Empty,

            // Trading Settings
            Leverage = user.Leverage(),
            Agent = user.Agent(),
            LimitOrders = user.LimitOrders(),
            LimitPositionsValue = (ulong)user.LimitPositionsValue(),

            // Marketing
            LeadSource = user.LeadSource() ?? string.Empty,
            LeadCampaign = user.LeadCampaign() ?? string.Empty,

            // Financial Information (Read-only)
            Balance = user.Balance(),
            BalancePrevDay = user.BalancePrevDay(),
            BalancePrevMonth = user.BalancePrevMonth(),
            EquityPrevDay = user.EquityPrevDay(),
            EquityPrevMonth = user.EquityPrevMonth(),
            Credit = user.Credit(),

            // Commission & Interest (Read-only)
            InterestRate = user.InterestRate(),
            CommissionDaily = user.CommissionDaily(),
            CommissionMonthly = user.CommissionMonthly(),
            CommissionAgentDaily = user.CommissionAgentDaily(),
            CommissionAgentMonthly = user.CommissionAgentMonthly()
        };
    }

    /// <summary>
    /// Update CIMTUser COM object from Proto UserModel
    /// </summary>
    public static void UpdateFromProto(this CIMTUser user, UserModel model)
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
        user.ZIPCode(model.ZipCode);
        user.Address(model.Address);
        user.Phone(model.Phone);
        user.EMail(model.Email);
        user.ID(model.Id);

        // Account Identifiers
        // MQID is read-only
        user.ClientID(model.ClientId);
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
    }
}
