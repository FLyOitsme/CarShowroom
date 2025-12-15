namespace CarShowroom.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string EngineType { get; set; } = string.Empty;
        public double EngineVolume { get; set; }
        public int Mileage { get; set; }
        public string Transmission { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public string EngineNumber { get; set; } = string.Empty;
        public string BodyNumber { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;

        public string FullName => $"{Brand} {Model} ({Year})";
    }
}
