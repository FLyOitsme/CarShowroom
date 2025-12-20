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
                .AsNoTracking()
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
            return await _context.Discounts
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Discount?> GetDiscountByIdAsync(int id)
        {
            return await _context.Discounts
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Discount> CreateDiscountAsync(Discount discount)
        {
            // Получаем следующий доступный Id
            int nextId = 1;
            var discountsCount = await _context.Discounts.AsNoTracking().CountAsync();
            
            if (discountsCount > 0)
            {
                var maxId = await _context.Discounts
                    .AsNoTracking()
                    .MaxAsync(d => (int?)d.Id);
                
                if (maxId.HasValue)
                {
                    nextId = maxId.Value + 1;
                }
            }

            // Отсоединяем переданную сущность, если она отслеживается
            try
            {
                var entry = _context.Entry(discount);
                if (entry.State != EntityState.Detached)
                {
                    entry.State = EntityState.Detached;
                }
            }
            catch
            {
                // Игнорируем, если сущность не отслеживается
            }

            // Создаем новую сущность
            var newDiscount = new Discount
            {
                Id = nextId,
                Name = discount.Name,
                Description = discount.Description,
                Cost = discount.Cost,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate
            };

            _context.Discounts.Add(newDiscount);
            await _context.SaveChangesAsync();

            return newDiscount;
        }

        public async Task UpdateDiscountAsync(Discount discount)
        {
            // Проверяем, не отслеживается ли уже сущность с таким Id
            var existingEntity = _context.ChangeTracker.Entries<Discount>()
                .FirstOrDefault(e => e.Entity.Id == discount.Id);

            if (existingEntity != null)
            {
                // Если сущность уже отслеживается, отсоединяем её
                existingEntity.State = EntityState.Detached;
            }

            // Отсоединяем переданную сущность, если она отслеживается
            try
            {
                var entry = _context.Entry(discount);
                if (entry.State != EntityState.Detached)
                {
                    entry.State = EntityState.Detached;
                }
            }
            catch
            {
                // Игнорируем, если сущность не отслеживается
            }

            // Создаем новую сущность с теми же данными для обновления
            var discountToUpdate = new Discount
            {
                Id = discount.Id,
                Name = discount.Name,
                Description = discount.Description,
                Cost = discount.Cost,
                StartDate = discount.StartDate,
                EndDate = discount.EndDate
            };

            _context.Discounts.Update(discountToUpdate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDiscountAsync(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount != null)
            {
                _context.Discounts.Remove(discount);
                await _context.SaveChangesAsync();
            }
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
                .AsNoTracking()
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
                .AsNoTracking()
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

        public async Task<int> GetClientPurchaseCountAsync(int? clientId, string? clientName)
        {
            // Если есть ID клиента, используем его для поиска
            if (clientId.HasValue)
            {
                var client = await _context.Users
                    .AsNoTracking()
                    .Include(u => u.Sales)
                    .FirstOrDefaultAsync(u => u.Id == clientId.Value);
                
                if (client != null && client.Sales != null)
                {
                    return client.Sales.Count;
                }
            }

            // Если ID нет, но есть имя, пытаемся найти клиента по имени
            if (!string.IsNullOrWhiteSpace(clientName))
            {
                var nameParts = clientName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 2)
                {
                    var surname = nameParts[0];
                    var name = nameParts[1];
                    
                    var client = await _context.Users
                        .AsNoTracking()
                        .Include(u => u.Sales)
                        .Where(u => u.Surname == surname && u.Name == name)
                        .FirstOrDefaultAsync();
                    
                    if (client != null && client.Sales != null)
                    {
                        return client.Sales.Count;
                    }
                }
            }

            // Если клиент не найден, значит это первая покупка
            return 0;
        }
    }
}

