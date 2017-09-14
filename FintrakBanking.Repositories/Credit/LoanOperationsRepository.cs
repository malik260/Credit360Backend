using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using System.Linq;

namespace FintrakBanking.Repositories.Credit

{
    public class LoanOperationsRepository : ILoanOperationsRepository
    {
        private FinTrakBankingContext context;

        public LoanOperationsRepository(

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
