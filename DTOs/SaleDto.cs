namespace CarShowroom.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerSurname { get; set; }
        public DateOnly? Date { get; set; }
        public float? Cost { get; set; }
        public long CarId { get; set; }
        public string? CarBrand { get; set; }
        public string? CarModel { get; set; }
        public long? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string? ClientSurname { get; set; }
        public string? ClientPatronyc { get; set; }
    }
}

