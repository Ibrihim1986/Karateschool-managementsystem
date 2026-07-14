namespace KarateSchool.Web.Repositories;

/// <summary>Generic repository abstraction centralizing data access for a given entity type.</summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);

    Task<IReadOnlyList<T>> GetAllAsync();

    Task AddAsync(T entity);

    void Update(T entity);

    void Remove(T entity);

    Task SaveChangesAsync();
}
