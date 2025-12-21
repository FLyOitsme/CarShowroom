namespace CarShowroom.DTOs
{
    public class ClientDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Patronyc { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PassData { get; set; }
        public string FullName => $"{Surname} {Name} {Patronyc}".Trim();
    }
}

