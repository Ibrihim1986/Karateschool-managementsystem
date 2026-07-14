namespace KarateSchool.Web.Services.Auth;

public interface IPasswordHasher
{
    string HashPassword(string plainTextPassword);

    bool VerifyPassword(string plainTextPassword, string passwordHash);
}
