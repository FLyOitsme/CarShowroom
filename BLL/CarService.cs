using Dom;
using CarShowroom.Interfaces;
using CarShowroom.Repositories;

namespace CarShowroom.Services
{
    public class CarService : ICarService
    {
        private readonly ICarRepository _carRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IModelRepository _modelRepository;
        private readonly IRepository<CarType> _carTypeRepository;
        private readonly IRepository<ConditionType> _conditionTypeRepository;
        private readonly IRepository<EngineType> _engineTypeRepository;
        private readonly IRepository<Transmission> _transmissionRepository;
        private readonly IRepository<Wdtype> _wdTypeRepository;

        public CarService(
            ICarRepository carRepository,
            IBrandRepository brandRepository,
            IModelRepository modelRepository,
            IRepository<CarType> carTypeRepository,
            IRepository<ConditionType> conditionTypeRepository,
            IRepository<EngineType> engineTypeRepository,
            IRepository<Transmission> transmissionRepository,
            IRepository<Wdtype> wdTypeRepository)
        {
            _carRepository = carRepository;
            _brandRepository = brandRepository;
            _modelRepository = modelRepository;
            _carTypeRepository = carTypeRepository;
            _conditionTypeRepository = conditionTypeRepository;
            _engineTypeRepository = engineTypeRepository;
            _transmissionRepository = transmissionRepository;
            _wdTypeRepository = wdTypeRepository;
        }

        public async Task<List<Car>> GetAllCarsAsync()
        {
            return await _carRepository.GetAvailableCarsAsync();
        }

        public async Task<Car?> GetCarByIdAsync(long id)
        {
            return await _carRepository.GetCarByIdWithDetailsAsync(id);
        }

        public async Task<List<Car>> SearchCarsAsync(string searchText)
        {
            return await _carRepository.SearchCarsAsync(searchText);
        }

        public async Task AddCarAsync(Car car)
        {
            await _carRepository.AddAsync(car);
        }

        public async Task UpdateCarAsync(Car car)
        {
            await _carRepository.UpdateAsync(car);
        }

        public async Task DeleteCarAsync(long id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car != null)
            {
                await _carRepository.DeleteAsync(car);
            }
        }

        public async Task<List<Brand>> GetAllBrandsAsync()
        {
            return await _brandRepository.GetAllBrandsWithCountryAsync();
        }

        public async Task<List<Model>> GetModelsByBrandIdAsync(int brandId)
        {
            return await _modelRepository.GetModelsByBrandIdAsync(brandId);
        }

        public async Task<List<CarType>> GetAllCarTypesAsync()
        {
            return (await _carTypeRepository.GetAllAsync()).ToList();
        }

        public async Task<List<ConditionType>> GetAllConditionTypesAsync()
        {
            return (await _conditionTypeRepository.GetAllAsync()).ToList();
        }

        public async Task<List<EngineType>> GetAllEngineTypesAsync()
        {
            return (await _engineTypeRepository.GetAllAsync()).ToList();
        }

        public async Task<List<Transmission>> GetAllTransmissionsAsync()
        {
            return (await _transmissionRepository.GetAllAsync()).ToList();
        }

        public async Task<List<Wdtype>> GetAllWdTypesAsync()
        {
            return (await _wdTypeRepository.GetAllAsync()).ToList();
        }
    }
}
