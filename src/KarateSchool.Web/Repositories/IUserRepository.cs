using KarateSchool.Web.Models.Entities;

namespace KarateSchool.Web.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);

    /// <summary>Centralized duplicate-identity check: each email may only be associated with one account.</summary>
    Task<bool> EmailExistsAsync(string email);

    Task<IReadOnlyList<Student>> SearchStudentsAsync(string? searchTerm);
}
