namespace CarShowroom.Models
{
    public class SaleContract
    {
        public int Id { get; set; }
        public DateTime ContractDate { get; set; } = DateTime.Now;
        public int CarId { get; set; }
        
        // Продавец (юридическое лицо - автосалон)
        public string SellerCompanyName { get; set; } = string.Empty;
        public string SellerInn { get; set; } = string.Empty;
        public string SellerOgrn { get; set; } = string.Empty;
        public string SellerAddress { get; set; } = string.Empty;
        public string SellerDirectorName { get; set; } = string.Empty;
        public string SellerDirectorPosition { get; set; } = string.Empty;
        
        // Покупатель (физическое лицо)
        public string BuyerFullName { get; set; } = string.Empty;
        public string BuyerPassportSeries { get; set; } = string.Empty;
        public string BuyerPassportNumber { get; set; } = string.Empty;
        public string BuyerPassportIssuedBy { get; set; } = string.Empty;
        public string BuyerAddress { get; set; } = string.Empty;
        
        // Автомобиль
        public string CarBrand { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public int CarYear { get; set; }
        public string CarVin { get; set; } = string.Empty;
        public string CarEngineNumber { get; set; } = string.Empty;
        public string CarBodyNumber { get; set; } = string.Empty;
        public string CarColor { get; set; } = string.Empty;
        public string CarRegistrationNumber { get; set; } = string.Empty;
        
        // Условия сделки
        public decimal SalePrice { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string AdditionalTerms { get; set; } = string.Empty;
        
        // Подписи
        public bool SellerSigned { get; set; }
        public bool BuyerSigned { get; set; }
    }
}
