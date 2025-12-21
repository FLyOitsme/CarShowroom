using Dom;

namespace CarShowroom.Repositories
{
    public interface ISaleRepository : IRepository<Sale>
    {
        Task<List<Sale>> GetAllSalesWithDetailsAsync();
        Task<Sale?> GetSaleByIdWithDetailsAsync(int id);
        Task<List<int>> GetSaleAdditionIdsAsync(int saleId);
        Task<List<int>> GetSaleDiscountIdsAsync(int saleId);
        Task<int> GetClientPurchaseCountAsync(long? clientId);
        Task<List<Sale>> GetSalesByClientIdAsync(long clientId);
        Task<List<Sale>> GetSalesByDateRangeAsync(DateOnly startDate, DateOnly endDate);
        Task AddSaleAdditionsAsync(int saleId, List<int> additionIds);
        Task AddSaleDiscountsAsync(int saleId, List<int> discountIds);
        Task RemoveSaleAdditionsAsync(int saleId);
        Task RemoveSaleDiscountsAsync(int saleId);
    }
}
