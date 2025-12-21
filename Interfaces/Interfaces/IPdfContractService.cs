using Dom;

namespace CarShowroom.Interfaces
{
    public interface IPdfContractService
    {
        byte[] GenerateContract(
            Sale sale,
            Client client,
            User manager,
            Car car,
            List<Addition> additions,
            List<Discount> discounts,
            decimal basePrice,
            decimal finalPrice);
    }
}

