using Microsoft.EntityFrameworkCore;
using Dom;
namespace CarShowroom.Repositories
{
    public class DiscountRepository : Repository<Discount>, IDiscountRepository
    {
        public DiscountRepository(CarShowroomDbContext context) : base(context)
        {
        }

        public async Task<List<Discount>> GetActiveDiscountsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            return await _dbSet
                .AsNoTracking()
                .Where(d =>
                    (!d.StartDate.HasValue || d.StartDate <= today) &&
                    (!d.EndDate.HasValue || d.EndDate >= today))
                .ToListAsync();
        }

        public async Task<List<Discount>> GetDiscountsByIdsAsync(List<int> discountIds)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(d => discountIds.Contains(d.Id))
                .ToListAsync();
        }

        public new async Task<Discount> AddAsync(Discount entity)
        {
            int nextId = 1;
            var discountsCount = await _dbSet.AsNoTracking().CountAsync();

            if (discountsCount > 0)
            {
                var maxId = await _dbSet
                    .AsNoTracking()
                    .MaxAsync(d => (int?)d.Id);

                if (maxId.HasValue)
                {
                    nextId = maxId.Value + 1;
                }
            }

            var newDiscount = new Discount
            {
                Id = nextId,
                Name = entity.Name,
                Description = entity.Description,
                Cost = entity.Cost,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate
            };

            await _dbSet.AddAsync(newDiscount);
            await _context.SaveChangesAsync();
            return newDiscount;
        }
    }
}
