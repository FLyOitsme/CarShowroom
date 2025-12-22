using Dom;

namespace CarShowroom.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllManagersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> SearchClientByNameAsync(string name);
        Task<List<User>> SearchClientsAsync(string searchText);
        Task<User> CreateOrGetClientAsync(string fullName, string? phone = null, string? address = null);
        Task<List<User>> GetAllUsersAsync();
        Task<List<RoleType>> GetAllRoleTypesAsync();
        Task<List<User>> GetAllClientsAsync();
        Task<Client> CreateOrGetClientEntityAsync(string fullName, string? phone, string passData);
        Task<Client?> SearchClientEntityByPassDataAsync(string passData);
        Task<List<Client>> GetAllClientEntitiesAsync();
    }
}

