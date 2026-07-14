using KarateSchool.Web.Services.Auth;
using Xunit;

namespace KarateSchool.Tests.Services;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("short1!")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoDigitsHere!")]
    [InlineData("NoSpecialChar1")]
    public void Validate_WeakPassword_ThrowsArgumentException(string weakPassword)
    {
        Assert.Throws<ArgumentException>(() => PasswordPolicy.Validate(weakPassword));
    }

    [Fact]
    public void Validate_StrongPassword_DoesNotThrow()
    {
        var exception = Record.Exception(() => PasswordPolicy.Validate("Str0ng!Passw0rd"));
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NullPassword_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => PasswordPolicy.Validate(null!));
    }
}
