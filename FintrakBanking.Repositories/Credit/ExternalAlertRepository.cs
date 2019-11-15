using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
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
        // place holders
        private readonly string customerNameHolder = "@{{customerName}}";
        private readonly string branchNameHolder = "@{{branchName}}";
        private readonly string descriptionNameHolder = "@{{description}}";
        private readonly string expiryDateHolder = "@{{expiryDate}}";
        private readonly string meetingDateHolder = "@{{meetingDate}}";
        private readonly string provideHolder = "@{{provideHolder}}";
        private readonly string dueDateHolder = "@{{dueDate}}";
        private readonly string accountNumberHolder = "@{{accountNumber}}";
        private readonly string accountOfficerNameHolder = "@{{accountOfficerName}}";
        
        
        // properties to have getter methods for interfacing
        private string customerName;
        private string branchName;
        private string description;
        private string expiryDate;
        private string meetingDate;
        private string provider;
        private string dueDate;
        private string accountNumber;
        private string accountOfficerName;

        private bool InitializeAlertProperties() // feeder
        {
            var debitPosition = context.TBL_GLOBAL_EXPOSURE.Where(x => x.TOTALEXPOSURE < 100);
            foreach (var d in debitPosition)
            {
                this.customerName = d.CUSTOMERNAME;
                this.accountOfficerName = d.ACCOUNTOFFICERNAME;
                this.accountNumber = d.ACCOUNTNUMBER;
                this.branchName = d.GROUPOBLIGORNAME;
                this.dueDate = d.MATURITYDATE;
                this.description = "N/A";
                this.provider = "N/A";
                this.meetingDate = DateTime.Now.ToShortDateString();

            }
            return true;
        }
        
       

        public string Replace(string content) // placeholders replace
        {
            content = content.Replace(customerNameHolder, customerName);
            content = content.Replace(branchNameHolder, branchName);
            content = content.Replace(descriptionNameHolder, description);
            content = content.Replace(expiryDateHolder, expiryDate);
            content = content.Replace(meetingDateHolder, meetingDate);
            content = content.Replace(provideHolder, provideHolder);
            content = content.Replace(dueDateHolder, dueDate);
            content = content.Replace(accountNumberHolder, accountNumber);
            content = content.Replace(accountOfficerNameHolder, accountOfficerName);

           return content;
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
        


    }
 }
