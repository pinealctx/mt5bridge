using MT5Bridge.Core.Text;

namespace MT5Bridge.Core.Tests.Text;

public class StringXTests
{
    [Fact]
    public void SplitParenthesesTuple_WithValidInput_ShouldParse()
    {
        var (str1, str2, success) = StringX.SplitParenthesesTuple("(Hello):(World)");

        Assert.True(success);
        Assert.Equal("Hello", str1);
        Assert.Equal("World", str2);
    }

    [Fact]
    public void SplitParenthesesTuple_WithEmptyStrings_ShouldParse()
    {
        var (str1, str2, success) = StringX.SplitParenthesesTuple("():()");

        Assert.True(success);
        Assert.Equal("", str1);
        Assert.Equal("", str2);
    }

    [Fact]
    public void SplitParenthesesTuple_WithNull_ShouldReturnFailure()
    {
        var (str1, str2, success) = StringX.SplitParenthesesTuple(null);

        Assert.False(success);
        Assert.Equal(string.Empty, str1);
        Assert.Equal(string.Empty, str2);
    }

    [Fact]
    public void SplitParenthesesTuple_WithInvalidFormat_ShouldReturnFailure()
    {
        var (str1, str2, success) = StringX.SplitParenthesesTuple("Hello:World");

        Assert.False(success);
        Assert.Equal(string.Empty, str1);
        Assert.Equal(string.Empty, str2);
    }

    [Fact]
    public void SplitBy_WithValidInput_ShouldSplit()
    {
        var (part1, part2, success) = StringX.SplitBy("key:value", ":");

        Assert.True(success);
        Assert.Equal("key", part1);
        Assert.Equal("value", part2);
    }

    [Fact]
    public void SplitBy_WithEmptyParts_ShouldSplit()
    {
        var (part1, part2, success) = StringX.SplitBy(":value", ":");

        Assert.True(success);
        Assert.Equal("", part1);
        Assert.Equal("value", part2);
    }

    [Fact]
    public void SplitBy_WithNull_ShouldReturnFailure()
    {
        var (part1, part2, success) = StringX.SplitBy(null, ":");

        Assert.False(success);
        Assert.Equal(string.Empty, part1);
        Assert.Equal(string.Empty, part2);
    }

    [Fact]
    public void SplitBy_WithNoSeparator_ShouldReturnFailure()
    {
        var (part1, part2, success) = StringX.SplitBy("keyvalue", ":");

        Assert.False(success);
    }

    [Fact]
    public void SplitBy_WithMultipleSeparators_ShouldReturnFailure()
    {
        var (part1, part2, success) = StringX.SplitBy("key:value:extra", ":");

        Assert.False(success);
    }

    [Fact]
    public void ExtractStartUpParamValue_WithValidKey_ShouldExtract()
    {
        string input = "/name:xSyphon Gateway|/address:127.0.0.1:16385|/login:1";

        string? name = StringX.ExtractStartUpParamValue(input, "name");
        string? address = StringX.ExtractStartUpParamValue(input, "address");
        string? login = StringX.ExtractStartUpParamValue(input, "login");

        Assert.Equal("xSyphon Gateway", name);
        Assert.Equal("127.0.0.1:16385", address);
        Assert.Equal("1", login);
    }

    [Fact]
    public void ExtractStartUpParamValue_WithNonExistentKey_ShouldReturnNull()
    {
        string input = "/name:Test|/port:8080";

        string? result = StringX.ExtractStartUpParamValue(input, "missing");

        Assert.Null(result);
    }

    [Fact]
    public void ExtractStartUpParamValue_WithEmptyValue_ShouldExtractEmpty()
    {
        string input = "/name:|/port:8080";

        string? name = StringX.ExtractStartUpParamValue(input, "name");

        Assert.Equal("", name);
    }

    [Fact]
    public void ExtractStartUpParamValue_AtEndOfString_ShouldExtract()
    {
        string input = "/name:Test|/port:8080";

        string? port = StringX.ExtractStartUpParamValue(input, "port");

        Assert.Equal("8080", port);
    }

    [Fact]
    public void ArgsRemovePassword_WithPasswordArg_ShouldMask()
    {
        string[] args = { "/user:admin", "/password:secret123", "/host:localhost" };

        string sanitized = StringX.ArgsRemovePassword(args);

        Assert.Equal("/user:admin|/password:******|/host:localhost", sanitized);
    }

    [Fact]
    public void ArgsRemovePassword_WithoutPasswordArg_ShouldNotChange()
    {
        string[] args = { "/user:admin", "/host:localhost", "/port:8080" };

        string sanitized = StringX.ArgsRemovePassword(args);

        Assert.Equal("/user:admin|/host:localhost|/port:8080", sanitized);
    }

    [Fact]
    public void ArgsRemovePassword_CaseInsensitive_ShouldMask()
    {
        string[] args = { "/PASSWORD:secret", "/Password:test", "/PassWord:pass" };

        string sanitized = StringX.ArgsRemovePassword(args);

        Assert.Equal("/password:******|/password:******|/password:******", sanitized);
    }

    [Fact]
    public void ArgsRemovePassword_WithEmptyArray_ShouldReturnEmpty()
    {
        string[] args = Array.Empty<string>();

        string sanitized = StringX.ArgsRemovePassword(args);

        Assert.Equal("", sanitized);
    }

    [Fact]
    public void ArgsRemovePassword_WithMultiplePasswords_ShouldMaskAll()
    {
        string[] args = { "/password:first", "/user:admin", "/password:second" };

        string sanitized = StringX.ArgsRemovePassword(args);

        Assert.Equal("/password:******|/user:admin|/password:******", sanitized);
    }
}
