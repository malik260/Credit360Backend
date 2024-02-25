using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;

namespace FintrakBanking.Repositories.Credit
{
    public class ExternalAlertRepository : IExternalAlertRepository
    {
        // dependencies
        private FinTrakBankingContext context;
        private FinTrakBankingStagingContext context2;
        private FinTrakBankingDocumentsContext docContext;
        private IAppraisalMemorandumRepository memo;
        private ILoanRepository loanRepo;
        private IFinanceTransactionRepository financeTransaction;
        private ICustomerGroupRepository groupRepo;
        private ITransactionDynamicsRepository transactionsRepo;
        private IConditionPrecedentRepository conditionsRepo;
        private ICustomerCollateralRepository collateralRepo;
        private IGeneralSetupRepository _genSetup;
        private IWorkflow workflow;

        public ExternalAlertRepository(
            FinTrakBankingContext context,
            FinTrakBankingStagingContext context2,
            FinTrakBankingDocumentsContext docContext,
            IAppraisalMemorandumRepository memo,
            ILoanRepository loanRepo,
            IFinanceTransactionRepository financeTransaction,
            ICustomerGroupRepository groupRepo,
            ITransactionDynamicsRepository transactionsRepo,
            IConditionPrecedentRepository conditionsRepo,
            ICustomerCollateralRepository collateralRepo,
           IGeneralSetupRepository genSetup,
           IWorkflow _workflow
            )
        {
            this.workflow = _workflow;
            this.context = context;
            this.context2 = context2;
            this.docContext = docContext;
            this.memo = memo;
            this.loanRepo = loanRepo;
            this.financeTransaction = financeTransaction;
            this.groupRepo = groupRepo;
            this.transactionsRepo = transactionsRepo;
            this.conditionsRepo = conditionsRepo;
            this.collateralRepo = collateralRepo;
            this._genSetup = genSetup;
        }


