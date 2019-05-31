using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Reports
{
    public class OutputDocumentRepository : IOutputDocumentRepository
    {
        FinTrakBankingContext context;

        public OutputDocumentRepository(FinTrakBankingContext _context)
        {
            context = _context;
        }
        public IEnumerable<OutPutDocumentApprovalViewModel> GetApplicationApproval(int loanApplicationId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OutPutDocumentChecklistViewModel> GetChecklist(int loanApplicationId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OutPutDocumentCollateralViewModel> GetCollateral(int loanApplicationId)
        {
            var collateral = (from x in context.TBL_LOAN_APPLICATION_COLLATRL2
                              join b in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                              where b.LOANAPPLICATIONID == loanApplicationId
                              select new OutPutDocumentCollateralViewModel
                              {
                                  collateralDetail = x.COLLATERALDETAIL,
                                  collateralValue = x.COLLATERALVALUE,
                                  stapedToCoverAmount = x.STAMPEDTOCOVERAMOUNT
                              }).ToList();

            return collateral;
        }

        public IEnumerable<OutPutDocumentConcurrencesViewModel> GetConcurrences(int loanApplicationId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OutPutDocumentCustomerFacilitiesViewModel> GetCustomerFacilities(int loanApplicationId)
        {
            var loanDetails = (from a in context.TBL_LOAN_APPLICATION
                               join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                               from d in cg.DefaultIfEmpty()
                               join e in context.TBL_CURRENCY on b.CURRENCYID equals e.CURRENCYID
                               where a.LOANAPPLICATIONID == loanApplicationId
                               && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                               && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                               && b.STATUSID == (int)ApprovalStatusEnum.Approved
                               select new OutPutDocumentCustomerFacilitiesViewModel()
                               {
                                   facility = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == b.APPROVEDPRODUCTID).PRODUCTNAME,
                                   amount = e.CURRENCYNAME + " " + b.APPROVEDAMOUNT,
                                   maturity = b.EXPIRYDATE,
                                   security = "",
                                   performance = ""

                                  
                               }).ToList();



            return loanDetails;
        }

        public IEnumerable<OutPutDocumentCustomerInformationViewModel> GetCustomerInformation(int loanApplicationId)
        {
            var loanDetails = (from a in context.TBL_LOAN_APPLICATION
                               join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                               from d in cg.DefaultIfEmpty()
                               join e in context.TBL_CUSTOMER_ADDRESS on a.CUSTOMERID equals e.CUSTOMERID into dg
                               from e in dg.DefaultIfEmpty()
                               join g in context.TBL_CUSTOMER_PHONECONTACT on a.CUSTOMERID equals g.CUSTOMERID into gg
                               from g in gg.DefaultIfEmpty()
                               join h in context.TBL_CURRENCY on b.CURRENCYID equals h.CURRENCYID into hh
                               from h in hh.DefaultIfEmpty()
                               where a.LOANAPPLICATIONID == loanApplicationId
                               // b.STATUSID == (int)ApprovalStatusEnum.Approved
                               select new OutPutDocumentCustomerInformationViewModel()
                               {
                                   borrower = c.FIRSTNAME + " " + c.LASTNAME,
                                   location = e.ADDRESS ?? " ",
                                   business = "",
                                   accountNumber = b.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   incorporationDate = "",
                                   principalPromoters = "",
                                   customerRiskRating = "",
                                   classification = "",
                                   accountOpeningDate = "",
                                   businessCommencementDate ="",

                               }).ToList();

            return loanDetails;
        }

        public IEnumerable<OutPutDocumentFeeViewModel> GetFee(int loanApplicationId)
        {
            var fees = (from a in context.TBL_LOAN_APPLICATION_DETL_FEE
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                        join c in context.TBL_CHARGE_FEE on a.CHARGEFEEID equals c.CHARGEFEEID
                        join d in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                        join e in context.TBL_PRODUCT on b.PROPOSEDPRODUCTID equals e.PRODUCTID
                        where d.LOANAPPLICATIONID == loanApplicationId
                                //&& d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                //&& d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                //&& b.STATUSID == (int)ApprovalStatusEnum.Approved
                        select new OutPutDocumentFeeViewModel()
                        {
                            feeName = c.CHARGEFEENAME,
                            rateValue = a.RECOMMENDED_FEERATEVALUE,
                            productName = e.PRODUCTNAME
                        }).ToList();

                return fees;
           
        }

        public IEnumerable<OutPutDocumentMonthsActivityViewModel> GetMonthsActivity(int loanApplicationId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<OutPutDocumentMonthActivitySignViewModel> MonthActivitySignature(int loanApplicationId)
        {
            throw new NotImplementedException();
        }
    }
}
