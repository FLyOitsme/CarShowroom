using DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using CarShowroom.Interfaces;

namespace CarShowroom.Services
{
    public class SaleService : ISaleService
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
                .AsNoTracking()
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Model)
                        .ThenInclude(m => m!.Brand)
                .Include(s => s.Manager)
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Condition)
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Transmission)
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Type)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Addition>> GetSaleAdditionsAsync(int saleId)
        {
            var additionIds = await GetSaleAdditionIdsAsync(saleId);
            return await _context.Additions
                .Where(a => additionIds.Contains(a.Id))
                .ToListAsync();
        }

        public async Task<List<Discount>> GetSaleDiscountsAsync(int saleId)
        {
            var discountIds = await GetSaleDiscountIdsAsync(saleId);
            return await _context.Discounts
                .Where(d => discountIds.Contains(d.Id))
                .ToListAsync();
        }

        public async Task<Sale> CreateSaleAsync(Sale sale, List<int> additionIds, List<int> discountIds)
        {
            // Проверяем, что все необходимые данные присутствуют
            if (sale.CarId == 0)
            {
                throw new ArgumentException("CarId не может быть равен 0");
            }

            // Проверяем существование автомобиля в базе
            var carExists = await _context.Cars.AnyAsync(c => c.Id == sale.CarId);
            if (!carExists)
            {
                throw new ArgumentException($"Автомобиль с ID {sale.CarId} не найден в базе данных");
            }

            // Проверяем существование менеджера в базе (если указан)
            if (sale.ManagerId.HasValue)
            {
                var managerExists = await _context.Users.AnyAsync(u => u.Id == sale.ManagerId.Value);
                if (!managerExists)
                {
                    throw new ArgumentException($"Менеджер с ID {sale.ManagerId.Value} не найден в базе данных");
                }
            }
            
            // Сохраняем значения полей перед созданием новой сущности
            var carId = sale.CarId;
            var managerId = sale.ManagerId;
            var date = sale.Date;
            var cost = sale.Cost;
            
            // Отсоединяем переданную сущность, если она отслеживается
            try
            {
                var entry = _context.Entry(sale);
                if (entry.State != EntityState.Detached)
                {
                    entry.State = EntityState.Detached;
                }
            }
            catch
            {
                // Игнорируем, если сущность не отслеживается
            }
            
            // Проверяем, нет ли уже отслеживаемых сущностей Sale с таким же Id (если Id был установлен)
            // Очищаем отслеживание всех сущностей Sale, чтобы избежать конфликтов
            var trackedSales = _context.ChangeTracker.Entries<Sale>()
                .Where(e => e.State != EntityState.Detached)
                .ToList();
            
            foreach (var trackedSale in trackedSales)
            {
                trackedSale.State = EntityState.Detached;
            }
            
            // Получаем следующий доступный Id из базы данных
            // Так как ValueGeneratedNever(), нужно вручную получить следующий Id
            int nextId = 1;
            var salesCount = await _context.Sales.AsNoTracking().CountAsync();
            
            if (salesCount > 0)
            {
                // Если есть записи, получаем максимальный Id
                var maxId = await _context.Sales
                    .AsNoTracking()
                    .MaxAsync(s => (int?)s.Id);
                
                if (maxId.HasValue)
                {
                    nextId = maxId.Value + 1;
                }
            }
            
            // Создаем полностью новую сущность только с необходимыми полями
            // Не используем навигационные свойства, чтобы избежать конфликтов отслеживания
            var newSale = new Sale
            {
                Id = nextId, // Устанавливаем следующий доступный Id
                CarId = carId,
                ManagerId = managerId,
                Date = date,
                Cost = cost
            };
            
            try
            {
                _context.Sales.Add(newSale);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                // Обрабатываем ошибки базы данных более детально
                var errorDetails = dbEx.Message;
                if (dbEx.InnerException != null)
                {
                    errorDetails += $"\nВнутренняя ошибка: {dbEx.InnerException.Message}";
                }
                throw new Exception($"Ошибка при сохранении продажи в базу данных: {errorDetails}", dbEx);
            }
            catch (Exception ex)
            {
                // Логируем детали ошибки для отладки
                throw new Exception($"Ошибка при сохранении продажи: {ex.Message}", ex);
            }
            
            return newSale;

            // Добавляем дополнительные опции
            if (additionIds.Any())
            {
                foreach (var addId in additionIds)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO ""Sale_Additions"" (""Sale_Id"", ""Add_Id"") VALUES ({0}, {1})",
                        newSale.Id, addId);
                }
            }

            // Добавляем скидки
            if (discountIds.Any())
            {
                foreach (var discountId in discountIds)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO ""Sale_Discount"" (""Sale_Id"", ""Discount_Id"") VALUES ({0}, {1})",
                        newSale.Id, discountId);
                }
            }

            return newSale;
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

        public async Task<decimal> CalculateOriginalPriceAsync(decimal finalPrice, List<int> discountIds)
        {
            if (!discountIds.Any())
                return finalPrice;

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

            // Обратный расчет: finalPrice = basePrice * (1 - discount/100)
            // basePrice = finalPrice / (1 - discount/100)
            if (totalDiscountPercent >= 100)
                return finalPrice;

            return finalPrice / (1 - totalDiscountPercent / 100);
        }
    }
}

