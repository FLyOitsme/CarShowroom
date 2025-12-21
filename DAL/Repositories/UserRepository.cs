using Microsoft.EntityFrameworkCore;
using Dom;

namespace CarShowroom.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(CarShowroomDbContext context) : base(context)
        {
        }

        public async Task<List<User>> GetAllManagersAsync()
        {
            return await _dbSet
                .Include(u => u.RoleType)
                .Where(u => u.RoleTypeId == 2 || u.RoleTypeId == 3)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdWithRoleAsync(int id)
        {
            return await _dbSet
                .Include(u => u.RoleType)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> SearchClientByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var nameParts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length == 0)
                return null;

            var query = _dbSet
                .Include(u => u.RoleType)
                .AsQueryable();

            if (nameParts.Length >= 1)
            {
                var firstName = nameParts[0];
                query = query.Where(u =>
                    (u.Name != null && u.Name.Contains(firstName)) ||
                    (u.Surname != null && u.Surname.Contains(firstName)) ||
                    (u.Patronyc != null && u.Patronyc.Contains(firstName)));
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<User>> SearchClientsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<User>();

            var lowerSearch = searchText.ToLower();
            return await _dbSet
                .Include(u => u.RoleType)
                .Where(u =>
                    (u.Name != null && u.Name.ToLower().Contains(lowerSearch)) ||
                    (u.Surname != null && u.Surname.ToLower().Contains(lowerSearch)) ||
                    (u.Patronyc != null && u.Patronyc.ToLower().Contains(lowerSearch)) ||
                    (u.Login != null && u.Login.ToLower().Contains(lowerSearch)))
                .ToListAsync();
        }

        public async Task<List<User>> GetAllClientsAsync()
        {
            return await _dbSet
                .Include(u => u.RoleType)
                .Where(u => u.RoleTypeId != 2 && u.RoleTypeId != 1)
                .OrderBy(u => u.Surname)
                .ThenBy(u => u.Name)
                .ToListAsync();
        }
    }
}
