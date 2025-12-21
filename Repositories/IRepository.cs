using System.Linq.Expressions;

namespace CarShowroom.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync<TKey>(TKey id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync<TKey>(TKey id);
    }
}

