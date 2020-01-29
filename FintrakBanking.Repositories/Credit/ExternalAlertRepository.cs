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
            .Select(d => d.GROUPCODE).Distinct().ToList();

            var staffList = (from s in context.TBL_STAFF
                             where groupHeadsEmails.Contains(s.MISCODE)
                             select new StaffInfoViewModel
                             {
                                 staffId = s.STAFFID,
                                 supervisorStaffId = s.SUPERVISOR_STAFFID,
                                 Email = s.EMAIL,
                                 misCode = s.MISCODE,
                             }).ToList();

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

        public IEnumerable<StaffInfoViewModel> GetPastDueObligationsReminderByGroupHeads()
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

        public IEnumerable<GlobalExposureViewModel> GetFacilityRestructuredNotification()
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

        public IEnumerable<GlobalExposureViewModel> GetSLAReport()
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

        public IEnumerable<GlobalExposureViewModel> GetPastDueDeferredDocuments()
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
        public IEnumerable<GlobalExposureViewModel> GetExpiredInsurancePolicies()
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
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => d.UNPODAYSOVERDUE > 0)
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

        public IEnumerable<GlobalExposureViewModel> GetStaffLoanPortfolioReport()
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
