using Dom;
using CarShowroom.Interfaces;
using CarShowroom.Repositories;

namespace CarShowroom.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ICarRepository _carRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IAdditionRepository _additionRepository;
        private readonly IDiscountRepository _discountRepository;

        public SaleService(
            ISaleRepository saleRepository,
            ICarRepository carRepository,
            IUserRepository userRepository,
            IClientRepository clientRepository,
            IAdditionRepository additionRepository,
            IDiscountRepository discountRepository)
        {
            _saleRepository = saleRepository;
            _carRepository = carRepository;
            _userRepository = userRepository;
            _clientRepository = clientRepository;
            _additionRepository = additionRepository;
            _discountRepository = discountRepository;
        }

        public async Task<List<Sale>> GetAllSalesAsync()
        {
            return await _saleRepository.GetAllSalesWithDetailsAsync();
        }

        public async Task<Sale?> GetSaleByIdAsync(int id)
        {
            return await _saleRepository.GetSaleByIdWithDetailsAsync(id);
        }

        public async Task<List<Addition>> GetSaleAdditionsAsync(int saleId)
        {
            var additionIds = await _saleRepository.GetSaleAdditionIdsAsync(saleId);
            return await _additionRepository.GetAdditionsByIdsAsync(additionIds);
        }

        public async Task<List<Discount>> GetSaleDiscountsAsync(int saleId)
        {
            var discountIds = await _saleRepository.GetSaleDiscountIdsAsync(saleId);
            return await _discountRepository.GetDiscountsByIdsAsync(discountIds);
        }

        public async Task<Sale> CreateSaleAsync(Sale sale, List<int> additionIds, List<int> discountIds)
        {
            var car = await _carRepository.GetByIdAsync(sale.CarId);
            if (car == null)
            {
                throw new ArgumentException($"Автомобиль с ID {sale.CarId} не найден в базе данных");
            }
            
            if (car.Stock == false)
            {
                throw new ArgumentException("Этот автомобиль уже продан и не может быть продан повторно");
            }

            if (sale.ManagerId.HasValue)
            {
                var managerExists = await _userRepository.ExistsAsync(sale.ManagerId.Value);
                if (!managerExists)
                {
                    throw new ArgumentException($"Менеджер с ID {sale.ManagerId.Value} не найден в базе данных");
                }
            }

            if (sale.ClientId.HasValue)
            {
                var clientExists = await _clientRepository.ExistsAsync(sale.ClientId.Value);
                if (!clientExists)
                {
                    throw new ArgumentException($"Клиент с ID {sale.ClientId.Value} не найден в базе данных");
                }
            }
            
            var newSale = new Sale
            {
                CarId = sale.CarId,
                ManagerId = sale.ManagerId,
                ClientId = sale.ClientId,
                Date = sale.Date,
                Cost = sale.Cost
            };
            
            var createdSale = await _saleRepository.AddAsync(newSale);

                car.Stock = false;
            await _carRepository.UpdateAsync(car);

            if (additionIds.Any())
            {
                await _saleRepository.AddSaleAdditionsAsync(createdSale.Id, additionIds);
            }

            if (discountIds.Any())
            {
                await _saleRepository.AddSaleDiscountsAsync(createdSale.Id, discountIds);
            }

            return createdSale;
        }

        public async Task UpdateSaleAsync(Sale sale, List<int> additionIds, List<int> discountIds)
        {
            await _saleRepository.UpdateAsync(sale);

            await _saleRepository.RemoveSaleAdditionsAsync(sale.Id);
            await _saleRepository.RemoveSaleDiscountsAsync(sale.Id);

            if (additionIds.Any())
            {
                await _saleRepository.AddSaleAdditionsAsync(sale.Id, additionIds);
            }

            if (discountIds.Any())
            {
                await _saleRepository.AddSaleDiscountsAsync(sale.Id, discountIds);
            }
        }

        public async Task<List<Addition>> GetAllAdditionsAsync()
        {
            return (await _additionRepository.GetAllAsync()).ToList();
        }

        public async Task<List<Discount>> GetAllDiscountsAsync()
        {
            return (await _discountRepository.GetAllAsync()).ToList();
        }

        public async Task<Discount?> GetDiscountByIdAsync(int id)
        {
            return await _discountRepository.GetByIdAsync(id);
        }

        public async Task<Discount> CreateDiscountAsync(Discount discount)
        {
            return await _discountRepository.AddAsync(discount);
        }

        public async Task UpdateDiscountAsync(Discount discount)
        {
            await _discountRepository.UpdateAsync(discount);
        }

        public async Task DeleteDiscountAsync(int id)
        {
            var discount = await _discountRepository.GetByIdAsync(id);
            if (discount != null)
            {
                await _discountRepository.DeleteAsync(discount);
            }
        }

        public async Task<List<int>> GetSaleAdditionIdsAsync(int saleId)
        {
            return await _saleRepository.GetSaleAdditionIdsAsync(saleId);
        }

        public async Task<List<int>> GetSaleDiscountIdsAsync(int saleId)
        {
            return await _saleRepository.GetSaleDiscountIdsAsync(saleId);
        }

        public async Task<decimal> CalculateFinalPriceAsync(decimal basePrice, List<int> discountIds)
        {
            if (!discountIds.Any())
                return basePrice;

            var discounts = await _discountRepository.GetDiscountsByIdsAsync(discountIds);

            decimal totalDiscountPercent = 0;
            foreach (var discount in discounts)
            {
                if (discount.Cost.HasValue)
                {
                    totalDiscountPercent += (decimal)discount.Cost.Value;
                }
            }

            if (totalDiscountPercent > 100)
                totalDiscountPercent = 100;

            return basePrice * (1 - totalDiscountPercent / 100);
        }

        public async Task<decimal> CalculateOriginalPriceAsync(decimal finalPrice, List<int> discountIds)
        {
            if (!discountIds.Any())
                return finalPrice;

            var discounts = await _discountRepository.GetDiscountsByIdsAsync(discountIds);

            decimal totalDiscountPercent = 0;
            foreach (var discount in discounts)
            {
                if (discount.Cost.HasValue)
                {
                    totalDiscountPercent += (decimal)discount.Cost.Value;
                }
            }

            if (totalDiscountPercent > 100)
                totalDiscountPercent = 100;

            if (totalDiscountPercent >= 100)
                return finalPrice;

            return finalPrice / (1 - totalDiscountPercent / 100);
        }

        public async Task<int> GetClientPurchaseCountAsync(long? clientId, string? clientName)
        {
            if (clientId.HasValue)
            {
                return await _saleRepository.GetClientPurchaseCountAsync(clientId.Value);
            }

            if (!string.IsNullOrWhiteSpace(clientName))
            {
                var client = await _clientRepository.SearchClientByNameAsync(clientName);
                if (client != null)
                {
                    return await _saleRepository.GetClientPurchaseCountAsync(client.Id);
                }
            }

            return 0;
        }
    }
}