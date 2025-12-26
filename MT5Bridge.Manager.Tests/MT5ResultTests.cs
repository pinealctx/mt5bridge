using MetaQuotes.MT5CommonAPI;
using Xunit;

namespace MT5Bridge.Manager.Tests;

public class MT5ResultTests
{
    [Fact]
    public void Success_ShouldSetPropertiesCorrectly()
    {
        // Act
        var result = MT5Result.Success("Operation completed");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(MTRetCode.MT_RET_OK, result.RetCode);
        Assert.Equal("Operation completed", result.Message);
    }

    [Fact]
    public void Failure_WithRetCode_ShouldSetPropertiesCorrectly()
    {
        // Act
        var result = MT5Result.Failure(MTRetCode.MT_RET_ERR_PARAMS, "Invalid parameters");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(MTRetCode.MT_RET_ERR_PARAMS, result.RetCode);
        Assert.Equal("Invalid parameters", result.Message);
    }

    [Fact]
    public void Failure_WithMessageOnly_ShouldSetDefaultRetCode()
    {
        // Act
        var result = MT5Result.Failure("Generic error");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(MTRetCode.MT_RET_ERROR, result.RetCode);
        Assert.Equal("Generic error", result.Message);
    }

    [Fact]
    public void GenericSuccess_ShouldIncludeData()
    {
        // Arrange
        var data = "Test Data";

        // Act
        var result = MT5Result<string>.Success(data, "Data retrieved");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(MTRetCode.MT_RET_OK, result.RetCode);
        Assert.Equal("Data retrieved", result.Message);
        Assert.Equal(data, result.Data);
    }

    [Fact]
    public void GenericFailure_ShouldNotHaveData()
    {
        // Act
        var result = MT5Result<int>.Failure(MTRetCode.MT_RET_ERR_NOTFOUND, "Not found");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(MTRetCode.MT_RET_ERR_NOTFOUND, result.RetCode);
        Assert.Equal("Not found", result.Message);
        Assert.Equal(0, result.Data);
    }
}
