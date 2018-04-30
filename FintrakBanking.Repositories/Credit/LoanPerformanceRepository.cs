using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.Setups.Credit;
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
        private IGeneralSetupRepository generalSetup;
        private IFinanceTransactionRepository financeTransaction;
        private IAuditTrailRepository auditTrail;

        public LoanPerformanceRepository(FinTrakBankingContext _contex, IGeneralSetupRepository _genSetup,
            IFinanceTransactionRepository _financeTransaction, IAuditTrailRepository _auditTrail)
        {
            this.context = _contex;
            this.generalSetup = _genSetup;
            this.financeTransaction = _financeTransaction;
            this.auditTrail = _auditTrail;
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
                                   select new PrudGuildlineTypeViewModel
                                   {
                                       prudentialGuildlineTypeId = a.PRUDENTIALGUIDELINETYPEID,
                                       prudentialGuildlineTypeName = a.PRUDENTIALGUIDELINETYPENAME
                                   }).ToList();
            return allFilteredLoan;
        }

        public IEnumerable<PrudentialGuidelineViewModel> GetPrudGuildlineStatus()
        {
            var status = (from a in context.TBL_LOAN_PRUDENTIALGUIDELINE
                          select new PrudentialGuidelineViewModel
                          {
                              prudentialGuidelineId = a.PRUDENTIALGUIDELINESTATUSID,
                              prudentialGuidelineTypeId = a.PRUDENTIALGUIDELINETYPEID,
                              statusName = a.STATUSNAME,

                          }).ToList();
            return status;
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
                                       externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                                       userPrudentialGuidelineStatusId = a.USER_PRUDENTIAL_GUIDE_STATUSID,
                                       internalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.FirstOrDefault(x => x.PRUDENTIALGUIDELINESTATUSID == a.INT_PRUDENT_GUIDELINE_STATUSID).STATUSNAME,
                                       externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.FirstOrDefault(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).STATUSNAME,
                                       userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.FirstOrDefault(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).STATUSNAME,
                                       currencyId = a.CURRENCYID,
                                       productId = a.PRODUCTID,
                                       branchId = a.BRANCHID,
                                       casaAccountId = a.CASAACCOUNTID
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
                                       outstandingInterest = (decimal)a.INTEREST_AMOUNT,
                                       outstandingPrincipal = a.OVERDRAFTLIMIT,
                                       internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                                       externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                                       userPrudentialGuidelineStatusId = a.USER_PRUDENTIAL_GUIDE_STATUSID,
                                       internalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.FirstOrDefault(x => x.PRUDENTIALGUIDELINESTATUSID == a.INT_PRUDENT_GUIDELINE_STATUSID).STATUSNAME,
                                       externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.FirstOrDefault(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).STATUSNAME,
                                       userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.FirstOrDefault(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).STATUSNAME,
                                       currencyId = a.CURRENCYID,
                                       productId = a.PRODUCTID,
                                       branchId = a.BRANCHID,
                                       casaAccountId = a.CASAACCOUNTID
                                   });
            return allFilteredLoan;
        }

        private void LoanPerformancePosting(int prudTypeId, LoanViewModel entity)
        {

            List<FinanceTransactionViewModel> inputTransactions = new List<FinanceTransactionViewModel>();
            if (prudTypeId == 1)
            {
                inputTransactions.AddRange(BuildPerformingToNonPerformingLoanPosting(entity));
            }
            else if (prudTypeId == 2)
            {
                inputTransactions.AddRange(BuildNonPerformingToPerformingLoanPosting(entity));
            }
            if (inputTransactions.Count > 0) financeTransaction.PostTransaction(inputTransactions);
        }

        public List<FinanceTransactionViewModel> BuildPerformingToNonPerformingLoanPosting(LoanViewModel model)
        {
            List<FinanceTransactionViewModel> loanTransaction = new List<FinanceTransactionViewModel>();


            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.LoanPerformance;
            debit.description = "Performing Loan to Non-Performing Loan";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = (short)model.currencyId;
            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId).PRINCIPALBALANCEGL2.Value;
            debit.sourceReferenceNumber = model.loanReferenceNumber;
            debit.casaAccountId = null;
            debit.debitAmount = model.outstandingPrincipal;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.userBranchId;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.LoanPerformance;
            credit.description = "Performing Loan to Non-Performing Loan";
            credit.valueDate = debit.valueDate;
            credit.transactionDate = debit.valueDate;
            credit.currencyId = (short)model.currencyId;
            credit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = debit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;

            var repaymentAccountGL = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId).PRINCIPALBALANCEGL.Value;
            credit.glAccountId = repaymentAccountGL;
            credit.sourceReferenceNumber = model.loanReferenceNumber;
            credit.casaAccountId = model.casaAccountId;
            credit.debitAmount = 0;
            credit.creditAmount = model.outstandingPrincipal;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.userBranchId;


            loanTransaction.Add(debit);
            loanTransaction.Add(credit);


            return loanTransaction;

        }
        public List<FinanceTransactionViewModel> BuildNonPerformingToPerformingLoanPosting(LoanViewModel model)
        {
            List<FinanceTransactionViewModel> loanTransaction = new List<FinanceTransactionViewModel>();

            //  var loanInfo = this.context.TBL_LOAN.FirstOrDefault(x => x.TERMLOANID == model.loanId && x.COMPANYID == model.companyId);

            FinanceTransactionViewModel debit = new FinanceTransactionViewModel();
            debit.operationId = (int)OperationsEnum.LoanPerformance;
            debit.description = "Non-Performing Loan To Performing Loan ";
            debit.valueDate = generalSetup.GetApplicationDate();
            debit.transactionDate = debit.valueDate;
            debit.currencyId = (short)model.currencyId;
            debit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            debit.isApproved = true;
            debit.postedBy = model.createdBy;
            debit.approvedBy = model.createdBy;
            debit.approvedDate = debit.transactionDate;
            debit.approvedDateTime = DateTime.Now;
            debit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            debit.companyId = model.companyId;

            debit.glAccountId = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId).PRINCIPALBALANCEGL.Value;
            debit.sourceReferenceNumber = model.loanReferenceNumber;
            debit.casaAccountId = null;
            debit.debitAmount = model.outstandingPrincipal;
            debit.creditAmount = 0;
            debit.sourceBranchId = model.branchId;
            debit.destinationBranchId = model.userBranchId;


            FinanceTransactionViewModel credit = new FinanceTransactionViewModel();
            credit.operationId = (int)OperationsEnum.LoanPerformance;
            credit.description = "Non-Performing Loan To Performing Loan";
            credit.valueDate = debit.valueDate;
            credit.transactionDate = debit.valueDate;
            credit.currencyId = (short)model.currencyId;
            credit.currencyRate = financeTransaction.GetExchangeRate(debit.valueDate, debit.currencyId, model.companyId).sellingRate;
            credit.isApproved = true;
            credit.postedBy = model.createdBy;
            credit.approvedBy = model.createdBy;
            credit.approvedDate = debit.transactionDate;
            credit.approvedDateTime = DateTime.Now;
            credit.sourceApplicationId = (short)SourceApplicationEnum.FinTrakBanking;
            credit.companyId = model.companyId;

            var repaymentAccountGL = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId).PRINCIPALBALANCEGL2.Value;
            credit.glAccountId = repaymentAccountGL;
            credit.sourceReferenceNumber = model.loanReferenceNumber;
            credit.casaAccountId = model.casaAccountId;
            credit.debitAmount = 0;
            credit.creditAmount = model.outstandingPrincipal;
            credit.sourceBranchId = model.branchId;
            credit.destinationBranchId = model.userBranchId;


            loanTransaction.Add(debit);
            loanTransaction.Add(credit);


            return loanTransaction;

        }
        public bool LoanPerformanceStatusChange(PrudGuidelineStatusChangeViewModel entity)
        {
            var prudTypeId = context.TBL_LOAN_PRUDENTIALGUIDELINE.FirstOrDefault(f => f.PRUDENTIALGUIDELINESTATUSID == entity.prudentialGuidelineStatusId).PRUDENTIALGUIDELINETYPEID;
            if (entity.productTypeId == (int)LoanProductTypeEnum.TermLoan)
            {
                var termLoan = GetTermLoan().Where(x => x.loanId == entity.loanId).FirstOrDefault();
                termLoan.createdBy = entity.createdBy;
                termLoan.userBranchId = entity.userBranchId;
                termLoan.companyId = entity.companyId;
                var loanRecord = context.TBL_LOAN.Find(entity.loanId);
                if (loanRecord != null)
                {
                    loanRecord.USER_PRUDENTIAL_GUIDE_STATUSID = entity.prudentialGuidelineStatusId;
                    LoanPerformancePosting(prudTypeId, termLoan);
                }

            }
            else if (entity.productTypeId == (int)LoanProductTypeEnum.RevolvingLoan)
            {
                var revolvingLoan = GetRevolvingLoan().Where(x => x.loanId == entity.loanId).FirstOrDefault();
                revolvingLoan.createdBy = entity.createdBy;
                revolvingLoan.userBranchId = entity.userBranchId;
                revolvingLoan.companyId = entity.companyId;

                var revolvingLoanRecord = context.TBL_LOAN_REVOLVING.Find(entity.loanId);
                if (revolvingLoanRecord != null)
                {
                    revolvingLoanRecord.USER_PRUDENTIAL_GUIDE_STATUSID = entity.prudentialGuidelineStatusId;
                    LoanPerformancePosting(prudTypeId,  revolvingLoan);
                }
            }
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanPerformanceChange,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Loan Performance Status Change for Loan with LoanId ({entity.loanId})",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            return context.SaveChanges() > 0;
        }
    }
}
