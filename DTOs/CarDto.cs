namespace CarShowroom.DTOs
{
    public class CarDto
    {
        public long Id { get; set; }
        public int? Year { get; set; }
        public bool? Stock { get; set; }
        public int? ConditionId { get; set; }
        public string? ConditionName { get; set; }
        public float? Mileage { get; set; }
        public int? TypeId { get; set; }
        public string? TypeName { get; set; }
        public int? WdId { get; set; }
        public string? WdName { get; set; }
        public int? TransmissionId { get; set; }
        public string? TransmissionName { get; set; }
        public float? EngVol { get; set; }
        public float? Power { get; set; }
        public string? Color { get; set; }
        public int? EngTypeId { get; set; }
        public string? EngTypeName { get; set; }
        public float? Cost { get; set; }
        public int? ModelId { get; set; }
        public string? ModelName { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? ImageUrl { get; set; }
    }
}

