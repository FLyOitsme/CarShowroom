using Dom;
using Microsoft.EntityFrameworkCore;
using CarShowroom.Interfaces;
using CarShowroom.Repositories;

namespace CarShowroom.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IRepository<RoleType> _roleTypeRepository;
        private readonly CarShowroomDbContext _context;

        public UserService(
            IUserRepository userRepository,
            IClientRepository clientRepository,
            IRepository<RoleType> roleTypeRepository,
            CarShowroomDbContext context)
        {
            _userRepository = userRepository;
            _clientRepository = clientRepository;
            _roleTypeRepository = roleTypeRepository;
            _context = context;
        }

        public async Task<List<User>> GetAllManagersAsync()
        {
            return await _userRepository.GetAllManagersAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdWithRoleAsync(id);
        }

        public async Task<User?> SearchClientByNameAsync(string name)
        {
            return await _userRepository.SearchClientByNameAsync(name);
        }

        public async Task<List<User>> SearchClientsAsync(string searchText)
        {
            return await _userRepository.SearchClientsAsync(searchText);
        }

        public async Task<User> CreateOrGetClientAsync(string fullName, string? phone = null, string? address = null)
        {
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

            User? existingClient = null;
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                existingClient = await _userRepository.SearchClientByNameAsync(fullName);
            }

            if (existingClient != null)
            {
                return existingClient;
            }

            var clientRole = await _context.RoleTypes.FirstOrDefaultAsync(r => r.Id == 3);
            if (clientRole == null)
            {
                var newClient = new User
                {
                    Name = firstName,
                    Surname = lastName,
                    Patronyc = patronymic,
                    Login = phone ?? $"client_{DateTime.Now.Ticks}",
                    Password = null,
                    RoleTypeId = null
                };
                await _userRepository.AddAsync(newClient);
                return newClient;
            }

            var client = new User
            {
                Name = firstName,
                Surname = lastName,
                Patronyc = patronymic,
                Login = phone ?? $"client_{DateTime.Now.Ticks}",
                Password = null,
                RoleTypeId = 3
            };

            await _userRepository.AddAsync(client);
            return client;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetQueryable().ToListAsync();
            foreach (var user in users)
            {
                if (user.RoleTypeId.HasValue)
                {
                    var roleType = await _context.RoleTypes.FindAsync(user.RoleTypeId.Value);
                    user.RoleType = roleType;
                }
            }
            return users;
        }

        public async Task<List<RoleType>> GetAllRoleTypesAsync()
        {
            return (await _roleTypeRepository.GetAllAsync()).ToList();
        }

        public async Task<List<User>> GetAllClientsAsync()
        {
            return await _userRepository.GetAllClientsAsync();
        }

        public async Task<Client> CreateOrGetClientEntityAsync(string fullName, string? phone, string passData)
        {
            if (string.IsNullOrWhiteSpace(passData))
            {
                throw new ArgumentException("Паспортные данные обязательны", nameof(passData));
            }

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

            var existingClientByPass = await _clientRepository.GetClientByPassDataAsync(passData);
            if (existingClientByPass != null)
            {
                bool needsUpdate = false;
                if (existingClientByPass.Surname != lastName || 
                    existingClientByPass.Name != firstName || 
                    existingClientByPass.Patronyc != patronymic)
                {
                    existingClientByPass.Surname = lastName;
                    existingClientByPass.Name = firstName;
                    existingClientByPass.Patronyc = patronymic;
                    needsUpdate = true;
                }
                if (existingClientByPass.PhoneNumber != phone)
                {
                    existingClientByPass.PhoneNumber = phone;
                    needsUpdate = true;
                }
                if (needsUpdate)
                {
                    await _clientRepository.UpdateAsync(existingClientByPass);
                }
                return existingClientByPass;
            }

            var newClient = new Client
            {
                Name = firstName,
                Surname = lastName,
                Patronyc = patronymic,
                PhoneNumber = phone,
                PassData = passData
            };

            return await _clientRepository.AddAsync(newClient);
        }

        public async Task<Client?> SearchClientEntityByPassDataAsync(string passData)
        {
            if (string.IsNullOrWhiteSpace(passData))
                return null;

            return await _clientRepository.GetClientByPassDataAsync(passData);
        }

        public async Task<List<Client>> GetAllClientEntitiesAsync()
        {
            return (await _clientRepository.GetAllAsync()).ToList();
        }
    }
}