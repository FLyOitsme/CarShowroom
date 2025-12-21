using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace CarShowroom.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync<TKey>(TKey id);
        Task<T?> GetByIdAsync<TKey>(TKey id, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<bool> ExistsAsync<TKey>(TKey id);
        IQueryable<T> GetQueryable();
        IQueryable<T> GetQueryable(params Expression<Func<T, object>>[] includes);
    }
}


