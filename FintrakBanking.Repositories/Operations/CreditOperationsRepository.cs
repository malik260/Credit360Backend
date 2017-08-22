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


        public decimal GetCollateralSearchChargeAmount(int collateralcustomerId)
        {
            var collateralAmount = this.context.tbl_Collateral_Immovable_Property.Include("tbl_City").Include("tbl_State").FirstOrDefault(x => x.CollateralCustomerId == collateralcustomerId).tbl_City.tbl_State.CollateralSearchChargeAmount;
            return collateralAmount;
        }
    }
}
