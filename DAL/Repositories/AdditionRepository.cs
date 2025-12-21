using Microsoft.EntityFrameworkCore;
using Dom;

namespace CarShowroom.Repositories
{
    public class AdditionRepository : Repository<Addition>, IAdditionRepository
    {
        public AdditionRepository(CarShowroomDbContext context) : base(context)
        {
        }

        public async Task<List<Addition>> GetAdditionsByIdsAsync(List<int> additionIds)
        {
            return await _dbSet
                .Where(a => additionIds.Contains(a.Id))
                .ToListAsync();
        }
    }
}
