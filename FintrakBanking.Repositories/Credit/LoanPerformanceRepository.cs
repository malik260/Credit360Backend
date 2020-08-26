using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.AlertMonitoring;
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
        private IEmailAlertLogger emailLogger;

        public LoanPerformanceRepository(FinTrakBankingContext _contex, IGeneralSetupRepository _genSetup,
            IFinanceTransactionRepository _financeTransaction, IAuditTrailRepository _auditTrail, IEmailAlertLogger _emailLogger)
        {
            this.context = _contex;
            this.generalSetup = _genSetup;
            this.financeTransaction = _financeTransaction;
            this.auditTrail = _auditTrail;
            this.emailLogger = _emailLogger;
        }
        public IQueryable<LoanViewModel> GetAllLoan()
        {
            try
            {
                //var gerlist = GetTermLoan();
                //var rev = GetRevolvingLoan();
                //var res = gerlist.ToList().Concat(rev.ToList());
                var test = GetTermLoan().Concat(GetRevolvingLoan());
                return test;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public IEnumerable<LoanViewModel> GetAllLoans()
        {
            try
            {
                var gerlist = GetTermLoan();
                var rev = GetRevolvingLoan();
                var res = gerlist.ToList().Concat(rev.ToList());
                //var test = GetTermLoan().Concat(GetRevolvingLoan());
                return res;
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
                                   //join lmsd in context.TBL_LMSR_APPLICATION_DETAIL on a.TERMLOANID equals lmsd.LOANID
                                   //join lms in context.TBL_LMSR_APPLICATION on lmsd.LOANAPPLICATIONID equals lms.LOANAPPLICATIONID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   join d in context.TBL_LOAN_APPLICATION_DETAIL  on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                   join e in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                                   join f in context.TBL_LOAN_APPLICATION_TYPE on e.LOANAPPLICATIONTYPEID equals f.LOANAPPLICATIONTYPEID
                                   join g in context.TBL_PRODUCT on a.PRODUCTID equals g.PRODUCTID
                                   join h in context.TBL_CUSTOMER on a.CUSTOMERID equals h.CUSTOMERID
                                   where a.ISDISBURSED == true
                                   select new LoanViewModel
                                   {
                                       loanSystemTypeId=a.LOANSYSTEMTYPEID,
                                       loanId = a.TERMLOANID,
                                       customerId = a.CUSTOMERID,
                                       customerName = h.FIRSTNAME + " " + h.LASTNAME,//a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER ?? "N/A",//a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = e.LOANAPPLICATIONID,//a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.PRINCIPALAMOUNT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = f.LOANAPPLICATIONTYPENAME,//a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = g.PRODUCTTYPEID,//a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = g.PRODUCTNAME,//a.TBL_PRODUCT.PRODUCTNAME,
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
                                       casaAccountId = a.CASAACCOUNTID,
                                       customerEmail = b.EMAILADDRESS
                                   });
       var bbc =     allFilteredLoan.ToList();
            return allFilteredLoan;
        }
        private IQueryable<LoanViewModel> GetRevolvingLoan()
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_REVOLVING
                                   //join lmsd in context.TBL_LMSR_APPLICATION_DETAIL on a.REVOLVINGLOANID equals lmsd.LOANID
                                   //join lms in context.TBL_LMSR_APPLICATION on lmsd.LOANAPPLICATIONID equals lms.LOANAPPLICATIONID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join c in context.TBL_CASA on a.CASAACCOUNTID equals c.CASAACCOUNTID
                                   join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                   join e in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals e.LOANAPPLICATIONID
                                   join f in context.TBL_LOAN_APPLICATION_TYPE on e.LOANAPPLICATIONTYPEID equals f.LOANAPPLICATIONTYPEID
                                   join g in context.TBL_PRODUCT on a.PRODUCTID equals g.PRODUCTID
                                   join h in context.TBL_CUSTOMER on a.CUSTOMERID equals h.CUSTOMERID
                                   join i in context.TBL_CASA on a.CASAACCOUNTID equals i.CASAACCOUNTID                                   
                                   where a.ISDISBURSED == true
                                   select new LoanViewModel
                                   {
                                       loanSystemTypeId = a.LOANSYSTEMTYPEID,
                                       loanId = a.REVOLVINGLOANID,
                                       customerId = a.CUSTOMERID,
                                       customerName = h.FIRSTNAME + " " + h.LASTNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = e.APPLICATIONREFERENCENUMBER ?? "N/A",//a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       loanApplicationId = e.LOANAPPLICATIONID,//a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONID,
                                       interestRate = a.INTERESTRATE,
                                       principalAmount = a.OVERDRAFTLIMIT,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       loanTypeName = f.LOANAPPLICATIONTYPENAME,//a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                       productTypeId = g.PRODUCTTYPEID,//a.TBL_PRODUCT.PRODUCTTYPEID,
                                       productName = g.PRODUCTNAME,//a.TBL_PRODUCT.PRODUCTNAME,
                                       outstandingInterest = 0,
                                       //outstandingPrincipal = (decimal)i.OVERDRAFTAMOUNT,//(decimal)a.TBL_CASA.OVERDRAFTAMOUNT,
                                       internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                                       externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                                       userPrudentialGuidelineStatusId = a.USER_PRUDENTIAL_GUIDE_STATUSID,
                                       internalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.FirstOrDefault(x => x.PRUDENTIALGUIDELINESTATUSID == a.INT_PRUDENT_GUIDELINE_STATUSID).STATUSNAME,
                                       externalPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.FirstOrDefault(x => x.PRUDENTIALGUIDELINESTATUSID == a.EXT_PRUDENT_GUIDELINE_STATUSID).STATUSNAME,
                                       userPrudentialGuidelineStatus = context.TBL_LOAN_PRUDENTIALGUIDELINE.FirstOrDefault(x => x.PRUDENTIALGUIDELINESTATUSID == a.USER_PRUDENTIAL_GUIDE_STATUSID).STATUSNAME,
                                       currencyId = a.CURRENCYID,
                                       productId = a.PRODUCTID,
                                       branchId = a.BRANCHID,
                                       casaAccountId = a.CASAACCOUNTID,
                                       customerEmail =b.EMAILADDRESS
                                   });
       var bbw =     allFilteredLoan.ToList();
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
            credit.casaAccountId = null;
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

            var repaymentAccountGL = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == model.productId).PRINCIPALBALANCEGL2;
            credit.glAccountId = (int)repaymentAccountGL;
            credit.sourceReferenceNumber = model.loanReferenceNumber;
            credit.casaAccountId = null;
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
            string emailBody = "";
            string emailSubject = "";
            //var option = new TransactionOptions
            //{
            //    IsolationLevel = IsolationLevel.ReadCommitted,
            //    Timeout = TimeSpan.FromSeconds(60)
            //};
            //using (var scopeOuter = new TransactionScope(TransactionScopeOption.Required, option))
            //{

            //}
            //    var prudTypeId = context.TBL_LOAN_PRUDENTIALGUIDELINE.FirstOrDefault(f => f.PRUDENTIALGUIDELINESTATUSID == entity.prudentialGuidelineStatusId).PRUDENTIALGUIDELINETYPEID;
            //if (entity.productTypeId == (int)LoanProductTypeEnum.TermLoan)
            if (entity.loanSystemTypeId == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                var termLoan = GetTermLoan().Where(x => x.loanId == entity.loanId).FirstOrDefault();
                termLoan.createdBy = entity.createdBy;
                termLoan.userBranchId = entity.userBranchId;
                termLoan.companyId = entity.companyId;
                var loanRecord = context.TBL_LOAN.Find(entity.loanId);
                if (loanRecord != null)
                {
                    loanRecord.USER_PRUDENTIAL_GUIDE_STATUSID = entity.prudentialGuidelineStatusId;
                    context.SaveChanges();
                    //LoanPerformancePosting(prudTypeId, termLoan);
                }

                //Send Email to Customer
                string loanClasssification = context.TBL_LOAN_PRUDENT_GUIDE_TYPE.Where(o => o.PRUDENTIALGUIDELINETYPEID == entity.prudentialGuidelineStatusId).Select(o => o.PRUDENTIALGUIDELINETYPENAME).FirstOrDefault();
                emailBody = "Dear Valuable Customer, <br /><br /> Your facility with Reference Number: " + termLoan.loanReferenceNumber + " has been classified as "+ loanClasssification + ".,<br /> Kindly contact your Relationship Manager for more information.";
                emailSubject = "LOAN PERFORMANCE STATUS REPORT";

                emailLogger.ComposeEmail(termLoan.loanReferenceNumber, emailBody, emailSubject, termLoan.customerEmail,false);
            }
            else if (entity.loanSystemTypeId == (int)LoanSystemTypeEnum.OverdraftFacility)
            {
                var revolvingLoan = GetRevolvingLoan().Where(x => x.loanId == entity.loanId).FirstOrDefault();
                revolvingLoan.createdBy = entity.createdBy;
                revolvingLoan.userBranchId = entity.userBranchId;
                revolvingLoan.companyId = entity.companyId;

                var revolvingLoanRecord = context.TBL_LOAN_REVOLVING.Find(entity.loanId);
                if (revolvingLoanRecord != null)
                {
                    revolvingLoanRecord.USER_PRUDENTIAL_GUIDE_STATUSID = entity.prudentialGuidelineStatusId;
                    //LoanPerformancePosting(prudTypeId,  revolvingLoan);
                    context.SaveChanges();
                }

                //Send Email to Customer
                string loanClasssification = context.TBL_LOAN_PRUDENT_GUIDE_TYPE.Where(o => o.PRUDENTIALGUIDELINETYPEID == entity.prudentialGuidelineStatusId).Select(o => o.PRUDENTIALGUIDELINETYPENAME).FirstOrDefault();
                emailBody = "Dear Valuable Customer, <br /><br /> Your facility with Reference Number: " + revolvingLoan.loanReferenceNumber + " has been classified as " + loanClasssification + ".,<br /> Kindly contact your Relationship Manager for more information.";
                emailSubject = "LOAN PERFORMANCE STATUS REPORT";

                emailLogger.ComposeEmail(revolvingLoan.loanReferenceNumber, emailBody, emailSubject, revolvingLoan.customerEmail,false);
            }

            //Send email to customer



            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanPerformanceChange,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Loan Performance Status Change for Loan with LoanId ({entity.loanId})",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()


            };
            this.auditTrail.AddAuditTrail(audit);

            return context.SaveChanges() > 0;
        }


        private IQueryable<LoanViewModel> GetTermLoanFromNonPerformingToPerforming()
        {
            var allFilteredLoan = (from a in context.TBL_LOAN
                                   where a.ISDISBURSED == true && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                   && a.EXT_PRUDENT_GUIDELINE_STATUSID != a.USER_PRUDENTIAL_GUIDE_STATUSID
                                   && a.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Doubtful
                                   || a.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Substandard
                                   || a.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Lost
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
                                       currencyId = a.CURRENCYID,
                                       productId = a.PRODUCTID,
                                       branchId = a.BRANCHID,
                                       casaAccountId = a.CASAACCOUNTID,
                                       companyId = a.COMPANYID,
                                       createdBy = (int)SystemStaff.System,
                                       userBranchId = a.BRANCHID,
                                   });
            var bbc = allFilteredLoan.ToList();

            foreach(var item in allFilteredLoan)
            {
                BuildNonPerformingToPerformingLoanPosting(item);

                TBL_LOAN result = (from p in context.TBL_LOAN
                                   where p.TERMLOANID == item.loanId
                                   select p).SingleOrDefault();

                result.USER_PRUDENTIAL_GUIDE_STATUSID = (int)item.externalPrudentialGuidelineStatusId;

                context.SaveChanges();
            }


            return allFilteredLoan;
        }
        private IQueryable<LoanViewModel> GetRevolvingLoanNonPerformingToPerforming()
        {
            var allFilteredLoan = (from a in context.TBL_LOAN_REVOLVING
                                   where a.ISDISBURSED == true && a.LOANSTATUSID == (short)LoanStatusEnum.Active
                                    && a.EXT_PRUDENT_GUIDELINE_STATUSID != a.USER_PRUDENTIAL_GUIDE_STATUSID
                                    && a.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Doubtful
                                    || a.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Substandard
                                    || a.EXT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Lost
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
                                       outstandingInterest = 0,
                                       outstandingPrincipal = (decimal)a.TBL_CASA.OVERDRAFTAMOUNT,
                                       internalPrudentialGuidelineStatusId = a.INT_PRUDENT_GUIDELINE_STATUSID,
                                       externalPrudentialGuidelineStatusId = a.EXT_PRUDENT_GUIDELINE_STATUSID,
                                       userPrudentialGuidelineStatusId = a.USER_PRUDENTIAL_GUIDE_STATUSID,
                                       currencyId = a.CURRENCYID,
                                       productId = a.PRODUCTID,
                                       branchId = a.BRANCHID,
                                       casaAccountId = a.CASAACCOUNTID,
                                       companyId = a.COMPANYID,
                                       createdBy = (int)SystemStaff.System,
                                       userBranchId = a.BRANCHID,
                                   });
            var bbw = allFilteredLoan.ToList();

            foreach (var item in allFilteredLoan)
            {
                BuildNonPerformingToPerformingLoanPosting(item);

                TBL_LOAN_REVOLVING result = (from p in context.TBL_LOAN_REVOLVING
                                   where p.REVOLVINGLOANID == item.loanId
                                   select p).SingleOrDefault();

                result.USER_PRUDENTIAL_GUIDE_STATUSID = (int)item.externalPrudentialGuidelineStatusId;

                context.SaveChanges();
            }
            return allFilteredLoan;
        }

    }
}
