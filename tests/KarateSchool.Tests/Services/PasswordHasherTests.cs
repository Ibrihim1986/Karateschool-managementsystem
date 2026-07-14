using KarateSchool.Web.Services.Auth;
using Xunit;

namespace KarateSchool.Tests.Services;

public class PasswordHasherTests
{
    private readonly IPasswordHasher _hasher = new Rfc2898PasswordHasher();

    [Fact]
    public void HashPassword_NeverReturnsThePlainTextPassword()
    {
        var hash = _hasher.HashPassword("Sup3r$ecret!");
        Assert.DoesNotContain("Sup3r$ecret!", hash);
    }

    [Fact]
    public void HashPassword_SamePasswordProducesDifferentHashesEachTime()
    {
        var hash1 = _hasher.HashPassword("Sup3r$ecret!");
        var hash2 = _hasher.HashPassword("Sup3r$ecret!");
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.HashPassword("Sup3r$ecret!");
        Assert.True(_hasher.VerifyPassword("Sup3r$ecret!", hash));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _hasher.HashPassword("Sup3r$ecret!");
        Assert.False(_hasher.VerifyPassword("wrong-password", hash));
    }

    [Fact]
    public void VerifyPassword_MalformedHash_ReturnsFalse()
    {
        Assert.False(_hasher.VerifyPassword("anything", "not-a-valid-hash"));
    }
}
