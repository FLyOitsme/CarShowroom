using Microsoft.EntityFrameworkCore;
using Dom;

namespace CarShowroom.Repositories
{
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        public ClientRepository(CarShowroomDbContext context) : base(context)
        {
        }

        public async Task<Client?> SearchClientByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var nameParts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length == 0)
                return null;

            var query = _dbSet.AsQueryable();

            if (nameParts.Length >= 1)
            {
                var firstName = nameParts[0];
                query = query.Where(c =>
                    (c.Name != null && c.Name.Contains(firstName)) ||
                    (c.Surname != null && c.Surname.Contains(firstName)) ||
                    (c.Patronyc != null && c.Patronyc.Contains(firstName)));
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<Client>> SearchClientsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<Client>();

            var lowerSearch = searchText.ToLower();
            return await _dbSet
                .Where(c =>
                    (c.Name != null && c.Name.ToLower().Contains(lowerSearch)) ||
                    (c.Surname != null && c.Surname.ToLower().Contains(lowerSearch)) ||
                    (c.Patronyc != null && c.Patronyc.ToLower().Contains(lowerSearch)) ||
                    (c.PhoneNumber != null && c.PhoneNumber.ToLower().Contains(lowerSearch)))
                .ToListAsync();
        }

        public async Task<Client?> GetClientByFullNameAsync(string surname, string? name, string? patronyc)
        {
            return await _dbSet
                .Where(c => c.Surname == surname &&
                       (name == null || c.Name == name) &&
                       (patronyc == null || c.Patronyc == patronyc))
                .FirstOrDefaultAsync();
        }

        public new async Task<Client> AddAsync(Client entity)
        {
            long nextId = 1;
            var clientsCount = await _dbSet.AsNoTracking().CountAsync();

            if (clientsCount > 0)
            {
                var maxId = await _dbSet
                    .AsNoTracking()
                    .MaxAsync(c => (long?)c.Id);

                if (maxId.HasValue)
                {
                    nextId = maxId.Value + 1;
                }
            }

            var newClient = new Client
            {
                Id = nextId,
                Name = entity.Name,
                Surname = entity.Surname,
                Patronyc = entity.Patronyc,
                PhoneNumber = entity.PhoneNumber,
                PassData = entity.PassData
            };

            await _dbSet.AddAsync(newClient);
            await _context.SaveChangesAsync();
            return newClient;
        }
    }
}
