using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Dom;

namespace CarShowroom.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly CarShowroomDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(CarShowroomDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync<TKey>(TKey id)
        {
            return await _dbSet.FindAsync(new object[] { id! });
        }

        public virtual async Task<T?> GetByIdAsync<TKey>(TKey id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            var parameter = Expression.Parameter(typeof(T), "e");
            var property = Expression.Property(parameter, "Id");
            var constant = Expression.Constant(id);
            var equality = Expression.Equal(property, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);
            
            return await query.FirstOrDefaultAsync(lambda);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.Where(predicate).ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            var entry = _context.Entry(entity);
            
            if (entry.State == EntityState.Detached)
            {
                var idProperty = typeof(T).GetProperty("Id");
                if (idProperty != null)
                {
                    var idValue = idProperty.GetValue(entity);
                    if (idValue != null)
                    {
                        var trackedEntity = await _dbSet.FindAsync(idValue);
                        if (trackedEntity != null && !ReferenceEquals(trackedEntity, entity))
                        {
                            _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
                            await _context.SaveChangesAsync();
                            return;
                        }
                    }
                }
                
                _dbSet.Update(entity);
            }
            else
            {
                entry.State = EntityState.Modified;
            }
            
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task<bool> ExistsAsync<TKey>(TKey id)
        {
            var entity = await GetByIdAsync(id);
            return entity != null;
        }

        public virtual IQueryable<T> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public virtual IQueryable<T> GetQueryable(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return query;
        }
    }
}
