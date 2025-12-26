using MT5Bridge.Manager.Models;
using Xunit;

namespace MT5Bridge.Manager.Tests;

public class UserModelTests
{
    [Fact]
    public void UserModel_Properties_ShouldWork()
    {
        // Arrange
        var user = new UserModel
        {
            Login = 12345,
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "Michael",
            Group = "demo\\main",
            Email = "john@example.com",
            Country = "USA",
            City = "New York",
            State = "NY",
            ZIPCode = "10001",
            Address = "123 Wall St",
            Phone = "123456789",
            Company = "Trading Corp",
            ID = "ID123456789"
        };

        // Assert
        Assert.Equal(12345u, user.Login);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("Michael", user.MiddleName);
        Assert.Equal("demo\\main", user.Group);
        Assert.Equal("john@example.com", user.Email);
        Assert.Equal("USA", user.Country);
        Assert.Equal("New York", user.City);
        Assert.Equal("NY", user.State);
        Assert.Equal("10001", user.ZIPCode);
        Assert.Equal("123 Wall St", user.Address);
        Assert.Equal("123456789", user.Phone);
        Assert.Equal("Trading Corp", user.Company);
        Assert.Equal("ID123456789", user.ID);
    }

    [Fact]
    public void UserModel_LegacyNameProperty_ShouldWork()
    {
        // Arrange
        var user = new UserModel
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
#pragma warning disable CS0618 // Type or member is obsolete
        user.Name = "Legacy Name";
        var name = user.Name;
#pragma warning restore CS0618

        // Assert
        Assert.Equal("Legacy Name", name);
        // Note: In our POCO model, Name is just a property, 
        // it doesn't automatically sync with FirstName/LastName 
        // unless we implement that logic. In the COM wrapper, 
        // the SDK handles the sync.
    }
}
