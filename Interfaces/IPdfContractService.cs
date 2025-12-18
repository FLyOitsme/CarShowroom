using DataLayer.Entities;

namespace CarShowroom.Interfaces
{
    public interface IPdfContractService
    {
        byte[] GenerateContract(
            Sale sale,
            User client,
            User manager,
            Car car,
            List<Addition> additions,
            List<Discount> discounts,
            decimal basePrice,
            decimal finalPrice);
    }
}

