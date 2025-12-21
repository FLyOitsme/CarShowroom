using Dom;

namespace CarShowroom.Repositories
{
    public interface IModelRepository : IRepository<Model>
    {
        Task<List<Model>> GetModelsByBrandIdAsync(int brandId);
    }
}
