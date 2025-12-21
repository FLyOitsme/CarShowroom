using Dom;

namespace CarShowroom.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<List<User>> GetAllManagersAsync();
        Task<User?> GetUserByIdWithRoleAsync(int id);
        Task<User?> SearchClientByNameAsync(string name);
        Task<List<User>> SearchClientsAsync(string searchText);
        Task<List<User>> GetAllClientsAsync();
    }
}
