using DataLayer.Entities;

namespace CarShowroom.Interfaces
{
    public interface ICarService
    {
        Task<List<Car>> GetAllCarsAsync();
        Task<Car?> GetCarByIdAsync(long id);
        Task<List<Car>> SearchCarsAsync(string searchText);
        Task AddCarAsync(Car car);
        Task UpdateCarAsync(Car car);
        Task DeleteCarAsync(long id);
        Task<List<Brand>> GetAllBrandsAsync();
        Task<List<Model>> GetModelsByBrandIdAsync(int brandId);
        Task<List<CarType>> GetAllCarTypesAsync();
        Task<List<ConditionType>> GetAllConditionTypesAsync();
        Task<List<EngineType>> GetAllEngineTypesAsync();
        Task<List<Transmission>> GetAllTransmissionsAsync();
        Task<List<Wdtype>> GetAllWdTypesAsync();
    }
}

