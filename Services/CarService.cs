using CarShowroom.Models;

namespace CarShowroom.Services
{
    public class CarService
    {
        private List<Car> _cars = new List<Car>
        {
            new Car
            {
                Id = 1,
                Brand = "Toyota",
                Model = "Camry",
                Year = 2022,
                Color = "Белый",
                Price = 2500000,
                EngineType = "Бензин",
                EngineVolume = 2.5,
                Mileage = 15000,
                Transmission = "Автомат",
                Description = "Отличное состояние, один владелец",
                ImageUrl = "dotnet_bot.png",
                Vin = "JTDBR32E123456789",
                EngineNumber = "2AR-FE123456",
                BodyNumber = "JTDBR32E123456789",
                RegistrationNumber = string.Empty
            },
            new Car
            {
                Id = 2,
                Brand = "BMW",
                Model = "X5",
                Year = 2021,
                Color = "Черный",
                Price = 4500000,
                EngineType = "Бензин",
                EngineVolume = 3.0,
                Mileage = 25000,
                Transmission = "Автомат",
                Description = "Премиум комплектация, полный привод",
                ImageUrl = "dotnet_bot.png",
                Vin = "WBAFR9C50ED123456",
                EngineNumber = "B58B30M123456",
                BodyNumber = "WBAFR9C50ED123456",
                RegistrationNumber = string.Empty
            },
            new Car
            {
                Id = 3,
                Brand = "Mercedes-Benz",
                Model = "C-Class",
                Year = 2023,
                Color = "Серебристый",
                Price = 3200000,
                EngineType = "Бензин",
                EngineVolume = 2.0,
                Mileage = 5000,
                Transmission = "Автомат",
                Description = "Новый автомобиль, гарантия",
                ImageUrl = "dotnet_bot.png",
                Vin = "WDDWF4KB3NR123456",
                EngineNumber = "M274920123456",
                BodyNumber = "WDDWF4KB3NR123456",
                RegistrationNumber = string.Empty
            },
            new Car
            {
                Id = 4,
                Brand = "Audi",
                Model = "A4",
                Year = 2020,
                Color = "Синий",
                Price = 2800000,
                EngineType = "Дизель",
                EngineVolume = 2.0,
                Mileage = 40000,
                Transmission = "Автомат",
                Description = "Экономичный двигатель, хорошее состояние",
                ImageUrl = "dotnet_bot.png",
                Vin = "WAUZZZ8KZLA123456",
                EngineNumber = "CJTD123456",
                BodyNumber = "WAUZZZ8KZLA123456",
                RegistrationNumber = string.Empty
            },
            new Car
            {
                Id = 5,
                Brand = "Volkswagen",
                Model = "Passat",
                Year = 2022,
                Color = "Серый",
                Price = 2100000,
                EngineType = "Бензин",
                EngineVolume = 1.8,
                Mileage = 20000,
                Transmission = "Механика",
                Description = "Надежный семейный автомобиль",
                ImageUrl = "dotnet_bot.png",
                Vin = "WVWZZZ3CZCE123456",
                EngineNumber = "EA888123456",
                BodyNumber = "WVWZZZ3CZCE123456",
                RegistrationNumber = string.Empty
            }
        };

        private int _nextId = 6;

        public List<Car> GetAllCars()
        {
            return _cars;
        }

        public Car? GetCarById(int id)
        {
            return _cars.FirstOrDefault(c => c.Id == id);
        }

        public void AddCar(Car car)
        {
            car.Id = _nextId++;
            _cars.Add(car);
        }

        public void UpdateCar(Car car)
        {
            var existingCar = _cars.FirstOrDefault(c => c.Id == car.Id);
            if (existingCar != null)
            {
                var index = _cars.IndexOf(existingCar);
                _cars[index] = car;
            }
        }

        public void DeleteCar(int id)
        {
            var car = _cars.FirstOrDefault(c => c.Id == id);
            if (car != null)
            {
                _cars.Remove(car);
            }
        }
    }
}
