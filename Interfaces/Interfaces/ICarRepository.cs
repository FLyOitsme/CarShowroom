using Dom;

namespace CarShowroom.Repositories
{
    public interface ICarRepository : IRepository<Car>
    {
        Task<List<Car>> GetAllCarsWithDetailsAsync();
        Task<Car?> GetCarByIdWithDetailsAsync(long id);
        Task<List<Car>> SearchCarsAsync(string searchText);
        Task<List<Car>> GetAvailableCarsAsync();
    }
}
