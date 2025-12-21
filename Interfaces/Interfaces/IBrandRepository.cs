using Dom;

namespace CarShowroom.Repositories
{
    public interface IBrandRepository : IRepository<Brand>
    {
        Task<List<Brand>> GetAllBrandsWithCountryAsync();
    }
}
