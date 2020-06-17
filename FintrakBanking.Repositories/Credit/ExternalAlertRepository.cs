using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class ExternalAlertRepository : IExternalAlertRepository
    {
        // dependencies
        private FinTrakBankingContext context;
        private IAppraisalMemorandumRepository memo;
        private ILoanRepository loanRepo;
        private IFinanceTransactionRepository financeTransaction;
        private ICustomerGroupRepository groupRepo;
        private ITransactionDynamicsRepository transactionsRepo;
        private IConditionPrecedentRepository conditionsRepo;
        private ICustomerCollateralRepository collateralRepo;
        private IGeneralSetupRepository _genSetup;


        public ExternalAlertRepository(
            FinTrakBankingContext context,
            IAppraisalMemorandumRepository memo,
            ILoanRepository loanRepo,
            IFinanceTransactionRepository financeTransaction,
            ICustomerGroupRepository groupRepo,
            ITransactionDynamicsRepository transactionsRepo,
            IConditionPrecedentRepository conditionsRepo,
            ICustomerCollateralRepository collateralRepo,
           IGeneralSetupRepository genSetup
            )
        {
            this.context = context;
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
            var immenentMaturities = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
            .Select(d => d.ACCOUNTOFFICERCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where immenentMaturities.Contains(s.MISCODE)
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
            var groupHeadsEmails = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
            .Select(d => d.GROUPCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where groupHeadsEmails.Contains(s.MISCODE)
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
            var regionOfficers = context.TBL_GLOBAL_EXPOSURE.Where(d =>d.GROUPCODE == groupHeadCode)
            .Select(d => d.DIVISIONCODE).ToList();

            var staffList = (from s in context.TBL_STAFF
                             where regionOfficers.Contains(s.MISCODE)
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
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.ADJFACILITYTYPE=="OVERDRAFT")
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
             var data = context.TBL_GLOBAL_EXPOSURE.Where(d=>DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value == 30)
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
            var query = context.TBL_GLOBAL_EXPOSURE.Where(d => d.NPL >0)
             .Select(d => d.ACCOUNTOFFICERCODE).ToList();
            var staffList = (from s in context.TBL_STAFF
                             where query.Contains(s.MISCODE)
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
            if(query == null)
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

        public IEnumerable<GlobalExposureViewModel> GetExpiredValuationReport()
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

        public IEnumerable<GlobalExposureViewModel> GetValuationReminder()
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
            }
            return apps;
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
                               deferralDuration = 1,
                               cummulativeDays = 1,
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
                               isLMS = c.ISLMS == true,
                               reason = c.DEFERRALREASON,
                           }).ToList();

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

        public IEnumerable<GlobalExposureViewModel> GetSiteVisitationAccountReminder()
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

        public IEnumerable<GlobalExposureViewModel> GetCreditFileChecklistReminder()
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
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d=>days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
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

    }
}
