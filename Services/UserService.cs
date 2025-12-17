using DataLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarShowroom.Services
{
    public class UserService
    {
        private readonly CarShowroomDbContext _context;

        public UserService(CarShowroomDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllManagersAsync()
        {
            // Получаем пользователей с ролью "Менеджер" (RoleTypeId = 2) или "Продавец" (RoleTypeId = 3)
            return await _context.Users
                .Include(u => u.RoleType)
                .Where(u => u.RoleTypeId == 2 || u.RoleTypeId == 3)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
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

            var query = _context.Users
                .Include(u => u.RoleType)
                .AsQueryable();

            // Ищем по имени, фамилии или отчеству
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
            return await _context.Users
                .Include(u => u.RoleType)
                .Where(u =>
                    (u.Name != null && u.Name.ToLower().Contains(lowerSearch)) ||
                    (u.Surname != null && u.Surname.ToLower().Contains(lowerSearch)) ||
                    (u.Patronyc != null && u.Patronyc.ToLower().Contains(lowerSearch)) ||
                    (u.Login != null && u.Login.ToLower().Contains(lowerSearch)))
                .ToListAsync();
        }

        public async Task<User> CreateOrGetClientAsync(string fullName, string? phone = null, string? address = null)
        {
            // Парсим ФИО
            var nameParts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string? firstName = null;
            string? lastName = null;
            string? patronymic = null;

            if (nameParts.Length >= 1)
                lastName = nameParts[0];
            if (nameParts.Length >= 2)
                firstName = nameParts[1];
            if (nameParts.Length >= 3)
                patronymic = nameParts[2];

            // Ищем существующего клиента
            User? existingClient = null;
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                existingClient = await _context.Users
                    .Where(u => u.Surname == lastName && 
                           (firstName == null || u.Name == firstName) &&
                           (patronymic == null || u.Patronyc == patronymic))
                    .FirstOrDefaultAsync();
            }

            if (existingClient != null)
            {
                return existingClient;
            }

            // Создаем нового клиента
            // Используем роль "Продавец" (Id=3) как клиента, или можно создать отдельную роль
            // Для простоты используем роль с Id=3 или создаем без роли
            var clientRole = await _context.RoleTypes.FirstOrDefaultAsync(r => r.Id == 3);
            if (clientRole == null)
            {
                // Если нет роли, создаем пользователя без роли
                var newClient = new User
                {
                    Name = firstName,
                    Surname = lastName,
                    Patronyc = patronymic,
                    Login = phone ?? $"client_{DateTime.Now.Ticks}",
                    Password = null,
                    RoleTypeId = null
                };
                _context.Users.Add(newClient);
                await _context.SaveChangesAsync();
                return newClient;
            }

            var client = new User
            {
                Name = firstName,
                Surname = lastName,
                Patronyc = patronymic,
                Login = phone ?? $"client_{DateTime.Now.Ticks}",
                Password = null,
                RoleTypeId = 3 // Используем роль "Продавец" как клиента
            };

            _context.Users.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.RoleType)
                .ToListAsync();
        }

        public async Task<List<RoleType>> GetAllRoleTypesAsync()
        {
            return await _context.RoleTypes.ToListAsync();
        }

        public async Task<List<User>> GetAllClientsAsync()
        {
            // Получаем всех клиентов (пользователей, которые не являются менеджерами)
            // Клиенты обычно имеют RoleTypeId = 3 (Продавец) или другую роль клиента
            // Или можно получить всех пользователей, исключая менеджеров
            return await _context.Users
                .Include(u => u.RoleType)
                .Where(u => u.RoleTypeId != 2 && u.RoleTypeId != 1) // Исключаем менеджеров и админов
                .OrderBy(u => u.Surname)
                .ThenBy(u => u.Name)
                .ToListAsync();
        }
    }
}

