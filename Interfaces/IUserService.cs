using DataLayer.Entities;

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
        Task<Client> CreateOrGetClientEntityAsync(string fullName, string? phone = null, string? passData = null);
        Task<Client?> SearchClientEntityByNameAsync(string name);
        Task<List<Client>> SearchClientEntitiesAsync(string searchText);
        Task<List<Client>> GetAllClientEntitiesAsync();
    }
}

