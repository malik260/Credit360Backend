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
    public class LoanPerformanceRepository : ILoanPerformanceRepository
    {
        private FinTrakBankingContext context;

        public LoanPerformanceRepository(FinTrakBankingContext _contex)
        {
            this.context = _contex;
        }
        public IQueryable<LoanViewModel> GetAllLoan()
        {
            try
            {

                return GetTermLoan().Concat(GetRevolvingLoan());
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IEnumerable<PrudGuildlineTypeViewModel> GetPrudGuildlineType()
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_PRUDENT_GUIDE_TYPE
                                   select new PrudGuildlineTypeViewModel {
                                       prudentialGuildlineTypeId = a.PRUDENTIALGUIDELINETYPEID,
                                       prudentialGuildlineTypeName = a.PRUDENTIALGUIDELINETYPENAME
                                   }).ToList();
            return allFilteredLoan;
        }
        private IQueryable<LoanViewModel> GetTermLoan()
        {
            var allFilteredLoan = (from a in context.TBL_LOAN
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true
                                   select new LoanViewModel
                                   {
                                       loanId = a.TERMLOANID,
                                       customerId = a.CUSTOMERID,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.PRINCIPALAMOUNT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                       outstandingInterest = a.OUTSTANDINGINTEREST,
                                       outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                       internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                                       externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID
                                   });
            return allFilteredLoan;
        }
        private IQueryable<LoanViewModel> GetRevolvingLoan()
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_REVOLVING
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   where a.ISDISBURSED == true
                                   select new LoanViewModel
                                   {
                                       loanId = a.REVOLVINGLOANID,
                                       customerId = a.CUSTOMERID,
                                       customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.OVERDRAFTLIMIT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = a.TBL_PRODUCT.PRODUCTNAME,
                                      // outstandingInterest = (decimal)a.INTEREST_AMOUNT,
                                       outstandingPrincipal = a.OVERDRAFTLIMIT,
                                       internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                                       externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID
                                   });
            return allFilteredLoan;
        }
    }
}
