using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.External;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.External.Customer;
using FintrakBanking.ViewModels.External.Document;
using FintrakBanking.ViewModels.External.Loan;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.CreditLimitValidations;
using System.IO;
using System.ServiceModel.Channels;
using System.Data.Entity.Migrations;
using ServiceStack;

namespace FintrakBanking.Repositories.External
{
    using LoanPaymentScheduleVM = ViewModels.External.Loan.LoanPaymentScheduleVM;
    using wct = XLeratorDLL_financial.XLeratorDLL_financial;
    public class LoanRepositoryExternal : ILoanRepositoryExternal
    {
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IReportRoutes repoReport;
        private ILoanApplicationRepository loanApplicationRepository;
        private ICreditLimitValidationsRepository limitValidation;
        private IIntegrationWithFinacle integration;


        public bool isGroupLoan;
        public TBL_LOAN_APPLICATION loanData;

        bool USE_THIRD_PARTY_INTEGRATION = false;

        public LoanRepositoryExternal(IReportRoutes _repoReport, ILoanApplicationRepository _loanApplicationRepository, ICreditLimitValidationsRepository _limitValidation, IAuditTrailRepository _auditTrail, IGeneralSetupRepository _genSetup, IIntegrationWithFinacle _integration)
        {
            this.repoReport = _repoReport;
            this.loanApplicationRepository = _loanApplicationRepository;
            this.limitValidation = _limitValidation;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            this.integration = _integration;

            using (var context = new FinTrakBankingContext())
            {
                var global = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (global != null) this.USE_THIRD_PARTY_INTEGRATION = global.USE_THIRD_PARTY_INTEGRATION;
            }
        }

        public List<LoanPaymentScheduleVM> LoanSchedule(string applicationRefNo)
        {
            using (var context = new FinTrakBankingContext())
            {
                var loan = context.TBL_LOAN_APPLICATION.FirstOrDefault(c => c.APPLICATIONREFERENCENUMBER.ToLower() == applicationRefNo.Trim().ToLower());
                if (loan != null)
                {
                    //int loanId = loan.TERMLOANID;
                    var loanSchedule = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                                        join b in context.TBL_LOAN on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                        join sch in context.TBL_LOAN_SCHEDULE_PERIODIC on b.TERMLOANID equals sch.LOANID
                                        where a.LOANAPPLICATIONID == loan.LOANAPPLICATIONID
                                        orderby sch.PAYMENTDATE ascending
                                        select new LoanPaymentScheduleVM
                                        {
                                            //loanId = sch.LOANID,
                                            paymentNumber = sch.PAYMENTNUMBER,
                                            paymentDate = (DateTime)sch.PAYMENTDATE,
                                            startPrincipalAmount = (double)sch.STARTPRINCIPALAMOUNT,
                                            periodPaymentAmount = (double)sch.PERIODPAYMENTAMOUNT,
                                            periodInterestAmount = (double)sch.PERIODINTERESTAMOUNT,
                                            periodPrincipalAmount = (double)sch.PERIODPRINCIPALAMOUNT,
                                            endPrincipalAmount = (double)sch.ENDPRINCIPALAMOUNT,
                                            interestRate = (decimal)sch.INTERESTRATE,
                                            //amortisedStartPrincipalAmount = (double)sch.AMORTISEDSTARTPRINCIPALAMOUNT,
                                            //amortisedPeriodPaymentAmount = (double)sch.AMORTISEDPERIODPAYMENTAMOUNT,
                                            //amortisedPeriodInterestAmount = (double)sch.AMORTISEDPERIODINTERESTAMOUNT,
                                            //amortisedPeriodPrincipalAmount = (double)sch.AMORTISEDPERIODPRINCIPALAMOUNT,
                                            //amortisedEndPrincipalAmount = (double)sch.AMORTISEDENDPRINCIPALAMOUNT,
                                            //effectiveInterestRate = (double)sch.EFFECTIVEINTERESTRATE
                                        }).ToList();
                    return loanSchedule;
                }
                return null;
            }
        }

        public List<ScheduleLoans> GetNHFLoans(string nhfNo)
        {
            using (var context = new FinTrakBankingContext())
            {
                var account = context.TBL_CASA.FirstOrDefault(c => c.PRODUCTACCOUNTNUMBER.ToLower() == nhfNo.Trim().ToLower());
                if (account != null)
                {
                    var loans = (from a in context.TBL_CASA
                                 join b in context.TBL_LOAN on a.CASAACCOUNTID equals b.CASAACCOUNTID
                                 join c in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                                 join d in context.TBL_LOAN_APPLICATION on c.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                 where a.PRODUCTACCOUNTNUMBER.ToLower() == nhfNo.Trim().ToLower()
                                 orderby b.TERMLOANID ascending
                                 select new ScheduleLoans
                                 {
                                     applicationReferenceNumber = d.APPLICATIONREFERENCENUMBER,
                                     loanReferenceNumber = b.LOANREFERENCENUMBER,
                                     nhfAccount = a.PRODUCTACCOUNTNUMBER,
                                     loanAmount = b.PRINCIPALAMOUNT,
                                     interestRate = (decimal)c.APPROVEDINTERESTRATE,
                                     product = context.TBL_PRODUCT.Where(b => b.PRODUCTID == c.APPROVEDPRODUCTID).Select(p => p.PRODUCTNAME).FirstOrDefault()
                                 }).ToList();
                    return loans;
                }
                return null;
            }
        }

        public List<LoanVM> GetDisbursedLoans(int companyId)
        {
            using (var context = new FinTrakBankingContext())
            {
                var loans = (from a in context.TBL_COMPANY
                             join b in context.TBL_LOAN on a.COMPANYID equals b.COMPANYID
                             join c in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                             join d in context.TBL_LOAN_APPLICATION on c.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                             join e in context.TBL_CUSTOMER on b.CUSTOMERID equals e.CUSTOMERID
                             where a.COMPANYID == companyId && b.ISDISBURSED == true
                             orderby b.TERMLOANID ascending
                             select new LoanVM
                             {
                                 applicationReferenceNumber = d.APPLICATIONREFERENCENUMBER,
                                 loanReferenceNumber = b.LOANREFERENCENUMBER,
                                 accountNumber = b.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                 loanAmount = b.PRINCIPALAMOUNT,
                                 interestRate = c.APPROVEDINTERESTRATE,
                                 product = context.TBL_PRODUCT.Where(b => b.PRODUCTID == c.APPROVEDPRODUCTID).Select(p => p.PRODUCTNAME).FirstOrDefault(),
                                 loanStatus = b.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                 disbursedDate = b.DISBURSEDATE,
                                 company = a.NAME,
                                 customerName = e.FIRSTNAME + " " + e.MIDDLENAME + " " + e.LASTNAME,
                                 tenor = d.APPLICATIONTENOR

                             }).ToList();
                return loans;
            }
        }

        public List<AffordabilityViewModel> AffordabilityChecks(AffordabilityViewModel model)
        {
            using (var context = new FinTrakBankingContext())
            {
                List<AffordabilityViewModel> rec = new List<AffordabilityViewModel>();

                var CustomerAccount = context.TBL_CASA.Where(x => x.PRODUCTACCOUNTNUMBER == model.nhfAccount).FirstOrDefault();

                if (CustomerAccount == null)
                {
                    throw new SecureException($"Customer with NHF account {model.nhfAccount} does not exist");
                }

                var product = context.TBL_PRODUCT.Where(p => p.PRODUCTID == model.productId).FirstOrDefault();
                var repaymentConst = product.MAXIMUMTENOR;
                var maxProductAmount = product.MAXIMUMAMOUNT;
                var productName = product.PRODUCTNAME;
                var maxProductRate = product.MAXIMUMRATE;

                if (model.amountRequested > (double)maxProductAmount)
                {
                    throw new SecureException($"Maximum product Amount Exceeded! The maximum product amount for {productName} is NGN{maxProductAmount.Value.ToString("N")}");
                }

                if (model.proposedTenor > repaymentConst)
                {
                    throw new SecureException($"Maximum product Tenor Exceeded! The maximum tenor for {productName} is {repaymentConst} years");
                }

                const int yearsConst = 60;
                const int yearsInServiceConst = 35;


                //var approvedAmountMultiple = context.TBL_SETUP_GLOBAL.FirstOrDefault().AMOUNTMULTIPLE;

                DateTime applicationDate = genSetup.GetApplicationDate();
                try
                {
                    rec = (from a in context.TBL_CASA
                           join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                           where a.CASAACCOUNTID == CustomerAccount.CASAACCOUNTID
                           select new AffordabilityViewModel()
                           {
                               dateOfBirth = b.DATEOFBIRTH,
                               dateOfEmployment = b.DATEOFEMPLOYMENT,
                               nhfAccount = a.PRODUCTACCOUNTNUMBER,
                               customerName = b.FIRSTNAME + " " + b.LASTNAME + " " + b.MIDDLENAME,
                               customerId = b.CUSTOMERID,
                               monthlyIncome = b.MONTHLYINCOME,
                               sortCode = b.OTHERBANKSORTCODE,
                           }).ToList().Select(x =>
                           {
                               x.quotient = 0;
                               //int result;
                               x.customerName = x.customerName.ToUpper();
                               x.age = (applicationDate.Subtract(x.dateOfBirth.Value).Days) / 365;
                               x.yearsToClockSixty = yearsConst - x.age;
                               x.yearsInService = (applicationDate.Subtract(x.dateOfEmployment.Value).Days) / 365;
                               x.yearsToClockThirtyFive = yearsInServiceConst - x.yearsInService;
                               x.minYearsInServiceYearsToClockSixty = Math.Min(x.yearsToClockSixty, x.yearsToClockThirtyFive);
                               x.repaymentPeriod = (model.proposedTenor == 0 || model.proposedTenor < 0) ? Math.Min(x.minYearsInServiceYearsToClockSixty, repaymentConst) : model.proposedTenor > 0 ? Math.Min(repaymentConst, model.proposedTenor) : 0;
                               x.presentValue = Math.Round(-wct.PV((double)maxProductRate / 1200, x.repaymentPeriod * 12, (double)x.monthlyIncome / 3, 0, 0), 0);
                               //x.presentValue = 100000;
                               x.affordableAmount = x.presentValue > model.amountRequested ? model.amountRequested : x.presentValue;
                               //x.quotient = Math.DivRem((int)x.affordableAmount, (int)approvedAmountMultiple, out result);
                               //x.affordableAmount = x.quotient * (int)approvedAmountMultiple;
                               x.monthlyRepayment = Math.Abs(Math.Round(wct.PMT((double)maxProductRate / 1200, x.repaymentPeriod * 12, x.affordableAmount, 0, 0), 2));
                               //x.monthlyRepayment = 5000;
                               x.profitability = Math.Round((x.monthlyRepayment * 12 * x.repaymentPeriod) - x.affordableAmount, 2);
                               x.rate = (double)maxProductRate;
                               x.amountRequested = model.amountRequested;
                               x.tenorOverride = model.proposedTenor > 0 ? true : false;
                               x.productId = model.productId;
                               x.casaAccountId = CustomerAccount.CASAACCOUNTID;
                               return x;
                           }).ToList();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return rec;
            }
        }


        public async Task<List<LoanApplicationForReturn>> GetLoanApplicationByCustomer(string customerCode)
        {
            using (var context = new FinTrakBankingContext())
            {
                return await GetLoanApplication(context).Where(o => o.customerCode == customerCode).ToListAsync();
            }
        }

        public async Task<LoanApplicationForReturn> GetLoanApplicationByRefNo(string applicationRefNo)
        {
            using (var context = new FinTrakBankingContext())
            {
                return await GetLoanApplication(context).Where(o => o.applicationReferenceNumber == applicationRefNo).FirstOrDefaultAsync();
            }
        }

        public async Task<LoanApplicationForReturn> GetLoanApplicationByRefNo(string applicationRefNo, FinTrakBankingContext context)
        {
            return await GetLoanApplication(context).Where(o => o.applicationReferenceNumber == applicationRefNo).FirstOrDefaultAsync();
        }

        public async Task<LoanApplicationForReturn> GetBatchedLoanApplicationByRefNo(string applicationRefNo, FinTrakBankingContext context)
        {
            return await GetBatchedLoanApplication(context).Where(o => o.applicationReferenceNumber == applicationRefNo).FirstOrDefaultAsync();
        }

        public IQueryable<LoanApplicationForReturn> GetLoanApplication(FinTrakBankingContext context)
        {

            int[] operations = {
                    (int)OperationsEnum.OfferLetterApproval,
                    (int)OperationsEnum.CreditAppraisal,
                    (int)OperationsEnum.ContigentLoanBooking,
                    (int)OperationsEnum.ContingentLiabilityRenewal,
                    (int)OperationsEnum.ContingentLiabilityUsage,
                    (int)OperationsEnum.ContingentRequestBooking,
                    (int)OperationsEnum.CommercialLoanBooking,
                    (int)OperationsEnum.LoanAvailment
                };

            var applications = (from x in context.TBL_LOAN_APPLICATION
                                join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                where
                                //c.CUSTOMERCODE == customerCode && 
                                x.DELETED == false

                                let atrail = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).OrderByDescending(r => r.APPROVALTRAILID).ThenByDescending(w => w.SYSTEMARRIVALDATETIME).FirstOrDefault()
                                let branchRegion = context.TBL_BRANCH_REGION.Where(o => o.REGIONID == x.CAPREGIONID).FirstOrDefault()
                                //let availmentRegion = x.CAPREGIONID != null ? context.TBL_BRANCH_REGION_STAFF.Where(o => o.REGIONID == branchRegion.REGIONID2).FirstOrDefault() : null
                                let houStaff = x.CAPREGIONID != null ? context.TBL_BRANCH_REGION_STAFF.Where(o => o.REGIONID == branchRegion.REGIONID).FirstOrDefault() : null
                                //let availmentStaff = x.CAPREGIONID != null ? context.TBL_BRANCH_REGION_STAFF.Where(o => o.REGIONID == branchRegion.REGIONID2).FirstOrDefault() : null
                                let houStaffRec = x.CAPREGIONID != null ? context.TBL_STAFF.Where(o => o.STAFFID == houStaff.STAFFID).FirstOrDefault() : null
                                //let availmentStaffRec = x.CAPREGIONID != null ? context.TBL_STAFF.Where(o => o.STAFFID == availmentStaff.STAFFID).FirstOrDefault() : null

                                select new LoanApplicationForReturn
                                {
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    customerCode = c.CUSTOMERCODE,
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    //loanApplicationId = x.LOANAPPLICATIONID, 
                                    loanBranch = x.TBL_BRANCH.BRANCHNAME + " - " + x.TBL_BRANCH.BRANCHCODE,
                                    //customerGroupId = x.CUSTOMERGROUPID,
                                    //loanTypeId = x.LOANAPPLICATIONTYPEID,
                                    //relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                                    //relationshipManagerId = x.RELATIONSHIPMANAGERID,
                                    applicationDate = (DateTime)x.APPLICATIONDATE,
                                    //applicationAmount = x.APPLICATIONAMOUNT,
                                    //approvedAmount = x.APPROVEDAMOUNT,
                                    //interestRate = x.INTERESTRATE,
                                    //applicationTenor = x.APPLICATIONTENOR, 
                                    submittedForAppraisal = x.SUBMITTEDFORAPPRAISAL,
                                    approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,

                                    currentApprovalLevel = atrail != null ? atrail.TOAPPROVALLEVELID != null ? atrail.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a" : "n/a",

                                    //applicationStatusId = x.APPLICATIONSTATUSID,
                                    applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new  
                                    relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME + " - " + x.TBL_STAFF.STAFFCODE,
                                    relationshipManagerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME + " - " + x.TBL_STAFF.STAFFCODE,
                                    isOfferLetterAvailable = context.TBL_LOAN_OFFER_LETTER.Where(ol => ol.LOANAPPLICATIONID == x.LOANAPPLICATIONID && ol.ISLMS == false).Any(),

                                    loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == x.LOANAPPLICATIONID)
                                     .Select(c => new LoanApplicationDetailForReturn()
                                     {
                                         //equityAmount = c.EQUITYAMOUNT,
                                         //equityCasaAccountId = c.EQUITYCASAACCOUNTID,
                                         amount = c.APPROVEDAMOUNT,
                                         //approvedInterestRate = c.APPROVEDINTERESTRATE,
                                         productId = (short)c.APPROVEDPRODUCTID,
                                         approvedTenor = c.APPROVEDTENOR,
                                         //currencyId = c.CURRENCYID,
                                         //currencyName = c.TBL_CURRENCY.CURRENCYNAME,
                                         //customerId = c.CUSTOMERID,
                                         //exchangeRate = c.EXCHANGERATE,
                                         //loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                                         //subSectorId = c.SUBSECTORID,
                                         //loanApplicationId = c.LOANAPPLICATIONID,
                                         //proposedAmount = c.PROPOSEDAMOUNT,
                                         //proposedInterestRate = c.PROPOSEDINTERESTRATE,
                                         //proposedProductId = c.PROPOSEDPRODUCTID,
                                         productName = c.TBL_PRODUCT.PRODUCTNAME,
                                         customerName = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == c.CUSTOMERID).Select(cc => cc.FIRSTNAME + " " + cc.MIDDLENAME + " " + cc.LASTNAME).FirstOrDefault(),
                                         nhfAccount = context.TBL_CASA.Where(cc => cc.CUSTOMERID == c.CUSTOMERID).Select(cc => cc.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                         //statusId = c.STATUSID

                                     }).ToList()

                                });


