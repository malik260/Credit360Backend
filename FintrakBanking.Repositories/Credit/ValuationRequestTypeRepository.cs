using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class ValuationRequestTypeRepository : IValuationRequestTypeRepository
    {
        private FinTrakBankingContext _context;
        //private IGeneralSetupRepository _general;
        //private IAuditTrailRepository _audit;

        public ValuationRequestTypeRepository(FinTrakBankingContext context)
        {
            _context = context;
        }

        public List<ValuationRequestTypeViewModel> GetAllValuationRequestTypes()
        {
            return (from O in _context.TBL_COMPANY_CLASS
                        select new ValuationRequestTypeViewModel
                        {
                            valuationRequestTypeId = O.COMPANYCLASSID,
                            valuationRequestType = O.DESCRIPTION,
                        }).ToList();

            //var test = from O in _context.TBL_COLLATERAL_DOCUMENT_TYPE
            //           select new CollateralDocumentTypeViewModel
            //           {
            //               collateralTypeId = O.COLLATERALTYPEID
            //           };
            //return test.ToList();
        }
    }
}


