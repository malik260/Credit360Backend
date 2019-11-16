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
                id = (short)d.ID,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.FACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
                teamCode = d.TEAMCODE,
                lastCreditAmount = d.LASTCREDITAMOUNT,
                principalOutStandingBaltcy = d.PRINCIPALOUTSTANDINGBALTCY,
                principalOutStandingBallcy = d.PRINCIPALOUTSTANDINGBALLCY,
                loanAmounyLcy = d.LOANAMOUNYLCY,
                loanAmounyTcy = d.LOANAMOUNYTCY,
                cardLimit = d.CARDLIMIT,
                fxrate = d.FXRATE,
                shf = d.SHF,
                interestrate = d.INTERESTRATE,
                sectionedLoanLimitDirectFcy = d.SECTIONEDLOANLIMITDIRECTFCY,
                sectionedLoanLimitDirectLcy = d.SECTIONEDLOANLIMITDIRECTLCY,
                totalExposuLcyPreviousYear = d.TOTALEXPOSULCYPREVIOUSYEAR,
                totalExposuLcyPrevious6Months = d.TOTAEXPOSURELCYPREVIOUS6MONTHS,
                totalExposuLcyPrevious3Months = d.TOTAEXPOSURELCYPREVIOUS3MONTHS,
                totalExposuLcyPreviousMonths = d.TOTAEXPOSURELCYPREVIOUSMONTHS,
                totalExposuLcyPreviousDay = d.TOTAEXPOSURELCYPREVIOUSDAY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.UNPAIDOBLIGATIONAMOUNT,
                interestReceivableTcy = d.INTERESTRECEIVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();
        }

        public IEnumerable<GlobalExposureViewModel> GetImminentMaturities()
        {
            List<int> days = new List<int> { 60, 90, 30, 21, 14, 7, 3, 1 };
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
            .Select(d => new GlobalExposureViewModel
              {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,

                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.FACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
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
                sectionedLoanLimitDirectFcy = d.SECTIONEDLOANLIMITDIRECTFCY,
                sectionedLoanLimitDirectLcy = d.SECTIONEDLOANLIMITDIRECTLCY,
                totalExposuLcyPreviousYear = d.TOTALEXPOSULCYPREVIOUSYEAR,
                totalExposuLcyPrevious6Months = d.TOTAEXPOSURELCYPREVIOUS6MONTHS,
                totalExposuLcyPrevious3Months = d.TOTAEXPOSURELCYPREVIOUS3MONTHS,
                totalExposuLcyPreviousMonths = d.TOTAEXPOSURELCYPREVIOUSMONTHS,
                totalExposuLcyPreviousDay = d.TOTAEXPOSURELCYPREVIOUSDAY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.UNPAIDOBLIGATIONAMOUNT,
                interestReceivableTcy = d.INTERESTRECEIVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }


        public IEnumerable<GlobalExposureViewModel> GetCreditCardMaturingObligations()
        {
            List<int> days = new List<int> { 60, 89 };
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.FACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
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
                sectionedLoanLimitDirectFcy = d.SECTIONEDLOANLIMITDIRECTFCY,
                sectionedLoanLimitDirectLcy = d.SECTIONEDLOANLIMITDIRECTLCY,
                totalExposuLcyPreviousYear = d.TOTALEXPOSULCYPREVIOUSYEAR,
                totalExposuLcyPrevious6Months = d.TOTAEXPOSURELCYPREVIOUS6MONTHS,
                totalExposuLcyPrevious3Months = d.TOTAEXPOSURELCYPREVIOUS3MONTHS,
                totalExposuLcyPreviousMonths = d.TOTAEXPOSURELCYPREVIOUSMONTHS,
                totalExposuLcyPreviousDay = d.TOTAEXPOSURELCYPREVIOUSDAY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.UNPAIDOBLIGATIONAMOUNT,
                interestReceivableTcy = d.INTERESTRECEIVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetExpiringFacilityReport()
        {
            List<int> days = new List<int> { 90 };
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.FACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
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
                sectionedLoanLimitDirectFcy = d.SECTIONEDLOANLIMITDIRECTFCY,
                sectionedLoanLimitDirectLcy = d.SECTIONEDLOANLIMITDIRECTLCY,
                totalExposuLcyPreviousYear = d.TOTALEXPOSULCYPREVIOUSYEAR,
                totalExposuLcyPrevious6Months = d.TOTAEXPOSURELCYPREVIOUS6MONTHS,
                totalExposuLcyPrevious3Months = d.TOTAEXPOSURELCYPREVIOUS3MONTHS,
                totalExposuLcyPreviousMonths = d.TOTAEXPOSURELCYPREVIOUSMONTHS,
                totalExposuLcyPreviousDay = d.TOTAEXPOSURELCYPREVIOUSDAY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.UNPAIDOBLIGATIONAMOUNT,
                interestReceivableTcy = d.INTERESTRECEIVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetLoanExpirationReminder()
        {
            List<int> days = new List<int> { 30 };
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d =>
            days.Contains(DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value))
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.FACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
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
                sectionedLoanLimitDirectFcy = d.SECTIONEDLOANLIMITDIRECTFCY,
                sectionedLoanLimitDirectLcy = d.SECTIONEDLOANLIMITDIRECTLCY,
                totalExposuLcyPreviousYear = d.TOTALEXPOSULCYPREVIOUSYEAR,
                totalExposuLcyPrevious6Months = d.TOTAEXPOSURELCYPREVIOUS6MONTHS,
                totalExposuLcyPrevious3Months = d.TOTAEXPOSURELCYPREVIOUS3MONTHS,
                totalExposuLcyPreviousMonths = d.TOTAEXPOSURELCYPREVIOUSMONTHS,
                totalExposuLcyPreviousDay = d.TOTAEXPOSURELCYPREVIOUSDAY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.UNPAIDOBLIGATIONAMOUNT,
                interestReceivableTcy = d.INTERESTRECEIVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetUnAuthorizedOverdraftReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.FACILITYTYPE=="OVERDRAFT")
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.FACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
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
                sectionedLoanLimitDirectFcy = d.SECTIONEDLOANLIMITDIRECTFCY,
                sectionedLoanLimitDirectLcy = d.SECTIONEDLOANLIMITDIRECTLCY,
                totalExposuLcyPreviousYear = d.TOTALEXPOSULCYPREVIOUSYEAR,
                totalExposuLcyPrevious6Months = d.TOTAEXPOSURELCYPREVIOUS6MONTHS,
                totalExposuLcyPrevious3Months = d.TOTAEXPOSURELCYPREVIOUS3MONTHS,
                totalExposuLcyPreviousMonths = d.TOTAEXPOSURELCYPREVIOUSMONTHS,
                totalExposuLcyPreviousDay = d.TOTAEXPOSURELCYPREVIOUSDAY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.UNPAIDOBLIGATIONAMOUNT,
                interestReceivableTcy = d.INTERESTRECEIVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetOverlineMonitoringReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => d.FACILITYTYPE == "OVERDRAFT" && DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE)
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90) 
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.FACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
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
                sectionedLoanLimitDirectFcy = d.SECTIONEDLOANLIMITDIRECTFCY,
                sectionedLoanLimitDirectLcy = d.SECTIONEDLOANLIMITDIRECTLCY,
                totalExposuLcyPreviousYear = d.TOTALEXPOSULCYPREVIOUSYEAR,
                totalExposuLcyPrevious6Months = d.TOTAEXPOSURELCYPREVIOUS6MONTHS,
                totalExposuLcyPrevious3Months = d.TOTAEXPOSURELCYPREVIOUS3MONTHS,
                totalExposuLcyPreviousMonths = d.TOTAEXPOSURELCYPREVIOUSMONTHS,
                totalExposuLcyPreviousDay = d.TOTAEXPOSURELCYPREVIOUSDAY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.UNPAIDOBLIGATIONAMOUNT,
                interestReceivableTcy = d.INTERESTRECEIVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetCreditCardDelinquencyMonitoringReport()
        {
            var data = context.TBL_GLOBAL_EXPOSURE.Where(d => DbFunctions.TruncateTime(d.MATURITYDATE) == DbFunctions.TruncateTime(d.BOOKINGDATE) && d.FACILITYTYPE == "OVERDRAFT"
             && DbFunctions.DiffDays(DateTime.UtcNow, d.MATURITYDATE).Value > 90)
            .Select(d => new GlobalExposureViewModel
            {
                customerName = d.CUSTOMERNAME,
                accountOfficerName = d.ACCOUNTOFFICERNAME,
                accountNumber = d.ACCOUNTNUMBER,
                branchName = d.GROUPOBLIGORNAME,
                maturityDate = d.MATURITYDATE,
                id = d.ID,
                referenceNumber = d.REFERENCENUMBER,
                accountOfficerCode = d.ACCOUNTOFFICERCODE,
                date = d.DATE,
                customerId = d.CUSTOMERID,
                groupObligorName = d.GROUPOBLIGORNAME,
                alphaCode = d.ALPHACODE,
                productCode = d.PRODUCTCODE,
                currencyName = d.CURRENCYNAME,
                productName = d.PRODUCTNAME,
                facilityType = d.FACILITYTYPE,
                adjFacilityType = d.ADJFACILITYTYPE,
                adjFacilityTypeId = d.ADJFACILITYTYPEid,
                odStatus = d.ODSTATUS,
                currencyType = d.CURRENCYTYPE,
                cbnSector = d.CBNSECTOR,
                cbnSectorAdjusted = d.CBNSECTORADJUSTED,
                cbnClassification = d.CBNCLASSIFICATION,
                pwcClassification = d.PWCCLASSIFICATION,
                ifrsClassification = d.IFRSCLASSIFICATION,
                tenor = d.TENOR,
                location = d.LOCATION,
                bookingDate = d.BOOKINGDATE,
                valueDate = d.VALUEDATE,
                maturityBand = d.MATURITYBAND,
                customerType = d.CUSTOMERTYPE,
                branchCode = d.BRANCHCODE,
                obligorRiskRating = d.OBLIGORRISKRATING,
                lastCrDate = d.LASTCRDATE,
                productId = d.PRODUCTID,
                exposureType = d.EXPOSURETYPE,
                exposureTypeCode = d.EXPOSURETYPECODE,
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
                sectionedLoanLimitDirectFcy = d.SECTIONEDLOANLIMITDIRECTFCY,
                sectionedLoanLimitDirectLcy = d.SECTIONEDLOANLIMITDIRECTLCY,
                totalExposuLcyPreviousYear = d.TOTALEXPOSULCYPREVIOUSYEAR,
                totalExposuLcyPrevious6Months = d.TOTAEXPOSURELCYPREVIOUS6MONTHS,
                totalExposuLcyPrevious3Months = d.TOTAEXPOSURELCYPREVIOUS3MONTHS,
                totalExposuLcyPreviousMonths = d.TOTAEXPOSURELCYPREVIOUSMONTHS,
                totalExposuLcyPreviousDay = d.TOTAEXPOSURELCYPREVIOUSDAY,
                totalExposure = d.TOTALEXPOSURE,
                impairmentAmount = d.IMPAIRMENTAMOUNT,
                unpoInterestAmount = d.UNPOINTERESTAMOUNT,
                unpaidObligationAmount = d.UNPAIDOBLIGATIONAMOUNT,
                interestReceivableTcy = d.INTERESTRECEIVABLETCY,
                amountDue = d.AMOUNTDUE,
            }).ToList();

            return data;
        }

        public IEnumerable<GlobalExposureViewModel> GetPastDueObligationsReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetScheduleOfDirectorsAccounts()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetLcUtilizationReportOne()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetLcUtilizationReportTwo()
        {
            return null;
        }


        public IEnumerable<GlobalExposureViewModel> GetNplOnCreditPortfolio()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetOverlineCreditCardPosition()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetRiskAssetsReportNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetRiskAssetsReportReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetDashboardReportNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetDashboardReportReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCACReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetSignificantMovementInDailyRiskAsset()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetUSDCreditCardReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetNairaCreditCardReport()
        {
            return null;
        }
        public IEnumerable<GlobalExposureViewModel> GetCreditProgramsLimits()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetSchemePerformanceReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetExpiredValuationReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetExtentionReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCustomerStockTaking()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCustomerStockTakingReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetTranchPaymentReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetValuationReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetInsuranceReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetExtendedFacilityNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetEnhancedDisbursementToDirectorsNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetFacilityRestructuredNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetMccAndPpmcDeliverables()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetMccAndPpmcDeliverablesReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetPastDueMccAndPpmcDeliverablesReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetDSRAReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetSLAReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetOutstandingCreditDocumentation()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetSiteVisitationCustomerReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetPastDueDeferredDocuments()
        {
            return null;
        }
        public IEnumerable<GlobalExposureViewModel> GetExpiredInsurancePolicies()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetAMCONCollectionAndStatusReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetSiteVisitationAccountReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetEAndSRiskCategorisationDashboard()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetBankExposureEAndSExclusionListReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetGreenBondProceedsUtilizationReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetEandSConditionsPrecedentConfirmationReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetEandSConditionsSubsequentMonitoringReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetEandSCovenantDefaultReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetInvoiceConfirmationReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetStaockValuationReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetEndUseOfFundsReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCollateralVerificationReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCallMemoReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCreditFileChecklistReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetPendingCreditApprovalReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetEmployerDeliquencyReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetFacilitiesWithMissedPaymentReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetRunoffs()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCashFlowMonitoringReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetSalaryBackedLoans()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetTODStatusReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCreditFileWithIncompleteDocumentationReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetOutstandingCollateralDocumentation()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetAccountDeferralReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetContigentLiabilityReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetProcessedTransactionReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetPendingAndDeclinedTransactionReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetLoanCovenantsReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetVisitationAndSiteInspectionReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCollateralReleaseAndAccountDeclassification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetStockMonitoringReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetDSRAReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetPostDisbursementReview()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetMeetUpMonthlyTurnoverRequrements()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetMeetUpMonthlyTurnoverRequrementSMS()
        {
            return null;
        }


        public IEnumerable<GlobalExposureViewModel> GetBreachInCreditKeyMetricsNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetBreachInCreditKeyMetricsNotificationReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetAssignedDeliverableFromCACMeeting()
        {
            return null;
        }
        public IEnumerable<GlobalExposureViewModel> GetExpiredFacilityNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetLargeExposureAbove18PercentNotification()
        {
            return null;
        }
        public IEnumerable<GlobalExposureViewModel> GetBreachInGeographyLimitNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetBreachInORRLimitNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetBreachInSectorLimitNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetImminentObligationRentalScheduleNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetImminentObligationMaturityFacilityNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetOverlineFacilityNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetPastDueFacilitiesNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetLoanRepaymentReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetOverlineReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetMaturingObligationsReport()
        {
            return null;
        }
        public IEnumerable<GlobalExposureViewModel> GetLargeExposureMonitoring()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetUnpaidObligationReminder()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetStaffLoanPortfolioReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetExStaffLoanPortfolioReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetDigitalLoansReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetImminentMaturitiesAlertSMS()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetImminentMaturitiesAlertEmail()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetDelinquentCustomersAlertEmail()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetDelinquentCustomersAlertSMS()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCoreExposureForOneYearPeriodNotification()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetPotentialAndPipelineAssetToBeFinance()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCashBuildupReport()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCustomersWithNoInflows()
        {
            return null;
        }

        public IEnumerable<GlobalExposureViewModel> GetCustomersWithTurnoverLessthan100()
        {
            return null;
        }

    }
}
