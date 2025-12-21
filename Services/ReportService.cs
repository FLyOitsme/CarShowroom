using DataLayer.Entities;
using Microsoft.EntityFrameworkCore;
using CarShowroom.Interfaces;

namespace CarShowroom.Services
{
    public class ReportService : IReportService
    {
        private readonly CarShowroomDbContext _context;

        public ReportService(CarShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTotalSalesCountAsync()
        {
            return await _context.Sales
                .AsNoTracking()
                .CountAsync()
                .ConfigureAwait(false);
        }

        public async Task<int> GetTotalSalesCountByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var start = DateOnly.FromDateTime(startDate);
            var end = DateOnly.FromDateTime(endDate);
            
            return await _context.Sales
                .AsNoTracking()
                .Where(s => s.Date >= start && s.Date <= end)
                .CountAsync()
                .ConfigureAwait(false);
        }

        public async Task<int> GetAvailableCarsCountAsync()
        {
            return await _context.Cars
                .AsNoTracking()
                .Where(c => c.Stock == true)
                .CountAsync()
                .ConfigureAwait(false);
        }

        public async Task<int> GetSoldCarsCountAsync()
        {
            // Количество уникальных автомобилей, которые были проданы
            return await _context.Sales
                .AsNoTracking()
                .Select(s => s.CarId)
                .Distinct()
                .CountAsync()
                .ConfigureAwait(false);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            var total = await _context.Sales
                .AsNoTracking()
                .SumAsync(s => (decimal?)(s.Cost ?? 0))
                .ConfigureAwait(false);
            
            return total ?? 0;
        }

        public async Task<decimal> GetRevenueByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var start = DateOnly.FromDateTime(startDate);
            var end = DateOnly.FromDateTime(endDate);
            
            var total = await _context.Sales
                .AsNoTracking()
                .Where(s => s.Date >= start && s.Date <= end)
                .SumAsync(s => (decimal?)(s.Cost ?? 0))
                .ConfigureAwait(false);
            
            return total ?? 0;
        }

        public async Task<decimal> GetAverageSalePriceAsync()
        {
            var avg = await _context.Sales
                .AsNoTracking()
                .Where(s => s.Cost.HasValue)
                .AverageAsync(s => (decimal?)(s.Cost ?? 0))
                .ConfigureAwait(false);
            
            return avg ?? 0;
        }

        public async Task<int> GetActiveDiscountsCountAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            
            return await _context.Discounts
                .AsNoTracking()
                .Where(d => 
                    (!d.StartDate.HasValue || d.StartDate <= today) &&
                    (!d.EndDate.HasValue || d.EndDate >= today))
                .CountAsync()
                .ConfigureAwait(false);
        }

        public async Task<Dictionary<string, int>> GetSalesByBrandAsync()
        {
            var sales = await _context.Sales
                .AsNoTracking()
                .Include(s => s.Car)
                    .ThenInclude(c => c!.Model)
                        .ThenInclude(m => m!.Brand)
                .Where(s => s.Car != null && s.Car.Model != null && s.Car.Model.Brand != null)
                .GroupBy(s => s.Car!.Model!.Brand!.Name ?? "Неизвестно")
                .Select(g => new { Brand = g.Key, Count = g.Count() })
                .ToListAsync()
                .ConfigureAwait(false);
            
            return sales.ToDictionary(s => s.Brand, s => s.Count);
        }

        public async Task<Dictionary<string, int>> GetSalesByManagerAsync()
        {
            // Загружаем только ManagerId из продаж, чтобы избежать проблем с Include и LeftJoin
            var salesWithManagers = await _context.Sales
                .AsNoTracking()
                .Where(s => s.ManagerId != null)
                .Select(s => new { ManagerId = s.ManagerId!.Value })
                .ToListAsync()
                .ConfigureAwait(false);

            // Получаем уникальные ID менеджеров
            var managerIds = salesWithManagers
                .Select(s => s.ManagerId)
                .Distinct()
                .ToList();

            // Загружаем менеджеров отдельным запросом
            var managers = await _context.Users
                .AsNoTracking()
                .Where(u => managerIds.Contains(u.Id))
                .Select(u => new {
                    u.Id,
                    Surname = u.Surname ?? string.Empty,
                    Name = u.Name ?? string.Empty,
                    Patronyc = u.Patronyc ?? string.Empty
                })
                .ToListAsync()
                .ConfigureAwait(false);

            // Создаем словарь для быстрого поиска менеджеров
            var managerDict = managers.ToDictionary(m => m.Id, m =>
                $"{m.Surname} {m.Name} {m.Patronyc}".Trim());

            // Группируем продажи по менеджерам в памяти
            var grouped = salesWithManagers
                .GroupBy(s => managerDict.GetValueOrDefault(s.ManagerId, "Неизвестно"))
                .Select(g => new { Manager = g.Key, Count = g.Count() })
                .ToList();

            return grouped.ToDictionary(s => s.Manager, s => s.Count);
        }

        public async Task<List<MonthlyStatistic>> GetMonthlyStatisticsAsync(int year)
        {
            var monthNames = new[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
            
            var startOfYear = new DateOnly(year, 1, 1);
            var endOfYear = new DateOnly(year, 12, 31);
            
            // Получаем все продажи за год одним запросом
            var sales = await _context.Sales
                .AsNoTracking()
                .Where(s => s.Date >= startOfYear && s.Date <= endOfYear)
                .Select(s => new { s.Date, s.Cost })
                .ToListAsync()
                .ConfigureAwait(false);
            
            var statistics = new List<MonthlyStatistic>();
            
            // Группируем по месяцам в памяти
            for (int month = 1; month <= 12; month++)
            {
                var monthSales = sales
                    .Where(s => s.Date.HasValue && s.Date.Value.Year == year && s.Date.Value.Month == month)
                    .ToList();
                
                statistics.Add(new MonthlyStatistic
                {
                    Month = month,
                    MonthName = monthNames[month - 1],
                    SalesCount = monthSales.Count,
                    Revenue = monthSales.Sum(s => (decimal)(s.Cost ?? 0))
                });
            }
            
            return statistics;
        }
    }
}

