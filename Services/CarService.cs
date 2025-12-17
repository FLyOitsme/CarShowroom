using DataLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarShowroom.Services
{
    public class CarService
    {
        private readonly CarShowroomDbContext _context;

        public CarService(CarShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<List<Car>> GetAllCarsAsync()
        {
            return await _context.Cars
                .Include(c => c.Model)
                    .ThenInclude(m => m!.Brand)
                .Include(c => c.Condition)
                .Include(c => c.Type)
                .Include(c => c.EngType)
                .Include(c => c.Transmission)
                .Include(c => c.Wd)
                .ToListAsync();
        }

        public async Task<Car?> GetCarByIdAsync(long id)
        {
            return await _context.Cars
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
            var query = _context.Cars
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

        public async Task AddCarAsync(Car car)
        {
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCarAsync(Car car)
        {
            _context.Cars.Update(car);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCarAsync(long id)
        {
            var car = await _context.Cars.FindAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Brand>> GetAllBrandsAsync()
        {
            return await _context.Brands
                .Include(b => b.Country)
                .ToListAsync();
        }

        public async Task<List<Model>> GetModelsByBrandIdAsync(int brandId)
        {
            return await _context.Models
                .Where(m => m.BrandId == brandId)
                .ToListAsync();
        }

        public async Task<List<CarType>> GetAllCarTypesAsync()
        {
            return await _context.CarTypes.ToListAsync();
        }

        public async Task<List<ConditionType>> GetAllConditionTypesAsync()
        {
            return await _context.ConditionTypes.ToListAsync();
        }

        public async Task<List<EngineType>> GetAllEngineTypesAsync()
        {
            return await _context.EngineTypes.ToListAsync();
        }

        public async Task<List<Transmission>> GetAllTransmissionsAsync()
        {
            return await _context.Transmissions.ToListAsync();
        }

        public async Task<List<Wdtype>> GetAllWdTypesAsync()
        {
            return await _context.Wdtypes.ToListAsync();
        }
    }
}
