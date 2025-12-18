using DataLayer.Entities;

namespace CarShowroom.Interfaces
{
    public interface ISaleService
    {
        Task<List<Sale>> GetAllSalesAsync();
        Task<Sale?> GetSaleByIdAsync(int id);
        Task<List<Addition>> GetSaleAdditionsAsync(int saleId);
        Task<List<Discount>> GetSaleDiscountsAsync(int saleId);
        Task<Sale> CreateSaleAsync(Sale sale, List<int> additionIds, List<int> discountIds);
        Task UpdateSaleAsync(Sale sale, List<int> additionIds, List<int> discountIds);
        Task<List<Addition>> GetAllAdditionsAsync();
        Task<List<Discount>> GetAllDiscountsAsync();
        Task<Discount?> GetDiscountByIdAsync(int id);
        Task<Discount> CreateDiscountAsync(Discount discount);
        Task UpdateDiscountAsync(Discount discount);
        Task DeleteDiscountAsync(int id);
        Task<List<int>> GetSaleAdditionIdsAsync(int saleId);
        Task<List<int>> GetSaleDiscountIdsAsync(int saleId);
        Task<decimal> CalculateFinalPriceAsync(decimal basePrice, List<int> discountIds);
        Task<decimal> CalculateOriginalPriceAsync(decimal finalPrice, List<int> discountIds);
        Task<int> GetClientPurchaseCountAsync(int? clientId, string? clientName);
    }
}

