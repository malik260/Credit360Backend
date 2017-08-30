using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.CreditOperations;
using System.Linq;

namespace FintrakBanking.Repositories.CreditOperations

{
    public class CreditOperationsRepository : ICreditOperationsRepository
    {
        private FinTrakBankingContext context;

        public CreditOperationsRepository(

        FinTrakBankingContext _context)
        {

            this.context = _context;
          
        }


        public decimal GetCollateralSearchChargeAmount(int stateId)
        {
            var collateralSearchChargeAmount = this.context.tbl_State.FirstOrDefault(x => x.StateId == stateId).CollateralSearchChargeAmount;


            return collateralSearchChargeAmount;
        }
    }
}
