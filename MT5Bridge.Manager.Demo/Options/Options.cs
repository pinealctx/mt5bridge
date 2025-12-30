using CommandLine;

namespace MT5Bridge.Manager.Demo.Options;

public class CommonOptions
{
    [Option('c', "config", Default = "appsettings.json", HelpText = "Path to configuration file")]
    public string ConfigFile { get; set; } = "appsettings.json";

    [Option('s', "server", HelpText = "MT5 server address (e.g., localhost:443)")]
    public string? Server { get; set; }

    [Option('l', "login", HelpText = "Manager login")]
    public ulong? Login { get; set; }

    [Option('p', "password", HelpText = "Manager password")]
    public string? Password { get; set; }
}

[Verb("query-groups", HelpText = "List all user groups")]
public class QueryGroupsOptions : CommonOptions
{
}

[Verb("query-user", HelpText = "Query user information")]
public class QueryUserOptions : CommonOptions
{
    [Option("user-login", Required = true, HelpText = "User login to query")]
    public ulong UserLogin { get; set; }
}

[Verb("query-account", HelpText = "Query account information")]
public class QueryAccountOptions : CommonOptions
{
    [Option("user-login", Required = true, HelpText = "User login to query")]
    public ulong UserLogin { get; set; }
}

[Verb("query-deals", HelpText = "Query user deals")]
public class QueryDealsOptions : CommonOptions
{
    [Option("user-login", Required = true, HelpText = "User login to query")]
    public ulong UserLogin { get; set; }

    [Option('d', "days", Default = 7, HelpText = "Number of days to query")]
    public int Days { get; set; }
}

[Verb("query-balance", HelpText = "Query balance operations (deposits/withdrawals)")]
public class QueryBalanceOptions : CommonOptions
{
    [Option("user-login", Required = true, HelpText = "User login to query")]
    public ulong UserLogin { get; set; }

    [Option('d', "days", Default = 30, HelpText = "Number of days to query")]
    public int Days { get; set; }
}

[Verb("listen", HelpText = "Subscribe to real-time events")]
public class ListenOptions : CommonOptions
{
    [Option('t', "types", Default = "deal", HelpText = "Comma-separated list of event types (deal, order, position, all)")]
    public string Types { get; set; } = "deal";

    [Option("mode", Default = "poco", HelpText = "Handler mode: poco, protobuf, mixed")]
    public string Mode { get; set; } = "poco";
}

[Verb("deposit", HelpText = "Deposit funds to user account")]
public class DepositOptions : CommonOptions
{
    [Option("user-login", Required = true, HelpText = "User login")]
    public ulong UserLogin { get; set; }

    [Option("amount", Required = true, HelpText = "Deposit amount")]
    public double Amount { get; set; }

    [Option('c', "comment", HelpText = "Deposit comment")]
    public string? Comment { get; set; }
}

[Verb("withdraw", HelpText = "Withdraw funds from user account")]
public class WithdrawOptions : CommonOptions
{
    [Option("user-login", Required = true, HelpText = "User login")]
    public ulong UserLogin { get; set; }

    [Option("amount", Required = true, HelpText = "Withdrawal amount")]
    public double Amount { get; set; }

    [Option('c', "comment", HelpText = "Withdrawal comment")]
    public string? Comment { get; set; }
}
