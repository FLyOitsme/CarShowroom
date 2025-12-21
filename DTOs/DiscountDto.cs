namespace CarShowroom.DTOs
{
    public class DiscountDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public float? Cost { get; set; }
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsActive { get; set; }
    }
}

