using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
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

        public IEnumerable<GlobalExposureViewModel> GetAllGlobalExposure()
        {
            return context.TBL_GLOBAL_EXPOSURE
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
                //facilityType = d.ADJFACILITYTYPE,
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
                //shf = d.SHF,
                //sectionedLoanLimitDirectFcy = d.SECTIONEDLOANLIMITDIRECTFCY,
                //sectionedLoanLimitDirectLcy = d.SECTIONEDLOANLIMITDIRECTLCY,
                //totalExposuLcyPreviousYear = d.TOTALEXPOSULCYPREVIOUSYEAR,
                //totalExposuLcyPrevious6Months = d.TOTAEXPOSURELCYPREVIOUS6MONTHS,
                //totalExposuLcyPrevious3Months = d.TOTAEXPOSURELCYPREVIOUS3MONTHS,
                //totalExposuLcyPreviousMonths = d.TOTAEXPOSURELCYPREVIOUSMONTHS,
                //totalExposuLcyPreviousDay = d.TOTAEXPOSURELCYPREVIOUSDAY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetImminentMaturities()
        {
            List<int> days = new List<int> { 60, 90, 30, 21, 14, 7, 3, 1 };
            var data = GetAllGlobalExposure().Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value));
            return data.ToList();
        }


        public IEnumerable<GlobalExposureViewModel> GetCreditCardMaturingObligations()
        {
            List<int> days = new List<int> { 60, 89 };
            var data = GetAllGlobalExposure().Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value));
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetExpiringFacilityReport()
        {
            List<int> days = new List<int> { 90 };
            var data = GetAllGlobalExposure().Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value));
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetLoanExpirationReminder()
        {
            List<int> days = new List<int> { 30 };
            var data = GetAllGlobalExposure().Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value));
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetUnAuthorizedOverdraftReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT");
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetOverlineMonitoringReport()
        {
            var data = GetAllGlobalExposure().Where(d => d.adjFacilityType == "OVERDRAFT" && DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate)
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90); 
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetCreditCardDelinquencyMonitoringReport()
        {
            var data = GetAllGlobalExposure().Where(d => d.unPoDaysOverdue > 0);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetPastDueObligationsReminder()
        {
            var data = GetAllGlobalExposure().Where(d => d.unPoDaysOverdue > 0);
               return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetScheduleOfDirectorsAccounts()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value == 30);
             return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetLcUtilizationReportOne()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
             return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetLcUtilizationReportTwo()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
             return data.ToList();
        }


        public IEnumerable<GlobalExposureViewModel> GetNplOnCreditPortfolio()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetOverlineCreditCardPosition()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
             return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetRiskAssetsReportNotification()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetRiskAssetsReportReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetDashboardReportNotification()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetDashboardReportReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetCACReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetSignificantMovementInDailyRiskAsset()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetUSDCreditCardReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetNairaCreditCardReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }
        public IEnumerable<GlobalExposureViewModel> GetCreditProgramsLimits()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetSchemePerformanceReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetExpiredValuationReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetExtentionReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetCustomerStockTaking()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetCustomerStockTakingReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetTranchPaymentReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetValuationReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetInsuranceReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetExtendedFacilityNotification()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetEnhancedDisbursementToDirectorsNotification()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetFacilityRestructuredNotification()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetMccAndPpmcDeliverables()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetMccAndPpmcDeliverablesReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetPastDueMccAndPpmcDeliverablesReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetDSRAReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetSLAReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetOutstandingCreditDocumentation()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetSiteVisitationCustomerReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetPastDueDeferredDocuments()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }
        public IEnumerable<GlobalExposureViewModel> GetExpiredInsurancePolicies()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetAMCONCollectionAndStatusReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetSiteVisitationAccountReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetEAndSRiskCategorisationDashboard()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetBankExposureEAndSExclusionListReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetGreenBondProceedsUtilizationReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetEandSConditionsPrecedentConfirmationReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetEandSConditionsSubsequentMonitoringReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetEandSCovenantDefaultReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetInvoiceConfirmationReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetStaockValuationReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetEndUseOfFundsReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetCollateralVerificationReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetCallMemoReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetCreditFileChecklistReminder()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetPendingCreditApprovalReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetEmployerDeliquencyReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetFacilitiesWithMissedPaymentReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetRunoffs()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetCashFlowMonitoringReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetSalaryBackedLoans()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetTODStatusReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetCreditFileWithIncompleteDocumentationReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetOutstandingCollateralDocumentation()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetAccountDeferralReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetContigentLiabilityReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetProcessedTransactionReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetPendingAndDeclinedTransactionReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetLoanCovenantsReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetVisitationAndSiteInspectionReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetCollateralReleaseAndAccountDeclassification()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetStockMonitoringReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
              && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetDSRAReport()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetPostDisbursementReview()
        {
            var data = GetAllGlobalExposure().Where(d => DbFunctions.TruncateTime(d.maturityDate) == DbFunctions.TruncateTime(d.bookingDate) && d.adjFacilityType == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.maturityDate).Value > 90);
            return data.ToList();
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
                //shf = d.SHF,
                //sectionedLoanLimitDirectFcy = d.SECTIONEDLOANLIMITDIRECTFCY,
                //sectionedLoanLimitDirectLcy = d.SECTIONEDLOANLIMITDIRECTLCY,
                //totalExposuLcyPreviousYear = d.TOTALEXPOSULCYPREVIOUSYEAR,
                //totalExposuLcyPrevious6Months = d.TOTAEXPOSURELCYPREVIOUS6MONTHS,
                //totalExposuLcyPrevious3Months = d.TOTAEXPOSURELCYPREVIOUS3MONTHS,
                //totalExposuLcyPreviousMonths = d.TOTAEXPOSURELCYPREVIOUSMONTHS,
                //totalExposuLcyPreviousDay = d.TOTAEXPOSURELCYPREVIOUSDAY,
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
                //shf = d.SHF,
               
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
                //shf = d.SHF,
                
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
                //shf = d.SHF,
                
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
                //shf = d.SHF,
               
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }
        public IEnumerable<GlobalExposureViewModel> GetExpiredFacilityNotification()
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
                //shf = d.SHF,
                
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
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
                //shf = d.SHF,
                
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
                //shf = d.SHF,
                
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
                //shf = d.SHF,
               
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
                //shf = d.SHF,
               
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
                //shf = d.SHF,
               
                  totalExposure = d.TOTALEXPOSURE,
                  impairmentAmount = d.IMPAIRMENTAMOUNT,
                  unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                  unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                  interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                  amountDue = d.AMOUNTDUE,
              }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetImminentObligationMaturityFacilityNotification()
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
                //shf = d.SHF,
                
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetOverlineFacilityNotification()
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
                //shf = d.SHF,
                
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetPastDueFacilitiesNotification()
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
                //shf = d.SHF,
                
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetLoanRepaymentReminder()
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
                //shf = d.SHF,
                
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetOverlineReminder()
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
                //shf = d.SHF,
               
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetMaturingObligationsReport()
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
                //shf = d.SHF,
                
                  totalExposure = d.TOTALEXPOSURE,
                  impairmentAmount = d.IMPAIRMENTAMOUNT,
                  unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                  unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                  interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                  amountDue = d.AMOUNTDUE,
              }).ToList();

            return data;
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
                //shf = d.SHF,
                
                 totalExposure = d.TOTALEXPOSURE,
                 impairmentAmount = d.IMPAIRMENTAMOUNT,
                 unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                 unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                 interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                 amountDue = d.AMOUNTDUE,
             }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetUnpaidObligationReminder()
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
                //shf = d.SHF,
               
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.TOTALUNPAIDOBLIGATION,
                interestReceivableTcy = d.INTERESTRECIEVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
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
                //shf = d.SHF,
                
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
                //shf = d.SHF,
               
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
                //shf = d.SHF,
                
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
                //shf = d.SHF,
               
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
                //shf = d.SHF,
                
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
                //shf = d.SHF,
               
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
                //shf = d.SHF,
               
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
                //shf = d.SHF,
                
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
                //shf = d.SHF,
                
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
                //shf = d.SHF,
               
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
                //shf = d.SHF,
               
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
                //shf = d.SHF,
               
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
