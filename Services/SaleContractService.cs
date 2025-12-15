using CarShowroom.Models;

namespace CarShowroom.Services
{
    public class SaleContractService
    {
        private List<SaleContract> _contracts = new List<SaleContract>();
        private int _nextId = 1;

        public List<SaleContract> GetAllContracts()
        {
            return _contracts;
        }

        public SaleContract? GetContractById(int id)
        {
            return _contracts.FirstOrDefault(c => c.Id == id);
        }

        public void AddContract(SaleContract contract)
        {
            contract.Id = _nextId++;
            _contracts.Add(contract);
        }

        public void UpdateContract(SaleContract contract)
        {
            var existingContract = _contracts.FirstOrDefault(c => c.Id == contract.Id);
            if (existingContract != null)
            {
                var index = _contracts.IndexOf(existingContract);
                _contracts[index] = contract;
            }
        }

        public void DeleteContract(int id)
        {
            var contract = _contracts.FirstOrDefault(c => c.Id == id);
            if (contract != null)
            {
                _contracts.Remove(contract);
            }
        }
    }
}