        public IEnumerable<StaffInfoViewModel> GetAccountOfficersWithImminentMaturities() //done
        {
            List<int> days = new List<int> { 60, 90, 30, 21, 14, 7, 3, 1 };
            var immenentMaturities = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value)).Select(d => d.ACCOUNTOFFICERCODE).ToList();
            //var immenentMaturities = context.TBL_GLOBAL_EXPOSURE.Where(d => d.PRINCIPALOUTSTANDINGBALLCY > 0).Select(d => d.ACCOUNTOFFICERCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where immenentMaturities.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }
        public IEnumerable<StaffInfoViewModel> GetAccountOfficersWithImminentDocumentMaturities() //done
        {
            List<int> days = new List<int> { 30, 14, 7, 3, 1 };
            var immenentDocMaturities = docContext.TBL_DEFERRED_DOC_TRACKER.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.DUEDATE).Value)).Select(d => d.CREATEDBY).ToList();
            //var immenentMaturities = context.TBL_GLOBAL_EXPOSURE.Where(d => d.PRINCIPALOUTSTANDINGBALLCY > 0).Select(d => d.ACCOUNTOFFICERCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where immenentDocMaturities.Contains(s.STAFFID)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 StaffCode = s.STAFFCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<StaffInfoViewModel> GetAccountOfficersWithImminentMaturitiesCustomers() //done
        {
            List<string> customerIds = new List<string> { "000025950", "000234558" };
            var immenentMaturities = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            customerIds.Contains(d.CUSTOMERID))
            .Select(d => d.ACCOUNTOFFICERCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where immenentMaturities.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }


        public IEnumerable<StaffInfoViewModel> GetImminentMaturitiesGroupHeads()
        {
            List<int> days = new List<int> { 60, 90, 30, 21, 14, 7, 3, 1 };
            var groupHeadsEmails = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value)).Select(d => d.GROUPCODE).ToList();
            //var groupHeadsEmails = context.TBL_GLOBAL_EXPOSURE.Where(d => d.PRINCIPALOUTSTANDINGBALLCY > 0).Select(d => d.GROUPCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where groupHeadsEmails.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).Distinct().ToList();

            return staffList;
        }


        public IEnumerable<StaffInfoViewModel> GetAccountOfficersByGroupHeads(string groupHeadCode)
        {
            List<int> days = new List<int> { 60, 90, 30, 21, 14, 7, 3, 1 };
            var accountOfficers = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value) && d.GROUPCODE == groupHeadCode)
            .Select(d => d.ACCOUNTOFFICERCODE).Distinct().ToList();

            var staffList = (from s in context.TBL_STAFF
                             where accountOfficers.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).Distinct().ToList();

            return staffList;
        }

        public IEnumerable<StaffInfoViewModel> GetPasDueObligationsAccountOfficersByGroupHeads(string groupHeadCode)
        {
            var accountOfficers = context.TBL_GLOBAL_EXPOSURE.Where(d => d.UNPODAYSOVERDUE > 0 && d.GROUPCODE == groupHeadCode)
            .Select(d => d.ACCOUNTOFFICERCODE).Distinct().ToList();

            var staffList = (from s in context.TBL_STAFF
                             where accountOfficers.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com" 
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).Distinct().ToList();

            return staffList;
        }

        public IEnumerable<StaffInfoViewModel> GetDivisionalOfficersByGroupHeads(string groupHeadCode)
        {
            var regionOfficers = context.TBL_GLOBAL_EXPOSURE.Where(d => d.GROUPCODE == groupHeadCode)
            .Select(d => d.DIVISIONCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where regionOfficers.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).Distinct().ToList();

            return staffList;
        }


        public IEnumerable<StaffInfoViewModel> GetCreditCardMaturingObligations() //done
        {
            List<int> days = new List<int> { 60, 89 };
            var immenentMaturities = context.TBL_GLOBAL_EXPOSURE.Where(d =>
             days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
             .Select(d => d.ACCOUNTOFFICERCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where immenentMaturities.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<StaffInfoViewModel> GetExpiringFacilityReport() //done
        {
            List<int> days = new List<int> { 90 };
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
            .Select(d => d.ACCOUNTOFFICERCODE).ToList();
            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<GlobalExposureViewModel> GetLoanExpirationReminder() //done
        {
            List<int> days = new List<int> { 30 };
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
            }).ToList();

            return data;
        }

        public IEnumerable<StaffInfoViewModel> GetLoanExpirationReminderAccountOfficer()
        {
            List<int> days = new List<int> { 30 };
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
            .Select(d => d.ACCOUNTOFFICERCODE).ToList();
            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<GlobalExposureViewModel> GetUnAuthorizedOverdraftReport() // done
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT")
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
            }).ToList();

            return data;
        }

        public IEnumerable<StaffInfoViewModel> GetOverlineMonitoringReport() // done
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => d.ADJFACILITYTYPE == "OVERDRAFT" && DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE)
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => d.ACCOUNTOFFICERCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<StaffInfoViewModel> GetCreditCardDelinquencyMonitoringReport() // done
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => d.UNPODAYSOVERDUE > 0)
            .Select(d => d.ACCOUNTOFFICERCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<StaffInfoViewModel> GetPastDueObligationsReminder()
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => d.UNPODAYSOVERDUE > 0)
               .Select(d => d.ACCOUNTOFFICERCODE).Distinct().ToList();

            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).Distinct().ToList();

            return staffList;
        }

        public IEnumerable<StaffInfoViewModel> GetPastDueObligationsReminderByGroupHeads()
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => d.UNPODAYSOVERDUE > 0)
               .Select(d => d.GROUPCODE).Distinct().ToList();

            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).Distinct().ToList();

            return staffList;
        }

        public IEnumerable<GlobalExposureViewModel> GetScheduleOfDirectorsAccounts() //pending
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value == 30)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetLcUtilizationReportOne() //pending
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetLcUtilizationReportTwo() //pending
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<StaffInfoViewModel> GetNplOnCreditPortfolio()
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => d.NPL > 0)
             .Select(d => d.ACCOUNTOFFICERCODE).ToList();
            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<StaffInfoViewModel> GetOverlineCreditCardPosition()
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE)
                                                         && d.ADJFACILITYTYPE == "OVERDRAFT")
             .Select(d => d.ACCOUNTOFFICERCODE).ToList();
            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.EMAIL.ToLower() != "herbert.wigwe@accessbankplc.com"
                             && s.EMAIL.ToLower() != "wigweh@accessbankplc.com"
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public bool GetRiskAssetsReportNotification() //Done
        {
            var query = context.TBL_ALERT_DAILYREPORT.Where(d => DbFunctions.TruncateTime(d.PROCESSINGDATE) == DbFunctions.TruncateTime(DateTime.UtcNow)).FirstOrDefault();
            if (query == null)
            {
                return false;
            }
            else if (query.SUCCESSFULPROCESSINGIND == "Y")
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool GetDashboardReportNotification() //Done
        {
            var query = context.TBL_ALERT_DAILYREPORT.Where(d => DbFunctions.TruncateTime(d.PROCESSINGDATE) == DbFunctions.TruncateTime(DateTime.UtcNow)).FirstOrDefault();
            if (query == null)
            {
                return false;
            }
            else if (query.SUCCESSFULPROCESSINGIND == "Y")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool GetCACReport() //done
        {
            var query = context.TBL_ALERT_DAILYREPORT.Where(d => DbFunctions.TruncateTime(d.PROCESSINGDATE) == DbFunctions.TruncateTime(DateTime.UtcNow)).FirstOrDefault();
            if (query == null)
            {
                return false;
            }
            else if (query.SUCCESSFULPROCESSINGIND == "Y")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<GlobalExposureViewModel> GetSignificantMovementInDailyRiskAsset() //pending
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetUSDCreditCardReport() //pending
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetNairaCreditCardReport() //pending
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }
        public IEnumerable<GlobalExposureViewModel> GetCreditProgramsLimits()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetSchemePerformanceReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<AllCollateralViewModel> GetExpiredValuationReport()
        {

            var valuationData = context.TBL_COLLATERAL_IMMOVE_PROPERTY
                              .Join(context.TBL_COLLATERAL_CUSTOMER.Where(s => s.VALUATIONCYCLE > 0 || s.VALUATIONCYCLE != null)
                              , us => us.COLLATERALCUSTOMERID, up => up.COLLATERALCUSTOMERID, (us, up) =>
                                  new
                                  {
                                      collateralPropertyId = us.COLLATERALPROPERTYID,
                                      customerName = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == up.CUSTOMERCODE).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME + " (" + x.CUSTOMERCODE + ")").FirstOrDefault(),
                                      accountOfficerName = context.TBL_STAFF.Where(x => x.STAFFID == up.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault(),
                                      accountOfficerCode = context.TBL_STAFF.Where(x => x.STAFFID == up.CREATEDBY).Select(x => x.MISCODE).FirstOrDefault(),
                                      lastValuationDate = us.LASTVALUATIONDATE,
                                      valuationCycle = up.VALUATIONCYCLE,
                                      collateralSummary = up.COLLATERALSUMMARY,
                                      collateralCode = up.COLLATERALCODE,
                                  }).AsEnumerable()
                                   .Select(a => new AllCollateralViewModel
                                   {
                                       collateralPropertyId = a.collateralPropertyId,
                                       customerName = a.customerName,
                                       accountOfficerName = a.accountOfficerName,
                                       accountOfficerCode = a.accountOfficerCode,
                                       lastValuationDate = a.lastValuationDate,
                                       valuationCycle = a.valuationCycle,
                                       nextValuationDate = a.lastValuationDate.AddDays((double)(a.valuationCycle)),
                                       collateralSummary = a.collateralSummary,
                                       collateralCode = a.collateralCode,
                                   }).ToList();

            return valuationData;
        }

        public IEnumerable<SectorLimitAlertViewModel> GetSectorLimitValidationBBD()
        {
            var sectorLimitValidationBBD = (from a in context2.TBL_SECTOR_LIMIT_ALERT
                                            where a.EXPOSURE != null && a.EXPOSURE > 0
                                            select new SectorLimitAlertViewModel
                                            {
                                                sector = a.SECTOR,
                                                bbd = a.BBD,
                                                exposure = a.EXPOSURE
                                            }).ToList();
            return sectorLimitValidationBBD;
        }

        public IEnumerable<SectorLimitAlertViewModel> GetSectorLimitValidationCBD()
        {
            var sectorLimitValidationCBD = (from a in context2.TBL_SECTOR_LIMIT_ALERT
                                            where a.EXPOSURE != null && a.EXPOSURE > 0
                                            select new SectorLimitAlertViewModel
                                            {
                                                sector = a.SECTOR,
                                                cbd = a.CBD,
                                                exposure = a.EXPOSURE
                                            }).ToList();
            return sectorLimitValidationCBD;
        }

        public IEnumerable<SectorLimitAlertViewModel> GetSectorLimitValidationCIBD()
        {
            var sectorLimitValidationCIBD = (from a in context2.TBL_SECTOR_LIMIT_ALERT
                                             where a.EXPOSURE != null && a.EXPOSURE > 0
                                             select new SectorLimitAlertViewModel
                                             {
                                                 sector = a.SECTOR,
                                                 cibd = a.CIBD,
                                                 exposure = a.EXPOSURE
                                             }).ToList();
            return sectorLimitValidationCIBD;
        }

        public IEnumerable<SectorLimitAlertViewModel> GetSectorLimitValidationRBD()
        {
            var sectorLimitValidationRBD = (from a in context2.TBL_SECTOR_LIMIT_ALERT
                                            where a.EXPOSURE != null && a.EXPOSURE > 0
                                            select new SectorLimitAlertViewModel
                                            {
                                                sector = a.SECTOR,
                                                rbd = a.RBD,
                                                exposure = a.EXPOSURE
                                            }).ToList();
            return sectorLimitValidationRBD;
        }

        public IEnumerable<SectorLimitAlertViewModel> GetSectorLimitValidationBank()
        {
            var sectorLimitValidationRBD = (from a in context2.TBL_SECTOR_LIMIT_ALERT
                                            where a.EXPOSURE != null && a.EXPOSURE > 0
                                            select new SectorLimitAlertViewModel
                                            {
                                                sector = a.SECTOR,
                                                bank = a.BANK,
                                                exposure = a.EXPOSURE
                                            }).ToList();
            return sectorLimitValidationRBD;
        }

        public IEnumerable<GlobalExposureViewModel> GetExtentionReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCustomerStockTaking()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCustomerStockTakingReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
               && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
              .Select(d => new GlobalExposureViewModel
              {
                  customerName = d.CUSTOMERNAME,
                  accountOfficerName = d.ACCOUNTOFFICERNAME,
                  accountNumber = d.ACCOUNTNUMBER,
                  branchName = d.GROUPOBLIGORNAME,
                  maturityDate = d.MATURITYDATE,
                  id = d.ID,
                  referenceNumber = d.REFERENCENUMBER,
                  accountOfficerCode = d.ACCOUNTOFFICERCODE,
                  date = d.DATE,
                  customerId = d.CUSTOMERID,
                  groupObligorName = d.GROUPOBLIGORNAME,
                  alphaCode = d.ALPHACODE,
                  productCode = d.PRODUCTCODE,
                  currencyName = d.CURRENCYNAME,
                  productName = d.PRODUCTNAME,
                  facilityType = d.ADJFACILITYTYPE,
                  adjFacilityType = d.ADJFACILITYTYPE,
                  adjFacilityTypeId = d.ADJFACILITYTYPEid,
                  odStatus = d.ODSTATUS,
                  currencyType = d.CURRENCYTYPE,
                  cbnSector = d.CBNSECTOR,
                  cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                  cbnClassification = d.CBNCLASSIFICATION,
                  pwcClassification = d.PWCCLASSIFICATION,
                  ifrsClassification = d.IFRSCLASSIFICATION,
                  tenor = d.TENOR,
                  location = d.LOCATION,
                  bookingDate = d.BOOKINGDATE,
                  valueDate = d.VALUEDATE,
                  maturityBand = d.MATURITYBAND,
                  customerType = d.CUSTOMERTYPE,
                  branchCode = d.BRANCHCODE,
                  obligorRiskRating = d.OBLIGORRISKRATING,
                  lastCrDate = d.LASTCRDATE,
                  productId = d.PRODUCTID,
                  exposureType = d.EXPOSURETYPE,
                  exposureTypeCode = d.EXPOSURETYPECODE,
                  teamCode = d.TEAMCODE,
                  lastCreditAmount = d.LASTCREDITAMOUNT,
                  cardLimit = d.CARDLIMIT,
                  fxrate = d.FXRATE,
                  interestrate = d.INTERESTRATE,
                  principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                  principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                  loanAmounyLcy = d.LOANAMOUNYLCY,
                  loanAmounyTcy = d.LOANAMOUNYTCY,
                  totalExposure = d.TOTALEXPOSURE,
                  impairmentAmount = d.IMPAIRMENTAMOUNT,
                  unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                  unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                  interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                  amountDue = d.AMOUNTDUE,
              }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetTranchPaymentReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<AllCollateralViewModel> GetValuationReminder()
        {

            var valuationData = context.TBL_COLLATERAL_IMMOVE_PROPERTY
                               .Join(context.TBL_COLLATERAL_CUSTOMER.Where(s => s.VALUATIONCYCLE > 0 && s.VALUATIONCYCLE != null)
                               , us => us.COLLATERALCUSTOMERID, up => up.COLLATERALCUSTOMERID, (us, up) =>
                                   new
                                   {
                                       collateralPropertyId = us.COLLATERALPROPERTYID,
                                       customerName = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == up.CUSTOMERCODE).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME + " (" + x.CUSTOMERCODE + ")").FirstOrDefault(),
                                       accountOfficerName = context.TBL_STAFF.Where(x => x.STAFFID == up.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault(),
                                       accountOfficerCode = context.TBL_STAFF.Where(x => x.STAFFID == up.CREATEDBY).Select(x => x.MISCODE).FirstOrDefault(),
                                       lastValuationDate = us.LASTVALUATIONDATE,
                                       valuationCycle = up.VALUATIONCYCLE,
                                       collateralSummary = up.COLLATERALSUMMARY,
                                       collateralCode = up.COLLATERALCODE,
                                   }).AsEnumerable()
                                    .Select(a => new AllCollateralViewModel {
                                        collateralPropertyId = a.collateralPropertyId,
                                        customerName = a.customerName,
                                        accountOfficerName = a.accountOfficerName,
                                        accountOfficerCode = a.accountOfficerCode,
                                        lastValuationDate = a.lastValuationDate,
                                        valuationCycle = a.valuationCycle,
                                        nextValuationDate = a.lastValuationDate.AddDays((double)(a.valuationCycle)),
                                        collateralSummary = a.collateralSummary,
                                        collateralCode = a.collateralCode,
                                    }).ToList();

            return valuationData;
        }

        public IEnumerable<GlobalExposureViewModel> GetInsuranceReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetExtendedFacilityNotification()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetEnhancedDisbursementToDirectorsNotification()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<TBL_LOAN_REVIEW_OPERATION> GetFacilityRestructuredNotification()
        {
            List<int> operationIds = new List<int> {(int)OperationsEnum.OverdraftTenorExtension,
                                                    (int)OperationsEnum.TenorChange,
                                                    (int)OperationsEnum.ContingentLiabilityTenorExtension,
                                                    (int)OperationsEnum.ContractualInterestRateChange,
                                                    (int)OperationsEnum.PaymentDateChange,
                                                    (int)OperationsEnum.PrincipalFrequencyChange,
                                                    (int)OperationsEnum.InterestandPrincipalFrequencyChange,
                                                    (int)OperationsEnum.Fee_chargeChange,
                                                    (int)OperationsEnum.Restructured,
                                                    (int)OperationsEnum.OverdraftInterestRate,
                                                    (int)OperationsEnum.OverdraftTopup };

            var data = context.TBL_LOAN_REVIEW_OPERATION.Where(d => operationIds.Contains(d.OPERATIONTYPEID) && d.OPERATIONCOMPLETED == true && d.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).ToList();
            var records = data.GroupBy(x => x.LOANID).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.LOANID);
            return records;
        }

        public IEnumerable<GlobalExposureViewModel> GetMccAndPpmcDeliverables()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
                && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
               .Select(d => new GlobalExposureViewModel
               {
                   customerName = d.CUSTOMERNAME,
                   accountOfficerName = d.ACCOUNTOFFICERNAME,
                   accountNumber = d.ACCOUNTNUMBER,
                   branchName = d.GROUPOBLIGORNAME,
                   maturityDate = d.MATURITYDATE,
                   id = d.ID,
                   referenceNumber = d.REFERENCENUMBER,
                   accountOfficerCode = d.ACCOUNTOFFICERCODE,
                   date = d.DATE,
                   customerId = d.CUSTOMERID,
                   groupObligorName = d.GROUPOBLIGORNAME,
                   alphaCode = d.ALPHACODE,
                   productCode = d.PRODUCTCODE,
                   currencyName = d.CURRENCYNAME,
                   productName = d.PRODUCTNAME,
                   facilityType = d.ADJFACILITYTYPE,
                   adjFacilityType = d.ADJFACILITYTYPE,
                   adjFacilityTypeId = d.ADJFACILITYTYPEid,
                   odStatus = d.ODSTATUS,
                   currencyType = d.CURRENCYTYPE,
                   cbnSector = d.CBNSECTOR,
                   cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                   cbnClassification = d.CBNCLASSIFICATION,
                   pwcClassification = d.PWCCLASSIFICATION,
                   ifrsClassification = d.IFRSCLASSIFICATION,
                   tenor = d.TENOR,
                   location = d.LOCATION,
                   bookingDate = d.BOOKINGDATE,
                   valueDate = d.VALUEDATE,
                   maturityBand = d.MATURITYBAND,
                   customerType = d.CUSTOMERTYPE,
                   branchCode = d.BRANCHCODE,
                   obligorRiskRating = d.OBLIGORRISKRATING,
                   lastCrDate = d.LASTCRDATE,
                   productId = d.PRODUCTID,
                   exposureType = d.EXPOSURETYPE,
                   exposureTypeCode = d.EXPOSURETYPECODE,
                   teamCode = d.TEAMCODE,
                   lastCreditAmount = d.LASTCREDITAMOUNT,
                   cardLimit = d.CARDLIMIT,
                   fxrate = d.FXRATE,
                   interestrate = d.INTERESTRATE,
                   principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                   principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                   loanAmounyLcy = d.LOANAMOUNYLCY,
                   loanAmounyTcy = d.LOANAMOUNYTCY,
                   totalExposure = d.TOTALEXPOSURE,
                   impairmentAmount = d.IMPAIRMENTAMOUNT,
                   unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                   unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                   interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                   amountDue = d.AMOUNTDUE,
               }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetMccAndPpmcDeliverablesReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetPastDueMccAndPpmcDeliverablesReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetDSRAReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<LoanApplicationViewModel> GetSLAReport()
        {
            return GetPendingLoanApplications();
        }

        #region sla logic
        public IQueryable<LoanApplicationViewModel> GetPendingLoanApplications()
        {

            var operations = context.TBL_LOAN_APPLICATION.Where(x => x.DELETED == false && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                && x.ISADHOCAPPLICATION != true).Select(x => x.OPERATIONID).ToList();

            IQueryable<LoanApplicationViewModel> applications = null;
            var query = new List<LoanApplicationViewModel>();

            query = context.TBL_LOAN_APPLICATION.Where(x =>
                x.DELETED == false && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                && x.ISADHOCAPPLICATION != true
            )
        .OrderByDescending(x => x.LOANAPPLICATIONID)
        .Join(
            context.TBL_APPROVAL_TRAIL.Where(x => (operations.Contains(x.OPERATIONID) || operations.Contains(x.DESTINATIONOPERATIONID ?? 0))
                && x.APPROVALSTATEID != (int)ApprovalState.Ended
                && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                && x.RESPONSESTAFFID == null
                && (x.TOSTAFFID == null)
            ),
            a => a.LOANAPPLICATIONID,
            b => b.TARGETID,
            (a, b) => new { a, b })
        .Select(x => new LoanApplicationViewModel
        {
            loanApplicationId = x.a.LOANAPPLICATIONID,
            applicationReferenceNumber = x.a.APPLICATIONREFERENCENUMBER,
            relatedReferenceNumber = x.a.RELATEDREFERENCENUMBER,
            customerId = x.a.CUSTOMERID,
            branchId = x.a.BRANCHID,
            currencyId = context.TBL_LOAN_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.CURRENCYID)
                                        .FirstOrDefault(),
            productClassId = x.a.PRODUCTCLASSID,
            productClassName = x.a.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

            customerGroupId = x.a.CUSTOMERGROUPID,
            loanTypeId = x.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
            relationshipOfficerId = x.a.RELATIONSHIPOFFICERID,
            relationshipManagerId = x.a.RELATIONSHIPMANAGERID,
            applicationDate = x.a.APPLICATIONDATE,
            systemDateTime = x.a.SYSTEMDATETIME,
            applicationAmount = x.a.APPLICATIONAMOUNT,
            facility = x.a.TBL_LOAN_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() > 1 ? "Multilple(" + x.a.TBL_LOAN_APPLICATION_DETAIL.Where(t => t.DELETED == false).Count() + ")" : context.TBL_LOAN_APPLICATION_DETAIL
                                        .Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED == false)
                                        .Select(s => s.TBL_PRODUCT.PRODUCTNAME.Substring(0, 20))
                                        .FirstOrDefault(),
            approvedAmount = x.a.APPROVEDAMOUNT,
            interestRate = x.a.INTERESTRATE,
            applicationTenor = x.a.APPLICATIONTENOR,
            lastComment = x.b.COMMENT,
            currentApprovalStateId = x.b.APPROVALSTATEID,
            currentApprovalLevelId = x.b.TOAPPROVALLEVELID,
            currentApprovalLevel = x.b.TBL_APPROVAL_LEVEL1.LEVELNAME,
            currentApprovalLevelTypeId = x.b.TBL_APPROVAL_LEVEL1.LEVELTYPEID,
            approvalTrailId = x.b == null ? 0 : x.b.APPROVALTRAILID,
            toStaffId = x.b.TOSTAFFID,
            divisionCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == x.a.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
            divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == x.a.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
            customerBusinessUnitId = context.TBL_CUSTOMER.Where(s => s.CUSTOMERID == x.a.CUSTOMERID).Select(c => c.BUSINESSUNTID).FirstOrDefault(),
            timeIn = x.b.SYSTEMARRIVALDATETIME,
            slaTime = x.b.SLADATETIME,

            loanInformation = x.a.LOANINFORMATION,
            submittedForAppraisal = x.a.SUBMITTEDFORAPPRAISAL,
            customerInfoValidated = x.a.CUSTOMERINFOVALIDATED,
            isRelatedParty = x.a.ISRELATEDPARTY,
            isPoliticallyExposed = x.a.ISPOLITICALLYEXPOSED,
            approvalStatusId = x.b.APPROVALSTATUSID,
            applicationStatusId = x.a.APPLICATIONSTATUSID,
            branchName = x.a.TBL_BRANCH.BRANCHNAME,
            relationshipOfficerName = x.a.TBL_STAFF.FIRSTNAME + " " + x.a.TBL_STAFF.MIDDLENAME + " " + x.a.TBL_STAFF.LASTNAME,
            relationshipManagerName = x.a.TBL_STAFF1.FIRSTNAME + " " + x.a.TBL_STAFF1.MIDDLENAME + " " + x.a.TBL_STAFF1.LASTNAME,
            misCode = x.a.MISCODE,
            loanTypeName = x.a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
            createdBy = x.a.OWNEDBY,
            loanPreliminaryEvaluationId = x.a.LOANPRELIMINARYEVALUATIONID,
            customerGroupName = x.a.CUSTOMERGROUPID.HasValue ? x.a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
            customerName = x.a.CUSTOMERID.HasValue ? x.a.TBL_CUSTOMER.FIRSTNAME + " " + x.a.TBL_CUSTOMER.MIDDLENAME + " " + x.a.TBL_CUSTOMER.LASTNAME : "",
            customerTypeId = x.a.LOANAPPLICATIONTYPEID,
            isInvestmentGrade = x.a.ISINVESTMENTGRADE,
            loantermSheetId = x.a.LOANTERMSHEETID,
            loansWithOthers = x.a.LOANSWITHOTHERS,
            ownershipStructure = x.a.OWNERSHIPSTRUCTURE,
            requireCollateral = x.a.REQUIRECOLLATERAL,
            regionId = x.a.CAPREGIONID,
            collateralDetail = x.a.COLLATERALDETAIL,
            isadhocapplication = x.a.ISADHOCAPPLICATION,
            requireCollateralTypeId = x.a.REQUIRECOLLATERALTYPEID,
            operationId = x.a.OPERATIONID,
            productClassProcessId = x.a.PRODUCT_CLASS_PROCESSID,
            tranchLevelId = x.a.TRANCHEAPPROVAL_LEVELID,
            countryId = context.TBL_COUNTRY.FirstOrDefault().COUNTRYID,
            globalsla = context.TBL_LOAN_APPLICATION_DETAIL
                                            .Where(s => s.LOANAPPLICATIONID == x.a.LOANAPPLICATIONID && s.DELETED == false)
                                            .Select(s => s.TBL_PRODUCT1.TBL_PRODUCT_CLASS.GLOBALSLA)
                                            .FirstOrDefault(),
            currentApprovalLevelSlaInterval = x.b.TBL_APPROVAL_LEVEL1.SLAINTERVAL,
            dateTimeCreated = x.a.DATETIMECREATED,
            apiRequestId = x.a.APIREQUESTID
        }).ToList();

            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.loanApplicationId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault());
            return applications;
        }
        public List<LoanApplicationViewModel> CalculateSLA(List<LoanApplicationViewModel> apps)
        {
            foreach (var app in apps)
            {
                app.slaGlobalStatus = GetSlaGlobalStatus(app);
                app.slaInduvidualStatus = GetSlaInduvidualStatus(app);
                if (app.slaGlobalStatus.ToLower() == "danger" || app.slaInduvidualStatus.ToLower() == "danger")
                {
                    SlaNotification(app);
                }
            }
            return apps;
        }

        private void SlaNotification(LoanApplicationViewModel app)
        {

            AlertsViewModel alert = new AlertsViewModel();
            var ownerRecord = context.TBL_STAFF.Where(s => s.STAFFID == app.responseStaffId).Select(s => s.FIRSTNAME + " " + s.LASTNAME).FirstOrDefault();
            var alertTitle = "SLA/TRT BREACH ON LOAN APPLICATION NUMBER " + app.applicationReferenceNumber;
            var alertTemplate = "The transaction with reference number " + app.applicationReferenceNumber + " and product name " + app.proposedProductName + " which is currently with " + app.currentApprovalLevel + "(" + ownerRecord + ") SLA/TRT has been breach";
            string emailList = GetBusinessUsersEmailsToGroupHead(app.createdBy);

            var message = new TBL_MESSAGE_LOG()
            {
                //MessageId = model.MessageId,
                MESSAGESUBJECT = alertTitle,
                MESSAGEBODY = alertTemplate,
                MESSAGESTATUSID = 1,
                MESSAGETYPEID = 1,
                FROMADDRESS = "fintrakdevops@gmail.com",
                TOADDRESS = emailList,
                DATETIMERECEIVED = DateTime.Now,
                SENDONDATETIME = DateTime.Now,
                OPERATIONMETHOD = "SLABREACH"
            };

            context.TBL_MESSAGE_LOG.Add(message);
            context.SaveChanges();
        }

        private string GetBusinessUsersEmailsToGroupHead(int accountOfficerId)
        {
            string emailList = "";

            var accountOfficer = context.TBL_STAFF.Where(x => x.STAFFID == accountOfficerId && x.DELETED == false).FirstOrDefault();
            if (accountOfficer != null)
            {
                emailList = accountOfficer.EMAIL;
                if (accountOfficer.SUPERVISOR_STAFFID != null)
                {
                    var relationshipManager = context.TBL_STAFF.Where(x => x.STAFFID == accountOfficer.SUPERVISOR_STAFFID && x.DELETED == false).FirstOrDefault();
                    if (relationshipManager != null)
                    {
                        emailList = emailList + ";" + relationshipManager.EMAIL;
                        if (relationshipManager.SUPERVISOR_STAFFID != null)
                        {
                            var zonalHead = context.TBL_STAFF.Where(x => x.STAFFID == relationshipManager.SUPERVISOR_STAFFID && x.DELETED == false).FirstOrDefault();
                            if (zonalHead != null)
                            {
                                emailList = emailList + ";" + zonalHead.EMAIL;

                                var groupHead = context.TBL_STAFF.Where(x => x.STAFFID == zonalHead.SUPERVISOR_STAFFID && x.DELETED == false).FirstOrDefault();

                                if (groupHead != null)
                                {
                                    emailList = emailList + ";" + groupHead.EMAIL;
                                }
                            }
                        }
                    }
                }

            }

            return emailList;
        }
        public string GetSlaInduvidualStatus(LoanApplicationViewModel app)
        {
            float sla = app.currentApprovalLevelSlaInterval;
            //int? elapse = (DateTime.Now - timeIn)?.Hours;
            int? elapse = (int)GetTimeIntervalHours(app.timeIn.Value, DateTime.Now);
            return SlaStatus(sla, elapse);
        }
        public string GetSlaGlobalStatus(LoanApplicationViewModel app)
        {
            float sla = app.globalsla;
            //int? elapse = (DateTime.Now - dateTimeCreated).Hours;
            int? elapse = (int)GetTimeIntervalHours(app.dateTimeCreated, DateTime.Now);
            return SlaStatus(sla, elapse);
        }
        public string SlaStatus(float sla, int? elapse)
        {
            if (sla == 0) return "success";
            if (elapse == 0 || elapse == null) return "success";
            float factor = (float)(elapse / sla) * 100;
            if (factor <= 30) return "success";
            if (factor <= 70) return "warning";
            if (factor <= 100) return "danger";
            return "danger";
        }
        public double GetTimeIntervalHours(DateTime startDate, DateTime endDate)
        {
            double hours = 0;
            var second = new TimeSpan(0, 0, 1);
            var range = GetDateRange(startDate, endDate);
            var test = range.ToList();
            range = FilterHolidaysFromDateIntervals(range);
            var intervals = range.Select(r => new DateTimeAndTimeOfDayViewModel
            {
                dateTime = r
            });
            var list = intervals.ToList();
            //dateTimeAndTimeOfDay = list;
            for (int i = 0; i < list.Count - 1; i++)
            {
                var elapsed = list[i + 1].dateTime.Subtract(list[i].dateTime);
                if (elapsed.Days <= 1)
                {
                    list[i + 1].timeOfDay = elapsed;
                }
                else
                {
                    list[i + 1].timeOfDay = list[i + 1].dateTime.TimeOfDay + second;
                }
            }
            hours = list.Sum(l => l.timeOfDay.TotalHours);
            return hours;
        }
        public IEnumerable<DateTime> FilterHolidaysFromDateIntervals(IEnumerable<DateTime> dateTimes)
        {
            var list = dateTimes.ToList();
            var countryId = context.TBL_COUNTRY.FirstOrDefault().COUNTRYID;
            list = list.FindAll(l => !IsInHolidays(l, countryId));
            return list;
        }
        public IEnumerable<DateTime> GetDateRange(DateTime startDate, DateTime endDate)
        {
            if (endDate < startDate)
                throw new ArgumentException("endDate must be greater than or equal to startDate");
            yield return startDate;

            while (startDate.Date < endDate.Date && startDate.AddDays(1).Date < endDate.Date)
            {
                yield return new DateTime(startDate.AddDays(1).Year, startDate.AddDays(1).Month, startDate.AddDays(1).Day, 23, 59, 59);
                startDate = startDate.AddDays(1);
            }
            yield return endDate;
        }
        public bool IsInHolidays(DateTime date, int countryId)
        {
            List<TBL_PUBLIC_HOLIDAY> holidays;
            holidays = context.TBL_PUBLIC_HOLIDAY.ToList();
            var output = holidays.Any(x => x.DATE == date.Date);
            return output;
        }
        #endregion sla logic


        public IEnumerable<GlobalExposureViewModel> GetOutstandingCreditDocumentation()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetSiteVisitationCustomerReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<ChecklistApprovalViewModel> GetPastDueDeferredDocuments()
        {
            List<int> days = new List<int> { 14, 7, 3, 1 };
            var dataLOS = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                           join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           join c in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals c.LOANCONDITIONID
                           where b.CHECKLISTSTATUSID == (int)CheckListStatusEnum.Deferred
                           && (days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, b.DEFEREDDATE).Value)
                           || (DbFunctions.TruncateTime(b.DEFEREDDATE) < DbFunctions.TruncateTime(DateTime.UtcNow)))
                           select new ChecklistApprovalViewModel()
                           {
                               customerName = a.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                               customerId = a.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID : a.TBL_CUSTOMER.CUSTOMERID,
                               proposedAmount = a.APPROVEDAMOUNT,
                               approvalStatus = b.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                               deferredDate = b.DEFEREDDATE,
                               condition = b.CONDITION,
                               loanApplicationDetailId = a.LOANAPPLICATIONDETAILID,
                               conditionId = b.LOANCONDITIONID,
                               loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                               applicationReferenceNumber = a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                               checklistStatus = b.TBL_CHECKLIST_STATUS.CHECKLISTSTATUSNAME,
                               relationshipOfficerName = a.TBL_LOAN_APPLICATION.TBL_STAFF.FIRSTNAME + " " + a.TBL_LOAN_APPLICATION.TBL_STAFF.FIRSTNAME,
                               relationshipManagerName = a.TBL_LOAN_APPLICATION.TBL_STAFF1.FIRSTNAME + " " + a.TBL_LOAN_APPLICATION.TBL_STAFF1.FIRSTNAME,
                               applicationAmount = a.TBL_LOAN_APPLICATION.APPLICATIONAMOUNT,
                               applicationTenor = a.PROPOSEDTENOR,
                               applicationDate = a.TBL_LOAN_APPLICATION.APPLICATIONDATE,
                               isInvestmentGrade = a.TBL_LOAN_APPLICATION.ISINVESTMENTGRADE,
                               isPoliticallyExposed = a.TBL_LOAN_APPLICATION.ISPOLITICALLYEXPOSED,
                               isRelatedParty = a.TBL_LOAN_APPLICATION.ISRELATEDPARTY,
                               approvalStatusId = b.APPROVALSTATUSID,
                               applicationStatusId = a.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID,
                               submittedForAppraisal = a.TBL_LOAN_APPLICATION.SUBMITTEDFORAPPRAISAL,
                               loanInformation = a.LOANPURPOSE,
                               isLms = c.ISLMS == true,
                               reason = c.DEFERRALREASON,
                               deferredDateOnFinalApproval = c.DEFEREDDATEONFINALAPPROVAL,
                               dateApproved = c.DATEAPPROVED == null ? c.DATETIMECREATED : c.DATEAPPROVED,
                           }).ToList();

            foreach (var x in dataLOS)
            {
                x.deferralDuration = x.deferredDateOnFinalApproval != null ? (x.deferredDateOnFinalApproval - x.dateApproved).Value.Days : 0;

            }
            return dataLOS;
        }

        public IEnumerable<InsurancePolicy> GetExpiredInsurancePolicies()
        {
            List<int> days = new List<int> { 30, 25, 14, 7, 3, 1 };
            var dataLOS = (from i in context.TBL_COLLATERAL_ITEM_POLICY
                           join b in context.TBL_COLLATERAL_CUSTOMER on i.COLLATERALCUSTOMERID equals b.COLLATERALCUSTOMERID
                           where i.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                           && (days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, i.ENDDATE).Value)
                           || (DbFunctions.TruncateTime(i.ENDDATE) < DbFunctions.TruncateTime(DateTime.UtcNow)))
                           select new InsurancePolicy()
                           {
                               referenceNumber = i.POLICYREFERENCENUMBER,
                               insuranceCompanyId = i.INSURANCECOMPANYID,
                               insuranceCompany = context.TBL_INSURANCE_COMPANY.Where(o => o.INSURANCECOMPANYID == i.INSURANCECOMPANYID).Select(o => o.COMPANYNAME).FirstOrDefault(),
                               sumInsured = i.SUMINSURED,
                               startDate = i.STARTDATE,
                               expiryDate = i.ENDDATE,
                               customerName = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == b.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME).FirstOrDefault(),
                               accountOfficer = context.TBL_STAFF.Where(c => c.CREATEDBY == i.CREATEDBY).Select(c => c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME).FirstOrDefault(),
                               insuranceTypeId = i.INSURANCETYPEID,
                               hasExpired = i.HASEXPIRED,
                               policyId = i.POLICYID,
                               inSurPremiumAmount = i.PREMIUMAMOUNT,
                               description = i.DESCRIPTION,
                               premiumPercent = i.PREMIUMPERCENT,
                               insuranceType = context.TBL_INSURANCE_TYPE.Where(ins => ins.INSURANCETYPEID == i.INSURANCETYPEID).Select(ins => ins.INSURANCETYPE).FirstOrDefault(),
                               customerId = (int)i.TBL_COLLATERAL_CUSTOMER.CUSTOMERID,
                           })?.ToList();
            return dataLOS;
        }

        public IEnumerable<GlobalExposureViewModel> GetAMCONCollectionAndStatusReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<AllCollateralViewModel> GetSiteVisitationAccountReminder()
        {
            var valuationData = (from d in context.TBL_COLLATERAL_VISITATION
                                 join a in context.TBL_COLLATERAL_IMMOVE_PROPERTY on d.COLLATERALCUSTOMERID equals a.COLLATERALCUSTOMERID
                                 join c in context.TBL_COLLATERAL_CUSTOMER on d.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                 select new AllCollateralViewModel
                                 {
                                     customerName = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == c.CUSTOMERCODE).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME + " (" + x.CUSTOMERCODE + ")").FirstOrDefault(),
                                     accountOfficerName = context.TBL_STAFF.Where(x => x.STAFFID == c.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME).FirstOrDefault(),
                                     accountOfficerCode = context.TBL_STAFF.Where(x => x.STAFFID == c.CREATEDBY).Select(x => x.MISCODE).FirstOrDefault(),
                                     lastVisitationdate = d.VISITATIONDATE,
                                     valuationCycle = c.VALUATIONCYCLE,
                                     nextVisitationDates = d.NEXTVISITATIONDATE,
                                     collateralSummary = c.COLLATERALSUMMARY,
                                     propertyName = a.PROPERTYNAME,
                                     propertyAddress = a.PROPERTYADDRESS,
                                     collateralCode = c.COLLATERALCODE,
                                 }).ToList();

            return valuationData;
        }

        public IEnumerable<GlobalExposureViewModel> GetEAndSRiskCategorisationDashboard()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetBankExposureEAndSExclusionListReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetGreenBondProceedsUtilizationReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetEandSConditionsPrecedentConfirmationReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetEandSConditionsSubsequentMonitoringReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetEandSCovenantDefaultReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetInvoiceConfirmationReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetStaockValuationReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetEndUseOfFundsReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCollateralVerificationReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCallMemoReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<DeferredChecklistViewModel> GetCreditFileChecklistReminder()
        {
            var condition = (from c in context.TBL_LOAN_CONDITION_PRECEDENT
                             join a in context.TBL_LOAN_APPLICATION_DETAIL on c.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                             where (DbFunctions.DiffDays(DateTime.UtcNow, c.DEFEREDDATE).Value <= c.DEFEREDDAYS
                             && c.ISSUBSEQUENT == false
                             && c.CHECKLISTSTATUSID != null
                             && c.DEFEREDDATE != null
                             && c.DEFEREDDAYS != null)
                             select new DeferredChecklistViewModel()
                             {
                                 customerId = a.CUSTOMERID,
                                 loanApplicationId = a.LOANAPPLICATIONID,
                                 condition = c.CONDITION,
                                 deferredDate = c.DEFEREDDATE,
                                 deferredDays = c.DEFEREDDAYS,
                                 accountOfficer = a.CREATEDBY,
                             }).ToList();
            return condition;
        }

        public IEnumerable<GlobalExposureViewModel> GetPendingCreditApprovalReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetEmployerDeliquencyReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetFacilitiesWithMissedPaymentReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetRunoffs()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCashFlowMonitoringReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetSalaryBackedLoans()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetTODStatusReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCreditFileWithIncompleteDocumentationReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetOutstandingCollateralDocumentation()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetAccountDeferralReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetContigentLiabilityReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
               && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
              .Select(d => new GlobalExposureViewModel
              {
                  customerName = d.CUSTOMERNAME,
                  accountOfficerName = d.ACCOUNTOFFICERNAME,
                  accountNumber = d.ACCOUNTNUMBER,
                  branchName = d.GROUPOBLIGORNAME,
                  maturityDate = d.MATURITYDATE,
                  id = d.ID,
                  referenceNumber = d.REFERENCENUMBER,
                  accountOfficerCode = d.ACCOUNTOFFICERCODE,
                  date = d.DATE,
                  customerId = d.CUSTOMERID,
                  groupObligorName = d.GROUPOBLIGORNAME,
                  alphaCode = d.ALPHACODE,
                  productCode = d.PRODUCTCODE,
                  currencyName = d.CURRENCYNAME,
                  productName = d.PRODUCTNAME,
                  facilityType = d.ADJFACILITYTYPE,
                  adjFacilityType = d.ADJFACILITYTYPE,
                  adjFacilityTypeId = d.ADJFACILITYTYPEid,
                  odStatus = d.ODSTATUS,
                  currencyType = d.CURRENCYTYPE,
                  cbnSector = d.CBNSECTOR,
                  cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                  cbnClassification = d.CBNCLASSIFICATION,
                  pwcClassification = d.PWCCLASSIFICATION,
                  ifrsClassification = d.IFRSCLASSIFICATION,
                  tenor = d.TENOR,
                  location = d.LOCATION,
                  bookingDate = d.BOOKINGDATE,
                  valueDate = d.VALUEDATE,
                  maturityBand = d.MATURITYBAND,
                  customerType = d.CUSTOMERTYPE,
                  branchCode = d.BRANCHCODE,
                  obligorRiskRating = d.OBLIGORRISKRATING,
                  lastCrDate = d.LASTCRDATE,
                  productId = d.PRODUCTID,
                  exposureType = d.EXPOSURETYPE,
                  exposureTypeCode = d.EXPOSURETYPECODE,
                  teamCode = d.TEAMCODE,
                  lastCreditAmount = d.LASTCREDITAMOUNT,
                  cardLimit = d.CARDLIMIT,
                  fxrate = d.FXRATE,
                  interestrate = d.INTERESTRATE,
                  principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                  principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                  loanAmounyLcy = d.LOANAMOUNYLCY,
                  loanAmounyTcy = d.LOANAMOUNYTCY,
                  totalExposure = d.TOTALEXPOSURE,
                  impairmentAmount = d.IMPAIRMENTAMOUNT,
                  unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                  unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                  interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                  amountDue = d.AMOUNTDUE,
              }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetProcessedTransactionReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetPendingAndDeclinedTransactionReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetLoanCovenantsReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetVisitationAndSiteInspectionReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCollateralReleaseAndAccountDeclassification()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetStockMonitoringReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetDSRAReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetPostDisbursementReview()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetMeetUpMonthlyTurnoverRequrements()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetMeetUpMonthlyTurnoverRequrementSMS()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }


        public IEnumerable<GlobalExposureViewModel> GetBreachInCreditKeyMetricsNotification()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetBreachInCreditKeyMetricsNotificationReminder()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetAssignedDeliverableFromCACMeeting()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }
        public IEnumerable<StaffInfoViewModel> GetExpiredFacilityNotification()
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => d.EXPIRYBANDID >= 4)
           .Select(d => d.ACCOUNTOFFICERCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<GlobalExposureViewModel> GetLargeExposureAbove18PercentNotification()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
                && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
               .Select(d => new GlobalExposureViewModel
               {
                   customerName = d.CUSTOMERNAME,
                   accountOfficerName = d.ACCOUNTOFFICERNAME,
                   accountNumber = d.ACCOUNTNUMBER,
                   branchName = d.GROUPOBLIGORNAME,
                   maturityDate = d.MATURITYDATE,
                   id = d.ID,
                   referenceNumber = d.REFERENCENUMBER,
                   accountOfficerCode = d.ACCOUNTOFFICERCODE,
                   date = d.DATE,
                   customerId = d.CUSTOMERID,
                   groupObligorName = d.GROUPOBLIGORNAME,
                   alphaCode = d.ALPHACODE,
                   productCode = d.PRODUCTCODE,
                   currencyName = d.CURRENCYNAME,
                   productName = d.PRODUCTNAME,
                   facilityType = d.ADJFACILITYTYPE,
                   adjFacilityType = d.ADJFACILITYTYPE,
                   adjFacilityTypeId = d.ADJFACILITYTYPEid,
                   odStatus = d.ODSTATUS,
                   currencyType = d.CURRENCYTYPE,
                   cbnSector = d.CBNSECTOR,
                   cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                   cbnClassification = d.CBNCLASSIFICATION,
                   pwcClassification = d.PWCCLASSIFICATION,
                   ifrsClassification = d.IFRSCLASSIFICATION,
                   tenor = d.TENOR,
                   location = d.LOCATION,
                   bookingDate = d.BOOKINGDATE,
                   valueDate = d.VALUEDATE,
                   maturityBand = d.MATURITYBAND,
                   customerType = d.CUSTOMERTYPE,
                   branchCode = d.BRANCHCODE,
                   obligorRiskRating = d.OBLIGORRISKRATING,
                   lastCrDate = d.LASTCRDATE,
                   productId = d.PRODUCTID,
                   exposureType = d.EXPOSURETYPE,
                   exposureTypeCode = d.EXPOSURETYPECODE,
                   teamCode = d.TEAMCODE,
                   lastCreditAmount = d.LASTCREDITAMOUNT,
                   cardLimit = d.CARDLIMIT,
                   fxrate = d.FXRATE,
                   interestrate = d.INTERESTRATE,
                   principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                   principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                   loanAmounyLcy = d.LOANAMOUNYLCY,
                   loanAmounyTcy = d.LOANAMOUNYTCY,
                   totalExposure = d.TOTALEXPOSURE,
                   impairmentAmount = d.IMPAIRMENTAMOUNT,
                   unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                   unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                   interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                   amountDue = d.AMOUNTDUE,
               }).ToList();

            return data;
        }
        public IEnumerable<GlobalExposureViewModel> GetBreachInGeographyLimitNotification()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetBreachInORRLimitNotification()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetBreachInSectorLimitNotification()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetImminentObligationRentalScheduleNotification()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
               && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
              .Select(d => new GlobalExposureViewModel
              {
                  customerName = d.CUSTOMERNAME,
                  accountOfficerName = d.ACCOUNTOFFICERNAME,
                  accountNumber = d.ACCOUNTNUMBER,
                  branchName = d.GROUPOBLIGORNAME,
                  maturityDate = d.MATURITYDATE,
                  id = d.ID,
                  referenceNumber = d.REFERENCENUMBER,
                  accountOfficerCode = d.ACCOUNTOFFICERCODE,
                  date = d.DATE,
                  customerId = d.CUSTOMERID,
                  groupObligorName = d.GROUPOBLIGORNAME,
                  alphaCode = d.ALPHACODE,
                  productCode = d.PRODUCTCODE,
                  currencyName = d.CURRENCYNAME,
                  productName = d.PRODUCTNAME,
                  facilityType = d.ADJFACILITYTYPE,
                  adjFacilityType = d.ADJFACILITYTYPE,
                  adjFacilityTypeId = d.ADJFACILITYTYPEid,
                  odStatus = d.ODSTATUS,
                  currencyType = d.CURRENCYTYPE,
                  cbnSector = d.CBNSECTOR,
                  cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                  cbnClassification = d.CBNCLASSIFICATION,
                  pwcClassification = d.PWCCLASSIFICATION,
                  ifrsClassification = d.IFRSCLASSIFICATION,
                  tenor = d.TENOR,
                  location = d.LOCATION,
                  bookingDate = d.BOOKINGDATE,
                  valueDate = d.VALUEDATE,
                  maturityBand = d.MATURITYBAND,
                  customerType = d.CUSTOMERTYPE,
                  branchCode = d.BRANCHCODE,
                  obligorRiskRating = d.OBLIGORRISKRATING,
                  lastCrDate = d.LASTCRDATE,
                  productId = d.PRODUCTID,
                  exposureType = d.EXPOSURETYPE,
                  exposureTypeCode = d.EXPOSURETYPECODE,
                  teamCode = d.TEAMCODE,
                  lastCreditAmount = d.LASTCREDITAMOUNT,
                  cardLimit = d.CARDLIMIT,
                  fxrate = d.FXRATE,
                  interestrate = d.INTERESTRATE,
                  principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                  principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                  loanAmounyLcy = d.LOANAMOUNYLCY,
                  loanAmounyTcy = d.LOANAMOUNYTCY,
                  totalExposure = d.TOTALEXPOSURE,
                  impairmentAmount = d.IMPAIRMENTAMOUNT,
                  unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                  unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                  interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                  amountDue = d.AMOUNTDUE,
              }).ToList();

            return data;
        }

        public IEnumerable<StaffInfoViewModel> GetImminentObligationMaturityFacilityNotification()
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => d.MATURITYBANDID <= 4)
                      .Select(d => d.ACCOUNTOFFICERCODE).ToList();
            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;

        }

        public IEnumerable<StaffInfoViewModel> GetOverlineFacilityNotification()
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT")
                        .Select(d => d.ACCOUNTOFFICERCODE).ToList();
            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;

        }

        public IEnumerable<StaffInfoViewModel> GetPastDueFacilitiesNotification()
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => d.EXPIRYBANDID > 0)
            .Select(d => d.ACCOUNTOFFICERCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<GlobalExposureViewModel> GetLoanRepaymentReminder()
        {
            List<int> days = new List<int> { 21, 14, 7, 3, 1 };
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
                 maturityDate = d.MATURITYDATE,
                 unPoDaysOverdue = d.UNPODAYSOVERDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<StaffInfoViewModel> GetLoanRepaymentReminderAccountOfficer()
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => d.UNPODAYSOVERDUE > 0)
             .Select(d => d.ACCOUNTOFFICERCODE).ToList();
            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<StaffInfoViewModel> GetOverlineReminder()
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT")
             .Select(d => d.ACCOUNTOFFICERCODE).ToList();
            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<StaffInfoViewModel> GetMaturingObligationsReport()
        {
            List<int> days = new List<int> { 90 };
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d =>
             days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
             .Select(d => d.ACCOUNTOFFICERCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<GlobalExposureViewModel> GetLargeExposureMonitoring()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetUnpaidObligationReminder() // done
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => d.TOTALUNPAIDOBLIGATION > 0)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                maturityDays = DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value,
                customerId = d.CUSTOMERID,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                adjFacilityType = d.ADJFACILITYTYPE,
                amountDue = d.AMOUNTDUE,
                unPoDaysOverdue = d.UNPODAYSOVERDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<StaffInfoViewModel> GetUnpaidObligationReminderAccountOfficer()
        {
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => d.TOTALUNPAIDOBLIGATION > 0)
           .Select(d => d.ACCOUNTOFFICERCODE).ToList();
            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<StaffInfoViewModel> GetStaffLoanPortfolioReport()
        {
            List<int> days = new List<int> { 21, 14, 7, 3, 1 };
            var staffLoanPortfolio = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
            .Select(d => d.ACCOUNTOFFICERCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where staffLoanPortfolio.Contains(s.MISCODE)
                             && s.DELETED == false
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

            return staffList;
        }

        public IEnumerable<GlobalExposureViewModel> GetExStaffLoanPortfolioReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetDigitalLoansReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetImminentMaturitiesAlertSMS()
        {
            List<int> days = new List<int> { 7, 14, 21, 30, 60 };
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetImminentMaturitiesAlertEmail()
        {
            List<int> days = new List<int> { 7, 14, 21, 30, 60 };
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetDelinquentCustomersAlertEmail()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetDelinquentCustomersAlertSMS()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCoreExposureForOneYearPeriodNotification()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetPotentialAndPipelineAssetToBeFinance()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCashBuildupReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCustomersWithNoInflows()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.ADJFACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                interestrate = d.INTERESTRATE,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCustomersWithTurnoverLessthan100()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
             .Select(d => new GlobalExposureViewModel
             {
                 customerName = d.CUSTOMERNAME,
                 accountOfficerName = d.ACCOUNTOFFICERNAME,
                 accountNumber = d.ACCOUNTNUMBER,
                 branchName = d.GROUPOBLIGORNAME,
                 maturityDate = d.MATURITYDATE,
                 id = d.ID,
                 referenceNumber = d.REFERENCENUMBER,
                 accountOfficerCode = d.ACCOUNTOFFICERCODE,
                 date = d.DATE,
                 customerId = d.CUSTOMERID,
                 groupObligorName = d.GROUPOBLIGORNAME,
                 alphaCode = d.ALPHACODE,
                 productCode = d.PRODUCTCODE,
                 currencyName = d.CURRENCYNAME,
                 productName = d.PRODUCTNAME,
                 facilityType = d.ADJFACILITYTYPE,
                 adjFacilityType = d.ADJFACILITYTYPE,
                 adjFacilityTypeId = d.ADJFACILITYTYPEid,
                 odStatus = d.ODSTATUS,
                 currencyType = d.CURRENCYTYPE,
                 cbnSector = d.CBNSECTOR,
                 cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                 cbnClassification = d.CBNCLASSIFICATION,
                 pwcClassification = d.PWCCLASSIFICATION,
                 ifrsClassification = d.IFRSCLASSIFICATION,
                 tenor = d.TENOR,
                 location = d.LOCATION,
                 bookingDate = d.BOOKINGDATE,
                 valueDate = d.VALUEDATE,
                 maturityBand = d.MATURITYBAND,
                 customerType = d.CUSTOMERTYPE,
                 branchCode = d.BRANCHCODE,
                 obligorRiskRating = d.OBLIGORRISKRATING,
                 lastCrDate = d.LASTCRDATE,
                 productId = d.PRODUCTID,
                 exposureType = d.EXPOSURETYPE,
                 exposureTypeCode = d.EXPOSURETYPECODE,
                 teamCode = d.TEAMCODE,
                 lastCreditAmount = d.LASTCREDITAMOUNT,
                 cardLimit = d.CARDLIMIT,
                 fxrate = d.FXRATE,
                 interestrate = d.INTERESTRATE,
                 principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                 principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                 loanAmounyLcy = d.LOANAMOUNYLCY,
                 loanAmounyTcy = d.LOANAMOUNYTCY,
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }


        public IEnumerable<RepaymentAlertViewModel> GetRepaymentDefaultersAlert()
        {
            var dataLoanTerm = (from a in context.TBL_LOAN
                            join b in context.TBL_LOAN_APPLICATION_COLLATERL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                            join c in context.TBL_COLLATERAL_CUSTOMER on b.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                            join d in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals d.COLLATERALTYPEID
                            join e in context.TBL_LOAN_SCHEDULE_PERIODIC on a.TERMLOANID equals e.LOANID
                            where
                            d.COLLATERALTYPEID == (int)CollateralTypeEnum.Gaurantee
                            && a.OUTSTANDINGPRINCIPAL > 0
                            && (DbFunctions.TruncateTime(e.PAYMENTDATE) == DbFunctions.TruncateTime(DateTime.Now))
                            && a.OUTSTANDINGPRINCIPAL > e.ENDPRINCIPALAMOUNT

                            orderby a.TERMLOANID descending
                            select new RepaymentAlertViewModel
                            {
                                customerName = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault() == null ? context.TBL_CUSTOMER_GROUP.Where(cc => cc.CUSTOMERGROUPID == a.CUSTOMERID).Select(cc => cc.GROUPNAME).FirstOrDefault() : context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault(),
                                customerEmail = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.EMAILADDRESS).FirstOrDefault(),
                                outStandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                guarantorEmail = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.EMAILADDRESS).FirstOrDefault(),
                                paymentDate = e.PAYMENTDATE,
                                periodPaymentAmount = e.PERIODPAYMENTAMOUNT,
                                periodPrincipalAmount = e.PERIODPRINCIPALAMOUNT,
                                endPrincipalAmount = e.ENDPRINCIPALAMOUNT,
                                guarantorName = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.FIRSTNAME + " "+ g.MIDDLENAME + " " + g.LASTNAME).FirstOrDefault(),
                            }).ToList();

            var dataLoanRevolving = (from a in context.TBL_LOAN_REVOLVING
                            join b in context.TBL_LOAN_APPLICATION_COLLATERL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                            join c in context.TBL_COLLATERAL_CUSTOMER on b.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                            join d in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals d.COLLATERALTYPEID
                            join e in context.TBL_LOAN_SCHEDULE_PERIODIC on a.REVOLVINGLOANID equals e.LOANID
                            where
                            d.COLLATERALTYPEID == (int)CollateralTypeEnum.Gaurantee
                            && a.OVERDRAFTLIMIT > 0
                            && (DbFunctions.TruncateTime(e.PAYMENTDATE) == DbFunctions.TruncateTime(DateTime.Now))
                            && a.OVERDRAFTLIMIT > e.ENDPRINCIPALAMOUNT

                            orderby a.REVOLVINGLOANID descending
                            select new RepaymentAlertViewModel
                            {
                                customerName = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault() == null ? context.TBL_CUSTOMER_GROUP.Where(cc => cc.CUSTOMERGROUPID == a.CUSTOMERID).Select(cc => cc.GROUPNAME).FirstOrDefault() : context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault(),
                                customerEmail = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.EMAILADDRESS).FirstOrDefault(),
                                outStandingPrincipal = a.OVERDRAFTLIMIT,
                                guarantorEmail = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.EMAILADDRESS).FirstOrDefault(),
                                paymentDate = e.PAYMENTDATE,
                                periodPaymentAmount = e.PERIODPAYMENTAMOUNT,
                                periodPrincipalAmount = e.PERIODPRINCIPALAMOUNT,
                                endPrincipalAmount = e.ENDPRINCIPALAMOUNT,
                                guarantorName = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.FIRSTNAME + " " + g.MIDDLENAME + " " + g.LASTNAME).FirstOrDefault(),
                            }).ToList();

            var dataLoanContingent = (from a in context.TBL_LOAN_CONTINGENT
                            join b in context.TBL_LOAN_APPLICATION_COLLATERL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                            join c in context.TBL_COLLATERAL_CUSTOMER on b.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                            join d in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals d.COLLATERALTYPEID
                            join e in context.TBL_LOAN_SCHEDULE_PERIODIC on a.CONTINGENTLOANID equals e.LOANID
                            where
                            d.COLLATERALTYPEID == (int)CollateralTypeEnum.Gaurantee
                            && a.CONTINGENTAMOUNT > 0
                            && (DbFunctions.TruncateTime(e.PAYMENTDATE) == DbFunctions.TruncateTime(DateTime.Now))
                            && a.CONTINGENTAMOUNT > e.ENDPRINCIPALAMOUNT

                            orderby a.CONTINGENTLOANID descending
                            select new RepaymentAlertViewModel
                            {
                                customerName = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault() == null ? context.TBL_CUSTOMER_GROUP.Where(cc => cc.CUSTOMERGROUPID == a.CUSTOMERID).Select(cc => cc.GROUPNAME).FirstOrDefault() : context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault(),
                                customerEmail = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.EMAILADDRESS).FirstOrDefault(),
                                outStandingPrincipal = a.CONTINGENTAMOUNT,
                                guarantorEmail = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.EMAILADDRESS).FirstOrDefault(),
                                paymentDate = e.PAYMENTDATE,
                                periodPaymentAmount = e.PERIODPAYMENTAMOUNT,
                                periodPrincipalAmount = e.PERIODPRINCIPALAMOUNT,
                                endPrincipalAmount = e.ENDPRINCIPALAMOUNT,
                                guarantorName = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.FIRSTNAME + " " + g.MIDDLENAME + " " + g.LASTNAME).FirstOrDefault(),
                            }).ToList();

            var data = dataLoanTerm.Union(dataLoanRevolving).Union(dataLoanContingent);
            var dataLoan = data.ToList();

            return dataLoan;
        }

        public IEnumerable<RepaymentAlertViewModel> GetRepaymentPayDownAlert()
        {
            var dataLoanTerm = (from a in context.TBL_LOAN
                            join b in context.TBL_LOAN_APPLICATION_COLLATERL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                            join c in context.TBL_COLLATERAL_CUSTOMER on b.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                            join d in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals d.COLLATERALTYPEID
                            join e in context.TBL_LOAN_SCHEDULE_PERIODIC on a.TERMLOANID equals e.LOANID
                            where
                            d.COLLATERALTYPEID == (int)CollateralTypeEnum.Gaurantee
                            && (DbFunctions.TruncateTime(e.PAYMENTDATE) == DbFunctions.TruncateTime(DateTime.Now))
                            && a.OUTSTANDINGPRINCIPAL == e.ENDPRINCIPALAMOUNT
                            && e.PERIODPAYMENTAMOUNT > 0

                            orderby a.TERMLOANID descending
                            select new RepaymentAlertViewModel
                            {
                                customerName = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault() == null ? context.TBL_CUSTOMER_GROUP.Where(cc => cc.CUSTOMERGROUPID == a.CUSTOMERID).Select(cc => cc.GROUPNAME).FirstOrDefault() : context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault(),
                                customerEmail = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.EMAILADDRESS).FirstOrDefault(),
                                outStandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                guarantorEmail = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.EMAILADDRESS).FirstOrDefault(),
                                paymentDate = e.PAYMENTDATE,
                                periodPaymentAmount = e.PERIODPAYMENTAMOUNT,
                                periodPrincipalAmount = e.PERIODPRINCIPALAMOUNT,
                                endPrincipalAmount = e.ENDPRINCIPALAMOUNT,
                                guarantorName = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.FIRSTNAME + " " + g.MIDDLENAME + " " + g.LASTNAME).FirstOrDefault(),
                            }).ToList();

            var dataLoanRevolving = (from a in context.TBL_LOAN_REVOLVING
                            join b in context.TBL_LOAN_APPLICATION_COLLATERL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                            join c in context.TBL_COLLATERAL_CUSTOMER on b.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                            join d in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals d.COLLATERALTYPEID
                            join e in context.TBL_LOAN_SCHEDULE_PERIODIC on a.REVOLVINGLOANID equals e.LOANID
                            where
                            d.COLLATERALTYPEID == (int)CollateralTypeEnum.Gaurantee
                            && (DbFunctions.TruncateTime(e.PAYMENTDATE) == DbFunctions.TruncateTime(DateTime.Now))
                            && a.OVERDRAFTLIMIT == e.ENDPRINCIPALAMOUNT
                            && e.PERIODPAYMENTAMOUNT > 0

                            orderby a.REVOLVINGLOANID descending
                            select new RepaymentAlertViewModel
                            {
                                customerName = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault() == null ? context.TBL_CUSTOMER_GROUP.Where(cc => cc.CUSTOMERGROUPID == a.CUSTOMERID).Select(cc => cc.GROUPNAME).FirstOrDefault() : context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault(),
                                customerEmail = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.EMAILADDRESS).FirstOrDefault(),
                                outStandingPrincipal = a.OVERDRAFTLIMIT,
                                guarantorEmail = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.EMAILADDRESS).FirstOrDefault(),
                                paymentDate = e.PAYMENTDATE,
                                periodPaymentAmount = e.PERIODPAYMENTAMOUNT,
                                periodPrincipalAmount = e.PERIODPRINCIPALAMOUNT,
                                endPrincipalAmount = e.ENDPRINCIPALAMOUNT,
                                guarantorName = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.FIRSTNAME + " " + g.MIDDLENAME + " " + g.LASTNAME).FirstOrDefault(),
                            }).ToList();

            var dataLoanContingent = (from a in context.TBL_LOAN_CONTINGENT
                            join b in context.TBL_LOAN_APPLICATION_COLLATERL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                            join c in context.TBL_COLLATERAL_CUSTOMER on b.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                            join d in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals d.COLLATERALTYPEID
                            join e in context.TBL_LOAN_SCHEDULE_PERIODIC on a.CONTINGENTLOANID equals e.LOANID
                            where
                            d.COLLATERALTYPEID == (int)CollateralTypeEnum.Gaurantee
                            && (DbFunctions.TruncateTime(e.PAYMENTDATE) == DbFunctions.TruncateTime(DateTime.Now))
                            && a.CONTINGENTAMOUNT == e.ENDPRINCIPALAMOUNT
                            && e.PERIODPAYMENTAMOUNT > 0

                            orderby a.CONTINGENTLOANID descending
                            select new RepaymentAlertViewModel
                            {
                                customerName = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault() == null ? context.TBL_CUSTOMER_GROUP.Where(cc => cc.CUSTOMERGROUPID == a.CUSTOMERID).Select(cc => cc.GROUPNAME).FirstOrDefault() : context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.FIRSTNAME + "" + cc.MIDDLENAME + "" + cc.LASTNAME).FirstOrDefault(),
                                customerEmail = context.TBL_CUSTOMER.Where(cc => cc.CUSTOMERID == a.CUSTOMERID).Select(cc => cc.EMAILADDRESS).FirstOrDefault(),
                                outStandingPrincipal = a.CONTINGENTAMOUNT,
                                guarantorEmail = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.EMAILADDRESS).FirstOrDefault(),
                                paymentDate = e.PAYMENTDATE,
                                periodPaymentAmount = e.PERIODPAYMENTAMOUNT,
                                periodPrincipalAmount = e.PERIODPRINCIPALAMOUNT,
                                endPrincipalAmount = e.ENDPRINCIPALAMOUNT,
                                guarantorName = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == b.COLLATERALCUSTOMERID).Select(g => g.FIRSTNAME + " " + g.MIDDLENAME + " " + g.LASTNAME).FirstOrDefault(),
                            }).ToList();

            var data = dataLoanTerm.Union(dataLoanRevolving).Union(dataLoanContingent);
            var dataLoan = data.ToList();
            return dataLoan;
        }


        private IEnumerable<GlobalExposureApplicationViewModel> GetLoanOperationRecoveryAnalysisInternal(string custmerId, string customerCode)
        {
            var applicationDate = _genSetup.GetApplicationDate();
            var loansId = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(x => x.DELETED == false).Select(x => x.LOANREFERENCE).ToList();

            var exposureNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         !loansId.Contains(ln.REFERENCENUMBER)
                                         && ln.NPL != null
                                         && ln.UNPODAYSOVERDUE >= 30
                                         && ln.UNPODAYSOVERDUE <= 360
                                         && ln.CUSTOMERID == customerCode
                                         && ln.CBNCLASSIFICATION.Trim() != "PERFORMING"

                                         orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             loanId = ln.ID,
                                             customerCode = ln.CUSTOMERID,
                                             productCode = ln.PRODUCTID,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                             customerName = ln.CUSTOMERNAME,
                                             productName = ln.PRODUCTNAME,
                                             relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                             exposureType = ln.EXPOSURETYPE,
                                             expiryBand = ln.EXPIRINGBAND,
                                             divisionName = ln.DIVISIONNAME,
                                             totalAmountRecovery = (decimal)ln.TOTALEXPOSURE,
                                             totalUnsettledAmount = ln.TOTALUNSETTLEDAMOUNT,
                                             dpdExposure = ln.UNPODAYSOVERDUE,
                                             loanCategory = ln.CBNCLASSIFICATION
                                         }).ToList();
            foreach (var xx in exposureNonPerforming)
            {
                xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
                xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
            }

            var exposureDigitalNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         !loansId.Contains(ln.REFERENCENUMBER)
                                         && ln.NPL != null
                                         && ln.UNPODAYSOVERDUE >= 30
                                         && ln.UNPODAYSOVERDUE <= 360
                                         && ln.CUSTOMERID == customerCode
                                         && ln.CBNCLASSIFICATION.Trim() != "PERFORMING"

                                                orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             loanId = ln.ID,
                                             customerCode = ln.CUSTOMERID,
                                             productCode = ln.PRODUCTID,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                             customerName = ln.CUSTOMERNAME,
                                             productName = ln.PRODUCTNAME,
                                             relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                             exposureType = ln.EXPOSURETYPE,
                                             expiryBand = ln.EXPIRINGBAND,
                                             divisionName = ln.DIVISIONNAME,
                                             totalAmountRecovery = (decimal)ln.TOTALEXPOSURE,
                                             totalUnsettledAmount = ln.TOTALUNSETTLEDAMOUNT,
                                             dpdExposure = ln.UNPODAYSOVERDUE,
                                             loanCategory = ln.CBNCLASSIFICATION
                                         }).ToList();
            foreach (var xx in exposureDigitalNonPerforming)
            {
                xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
                xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
            }

            /*
            var dataLoanNonPerforming = (from ln in context.TBL_LOAN
                                         join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                                         where
                                         cu.CUSTOMERCODE == custmerId
                                         && !loansId.Contains(ln.LOANREFERENCENUMBER)
                                         && pr.EXCLUDEFROMLITIGATION == false
                                         && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                         && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                         && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value > 30
                                         && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value <= 360

                                         orderby ln.DATETIMECREATED descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             creditAppraisalLoanApplicationId = lp.LOANAPPLICATIONID,
                                             creditAppraisalOperationId = lp.OPERATIONID,
                                             loanApplicationDetailId = ld.LOANAPPLICATIONDETAILID,
                                             loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                             loanId = ln.TERMLOANID,
                                             customerId = cu.CUSTOMERCODE,
                                             productId = ln.PRODUCTID,
                                             productClassId = pr.PRODUCTCLASSID,
                                             productTypeId = pr.PRODUCTTYPEID,
                                             casaAccountId = ln.CASAACCOUNTID,
                                             loanCategory = "Non Performing",
                                             casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                             casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                             branchId = ln.BRANCHID,
                                             amount = ld.APPROVEDAMOUNT,
                                             operationTypeName = tt.OPERATIONNAME,
                                             loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                             applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                             principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                                             pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                             interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                                             interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                             principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                                             interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                                             relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                             relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                             misCode = ln.MISCODE,
                                             teamMiscode = ln.TEAMMISCODE,
                                             interestRate = ln.INTERESTRATE,
                                             effectiveDate = ln.EFFECTIVEDATE,
                                             maturityDate = ln.MATURITYDATE,
                                             bookingDate = ln.BOOKINGDATE,
                                             totalUnsettledAmount = (ln.OUTSTANDINGPRINCIPAL + ln.PASTDUEINTEREST + ln.PASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL) + (from a in context.TBL_LOAN_SCHEDULE_DAILY where a.TBL_LOAN.TERMLOANID == ln.TERMLOANID && a.DATE == applicationDate select a.ACCRUEDINTEREST).FirstOrDefault(),
                                             totalAmountRecovery = (ln.OUTSTANDINGPRINCIPAL + ln.PASTDUEINTEREST + ln.PASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL) + (from a in context.TBL_LOAN_SCHEDULE_DAILY where a.TBL_LOAN.TERMLOANID == ln.TERMLOANID && a.DATE == applicationDate select a.ACCRUEDINTEREST).FirstOrDefault(),
                                             principalAmount = ln.OUTSTANDINGPRINCIPAL,
                                             principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                                             interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                                             approvedBy = (int)ln.APPROVEDBY,
                                             approverComment = ln.APPROVERCOMMENT,
                                             dateApproved = ln.DATEAPPROVED,
                                             loanStatusId = ln.LOANSTATUSID,
                                             scheduleTypeId = ln.SCHEDULETYPEID,
                                             isDisbursed = ln.ISDISBURSED,
                                             disbursedBy = (int)ln.DISBURSEDBY,
                                             disburserComment = ln.DISBURSERCOMMENT,
                                             disburseDate = ln.DISBURSEDATE,
                                             customerGroupId = lp.CUSTOMERGROUPID,
                                             operationId = ln.OPERATIONID,
                                             loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                             equityContribution = ln.EQUITYCONTRIBUTION,
                                             subSectorId = ln.SUBSECTORID,
                                             subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                             sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                             firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                             firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                             outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                                             principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                                             principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                                             fixedPrincipal = ln.FIXEDPRINCIPAL,
                                             profileLoan = ln.PROFILELOAN,
                                             dischargeLetter = ln.DISCHARGELETTER,
                                             suspendInterest = ln.SUSPENDINTEREST,
                                             scheduled = ln.ISSCHEDULEDPREPAYMENT,
                                             isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                                             scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                                             scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,
                                             customerCode = cu.CUSTOMERCODE,
                                             productAccountNumber = ch.ACCOUNTCODE,
                                             productAccountName = ch.ACCOUNTNAME,
                                             loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                             customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                             currencyId = ln.CURRENCYID,
                                             branchName = br.BRANCHNAME,
                                             relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                             relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                             productName = pr.PRODUCTNAME,
                                             comment = "",
                                             approvedAmount = ld.APPROVEDAMOUNT,
                                             creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                             pastDueInterest = ln.PASTDUEINTEREST,
                                             pastDuePrincipal = ln.PASTDUEPRINCIPAL,
                                             interestOnPastDueInterest = ln.INTERESTONPASTDUEINTEREST,
                                             interestOnPastDuePrincipal = ln.INTERESTONPASTDUEPRINCIPAL,
                                             outstandingInterest = ln.OUTSTANDINGINTEREST,
                                             accruedInterest = (from a in context.TBL_LOAN_SCHEDULE_DAILY where a.TBL_LOAN.TERMLOANID == ln.TERMLOANID && a.DATE == applicationDate select a.ACCRUEDINTEREST).FirstOrDefault(),
                                             dateTimeCreated = ln.DATETIMECREATED,
                                         }).ToList();

            var dataRevolvingNonPerforming = (from ln in context.TBL_LOAN_REVOLVING
                                              join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                              join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                              join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                              join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                              join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                              join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                              join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                              join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                              join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                              where
                                              cu.CUSTOMERCODE == custmerId
                                              && !loansId.Contains(ln.LOANREFERENCENUMBER)
                                              && pr.EXCLUDEFROMLITIGATION == false
                                              && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                              && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                              && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value > 30
                                              && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value <= 360

                                              orderby ln.DATETIMECREATED descending
                                              select new GlobalExposureApplicationViewModel
                                              {
                                                  loanApplicationDetailId = ld.LOANAPPLICATIONDETAILID,
                                                  creditAppraisalLoanApplicationId = lp.LOANAPPLICATIONID,
                                                  creditAppraisalOperationId = lp.OPERATIONID,
                                                  loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                                  loanId = ln.REVOLVINGLOANID,
                                                  customerId = cu.CUSTOMERCODE,
                                                  productId = ln.PRODUCTID,
                                                  productClassId = pr.PRODUCTCLASSID,
                                                  productTypeId = pr.PRODUCTTYPEID,
                                                  casaAccountId = ln.CASAACCOUNTID,
                                                  casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                  casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                                  branchId = ln.BRANCHID,
                                                  totalUnsettledAmount = (ln.PASTDUEPRINCIPAL + ln.PASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.PENALCHARGEAMOUNT),
                                                  totalAmountRecovery = (ln.PASTDUEPRINCIPAL + ln.PASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.PENALCHARGEAMOUNT),
                                                  loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                                  applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                                  relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                                  relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                                  misCode = ln.MISCODE,
                                                  teamMiscode = ln.TEAMMISCODE,
                                                  interestRate = ln.INTERESTRATE,
                                                  effectiveDate = ln.EFFECTIVEDATE,
                                                  maturityDate = ln.MATURITYDATE,
                                                  bookingDate = ln.BOOKINGDATE,
                                                  loanCategory = "Non Performing",
                                                  operationTypeName = tt.OPERATIONNAME,
                                                  approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == ln.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                                  approvedBy = (int)ln.APPROVEDBY,
                                                  approverComment = ln.APPROVERCOMMENT,
                                                  dateApproved = ln.DATEAPPROVED,
                                                  loanStatusId = ln.LOANSTATUSID,
                                                  isDisbursed = ln.ISDISBURSED,
                                                  disburserComment = ln.DISBURSERCOMMENT,
                                                  disburseDate = ln.DISBURSEDATE,
                                                  customerGroupId = lp.CUSTOMERGROUPID,
                                                  operationId = ln.OPERATIONID,
                                                  loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                                  subSectorId = ln.SUBSECTORID,
                                                  subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                                  sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                                  dischargeLetter = ln.DISCHARGELETTER,
                                                  suspendInterest = ln.SUSPENDINTEREST,
                                                  customerCode = cu.CUSTOMERCODE,
                                                  loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                                  customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                  currencyId = ln.CURRENCYID,
                                                  branchName = br.BRANCHNAME,
                                                  relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                  relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                                  productName = pr.PRODUCTNAME,
                                                  comment = "",
                                                  productAccountNumber = "N/A",
                                                  productAccountName = "N/A",
                                                  approvedAmount = ld.APPROVEDAMOUNT,
                                                  creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                              }).ToList();

            var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.loanId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();
            var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.loanId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();

            var allData = termLoanDataNon.Union(revolvingLoanDataNon);*/
            var allData = exposureNonPerforming.Union(exposureDigitalNonPerforming);
            var data = allData.GroupBy(x => x.loanReferenceNumber).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();
            return data;
        }
        private IEnumerable<GlobalExposureApplicationViewModel> GetLoanOperationRecoveryAnalysisExternal(string custmerId, string customerCode)
        {
            var applicationDate = _genSetup.GetApplicationDate();
            var loansId = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(x => x.DELETED == false).Select(x => x.LOANREFERENCE).ToList();

            var exposureNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         !loansId.Contains(ln.REFERENCENUMBER)
                                         && ln.NPL != null
                                         && ln.UNPODAYSOVERDUE > 360
                                         && ln.CUSTOMERID == customerCode
                                         && ln.CBNCLASSIFICATION.Trim() != "PERFORMING"

                                         orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             loanId = ln.ID,
                                             customerCode = ln.CUSTOMERID,
                                             productCode = ln.PRODUCTID,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                             customerName = ln.CUSTOMERNAME,
                                             productName = ln.PRODUCTNAME,
                                             relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                             exposureType = ln.EXPOSURETYPE,
                                             expiryBand = ln.EXPIRINGBAND,
                                             divisionName = ln.DIVISIONNAME,
                                             totalAmountRecovery = (decimal)ln.TOTALEXPOSURE,
                                             totalUnsettledAmount = ln.TOTALUNPAIDOBLIGATION,
                                             dpdExposure = ln.UNPODAYSOVERDUE,
                                             loanCategory = ln.CBNCLASSIFICATION
                                         }).ToList();

            foreach (var xx in exposureNonPerforming)
            {
                xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
                xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
            }

            var exposureDigitalNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         !loansId.Contains(ln.REFERENCENUMBER)
                                         && ln.NPL != null
                                         && ln.UNPODAYSOVERDUE > 360
                                         && ln.CUSTOMERID == customerCode
                                         && ln.CBNCLASSIFICATION.Trim() != "PERFORMING"

                                                orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             loanId = ln.ID,
                                             customerCode = ln.CUSTOMERID,
                                             productCode = ln.PRODUCTID,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                             customerName = ln.CUSTOMERNAME,
                                             productName = ln.PRODUCTNAME,
                                             relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                             exposureType = ln.EXPOSURETYPE,
                                             expiryBand = ln.EXPIRINGBAND,
                                             divisionName = ln.DIVISIONNAME,
                                             totalAmountRecovery = (decimal)ln.TOTALEXPOSURE,
                                             totalUnsettledAmount = ln.TOTALUNSETTLEDAMOUNT,
                                             dpdExposure = ln.UNPODAYSOVERDUE,
                                             loanCategory = ln.CBNCLASSIFICATION
                                         }).ToList();

            foreach (var xx in exposureDigitalNonPerforming)
            {
                xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
                xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
            }

            /*var dataLoanNonPerforming = (from ln in context.TBL_LOAN
                                         join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                                         where
                                         cu.CUSTOMERCODE == custmerId
                                         && !loansId.Contains(ln.LOANREFERENCENUMBER)
                                         && pr.EXCLUDEFROMLITIGATION == false
                                         && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                         && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                         && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value > 360

                                         orderby ln.DATETIMECREATED descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             creditAppraisalLoanApplicationId = lp.LOANAPPLICATIONID,
                                             creditAppraisalOperationId = lp.OPERATIONID,
                                             loanApplicationDetailId = ld.LOANAPPLICATIONDETAILID,
                                             loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                             loanId = ln.TERMLOANID,
                                             customerId = cu.CUSTOMERCODE,
                                             productId = ln.PRODUCTID,
                                             productClassId = pr.PRODUCTCLASSID,
                                             productTypeId = pr.PRODUCTTYPEID,
                                             casaAccountId = ln.CASAACCOUNTID,
                                             loanCategory = "Non Performing",
                                             casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                             casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                             branchId = ln.BRANCHID,
                                             amount = ld.APPROVEDAMOUNT,
                                             operationTypeName = tt.OPERATIONNAME,
                                             loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                             applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                             principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                                             pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                             interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                                             interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                             principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                                             interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                                             relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                             relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                             misCode = ln.MISCODE,
                                             teamMiscode = ln.TEAMMISCODE,
                                             interestRate = ln.INTERESTRATE,
                                             effectiveDate = ln.EFFECTIVEDATE,
                                             maturityDate = ln.MATURITYDATE,
                                             bookingDate = ln.BOOKINGDATE,
                                             totalUnsettledAmount = (ln.OUTSTANDINGPRINCIPAL + ln.PASTDUEINTEREST + ln.PASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL) + (from a in context.TBL_LOAN_SCHEDULE_DAILY where a.TBL_LOAN.TERMLOANID == ln.TERMLOANID && a.DATE == applicationDate select a.ACCRUEDINTEREST).FirstOrDefault(),
                                             totalAmountRecovery = (ln.OUTSTANDINGPRINCIPAL + ln.PASTDUEINTEREST + ln.PASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL) + (from a in context.TBL_LOAN_SCHEDULE_DAILY where a.TBL_LOAN.TERMLOANID == ln.TERMLOANID && a.DATE == applicationDate select a.ACCRUEDINTEREST).FirstOrDefault(),
                                             principalAmount = ln.OUTSTANDINGPRINCIPAL,
                                             principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                                             interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                                             approvedBy = (int)ln.APPROVEDBY,
                                             approverComment = ln.APPROVERCOMMENT,
                                             dateApproved = ln.DATEAPPROVED,
                                             loanStatusId = ln.LOANSTATUSID,
                                             scheduleTypeId = ln.SCHEDULETYPEID,
                                             isDisbursed = ln.ISDISBURSED,
                                             disbursedBy = (int)ln.DISBURSEDBY,
                                             disburserComment = ln.DISBURSERCOMMENT,
                                             disburseDate = ln.DISBURSEDATE,
                                             customerGroupId = lp.CUSTOMERGROUPID,
                                             operationId = ln.OPERATIONID,
                                             loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                             equityContribution = ln.EQUITYCONTRIBUTION,
                                             subSectorId = ln.SUBSECTORID,
                                             subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                             sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                             firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                             firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                             outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                                             principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                                             principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                                             fixedPrincipal = ln.FIXEDPRINCIPAL,
                                             profileLoan = ln.PROFILELOAN,
                                             dischargeLetter = ln.DISCHARGELETTER,
                                             suspendInterest = ln.SUSPENDINTEREST,
                                             scheduled = ln.ISSCHEDULEDPREPAYMENT,
                                             isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                                             scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                                             scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,
                                             customerCode = cu.CUSTOMERCODE,
                                             productAccountNumber = ch.ACCOUNTCODE,
                                             productAccountName = ch.ACCOUNTNAME,
                                             loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                             customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                             currencyId = ln.CURRENCYID,
                                             branchName = br.BRANCHNAME,
                                             relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                             relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                             productName = pr.PRODUCTNAME,
                                             comment = "",
                                             approvedAmount = ld.APPROVEDAMOUNT,
                                             creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                             pastDueInterest = ln.PASTDUEINTEREST,
                                             pastDuePrincipal = ln.PASTDUEPRINCIPAL,
                                             interestOnPastDueInterest = ln.INTERESTONPASTDUEINTEREST,
                                             interestOnPastDuePrincipal = ln.INTERESTONPASTDUEPRINCIPAL,
                                             outstandingInterest = ln.OUTSTANDINGINTEREST,
                                             accruedInterest = (from a in context.TBL_LOAN_SCHEDULE_DAILY where a.TBL_LOAN.TERMLOANID == ln.TERMLOANID && a.DATE == applicationDate select a.ACCRUEDINTEREST).FirstOrDefault(),
                                             dateTimeCreated = ln.DATETIMECREATED,
                                         }).ToList();

            var dataRevolvingNonPerforming = (from ln in context.TBL_LOAN_REVOLVING
                                              join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                              join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                              join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                              join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                              join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                              join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                              join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                              join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                              join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                              where
                                              cu.CUSTOMERCODE == custmerId
                                              && !loansId.Contains(ln.LOANREFERENCENUMBER)
                                              && pr.EXCLUDEFROMLITIGATION == false
                                              && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                              && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                              && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value > 360

                                              orderby ln.DATETIMECREATED descending
                                              select new GlobalExposureApplicationViewModel
                                              {
                                                  loanApplicationDetailId = ld.LOANAPPLICATIONDETAILID,
                                                  creditAppraisalLoanApplicationId = lp.LOANAPPLICATIONID,
                                                  creditAppraisalOperationId = lp.OPERATIONID,
                                                  loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                                  loanId = ln.REVOLVINGLOANID,
                                                  customerId = cu.CUSTOMERCODE,
                                                  productId = ln.PRODUCTID,
                                                  productClassId = pr.PRODUCTCLASSID,
                                                  productTypeId = pr.PRODUCTTYPEID,
                                                  casaAccountId = ln.CASAACCOUNTID,
                                                  casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                  casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                                  branchId = ln.BRANCHID,
                                                  totalUnsettledAmount = (ln.PASTDUEPRINCIPAL + ln.PASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.PENALCHARGEAMOUNT),
                                                  totalAmountRecovery = (ln.PASTDUEPRINCIPAL + ln.PASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.PENALCHARGEAMOUNT),
                                                  loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                                  applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                                  relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                                  relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                                  misCode = ln.MISCODE,
                                                  teamMiscode = ln.TEAMMISCODE,
                                                  interestRate = ln.INTERESTRATE,
                                                  effectiveDate = ln.EFFECTIVEDATE,
                                                  maturityDate = ln.MATURITYDATE,
                                                  bookingDate = ln.BOOKINGDATE,
                                                  loanCategory = "Non Performing",
                                                  operationTypeName = tt.OPERATIONNAME,
                                                  approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == ln.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                                  approvedBy = (int)ln.APPROVEDBY,
                                                  approverComment = ln.APPROVERCOMMENT,
                                                  dateApproved = ln.DATEAPPROVED,
                                                  loanStatusId = ln.LOANSTATUSID,
                                                  isDisbursed = ln.ISDISBURSED,
                                                  disburserComment = ln.DISBURSERCOMMENT,
                                                  disburseDate = ln.DISBURSEDATE,
                                                  customerGroupId = lp.CUSTOMERGROUPID,
                                                  operationId = ln.OPERATIONID,
                                                  loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                                  subSectorId = ln.SUBSECTORID,
                                                  subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                                  sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                                  dischargeLetter = ln.DISCHARGELETTER,
                                                  suspendInterest = ln.SUSPENDINTEREST,
                                                  customerCode = cu.CUSTOMERCODE,
                                                  loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                                  customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                  currencyId = ln.CURRENCYID,
                                                  branchName = br.BRANCHNAME,
                                                  relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                  relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                                  productName = pr.PRODUCTNAME,
                                                  comment = "",
                                                  productAccountNumber = "N/A",
                                                  productAccountName = "N/A",
                                                  approvedAmount = ld.APPROVEDAMOUNT,
                                                  creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                              }).ToList();
            
            var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.loanId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();
            var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.loanId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();
            
            var allData = termLoanDataNon.Union(revolvingLoanDataNon);*/
            var allData = exposureNonPerforming.Union(exposureDigitalNonPerforming);
            var data = allData.GroupBy(x => x.loanReferenceNumber).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();
            return data;
        }
        private IEnumerable<GlobalExposureApplicationViewModel> GetQuarterlyLoanOperationRecoveryAnalysisInternal(string customerId, string customerCode)
        {

            var applicationDate = _genSetup.GetApplicationDate();
            var loansId = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(x => x.DELETED == false).Select(x => x.LOANREFERENCE).ToList();
            var custmerIds = customerId.ToString();

            var exposureNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         !loansId.Contains(ln.REFERENCENUMBER)
                                         && ln.NPL != null
                                         && ln.UNPODAYSOVERDUE >= 30
                                         && ln.UNPODAYSOVERDUE <= 360
                                         && ln.CUSTOMERID == customerCode
                                         && ln.CBNCLASSIFICATION.Trim() != "PERFORMING"

                                         orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             loanId = ln.ID,
                                             customerCode = ln.CUSTOMERID,
                                             productCode = ln.PRODUCTID,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                             customerName = ln.CUSTOMERNAME,
                                             productName = ln.PRODUCTNAME,
                                             relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                             exposureType = ln.EXPOSURETYPE,
                                             expiryBand = ln.EXPIRINGBAND,
                                             divisionName = ln.DIVISIONNAME,
                                             totalAmountRecovery = (decimal)ln.TOTALUNSETTLEDAMOUNT,
                                             dpdExposure = ln.UNPODAYSOVERDUE,
                                             loanCategory = ln.CBNCLASSIFICATION
                                         }).ToList();

            foreach (var xx in exposureNonPerforming)
            {
                xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
                xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
            }

            var exposureDigitalNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         !loansId.Contains(ln.REFERENCENUMBER)
                                         && ln.NPL != null
                                         && ln.UNPODAYSOVERDUE >= 30
                                         && ln.UNPODAYSOVERDUE <= 360
                                         && ln.CUSTOMERID == customerCode
                                         && ln.CBNCLASSIFICATION.Trim() != "PERFORMING"

                                                orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             loanId = ln.ID,
                                             customerCode = ln.CUSTOMERID,
                                             productCode = ln.PRODUCTID,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                             customerName = ln.CUSTOMERNAME,
                                             productName = ln.PRODUCTNAME,
                                             relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                             exposureType = ln.EXPOSURETYPE,
                                             expiryBand = ln.EXPIRINGBAND,
                                             divisionName = ln.DIVISIONNAME,
                                             totalAmountRecovery = (decimal)ln.TOTALUNSETTLEDAMOUNT,
                                             dpdExposure = ln.UNPODAYSOVERDUE,
                                             loanCategory = ln.CBNCLASSIFICATION
                                         }).ToList();

            foreach (var xx in exposureDigitalNonPerforming)
            {
                xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
                xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
            }

            /*
            var dataLoanNonPerforming = (from ln in context.TBL_LOAN
                                         join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                                         where
                                         cu.CUSTOMERCODE == customerId
                                         && !loansId.Contains(ln.LOANREFERENCENUMBER)
                                         && pr.EXCLUDEFROMLITIGATION == false
                                         && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                         && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                         && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value >= 30
                                         && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value <= 360

                                         orderby ln.DATETIMECREATED descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             creditAppraisalLoanApplicationId = lp.LOANAPPLICATIONID,
                                             creditAppraisalOperationId = lp.OPERATIONID,
                                             loanApplicationDetailId = ld.LOANAPPLICATIONDETAILID,
                                             loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                             loanId = ln.TERMLOANID,
                                             customerId = cu.CUSTOMERCODE,
                                             productId = ln.PRODUCTID,
                                             productClassId = pr.PRODUCTCLASSID,
                                             productTypeId = pr.PRODUCTTYPEID,
                                             casaAccountId = ln.CASAACCOUNTID,
                                             loanCategory = "Non Performing",
                                             casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                             casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                             branchId = ln.BRANCHID,
                                             amount = ld.APPROVEDAMOUNT,
                                             operationTypeName = tt.OPERATIONNAME,
                                             loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                             applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                             principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                                             pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                             interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                                             interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                             principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                                             interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                                             relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                             relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                             misCode = ln.MISCODE,
                                             teamMiscode = ln.TEAMMISCODE,
                                             interestRate = ln.INTERESTRATE,
                                             effectiveDate = ln.EFFECTIVEDATE,
                                             maturityDate = ln.MATURITYDATE,
                                             bookingDate = ln.BOOKINGDATE,
                                             totalAmountRecovery = (ln.OUTSTANDINGPRINCIPAL + ln.PASTDUEINTEREST + ln.PASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL) + (from a in context.TBL_LOAN_SCHEDULE_DAILY where a.TBL_LOAN.TERMLOANID == ln.TERMLOANID && a.DATE == applicationDate select a.ACCRUEDINTEREST).FirstOrDefault(),
                                             principalAmount = ln.OUTSTANDINGPRINCIPAL,
                                             principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                                             interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                                             approvedBy = (int)ln.APPROVEDBY,
                                             approverComment = ln.APPROVERCOMMENT,
                                             dateApproved = ln.DATEAPPROVED,
                                             loanStatusId = ln.LOANSTATUSID,
                                             scheduleTypeId = ln.SCHEDULETYPEID,
                                             isDisbursed = ln.ISDISBURSED,
                                             disbursedBy = (int)ln.DISBURSEDBY,
                                             disburserComment = ln.DISBURSERCOMMENT,
                                             disburseDate = ln.DISBURSEDATE,
                                             customerGroupId = lp.CUSTOMERGROUPID,
                                             operationId = ln.OPERATIONID,
                                             loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                             equityContribution = ln.EQUITYCONTRIBUTION,
                                             subSectorId = ln.SUBSECTORID,
                                             subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                             sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                             firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                             firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                             outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                                             principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                                             principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                                             fixedPrincipal = ln.FIXEDPRINCIPAL,
                                             profileLoan = ln.PROFILELOAN,
                                             dischargeLetter = ln.DISCHARGELETTER,
                                             suspendInterest = ln.SUSPENDINTEREST,
                                             scheduled = ln.ISSCHEDULEDPREPAYMENT,
                                             isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                                             scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                                             scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,
                                             customerCode = cu.CUSTOMERCODE,
                                             productAccountNumber = ch.ACCOUNTCODE,
                                             productAccountName = ch.ACCOUNTNAME,
                                             loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                             customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                             currencyId = ln.CURRENCYID,
                                             branchName = br.BRANCHNAME,
                                             relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                             relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                             productName = pr.PRODUCTNAME,
                                             comment = "",
                                             approvedAmount = ld.APPROVEDAMOUNT,
                                             creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                             pastDueInterest = ln.PASTDUEINTEREST,
                                             pastDuePrincipal = ln.PASTDUEPRINCIPAL,
                                             interestOnPastDueInterest = ln.INTERESTONPASTDUEINTEREST,
                                             interestOnPastDuePrincipal = ln.INTERESTONPASTDUEPRINCIPAL,
                                             outstandingInterest = ln.OUTSTANDINGINTEREST,
                                             accruedInterest = (from a in context.TBL_LOAN_SCHEDULE_DAILY where a.TBL_LOAN.TERMLOANID == ln.TERMLOANID && a.DATE == applicationDate select a.ACCRUEDINTEREST).FirstOrDefault(),
                                             dateTimeCreated = ln.DATETIMECREATED,
                                         }).ToList();

            var dataRevolvingNonPerforming = (from ln in context.TBL_LOAN_REVOLVING
                                              join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                              join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                              join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                              join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                              join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                              join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                              join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                              join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                              join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                              where
                                              cu.CUSTOMERCODE == customerId
                                              && !loansId.Contains(ln.LOANREFERENCENUMBER)
                                              && pr.EXCLUDEFROMLITIGATION == false
                                              && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                              && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                              && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value >= 30
                                              && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value <= 360

                                              orderby ln.DATETIMECREATED descending
                                              select new GlobalExposureApplicationViewModel
                                              {
                                                  loanApplicationDetailId = ld.LOANAPPLICATIONDETAILID,
                                                  creditAppraisalLoanApplicationId = lp.LOANAPPLICATIONID,
                                                  creditAppraisalOperationId = lp.OPERATIONID,
                                                  loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                                  loanId = ln.REVOLVINGLOANID,
                                                  customerId = cu.CUSTOMERCODE,
                                                  productId = ln.PRODUCTID,
                                                  productClassId = pr.PRODUCTCLASSID,
                                                  productTypeId = pr.PRODUCTTYPEID,
                                                  casaAccountId = ln.CASAACCOUNTID,
                                                  casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                  casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                                  branchId = ln.BRANCHID,
                                                  totalAmountRecovery = (ln.PASTDUEPRINCIPAL + ln.PASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.PENALCHARGEAMOUNT),
                                                  loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                                  applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                                  relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                                  relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                                  misCode = ln.MISCODE,
                                                  teamMiscode = ln.TEAMMISCODE,
                                                  interestRate = ln.INTERESTRATE,
                                                  effectiveDate = ln.EFFECTIVEDATE,
                                                  maturityDate = ln.MATURITYDATE,
                                                  bookingDate = ln.BOOKINGDATE,
                                                  loanCategory = "Non Performing",
                                                  operationTypeName = tt.OPERATIONNAME,
                                                  approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == ln.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                                  approvedBy = (int)ln.APPROVEDBY,
                                                  approverComment = ln.APPROVERCOMMENT,
                                                  dateApproved = ln.DATEAPPROVED,
                                                  loanStatusId = ln.LOANSTATUSID,
                                                  isDisbursed = ln.ISDISBURSED,
                                                  disburserComment = ln.DISBURSERCOMMENT,
                                                  disburseDate = ln.DISBURSEDATE,
                                                  customerGroupId = lp.CUSTOMERGROUPID,
                                                  operationId = ln.OPERATIONID,
                                                  loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                                  subSectorId = ln.SUBSECTORID,
                                                  subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                                  sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                                  dischargeLetter = ln.DISCHARGELETTER,
                                                  suspendInterest = ln.SUSPENDINTEREST,
                                                  customerCode = cu.CUSTOMERCODE,
                                                  loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                                  customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                  currencyId = ln.CURRENCYID,
                                                  branchName = br.BRANCHNAME,
                                                  relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                  relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                                  productName = pr.PRODUCTNAME,
                                                  comment = "",
                                                  productAccountNumber = "N/A",
                                                  productAccountName = "N/A",
                                                  approvedAmount = ld.APPROVEDAMOUNT,
                                                  creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                              }).ToList();

            var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.loanId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();
            var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.loanId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();

            var allData = termLoanDataNon.Union(revolvingLoanDataNon);*/
            var allData = exposureNonPerforming.Union(exposureDigitalNonPerforming);
            var data = allData.GroupBy(x => x.loanReferenceNumber).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();
            return data;
        }
        private IEnumerable<GlobalExposureApplicationViewModel> GetQuarterlyLoanOperationRecoveryAnalysisExternal(string customerId, string customerCode)
        {

            var applicationDate = _genSetup.GetApplicationDate();
            var loansId = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(x => x.DELETED == false).Select(x => x.LOANREFERENCE).ToList();
            var custmerIds = customerId.ToString();

            var exposureNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         !loansId.Contains(ln.REFERENCENUMBER)
                                         && ln.NPL != null
                                         && ln.UNPODAYSOVERDUE > 360
                                         && ln.CUSTOMERID == customerCode

                                         orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             loanId = ln.ID,
                                             customerCode = ln.CUSTOMERID,
                                             productCode = ln.PRODUCTID,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                             customerName = ln.CUSTOMERNAME,
                                             productName = ln.PRODUCTNAME,
                                             relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                             exposureType = ln.EXPOSURETYPE,
                                             expiryBand = ln.EXPIRINGBAND,
                                             divisionName = ln.DIVISIONNAME,
                                             totalAmountRecovery = (decimal)ln.TOTALUNSETTLEDAMOUNT,
                                             dpdExposure = ln.UNPODAYSOVERDUE,
                                             loanCategory = ln.CBNCLASSIFICATION
                                         }).ToList();

            foreach (var xx in exposureNonPerforming)
            {
                xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
                xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
            }

            var exposureDigitalNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         !loansId.Contains(ln.REFERENCENUMBER)
                                         && ln.NPL != null
                                         && ln.UNPODAYSOVERDUE > 360
                                         && ln.CUSTOMERID == customerCode

                                                orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             loanId = ln.ID,
                                             customerCode = ln.CUSTOMERID,
                                             productCode = ln.PRODUCTID,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                             customerName = ln.CUSTOMERNAME,
                                             productName = ln.PRODUCTNAME,
                                             relationshipManagerName = ln.ACCOUNTOFFICERNAME,
                                             exposureType = ln.EXPOSURETYPE,
                                             expiryBand = ln.EXPIRINGBAND,
                                             divisionName = ln.DIVISIONNAME,
                                             totalAmountRecovery = (decimal)ln.TOTALUNSETTLEDAMOUNT,
                                             dpdExposure = ln.UNPODAYSOVERDUE,
                                             loanCategory = ln.CBNCLASSIFICATION
                                         }).ToList();

            foreach (var xx in exposureDigitalNonPerforming)
            {
                xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
                xx.productId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTID).FirstOrDefault();
                xx.productClassId = context.TBL_PRODUCT.Where(x => x.PRODUCTCODE == xx.productCode).Select(x => x.PRODUCTCLASSID).FirstOrDefault();
            }

            /*
            var dataLoanNonPerforming = (from ln in context.TBL_LOAN
                                         join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         join ch in context.TBL_CHART_OF_ACCOUNT on pr.PRINCIPALBALANCEGL equals ch.GLACCOUNTID
                                         where
                                         cu.CUSTOMERCODE == customerId
                                         && !loansId.Contains(ln.LOANREFERENCENUMBER)
                                         && pr.EXCLUDEFROMLITIGATION == false
                                         && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                         && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                         && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value > 360

                                         orderby ln.DATETIMECREATED descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             creditAppraisalLoanApplicationId = lp.LOANAPPLICATIONID,
                                             creditAppraisalOperationId = lp.OPERATIONID,
                                             loanApplicationDetailId = ld.LOANAPPLICATIONDETAILID,
                                             loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                             loanId = ln.TERMLOANID,
                                             customerId = cu.CUSTOMERCODE,
                                             productId = ln.PRODUCTID,
                                             productClassId = pr.PRODUCTCLASSID,
                                             productTypeId = pr.PRODUCTTYPEID,
                                             casaAccountId = ln.CASAACCOUNTID,
                                             loanCategory = "Non Performing",
                                             casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                             casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                             branchId = ln.BRANCHID,
                                             amount = ld.APPROVEDAMOUNT,
                                             operationTypeName = tt.OPERATIONNAME,
                                             loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                             applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                             principalFrequencyTypeId = ln.PRINCIPALFREQUENCYTYPEID != null ? (short)ln.PRINCIPALFREQUENCYTYPEID : (short)0,
                                             pricipalFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                             interestFrequencyTypeId = ln.INTERESTFREQUENCYTYPEID != null ? (short)ln.INTERESTFREQUENCYTYPEID : (short)0,
                                             interestFrequencyTypeName = ln.TBL_FREQUENCY_TYPE.MODE,
                                             principalNumberOfInstallment = ln.PRINCIPALNUMBEROFINSTALLMENT,
                                             interestNumberOfInstallment = ln.INTERESTNUMBEROFINSTALLMENT,
                                             relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                             relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                             misCode = ln.MISCODE,
                                             teamMiscode = ln.TEAMMISCODE,
                                             interestRate = ln.INTERESTRATE,
                                             effectiveDate = ln.EFFECTIVEDATE,
                                             maturityDate = ln.MATURITYDATE,
                                             bookingDate = ln.BOOKINGDATE,
                                             totalAmountRecovery = (ln.OUTSTANDINGPRINCIPAL + ln.PASTDUEINTEREST + ln.PASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL) + (from a in context.TBL_LOAN_SCHEDULE_DAILY where a.TBL_LOAN.TERMLOANID == ln.TERMLOANID && a.DATE == applicationDate select a.ACCRUEDINTEREST).FirstOrDefault(),
                                             principalAmount = ln.OUTSTANDINGPRINCIPAL,
                                             principalInstallmentLeft = ln.PRINCIPALINSTALLMENTLEFT,
                                             interestInstallmentLeft = ln.INTERESTINSTALLMENTLEFT,
                                             approvedBy = (int)ln.APPROVEDBY,
                                             approverComment = ln.APPROVERCOMMENT,
                                             dateApproved = ln.DATEAPPROVED,
                                             loanStatusId = ln.LOANSTATUSID,
                                             scheduleTypeId = ln.SCHEDULETYPEID,
                                             isDisbursed = ln.ISDISBURSED,
                                             disbursedBy = (int)ln.DISBURSEDBY,
                                             disburserComment = ln.DISBURSERCOMMENT,
                                             disburseDate = ln.DISBURSEDATE,
                                             customerGroupId = lp.CUSTOMERGROUPID,
                                             operationId = ln.OPERATIONID,
                                             loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                             equityContribution = ln.EQUITYCONTRIBUTION,
                                             subSectorId = ln.SUBSECTORID,
                                             subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                             sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                             firstPrincipalPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                             firstInterestPaymentDate = ln.FIRSTINTERESTPAYMENTDATE,
                                             outstandingPrincipal = ln.OUTSTANDINGPRINCIPAL,
                                             principalAdditionCount = ln.PRINCIPALADDITIONCOUNT,
                                             principalReductionCount = ln.PRINCIPALREDUCTIONCOUNT,
                                             fixedPrincipal = ln.FIXEDPRINCIPAL,
                                             profileLoan = ln.PROFILELOAN,
                                             dischargeLetter = ln.DISCHARGELETTER,
                                             suspendInterest = ln.SUSPENDINTEREST,
                                             scheduled = ln.ISSCHEDULEDPREPAYMENT,
                                             isScheduledPrepayment = ln.ISSCHEDULEDPREPAYMENT,
                                             scheduledPrepaymentAmount = ln.SCHEDULEDPREPAYMENTAMOUNT,
                                             scheduledPrepaymentDate = ln.SCHEDULEDPREPAYMENTDATE,
                                             customerCode = cu.CUSTOMERCODE,
                                             productAccountNumber = ch.ACCOUNTCODE,
                                             productAccountName = ch.ACCOUNTNAME,
                                             loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                             customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                             currencyId = ln.CURRENCYID,
                                             branchName = br.BRANCHNAME,
                                             relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                             relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                             productName = pr.PRODUCTNAME,
                                             comment = "",
                                             approvedAmount = ld.APPROVEDAMOUNT,
                                             creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                             pastDueInterest = ln.PASTDUEINTEREST,
                                             pastDuePrincipal = ln.PASTDUEPRINCIPAL,
                                             interestOnPastDueInterest = ln.INTERESTONPASTDUEINTEREST,
                                             interestOnPastDuePrincipal = ln.INTERESTONPASTDUEPRINCIPAL,
                                             outstandingInterest = ln.OUTSTANDINGINTEREST,
                                             accruedInterest = (from a in context.TBL_LOAN_SCHEDULE_DAILY where a.TBL_LOAN.TERMLOANID == ln.TERMLOANID && a.DATE == applicationDate select a.ACCRUEDINTEREST).FirstOrDefault(),
                                             dateTimeCreated = ln.DATETIMECREATED,
                                         }).ToList();

            var dataRevolvingNonPerforming = (from ln in context.TBL_LOAN_REVOLVING
                                              join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                              join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                              join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                              join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                              join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                              join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                              join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                              join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                              join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                              where
                                              cu.CUSTOMERCODE == customerId
                                              && !loansId.Contains(ln.LOANREFERENCENUMBER)
                                              && pr.EXCLUDEFROMLITIGATION == false
                                              && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                              && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                              && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value > 360

                                              orderby ln.DATETIMECREATED descending
                                              select new GlobalExposureApplicationViewModel
                                              {
                                                  loanApplicationDetailId = ld.LOANAPPLICATIONDETAILID,
                                                  creditAppraisalLoanApplicationId = lp.LOANAPPLICATIONID,
                                                  creditAppraisalOperationId = lp.OPERATIONID,
                                                  loanSystemTypeId = ln.LOANSYSTEMTYPEID,
                                                  loanId = ln.REVOLVINGLOANID,
                                                  customerId = cu.CUSTOMERCODE,
                                                  productId = ln.PRODUCTID,
                                                  productClassId = pr.PRODUCTCLASSID,
                                                  productTypeId = pr.PRODUCTTYPEID,
                                                  casaAccountId = ln.CASAACCOUNTID,
                                                  casaAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                  casaAccountName = context.TBL_CASA.Where(x => x.CASAACCOUNTID == ln.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNAME).FirstOrDefault(),
                                                  branchId = ln.BRANCHID,
                                                  totalAmountRecovery = (ln.PASTDUEPRINCIPAL + ln.PASTDUEINTEREST + ln.INTERESTONPASTDUEPRINCIPAL + ln.INTERESTONPASTDUEINTEREST + ln.PENALCHARGEAMOUNT),
                                                  loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                                  applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER,
                                                  relationshipOfficerId = ln.RELATIONSHIPOFFICERID,
                                                  relationshipManagerId = ln.RELATIONSHIPMANAGERID,
                                                  misCode = ln.MISCODE,
                                                  teamMiscode = ln.TEAMMISCODE,
                                                  interestRate = ln.INTERESTRATE,
                                                  effectiveDate = ln.EFFECTIVEDATE,
                                                  maturityDate = ln.MATURITYDATE,
                                                  bookingDate = ln.BOOKINGDATE,
                                                  loanCategory = "Non Performing",
                                                  operationTypeName = tt.OPERATIONNAME,
                                                  approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(f => f.APPROVALSTATUSID == ln.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                                  approvedBy = (int)ln.APPROVEDBY,
                                                  approverComment = ln.APPROVERCOMMENT,
                                                  dateApproved = ln.DATEAPPROVED,
                                                  loanStatusId = ln.LOANSTATUSID,
                                                  isDisbursed = ln.ISDISBURSED,
                                                  disburserComment = ln.DISBURSERCOMMENT,
                                                  disburseDate = ln.DISBURSEDATE,
                                                  customerGroupId = lp.CUSTOMERGROUPID,
                                                  operationId = ln.OPERATIONID,
                                                  loanTypeId = lp.LOANAPPLICATIONTYPEID,
                                                  subSectorId = ln.SUBSECTORID,
                                                  subSectorName = ln.TBL_SUB_SECTOR.NAME,
                                                  sectorName = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                                  dischargeLetter = ln.DISCHARGELETTER,
                                                  suspendInterest = ln.SUSPENDINTEREST,
                                                  customerCode = cu.CUSTOMERCODE,
                                                  loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                                  customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                                  currencyId = ln.CURRENCYID,
                                                  branchName = br.BRANCHNAME,
                                                  relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                                  relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                                  productName = pr.PRODUCTNAME,
                                                  comment = "",
                                                  productAccountNumber = "N/A",
                                                  productAccountName = "N/A",
                                                  approvedAmount = ld.APPROVEDAMOUNT,
                                                  creatorName = context.TBL_STAFF.Where(x => x.STAFFID == ld.CREATEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),
                                              }).ToList();

            var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.loanId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();
            var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.loanId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();

            var allData = termLoanDataNon.Union(revolvingLoanDataNon);*/
            var allData = exposureNonPerforming.Union(exposureDigitalNonPerforming);
            var data = allData.GroupBy(x => x.loanReferenceNumber).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.applicationReferenceNumber).ToList();
            return data;
        }

        public IEnumerable<AccreditedConsultantsViewModel> GetAccreditedRecoveryConsultants(int location)
        {
            var data = (from m in context.TBL_ACCREDITEDCONSULTANT
                        join c in context.TBL_ACCREDITEDCONSULTANT_STATE on m.ACCREDITEDCONSULTANTID equals c.ACCREDITEDCONSULTANTID
                        where location == c.STATEID
                        && m.ACCREDITEDCONSULTANTTYPEID == (int)AccreditedConsultantTypeEnum.RecoveryAgent
                        && m.AGENTCATEGORY.ToLower() == "retail"
                        select new AccreditedConsultantsViewModel
                        {
                            accreditedConsultantId = m.ACCREDITEDCONSULTANTID,
                            registrationNumber = m.REGISTRATIONNUMBER,
                            name = m.NAME,
                            firmName = m.FIRMNAME,
                            accreditedConsultantTypeId = m.ACCREDITEDCONSULTANTTYPEID,
                            cityId = (short)m.CITYID,
                            accountNumber = m.ACCOUNTNUMBER,
                            solicitorBVN = m.SOLICITORBVN,
                            countryId = m.COUNTRYID,
                            emailAddress = m.EMAILADDRESS,
                            phoneNumber = m.PHONENUMBER,
                            address = m.ADDRESS,
                            totalRecordsAssigned = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(r=>r.ACCREDITEDCONSULTANT == m.ACCREDITEDCONSULTANTID && r.DELETED == false).Count(),
                            dateOfEngagement = m.DATEOFENGAGEMENT.Value,
                            category = m.CATEGORY,
                            agentCategory = m.AGENTCATEGORY,
                            consultantType = context.TBL_ACCREDITEDCONSULTANT_TYPE.Where(t => t.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTTYPEID).Select(t => t.NAME).FirstOrDefault(),
                            coreCompetence = m.CORECOMPETENCE,
                            stateName = (from s in context.TBL_STATE join c in context.TBL_ACCREDITEDCONSULTANT_STATE on s.STATEID equals c.STATEID where c.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTID select s.STATENAME).FirstOrDefault(),
                        }).OrderBy(x => x.totalRecordsAssigned).Take(1).ToList();
            return data;
        }

        public IEnumerable<AccreditedConsultantsViewModel> GetAccreditedRecoveryConsultantsByAutoReAssignment(int location)
        {
            var data = (from m in context.TBL_ACCREDITEDCONSULTANT
                        join c in context.TBL_ACCREDITEDCONSULTANT_STATE on m.ACCREDITEDCONSULTANTID equals c.ACCREDITEDCONSULTANTID
                        where location == c.STATEID
                        && m.ACCREDITEDCONSULTANTTYPEID == (int)AccreditedConsultantTypeEnum.RecoveryAgent
                        && m.AGENTCATEGORY.ToLower() == "retail"
                        select new AccreditedConsultantsViewModel
                        {
                            accreditedConsultantId = m.ACCREDITEDCONSULTANTID,
                            registrationNumber = m.REGISTRATIONNUMBER,
                            name = m.NAME,
                            firmName = m.FIRMNAME,
                            accreditedConsultantTypeId = m.ACCREDITEDCONSULTANTTYPEID,
                            cityId = (short)m.CITYID,
                            accountNumber = m.ACCOUNTNUMBER,
                            solicitorBVN = m.SOLICITORBVN,
                            countryId = m.COUNTRYID,
                            emailAddress = m.EMAILADDRESS,
                            phoneNumber = m.PHONENUMBER,
                            address = m.ADDRESS,
                            totalRecordsAssigned = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(r => r.ACCREDITEDCONSULTANT == m.ACCREDITEDCONSULTANTID && r.DELETED == false && r.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending).Count(),
                            dateOfEngagement = m.DATEOFENGAGEMENT.Value,
                            category = m.CATEGORY,
                            agentCategory = m.AGENTCATEGORY,
                            consultantType = context.TBL_ACCREDITEDCONSULTANT_TYPE.Where(t => t.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTTYPEID).Select(t => t.NAME).FirstOrDefault(),
                            coreCompetence = m.CORECOMPETENCE,
                            stateName = (from s in context.TBL_STATE join c in context.TBL_ACCREDITEDCONSULTANT_STATE on s.STATEID equals c.STATEID where c.ACCREDITEDCONSULTANTID == m.ACCREDITEDCONSULTANTID select s.STATENAME).FirstOrDefault(),
                        }).OrderBy(x => x.totalRecordsAssigned).Take(1).ToList();

            return data;
        }


        public bool saveBulkLoanAssignmentToAgent(List<GlobalExposureApplicationViewModel> models, int accreditedConsultant, DateTime? expCompletionDate, string source, string assignmentType)
        {
            bool result = false;
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
            List<TBL_LOAN_RECOVERY_ASSIGNMENT> bulkLoanTable = new List<TBL_LOAN_RECOVERY_ASSIGNMENT>();
            var userRole = context.TBL_STAFF_ROLE.Where(x => x.STAFFROLECODE.ToUpper().Contains("TLRC")).FirstOrDefault();
            var staffInRole = context.TBL_STAFF.Where(x => x.STAFFROLEID == userRole.STAFFROLEID).FirstOrDefault();
            GlobalExposureApplicationViewModel assignOperations = new GlobalExposureApplicationViewModel();

            foreach (var customerRequest in models)
            {
                assignOperations.createdBy = staffInRole.STAFFID;
                assignOperations.accreditedConsultant = accreditedConsultant;
                assignOperations.applicationReferenceNumber = customerRequest.loanReferenceNumber; 
                assignOperations.loanReferenceNumber = customerRequest.loanReferenceNumber;
                assignOperations.expCompletionDate = expCompletionDate;
                assignOperations.referenceId = referenceNumber;
                assignOperations.approvalStatusId = (int)ApprovalStatusEnum.Processing;
                assignOperations.operationId = (int)OperationsEnum.RetailRecoveryAssignmentApproval;
                assignOperations.operationCompleted = false;
                assignOperations.totalAmountRecovery = customerRequest.totalAmountRecovery;
                assignOperations.source = source;
                assignOperations.productId = customerRequest.productId;
                assignOperations.loanId = customerRequest.loanId;
                assignOperations.assignmentType = assignmentType;
                assignOperations.productClassId = customerRequest.productClassId;
                assignOperations.customerId = customerRequest.customerId;
                var loanData = addBulkLoanAssignmentToAgent(assignOperations);
                bulkLoanTable.Add(loanData);
            }

            context.TBL_LOAN_RECOVERY_ASSIGNMENT.AddRange(bulkLoanTable);
            context.SaveChanges();

            TBL_BULK_RECOVERY_ASSIGNMENT_AGENT_APPROVAL removeLienOperation = new TBL_BULK_RECOVERY_ASSIGNMENT_AGENT_APPROVAL();
            removeLienOperation = context.TBL_BULK_RECOVERY_ASSIGNMENT_AGENT_APPROVAL.Add(new TBL_BULK_RECOVERY_ASSIGNMENT_AGENT_APPROVAL
            {
                ACCREDITEDCONSULTANTID = accreditedConsultant,
                REFERENCEBATCHID = referenceNumber,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                OPERATIONID = (int)OperationsEnum.RetailRecoveryAssignmentApproval,
                REQUESTDATE = DateTime.Now,
                SOURCE = source,
                ASSIGNMENTTYPE = assignmentType
            });

            int resultStatus = context.SaveChanges();

            using (TransactionScope transactionScope = new TransactionScope())
            {
                try
                {
                    workflow.StaffId = staffInRole.STAFFID;
                    workflow.CompanyId = 1;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.TargetId = removeLienOperation.BULKRECOVERYAPPROVALID;
                    workflow.Comment = "Kindly help approve the loan recovery assignment to agent";
                    workflow.OperationId = (int)OperationsEnum.RetailRecoveryAssignmentApproval;
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = false;

                    var response = workflow.LogActivity();
                    context.SaveChanges();

                    transactionScope.Complete();

                    transactionScope.Dispose();
                }catch(Exception e)
                {
                    throw e;
                }
            }
            if (resultStatus > 0)
            {
                result = true;
            }

            return result;
        }

        private TBL_LOAN_RECOVERY_ASSIGNMENT addBulkLoanAssignmentToAgent(GlobalExposureApplicationViewModel entity)
        {
            var data = new TBL_LOAN_RECOVERY_ASSIGNMENT
            {
                LOANID = entity.loanId,
                APPLICATIONREFERENCENUMBER = entity.applicationReferenceNumber,
                CUSTOMERID = entity.customerId,
                ACCREDITEDCONSULTANT = entity.accreditedConsultant,
                DATEASSIGNED = DateTime.Now,
                CREATEDBY = entity.createdBy,
                EXPCOMPLETIONDATE = entity.expCompletionDate,
                REFERENCEID = entity.referenceId,
                OPERATIONID = entity.operationId,
                APPROVALSTATUSID = entity.approvalStatusId,
                OPERATIONCOMPLETED = entity.operationCompleted,
                TOTALAMOUNTRECOVERY = entity.totalAmountRecovery,
                SOURCE = entity.source,
                PRODUCTCLASSID = entity.productClassId,
                LOANREFERENCE = entity.loanReferenceNumber,
                PRODUCTID = entity.productId,
                ASSIGNMENTTYPE = entity.assignmentType
            };
            return data;
        }

        private void deletePreviousAssignment(string customerId)
        {
            var data = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(c=>c.CUSTOMERID == customerId && c.APPROVALSTATUSID==(int)ApprovalStatusEnum.Approved).ToList();
            if (data.Count() > 0 )
            {
                foreach(var d in data)
                {
                    var record = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Find(d.LOANASSIGNID);
                    record.DELETED = true;
                    record.DATETIMEDELETED = DateTime.Now;
                    context.SaveChanges();
                }
                
            }
            
        }

        public bool MonthlyAutoAssignRecoveryAnalysisByCustomer()
        {
            var applicationDate = _genSetup.GetApplicationDate();
            var loansId = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(x => x.DELETED == false).Select(x => x.LOANREFERENCE).ToList();
           
                var exposureNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE
                                             join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                             where
                                             !loansId.Contains(ln.REFERENCENUMBER)
                                             && ln.NPL != null
                                             && ln.UNPODAYSOVERDUE >= 30
                                             && ln.CBNCLASSIFICATION.Trim() != "PERFORMING"

                                             orderby ln.ID descending
                                             select new GlobalExposureApplicationViewModel
                                             {
                                                 stateId = b.STATEID,
                                                 loanId = ln.ID,
                                                 customerCode = ln.CUSTOMERID,
                                                 customerId = ln.CUSTOMERID,
                                                 branchCode = ln.BRANCHCODE,
                                                 branchName = b.BRANCHNAME,
                                                 loanReferenceNumber = ln.REFERENCENUMBER,
                                             }).ToList();

                foreach (var xx in exposureNonPerforming)
                {
                    xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                    xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
                }

                var exposureDigitalNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN
                                                    join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                                    where
                                                    !loansId.Contains(ln.REFERENCENUMBER)
                                                    && ln.NPL != null
                                                    && ln.UNPODAYSOVERDUE >= 30
                                                    && ln.CBNCLASSIFICATION.Trim() != "PERFORMING"

                                                    orderby ln.ID descending
                                                    select new GlobalExposureApplicationViewModel
                                                    {
                                                        stateId = b.STATEID,
                                                        loanId = ln.ID,
                                                        customerCode = ln.CUSTOMERID,
                                                        customerId = ln.CUSTOMERID,
                                                        branchCode = ln.BRANCHCODE,
                                                        branchName = b.BRANCHNAME,
                                                        loanReferenceNumber = ln.REFERENCENUMBER,
                                                    }).ToList();

                foreach (var xx in exposureDigitalNonPerforming)
                {
                    xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                    xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
                }

                var dataLoanNonPerforming = (from ln in context.TBL_LOAN
                                             join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                             join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                             join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                             join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                             join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                             join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                             join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                             where
                                             !loansId.Contains(ln.LOANREFERENCENUMBER)
                                             && pr.EXCLUDEFROMLITIGATION == false
                                             && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                             && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                             && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value >= 30

                                             orderby ln.DATETIMECREATED descending
                                             select new GlobalExposureApplicationViewModel
                                             {
                                                 stateId = br.STATEID,
                                                 loanId = ln.TERMLOANID,
                                                 customerId = cu.CUSTOMERCODE,
                                                 customerCode = cu.CUSTOMERCODE,
                                                 branchId = ln.BRANCHID,
                                                 branchName = br.BRANCHNAME,
                                                 loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                             }).ToList();

                var dataRevolvingNonPerforming = (from ln in context.TBL_LOAN_REVOLVING
                                                  join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                                  join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                                  join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                                  join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                                  join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                                  join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                                  join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                                  where
                                                  !loansId.Contains(ln.LOANREFERENCENUMBER)
                                                  && pr.EXCLUDEFROMLITIGATION == false
                                                  && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                                  && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                                  && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value >= 30

                                                  orderby ln.DATETIMECREATED descending
                                                  select new GlobalExposureApplicationViewModel
                                                  {
                                                      stateId = br.STATEID,
                                                      loanId = ln.REVOLVINGLOANID,
                                                      customerId = cu.CUSTOMERCODE,
                                                      customerCode = cu.CUSTOMERCODE,
                                                      branchId = ln.BRANCHID,
                                                      branchName = br.BRANCHNAME,
                                                      loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                                  }).ToList();

                var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.customerId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReferenceNumber).ToList();
                var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.customerId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReferenceNumber).ToList();
                var allData = termLoanDataNon.Union(revolvingLoanDataNon);
                allData = allData.Union(exposureNonPerforming).Union(exposureDigitalNonPerforming);
                var data = allData.GroupBy(x => x.customerId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReferenceNumber).ToList();

                if (data.Count() > 0)
                {
                    foreach (var record in data)
                    {
                        if (record.stateId != null)
                        {
                                var recoveryAgents = GetAccreditedRecoveryConsultants((int)record.stateId);
                                var customerRecordsInternal = GetLoanOperationRecoveryAnalysisInternal(record.customerId,record.customerCode).ToList();
                                var customerRecordsExternal = GetLoanOperationRecoveryAnalysisExternal(record.customerId, record.customerCode).ToList();
                                if (recoveryAgents.Count() > 0 && (customerRecordsInternal.Count() > 0 || customerRecordsExternal.Count() > 0))
                                {
                                    
                                        var consultant = recoveryAgents.ElementAt(0).accreditedConsultantId;
                                        var category = recoveryAgents.ElementAt(0).category;
                                        if (category.ToLower() == "internal" && customerRecordsInternal.Count() > 0)
                                        {
                                            saveBulkLoanAssignmentToAgent(customerRecordsInternal, consultant, DateTime.Now, "RETAIL", "AUTO");
                                        }
                                        else if (category.ToLower() == "external" && customerRecordsExternal.Count() > 0)
                                        {
                                            saveBulkLoanAssignmentToAgent(customerRecordsExternal, consultant, DateTime.Now, "RETAIL", "AUTO");
                                        }
                                }
                        }

                    }
                    return true;
                }
                return false;
        }



        public bool QuarterlyAutoAssignRecoveryAnalysisByCustomer()
        {
            var applicationDate = _genSetup.GetApplicationDate();
            var loansId = context.TBL_LOAN_RECOVERY_ASSIGNMENT.Where(x => x.DELETED == false).Select(x => x.LOANREFERENCE).ToList();

            var exposureNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         !loansId.Contains(ln.REFERENCENUMBER)
                                         && ln.NPL != null
                                         && ln.UNPODAYSOVERDUE >= 30

                                         orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             stateId = b.STATEID,
                                             loanId = ln.ID,
                                             customerCode = ln.CUSTOMERID,
                                             customerId = ln.CUSTOMERID,
                                             branchCode = ln.BRANCHCODE,
                                             branchName = b.BRANCHNAME,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                         }).ToList();

            foreach (var xx in exposureNonPerforming)
            {
                xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
            }

            var exposureDigitalNonPerforming = (from ln in context.TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         !loansId.Contains(ln.REFERENCENUMBER)
                                         && ln.NPL != null
                                         && ln.UNPODAYSOVERDUE >= 30

                                         orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             stateId = b.STATEID,
                                             loanId = ln.ID,
                                             customerCode = ln.CUSTOMERID,
                                             customerId = ln.CUSTOMERID,
                                             branchCode = ln.BRANCHCODE,
                                             branchName = b.BRANCHNAME,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                         }).ToList();

            foreach (var xx in exposureDigitalNonPerforming)
            {
                xx.branchId = context.TBL_BRANCH.Where(x => x.BRANCHCODE == xx.branchCode).Select(x => x.BRANCHID).FirstOrDefault();
                xx.customerId = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERCODE).FirstOrDefault();
            }

            var dataLoanNonPerforming = (from ln in context.TBL_LOAN
                                         join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         where
                                         !loansId.Contains(ln.LOANREFERENCENUMBER)
                                         && pr.EXCLUDEFROMLITIGATION == false
                                         && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                         && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                         && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value >= 30

                                         orderby ln.DATETIMECREATED descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             stateId = br.STATEID,
                                             loanId = ln.TERMLOANID,
                                             customerId = cu.CUSTOMERCODE,
                                             customerCode = cu.CUSTOMERCODE,
                                             branchId = ln.BRANCHID,
                                             branchName = br.BRANCHNAME,
                                             loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                         }).ToList();

            var dataRevolvingNonPerforming = (from ln in context.TBL_LOAN_REVOLVING
                                              join tt in context.TBL_OPERATIONS on ln.OPERATIONID equals tt.OPERATIONID
                                              join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                              join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                              join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                              join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                              join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                              join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                              where
                                              !loansId.Contains(ln.LOANREFERENCENUMBER)
                                              && pr.EXCLUDEFROMLITIGATION == false
                                              && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                              && ln.USER_PRUDENTIAL_GUIDE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                                              && DbFunctions.DiffDays(DateTime.UtcNow, ln.MATURITYDATE).Value >= 30

                                              orderby ln.DATETIMECREATED descending
                                              select new GlobalExposureApplicationViewModel
                                              {
                                                  stateId = br.STATEID,
                                                  loanId = ln.REVOLVINGLOANID,
                                                  customerId = cu.CUSTOMERCODE,
                                                  customerCode = cu.CUSTOMERCODE,
                                                  branchId = ln.BRANCHID,
                                                  branchName = br.BRANCHNAME,
                                                  loanReferenceNumber = ln.LOANREFERENCENUMBER,
                                              }).ToList();

            var termLoanDataNon = dataLoanNonPerforming.GroupBy(x => x.customerId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReferenceNumber).ToList();
            var revolvingLoanDataNon = dataRevolvingNonPerforming.GroupBy(x => x.customerId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReferenceNumber).ToList();
            var allData = termLoanDataNon.Union(revolvingLoanDataNon);
            allData = allData.Union(exposureNonPerforming).Union(exposureDigitalNonPerforming);
            var data = allData.GroupBy(x => x.customerId).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReferenceNumber).ToList();

            if (data.Count() > 0)
            {
                foreach (var record in data)
                {
                    var recoveryAgents = GetAccreditedRecoveryConsultantsByAutoReAssignment((int)record.stateId);
                    var customerRecordsInternal = GetQuarterlyLoanOperationRecoveryAnalysisInternal(record.customerId, record.customerCode).ToList();
                    var customerRecordsExternal = GetQuarterlyLoanOperationRecoveryAnalysisExternal(record.customerId, record.customerCode).ToList();
                    if (recoveryAgents.Count() > 0 && (customerRecordsInternal.Count() > 0 || customerRecordsExternal.Count() > 0))
                    {
                        var consultant = recoveryAgents.ElementAt(0).accreditedConsultantId;
                        var category = recoveryAgents.ElementAt(0).category;

                        if (category.ToLower() == "internal" && customerRecordsInternal.Count() > 0)
                        {
                            saveBulkLoanAssignmentToAgent(customerRecordsInternal, consultant, DateTime.Now, "RETAIL", "AUTO");
                        }
                        else if (category.ToLower() == "external" && customerRecordsExternal.Count() > 0)
                        {
                            saveBulkLoanAssignmentToAgent(customerRecordsExternal, consultant, DateTime.Now, "RETAIL", "AUTO");
                        }

                        deletePreviousAssignment(record.customerId);
                    }

                }
            }
            return true;
        }


        public IEnumerable<GlobalExposureApplicationViewModel> GetRecoveryAssignmentDueCompletionDate()
        {

            var dataLoanNonPerforming = (from r in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                         join ln in context.TBL_GLOBAL_EXPOSURE on r.LOANREFERENCE equals ln.REFERENCENUMBER
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         r.DELETED == false
                                         && r.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                         && r.ISFULLYRECOVERED == false
                                         && r.SOURCE.ToLower() == "remedial"
                                         && (DbFunctions.DiffDays(DateTime.UtcNow, r.DATEASSIGNED).Value >= 1 && DbFunctions.DiffDays(DateTime.UtcNow, r.DATEASSIGNED).Value <= 10)

                                         orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             productName = ln.PRODUCTNAME,
                                             loanAssignId = r.LOANASSIGNID,
                                             dateOfAssignment = r.DATEASSIGNED,
                                             expCompletionDate = r.EXPCOMPLETIONDATE,
                                             totalRecoveryAmount = (decimal)r.TOTALAMOUNTRECOVERY,
                                             createdBy = r.CREATEDBY,
                                             staffFullName = ln.ACCOUNTOFFICERNAME,
                                             misCode = ln.ACCOUNTOFFICERCODE,
                                             accreditedConsultantCompany = context.TBL_ACCREDITEDCONSULTANT.Where(c=>c.ACCREDITEDCONSULTANTID == r.ACCREDITEDCONSULTANT).Select(c=>c.FIRMNAME).FirstOrDefault(),
                                             accreditedConsultantEmail = context.TBL_ACCREDITEDCONSULTANT.Where(c => c.ACCREDITEDCONSULTANTID == r.ACCREDITEDCONSULTANT).Select(c => c.EMAILADDRESS).FirstOrDefault(),
                                             customerName = ln.CUSTOMERNAME,
                                             stateId = b.STATEID,
                                             loanId = ln.ID,
                                             customerId = r.CUSTOMERID,
                                             branchCode = ln.BRANCHCODE,
                                             branchName = ln.BRANCHNAME,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                             referenceId = r.REFERENCEID,
                                         }).ToList();

            var dataDigitalLoanNonPerforming = (from r in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                         join ln in context.TBL_GLOBAL_EXPOSURE_DIGITAL_LOAN on r.LOANREFERENCE equals ln.REFERENCENUMBER
                                         join b in context.TBL_BRANCH on ln.BRANCHCODE equals b.BRANCHCODE
                                         where
                                         r.DELETED == false
                                         && r.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                         && r.ISFULLYRECOVERED == false
                                         && r.SOURCE.ToLower() == "remedial"
                                         && (DbFunctions.DiffDays(DateTime.UtcNow, r.DATEASSIGNED).Value >= 1 && DbFunctions.DiffDays(DateTime.UtcNow, r.DATEASSIGNED).Value <= 10)

                                         orderby ln.ID descending
                                         select new GlobalExposureApplicationViewModel
                                         {
                                             productName = ln.PRODUCTNAME,
                                             loanAssignId = r.LOANASSIGNID,
                                             dateOfAssignment = r.DATEASSIGNED,
                                             expCompletionDate = r.EXPCOMPLETIONDATE,
                                             totalRecoveryAmount = (decimal)r.TOTALAMOUNTRECOVERY,
                                             createdBy = r.CREATEDBY,
                                             staffFullName = ln.ACCOUNTOFFICERNAME,
                                             misCode = ln.ACCOUNTOFFICERCODE,
                                             accreditedConsultantCompany = context.TBL_ACCREDITEDCONSULTANT.Where(c => c.ACCREDITEDCONSULTANTID == r.ACCREDITEDCONSULTANT).Select(c => c.FIRMNAME).FirstOrDefault(),
                                             accreditedConsultantEmail = context.TBL_ACCREDITEDCONSULTANT.Where(c => c.ACCREDITEDCONSULTANTID == r.ACCREDITEDCONSULTANT).Select(c => c.EMAILADDRESS).FirstOrDefault(),
                                             customerName = ln.CUSTOMERNAME,
                                             stateId = b.STATEID,
                                             loanId = ln.ID,
                                             customerId = r.CUSTOMERID,
                                             branchCode = ln.BRANCHCODE,
                                             branchName = ln.BRANCHNAME,
                                             loanReferenceNumber = ln.REFERENCENUMBER,
                                             referenceId = r.REFERENCEID,
                                         }).ToList();

            var allData = dataLoanNonPerforming.Union(dataDigitalLoanNonPerforming);
            var data = allData.GroupBy(x => x.loanReferenceNumber).Select(y => y.FirstOrDefault()).OrderByDescending(x => x.loanReferenceNumber).ToList();
            return data;
        }

        public void ValidateProfiledUsers(int maxUsers)
        {
                var list = (from t in context.TBL_PROFILE_USER
                            where t.ISACTIVE == true 
                            orderby t.USERID
                            select t.USERID).Take(maxUsers).ToList();

                var terminateUser = (from u in context.TBL_PROFILE_USER
                                     where !list.Contains(u.USERID)
                                     && u.ISACTIVE == true
                                     select u).ToList();

                foreach (var user in terminateUser)
                {
                    var lockUser = context.TBL_PROFILE_USER.Find(user.USERID);
                    lockUser.ISLOCKED = true;
                    lockUser.ISACTIVE = false;
                    lockUser.LASTLOCKOUTDATE = DateTime.Now;
                    lockUser.FAILEDLOGONATTEMPT = 3;
                }
                context.SaveChanges();
        }

        public string Decrypt(string cipher)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    using (var transform = tdes.CreateDecryptor())
                    {
                        byte[] cipherBytes = Convert.FromBase64String(cipher);
                        byte[] bytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                        return UTF8Encoding.UTF8.GetString(bytes);
                    }
                }
            }
        }


    }
}