using Microsoft.EntityFrameworkCore;
using Dom;
namespace CarShowroom.Repositories
{
    public class CarRepository : Repository<Car>, ICarRepository
    {
        public CarRepository(CarShowroomDbContext context) : base(context)
        {
        }

        public async Task<List<Car>> GetAllCarsWithDetailsAsync()
        {
            return await _dbSet
                .Include(c => c.Model)
                    .ThenInclude(m => m!.Brand)
                .Include(c => c.Condition)
                .Include(c => c.Type)
                .Include(c => c.EngType)
                .Include(c => c.Transmission)
                .Include(c => c.Wd)
                .ToListAsync();
        }

        public async Task<Car?> GetCarByIdWithDetailsAsync(long id)
        {
            return await _dbSet
                .Include(c => c.Model)
                    .ThenInclude(m => m!.Brand)
                .Include(c => c.Condition)
                .Include(c => c.Type)
                .Include(c => c.EngType)
                .Include(c => c.Transmission)
                .Include(c => c.Wd)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Car>> SearchCarsAsync(string searchText)
        {
            var query = _dbSet
                .Where(c => c.Stock == true)
                .Include(c => c.Model)
                    .ThenInclude(m => m!.Brand)
                .Include(c => c.Condition)
                .Include(c => c.Type)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var lowerSearch = searchText.ToLower();
                query = query.Where(c =>
                    (c.Model != null && c.Model.Brand != null && c.Model.Brand.Name != null && c.Model.Brand.Name.ToLower().Contains(lowerSearch)) ||
                    (c.Model != null && c.Model.Name != null && c.Model.Name.ToLower().Contains(lowerSearch)) ||
                    (c.Color != null && c.Color.ToLower().Contains(lowerSearch))
                );
            }

            return await query.ToListAsync();
        }

        public async Task<List<Car>> GetAvailableCarsAsync()
        {
            return await _dbSet
                .Where(c => c.Stock == true)
                .Include(c => c.Model)
                    .ThenInclude(m => m!.Brand)
                .Include(c => c.Condition)
                .Include(c => c.Type)
                .Include(c => c.EngType)
                .Include(c => c.Transmission)
                .Include(c => c.Wd)
                .ToListAsync();
        }
    }
}
