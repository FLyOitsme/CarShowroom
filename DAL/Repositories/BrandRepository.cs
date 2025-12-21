using Microsoft.EntityFrameworkCore;
using Dom;

namespace CarShowroom.Repositories
{
    public class BrandRepository : Repository<Brand>, IBrandRepository
    {
        public BrandRepository(CarShowroomDbContext context) : base(context)
        {
        }

        public async Task<List<Brand>> GetAllBrandsWithCountryAsync()
        {
            return await _dbSet
                .Include(b => b.Country)
                .ToListAsync();
        }
    }
}
