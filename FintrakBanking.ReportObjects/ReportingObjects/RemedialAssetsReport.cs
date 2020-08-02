using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public partial class RemedialAssetsReport
    {
        public IEnumerable<LoanReviewOperationApprovalViewModel> OutOfCourtSettlement(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join a in context.TBL_LOAN_APPLICATION on ln.APPLICATIONREFERENCENUMBER equals a.APPLICATIONREFERENCENUMBER
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERID
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.APPLICATIONREFERENCENUMBER  equals b.APPLICATIONREFERENCENUMBER
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new LoanReviewOperationApprovalViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();
               
                return dataLoan;
            }

        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> CollateralSales(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join a in context.TBL_LOAN_APPLICATION on ln.APPLICATIONREFERENCENUMBER equals a.APPLICATIONREFERENCENUMBER
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERID
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new LoanReviewOperationApprovalViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();

                return dataLoan;
            }

        }
        public IEnumerable<LoanReviewOperationApprovalViewModel> RecoveryAgentUpdate(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join a in context.TBL_LOAN_APPLICATION on ln.APPLICATIONREFERENCENUMBER equals a.APPLICATIONREFERENCENUMBER
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERID
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new LoanReviewOperationApprovalViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfAssignment = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                    email = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().EMAILADDRESS,

                                }).ToList();
                var groupRecord = dataLoan.GroupBy(x => x.accreditedConsultant)
                    .Select(g => g.OrderByDescending(b => b.accreditedConsultant).FirstOrDefault())
                ; ;
                foreach(var i in groupRecord)
                {
                    i.listOfAccountAssigned = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(x => x.ACCREDITEDCONSULTANT == i.accreditedConsultant).Select(x => x.APPLICATIONREFERENCENUMBER).ToList();
;                }

                return dataLoan;
            }

        }
        public IEnumerable<LoanReviewOperationApprovalViewModel> RecoveryCommission(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join a in context.TBL_LOAN_APPLICATION on ln.APPLICATIONREFERENCENUMBER equals a.APPLICATIONREFERENCENUMBER
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERID
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new LoanReviewOperationApprovalViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();

                return dataLoan;
            }

        }
        public IEnumerable<LoanReviewOperationApprovalViewModel> RecoveryAgentPerformance(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join a in context.TBL_LOAN_APPLICATION on ln.APPLICATIONREFERENCENUMBER equals a.APPLICATIONREFERENCENUMBER
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERID
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate) 
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new LoanReviewOperationApprovalViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();

                return dataLoan;
            }

        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> LitigationRecoveries(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join a in context.TBL_LOAN_APPLICATION on ln.APPLICATIONREFERENCENUMBER equals a.APPLICATIONREFERENCENUMBER
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERID
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new LoanReviewOperationApprovalViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();

                return dataLoan;
            }

        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> RevalidationOfFullAndFinalSettlement(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join a in context.TBL_LOAN_APPLICATION on ln.APPLICATIONREFERENCENUMBER equals a.APPLICATIONREFERENCENUMBER
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERID
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new LoanReviewOperationApprovalViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();

                return dataLoan;
            }

        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> IdleAssetsSales(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join a in context.TBL_LOAN_APPLICATION on ln.APPLICATIONREFERENCENUMBER equals a.APPLICATIONREFERENCENUMBER
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERID
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new LoanReviewOperationApprovalViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();

                return dataLoan;
            }

        }

        public IEnumerable<LoanReviewOperationApprovalViewModel> FullAndFinalSettlementAndWaivers(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var dataLoan = (from ln in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join a in context.TBL_LOAN_APPLICATION on ln.APPLICATIONREFERENCENUMBER equals a.APPLICATIONREFERENCENUMBER
                                join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERID
                                join b in context.TBL_COLLATERAL_LIQUIDATION_RECOVERY on ln.APPLICATIONREFERENCENUMBER equals b.APPLICATIONREFERENCENUMBER
                                where
                                (DbFunctions.TruncateTime(ln.DATEASSIGNED) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(ln.DATEASSIGNED) <= DbFunctions.TruncateTime(endDate))
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                select new LoanReviewOperationApprovalViewModel
                                {
                                    accountNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().ACCOUNTNUMBER,
                                    accountName = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    nameOfRecoveryAgent = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().NAME,
                                    address = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().FIRMNAME,
                                    telephoneNumber = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == ln.ACCREDITEDCONSULTANT).FirstOrDefault().PHONENUMBER,
                                    expectedRecoveryDate = (DateTime)ln.EXPCOMPLETIONDATE,
                                    amountRecovered = b.ISFULLYRECOVERED == true ? b.TOTALRECOVERYAMOUNT : b.RECOVEREDAMOUNT,
                                    dateOfEngagement = ln.DATEASSIGNED,
                                    accountBalance = (double)0.0,
                                }).ToList();

                return dataLoan;
            }

        }
    }
}
