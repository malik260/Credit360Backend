using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class TestCollateralRepository : ITestCollateralRepository
    {
        private FinTrakBankingContext _context;

        public TestCollateralRepository(FinTrakBankingContext context)
        {
            _context = context;
        }

        public CollateralViewModel GetCustomerCollateralByCollateralId(int collateralId, int typeId)
        {
            //var collateral = _context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false
            //    && x.COLLATERALCUSTOMERID == collateralId
            //)
            //.Select(x => new CollateralViewModel
            //{
            //    collateralId = x.COLLATERALCUSTOMERID,
            //    collateralTypeId = x.COLLATERALTYPEID,
            //    collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
            //    collateralSubTypeId = x.COLLATERALSUBTYPEID,
            //    customerId = x.CUSTOMERID,
            //    currencyId = x.CURRENCYID,
            //    currency = x.TBL_CURRENCY.CURRENCYNAME,
            //    currencyCode = x.TBL_CURRENCY.CURRENCYCODE,
            //    collateralCode = x.COLLATERALCODE,
            //    collateralValue = x.COLLATERALVALUE,
            //    camRefNumber = x.CAMREFNUMBER,
            //    allowSharing = x.ALLOWSHARING,
            //    isLocationBased = (bool)x.ISLOCATIONBASED,
            //    valuationCycle = x.VALUATIONCYCLE,
            //    haircut = x.HAIRCUT,
            //    approvalStatus = x.APPROVALSTATUS,
            //    //collateralValue = x.CollateralValue
            //    exchangeRate = x.EXCHANGERATE

            //})
            //.FirstOrDefault();

            var collateral = (from O in _context.TBL_COLLATERAL_CUSTOMER
                                where O.COLLATERALCUSTOMERID == collateralId && O.DELETED == false
                                select new CollateralViewModel() {
                                    collateralId = O.COLLATERALCUSTOMERID,
                                    collateralTypeId = O.COLLATERALTYPEID,
                                    collateralTypeName = O.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                    collateralSubTypeId = O.COLLATERALSUBTYPEID,
                                    customerId = O.CUSTOMERID.Value,
                                    currencyId = O.CURRENCYID,
                                    currency = O.TBL_CURRENCY.CURRENCYNAME,
                                    currencyCode = O.TBL_CURRENCY.CURRENCYCODE,
                                    collateralCode = O.COLLATERALCODE,
                                    collateralValue = O.COLLATERALVALUE,
                                    camRefNumber = O.CAMREFNUMBER,
                                    allowSharing = O.ALLOWSHARING,
                                    isLocationBased = (bool) O.ISLOCATIONBASED,
                                    valuationCycle = O.VALUATIONCYCLE,
                                    haircut = O.HAIRCUT,
                                    approvalStatusName = O.APPROVALSTATUS,
                                    //collateralValue = x.CollateralValue
                                    exchangeRate = O.EXCHANGERATE
                                }).FirstOrDefault();

            return collateral;
        }
    }
}
