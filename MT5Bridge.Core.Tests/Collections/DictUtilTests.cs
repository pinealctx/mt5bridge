using System.Collections.Concurrent;
using MT5Bridge.Core.Collections;

namespace MT5Bridge.Core.Tests.Collections;

public class DictUtilTests
{
    [Fact]
    public void GetThenRemove_WithExistingKey_ShouldReturnValueAndRemove()
    {
        var dict = new Dictionary<string, int>
        {
            ["apple"] = 1,
            ["banana"] = 2,
            ["cherry"] = 3
        };

        int value = DictUtil.GetThenRemove(dict, "banana", -1);

        Assert.Equal(2, value);
        Assert.False(dict.ContainsKey("banana"));
        Assert.Equal(2, dict.Count);
    }

    [Fact]
    public void GetThenRemove_WithNonExistentKey_ShouldReturnDefault()
    {
        var dict = new Dictionary<string, int>
        {
            ["apple"] = 1,
            ["banana"] = 2
        };

        int value = DictUtil.GetThenRemove(dict, "orange", -1);

        Assert.Equal(-1, value);
        Assert.Equal(2, dict.Count);
    }

    [Fact]
    public void GetThenRemove_WithNullValue_ShouldHandleCorrectly()
    {
        var dict = new Dictionary<string, string?>
        {
            ["key1"] = "value1",
            ["key2"] = null
        };

        string? value = DictUtil.GetThenRemove(dict, "key2", "default");

        Assert.Null(value);
        Assert.False(dict.ContainsKey("key2"));
    }

    [Fact]
    public void GetWithDefault_WithExistingKey_ShouldReturnValue()
    {
        var dict = new Dictionary<string, int>
        {
            ["apple"] = 1,
            ["banana"] = 2
        };

        int value = DictUtil.GetWithDefault(dict, "apple", -1);

        Assert.Equal(1, value);
        Assert.True(dict.ContainsKey("apple")); // Should not remove
    }

    [Fact]
    public void GetWithDefault_WithNonExistentKey_ShouldReturnDefault()
    {
        var dict = new Dictionary<string, int>
        {
            ["apple"] = 1,
            ["banana"] = 2
        };

        int value = DictUtil.GetWithDefault(dict, "orange", 999);

        Assert.Equal(999, value);
        Assert.Equal(2, dict.Count);
    }

    [Fact]
    public void GetWithDefault_WithNullValue_ShouldReturnNull()
    {
        var dict = new Dictionary<string, string?>
        {
            ["key1"] = "value1",
            ["key2"] = null
        };

        string? value = DictUtil.GetWithDefault(dict, "key2", "default");

        Assert.Null(value);
    }

    [Fact]
    public void GetWithDefault_WithComplexType_ShouldWork()
    {
        var defaultObj = new { Name = "Default", Value = 0 };
        var actualObj = new { Name = "Actual", Value = 42 };

        var dict = new Dictionary<string, object>
        {
            ["key1"] = actualObj
        };

        var value = DictUtil.GetWithDefault(dict, "key1", defaultObj);

        Assert.Equal(actualObj, value);
    }

    [Fact]
    public void ConvertToDictionary_WithConcurrentDictionary_ShouldConvert()
    {
        var concurrent = new ConcurrentDictionary<string, string>();
        concurrent["key1"] = "value1";
        concurrent["key2"] = "value2";
        concurrent["key3"] = "value3";

        var regular = DictUtil.ConvertToDictionary(concurrent);

        Assert.IsType<Dictionary<string, string>>(regular);
        Assert.Equal(3, regular.Count);
        Assert.Equal("value1", regular["key1"]);
        Assert.Equal("value2", regular["key2"]);
        Assert.Equal("value3", regular["key3"]);
    }

    [Fact]
    public void ConvertToDictionary_WithEmptyConcurrentDictionary_ShouldReturnEmpty()
    {
        var concurrent = new ConcurrentDictionary<string, string>();

        var regular = DictUtil.ConvertToDictionary(concurrent);

        Assert.Empty(regular);
    }

    [Fact]
    public void ConvertToDictionary_ShouldCreateSnapshot()
    {
        var concurrent = new ConcurrentDictionary<string, string>();
        concurrent["key1"] = "value1";

        var regular = DictUtil.ConvertToDictionary(concurrent);

        // Modify concurrent dictionary
        concurrent["key2"] = "value2";

        // Regular dictionary should not be affected
        Assert.Single(regular);
        Assert.False(regular.ContainsKey("key2"));
    }

    [Fact]
    public void GetThenRemove_WithIntKey_ShouldWork()
    {
        var dict = new Dictionary<int, string>
        {
            [1] = "one",
            [2] = "two",
            [3] = "three"
        };

        string value = DictUtil.GetThenRemove(dict, 2, "default");

        Assert.Equal("two", value);
        Assert.False(dict.ContainsKey(2));
    }
}