            return applications;
        }

        public IQueryable<LoanApplicationForReturn> GetBatchedLoanApplication(FinTrakBankingContext context)
        {

            int[] operations = {
                    (int)OperationsEnum.OfferLetterApproval,
                    (int)OperationsEnum.CreditAppraisal,
                    (int)OperationsEnum.ContigentLoanBooking,
                    (int)OperationsEnum.ContingentLiabilityRenewal,
                    (int)OperationsEnum.ContingentLiabilityUsage,
                    (int)OperationsEnum.ContingentRequestBooking,
                    (int)OperationsEnum.CommercialLoanBooking,
                    (int)OperationsEnum.LoanAvailment
                };

            var applications = (from x in context.TBL_LOAN_APPLICATION
                                join c in context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                where
                                //c.CUSTOMERCODE == customerCode && 
                                x.DELETED == false

                                let atrail = context.TBL_APPROVAL_TRAIL.Where(o => o.TARGETID == x.LOANAPPLICATIONID && operations.Contains(o.OPERATIONID)).OrderByDescending(r => r.APPROVALTRAILID).ThenByDescending(w => w.SYSTEMARRIVALDATETIME).FirstOrDefault()
                                let branchRegion = context.TBL_BRANCH_REGION.Where(o => o.REGIONID == x.CAPREGIONID).FirstOrDefault()
                                //let availmentRegion = x.CAPREGIONID != null ? context.TBL_BRANCH_REGION_STAFF.Where(o => o.REGIONID == branchRegion.REGIONID2).FirstOrDefault() : null
                                let houStaff = x.CAPREGIONID != null ? context.TBL_BRANCH_REGION_STAFF.Where(o => o.REGIONID == branchRegion.REGIONID).FirstOrDefault() : null
                                //let availmentStaff = x.CAPREGIONID != null ? context.TBL_BRANCH_REGION_STAFF.Where(o => o.REGIONID == branchRegion.REGIONID2).FirstOrDefault() : null
                                let houStaffRec = x.CAPREGIONID != null ? context.TBL_STAFF.Where(o => o.STAFFID == houStaff.STAFFID).FirstOrDefault() : null
                                //let availmentStaffRec = x.CAPREGIONID != null ? context.TBL_STAFF.Where(o => o.STAFFID == availmentStaff.STAFFID).FirstOrDefault() : null

                                select new LoanApplicationForReturn
                                {
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    customerCode = c.CUSTOMERCODE,
                                    applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                                    loanBranch = x.TBL_BRANCH.BRANCHNAME + " - " + x.TBL_BRANCH.BRANCHCODE,
                                    applicationDate = (DateTime)x.APPLICATIONDATE,
                                    submittedForAppraisal = x.SUBMITTEDFORAPPRAISAL,
                                    approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == x.APPROVALSTATUSID).APPROVALSTATUSNAME,

                                    currentApprovalLevel = atrail != null ? atrail.TOAPPROVALLEVELID != null ? atrail.TBL_APPROVAL_LEVEL1.LEVELNAME : "n/a" : "n/a",

                                    //applicationStatusId = x.APPLICATIONSTATUSID,
                                    applicationStatus = context.TBL_LOAN_APPLICATION_STATUS.Where(o => o.APPLICATIONSTATUSID == x.APPLICATIONSTATUSID).Select(o => o.APPLICATIONSTATUSNAME).FirstOrDefault(), // <----------------- new  
                                    relationshipOfficerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME + " - " + x.TBL_STAFF.STAFFCODE,
                                    relationshipManagerName = x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.MIDDLENAME + " " + x.TBL_STAFF.LASTNAME + " - " + x.TBL_STAFF.STAFFCODE,
                                    isOfferLetterAvailable = context.TBL_LOAN_OFFER_LETTER.Where(ol => ol.LOANAPPLICATIONID == x.LOANAPPLICATIONID && ol.ISLMS == false).Any(),

                                    loanApplicationDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == x.LOANAPPLICATIONID)
                                     .Select(c => new LoanApplicationDetailForReturn()
                                     {
                                         amount = c.APPROVEDAMOUNT,
                                         productId = (short)c.APPROVEDPRODUCTID,
                                         approvedTenor = c.APPROVEDTENOR,
                                         productName = c.TBL_PRODUCT.PRODUCTNAME,
                                         customerName = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == c.CUSTOMERID).Select(cc => cc.FIRSTNAME + " " + cc.MIDDLENAME + " " + cc.LASTNAME).FirstOrDefault(),
                                         nhfAccount = context.TBL_CASA.Where(cc => cc.CUSTOMERID == c.CUSTOMERID).Select(cc => cc.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                     }).ToList()
                                });

            return applications;
        }


        public async Task<string> DownloadOfferLetter(string applicationRefNo)
        {
            using (var context = new FinTrakBankingContext())
            {
                var loanApplication = await context.TBL_LOAN_APPLICATION.FirstOrDefaultAsync(f => f.APPLICATIONREFERENCENUMBER == applicationRefNo);
                var staff = await context.TBL_STAFF.FirstOrDefaultAsync(s => s.STAFFID == loanApplication.CREATEDBY);

                if (loanApplication != null && staff != null)
                {
                    //var offerLetterURL = repoReport.GetGeneratedOfferLetterNew(loanApplication.CREATEDBY, staff.STAFFROLEID, loanApplication.APPLICATIONREFERENCENUMBER, "docx");
                    //return offerLetterURL;
                    return "check again later";
                }

                return "Kinldy confirm the reference no";
            }
        }


        //private int hasCustomerAppliedForNHFLoan(int customerId, FinTrakBankingContext context)
        //{
        //    try
        //    {

        //        int[] disapprovedCancelledApplicationStatusId = {(int)LoanApplicationStatusEnum.ApplicationRejected,
        //                                                            (int)LoanApplicationStatusEnum.CancellationCompleted};

        //        var nhf = (from l in context.TBL_LOAN
        //                   where (l.CUSTOMERID == customerId && l.PRODUCTID == (int)FMBNProductEnum.NHFMortgageLoan && l.ISDISBURSED == 1)
        //                   select l.LOANREFERENCENUMBER).FirstOrDefault();

        //        var nhf2 = (from l in context.TBL_LOAN_APPLICATION
        //                    join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
        //                    where (l.CUSTOMERID == customerId && b.PROPOSEDPRODUCTID == (int)FMBNProductEnum.NHFMortgageLoan && !disapprovedCancelledApplicationStatusId.Contains(l.APPLICATIONSTATUSID))
        //                    select l.APPLICATIONREFERENCENUMBER).FirstOrDefault();

        //        var hrl = (from l in context.TBL_LOAN
        //                      where (l.CUSTOMERID == customerId && l.PRODUCTID == (int)FMBNProductEnum.HomeRenovationLoan && l.LOANSTATUSID != (int)LoanStatusEnum.Completed)
        //                      select l.LOANREFERENCENUMBER).FirstOrDefault();

        //        var rto = (from l in context.TBL_LOAN
        //                   where (l.CUSTOMERID == customerId && l.PRODUCTID == (int)FMBNProductEnum.HomeRenovationLoan && l.LOANSTATUSID != (int)LoanStatusEnum.Completed)
        //                   select l.LOANREFERENCENUMBER).FirstOrDefault();

        //        var constructionFinLoan = (from l in context.TBL_LOAN
        //                   where (l.CUSTOMERID == customerId && l.PRODUCTID == (int)FMBNProductEnum.ConstructionFinanceLoan && l.LOANSTATUSID != (int)LoanStatusEnum.Completed)
        //                   select l.LOANREFERENCENUMBER).FirstOrDefault();

        //        var hrl2 = (from l in context.TBL_LOAN_APPLICATION
        //                    join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
        //                    where (l.CUSTOMERID == customerId && b.PROPOSEDPRODUCTID == (int)FMBNProductEnum.HomeRenovationLoan && !disapprovedCancelledApplicationStatusId.Contains(l.APPLICATIONSTATUSID))
        //                    select l.APPLICATIONREFERENCENUMBER).FirstOrDefault();

        //        var rto2 = (from l in context.TBL_LOAN_APPLICATION
        //                       join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
        //                       where (l.CUSTOMERID == customerId && (b.PROPOSEDPRODUCTID == (int)FMBNProductEnum.RentToOwnLoan || b.PROPOSEDPRODUCTID == (int)FMBNProductEnum.RentToOwnLoanUnfunded) && !disapprovedCancelledApplicationStatusId.Contains(l.APPLICATIONSTATUSID))
        //                       select l.APPLICATIONREFERENCENUMBER).FirstOrDefault();

        //        var constructionFinLoan2 = (from l in context.TBL_LOAN_APPLICATION
        //                    join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
        //                    where (l.CUSTOMERID == customerId && b.PROPOSEDPRODUCTID == (int)FMBNProductEnum.ConstructionFinanceLoan && !disapprovedCancelledApplicationStatusId.Contains(l.APPLICATIONSTATUSID))
        //                    select l.APPLICATIONREFERENCENUMBER).FirstOrDefault();

        //        if (nhf != null)
        //        {
        //            throw new SecureException($"An NHF Mortgage Loan with Loan Ref. Number ({nhf}) has already been disbursed for this customer");
        //        }
        //        if (nhf2 != null)
        //        {
        //            throw new SecureException($"An NHF Mortgage Loan with Application Ref. Number ({nhf2}) is undergoing approval for this customer");
        //        }
        //        if (rto != null)
        //        {
        //            throw new SecureException($"An RTO Loan with Loan Ref. Number ({rto}) has already been disbursed for this customer and it's yet to be liquidated");
        //        }
        //        if (rto2 != null)
        //        {
        //            throw new SecureException($"An RTO Loan Application with Application Ref. Number ({rto2}) is undergoing approval for this customer");
        //        }
        //        if (hrl != null)
        //        {
        //            throw new SecureException($"An HRL Loan with Loan Ref. Number ({hrl}) has already been disbursed for this customer and it's yet to be liquidated");
        //        }
        //        if (hrl2 != null)
        //        {
        //            throw new SecureException($"An HRL Loan Application with Application Ref. Number ({hrl}) is undergoing approval for this customer");
        //        }
        //        if (constructionFinLoan != null)
        //        {
        //            throw new SecureException($"A Construction Finance Loan with Loan Ref. Number ({constructionFinLoan}) has already been disbursed for this customer and it's yet to be liquidated");
        //        }
        //        if (constructionFinLoan2 != null)
        //        {
        //            throw new SecureException($"A Construction Finance Loan Application with Application Ref. Number ({constructionFinLoan2}) is undergoing approval for this customer");
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        //private int hasBatchedCustomerAppliedForNHFLoan(int customerId, FinTrakBankingContext context)
        //{
        //    try
        //    {
        //        var nhfAccount = context.TBL_CASA.Where(c => c.CUSTOMERID == customerId).Select(c => c.PRODUCTACCOUNTNUMBER).FirstOrDefault();

        //        int[] disapprovedCancelledApplicationStatusId = {(int)LoanApplicationStatusEnum.ApplicationRejected,
        //                                                            (int)LoanApplicationStatusEnum.CancellationCompleted};

        //        var nhf = (from l in context.TBL_LOAN
        //                   where (l.CUSTOMERID == customerId && l.PRODUCTID == (int)FMBNProductEnum.NHFMortgageLoan && l.ISDISBURSED == 1)
        //                   select l.LOANREFERENCENUMBER).FirstOrDefault();

        //        var nhf2 = (from l in context.TBL_LOAN_APPLICATION
        //                    join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
        //                    where (l.CUSTOMERID == customerId && b.PROPOSEDPRODUCTID == (int)FMBNProductEnum.NHFMortgageLoan && !disapprovedCancelledApplicationStatusId.Contains(l.APPLICATIONSTATUSID))
        //                    select l.APPLICATIONREFERENCENUMBER).FirstOrDefault();

        //        var hrl = (from l in context.TBL_LOAN
        //                   where (l.CUSTOMERID == customerId && l.PRODUCTID == (int)FMBNProductEnum.HomeRenovationLoan && l.LOANSTATUSID != (int)LoanStatusEnum.Completed)
        //                   select l.LOANREFERENCENUMBER).FirstOrDefault();

        //        var rto = (from l in context.TBL_LOAN
        //                   where (l.CUSTOMERID == customerId && l.PRODUCTID == (int)FMBNProductEnum.HomeRenovationLoan && l.LOANSTATUSID != (int)LoanStatusEnum.Completed)
        //                   select l.LOANREFERENCENUMBER).FirstOrDefault();

        //        var constructionFinLoan = (from l in context.TBL_LOAN
        //                                   where (l.CUSTOMERID == customerId && l.PRODUCTID == (int)FMBNProductEnum.ConstructionFinanceLoan && l.LOANSTATUSID != (int)LoanStatusEnum.Completed)
        //                                   select l.LOANREFERENCENUMBER).FirstOrDefault();

        //        var hrl2 = (from l in context.TBL_LOAN_APPLICATION
        //                    join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
        //                    where (l.CUSTOMERID == customerId && b.PROPOSEDPRODUCTID == (int)FMBNProductEnum.HomeRenovationLoan && !disapprovedCancelledApplicationStatusId.Contains(l.APPLICATIONSTATUSID))
        //                    select l.APPLICATIONREFERENCENUMBER).FirstOrDefault();

        //        var rto2 = (from l in context.TBL_LOAN_APPLICATION
        //                    join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
        //                    where (l.CUSTOMERID == customerId && (b.PROPOSEDPRODUCTID == (int)FMBNProductEnum.RentToOwnLoan || b.PROPOSEDPRODUCTID == (int)FMBNProductEnum.RentToOwnLoanUnfunded) && !disapprovedCancelledApplicationStatusId.Contains(l.APPLICATIONSTATUSID))
        //                    select l.APPLICATIONREFERENCENUMBER).FirstOrDefault();

        //        var constructionFinLoan2 = (from l in context.TBL_LOAN_APPLICATION
        //                                    join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
        //                                    where (l.CUSTOMERID == customerId && b.PROPOSEDPRODUCTID == (int)FMBNProductEnum.ConstructionFinanceLoan && !disapprovedCancelledApplicationStatusId.Contains(l.APPLICATIONSTATUSID))
        //                                    select l.APPLICATIONREFERENCENUMBER).FirstOrDefault();

        //        if (nhf != null)
        //        {
        //            throw new SecureException($"An NHF Mortgage Loan has already been disbursed for the customer with NHF account '{nhfAccount}'");
        //        }
        //        if (nhf2 != null)
        //        {
        //            throw new SecureException($"An NHF Mortgage Loan is undergoing approval for the customer with NHF account '{nhfAccount}'");
        //        }
        //        if (rto != null)
        //        {
        //            throw new SecureException($"An RTO Loan has already been disbursed for the customer with NHF account '{nhfAccount}' and it's yet to be liquidated");
        //        }
        //        if (rto2 != null)
        //        {
        //            throw new SecureException($"An RTO Loan Application is undergoing approval for the customer with NHF account '{nhfAccount}'");
        //        }
        //        if (hrl != null)
        //        {
        //            throw new SecureException($"An HRL Loan has already been disbursed for the customer with NHF account '{nhfAccount}' and it's yet to be liquidated");
        //        }
        //        if (hrl2 != null)
        //        {
        //            throw new SecureException($"An HRL Loan Application is undergoing approval for the customer with NHF account '{nhfAccount}'");
        //        }
        //        if (constructionFinLoan != null)
        //        {
        //            throw new SecureException($"A Construction Finance Loan has already been disbursed for the customer with NHF account '{nhfAccount}' and it's yet to be liquidated");
        //        }
        //        if (constructionFinLoan2 != null)
        //        {
        //            throw new SecureException($"A Construction Finance Loan Application is undergoing approval for the customer with NHF account '{nhfAccount}'");
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public LoanApplicationForReturn AddLoanApplication(LoanApplicationForCreation loan)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        if (loan == null && loan.loanApplicationDetail == null)
                            throw new SecureException("The loan application and detail information cannot be null.");

                        // get the loan application detail
                        var loanDetail = loan.loanApplicationDetail;
                        if (loanDetail == null)
                            throw new SecureException("Kindly capture the loan application detail.");

                        // get and validate the selected product details
                        var productInfo = context.TBL_PRODUCT.Where(q => q.PRODUCTID == loanDetail.proposedProductId).FirstOrDefault();
                        if (productInfo == null)
                            throw new SecureException("Kindly select an existing product.");

                        if (loanDetail.proposedAmount > productInfo.MAXIMUMAMOUNT)
                        {
                            throw new SecureException($"Maximum product Amount Exceeded! The maximum product amount for {productInfo.PRODUCTNAME} is NGN{productInfo.MAXIMUMAMOUNT.Value.ToString("N")}");
                        }

                        if (loanDetail.proposedAmount < productInfo.MINIMUMAMOUNT)
                        {
                            throw new SecureException($"Minimum product Amount Not Met! The minimum product amount for {productInfo.PRODUCTNAME} is NGN{productInfo.MINIMUMAMOUNT.Value.ToString("N")}");
                        }

                        if (loanDetail.proposedTenor > productInfo.MAXIMUMTENOR)
                        {
                            throw new SecureException($"Maximum product Tenor Exceeded! The maximum product tenor for {productInfo.PRODUCTNAME} is NGN{productInfo.MAXIMUMTENOR} days");
                        }

                        if (loanDetail.proposedTenor < productInfo.MINIMUMTENOR)
                        {
                            throw new SecureException($"Minimum product Tenor Not Met! The minimum product tenor for {productInfo.PRODUCTNAME} is NGN{productInfo.MINIMUMTENOR} days");
                        }

                        //validate affordability
                        //if (loan.affordabilityDetails == null)
                        //{
                        //    throw new SecureException($"Kindly capture affordability details");
                        //}

                        //if (loan.affordabilityDetails.affordableAmount != (double)loanDetail.proposedAmount)
                        //{
                        //    throw new ConditionNotMetException($"Kindly process affordability amount as the proposed amount");
                        //}

                        //validate loan application source
                        if (loan.loanApplicationSourceId > 0)
                        {
                            var applicationSources = context.TBL_SOURCE_APPLICATION.Select(s => s.APPLICATIONID).ToList();
                            if (applicationSources.Count > 0)
                            {
                                if (!applicationSources.Contains((short)loan.loanApplicationSourceId))
                                {
                                    throw new ConditionNotMetException($"This loan application source is not valid");
                                }
                            }
                        }
                        else
                        {
                            throw new ConditionNotMetException($"Kindly enter a valid application source");
                        }

                        // get the customer based on the customer code
                        var customer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == loan.customerCode.Trim()).FirstOrDefault();
                        if (customer == null)
                            throw new SecureException($"There is no customer with code {loan.customerCode.Trim()}.");

                        // validate account
                        TBL_CASA account = new TBL_CASA();
                        var PmbSingleCustomerId = 0;
                        if (loan.loanApplicationSourceId == 3)
                        {
                            account = context.TBL_CASA.Where(q => q.PRODUCTACCOUNTNUMBER == loanDetail.operatingAccountNo).FirstOrDefault(); //PMB NHF account set as operating account
                            if (account == null)
                                throw new SecureException($"Kindly profile PMB's account");

                            //integration.GetCustomerAccreditationStatus(loanDetail.operatingAccountNo); //PMB accreditation check
                            //if (!acct.Any(a => a.PRODUCTACCOUNTNUMBER == loanDetail.operatingAccountNo))
                            //{
                            //    throw new SecureException($"Kindly ensure Operating Account is PMB's Account");
                            //}

                            //account = acct.Where(a => a.PRODUCTACCOUNTNUMBER == loanDetail.operatingAccountNo).FirstOrDefault();

                            //var PmbSinglecustomer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == loanDetail.customerCode.Trim()).FirstOrDefault();

                            if (customer != null)
                            {
                                PmbSingleCustomerId = customer.CUSTOMERID;

                                var customerCasa = context.TBL_CASA.Where(q => q.CUSTOMERID == PmbSingleCustomerId).FirstOrDefault();

                                //if (customerCasa != null && customerCasa.PMBNHFACCOUNT != null)
                                //{
                                //    if (customerCasa.PMBNHFACCOUNT != loanDetail.operatingAccountNo) //confirm loan source is from PMB that profiled the customer
                                //    {
                                //        throw new SecureException($"Customer was profiled and mapped to the PMB with NHF number {customerCasa.PMBNHFACCOUNT}");
                                //    }
                                //}                                
                                if (customerCasa == null)
                                {
                                    throw new SecureException($"Customer account is not yet synced.");
                                }

                                //validate approval and contribution from CBA
                                //var getCustomerAwaitingApproval = integration.GetCustomerAwaitingApproval(customerCasa.PRODUCTACCOUNTNUMBER);
                                //var customerNHFContributionCheck = integration.CustomerNHFContributionCheck(customerCasa.PRODUCTACCOUNTNUMBER);

                                //check if customer has a disbursed loan or an application is ongoing
                                //hasCustomerAppliedForNHFLoan(PmbSingleCustomerId, context);
                            }
                        }
                        else
                        {
                            account = context.TBL_CASA.Where(q => q.PRODUCTACCOUNTNUMBER == loanDetail.operatingAccountNo.Trim()).FirstOrDefault();
                            if (account == null)
                                throw new SecureException($"Customer account {loanDetail.operatingAccountNo.Trim()} is not yet synced, kindly contact your credit officer.");

                            //validate approval and contribution from CBA
                            //var getCustomerAwaitingApproval = integration.GetCustomerAwaitingApproval(account.PRODUCTACCOUNTNUMBER);
                            //var customerNHFContributionCheck = integration.CustomerNHFContributionCheck(account.PRODUCTACCOUNTNUMBER);

                            //check if customer has a disbursed loan or an application is ongoing
                            //hasCustomerAppliedForNHFLoan(customer.CUSTOMERID, context);
                        }

                        //if (customer.CUSTOMERTYPEID == (short)CustomerTypeEnum.Individual)
                        //    loan.loanTypeId = (short)CustomerTypeEnum.Individual;

                        //else
                        //    loan.loanTypeId = (short)CustomerTypeEnum.Corporate;

                        // get the RM mapped to the customer
                        var staff = context.TBL_STAFF.Where(b => b.STAFFID == customer.RELATIONSHIPOFFICERID && b.DELETED == false).FirstOrDefault();
                        if (staff == null)
                            throw new SecureException($"There is no credit officer mapped to the customer.");

                        // confirm the with the branch code
                        var branch = context.TBL_BRANCH.Where(b => b.BRANCHID == staff.BRANCHID).FirstOrDefault();
                        if (branch == null)
                            throw new SecureException($"There is no branch mapped to the credit officer of the customer.");


                        loan.customerId = customer.CUSTOMERID;
                        loan.branchId = branch.BRANCHID;
                        loan.relationshipOfficerId = staff.STAFFID;
                        loan.isPoliticallyExposed = customer.ISPOLITICALLYEXPOSED == true ? 1 : 0;
                        loan.isRelatedParty = 0;
                        loan.proposedTenor = loanDetail.proposedTenor;
                        // also get the product class id 
                        loan.productClassId = (short)productInfo.TBL_PRODUCT_CLASS.PRODUCTCLASSID;


                        //if (loanDetail.proposedAmount > productInfo.MAXIMUMTODAMOUNT)
                        //    throw new SecureException($"The requested amount '{loanDetail.proposedAmount}' cannot be more than '{productInfo.MAXIMUMTODAMOUNT}'.");
                        if (loanDetail.proposedAmount > productInfo.MAXIMUMAMOUNT)
                            throw new SecureException($"The requested amount '{loanDetail.proposedAmount}' cannot be more than '{productInfo.MAXIMUMAMOUNT}'.");
                        //if (loanDetail.proposedInterestRate != productInfo.MAXIMUMRATE)
                        //    throw new SecureException($"Kindly input the exact interest rate. Interest Rate for '{productInfo.PRODUCTNAME}' is '{productInfo.MAXIMUMRATE}'.");

                        // PROPERTIES TO VALIDATE BEFORE HAND
                        // CustomerCode to get customerId 
                        // Branch Code to get BranchId
                        // Also capture MSI CODE of the RM at CREDIT APPLICATIONS
                        // validate the RM of a particular customer
                        // endpoint to get customer accounts // to be able to set casaAccount
                        // endpoint to get subsectors
                        // calculate the exchange rate.

                        //loan.teamMisCode = loan.misCode;

                        // set a list of subsectorId
                        var sectorIds = new List<short>();
                        sectorIds.Add(loanDetail.subSectorId);

                        //ValidateLoanApplicationLimits((int)loan.branchId, (int)loan.customerId, loanDetail.proposedAmount, sectorIds);

                        // get the summed exchange rate.
                        var additionalAmount = loanDetail.exchangeAmount;

                        // validate the region
                        /*
                        if (loan.regionId == null || loan.regionId == 0)
                        {
                            var capRegionId = productInfo.TBL_PRODUCT_CLASS.CAPREGIONID;

                            if (capRegionId == null || capRegionId == 0)
                                throw new SecureException("Kinldy contact an admin to setup up default cap region for this product class.");
                            else
                                loan.regionId = capRegionId;
                        }
                        */

                        //if (loan.productClassId == (int)ProductClassEnum.FirstTrader)
                        //    if (loanDetail.traderLoan == null)
                        //        throw new SecureException("Kindly Ensure You Capture Traders Information");


                        //var savedDetails = context.TBL_LOAN_APPLICATION_DETAIL.Where(c => c.LOANAPPLICATIONID == loan.loanApplicationId && c.DELETED == false);
                        //decimal cumulativeSum = 0;
                        //foreach (var s in savedDetails) { cumulativeSum = cumulativeSum + (s.PROPOSEDAMOUNT * (decimal)s.EXCHANGERATE); }

                        /*
                        if (loan.misCode != null)
                        {
                            using (FintrakStagingModel stagging = new FintrakStagingModel())
                            {
                                if (!stagging.STG_MIS_INFO.Where(x => x.FIELD1 == loan.misCode).Any())
                                {
                                    throw new SecureException("The MIS Code was not found.");
                                }
                            }
                        }
                        */

                        if (loan.relationshipOfficerId != 0)
                        {
                            var validation = ValidateCreditLimitByRMBM(loan.relationshipOfficerId);
                            if (validation.maximumAllowedLimit > 0)
                                if (additionalAmount > (decimal)validation.limit)
                                    throw new SecureException($"RM Limit Exceeded. The limit of this RM is {validation.limit}");
                        }

                        loan.applicationAmount = additionalAmount;
                        var totalExposureAmount = loan.applicationAmount + loanApplicationRepository.GetCustomerTotalOutstandingBalance(loan.customerId);


                        int changes = 0;

                        //if (loanData != null)
                        //{
                        //    loanData.APPLICATIONAMOUNT = loan.applicationAmount;
                        //    loanData.TOTALEXPOSUREAMOUNT = totalExposureAmount;
                        //}

                        if (string.IsNullOrEmpty(loan.applicationReferenceNumber))
                            loan.applicationReferenceNumber = loanApplicationRepository.GetRefrenceNumber();

                        try
                        {
                            // add loan application information
                            AddloanApplicationSub(loan, totalExposureAmount, context);

                            // add the loan application detail information
                            var applicationDetailId = 0;

                            if (loan.loanApplicationSourceId == 3)
                            {
                                applicationDetailId = AddLoanApplicationDetail(loanDetail, PmbSingleCustomerId, account.CASAACCOUNTID, loan.createdBy, context, ref changes);
                                loan.customerId = PmbSingleCustomerId;
                            }
                            else
                            {
                                applicationDetailId = AddLoanApplicationDetail(loanDetail, loan.customerId, account.CASAACCOUNTID, loan.createdBy, context, ref changes);
                            }

                            //AddLoanAffordabilityDetails(loan, context, applicationDetailId);

                            var output = context.SaveChanges() > 0;
                            trans.Commit();
                            trans.Dispose();




                            var result = new LoanApplicationForReturn();
                            Task.Run(async () => result = await GetLoanApplicationByRefNo(loan.applicationReferenceNumber, context)).GetAwaiter().GetResult();


                            return result;
                        }
                        catch (DbEntityValidationException ex)
                        {
                            trans.Rollback();

                            string errorMessages = string.Join("; ",
                            ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                            throw new DbEntityValidationException(errorMessages);
                        }

                        //int response = changes + context.SaveChanges();

                        //if (savedDetails.Count() == 0 || loan.isNewApplication)
                        //{
                        //    if (loanData != null)
                        //    {
                        //        if (loan.loanApplicationId == 0)
                        //        {
                        //            loan.loanApplicationId = loanData.LOANAPPLICATIONID;
                        //        }
                        //        UpdateLoanApplication(loan, context);
                        //        context.SaveChanges();
                        //    }
                        //}

                        //var applicationDetails = GetLoanApplicationByLoanRefrenceNo(loanData.APPLICATIONREFERENCENUMBER, loanData.COMPANYID);

                        //var productClass = applicationDetails.LoanApplicationDetail.Select(o => o.productClassId).Distinct().ToList();
                        //if (productClass.Count() > 1)
                        //{
                        //    var loanApplication = context.TBL_LOAN_APPLICATION.Where(o => o.APPLICATIONREFERENCENUMBER == loanData.APPLICATIONREFERENCENUMBER).FirstOrDefault();
                        //    loanApplication.PRODUCTCLASSID = null;
                        //    if (loanApplication.APPLICATIONTENOR == 0)
                        //    {
                        //        var loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == loanApplication.LOANAPPLICATIONID).ToList();
                        //        loanApplication.APPLICATIONTENOR = loanApplicationDetail.Max(o => o.PROPOSEDTENOR);


                        //    }
                        //    context.SaveChanges();
                        //}


                        //if (response > 0 && !loan.isNewApplication) applicationDetails.closeApplication = true; 
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }
            }
        }

        public LoanApplicationForReturn AddBatchedLoanApplication(LoanApplicationForCreation loan)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        var result = new LoanApplicationForReturn();

                        if (loan == null && loan.loanApplicationDetails.Count < 1)
                            throw new SecureException("The loan application and detail information cannot be null.");

                        // get the loan application detail
                        var loanDetail = loan.loanApplicationDetails;
                        if (loanDetail.Count < 1)
                            throw new SecureException("Kindly capture the loan application detail.");

                        //validate loan application source
                        if (loan.loanApplicationSourceId > 0)
                        {
                            var applicationSources = context.TBL_SOURCE_APPLICATION.Select(s => s.APPLICATIONID).ToList();
                            if (applicationSources.Count > 0)
                            {
                                if (!applicationSources.Contains((short)loan.loanApplicationSourceId))
                                {
                                    throw new ConditionNotMetException($"This loan application source is not valid");
                                }
                            }
                        }
                        else
                        {
                            throw new ConditionNotMetException($"Kindly process loan application from a valid source");
                        }

                        // get the PMB customer based on the customer code
                        var customer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == loan.customerCode.Trim()).FirstOrDefault();
                        if (customer == null)
                            throw new SecureException($"There is no PMB customer with code {loan.customerCode.Trim()}.");

                        // validate account
                        var account = context.TBL_CASA.Where(q => q.CUSTOMERID == customer.CUSTOMERID).FirstOrDefault(); //PMB NHF account set as operating account

                        if (account == null)
                            throw new SecureException($"Kindly profile PMB's customer and account details");

                        //integration.GetCustomerAccreditationStatus(account.PRODUCTACCOUNTNUMBER); //PMB accreditation check

                        bool notSamePmb = loanDetail.Any(p => p.operatingAccountNo != account.PRODUCTACCOUNTNUMBER);

                        if (notSamePmb)
                        {
                            throw new SecureException($"Kindly ensure all loan application are from this same PMB");
                        }

                        if (loan.loanApplicationSourceId == 3)
                        {
                            //var operatingCasa = loanDetail.FirstOrDefault().operatingAccountNo;
                            var proposedProduct = loanDetail.FirstOrDefault().proposedProductId;

                            bool notSameProduct = loanDetail.Any(p => p.proposedProductId != proposedProduct);

                            if (notSameProduct)
                            {
                                throw new SecureException($"Kindly ensure proposed product for all customers are same");
                            }

                            var customers = loanDetail.ToList();

                            foreach (var c in customers)
                            {
                                if (customers.Count(ct => ct.customerCode == c.customerCode) > 1)
                                {
                                    throw new SecureException($"Multiple application found for customer code {c.customerCode}");
                                }
                            }

                            if (customer != null)
                            {
                                // get the RM mapped to the customer
                                var staff = context.TBL_STAFF.Where(b => b.STAFFID == customer.RELATIONSHIPOFFICERID && b.DELETED == false).FirstOrDefault();
                                if (staff == null)
                                    throw new SecureException($"There is no credit officer mapped to the customer.");

                                // confirm the with the branch code
                                var branch = context.TBL_BRANCH.Where(b => b.BRANCHID == staff.BRANCHID).FirstOrDefault();
                                if (branch == null)
                                    throw new SecureException($"There is no branch mapped to the credit officer of the customer.");

                                loan.customerId = customer.CUSTOMERID;
                                loan.branchId = branch.BRANCHID;
                                loan.relationshipOfficerId = staff.STAFFID;
                                loan.isPoliticallyExposed = customer.ISPOLITICALLYEXPOSED == true ? 1 : 0;
                                loan.isRelatedParty = 0;
                                loan.proposedTenor = loanDetail.Max(m => m.proposedTenor); //since it's batched loan from different customers

                                // set a list of subsectorId
                                var sectorIds = new List<short>();
                                loanDetail.ForEach(l => { sectorIds.Add(l.subSectorId); });

                                // get the summed exchange rate.
                                var additionalAmount = loanDetail.Sum(s => s.exchangeAmount);
                                var sumProposedAmount = loanDetail.Sum(s => s.proposedAmount);

                                loan.applicationAmount = additionalAmount;
                                var totalExposureAmount = loan.applicationAmount + loanApplicationRepository.GetCustomerTotalOutstandingBalance((int)loan.customerId);

                                ValidateLoanApplicationLimits((int)loan.branchId, (int)loan.customerId, sumProposedAmount, sectorIds);



                                if (loan.relationshipOfficerId != 0)
                                {
                                    var validation = ValidateCreditLimitByRMBM(loan.relationshipOfficerId);
                                    if (validation.maximumAllowedLimit > 0)
                                        if (additionalAmount > (decimal)validation.limit)
                                            throw new SecureException($"Credit Officer Limit Exceeded. The limit of this Credit Officer is {validation.limit}");
                                }


                                // also get the product class id 
                                var product = context.TBL_PRODUCT.Where(q => q.PRODUCTID == proposedProduct).FirstOrDefault();

                                loan.productClassId = (short)product.TBL_PRODUCT_CLASS.PRODUCTCLASSID;

                                int changes = 0;
                                if (string.IsNullOrEmpty(loan.applicationReferenceNumber))
                                    loan.applicationReferenceNumber = loanApplicationRepository.GetRefrenceNumber();

                                // add loan application information
                                AddBatchedloanApplicationSub(loan, totalExposureAmount, context);

                                foreach (var cust in loanDetail)
                                {
                                    if (cust.affordabilityDetails == null)
                                    {
                                        throw new SecureException($"Kindly capture affordability details for customer code '{cust.customerCode}'");
                                    }

                                    if (cust.affordabilityDetails.affordableAmount != (double)cust.proposedAmount)
                                    {
                                        throw new ConditionNotMetException($"Kindly process affordability amount as the proposed amount for customer code '{cust.customerCode}'");
                                    }
                                    var singleCustomer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == cust.customerCode.Trim()).FirstOrDefault();

                                    if (singleCustomer == null)
                                        throw new SecureException($"There is no customer code {cust.customerCode.Trim()}.");

                                    var customerCasa = context.TBL_CASA.Where(q => q.CUSTOMERID == singleCustomer.CUSTOMERID).FirstOrDefault();

                                    if (customerCasa == null)
                                        throw new SecureException($"Kindly profile customer code {cust.customerCode} and account details.");

                                    //if (customerCasa != null && customerCasa.PMBNHFACCOUNT != null)
                                    //{
                                    //    if (customerCasa.PMBNHFACCOUNT != cust.operatingAccountNo) //confirm loan source is from PMB that profiled the customer
                                    //    {
                                    //        throw new SecureException($"Customer code {cust.customerCode} was profiled and mapped to the PMB with NHF number {customerCasa.PMBNHFACCOUNT}");
                                    //    }
                                    //}

                                    //validate approval and contribution from CBA
                                    //var getCustomerAwaitingApproval = integration.GetCustomerAwaitingApproval(customerCasa.PRODUCTACCOUNTNUMBER);
                                    //var customerNHFContributionCheck = integration.CustomerNHFContributionCheck(customerCasa.PRODUCTACCOUNTNUMBER);

                                    //check if customer has a disbursed loan or an application is ongoing
                                    //hasBatchedCustomerAppliedForNHFLoan(singleCustomer.CUSTOMERID, context);


                                    // get the selected product details
                                    var productInfo = context.TBL_PRODUCT.Where(q => q.PRODUCTID == cust.proposedProductId).FirstOrDefault();
                                    if (productInfo == null)
                                        throw new SecureException($"Kindly select an existing product for customer {cust.customerCode}");

                                    if (cust.proposedAmount > productInfo.MAXIMUMAMOUNT)
                                        throw new SecureException($"The requested amount '{cust.proposedAmount}' for customer {cust.customerCode} cannot be more than max product amount '{productInfo.MAXIMUMAMOUNT}'.");

                                    try
                                    {
                                        // add the loan application detail information
                                        var applicationDetailId = AddBatchedLoanApplicationDetail(cust, singleCustomer.CUSTOMERID, account.CASAACCOUNTID, loan.createdBy, context, ref changes);

                                        var loan_afford = cust.affordabilityDetails;

                                        loan_afford.customerId = singleCustomer.CUSTOMERID;
                                        loan_afford.createdBy = loan.relationshipOfficerId;

                                        //AddBatchedLoanAffordabilityDetails(loan_afford, context, applicationDetailId);

                                        var output = context.SaveChanges() > 0;
                                    }
                                    catch (DbEntityValidationException ex)
                                    {
                                        trans.Rollback();

                                        string errorMessages = string.Join("; ",
                                        ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                                        throw new DbEntityValidationException(errorMessages);
                                    }
                                }
                            }

                            Task.Run(async () => result = await GetBatchedLoanApplicationByRefNo(loan.applicationReferenceNumber, context)).GetAwaiter().GetResult();
                        }
                        else  //TO BE IMPLEMENTED FOR MULTIPLE LOANS FROM OTHER SOURCES
                        {
                            throw new SecureException($"Multiple loan application not yet applicable from other sources");
                        }

                        trans.Commit();
                        trans.Dispose();

                        return result;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }
            }
        }

        /*
        private static void AddLoanAffordabilityDetails(LoanApplicationForCreation loan, FinTrakBankingContext context, int applicationDetailId)
        {
            var affordabilityDetails = new TBL_LOAN_AFFORDABILITY();


            affordabilityDetails.CUSTOMERID = loan.customerId;
            affordabilityDetails.PRODUCTID = loan.affordabilityDetails.productId;
            affordabilityDetails.CASAACCOUNTID = loan.affordabilityDetails.casaAccountId;
            affordabilityDetails.LOANAPPLICATIONDETAILID = applicationDetailId;
            affordabilityDetails.AGE = loan.affordabilityDetails.age;
            affordabilityDetails.YEARSINSERVICE = loan.affordabilityDetails.yearsInService;
            affordabilityDetails.REPAYMENTPERIOD = loan.affordabilityDetails.repaymentPeriod;
            affordabilityDetails.PRESENTVALUE = loan.affordabilityDetails.presentValue;
            affordabilityDetails.AFFORDABLEAMOUNT = loan.affordabilityDetails.affordableAmount;
            affordabilityDetails.MONTHLYREPAYMENT = loan.affordabilityDetails.monthlyRepayment;
            affordabilityDetails.PROFITABILITY = loan.affordabilityDetails.profitability;
            affordabilityDetails.RATE = loan.affordabilityDetails.rate;
            affordabilityDetails.AMOUNTREQUESTED = loan.affordabilityDetails.amountRequested;
            affordabilityDetails.TENOROVERRIDE = loan.affordabilityDetails.tenorOverride;
            affordabilityDetails.CREATEDBY = loan.createdBy;
            affordabilityDetails.DATETIMECREATED = DateTime.Now;


            context.TBL_LOAN_AFFORDABILITY.Add(affordabilityDetails);
        }

        private static void AddBatchedLoanAffordabilityDetails(AffordabilityViewModel loan, FinTrakBankingContext context, int applicationDetailId)
        {
            var affordabilityDetails = new TBL_LOAN_AFFORDABILITY();


            affordabilityDetails.CUSTOMERID = loan.customerId;
            affordabilityDetails.PRODUCTID = loan.productId;
            affordabilityDetails.CASAACCOUNTID = loan.casaAccountId;
            affordabilityDetails.LOANAPPLICATIONDETAILID = applicationDetailId;
            affordabilityDetails.AGE = loan.age;
            affordabilityDetails.YEARSINSERVICE = loan.yearsInService;
            affordabilityDetails.REPAYMENTPERIOD = loan.repaymentPeriod;
            affordabilityDetails.PRESENTVALUE = loan.presentValue;
            affordabilityDetails.AFFORDABLEAMOUNT = loan.affordableAmount;
            affordabilityDetails.MONTHLYREPAYMENT = loan.monthlyRepayment;
            affordabilityDetails.PROFITABILITY = loan.profitability;
            affordabilityDetails.RATE = loan.rate;
            affordabilityDetails.AMOUNTREQUESTED = loan.amountRequested;
            affordabilityDetails.TENOROVERRIDE = loan.tenorOverride;
            affordabilityDetails.CREATEDBY = loan.createdBy;
            affordabilityDetails.DATETIMECREATED = DateTime.Now;


            context.TBL_LOAN_AFFORDABILITY.Add(affordabilityDetails);
        }
        */
        private void AddloanApplicationSub(LoanApplicationForCreation loan, decimal totalExposureAmount, FinTrakBankingContext context)
        {
            //short productClassProcessId = 0;
            //short? productClassId = null;
            //isGroupLoan = false; 
            //int loanId = 0; 

            //if (loan.loanTypeId == (int)LoanTypeEnum.CustomerGroup)
            //{
            //    isGroupLoan = true;
            //}
            //int? casaAccountId = null;
            //string refNumber = GenerateLoanReference(loan.customerId.Value);
            //if (loan.customerAccount != "N/A")
            //{
            //    casaAccountId = casa.GetCasaAccountId(loan.customerAccount, loan.companyId);
            //}


            short productClassId = loan.productClassId;
            //short productClassProcessId = (short)ProductClassProcessEnum.CAMBased;
            short productClassProcessId = context.TBL_PRODUCT_CLASS.Find(loan.productClassId).PRODUCT_CLASS_PROCESSID;

            string loanInformation = "New loan application";
            string tempMisInfo = "temp";

            loanData = new TBL_LOAN_APPLICATION
            {
                INTERESTRATE = 0, // COME IN AS ADDITIONAL PARAMETTER
                //CASAACCOUNTID = loan.casaAccountId, // COME IN AS ADDITIONAL PARAMETTER
                //ISINVESTMENTGRADE = loan.isInvestmentGrade, // COME IN AS ADDITIONAL PARAMETTER
                //BUSINESSUNIT = loan.businessUnit, // COME IN AS ADDITIONAL PARAMETTER
                MISCODE = tempMisInfo, // COME IN AS ADDITIONAL PARAMETTER
                TEAMMISCODE = tempMisInfo, //string.Empty, // COME IN AS ADDITIONAL PARAMETTER

                LOANAPPLICATIONTYPEID = loan.loanTypeId,
                REQUIRECOLLATERAL = loan.requireCollateral == 1 ? true : false,
                TOTALEXPOSUREAMOUNT = totalExposureAmount, // totalAmount,
                PRODUCTCLASSID = productClassId,
                APPLICATIONREFERENCENUMBER = loan.applicationReferenceNumber,
                PRODUCT_CLASS_PROCESSID = productClassProcessId,
                COMPANYID = loan.companyId,
                BRANCHID = (short)loan.branchId,
                RELATIONSHIPOFFICERID = loan.createdBy,
                RELATIONSHIPMANAGERID = loan.createdBy,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                //LOANINFORMATION = loan.loanInformation,
                ISRELATEDPARTY = loan.isRelatedParty == 1 ? true : false,
                ISPOLITICALLYEXPOSED = loan.isPoliticallyExposed == 1 ? true : false,
                CREATEDBY = (int)loan.createdBy,
                DATETIMECREATED = DateTime.Now,
                SYSTEMDATETIME = DateTime.Now,
                CUSTOMERGROUPID = loan.customerId,
                APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationInProgress,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                APPLICATIONAMOUNT = loan.applicationAmount,
                APPLICATIONTENOR = loan.proposedTenor,
                CAPREGIONID = loan.regionId,
                //REQUIRECOLLATERALTYPEID = loan.requireCollateralTypeId,
                //LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId,
                CUSTOMERID = loan.customerId,
                SUBMITTEDFORAPPRAISAL = false,
                OPERATIONID = (int)OperationsEnum.CreditAppraisal,
                //COLLATERALDETAIL = loan.collateralDetail, 
                ISFROMEXTERNALSOURCE = true,
                LOANINFORMATION = loanInformation,
                OWNEDBY = loan.createdBy,
                //LOANAPPLICATIONSOURCEID = loan.loanApplicationSourceId
            };

            //if (isGroupLoan)
            //{
            //    loanData.CUSTOMERGROUPID = loan.customerId;
            //    loanData.CUSTOMERID = null;
            //}
            //else
            //{
            loanData.CUSTOMERID = loan.customerId;
            loanData.CUSTOMERGROUPID = null;
            //}


            context.TBL_LOAN_APPLICATION.Add(loanData);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
                STAFFID = loan.createdBy,
                BRANCHID = (short)loan.branchId,
                DETAIL = $"Applied for loan with reference number: {loan.applicationReferenceNumber}",
                IPADDRESS = string.Empty,
                URL = string.Empty,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = loanData.LOANAPPLICATIONID
            };

            this.auditTrail.AddAuditTrail(audit);
        }

        private void AddBatchedloanApplicationSub(LoanApplicationForCreation loan, decimal totalExposureAmount, FinTrakBankingContext context)
        {
            short productClassId = loan.productClassId;
            short productClassProcessId = (short)ProductClassProcessEnum.CAMBased;

            string loanInformation = "New loan application";
            string tempMisInfo = "temp";

            loanData = new TBL_LOAN_APPLICATION
            {
                INTERESTRATE = 0, // COME IN AS ADDITIONAL PARAMETTER
                //CASAACCOUNTID = loan.casaAccountId, // COME IN AS ADDITIONAL PARAMETTER
                //ISINVESTMENTGRADE = loan.isInvestmentGrade, // COME IN AS ADDITIONAL PARAMETTER
                //BUSINESSUNIT = loan.businessUnit, // COME IN AS ADDITIONAL PARAMETTER
                MISCODE = tempMisInfo, // COME IN AS ADDITIONAL PARAMETTER
                TEAMMISCODE = tempMisInfo, //string.Empty, // COME IN AS ADDITIONAL PARAMETTER

                LOANAPPLICATIONTYPEID = loan.loanTypeId,
                REQUIRECOLLATERAL = loan.requireCollateral == 1 ? true : false,
                TOTALEXPOSUREAMOUNT = totalExposureAmount, // totalAmount,
                PRODUCTCLASSID = productClassId,
                APPLICATIONREFERENCENUMBER = loan.applicationReferenceNumber,
                PRODUCT_CLASS_PROCESSID = productClassProcessId,
                COMPANYID = loan.companyId,
                BRANCHID = (short)loan.branchId,
                RELATIONSHIPOFFICERID = loan.createdBy,
                RELATIONSHIPMANAGERID = loan.createdBy,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                //LOANINFORMATION = loan.loanInformation,
                ISRELATEDPARTY = loan.isRelatedParty == 1 ? true : false,
                ISPOLITICALLYEXPOSED = loan.isPoliticallyExposed == 1 ? true : false,
                CREATEDBY = (int)loan.createdBy,
                DATETIMECREATED = DateTime.Now,
                SYSTEMDATETIME = DateTime.Now,
                CUSTOMERGROUPID = loan.customerId,
                APPLICATIONSTATUSID = (short)LoanApplicationStatusEnum.ApplicationInProgress,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                APPLICATIONAMOUNT = loan.applicationAmount,
                APPLICATIONTENOR = loan.proposedTenor,
                CAPREGIONID = loan.regionId,
                //REQUIRECOLLATERALTYPEID = loan.requireCollateralTypeId,
                //LOANPRELIMINARYEVALUATIONID = loan.loanPreliminaryEvaluationId,
                CUSTOMERID = loan.customerId,
                SUBMITTEDFORAPPRAISAL = false,
                OPERATIONID = (int)OperationsEnum.CreditAppraisal,
                //COLLATERALDETAIL = loan.collateralDetail, 
                ISFROMEXTERNALSOURCE = true,
                LOANINFORMATION = loanInformation,
                //LOANAPPLICATIONSOURCEID = loan.loanApplicationSourceId
            };

            loanData.CUSTOMERID = loan.customerId;
            loanData.CUSTOMERGROUPID = null;

            context.TBL_LOAN_APPLICATION.Add(loanData);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanApplication,
                STAFFID = loan.createdBy,
                BRANCHID = (short)loan.branchId,
                DETAIL = $"Applied for loan with reference number: {loan.applicationReferenceNumber}",
                IPADDRESS = string.Empty,
                URL = string.Empty,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = loanData.LOANAPPLICATIONID
            };

            this.auditTrail.AddAuditTrail(audit);
        }

        private int AddLoanApplicationDetail(LoanApplicationDetailForCreation entity, int customerId, int accountId, int createdBy, FinTrakBankingContext context, ref int savedChanges)
        {
            if (entity.proposedTenor == 0)
            {
                throw new SecureException("Tenor can not be ZERO (0)");
            }

            var productTenor = context.TBL_PRODUCT.Find(entity.proposedProductId);
            if (entity.proposedTenor > productTenor.MAXIMUMTENOR)
            {
                throw new SecureException($"Maximum tenor exceeded! The maximum tenor for {productTenor.PRODUCTNAME} is {productTenor.MAXIMUMTENOR} years");
            }


            int applicationId = this.loanData == null ? 0 : this.loanData.LOANAPPLICATIONID; // ?
            //int tenor = loanApplicationRepository.ConvertTenorToDays(entity.proposedTenor, entity.tenorModeId);
            var product = context.TBL_PRODUCT.Where(f => f.PRODUCTID == entity.proposedProductId).FirstOrDefault();

            var productType = product.PRODUCTTYPEID;


            //if (entity.invoiceDetail != null && product.PRODUCTCLASSID == (int)ProductClassEnum.InvoiceDiscountingFacility)
            //{
            //    var percentage = context.TBL_PRODUCT_BEHAVIOUR.Where(b => b.PRODUCTID == entity.proposedProductId).Select(b => b.PRODUCT_LIMIT).FirstOrDefault();
            //    if (percentage != null && entity.proposedAmount > (entity.invoiceDetail.invoiceAmount * (decimal)percentage.Value / 100))
            //        throw new SecureException($"Facility amount exceeds {percentage.Value}% of the invoice amount");
            //}

            //List<ProductFeesViewModel> fees = new List<ProductFeesViewModel>();
            //ProductFeesViewModel fee = new ProductFeesViewModel();                 
            //if (a.productPriceIndexId <= 0)
            //{
            //    a.productPriceIndexId = null;
            //}

            var newLoanDetail = new TBL_LOAN_APPLICATION_DETAIL
            {
                APPROVEDINTERESTRATE = (double)product.MAXIMUMRATE.Value, //(double)entity.proposedInterestRate, // COME IN AS ADDITIONAL PARAMETTER
                                                                          //APR = entity.apr // COME IN AS ADDITIONAL PARAMETTER
                                                                          //REPAYMENTDATE = entity.repaymentDate, // COME IN AS ADDITIONAL PARAMETTER
                                                                          //EQUITYAMOUNT = entity.equityAmount, // COME IN AS ADDITIONAL PARAMETTER
                                                                          //EQUITYCASAACCOUNTID = entity.equityCasaAccountId, // COME IN AS ADDITIONAL PARAMETTER

                CURRENCYID = entity.currencyId,
                APPROVEDAMOUNT = entity.proposedAmount,
                APPROVEDPRODUCTID = (short)entity.proposedProductId,
                APPROVEDTENOR = entity.proposedTenor,
                //PROPOSEDPRODUCTDETAILID = entity.proposedProductDetailId,
                EXCHANGERATE = entity.exchangeRate,
                CUSTOMERID = customerId,
                LOANAPPLICATIONID = applicationId,
                STATUSID = (short)LoanApplicationDetailsStatusEnum.Pending,
                //REPAYMENTSCHEDULE = productType == (int)LoanProductTypeEnum.RevolvingLoan ? "N/A" : "To be advised after loan setup.",

                PROPOSEDAMOUNT = entity.proposedAmount,
                PROPOSEDINTERESTRATE = (double)product.MAXIMUMRATE.Value,
                PROPOSEDPRODUCTID = (short)entity.proposedProductId,
                //PROPOSEDPRODUCTDETAILID = (short)entity.proposedProductId,
                PROPOSEDTENOR = entity.proposedTenor,
                DELETED = false,
                SUBSECTORID = entity.subSectorId,
                CREATEDBY = createdBy,
                DATETIMECREATED = DateTime.Now,
                LOANPURPOSE = entity.loanPurpose,
                //CASAACCOUNTID = entity.casaAccountId,
                OPERATINGCASAACCOUNTID = accountId,
                REPAYMENTTERMS = string.Empty,

                //CRMSFUNDINGSOURCEID = entity.crmsFundingSourceId,
                // CRMSREPAYMENTSOURCEID = entity.crmsPaymentSourceId,
                //CRMSFUNDINGSOURCECATEGORY = entity.crmsFundingSourceCategory, 
                //CRMS_ECCI_NUMBER = entity.crms_ECCI_Number,
                //FIELD1 = entity.fieldOne,
                //FIELD2 = entity.fieldTwo,
                //FIELD3 = entity.fieldThree,
                //PRODUCTPRICEINDEXID = entity.productPriceIndexId,
                //PRODUCTPRICEINDEXRATE = entity.productPriceIndexRate,

                TENORFREQUENCYTYPEID = entity.tenorModeId,
                //PROPOSEDTENORFREQUENCYTYPEID = entity.tenorModeId,
                CRMSVALIDATED = false,
                //NEXTREVIEWDATE = null,
                //ESGMSREQUIRED = false,
                //ISUNADVICED = entity.isUnAdviced,
                //LOANDETAILTYPEID = (int)LoanApplicationDetailTypeEnum.NEW,
                TAKEFEETYPEID = (int)TakeFeeTypeEnum.ApprovedAmount,
                ISLINEFACILITY = entity.isLineFacility,
                PROPERTYTYPEID = entity.propertyTypeId,
                PROPERTYTITLE = entity.propertyTitle,
                PROPERTYPRICE = entity.propertyPrice,
                DOWNPAYMENT = entity.downPayment,
                //REQUESTEDAMOUNT = entity.requestedAmount,
                //REPAYMENTDATE = entity.repaymentDate,
                //CREDITSCORE = entity.creditScore,
                //CREDITRATING = entity.creditRating,
                //OPERATION_TYPE = entity.operationType != null ? entity.operationType : 0,

                //TEMPPRINCIPALNAME = entity.principalName,
                //TEMPPRINCIPALID = entity.principalId, 
            };

            var applicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Add(newLoanDetail);

            // save changed to get last saved ID
            int changes = context.SaveChanges();

            //if (entity.invoiceDetail == null && product.PRODUCTCLASSID == (short)ProductClassEnum.InvoiceDiscountingFacility)
            //    throw new SecureException("Kinldy capture the loan invoice details for an IDF (Invoice Discounting Facility) product.");

            //else if (entity.invoiceDetail != null && product.PRODUCTCLASSID == (short)ProductClassEnum.InvoiceDiscountingFacility)
            //{
            //    var exists = context.TBL_LOAN_APPLICATION_DETL_INV.Any(i => i.PRINCIPALID == entity.invoiceDetail.principalId && i.INVOICENO.ToLower() == entity.invoiceDetail.invoiceNo.ToLower());
            //    if (exists == true)
            //        throw new SecureException("Invoice Number for selected principal exists already");

            //    var invoicesList = new List<InvoiceDetailForCreation>();
            //    invoicesList.Add(entity.invoiceDetail);

            //    InvoiceDetails(invoicesList, createdBy, applicationId, newLoanDetail.LOANAPPLICATIONDETAILID, customerId, context);
            //}


            //if (entity.traderLoan == null && product.PRODUCTCLASSID == (short)ProductClassEnum.FirstTrader)
            //    throw new SecureException("Kinldy capture the loan trader information for First Trader product.");

            //else if (entity.traderLoan != null && product.PRODUCTCLASSID == (short)ProductClassEnum.FirstTrader)
            //{
            //    TraderLoan(entity.traderLoan, newLoanDetail.LOANAPPLICATIONDETAILID, createdBy, context);
            //}

            savedChanges = changes;

            //if (a.syndicatedLoan != null && a.syndicatedLoan.Count > 0)
            //{
            //    SyndicatedDetails(a.syndicatedLoan, a.loanApplicationDetailId, createdBy, context);
            //}

            //if (a.productFees != null)
            //{
            //    if (a.productFees.Count > 0)
            //    {
            //        ProductFees(a.productFees, a.loanApplicationDetailId, createdBy, context);
            //    }
            //}
            //else
            //{
            //    throw new SecureException("No fee is defined for this product(s)");
            //}


            //int changes = context.SaveChanges();
            //int changes2 = 0;

            //if (a.principalIds != null && a.principalIds.Count() > 0)
            //{
            //    foreach (var principalId in a.principalIds)
            //    {
            //        var loanDetailPrincipal = new TBL_LOAN_DETAIL_PRINCIPAL
            //        {
            //            LOANAPPLICATIONDETAILID = newLoanDetail.LOANAPPLICATIONDETAILID,
            //            PRINCIPALID = principalId
            //        };
            //        context.TBL_LOAN_DETAIL_PRINCIPAL.Add(loanDetailPrincipal);
            //    }
            //    changes2 = context.SaveChanges();
            //}


            //savedChanges = changes2 + changes;
            return applicationDetail.LOANAPPLICATIONDETAILID;
        }

        private int AddBatchedLoanApplicationDetail(LoanApplicationDetailForCreation entity, int customerId, int accountId, int createdBy, FinTrakBankingContext context, ref int savedChanges)
        {
            if (entity.proposedTenor == 0)
            {
                throw new SecureException("Tenor can not be ZERO (0)");
            }

            var productTenor = context.TBL_PRODUCT.Find(entity.proposedProductId);
            if (entity.proposedTenor > productTenor.MAXIMUMTENOR)
            {
                throw new SecureException($"Maximum tenor exceeded! The maximum tenor for {productTenor.PRODUCTNAME} is {productTenor.MAXIMUMTENOR} years");
            }

            int applicationId = this.loanData == null ? 0 : this.loanData.LOANAPPLICATIONID; // ?
            //int tenor = loanApplicationRepository.ConvertTenorToDays(entity.proposedTenor, entity.tenorModeId);
            var product = context.TBL_PRODUCT.Where(f => f.PRODUCTID == entity.proposedProductId).FirstOrDefault();

            var productType = product.PRODUCTTYPEID;

            var newLoanDetail = new TBL_LOAN_APPLICATION_DETAIL
            {
                APPROVEDINTERESTRATE = (double)product.MAXIMUMRATE.Value,
                CURRENCYID = entity.currencyId,
                APPROVEDAMOUNT = entity.proposedAmount,
                APPROVEDPRODUCTID = (short)entity.proposedProductId,
                APPROVEDTENOR = entity.proposedTenor,
                //PROPOSEDPRODUCTDETAILID = entity.proposedProductDetailId,
                EXCHANGERATE = entity.exchangeRate,
                CUSTOMERID = customerId,
                LOANAPPLICATIONID = applicationId,
                STATUSID = (short)LoanApplicationDetailsStatusEnum.Pending,
                //REPAYMENTSCHEDULE = productType == (int)LoanProductTypeEnum.RevolvingLoan ? "N/A" : "To be advised after loan setup.",

                PROPOSEDAMOUNT = entity.proposedAmount,
                PROPOSEDINTERESTRATE = (double)product.MAXIMUMRATE.Value,
                PROPOSEDPRODUCTID = (short)entity.proposedProductId,
                //PROPOSEDPRODUCTDETAILID = (short)entity.proposedProductId,
                PROPOSEDTENOR = entity.proposedTenor,
                DELETED = false,
                SUBSECTORID = entity.subSectorId,
                CREATEDBY = createdBy,
                DATETIMECREATED = DateTime.Now,
                LOANPURPOSE = entity.loanPurpose,
                //CASAACCOUNTID = entity.casaAccountId,
                OPERATINGCASAACCOUNTID = accountId,
                REPAYMENTTERMS = string.Empty,
                TENORFREQUENCYTYPEID = entity.tenorModeId,
                //PROPOSEDTENORFREQUENCYTYPEID = entity.tenorModeId,
                CRMSVALIDATED = false,
                //NEXTREVIEWDATE = null,
                //ESGMSREQUIRED = false,
                //ISUNADVICED = entity.isUnAdviced,
                //LOANDETAILTYPEID = (int)LoanApplicationDetailTypeEnum.NEW,
                TAKEFEETYPEID = (int)TakeFeeTypeEnum.ApprovedAmount,
                ISLINEFACILITY = entity.isLineFacility,
                //REQUESTEDAMOUNT = entity.requestedAmount,
                //REPAYMENTDATE = entity.repaymentDate,
                //CREDITSCORE = entity.creditScore,
                //CREDITRATING = entity.creditRating,
            };

            var applicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Add(newLoanDetail);

            // save changed to get last saved ID
            int changes = context.SaveChanges();

            savedChanges = changes;
            //savedChanges = changes2 + changes;
            return applicationDetail.LOANAPPLICATIONDETAILID;
        }

        //private void InvoiceDetails(List<InvoiceDetailForCreation> entity, int createdBy, int loanApplicationId, int loanApplicationDetailId, int customerId, FinTrakBankingContext context)
        //{
        //    var modifiedEntity = new List<InvoiceDetailForCreation>();
        //    foreach (var invoice in entity)
        //    { 
        //        if (invoice.principalId <= 0 && string.IsNullOrWhiteSpace(invoice.principalName) == false)
        //        {
        //            invoice.principalId = null;
        //        }
        //        else
        //        {
        //            var principal = context.TBL_LOAN_PRINCIPAL.Find(invoice.principalId);
        //            if (principal != null)
        //            {
        //                invoice.principalName = principal.NAME;
        //            }
        //        }

        //        modifiedEntity.Add(invoice);
        //    }


        //    var data = modifiedEntity.Select(c => new TBL_LOAN_APPLICATION_DETL_INV()
        //    {
        //        CONTRACT_ENDDATE = c.contractEndDate,
        //        CONTRACT_STARTDATE = c.contractStartDate,
        //        INVOICENO = c.invoiceNo,
        //        CONTRACTNO = c.contractNo,
        //        INVOICE_AMOUNT = c.invoiceAmount,
        //        INVOICE_CURRENCYID = c.invoiceCurrencyId,
        //        INVOICE_DATE = c.invoiceDate,
        //        LOANAPPLICATIONDETAILID = loanApplicationDetailId,
        //        PRINCIPALID = c.principalId,
        //        PRINCIPALNAME = c.principalName,
        //        DATETIMECREATED = DateTime.Now,
        //        EXCHANGERATE = (decimal)c.exchangeInvoiceRate,
        //        CREATEDBY = createdBy,
        //        PURCHASEORDERNUMBER = c.purchaseOrderNumber,
        //        CERTIFICATENO = c.certificateNumber,
        //        REVALIDATED = 0,
        //        ENTRYSHEETNUMBER = c.entrySheetNumber
        //    });
        //    context.TBL_LOAN_APPLICATION_DETL_INV.AddRange(data);
        //}

        //private void TraderLoan(TraderLoanForCreation entity, int loanApplicationId, int createdBy, FinTrakBankingContext context)
        //{

        //    var data = new TBL_LOAN_APPLICATION_DETL_TRA();
        //    data.AVERAGE_MONTHLY_TURNOVER = entity.averageMonthlyTurnover;
        //    data.CREATEDBY = createdBy;
        //    data.LOANAPPLICATIONDETAILID = loanApplicationId;
        //    data.DATETIMECREATED = DateTime.Now;

        //    if (entity.marketLocationId != null)
        //        data.MARKETID = entity.marketLocationId;

        //    else
        //    {
        //        data.MARKETID = null;
        //        data.MARKETLOCATIONS = entity.marketLocations;
        //    }

        //    if (entity.soldItemsId != null)
        //        data.SOLDITEMID = entity.soldItemsId;

        //    else
        //    {
        //        data.SOLDITEMID = null;
        //        data.SOLDITEMS = entity.soldItems;
        //    }

        //    context.TBL_LOAN_APPLICATION_DETL_TRA.Add(data);
        //}


        //public async Task<LoanEligibilityForReturn> LoanEligibility(LoanEligibilityForInquiry forInquiry)
        //{
        //    using (FinTrakBankingContext context = new FinTrakBankingContext())
        //    {
        //        // get the customer based on the customer code
        //        var customer = await context.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == forInquiry.customerCode.Trim()).FirstOrDefaultAsync();
        //        if (customer == null)
        //            throw new SecureException($"There is no customer with code {forInquiry.customerCode.Trim()}.");


        //        // get the selected product details
        //        var productInfo = await context.TBL_PRODUCT.Where(q => q.PRODUCTID == forInquiry.productId).FirstOrDefaultAsync();
        //        if (productInfo == null)
        //            throw new SecureException("Kinldy select an exinting product.");


        //        int turnoverDuration = 0;

        //        var apiTransactions = new List<ViewModels.ThridPartyIntegration.CustomerTurnoverViewModel>();
        //        var returnEligibilityObj = new LoanEligibilityForReturn();


        //        if (USE_THIRD_PARTY_INTEGRATION == 1)
        //        {
        //            try
        //            {
        //                var productConfig = await context.TBL_PRODUCT_CONFIG.FirstOrDefaultAsync(v => v.PRODUCTID == productInfo.PRODUCTID);

        //                if (productConfig != null)
        //                {
        //                    turnoverDuration = (int)productConfig.TURNOVERDURATION;
        //                    apiTransactions = integration.GetCustomerAccountTurnover(customer.CUSTOMERCODE, turnoverDuration);

        //                    if (apiTransactions.Any())
        //                    {
        //                        decimal? creditTurnover = apiTransactions.Sum(v => v.credit_Turnover);

        //                        if (creditTurnover == null || creditTurnover == 0)
        //                        {
        //                            returnEligibilityObj.isEligible = false;
        //                            returnEligibilityObj.eligibleAmount = 0;
        //                            returnEligibilityObj.eligibilityMessage = $"The customer with code '{customer.CUSTOMERCODE}' is not eligible to take a loan.";
        //                            // return respond
        //                            return returnEligibilityObj;
        //                        }

        //                        // get the percentage of the value
        //                        double percentage = (double)productConfig.PERCENTAGEAVETURNOVER / (double)100;

        //                        // based on the percentage calculate the eligible amount
        //                        var amount = (decimal)creditTurnover * (decimal)percentage;


        //                        returnEligibilityObj.isEligible = true;
        //                        returnEligibilityObj.eligibleAmount = amount;
        //                        returnEligibilityObj.eligibilityMessage = $"The customer with code '{customer.CUSTOMERCODE}' is eligible to take the sum of {amount}.";
        //                        // return respond
        //                        return returnEligibilityObj;
        //                    }
        //                    else
        //                    {
        //                        returnEligibilityObj.isEligible = false;
        //                        returnEligibilityObj.eligibleAmount = 0;
        //                        returnEligibilityObj.eligibilityMessage = $"The customer with code '{customer.CUSTOMERCODE}' is not eligible to take a loan.";
        //                        // return respond
        //                        return returnEligibilityObj;
        //                    }
        //                }
        //                else
        //                    throw new SecureException("There is no configuration existing for this product. Contact adming.");
        //            }
        //            catch (APIErrorException ex)
        //            {
        //                throw new SecureException(ex.Message);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new SecureException("An error occured while get customer account details");
        //            }
        //        }
        //        else
        //        {
        //            throw new SecureException($"Kinldy contact admin, as the application enviroment does not support loan eligibility enquiry.");
        //        }
        //    }
        //}


        public async Task<bool> LaonDocumentUpload(LoanDocumentViewModel model, byte[] file)
        {
            using (var dbcontext = new FinTrakBankingContext())
            {
                //file size
                int fileSize = file.Length;
                var MaxFileUploadSize = dbcontext.TBL_SETUP_GLOBAL.FirstOrDefault().MAXIMUMUPLOADFILESIZE;
                var mbSize = MaxFileUploadSize / (1024 * 1024);
                if (fileSize > (MaxFileUploadSize))//max file size should come from database
                    throw new ConditionNotMetException($"Exceed File Size. Maximum File size is {mbSize}MB");


                //var customer = await dbcontext.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == model.customerCode).FirstOrDefaultAsync();
                //if (customer == null)
                //    throw new ConditionNotMetException($"Customer with customer code {model.customerCode} does not exist.");

                var laonApp = await dbcontext.TBL_LOAN_APPLICATION.Where(a => a.APPLICATIONREFERENCENUMBER == model.loanReferenceNumber).FirstOrDefaultAsync();
                if (laonApp == null)
                    throw new ConditionNotMetException($"The transaction with ref no {model.loanReferenceNumber} does not exist.");


                //string fileExtendtion = model.fileExtension.Remove(0, 1);

                if (!CommonHelpers.FileTypes.Contains(model.fileExtension))
                {
                    throw new ConditionNotMetException("Kindly Upload Valid File with Accepted Extension " + CommonHelpers.FileTypes);
                }

                try
                {
                    if (model.documentTitle == null || string.IsNullOrEmpty(model.documentTitle))
                        model.documentTitle = "N/A";


                    if (model.loanReferenceNumber.Length > 50)
                    {
                        model.loanReferenceNumber = model.loanReferenceNumber.Substring(0, 50);
                    }

                    using (var context = new FinTrakBankingDocumentsContext())
                    {
                        var previousData = context.TBL_MEDIA_LOAN_DOCUMENTS.FirstOrDefault(d => d.LOANAPPLICATIONNUMBER == model.loanReferenceNumber && d.DOCUMENTTITLE.ToLower() == model.documentTitle.ToLower());

                        if (previousData != null)
                        {
                            //replace file with same documentTitle and loanApplicationNumber
                            previousData.FILEDATA = file;
                            previousData.SYSTEMDATETIME = DateTime.Now;
                            previousData.FILENAME = model.fileName;
                            previousData.FILEEXTENSION = model.fileExtension.ToLower();
                        }
                        else
                        {
                            //add new file
                            var data = new TBL_MEDIA_LOAN_DOCUMENTS();

                            data.FILEDATA = file;
                            data.LOANAPPLICATIONNUMBER = model.loanReferenceNumber;
                            data.LOANREFERENCENUMBER = laonApp.LOANAPPLICATIONID.ToString();
                            data.DOCUMENTTITLE = model.documentTitle;
                            data.DOCUMENTTYPEID = (short)LoanDocumentTypeEnum.Others;
                            //data.LOAN_BOOKING_REQUESTID = model.SourceId;
                            data.FILENAME = model.fileName;
                            data.FILEEXTENSION = model.fileExtension.ToLower();
                            data.SYSTEMDATETIME = DateTime.Now;
                            data.PHYSICALFILENUMBER = string.Empty;
                            data.PHYSICALLOCATION = "n/a";
                            data.ISPRIMARYDOCUMENT = false;
                            data.CREATEDBY = laonApp.CREATEDBY;

                            context.TBL_MEDIA_LOAN_DOCUMENTS.Add(data);
                        }


                        return context.SaveChanges() != 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
        }


        public async Task<bool> LoanDocumentUpload(List<LoanDocumentViewModel> model)
        {
            using (var dbcontext = new FinTrakBankingContext())
            {
                try
                {
                    int result = 0;
                    string loanReferenceNo = model[0].loanReferenceNumber;
                    var docModels = new List<TBL_MEDIA_LOAN_DOCUMENTS>();
                    var laonApp = await dbcontext.TBL_LOAN_APPLICATION.Where(a => a.APPLICATIONREFERENCENUMBER == loanReferenceNo).FirstOrDefaultAsync();
                    if (laonApp == null)
                        throw new ConditionNotMetException($"The transaction with ref no {model[0].loanReferenceNumber} does not exist.");

                    //if (model[0].loanReferenceNumber.Length > 50)
                    //{
                    //    model[0].loanReferenceNumber = model[0].loanReferenceNumber.Substring(0, 50);
                    //}

                    using (var context = new FinTrakBankingDocumentsContext())
                    {

                        foreach (var document in model)
                        {
                            //file size
                            int fileSize = document.fileData.Length;
                            var MaxFileUploadSize = dbcontext.TBL_SETUP_GLOBAL.FirstOrDefault().MAXIMUMUPLOADFILESIZE;
                            var mbSize = MaxFileUploadSize / (1024 * 1024);
                            if (fileSize > (MaxFileUploadSize))//max file size should come from database
                                throw new ConditionNotMetException($"Exceed File Size. Maximum File size is {mbSize}MB");

                            if (!CommonHelpers.FileTypes.Contains(document.fileExtension))
                            {
                                throw new ConditionNotMetException("Kindly Upload Valid File with Accepted Extension " + CommonHelpers.FileTypes);
                            }

                            if (document.documentTitle == null || string.IsNullOrEmpty(document.documentTitle))
                                document.documentTitle = "N/A";


                            if (document.loanReferenceNumber.Length > 50)
                            {
                                document.loanReferenceNumber = document.loanReferenceNumber.Substring(0, 50);
                            }


                            var previousData = context.TBL_MEDIA_LOAN_DOCUMENTS.FirstOrDefault(d => d.LOANAPPLICATIONNUMBER == document.loanReferenceNumber && d.DOCUMENTTITLE.ToLower() == document.documentTitle.ToLower());

                            if (previousData != null)
                            {
                                //replace file with same documentTitle and loanApplicationNumber
                                previousData.FILEDATA = document.fileData;
                                previousData.SYSTEMDATETIME = DateTime.Now;
                                previousData.FILENAME = document.fileName;
                                previousData.FILEEXTENSION = document.fileExtension.ToLower();
                                result = context.SaveChanges();
                            }
                            else
                            {
                                //add new file
                                var data = new TBL_MEDIA_LOAN_DOCUMENTS();

                                data.FILEDATA = document.fileData;
                                data.LOANAPPLICATIONNUMBER = document.loanReferenceNumber;
                                data.LOANREFERENCENUMBER = laonApp.LOANAPPLICATIONID.ToString();
                                data.DOCUMENTTITLE = document.documentTitle;
                                data.DOCUMENTTYPEID = (short)LoanDocumentTypeEnum.Others;
                                //data.LOAN_BOOKING_REQUESTID = model.SourceId;
                                data.FILENAME = document.fileName;
                                data.FILEEXTENSION = document.fileExtension.ToLower();
                                data.SYSTEMDATETIME = DateTime.Now;
                                data.PHYSICALFILENUMBER = string.Empty;
                                data.PHYSICALLOCATION = "n/a";
                                data.ISPRIMARYDOCUMENT = false;
                                data.CREATEDBY = laonApp.CREATEDBY;

                                docModels.Add(data);


                            }


                        }

                        context.TBL_MEDIA_LOAN_DOCUMENTS.AddRange(docModels);

                        result = result + context.SaveChanges();

                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }




                //var customer = await dbcontext.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == model.customerCode).FirstOrDefaultAsync();
                //if (customer == null)
                //    throw new ConditionNotMetException($"Customer with customer code {model.customerCode} does not exist.");




                //string fileExtendtion = model.fileExtension.Remove(0, 1);




            }
        }

        public async Task<List<LoanUploadedDocumentForReturn>> GetLoanDocumentUploadByRefNo(string loanReferenceNumber)
        {
            using (var dbcontext = new FinTrakBankingContext())
            {
                var laonApp = await dbcontext.TBL_LOAN_APPLICATION.Where(a => a.APPLICATIONREFERENCENUMBER == loanReferenceNumber).FirstOrDefaultAsync();
                if (laonApp == null)
                    throw new ConditionNotMetException($"The transaction with ref no {loanReferenceNumber} does not exist.");


                using (var context = new FinTrakBankingDocumentsContext())
                {
                    var Record = await context.TBL_MEDIA_LOAN_DOCUMENTS.Where(x => x.LOANAPPLICATIONNUMBER == loanReferenceNumber).Select(x => new LoanUploadedDocumentForReturn
                    {
                        //documentId = x.DOCUMENTID,
                        //customerId = x.CUSTOMERID,
                        loanReferenceNumber = x.LOANAPPLICATIONNUMBER,
                        documentTitle = x.DOCUMENTTITLE,
                        //documentTypeId = (short)x.DOCUMENTTYPEID,
                        fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        systemDateTime = x.SYSTEMDATETIME,
                        //physicalFileNumber = x.PHYSICALFILENUMBER,
                        //physicalLocation = x.PHYSICALLOCATION,
                    }).ToListAsync();

                    return Record;
                }
            }
        }

        public async Task<bool> CheckOutstandingLoan(string nhfNo)
        {
            using (var context = new FinTrakBankingContext())
            {
                var account = context.TBL_CASA.FirstOrDefaultAsync(c => c.PRODUCTACCOUNTNUMBER.ToLower() == nhfNo.Trim().ToLower());
                if (account.Result != null)
                {
                    string message = string.Empty;
                    int[] loanStatus = { (int)LoanStatusEnum.Active, (int)LoanStatusEnum.Suspended };

                    var loans = await (from a in context.TBL_LOAN_APPLICATION
                                       join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                       join c in context.TBL_LOAN on b.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                                       join d in context.TBL_CASA on c.CASAACCOUNTID equals d.CASAACCOUNTID
                                       join e in context.TBL_CURRENCY on b.CURRENCYID equals e.CURRENCYID

                                       where d.PRODUCTACCOUNTNUMBER.ToLower() == nhfNo.Trim().ToLower() && loanStatus.Contains(c.LOANSTATUSID)
                                       orderby c.TERMLOANID ascending
                                       select new
                                       {
                                           loanRefNumber = c.LOANREFERENCENUMBER,
                                           outstandingBalance = c.OUTSTANDINGPRINCIPAL,
                                           currency = e.CURRENCYCODE
                                       }).ToListAsync();

                    if (loans.Count() > 0)
                    {
                        if (loans.Count() == 1)
                        {
                            message = $"Loan Ref Number: {loans[0].loanRefNumber} with outstanding balance {loans[0].currency} {loans[0].outstandingBalance.ToString("N")} is yet to be fully repaid by customer";
                        }
                        else
                        {
                            int count = 1;

                            foreach (var loan in loans)
                            {
                                message = message + $"({count}) Loan Ref Number: {loan.loanRefNumber} with outstanding balance {loans[0].currency} {loan.outstandingBalance.ToString("N")} ";
                                count += 1;
                            }

                            message = message + "are yet to be fully repaid by customer";
                        }

                        throw new SecureException($"{message}");
                    }

                    return true;
                }
                else
                {
                    throw new ConditionNotMetException($"NHF Account {nhfNo} not found");
                }
            }
        }

        public CreditLimitValidationsModel ValidateCreditLimitByRMBM(int staffId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                decimal? accountOfficerMaximumNPLExposure = 0;

                var outstandingLoanSum = (from d in context.TBL_LOAN
                                          where d.LOANSTATUSID == (short)LoanStatusEnum.Active &&
                                          (d.RELATIONSHIPOFFICERID == staffId || d.RELATIONSHIPMANAGERID == staffId)
                                          select d.OUTSTANDINGPRINCIPAL).ToList();
                var outstandingLoan = outstandingLoanSum.Count() > 0 ? outstandingLoanSum.Sum() : 0;
                var outstandingRevolvingSum = (from d in context.TBL_LOAN_REVOLVING
                                               where d.LOANSTATUSID == (short)LoanStatusEnum.Active &&
                                               (d.RELATIONSHIPOFFICERID == staffId || d.RELATIONSHIPMANAGERID == staffId)
                                               select d.OVERDRAFTLIMIT).ToList();
                var outstandingRevolving = outstandingRevolvingSum.Count() > 0 ? outstandingRevolvingSum.Sum() : 0;
                var accountOfficerNPLExposure = outstandingLoan + outstandingRevolving;

                var officer = context.TBL_STAFF.FirstOrDefault(a => a.STAFFID == staffId);
                if (officer != null) accountOfficerMaximumNPLExposure = officer.NPL_LIMIT.HasValue ? officer.NPL_LIMIT : 0;

                var accountOfficerNPLLimit = accountOfficerMaximumNPLExposure - accountOfficerNPLExposure;


                var initiatedRecord = (from a in context.TBL_LOAN_APPLICATION
                                       join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                       where d.STATUSID == (short)ApprovalStatusEnum.Approved && a.APPROVEDDATE == null && d.DELETED == false
                                       select d.APPROVEDAMOUNT).ToList();
                var initiated = initiatedRecord.Count() > 0 ? initiatedRecord.Sum() : 0;

                var approvedRecord = (from a in context.TBL_LOAN_APPLICATION
                                      join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                      where d.STATUSID == (short)ApprovalStatusEnum.Approved && a.APPROVEDDATE != null &&
                                      (a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.BookingRequestCompleted &&
                                      a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.BookingRequestInitiated) && d.DELETED == false
                                      select d.APPROVEDAMOUNT).ToList();
                var approved = approvedRecord.Count() > 0 ? approvedRecord.Sum() : 0;

                return new CreditLimitValidationsModel
                {
                    initiated = initiated,
                    approved = approved,
                    limit = (double)accountOfficerNPLLimit < 0 ? 0 : (double)accountOfficerNPLLimit,
                    limitString = (accountOfficerMaximumNPLExposure == 0) ? string.Format("{0:#,0.00}", 0) : string.Format("{0:#,0.00}", accountOfficerNPLLimit),
                    nplExposure = accountOfficerNPLExposure,
                    maximumAllowedLimit = (decimal)accountOfficerMaximumNPLExposure
                };
            }
        }

        public void ValidateLoanApplicationLimits(int branchId, int customerId, decimal proposedAmountSum, List<short> sectorIds)
        {
            //LoanApplicationViewModel application
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                //var details = application.LoanApplicationDetail;
                //int branchId = (int)application.branchId;
                //int customerId = (int)application.customerId;

                var branchOverrideRequest = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId)
                    .Join(context.TBL_OVERRIDE_DETAIL.Where(x => x.OVERRIDE_ITEMID == (int)OverrideItem.BranchNplLimitOverride && x.ISUSED == false),
                        c => c.CUSTOMERCODE, o => o.CUSTOMERCODE, (c, o) => new { c, o })
                    .Select(x => new { id = x.o.OVERRIDE_DETAILID })
                    .FirstOrDefault();

                var sectorOverrideRequest = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerId)
                    .Join(context.TBL_OVERRIDE_DETAIL.Where(x => x.OVERRIDE_ITEMID == (int)OverrideItem.SectorNplLimitOverride && x.ISUSED == false),
                        c => c.CUSTOMERCODE, o => o.CUSTOMERCODE, (c, o) => new { c, o })
                    .Select(x => new { id = x.o.OVERRIDE_DETAILID })
                    .FirstOrDefault();

                if (branchOverrideRequest != null)
                {
                    //var request = context.TBL_OVERRIDE_DETAIL.Find(overrideRequest.id);
                    //request.ISUSED = true;
                }
                else
                {
                    // branch limits
                    var branchValidation = limitValidation.ValidateNPLByBranch((short)branchId);
                    decimal branchNplAmount = (decimal)branchValidation.outstandingBalance;
                    //decimal applicationAmount = details.Sum(x => x.proposedAmount); // proposedAmount should be approvedAmount after application
                    decimal applicationAmount = proposedAmountSum; // proposedAmount should be approvedAmount after application
                    var branch = context.TBL_BRANCH.Find(branchId);
                    if (branch.NPL_LIMIT > 0 && branch.NPL_LIMIT < (branchNplAmount + applicationAmount))
                        throw new SecureException("Branch NPL Limit exceeded!");
                }

                if (sectorOverrideRequest != null)
                {
                    //var request = context.TBL_OVERRIDE_DETAIL.Find(overrideRequest.id);
                    //request.ISUSED = true;
                }
                else
                {
                    // sector limits
                    // sectorId here is actually the subsectorId
                    //List<short> sectorIds = details.Select(x => x.subSectorId).ToList();
                    foreach (var sectorId in sectorIds)
                    {
                        var sectorValidation = limitValidation.ValidateNPLBySector(sectorId);
                        decimal sectorAmount = (decimal)sectorValidation.outstandingBalance;
                        //var sector = context.TBL_SECTOR.Find(sectorId);
                        if (sectorValidation.maximumAllowedLimit > 0 && sectorValidation.maximumAllowedLimit <= sectorAmount)
                            throw new SecureException("Sector Limit exceeded!");
                    }
                }
            }
        }


        public RefinanceViewModel RefinanceLoan(RefinanceViewModel Model)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        var message = string.Empty;
                        // validations to be added
                        foreach (var item in Model.RefinanceDetails)
                        {
                            var LoanExist = context.TblRefinancingLoan.Where(x => x.LoanId == item.LoanId).FirstOrDefault();
                            if (LoanExist != null)
                            {
                                message = "LoanID" + ": " + item.LoanId + " already submitted for refinancing";
                                throw new SecureException($"{message}");

                            }

                            if (Model.LoanSource == 1 && item.LoanId == null)
                            {
                                message = "Pls prove LoanId for internally applied loan";
                                throw new SecureException($"{message}");

                            }

                        }



                        var result = Model;
                        var Loans = new TblRefinancing
                        {
                            TotalAmount = Model.TotalAmount,
                            PmbId = long.Parse(Model.PmbId),
                            RefinanceNumber = Model.RefinanceBatchNumber,
                            Status = 1,
                            ApplicationDate = DateTime.Now,
                            ApplicationStatus = 0,
                            Disbursed = 0,
                            LenderId = long.Parse(Model.SecondaryLenderId)

                        };
                        context.TblRefinancing.Add(Loans);

                        foreach (var item in Model.RefinanceDetails)
                        {
                            var LoanBreakdown = new TblRefinancingLoan
                            {
                                Amount = item.Amount,
                                RefinanceNumber = item.RefinanceNumber,
                                ProductCode = item.ProductCode,
                                Nhfnumber = item.Nhfnumber,
                                ApplicationDate = DateTime.Now,
                                Approved = 0,
                                Rate = item.Rate,
                                Tenor = item.Tenor,
                                ApplicationStatus = 0,
                                LoanId = item.LoanId,
                                Status = 1,
                                Disbursed = 0,
                                CustomerName = item.CustomerName,

                            };
                            context.TblRefinancingLoan.Add(LoanBreakdown);
                        }


                        var output = context.SaveChanges() > 0;
                        trans.Commit();
                        trans.Dispose();


                        return result;
                    }
                    catch (DbEntityValidationException ex)
                    {
                        trans.Rollback();

                        string errorMessages = string.Join("; ",
                        ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                        throw new DbEntityValidationException(errorMessages);
                    }


                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }
            }
        }

        public async Task<List<TblRefinancingLoan>> GetLoanForRefinance1(long CompanyId)
        {
            try
            {
                using (var dbcontext = new FinTrakBankingContext())
                {
                    var RefinanceAplications = new List<TblRefinancingLoan>();
                    var LaonApp = dbcontext.TblRefinancing.Where(a => a.PmbId == CompanyId && a.Status == 1).ToList();
                    foreach (var item in LaonApp)
                    {
                        var RefinanceDetails = dbcontext.TblRefinancingLoan.Where(x => x.RefinanceNumber == item.RefinanceNumber && x.Checklisted != 1 && x.Status == 1).ToList();
                        RefinanceAplications.AddRange(RefinanceDetails);

                    }

                    return RefinanceAplications;

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<StNmrcEligibility>> GetUUSForObligor()
        {
            try
            {
                using (var dbcontext = new FinTrakBankingContext())
                {
                    var Criterias = dbcontext.StNmrcEligibilities.Where(a => a.Category == 1).ToList();

                    return Criterias;

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<CustomerUusViewModel> PostCustomersUItems(List<CustomerUusViewModel> Model)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        var nhfNumber = Model.FirstOrDefault().NhfNumber;
                        var message = string.Empty;
                        var UusItems = context.StNmrcEligibilities.Where(x => x.DocUpload == 1).ToList();
                        var EmployeeUusItems = context.StNmrcEligibilities.Where(x => x.Category == 1).ToList();
                        var ExisitngItems = context.TblCustomerUUS.Where(x => x.EmployeeNhfNumber == nhfNumber).ToList();


                        if ((Model.Count + ExisitngItems.Count) == EmployeeUusItems.Count)
                        {
                            var Id = Model.FirstOrDefault().LoanId;
                            var RefinanceMod = context.TblRefinancingLoan.Where(x => x.LoanId == Id).FirstOrDefault();
                            RefinanceMod.Checklisted = 1;

                            var output1 = context.SaveChanges() > 0;
                            trans.Commit();
                            trans.Dispose();


                            return Model;

                        }

                        if (ExisitngItems.Count == 0)
                        {
                            if (Model.Count < EmployeeUusItems.Count)
                            {

                                message = "pls complete all checklist items";
                                throw new SecureException($"{message}");

                            }
                        }

                        var NonExisting = new List<CustomerUusViewModel>();

                        if (ExisitngItems.Count > 0 && (Model.Count > ExisitngItems.Count))
                        {
                            foreach (var ExisitngItem in ExisitngItems)
                            {
                                var fil = Model.Where(x => x.ItemId == ExisitngItem.ItemId && x.NhfNumber == ExisitngItem.EmployeeNhfNumber).FirstOrDefault();
                                if (fil == null)
                                    NonExisting.Add(fil);

                            }

                        }

                        if (NonExisting.Count > 0)
                        {
                            foreach (var item in NonExisting)
                            {

                                var UusItem = UusItems.Where(x => x.Id == item.ItemId).FirstOrDefault();
                                if (item.Option != Options.Defer && UusItem != null && item.FileContentBase64 == null)
                                {
                                    message = "Document upload required for item " + item.Item;
                                    throw new SecureException($"{message}");
                                }

                                var CustomerUus = new TblCustomerUUS
                                {
                                    EmployeeNhfNumber = item.NhfNumber,
                                    PmbId = long.Parse(item.PmbId),
                                    Item = item.Item,
                                    Description = item.Description,
                                    Option = (int)item.Option,
                                    ItemId = item.ItemId,
                                };
                                context.TblCustomerUUS.Add(CustomerUus);
                                if (item.FileContentBase64 != null)
                                {
                                    if (item.FileContentBase64.Contains(","))
                                    {
                                        item.FileContentBase64 = item.FileContentBase64.Split(',')[1];
                                    }
                                    var fileData = Convert.FromBase64String(item.FileContentBase64);

                                    var CustomerDoc = new TblCustomerUUSDocument
                                    {
                                        // FileId = entity.Id,
                                        Nhfno = item.NhfNumber,
                                        Item = item.Item,
                                        Type = item.FileType,
                                        Label = item.FileName,
                                        Images = item.FileType,
                                        Size = fileData.Length,
                                        Filedata = fileData,
                                        ItemId = item.ItemId
                                    };
                                    context.TblCustomerUUSDocument.Add(CustomerDoc);

                                }
                            }


                        }


                        foreach (var item in Model)
                        {
                            var UusItem = UusItems.Where(x => x.Id == item.ItemId).FirstOrDefault();
                            if (item.Option != Options.Defer && UusItem != null && item.FileContentBase64 == null)
                            {
                                message = "Document upload required for item " + item.Item;
                                throw new SecureException($"{message}");
                            }

                            var CustomerUus = new TblCustomerUUS
                            {
                                EmployeeNhfNumber = item.NhfNumber,
                                PmbId = long.Parse(item.PmbId),
                                Item = item.Item,
                                Description = item.Description,
                                Option = (int)item.Option,
                                ItemId = item.ItemId
                            };
                            context.TblCustomerUUS.Add(CustomerUus);
                            if (item.FileContentBase64 != null)
                            {
                                if (item.FileContentBase64.Contains(","))
                                {
                                    item.FileContentBase64 = item.FileContentBase64.Split(',')[1];
                                }
                                var fileData = Convert.FromBase64String(item.FileContentBase64);

                                var CustomerDoc = new TblCustomerUUSDocument
                                {
                                    // FileId = entity.Id,
                                    Nhfno = item.NhfNumber,
                                    Item = item.Item,
                                    Type = item.FileType,
                                    Label = item.FileName,
                                    Images = item.FileType,
                                    Size = fileData.Length,
                                    Filedata = fileData,
                                    ItemId = item.ItemId
                                };
                                context.TblCustomerUUSDocument.Add(CustomerDoc);

                            }
                        }

                        var LoanId = Model.FirstOrDefault().LoanId;
                        var RefinanceModel = context.TblRefinancingLoan.Where(x => x.LoanId == LoanId).FirstOrDefault();
                        RefinanceModel.Checklisted = 1;

                        var output = context.SaveChanges() > 0;
                        trans.Commit();
                        trans.Dispose();


                        return Model;
                    }
                    catch (DbEntityValidationException ex)
                    {
                        trans.Rollback();

                        string errorMessages = string.Join("; ",
                        ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                        throw new DbEntityValidationException(errorMessages);
                    }


                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }
            }
        }



        public async Task<List<TblRefinancingLoan>> GetPmbsChecklistedLoan(long CompanyId)
        {
            try
            {
                using (var dbcontext = new FinTrakBankingContext())
                {
                    var RefinanceAplications = new List<TblRefinancingLoan>();
                    var LaonApp = dbcontext.TblRefinancing.Where(a => a.PmbId == CompanyId && a.Status == 1).ToList();
                    foreach (var item in LaonApp)
                    {
                        var RefinanceDetails = dbcontext.TblRefinancingLoan.Where(x => x.RefinanceNumber == item.RefinanceNumber && x.Checklisted == 1 && x.Status == 1 && x.ApplicationStatus == 0).ToList();
                        RefinanceAplications.AddRange(RefinanceDetails);

                    }

                    return RefinanceAplications;

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<TblRefinancingLoan> ApprovePmbRefinancing(List<int> Model)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        var random = new Random();
                        var message = string.Empty;
                        var LoanLists = new List<TblRefinancingLoan>();
                        var Loans = context.TblRefinancingLoan.ToList();
                        decimal? TotalAmount = 0;
                        var RefNumber = "Ref-" + random.Next(100000, 1000000);

                        foreach (var item in Model)
                        {
                            var Loan = Loans.Where(x => x.Id == item).FirstOrDefault();
                            Loan.ApplicationStatus = 1;
                            Loan.Approved = 1;
                            TotalAmount += Loan.Amount;
                            LoanLists.Add(Loan);

                        }

                        var NmrcRefinance = new TblNmrcRefinancing
                        {
                            TotalAmount = TotalAmount,
                            RefinanceNumber = RefNumber,
                            PmbId = LoanLists.FirstOrDefault().PmbId,
                            ApplicationDate = DateTime.Now,
                            LenderId = LoanLists.FirstOrDefault().LenderId,

                        };
                        context.TblNmrcRefinancing.Add(NmrcRefinance);


                        foreach (var item in LoanLists)
                        {
                            var NmrcLoans = new TblNmrcRefinancingLoan
                            {
                                Amount = item.Amount,
                                Nhfnumber = item.Nhfnumber,
                                CustomerName = item.CustomerName,
                                Disbursed = 0,
                                ApplicationDate = DateTime.Now,
                                Status = 0,
                                Checklisted = 0,
                                Rate = item.Rate,
                                Tenor = item.Tenor,
                                ApplicationStatus = 0,
                                LoanId = item.LoanId,
                                ProductCode = item.ProductCode,
                                RefinanceNumber = RefNumber,
                                PmbId = NmrcRefinance.PmbId,
                                LenderId = item.LenderId
                            };

                            context.TblNmrcRefinancingLoan.Add(NmrcLoans);
                        }


                        var output = context.SaveChanges() > 0;
                        trans.Commit();
                        trans.Dispose();


                        return LoanLists;
                    }
                    catch (DbEntityValidationException ex)
                    {
                        trans.Rollback();

                        string errorMessages = string.Join("; ",
                        ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                        throw new DbEntityValidationException(errorMessages);
                    }


                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }
            }
        }

        public async Task<List<TblCustomerUUS>> GetCustomerUusItems(string NhfNumber)
        {
            try
            {
                using (var dbcontext = new FinTrakBankingContext())
                {
                    var Underwritings = dbcontext.TblCustomerUUS.Where(a => a.EmployeeNhfNumber == NhfNumber).ToList();
                    return Underwritings;

                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<string> GetCustomerUusItemDoc(string NhfNumber, int ItemId)
        {
            try
            {
                using (var dbcontext = new FinTrakBankingContext())
                {
                    var Underwritings = dbcontext.TblCustomerUUSDocument.Where(a => a.Nhfno == NhfNumber && a.ItemId == ItemId).FirstOrDefault();
                    var Image = Underwritings?.Filedata;
                    string base64String = Convert.ToBase64String(Image);
                    var response = $"data:{Underwritings.Type};base64,{base64String}";
                    return response;

                }

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #region NMRC Activities

        public async Task<List<TblNmrcRefinancing>> GetAppliedLoanForNmrcRefinance()
        {
            try
            {
                using (var dbcontext = new FinTrakBankingContext())
                {
                    var AppliedLoans = dbcontext.TblNmrcRefinancing.Where(x=> x.Status != 1 && x.ApplicationStatus != 1 && x.Disbursed != 1).ToList();


                    return AppliedLoans;

                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<TblNmrcRefinancingLoan>> GetAppliedSubLoanForNmrcRefinance(string RefNo)
        {
            try
            {
                using (var dbcontext = new FinTrakBankingContext())
                {
                    var AppliedLoans = dbcontext.TblNmrcRefinancingLoan.Where(x => x.RefinanceNumber == RefNo && x.Checklisted != 1 && x.Reviewed != 1 && x.Approved !=2  && x.Disbursed != 1).ToList();

                    return AppliedLoans;

                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<TblNmrcRefinancingLoan>> GetSubLoanForNmrcReview()
        {
            try
            {
                using (var dbcontext = new FinTrakBankingContext())
                {
                    var AppliedLoans = dbcontext.TblNmrcRefinancingLoan.Where(x => x.Checklisted != 1 && x.Reviewed != 1 && x.Approved != 2 && x.Disbursed != 1).ToList();

                    return AppliedLoans;

                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // then get uus item for each obligors 

        public List<UUSReviewalItem> ReviewCustomersUItems(List<UUSReviewalItem> Model)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        var message = string.Empty;
                        foreach (var item in Model)
                        {
                            var ChecklistItem = context.TblCustomerUUS.Where(x => x.Id == item.Id).FirstOrDefault();
                            ChecklistItem.ReviewalComment = item.ReviewalComment;
                            ChecklistItem.ApprovalComment = item.ApprovalComment;
                            context.TblCustomerUUS.AddOrUpdate(ChecklistItem);
                        }

                        var output = context.SaveChanges() > 0;
                        trans.Commit();
                        trans.Dispose();


                        return Model;
                    }
                    catch (DbEntityValidationException ex)
                    {
                        trans.Rollback();

                        string errorMessages = string.Join("; ",
                        ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                        throw new DbEntityValidationException(errorMessages);
                    }


                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }
            }
        }

        public List<TblNmrcRefinancingLoan> ReviewalApproval(List<int> Model)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        var random = new Random();
                        var message = string.Empty;
                        var Loans = new List<TblNmrcRefinancingLoan>();

                        foreach (var item in Model)
                        {
                            var Loan = context.TblNmrcRefinancingLoan.Where(x => x.Id == item).FirstOrDefault();
                            Loan.Reviewed = 1;
                            Loan.Checklisted = 1;
                            Loans.Add(Loan);
                            context.TblNmrcRefinancingLoan.AddOrUpdate(Loan);
                        }


                        var output = context.SaveChanges() > 0;
                        trans.Commit();
                        trans.Dispose();


                        return Loans;
                    }
                    catch (DbEntityValidationException ex)
                    {
                        trans.Rollback();

                        string errorMessages = string.Join("; ",
                        ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                        throw new DbEntityValidationException(errorMessages);
                    }


                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }
            }
        }

         public List<TblNmrcRefinancingLoan> ReviewalDisApproval(List<int> Model)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        var random = new Random();
                        var message = string.Empty;
                        var Loans = new List<TblNmrcRefinancingLoan>();

                        foreach (var item in Model)
                        {
                            var Loan = context.TblNmrcRefinancingLoan.Where(x => x.Id == item).FirstOrDefault();
                            Loan.Reviewed = 1;
                            Loan.Checklisted = 1;
                            Loan.Approved = 2;
                            Loans.Add(Loan);
                            context.TblNmrcRefinancingLoan.AddOrUpdate(Loan);
                        }

                        

                        var output = context.SaveChanges() > 0;
                        trans.Commit();
                        trans.Dispose();


                        return Loans;
                    }
                    catch (DbEntityValidationException ex)
                    {
                        trans.Rollback();

                        string errorMessages = string.Join("; ",
                        ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                        throw new DbEntityValidationException(errorMessages);
                    }


                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }
            }
        }

        public async Task<List<TblNmrcRefinancingLoan>> GetReviewedForApproval()
        {
            try
            {
                using (var dbcontext = new FinTrakBankingContext())
                {
                    var AppliedLoans = dbcontext.TblNmrcRefinancingLoan.Where(x => x.Checklisted == 1 && x.Reviewed == 1 && x.Approved != 1 && x.Disbursed != 1).ToList();

                    return AppliedLoans;

                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<TblNmrcRefinancingLoan> ApproveReviewedLoan(List<int> Model)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        var random = new Random();
                        var message = string.Empty;
                        var Loans = new List<TblNmrcRefinancingLoan>();

                        foreach (var item in Model)
                        {
                            var Loan = context.TblNmrcRefinancingLoan.Where(x => x.Id == item).FirstOrDefault();
                            Loan.Reviewed = 1;
                            Loan.Checklisted = 1;
                            Loan.Approved = 1;
                            Loans.Add(Loan);
                            context.TblNmrcRefinancingLoan.AddOrUpdate(Loan);
                        }


                        var output = context.SaveChanges() > 0;
                        trans.Commit();
                        trans.Dispose();


                        return Loans;
                    }
                    catch (DbEntityValidationException ex)
                    {
                        trans.Rollback();

                        string errorMessages = string.Join("; ",
                        ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                        throw new DbEntityValidationException(errorMessages);
                    }


                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new SecureException(ex.Message);
                    }
                }
            }
        }




        #endregion

    }
}
