using KarateSchool.Web.Data;
using KarateSchool.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace KarateSchool.Web.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        var normalized = email.Trim().ToLowerInvariant();
        return await DbSet.FirstOrDefaultAsync(u => u.Email == normalized);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        var normalized = email.Trim().ToLowerInvariant();
        return await DbSet.AnyAsync(u => u.Email == normalized);
    }

    public async Task<IReadOnlyList<Student>> SearchStudentsAsync(string? searchTerm)
    {
        var query = Context.Students
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.KarateClass)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            if (int.TryParse(term, out var id))
            {
                query = query.Where(s => s.UserId == id);
            }
            else
            {
                query = query.Where(s =>
                    s.FullName.Contains(term) ||
                    s.Enrollments.Any(e => e.KarateClass.ClassName.Contains(term)));
            }
        }

        return await query.ToListAsync();
    }
}
