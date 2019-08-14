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
            return (from O in _context.TBL_VALUATION_REQUEST_TYPE
                    select new ValuationRequestTypeViewModel
                    {
                        valuationRequestTypeId = O.VALUATIONREQUESTTYPEID,
                        valuationRequestType = O.VALUATIONREQUESTTYPE,
                    }).ToList();
        }
    }
}


