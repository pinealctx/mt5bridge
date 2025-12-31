using MT5Bridge.Core.Collections;
using Xunit;

namespace MT5Bridge.Core.Tests.Collections;

public class ArrayUtilsTests
{
    [Fact]
    public void Convert_EmptySource_ReturnsEmptyArray()
    {
        // Arrange
        uint count = 0;
        Func<uint, string?> getItem = i => "item" + i;
        Func<string, string> converter = s => s.ToUpper();

        // Act
        var result = ArrayUtils.Convert(count, getItem, converter);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void Convert_AllItemsValid_ReturnsConvertedArray()
    {
        // Arrange
        var source = new[] { "a", "b", "c" };
        uint count = (uint)source.Length;
        Func<uint, string?> getItem = i => source[i];
        Func<string, string> converter = s => s.ToUpper();

        // Act
        var result = ArrayUtils.Convert(count, getItem, converter);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("A", result[0]);
        Assert.Equal("B", result[1]);
        Assert.Equal("C", result[2]);
    }

    [Fact]
    public void Convert_WithNulls_SkipsNullsAndResizes()
    {
        // Arrange
        var source = new[] { "a", null, "c", null, "e" };
        uint count = (uint)source.Length;
        Func<uint, string?> getItem = i => source[i];
        Func<string, string> converter = s => s.ToUpper();

        // Act
        var result = ArrayUtils.Convert(count, getItem, converter);

        // Assert
        Assert.Equal(3, result.Length);
        Assert.Equal("A", result[0]);
        Assert.Equal("C", result[1]);
        Assert.Equal("E", result[2]);
    }

    [Fact]
    public void Convert_WithFilter_FiltersItemsAndResizes()
    {
        // Arrange
        var source = new[] { 1, 2, 3, 4, 5 };
        uint count = (uint)source.Length;
        Func<uint, int?> getItem = i => source[i];
        // Use int? as TSource to allow null checks inside ArrayUtils (though source has no nulls here)
        Func<int?, string> converter = i => $"Num{i}";
        Func<int?, bool> filter = i => i.HasValue && i.Value % 2 == 0; // Keep evens

        // Act
        var result = ArrayUtils.Convert<int?, string>(count, getItem, converter, filter);

        // Assert
        Assert.Equal(2, result.Length); // 2, 4
        Assert.Equal("Num2", result[0]);
        Assert.Equal("Num4", result[1]);
    }

    [Fact]
    public void Convert_WithNullsAndFilter_HandlesBothCorrectly()
    {
        // Arrange
        var source = new int?[] { 1, null, 2, 3, null, 4, 5 };
        uint count = (uint)source.Length;
        Func<uint, int?> getItem = i => source[i];
        Func<int?, string> converter = i => $"Num{i}";
        Func<int?, bool> filter = i => i.HasValue && i.Value % 2 == 0; // Keep evens

        // Act
        var result = ArrayUtils.Convert<int?, string>(count, getItem, converter, filter);

        // Assert
        // Valid items: 1, 2, 3, 4, 5
        // Filter (even): 2, 4
        Assert.Equal(2, result.Length);
        Assert.Equal("Num2", result[0]);
        Assert.Equal("Num4", result[1]);
    }
}
