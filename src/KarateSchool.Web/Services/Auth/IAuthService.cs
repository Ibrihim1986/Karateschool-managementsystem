using KarateSchool.Web.Models.Entities;
using KarateSchool.Web.Models.ViewModels;

namespace KarateSchool.Web.Services.Auth;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password);

    Task<Student> RegisterStudentAsync(RegisterStudentViewModel model);
}
