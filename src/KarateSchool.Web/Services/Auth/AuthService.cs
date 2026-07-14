using KarateSchool.Web.Models.Entities;
using KarateSchool.Web.Models.ViewModels;
using KarateSchool.Web.Repositories;
using KarateSchool.Web.Services.Factories;

namespace KarateSchool.Web.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));

        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null)
            return LoginResult.Invalid();

        if (user.IsLockedOut)
            return LoginResult.LockedOutUntil(user.LockoutEnd!.Value);

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            user.RegisterFailedLogin();
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return user.IsLockedOut
                ? LoginResult.LockedOutUntil(user.LockoutEnd!.Value)
                : LoginResult.Invalid();
        }

        user.ResetFailedLogins();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return LoginResult.Success(user);
    }

    public async Task<Student> RegisterStudentAsync(RegisterStudentViewModel model)
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model));

        // Duplicate-identity enforcement: each email may only be associated with one account.
        if (await _userRepository.EmailExistsAsync(model.Email))
            throw new InvalidOperationException("An account with this email already exists.");

        PasswordPolicy.Validate(model.Password);

        var passwordHash = _passwordHasher.HashPassword(model.Password);

        var student = UserFactory.CreateStudent(
            model.FullName,
            model.Email,
            passwordHash,
            model.Phone,
            model.DateOfBirth,
            beltRank: "White Belt",
            enrollmentDate: DateTime.UtcNow.Date,
            emergencyContact: model.EmergencyContact);

        await _userRepository.AddAsync(student);
        await _userRepository.SaveChangesAsync();

        return student;
    }
}
