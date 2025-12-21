using Dom;

namespace CarShowroom.Repositories
{
    public interface IAdditionRepository : IRepository<Addition>
    {
        Task<List<Addition>> GetAdditionsByIdsAsync(List<int> additionIds);
    }
}
