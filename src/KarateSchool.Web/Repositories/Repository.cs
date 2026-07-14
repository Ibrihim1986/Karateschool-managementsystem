using KarateSchool.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace KarateSchool.Web.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(ApplicationDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllAsync() => await DbSet.ToListAsync();

    public async Task AddAsync(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));
        await DbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));
        DbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));
        DbSet.Remove(entity);
    }

    public Task SaveChangesAsync() => Context.SaveChangesAsync();
}
