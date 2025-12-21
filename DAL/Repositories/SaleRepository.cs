using Microsoft.EntityFrameworkCore;
using Dom;

namespace CarShowroom.Repositories
{
    public class SaleRepository : Repository<Sale>, ISaleRepository
    {
        public SaleRepository(CarShowroomDbContext context) : base(context)
        {
        }

        public async Task<List<Sale>> GetAllSalesWithDetailsAsync()
        {
            return await _dbSet
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Model)
                        .ThenInclude(m => m!.Brand)
                .Include(s => s.Manager)
                .Include(s => s.Client)
                .OrderByDescending(s => s.Date ?? DateOnly.MinValue)
                .ToListAsync();
        }

        public async Task<Sale?> GetSaleByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Model)
                        .ThenInclude(m => m!.Brand)
                .Include(s => s.Manager)
                .Include(s => s.Client)
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Condition)
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Transmission)
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Type)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<int>> GetSaleAdditionIdsAsync(int saleId)
        {
            var result = await _context.Database
                .SqlQueryRaw<int>(@"SELECT ""Add_Id"" FROM ""Sale_Additions"" WHERE ""Sale_Id"" = {0}", saleId)
                .ToListAsync();
            return result;
        }

        public async Task<List<int>> GetSaleDiscountIdsAsync(int saleId)
        {
            var result = await _context.Database
                .SqlQueryRaw<int>(@"SELECT ""Discount_Id"" FROM ""Sale_Discount"" WHERE ""Sale_Id"" = {0}", saleId)
                .ToListAsync();
            return result;
        }

        public async Task<int> GetClientPurchaseCountAsync(long? clientId)
        {
            if (!clientId.HasValue)
                return 0;

            return await _dbSet
                .AsNoTracking()
                .CountAsync(s => s.ClientId == clientId.Value);
        }

        public async Task<List<Sale>> GetSalesByClientIdAsync(long clientId)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.ClientId == clientId)
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Model)
                        .ThenInclude(m => m!.Brand)
                .ToListAsync();
        }

        public async Task<List<Sale>> GetSalesByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(s => s.Date >= startDate && s.Date <= endDate)
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Model)
                        .ThenInclude(m => m!.Brand)
                .Include(s => s.Manager)
                .Include(s => s.Client)
                .ToListAsync();
        }

        public new async Task<Sale> AddAsync(Sale entity)
        {
            int nextId = 1;
            var salesCount = await _dbSet.AsNoTracking().CountAsync();

            if (salesCount > 0)
            {
                var maxId = await _dbSet
                    .AsNoTracking()
                    .MaxAsync(s => (int?)s.Id);

                if (maxId.HasValue)
                {
                    nextId = maxId.Value + 1;
                }
            }

            var newSale = new Sale
            {
                Id = nextId,
                CarId = entity.CarId,
                ManagerId = entity.ManagerId,
                ClientId = entity.ClientId,
                Date = entity.Date,
                Cost = entity.Cost
            };

            await _dbSet.AddAsync(newSale);
            await _context.SaveChangesAsync();
            return newSale;
        }

        public async Task AddSaleAdditionsAsync(int saleId, List<int> additionIds)
        {
            foreach (var addId in additionIds)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO ""Sale_Additions"" (""Sale_Id"", ""Add_Id"") VALUES ({0}, {1})",
                    saleId, addId);
            }
        }

        public async Task AddSaleDiscountsAsync(int saleId, List<int> discountIds)
        {
            foreach (var discountId in discountIds)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    @"INSERT INTO ""Sale_Discount"" (""Sale_Id"", ""Discount_Id"") VALUES ({0}, {1})",
                    saleId, discountId);
            }
        }

        public async Task RemoveSaleAdditionsAsync(int saleId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"DELETE FROM ""Sale_Additions"" WHERE ""Sale_Id"" = {0}", saleId);
        }

        public async Task RemoveSaleDiscountsAsync(int saleId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"DELETE FROM ""Sale_Discount"" WHERE ""Sale_Id"" = {0}", saleId);
        }
    }
}
