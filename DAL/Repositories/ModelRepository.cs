using Microsoft.EntityFrameworkCore;
using Dom;

namespace CarShowroom.Repositories
{
    public class ModelRepository : Repository<Model>, IModelRepository
    {
        public ModelRepository(CarShowroomDbContext context) : base(context)
        {
        }

        public async Task<List<Model>> GetModelsByBrandIdAsync(int brandId)
        {
            return await _dbSet
                .Where(m => m.BrandId == brandId)
                .ToListAsync();
        }
    }
}
