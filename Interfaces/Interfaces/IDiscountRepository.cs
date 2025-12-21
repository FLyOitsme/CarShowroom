using Dom;

namespace CarShowroom.Repositories
{
    public interface IDiscountRepository : IRepository<Discount>
    {
        Task<List<Discount>> GetActiveDiscountsAsync();
        Task<List<Discount>> GetDiscountsByIdsAsync(List<int> discountIds);
    }
}
