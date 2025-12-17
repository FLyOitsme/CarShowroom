using DataLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarShowroom.Services
{
    public class SaleService
    {
        private readonly CarShowroomDbContext _context;

        public SaleService(CarShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<List<Sale>> GetAllSalesAsync()
        {
            return await _context.Sales
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Model)
                        .ThenInclude(m => m!.Brand)
                .Include(s => s.Manager)
                .ToListAsync();
        }

        public async Task<Sale?> GetSaleByIdAsync(int id)
        {
            return await _context.Sales
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Model)
                        .ThenInclude(m => m!.Brand)
                .Include(s => s.Manager)
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Condition)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Sale> CreateSaleAsync(Sale sale, List<int> additionIds, List<int> discountIds)
        {
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            // Добавляем дополнительные опции
            if (additionIds.Any())
            {
                foreach (var addId in additionIds)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO ""Sale_Additions"" (""Sale_Id"", ""Add_Id"") VALUES ({0}, {1})",
                        sale.Id, addId);
                }
            }

            // Добавляем скидки
            if (discountIds.Any())
            {
                foreach (var discountId in discountIds)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO ""Sale_Discount"" (""Sale_Id"", ""Discount_Id"") VALUES ({0}, {1})",
                        sale.Id, discountId);
                }
            }

            return sale;
        }

        public async Task UpdateSaleAsync(Sale sale, List<int> additionIds, List<int> discountIds)
        {
            _context.Sales.Update(sale);
            await _context.SaveChangesAsync();

            // Удаляем старые связи
            await _context.Database.ExecuteSqlRawAsync(
                @"DELETE FROM ""Sale_Additions"" WHERE ""Sale_Id"" = {0}", sale.Id);
            await _context.Database.ExecuteSqlRawAsync(
                @"DELETE FROM ""Sale_Discount"" WHERE ""Sale_Id"" = {0}", sale.Id);

            // Добавляем новые связи
            if (additionIds.Any())
            {
                foreach (var addId in additionIds)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO ""Sale_Additions"" (""Sale_Id"", ""Add_Id"") VALUES ({0}, {1})",
                        sale.Id, addId);
                }
            }

            if (discountIds.Any())
            {
                foreach (var discountId in discountIds)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO ""Sale_Discount"" (""Sale_Id"", ""Discount_Id"") VALUES ({0}, {1})",
                        sale.Id, discountId);
                }
            }
        }

        public async Task<List<Addition>> GetAllAdditionsAsync()
        {
            return await _context.Additions.ToListAsync();
        }

        public async Task<List<Discount>> GetAllDiscountsAsync()
        {
            return await _context.Discounts.ToListAsync();
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

        public async Task<decimal> CalculateFinalPriceAsync(decimal basePrice, List<int> discountIds)
        {
            if (!discountIds.Any())
                return basePrice;

            var discounts = await _context.Discounts
                .Where(d => discountIds.Contains(d.Id))
                .ToListAsync();

            decimal totalDiscountPercent = 0;
            foreach (var discount in discounts)
            {
                if (discount.Cost.HasValue)
                {
                    totalDiscountPercent += (decimal)discount.Cost.Value;
                }
            }

            // Ограничиваем максимальную скидку 100%
            if (totalDiscountPercent > 100)
                totalDiscountPercent = 100;

            return basePrice * (1 - totalDiscountPercent / 100);
        }
    }
}

