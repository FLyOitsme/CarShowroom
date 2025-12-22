using Dom;

namespace CarShowroom.Repositories
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client?> SearchClientByNameAsync(string name);
        Task<List<Client>> SearchClientsAsync(string searchText);
        Task<Client?> GetClientByFullNameAsync(string surname, string? name, string? patronyc);
        Task<Client?> GetClientByPassDataAsync(string passData);
    }
}
