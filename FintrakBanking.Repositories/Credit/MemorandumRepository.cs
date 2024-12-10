using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Risk;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class MemorandumRepository : IMemorandumRepository
    {
        // dependencies
        private FinTrakBankingContext context;
        private ICreditLimitValidationsRepository limitValidation;
        //private ILoanRepository loanRepo;
        private IExposureRepository expRepo;
        private IFinanceTransactionRepository financeTransaction;
        private ITransactionDynamicsRepository transactionsRepo;
        private IConditionPrecedentRepository conditionsRepo;
        private ICustomerCollateralRepository collateralRepo;
        private IGeneralSetupRepository _genSetup;


        public MemorandumRepository(
            FinTrakBankingContext context, 
            ICreditLimitValidationsRepository limitValidation,
            //ILoanRepository loanRepo,
            IExposureRepository expRepo,
        IFinanceTransactionRepository financeTransaction,
            ITransactionDynamicsRepository transactionsRepo,
            IConditionPrecedentRepository conditionsRepo,
            ICustomerCollateralRepository collateralRepo,
           IGeneralSetupRepository genSetup
            )
        {
            this.context = context;
            //this.memo = memo;
            this.limitValidation = limitValidation;
            //this.loanRepo = loanRepo;
            this.expRepo = expRepo;
            this.financeTransaction = financeTransaction;
            //this.groupRepo = groupRepo;
            this.transactionsRepo = transactionsRepo;
            this.conditionsRepo = conditionsRepo;
            this.collateralRepo = collateralRepo;
            this._genSetup = genSetup;
        }

        // init
        private int targetId;
        private int operationId;
        public decimal LLL;

        List<CustomerExposure> customerIds; // init

        // field variables
        TBL_LOAN_APPLICATION loanApplication = null;
        TBL_LOAN_APPLICATION_DETAIL loanApplicationDetail = null;
        TBL_LMSR_APPLICATION_DETAIL lmsrApplicationDetail = null;
        TBL_LMSR_APPLICATION lmsrApplication = null;
        List<TBL_LOAN_APPLICATION_DETAIL> customerFacilities = null;
        List<TBL_LMSR_APPLICATION_DETAIL> customerFacilitiesLms = null;
        List<CurrentCustomerExposure> globalExposure = new List<CurrentCustomerExposure>();
        int customerId;
        //private List<int> lmsCamOperationIds = new List<int> { 46, 71, 79 };
        private List<int> lmsCamOperationIds = new List<int>();
        private long legalLendingLimit;
        //private long legalLendingLimit = 200000000000;
        
        // place holders
        private readonly string customerNameHolder = "@{{CustomerName}}";
        private readonly string branchNameHolder = "@{{Branch}}";
        private readonly string companyLogoHolder = "@{{companyLogo}}";
        private readonly string locationNameHolder = "@{{Location}}";
        private readonly string managementProfileHolder = "@{{ManagementProfile}}";
        private readonly string ownershipHolder = "@{{Ownership}}";
        private readonly string customerExposureHolder = "@{{CustomerExposure}}";
        private readonly string recommendedInterestRateHolder = "@{{RecommendedInterest}}";
        private readonly string isRelatedPartyHolder = "@{{IsRelatedParty}}";
        private readonly string dateCreatedHolder = "@{{DateCreated}}";
        private readonly string accountNumbersHolder = "@{{AccountNumbers}}";
        private readonly string approvalLevelHolder = "@{{ApprovalLevel}}";
        private readonly string environmentalSocialRiskHolder = "@{{EnvironmentalSocialRisk}}";
        private readonly string monitoringTriggersHolder = "@{{MonitoringTriggers}}";
        private readonly string proposedConditionsHolder = "@{{ProposedConditions}}";
        private readonly string conditionsPrecedenceListHolder = "@{{ConditionsPrecedenceList}}";
        private readonly string dynamicsListHolder = "@{{DynamicsList}}";
        private readonly string conditionsPrecedentToDrawdownHolder = "@{{ConditionsPrecedentToDrawdown}}";
        private readonly string transactionsDynamicsHolder = "@{{TransactionsDynamics}}";
        private readonly string isSecurityHolder = "@{{IsSecurity}}";
        private readonly string isOwnerOccupiedHolder = "@{{IsOwnerOccupied}}";
        private readonly string rmCountryHolder = "@{{Country}}";
        private readonly string misCodeHolder = "@{{MisCode}}";
        private readonly string reviewTypeHolder = "@{{ReviewType}}";
        private readonly string preparedByHolder = "@{{PreparedBy}}";
        private readonly string businessSectorsHolder = "@{{BusinessSectors}}";
        private readonly string exchangeRateHolder = "@{{ExchangeRate}}";
        private readonly string groupFacilitySummaryHolder = "@{{GroupFacilitySummary}}";
        private readonly string mccStampHolder = "@{{MCCDigitalStamp}}";
        private readonly string bccStampHolder = "@{{BCCDigitalStamp}}";

        private readonly string recoveryAnalysisHolder = "@{{RecoveryAnalysisData}}";
        private readonly string recoveryAnalysisFirmNameHolder = "@{{firmName}}";
        private readonly string recoveryAnalysisAddressHolder = "@{{address}}";
        private readonly string recoveryAnalysisDateHolder = "@{{recoveryDate}}";



        //private readonly string groupFacilitySummaryFcyHolder = "@{{GroupFacilitySummaryFcy}}";
        //private readonly string directFacilitiesHolder = "@{{DirectFacilities}}";
        //private readonly string totalDirectsHolder = "@{{TotalDirects}}";
        //private readonly string contingentFacilitiesHolder = "@{{ContingentFacilities}}";
        //private readonly string totalContingentsHolder = "@{{TotalContingents}}";
        //private readonly string importFinanceFacilitiesHolder = "@{{ImportFinanceFacilities}}";
        //private readonly string totalImportFinanceFacilitiesHolder = "@{{TotalImportFinanceFacilities}}";
        //private readonly string foreignDirectFacilitiesHolder = "@{{ForeignDirectFacilities}}";
        //private readonly string totalForeignDirectsHolder = "@{{TotalForeignDirects}}";
        //private readonly string foreignContingentFacilitiesHolder = "@{{ForeignContingentFacilities}}";
        //private readonly string totalForeignContingentsHolder = "@{{TotalForeignContingents}}";
        //private readonly string foreignImportFinanceFacilitiesHolder = "@{{ForeignImportFinanceFacilities}}";
        //private readonly string totalForeignImportFinanceFacilitiesHolder = "@{{TotalForeignImportFinanceFacilities}}";
        //private readonly string totalFacilitiesHolder = "@{{TotalFacilities}}";

        private readonly string groupExposureHolder = "@{{GroupExposure}}";
        private readonly string approvalsHolder = "@{{Approvals}}";
        private readonly string currentDateHolder = "@{{CurrentDate}}";
        private readonly string annualReviewDateHolder = "@{{AnnualReviewDate}}";
        private readonly string securityAnalysisHolder = "@{{SecurityAnalysis}}";
        private readonly string allCustomerCollateralRemarksHolder = "@{{AllCustomerCollateralRemarks}}";
        private readonly string collateralCoverageHolder = "@{{CollateralCoverage}}";
        private readonly string allCustomerFacilitiesHolder = "@{{AllCustomerFacilities}}";
        private readonly string obligorRiskRatingHolder = "@{{ObligorRiskRating}}";
        private readonly string obligorClassificationHolder = "@{{ObligorClassification}}";
        //private readonly string totalGroupExposureHolder = "@{{TotalGroupExposure}}";
        // lms only
        private readonly string securityTypeHolder = "@{{SecurityType}}";
        private readonly string securityDescriptionHolder = "@{{SecurityDescription}}";
        private readonly string securityFirstSellValueHolder = "@{{SecurityFirstSellValue}}";
        private readonly string securityLocationHolder = "@{{SecurityLocation}}";
        private readonly string securityOpenMarketValueHolder = "@{{SecurityOpenMarketValue}}";
        private readonly string securityPerfectionStatusHolder = "@{{SecurityPerfectionStatus}}";
        private readonly string securityValuationDateHolder = "@{{SecurityValuationDate}}";
        private readonly string shareHoldersHolder = "@{{ShareHolders}}";
        private readonly string signitoriesHolder = "@{{Signitories}}";
        private readonly string directorsHolder = "@{{Directors}}";
        private readonly string amountDisbursedHolder = "@{{AmountDisbursed}}";
        private readonly string amountPaidSoFarHolder = "@{{AmountPaidSoFar}}";
        private readonly string amountProposedHolder = "@{{AmountProposed}}";
        private readonly string customerTurnoverHolder = "@{{CustomerTurnover}}"; 

        // for output document 
        private readonly string memoHolder = "@{{memoData}}";
        private readonly string exceptionMemoHolder = "@{{exceptionMemoData}}";
        private readonly string facilityUpgradeSupportSchemeHolder = "@{{facilityUpgradeSupportSchemeData}}";
        private readonly string invoiceDiscountingDataHolder = "@{{invoiceDiscountingData}}";
        private readonly string cashCollaterizedDataHolder = "@{{cashCollaterizedData}}";
        private readonly string temporaryOverdraftHolder = "@{{temporaryOverdraftData}}";
        private readonly string staffCarLoansDataHolder = "@{{staffCarLoansData}}";
        private readonly string staffMortgageLoansDataHolder = "@{{staffMortgageLoansData}}";
        private readonly string staffPersonalLoansAGMDataHolder = "@{{staffPersonalLoansAGMData}}";
        private readonly string staffPersonalLoanDataHolder = "@{{staffPersonalLoanData}}";
        private readonly string documentatonDeferralWaiverDataHolder = "@{{documentatonDeferralWaiverData}}";

        //FUSS
        private readonly string fussCustomerInformationHolder = "@{{fussCustomerInformationData}}";
        private readonly string fussSchoolFeesInformationHolder = "@{{fussSchoolFeesInformationData}}";
        private readonly string fussCustomerFacilityHolder = "@{{fussCustomerFacilityData}}";
        private readonly string fussCustomerAccountActivityHolder = "@{{fussCustomerAccountActivityData}}";
        private readonly string fussCustomerAccountActivitySummaryHolder = "@{{fussCustomerAccountActivitySummaryData}}";
        private readonly string fussCashFlowAnalysisHolder = "@{{fussCashFlowAnalysisData}}";
        private readonly string fussSummaryNetCashFlowHolder = "@{{fussSummaryNetCashFlowData}}";
        private readonly string fussCurrentRequestHolder = "@{{fussCurrentRequestData}}";
        private readonly string fussBackgroungInformationHolder = "@{{fussBackgroungInformationData}}";
        private readonly string fussChecklistEligibilitytHolder = "@{{fussChecklistEligibilityData}}";

        private readonly string fussCustomerConditionSubsequentHolder = "@{{fussCustomerConditionSubsequentData}}";
        private readonly string fussCustomerConditionDynamicsHolder = "@{{fussCustomerConditionDynamicsData}}";

        //IDF
        private readonly string idfCustomerInformationHolder = "@{{idfCustomerInformationData}}";
        private readonly string idfCustomerFacilityHolder = "@{{idfCustomerFacilityData}}";
        private readonly string idfCustomerAccountActivityHolder = "@{{idfCustomerAccountActivityData}}";
        private readonly string idfCurrentRequestHolder = "@{{idfCurrentRequestData}}";
        private readonly string idfBackgroungInformationHolder = "@{{idfBackgroungInformationData}}";
        private readonly string idfChecklistEligibilitytHolder = "@{{idfChecklistEligibilityData}}";
        private readonly string idfDocumentationChecklistHolder = "@{{idfDocumentationChecklistData}}";

        //CASH COLLATERIZED
        private readonly string cashCollaterizedCustomerInformationHolder = "@{{cashCollaterizedCustomerInformationData}}";
        private readonly string cashCollaterizedCustomerFacilityHolder = "@{{cashCollaterizedCustomerFacilityData}}";
        private readonly string cashCollaterizedCustomerAccountActivityHolder = "@{{cashCollaterizedCustomerAccountActivityData}}";
        private readonly string cashCollaterizedCurrentRequestHolder = "@{{cashCollaterizedCurrentRequestData}}";
        private readonly string cashCollaterizedBackgroungInformationHolder = "@{{cashCollaterizedBackgroungInformationData}}";
        private readonly string cashCollaterizedChecklistEligibilitytHolder = "@{{cashCollaterizedChecklistEligibilityData}}";
        private readonly string cashCollaterizedDocumentationChecklistHolder = "@{{cashCollaterizedDocumentationChecklistData}}";

        //TOD
        private readonly string todHeaderHolder = "@{{todHeaderData}}";
        private readonly string todCustomerInformationHolder = "@{{todCustomerInformationData}}";
        private readonly string todCurrentRequestHolder = "@{{todCurrentRequestData}}";
        private readonly string todCustomerAccountActivityHolder = "@{{todCustomerAccountActivityData}}";
        private readonly string todCustomerFacilityHolder = "@{{todCustomerFacilityData}}";
        private readonly string todBackgroungInformationHolder = "@{{todBackgroungInformationData}}";
        private readonly string currentLMSFlowHolder = "@{{currentLMSFlowData}}";

        private readonly string originalDocumentNonCreditProgramHolder = "@{{originalDocumentNonCreditProgramData}}";
        private readonly string originalDocumentCreditProgramHolder = "@{{originalDocumentCreditProgramData}}";


        // properties to have getter methods for interfacing
        private string customerName;
        private string applicationReferenceNumber;
        private string branchName;
        private string locationName;
        private string managementProfile;
        private string ownership;
        private string customerExposure;
        private string recommendedInterestRate;
        private string isRelatedParty;
        private string dateCreated;
        private string accountNumbers;
        private string approvalLevel;
        private string environmentalSocialRisk;
        private string monitoringTriggers;
        private string proposedConditions;
        private string conditionsPrecedenceList;
        private string dynamicsList;
        private string conditionsPrecedentToDrawdown;
        private string transactionsDynamics;
        private string rmCountry;
        private string misCode;
        private string reviewType;
        private string preparedBy;
        private string relationshipOfficerName;
        private string relationshipManagerName;
        private string businessSectors;
        private string exchangeRate;
        private string groupFacilitySummary;
        private string mccDigitalStamp;
        private string bccDigitalStamp;
        private string groupFacilitySummaryFcy;
        private string companyLogo;
        //private string directFacilities;
        //private string totalDirectFacilities;
        //private string contingentFacilities;
        //private string totalContingentFacilities;
        //private string importFinanceFacilities;
        //private string totalImportFinanceFacilities;
        //private string foreignDirectFacilities;
        //private string totalForeignDirectFacilities;
        //private string foreignContingentFacilities;
        //private string totalForeignContingentFacilities;
        //private string foreignImportFinanceFacilities;
        //private string totalForeignImportFinanceFacilities;
        //private string totalFacilities;
        private string groupExposure;
        private string approvals;
        private bool isThirdPartyFacility;
        private TBL_CUSTOMER customerRecord;
        private string currentDate;
        private string annualReviewDate;
        private string securityAnalysis;
        private string allCustomerCollateralRemarks;
        private string collateralCoverage;
        private string allCustomerFacilities;
        private string obligorRiskRating;
        private string obligorClassification;
        //private string totalGroupExposure;
        // lms
        private string securityType;
        private string securityDescription;
        private string securityFirstSellValue;
        private string securityLocation;
        private string securityOpenMarketValue;
        private string securityPerfectionStatus;
        private string securityValuationDate;
        private string shareHolders;
        private string signitories;
        private string directors;
        private string isSecurity;
        private string isOwnerOccupied;
        private string amountDisbursed;
        private string amountPaidSoFar;
        private string amountProposed;
        private string customerTurnover;

        // for drawdown memo
        //private string customerName;
        private string currentAccountNo;
        //private string branchName;
        private string facilityType;
        private string drawdownAmount;
        public int tenor;

        public string approvedTenorString
        {
            get
            {
                var units = tenor == 1 ? " day" : " days";
                if (tenor < 15) return tenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(tenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }

        private int? moratorium;
        private string principalRepayment;
        private string interestRepayment;
        private double interestRate;
        private double processingFee;
        private double managementFee;
        private double commitmentFee;
        private double otherFee;
        private DateTime? effectiveDate;
        //private string misCode;

        private string approvedAmount;
        private string amountUtilised;
        private string newRequest;

        private string requestType;

        private string inPlace1;
        private string perfected1;
        private string deferred1;
        private string inPlace2;
        private string perfected2;
        private string deferred2;
        private string inPlace3;
        private string perfected3;
        private string deferred3;
        private string inPlace4;
        private string perfected4;
        private string deferred4;
        private string inPlace5;
        private string perfected5;
        private string deferred5;
        private string inPlace6;
        private string perfected6;
        private string deferred6;
        private string inPlace7;
        private string perfected7;
        private string deferred7;

        private string relationshipOfficer;
        private string relationshipManager;
        private string riskManagement;
        private string legal;
        private string treasury;
        private string coo;
        private string crmInternational;

        private string othersInPlace1;
        private string othersPerfected1;
        private string othersDeferred1;
        private string othersInPlace2;
        private string othersPerfected2;
        private string othersDeferred2;
        private string othersInPlace3;
        private string othersPerfected3;
        private string othersDeferred3;
        private string othersInPlace4;
        private string othersPerfected4;
        private string othersDeferred4;
        private string othersInPlace5;
        private string othersPerfected5;
        private string othersDeferred5;
        private string othersInPlace6;
        private string othersPerfected6;
        private string othersDeferred6;
        private string othersInPlace7;
        private string othersPerfected7;
        private string othersDeferred7;

        // out ducument properties definition
        private string memoData;
        private string exceptionMemoData;
        private string facilityUpgradeSupportSchemeData;
        private string invoiceDiscountingData;
        private string cashCollaterizedData;
        private string staffcarLoansData;
        private string staffMortgageLoansData;
        private string staffPersonalLoansAGMData;
        private string staffPersonalLoanData;
        private string temporaryOverdraftData;
        private string documentatonDeferralWaiverData;
        private int loanApplicationDetailId;
        private string cardType;
        //private string OfferLetterForBondsAndGuaranteesData;

        //FUSS
        private string fussCustomerInformationData;
        private string fussSchoolFeesInformationData;
        private string fussCustomerFacilityData;
        private string fussCustomerAccountActivityData;
        private string fussCustomerAccountActivitySummaryData;
        private string fussCashFlowAnalysisData;
        private string fussSummaryNetCashFlowData;
        private string fussCurrentRequestData;
        private string fussBackgroungInformationData;
        private string fussChecklistEligibilityData;
        private string fussCustomerConditionSubsequentData;
        private string fussCustomerConditionDynamicsData;

        //IDF
        private string idfCustomerInformationData;
        private string idfCustomerFacilityData;
        private string idfCustomerAccountActivityData;
        private string idfCurrentRequestData;
        private string idfBackgroungInformationData;
        private string idfChecklistEligibilityData;
        private string idfDocumentationChecklistData;

        //CASH COLLATERIZED
        private string cashCollaterizedCustomerInformationData;
        private string cashCollaterizedCustomerFacilityData;
        private string cashCollaterizedCustomerAccountActivityData;
        private string cashCollaterizedCurrentRequestData;
        private string cashCollaterizedBackgroungInformationData;
        private string cashCollaterizedChecklistEligibilityData;
        private string cashCollaterizedDocumentationChecklistData;

        //TOD
        private string todHeaderData;
        private string todCustomerInformationData;
        private string todCurrentRequestData;
        private string todCustomerAccountActivityData;
        private string todCustomerFacilityData;
        private string todBackgroungInformationData;
        private string currentLMSFlowData;

        private string originalDocumentNonCreditProgramData;
        private string originalDocumentCreditProgramData;
        private string recoveryAnalysisData;
        private string recoveryAnalysisFirmNameData;
        private string recoveryAnalysisAddressData;
        private string recoveryAnalysisDateData;


        // init
        public bool Init(int operationId, int targetId, bool showMccStamp = false, bool showBccStamp = false, bool isDrawdwon = false) // feeder
        {
            if (isDrawdwon)
            {
                return InitializeDrawdownMemoProperties(targetId,operationId);
            }

            this.targetId = targetId;
            this.operationId = operationId;
            this.lmsCamOperationIds = context.TBL_OPERATIONS.Where(o => o.OPERATIONTYPEID == (int)OperationTypeEnum.LoanReviewApplication).Select(o => o.OPERATIONID).ToList();
            if (operationId == (int)OperationsEnum.CreditAppraisal) // LOS 
            {
                if (loanApplication == null)
                {
                    this.loanApplication = context.TBL_LOAN_APPLICATION.Find(targetId);
                    this.customerIds = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerExposure { customerId = x.CUSTOMERID }).Distinct().ToList();
                    //this.customerExposure = CustomerExposureMarkup();
                }

                //string customerName = String.Empty;

                if (loanApplication != null) {
                    if (loanApplication.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.CustomerGroup)
                    {
                        this.customerName = loanApplication.TBL_CUSTOMER_GROUP.GROUPNAME;
                        this.customerId = (int)loanApplication.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID;
                    }
                    if (loanApplication.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.Single)
                    {
                        this.customerName = loanApplication.TBL_CUSTOMER.FIRSTNAME + " " + loanApplication.TBL_CUSTOMER.MIDDLENAME + " " + loanApplication.TBL_CUSTOMER.LASTNAME;
                        this.customerId = (int)loanApplication.CUSTOMERID;
                    }
                }

                this.companyLogo = $@"
                <table style='font face: arial; size:12px' border=0 width=1100 cellpadding=0 cellspacing=0>
                    <tr>
                        <td align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr></table>";
                this.customerRecord = context.TBL_CUSTOMER.Find(this.customerId);
                this.customerFacilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.DELETED == false && f.CUSTOMERID == this.customerId && f.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted && f.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.ApplicationRejected).ToList();
                this.branchName = loanApplication.TBL_BRANCH?.BRANCHNAME;
                if (loanApplication.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.CustomerGroup)
                {
                    var custId = loanApplication.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault()?.CUSTOMERID;
                    this.locationName = context.TBL_CUSTOMER_ADDRESS.FirstOrDefault(a => a.CUSTOMERID == custId)?.ADDRESS;
                }
                else
                if (loanApplication.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.Single)
                {
                    this.locationName = context.TBL_CUSTOMER_ADDRESS.FirstOrDefault(a => a.CUSTOMERID == loanApplication.CUSTOMERID)?.ADDRESS;
                }
                //this.locationName = loanApplication.TBL_BRANCH.ADDRESSLINE1 + " " + loanApplication.TBL_BRANCH.ADDRESSLINE2;
                this.isRelatedParty = loanApplication.ISRELATEDPARTY == true ? "Yes" : "No";
                this.recommendedInterestRate = loanApplication.INTERESTRATE.ToString();
                this.dateCreated = loanApplication.DATETIMECREATED.ToShortDateString();
                this.environmentalSocialRisk = GetEnvironmentalSocialRiskMarkup();
                this.conditionsPrecedentToDrawdown = GetConditionsPrecedentToDrawdownMarkup();
                this.transactionsDynamics = GetTransactionsDynamicsMarkup();
                this.rmCountry = this.loanApplication.TBL_COMPANY.TBL_COUNTRY.NAME;
                this.misCode = this.loanApplication.TBL_STAFF.MISCODE;
                var reviewTypeId = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().LOANDETAILREVIEWTYPEID;
                this.reviewType = context.TBL_LOAN_DETAIL_REVIEW_TYPE.Find(reviewTypeId)?.LOANDETAILREVIEWTYPENAME;
                this.preparedBy = this.loanApplication.TBL_STAFF.FIRSTNAME + " " + this.loanApplication.TBL_STAFF.LASTNAME;
                this.businessSectors = GetBusinessSectorsMarkupLOS();
                this.loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONID == this.loanApplication.LOANAPPLICATIONID).FirstOrDefault();
                this.facilityType = context.TBL_PRODUCT.Where(O => O.PRODUCTID == this.loanApplicationDetail.APPROVEDPRODUCTID).Select(O => O.PRODUCTNAME).FirstOrDefault();
                this.approvedAmount = this.loanApplicationDetail.APPROVEDAMOUNT.ToString("#,##.00");
                this.exchangeRate = GetAllExchangeRates();
                this.obligorRiskRating = GetCustomerRiskRating();
                this.obligorClassification = GetObligorClassification();
                this.legalLendingLimit = (long)loanApplication.TBL_COMPANY.SINGLEOBLIGORLIMIT;
                this.interestRate = loanApplicationDetail.APPROVEDINTERESTRATE;
                if (this.loanApplication.PRODUCT_CLASS_PROCESSID == (int)ProductClassProcessEnum.ProductBased)
                {
                    //continue
                    //this.globalExposure = GetGloabalExposures();
                }
                else
                {
                    this.globalExposure = GetGloabalExposures();
                }
                //this.groupFacilitySummaryFcy = GetGroupFacilitySummaryFCYMarkupLOS();
                //this.contingentFacilities = GetContingentFacilitiesMarkupLOS();
                //this.totalContingentFacilities = GetTotalContingentFacilitiesMarkupLOS();
                //ImportFinanceFinance;
                //totalImportFinanceFinance;
                //this.totalFacilities = GetTotalFacilitiesMarkupLOS();
                //this.foreignDirectFacilities = GetForeignDirectFacilitiesMarkupLOS();
                //this.totalForeignDirectFacilities = GetTotalForeignDirectFacilitiesMarkupLOS();
                //this.foreignContingentFacilities = GetForeignContingentFacilitiesMarkupLOS();
                //this.totalForeignContingentFacilities = GetTotalForeignContingentFacilitiesMarkupLOS();
                //foreignImportFinanceFinance;
                //totalForeignImportFinanceFinance;
                this.groupExposure = GetGroupExposureMarkup();
                this.approvals = GetApprovalsMarkup();
                this.currentDate = DateTime.Now.ToShortDateString();
                this.annualReviewDate = this.loanApplication.APPLICATIONDATE.AddYears(1).ToShortDateString();
                this.securityAnalysis = GetSecurityAnalysisMarkUP();
                this.collateralCoverage = GetCollateralCoverageMarkupLOS();
                this.allCustomerCollateralRemarks = GetAllCustomerCollateralsMarkup();
                this.allCustomerFacilities = GetAllCustomerFacilitiesMarkup();
                this.managementProfile = GetManagementProfileMarkup();
                this.ownership = GetOwnershipMarkup();
                this.groupFacilitySummary = GetGroupFacilitySummaryMarkupLOS();
                if(showMccStamp)this.mccDigitalStamp = GetMccStamp();
                if(showBccStamp)this.bccDigitalStamp = GetBccStamp();
                this.tenor = context.TBL_LOAN_APPLICATION_DETAIL.Where(t => t.LOANAPPLICATIONID == this.loanApplication.LOANAPPLICATIONID).Select(t => t.APPROVEDTENOR).FirstOrDefault();
                
                //this.totalGroupExposure = GetTotalGroupExposureMarkupLOS();

                this.memoData = MemoMarkupHtml();
                
                this.facilityUpgradeSupportSchemeData = FacilityUpgradeSupportSchemeHtml();
                this.invoiceDiscountingData = InvoiceDiscountingHtml();
                this.cashCollaterizedData = CashCollaterizedHtml();
                this.staffcarLoansData = StaffCarLoansHtml();
                this.staffMortgageLoansData = StaffMortgageLoansHtml();
                this.staffPersonalLoansAGMData = StaffPersonalLoanAGMHtml();
                this.staffPersonalLoanData = StaffPersonalLoanHtml();
                this.temporaryOverdraftData = TemporaryOverdraftHtml();

                //FUSS
                this.fussCustomerInformationData = FussCustomerInformationHtml();
                this.fussSchoolFeesInformationData = FussSchoolFeesInformationHtml();
                this.fussCustomerFacilityData = FussCustomerFacilityHtml();
                this.fussCustomerAccountActivityData = FussCustomerAccountActivityHtml();
                this.fussCustomerAccountActivitySummaryData = FussCustomerAccountActivitySummaryHtml();
                this.fussCashFlowAnalysisData = FussCashFlowAnalysisHtml();
                this.fussSummaryNetCashFlowData = FussSummaryNetCashFlowHtml();
                this.fussCurrentRequestData = FussCurrentRequestHtml();
                this.fussBackgroungInformationData = FussBackgroungInformationHtml();
                this.fussChecklistEligibilityData = FussChecklistEligibilityHtml();
                this.fussCustomerConditionSubsequentData = FussCustomerConditionSubsequentHtml();
                this.fussCustomerConditionDynamicsData = FussCustomerConditionDynamicsHtml();

                //IDF
                this.idfCustomerInformationData = IdfCustomerInformationHtml();
                this.idfCustomerFacilityData = IdfCustomerFacilityHtml();
                this.idfCustomerAccountActivityData = IdfCustomerAccountActivityHtml();
                this.idfCurrentRequestData = IdfCurrentRequestHtml();
                this.idfBackgroungInformationData = IdfBackgroungInformationHtml();
                this.idfChecklistEligibilityData = IdfChecklistEligibilityHtml();
                this.idfDocumentationChecklistData = IdfDocumentationChecklistHtml();

                //CASH COLLATERIZED
                this.cashCollaterizedCustomerInformationData = cashCollaterizedCustomerInformationHtml();
                this.cashCollaterizedCustomerFacilityData = IdfCustomerFacilityHtml();
                this.cashCollaterizedCustomerAccountActivityData = IdfCustomerAccountActivityHtml();
                this.cashCollaterizedCurrentRequestData = IdfCurrentRequestHtml();
                this.cashCollaterizedBackgroungInformationData = IdfBackgroungInformationHtml();
                this.cashCollaterizedChecklistEligibilityData = IdfChecklistEligibilityHtml();
                this.cashCollaterizedDocumentationChecklistData = IdfDocumentationChecklistHtml();

                //TOD 
                this.todHeaderData = TodHeaderHtml();
                this.todCustomerInformationData = TodCustomerInformationHtml();
                this.todCustomerFacilityData = TodCustomerFacilityHtml();
                this.todCustomerAccountActivityData = TodCustomerAccountActivityHtml();
                this.todCurrentRequestData = TodCurrentRequestHtml();
                this.todBackgroungInformationData = TodBackgroungInformationHtml();

                this.originalDocumentNonCreditProgramData = NoncreditProgramCustomerInformationHtml();
                this.originalDocumentCreditProgramData = CreditProgramCustomerInformationHtml();
            }

            if (lmsCamOperationIds.Contains(operationId)) // LMS
            {
                if (lmsrApplication == null)
                {
                    this.lmsrApplication = context.TBL_LMSR_APPLICATION.Find(targetId);
                    this.customerIds = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerExposure { customerId = x.CUSTOMERID }).Distinct().ToList();
                    //this.customerExposure = CustomerExposureMarkup();
                }

                this.companyLogo = $@"
                 <table style='font face: arial; size:12px' border=0 width=1100 cellpadding=0 cellspacing=0>
                    <tr>
                        <td align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr></table>";
                this.interestRate = context.TBL_LMSR_APPLICATION_DETAIL.Where(i => i.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).Select(i => i.APPROVEDINTERESTRATE).FirstOrDefault();
                //string customerName = String.Empty;
                this.customerRecord = context.TBL_CUSTOMER.Find(lmsrApplication.CUSTOMERID);
                // if (lmsrAppllication.CUSTOMERGROUPID != null) this.customerName = lmsrAppllication.TBL_CUSTOMER_GROUP.GROUPNAME;
                if (lmsrApplication.CUSTOMERID != null) this.customerName = lmsrApplication.TBL_CUSTOMER.FIRSTNAME + " " + lmsrApplication.TBL_CUSTOMER.MIDDLENAME + " " + lmsrApplication.TBL_CUSTOMER.LASTNAME;
                initLoanAppForLms();
                this.customerFacilitiesLms = context.TBL_LMSR_APPLICATION_DETAIL.Where(f => f.DELETED == false && f.CUSTOMERID == this.customerId && f.TBL_LMSR_APPLICATION.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted && f.TBL_LMSR_APPLICATION.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.ApplicationRejected).ToList();
                this.tenor = context.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).Select(t => t.APPROVEDTENOR).FirstOrDefault();
                this.branchName = lmsrApplication.TBL_BRANCH.BRANCHNAME;
                this.locationName = lmsrApplication.TBL_BRANCH.ADDRESSLINE1 + " " + lmsrApplication.TBL_BRANCH.ADDRESSLINE2;
                //this.isRelatedParty = lmsrAppllication.ISRELATEDPARTY == true ? "Yes" : "No";
                //this.recommendedInterestRate = lmsrAppllication.INTERESTRATE.ToString();
                this.dateCreated = lmsrApplication.DATETIMECREATED.ToShortDateString();
                this.rmCountry = context.TBL_COMPANY.Find(this.lmsrApplication.COMPANYID).TBL_COUNTRY.NAME;
                //this.misCode = this.lmsrAppllication.MISCODE;
                this.reviewType = "Annual";
                //this.preparedBy = this.lmsrAppllication.TBL_STAFF.FIRSTNAME + " " + this.lmsrAppllication.TBL_STAFF.LASTNAME;
                this.businessSectors = GetBusinessSectorsMarkupLMS();
                //this.exchangeRate = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault().EXCHANGERATE.ToString();
                this.obligorRiskRating = GetCustomerRiskRatingLMS();
                this.obligorClassification = GetObligorClassification();
                this.legalLendingLimit = (long)context.TBL_COMPANY.Find(lmsrApplication.COMPANYID).SINGLEOBLIGORLIMIT;
                this.exchangeRate = GetAllExchangeRatesLMS();
                //this.groupFacilitySummaryFcy = GetGroupFacilitySummaryFCYMarkupLOS();//this.directFacilities = GetDirectFacilitiesMarkupLMS();
                //this.totalDirectFacilities = GetTotalDirectFacilitiesMarkupLMS();
                //this.contingentFacilities = GetContingentFacilitiesMarkupLMS();
                //this.totalContingentFacilities = GetTotalContingentFacilitiesMarkupLMS();
                //ImportFinanceFinance;
                //totalImportFinanceFinance;
                //this.foreignDirectFacilities = GetForeignDirectFacilitiesMarkupLMS();
                //this.totalForeignDirectFacilities = GetTotalForeignDirectFacilitiesMarkupLMS();
                //this.foreignContingentFacilities = GetForeignContingentFacilitiesMarkupLMS();
                //this.totalForeignContingentFacilities = GetTotalForeignContingentFacilitiesMarkupLMS();
                //foreignImportFinanceFinance;
                //foreigntotalImportFinanceFinance;

                this.lmsrApplicationDetail = context.TBL_LMSR_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).FirstOrDefault();
                this.facilityType = context.TBL_PRODUCT.Where(O => O.PRODUCTID == this.lmsrApplicationDetail.PRODUCTID).Select(O => O.PRODUCTNAME).FirstOrDefault();
                this.approvedAmount = this.lmsrApplicationDetail.APPROVEDAMOUNT.ToString("#,##.00");



                this.globalExposure = GetExposuresLMS();
                this.groupExposure = GetGroupExposureMarkupLMS();
                //this.approvals = GetApprovalsMarkup();
                this.approvals = GetLMSApprovalsMarkup(this.lmsrApplication.LOANAPPLICATIONID, this.lmsrApplication.OPERATIONID);
                this.currentDate = DateTime.Now.ToShortDateString();
                this.annualReviewDate = this.lmsrApplication.APPLICATIONDATE.AddYears(1).ToShortDateString();
                this.securityAnalysis = GetSecurityAnalysisMarkUP();
                this.collateralCoverage = GetCollateralCoverageMarkupLOS();

                this.managementProfile = GetManagementProfileMarkup();
                this.allCustomerCollateralRemarks = GetAllCustomerCollateralsMarkup();
                
                this.ownership = GetOwnershipMarkup();
                //this.groupFacilitySummary = GetGroupFacilitySummaryMarkupLOS();
                this.conditionsPrecedentToDrawdown = FussCustomerConditionSubsequentHtml();
                this.transactionsDynamics = FussCustomerConditionDynamicsHtml(); //GetTransactionsDynamicsMarkup();
                
                this.allCustomerFacilities = GetAllCustomerFacilitiesMarkup();
                // out ducument properties definition
                this.memoData = MemoMarkupHtml();
                this.facilityUpgradeSupportSchemeData = FacilityUpgradeSupportSchemeHtml();
                this.invoiceDiscountingData = InvoiceDiscountingHtml();
                this.cashCollaterizedData = CashCollaterizedHtml();
                this.staffcarLoansData = StaffCarLoansHtml();
                this.staffMortgageLoansData = StaffMortgageLoansHtml();
                this.staffPersonalLoansAGMData = StaffPersonalLoanAGMHtml();
                this.staffPersonalLoanData = StaffPersonalLoanHtml();
                this.temporaryOverdraftData = TemporaryOverdraftHtml();

                //FUSS
                this.fussCustomerInformationData = FussCustomerInformationHtml();
                this.fussSchoolFeesInformationData = FussSchoolFeesInformationHtml();
                this.fussCustomerFacilityData = FussCustomerFacilityHtml();
                this.fussCustomerAccountActivityData = FussCustomerAccountActivityHtml();
                this.fussCustomerAccountActivitySummaryData = FussCustomerAccountActivitySummaryHtml();
                this.fussCashFlowAnalysisData = FussCashFlowAnalysisHtml();
                this.fussSummaryNetCashFlowData = FussSummaryNetCashFlowHtml();
                this.fussCurrentRequestData = FussCurrentRequestHtml();
                this.fussBackgroungInformationData = FussBackgroungInformationHtml();
                this.fussChecklistEligibilityData = FussChecklistEligibilityHtml();
                this.fussCustomerConditionSubsequentData = FussCustomerConditionSubsequentHtml();
                this.fussCustomerConditionDynamicsData = FussCustomerConditionDynamicsHtml();

                //IDF
                this.idfCustomerInformationData = IdfCustomerInformationHtml();
                this.idfCustomerFacilityData = IdfCustomerFacilityHtml();
                this.idfCustomerAccountActivityData = IdfCustomerAccountActivityHtml();
                this.idfCurrentRequestData = IdfCurrentRequestHtml();
                this.idfBackgroungInformationData = IdfBackgroungInformationHtml();
                this.idfChecklistEligibilityData = IdfChecklistEligibilityHtml();
                this.idfDocumentationChecklistData = IdfDocumentationChecklistHtml();

                //CASH COLLATERIZED
                this.cashCollaterizedCustomerInformationData = IdfCustomerInformationHtml();
                this.cashCollaterizedCustomerFacilityData = IdfCustomerFacilityHtml();
                this.cashCollaterizedCustomerAccountActivityData = IdfCustomerAccountActivityHtml();
                this.cashCollaterizedCurrentRequestData = IdfCurrentRequestHtml();
                this.cashCollaterizedBackgroungInformationData = IdfBackgroungInformationHtml();
                this.cashCollaterizedChecklistEligibilityData = IdfChecklistEligibilityHtml();
                this.cashCollaterizedDocumentationChecklistData = IdfDocumentationChecklistHtml();

                //TOD 
                this.todHeaderData = TodHeaderHtml();
                this.todCustomerInformationData = TodCustomerInformationHtml();
                this.todCustomerFacilityData = TodCustomerFacilityHtml();
                this.todCustomerAccountActivityData = TodCustomerAccountActivityHtml();
                this.todCurrentRequestData = TodCurrentRequestHtml();
                this.todBackgroungInformationData = TodBackgroungInformationHtml();
                this.currentLMSFlowData = CurrentLMSFlowHtml();

                this.originalDocumentNonCreditProgramData = NoncreditProgramCustomerInformationHtml();
                this.originalDocumentCreditProgramData = CreditProgramCustomerInformationHtml();


                // cam
                var cam = ClassifiedAssetManagementReview(lmsrApplication.APPLICATIONREFERENCENUMBER);

                if (cam != null)
                {
                    this.securityType = cam.securityType;
                    this.securityDescription = cam.securityDescription;
                    this.securityFirstSellValue = cam.securityFirstSellValue.ToString();
                    this.securityLocation = cam.securityLocation;
                    this.securityOpenMarketValue = cam.securityOpenMarketValue.ToString();
                    this.securityPerfectionStatus = cam.securityPerfectionStatus.ToString();
                    this.securityValuationDate = cam.securityValuationDate.ToString();
                    this.shareHolders = cam.shareHolders;
                    this.signitories = cam.signitories;
                    this.directors = cam.directors;
                    this.isSecurity = cam.isResidential == true ? "Yes" : "No";
                    this.isOwnerOccupied = cam.isOwnerOccupied == true ? "Yes" : "No";
                    this.amountDisbursed = cam.amountDisbursed.ToString();
                    this.amountPaidSoFar = cam.amountPaidSoFar.ToString();
                    this.amountProposed = cam.amountProposed.ToString();
                }
            }

            if (this.customerIds?.Count > 0)
            {
                this.accountNumbers = AccountNumbersMarkup(this.customerIds?.Select(x => x.customerId).ToList());
            }

            this.approvalLevel = GetApprovalLevel();
            this.proposedConditions = GetProposedConditionsMarkup();
            this.conditionsPrecedenceList = GetConditionsMarkUp();
            this.dynamicsList = GetDynamicsMarkUp();
            this.monitoringTriggers = MonitoringTriggersMarkup();
            

            //this.customerTurnover = CustomerTurnoverMarkup(); // lazy loaded

            return true;
        }

        public bool InitGenericMemo(int operationId, int targetId, int targetIdForWorkFlow, int customerId) // feeder
        {
            this.targetId = targetId;
            this.operationId = operationId;
            var deferralOperations = new List<int>();
            deferralOperations.Add((int)OperationsEnum.DeferralExtension);
            deferralOperations.Add((int)OperationsEnum.ProvisionOfDeferredDocument);
            deferralOperations.Add((int)OperationsEnum.DefferedChecklistApproval);
            deferralOperations.Add((int)OperationsEnum.WaivedChecklistApproval);
            //if (operationId == (int)OperationsEnum.CollateralSwap)
            //{
            //    this.customerId = context.TBL_COLLATERAL_SWAP_REQUEST.FirstOrDefault(s => s.COLLATERALSWAPID == targetId)?.CUSTOMERID ?? 0;
            //}
            //if (operationId == (int)OperationsEnum.OriginalDocumentApproval)
            //{
            //    this.customerId = context.TBL_COLLATERAL_SWAP_REQUEST.FirstOrDefault(s => s.COLLATERALSWAPID == targetId)?.CUSTOMERID ?? 0;
            //}
            //if (operationId == (int)OperationsEnum.SecurityRelease)
            //{
            //    this.customerId = context.TBL_COLLATERAL_SWAP_REQUEST.FirstOrDefault(s => s.COLLATERALSWAPID == targetId)?.CUSTOMERID ?? 0;
            //}
            //if (operationId == (int)OperationsEnum.LienRemoval)
            //{
            //    this.customerId = context.TBL_COLLATERAL_SWAP_REQUEST.FirstOrDefault(s => s.COLLATERALSWAPID == targetId)?.CUSTOMERID ?? 0;
            //}
            //if (operationId == (int)OperationsEnum.lcIssuance)
            //{
            //    this.customerId = context.TBL_LC_ISSUANCE.FirstOrDefault(s => s.LCISSUANCEID == targetId)?.CUSTOMERID ?? 0;
            //}
            //if (deferralOperations.Contains(operationId))
            //{
            //    this.customerId = context.TBL_COLLATERAL_SWAP_REQUEST.FirstOrDefault(s => s.COLLATERALSWAPID == targetId)?.CUSTOMERID ?? 0;
            //}

            if (customerId == 0)
            {
                throw new Exception("Customer Id cannot be null!");
            }

            this.customerId = customerId;

            if (this.customerId > 0)
            {
                this.customerIds = new List<CustomerExposure>();
                this.customerIds.Add(new CustomerExposure { customerId = this.customerId });
                var customer = context.TBL_CUSTOMER.Find(this.customerId);
                this.customerName = customer?.FIRSTNAME + " " + customer?.MIDDLENAME + " " + customer?.LASTNAME;
                this.customerFacilities = new List<TBL_LOAN_APPLICATION_DETAIL>();
                this.customerFacilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.DELETED == false && f.CUSTOMERID == this.customerId && f.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted && f.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.ApplicationRejected).ToList();
                this.locationName = context.TBL_CUSTOMER_ADDRESS.FirstOrDefault(a => a.CUSTOMERID == this.customerId)?.ADDRESS;
                this.fussCustomerConditionSubsequentData = GenericConditionSubsequentHtml();
                this.fussCustomerConditionDynamicsData = GenericConditionDynamicsHtml();
                this.memoData = GenericMemoMarkupHtml();
                if (targetIdForWorkFlow > 0)
                {
                    this.approvals = GetGenericApprovalsMarkup(targetIdForWorkFlow, true);
                }
                else
                {
                    this.approvals = GetGenericApprovalsMarkup(this.targetId, true);
                }
                this.currentDate = DateTime.Now.ToShortDateString();
                if (this.customerIds?.Count > 0)
                {
                    this.accountNumbers = AccountNumbersMarkup(this.customerIds?.Select(x => x.customerId).ToList());
                }
            }
            return true;
        }

        private void initLoanAppForLms()
        {
            var loanId = this.lmsrApplication.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault().LOANID;
            int lmsrSystemType = this.lmsrApplication.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault().LOANSYSTEMTYPEID;
            int loanAppId;
            int detailId;
            if (lmsrSystemType == (int)LoanSystemTypeEnum.TermDisbursedFacility)
            {
                detailId = context.TBL_LOAN.Find(loanId).LOANAPPLICATIONDETAILID;
                loanAppId = context.TBL_LOAN_APPLICATION_DETAIL.Find(detailId).LOANAPPLICATIONID;
                this.loanApplication = context.TBL_LOAN_APPLICATION.Find(loanAppId);
            }
            if (lmsrSystemType == (int)LoanSystemTypeEnum.OverdraftFacility)
            {
                detailId = context.TBL_LOAN_REVOLVING.Find(loanId).LOANAPPLICATIONDETAILID;
                loanAppId = context.TBL_LOAN_APPLICATION_DETAIL.Find(detailId).LOANAPPLICATIONID;
                this.loanApplication = context.TBL_LOAN_APPLICATION.Find(loanAppId);
            }
            if (lmsrSystemType == (int)LoanSystemTypeEnum.ContingentLiability)
            {
                detailId = context.TBL_LOAN_CONTINGENT.Find(loanId).LOANAPPLICATIONDETAILID;
                loanAppId = context.TBL_LOAN_APPLICATION_DETAIL.Find(detailId).LOANAPPLICATIONID;
                this.loanApplication = context.TBL_LOAN_APPLICATION.Find(loanAppId);
            }
            if (lmsrSystemType == (int)LoanSystemTypeEnum.LineFacility)
            {
                detailId = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanId).LOANAPPLICATIONDETAILID;
                loanAppId = context.TBL_LOAN_APPLICATION_DETAIL.Find(detailId).LOANAPPLICATIONID;
                this.loanApplication = context.TBL_LOAN_APPLICATION.Find(loanAppId);
            }
            if (loanApplication != null)
          {
                if (loanApplication.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.CustomerGroup)
                {
                    this.customerName = loanApplication.TBL_CUSTOMER_GROUP.GROUPNAME;
                    this.customerId = (int)loanApplication.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID;
                }
                if (loanApplication.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.Single)
                {
                    this.customerName = loanApplication.TBL_CUSTOMER.FIRSTNAME + " " + loanApplication.TBL_CUSTOMER.MIDDLENAME + " " + loanApplication.TBL_CUSTOMER.LASTNAME;
                    this.customerId = (int)loanApplication.CUSTOMERID;
                }
            }
        }

        private bool InitializeDrawdownMemoProperties(int targetId, int operationId, int bookingRequestId = 0) // feeder
        {
            this.targetId = targetId;
            this.operationId = operationId;
            var loanDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(targetId);
            if (loanDetail != null)
            {
                this.loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanDetail.LOANAPPLICATIONDETAILID);
                this.loanApplication = context.TBL_LOAN_APPLICATION.Where(l=>l.LOANAPPLICATIONID == loanApplicationDetail.LOANAPPLICATIONID).FirstOrDefault();
            }
            //else
            //{
            //    var loanApplication = context.TBL_LOAN_APPLICATION.Find(targetId);
            //    this.loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x=>x.LOANAPPLICATIONID == loanApplication.LOANAPPLICATIONID).FirstOrDefault();
            //}


            var chargeFeeId = context.TBL_LOAN_APPLICATION_DETL_FEE.FirstOrDefault(f => f.LOANAPPLICATIONDETAILID == targetId)?.CHARGEFEEID;

                //this.documentatonDeferralWaiverData = DocumentationDeferralWaiverFormHtml();
                this.companyLogo = $@"
                <table style='font face: arial; size:12px' border=01 width=1100 cellpadding=0 cellspacing=0>
                <tr>
                    <td align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                </tr></table>";
            string customerName = String.Empty;
            if (this.loanApplication?.CUSTOMERGROUPID != null)
            {
                this.customerName = this.loanApplication.TBL_CUSTOMER_GROUP.GROUPNAME;
            }
            if (this.loanApplication?.CUSTOMERID != null)
            {
                this.customerName = loanApplication.TBL_CUSTOMER.FIRSTNAME + " " + loanApplication.TBL_CUSTOMER.MIDDLENAME + " " + loanApplication.TBL_CUSTOMER.LASTNAME;
            }
            this.applicationReferenceNumber = loanApplication.APPLICATIONREFERENCENUMBER;
            this.branchName = loanApplication.TBL_BRANCH.BRANCHNAME;
            this.locationName = loanApplication.TBL_BRANCH.ADDRESSLINE1 + " " + loanApplication.TBL_BRANCH.ADDRESSLINE2;
            this.currentAccountNo = context.TBL_CASA.Where(O => O.CASAACCOUNTID == loanApplicationDetail.OPERATINGCASAACCOUNTID).Select(O => O.PRODUCTACCOUNTNUMBER).FirstOrDefault()?? "N/A";
            this.facilityType = context.TBL_PRODUCT.Where(O => O.PRODUCTID == loanApplicationDetail.APPROVEDPRODUCTID).Select(O => O.PRODUCTNAME).FirstOrDefault();
            this.drawdownAmount = loanApplicationDetail?.APPROVEDAMOUNT.ToString("#,##.00");
            this.tenor = loanApplicationDetail.APPROVEDTENOR;
            this.moratorium = loanApplicationDetail.MORATORIUMDURATION;
            this.principalRepayment = "";
            this.interestRepayment = loanApplicationDetail.REPAYMENTTERMS;
            this.interestRate = loanApplicationDetail.APPROVEDINTERESTRATE;
            this.otherFee = (context.TBL_CHARGE_FEE_DETAIL.FirstOrDefault(p => p.CHARGEFEEID == chargeFeeId)?.VALUE) ?? 0; 
            this.effectiveDate = loanApplication.APPROVEDDATE;
            this.loanApplicationDetailId = loanApplicationDetail.LOANAPPLICATIONDETAILID;
            this.misCode = loanApplicationDetail.TBL_LOAN_APPLICATION.TBL_STAFF.MISCODE;
            this.currentDate = DateTime.Now.ToShortDateString();
            this.preparedBy = loanApplication.TBL_STAFF.FIRSTNAME + " " + loanApplication.TBL_STAFF.MIDDLENAME + " " + loanApplication.TBL_STAFF.LASTNAME;
            this.relationshipOfficerName = loanApplication.TBL_STAFF.FIRSTNAME + " " + loanApplication.TBL_STAFF.MIDDLENAME + " " + loanApplication.TBL_STAFF.LASTNAME;
            //this.relationshipManagerName = loanApplication.TBL_STAFF1.FIRSTNAME + " " + loanApplication.TBL_STAFF1.MIDDLENAME + " " + loanApplication.TBL_STAFF1.LASTNAME;
            this.approvedAmount = loanApplicationDetail.APPROVEDAMOUNT.ToString("#,##.00");
            if (bookingRequestId > 0)
            {
                var currentRequest = context.TBL_LOAN_BOOKING_REQUEST.Where(O => O.LOAN_BOOKING_REQUESTID == bookingRequestId).FirstOrDefault();
                var allRequests = context.TBL_LOAN_BOOKING_REQUEST.Where(O => O.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID).ToList();
                amountUtilised = allRequests.Where(r => r.LOAN_BOOKING_REQUESTID != bookingRequestId && r.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved)?.Sum(r => r.AMOUNT_REQUESTED).ToString("#,##.00") ?? "0.00";
                newRequest = currentRequest?.AMOUNT_REQUESTED.ToString("#,##.00") ?? "0.00";
                if (currentRequest?.PRODUCTID != null)
                {
                    this.facilityType = context.TBL_PRODUCT.Where(O => O.PRODUCTID == (currentRequest.PRODUCTID)).Select(O => O.PRODUCTNAME).FirstOrDefault();
                }

            }
            else
            {
                amountUtilised = "0.00";
                newRequest = context.TBL_LOAN_BOOKING_REQUEST.Where(O => O.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID).FirstOrDefault()?.AMOUNT_REQUESTED.ToString("#,##.00") ?? "0.00";
            }

            return true;
        }

        private bool InitializeCashBackMemoProperties(int operationId, int targetId) // feeder
        {
            this.targetId = targetId;
            this.operationId = operationId;

            if (loanApplicationDetail == null)
            {
                this.loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(targetId);
                this.loanApplication = loanApplicationDetail.TBL_LOAN_APPLICATION;
            }

            //var chargeFeeId = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(f=>f.LOANAPPLICATIONDETAILID == targetId).Select(f=>f.CHARGEFEEID).ToList();
            this.companyLogo = $@"
                 <table style='font face: arial; size:12px' border=0 width=1100 cellpadding=0 cellspacing=0>
                    <tr>
                        <td align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr></table>";
            string customerName = String.Empty;
            if (loanApplication.CUSTOMERGROUPID != null) this.customerName = loanApplication.TBL_CUSTOMER_GROUP.GROUPNAME;
            if (loanApplication.CUSTOMERID != null) this.customerName = loanApplication.TBL_CUSTOMER.FIRSTNAME + " " + loanApplication.TBL_CUSTOMER.MIDDLENAME + " " + loanApplication.TBL_CUSTOMER.LASTNAME;
            this.applicationReferenceNumber = loanApplication.APPLICATIONREFERENCENUMBER;
            this.branchName = loanApplication.TBL_BRANCH.BRANCHNAME;
            this.locationName = loanApplication.TBL_BRANCH.ADDRESSLINE1 + " " + loanApplication.TBL_BRANCH.ADDRESSLINE2;
            this.facilityType = context.TBL_PRODUCT.Where(O => O.PRODUCTID == loanApplicationDetail.APPROVEDPRODUCTID).Select(O => O.PRODUCTNAME).FirstOrDefault();
            this.principalRepayment = "";
            //this.processingFee = context.TBL_CHARGE_FEE_DETAIL.Where(p => p.CHARGEFEEID == chargeFeeId[0]).Select(p => p.VALUE).FirstOrDefault(); //""; //context.TBL_LOAN_APPLICATION_DETL_FEE.Where(O => O.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID).Select(O => O.TBL_CHARGE_FEE).FirstOrDefault();
            //this.managementFee = context.TBL_CHARGE_FEE_DETAIL.Where(p => p.CHARGEFEEID == chargeFeeId[1]).Select(p => p.VALUE).FirstOrDefault();
            //this.commitmentFee = context.TBL_CHARGE_FEE_DETAIL.Where(p => p.CHARGEFEEID == chargeFeeId[2]).Select(p => p.VALUE).FirstOrDefault();
            //this.otherFee = context.TBL_CHARGE_FEE_DETAIL.Where(p => p.CHARGEFEEID == chargeFeeId[3]).Select(p => p.VALUE).FirstOrDefault(); ;
            this.effectiveDate = loanApplication.APPROVEDDATE;
            this.currentDate = DateTime.Now.ToShortDateString();
            this.preparedBy = loanApplication.TBL_STAFF.FIRSTNAME + " " + loanApplication.TBL_STAFF.MIDDLENAME + " " + loanApplication.TBL_STAFF.LASTNAME;
            this.relationshipOfficerName = loanApplication.TBL_STAFF.FIRSTNAME + " " + loanApplication.TBL_STAFF.MIDDLENAME + " " + loanApplication.TBL_STAFF.LASTNAME;
            this.approvedAmount = loanApplicationDetail.APPROVEDAMOUNT.ToString("#,##.00");
            amountUtilised = "0.00";
           return true;
        }

        public bool InitForThirdpartyLoans(int operationId, int targetId) // feeder
        {
            this.targetId = targetId;
            this.operationId = operationId;
            this.isThirdPartyFacility = true;
            this.lmsCamOperationIds = context.TBL_OPERATIONS.Where(o => o.OPERATIONTYPEID == (int)OperationTypeEnum.LoanReviewApplication).Select(o => o.OPERATIONID).ToList();
            if (lmsCamOperationIds.Contains(operationId)) // LMS
            {
                if (lmsrApplication == null)
                {
                    this.lmsrApplication = context.TBL_LMSR_APPLICATION.Find(targetId);
                    this.customerIds = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerExposure { customerId = x.CUSTOMERID }).Distinct().ToList();
                    //this.customerExposure = CustomerExposureMarkup();
                }

                this.companyLogo = $@"
                 <table style='font face: arial; size:12px' border=0 width=1100 cellpadding=0 cellspacing=0>
                    <tr>
                        <td align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr></table>";
                this.interestRate = context.TBL_LMSR_APPLICATION_DETAIL.Where(i => i.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).Select(i => i.APPROVEDINTERESTRATE).FirstOrDefault();
                this.customerRecord = context.TBL_CUSTOMER.Find(lmsrApplication.CUSTOMERID);
                // if (lmsrAppllication.CUSTOMERGROUPID != null) this.customerName = lmsrAppllication.TBL_CUSTOMER_GROUP.GROUPNAME;
                if (lmsrApplication.CUSTOMERID != null) this.customerName = lmsrApplication.TBL_CUSTOMER.FIRSTNAME + " " + lmsrApplication.TBL_CUSTOMER.MIDDLENAME + " " + lmsrApplication.TBL_CUSTOMER.LASTNAME;
                this.customerFacilitiesLms = context.TBL_LMSR_APPLICATION_DETAIL.Where(f => f.DELETED == false && f.CUSTOMERID == this.customerId && f.TBL_LMSR_APPLICATION.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted && f.TBL_LMSR_APPLICATION.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.ApplicationRejected).ToList();
                this.tenor = context.TBL_LMSR_APPLICATION_DETAIL.Where(t => t.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).Select(t => t.APPROVEDTENOR).FirstOrDefault();
                this.branchName = lmsrApplication.TBL_BRANCH.BRANCHNAME;
                this.locationName = lmsrApplication.TBL_BRANCH.ADDRESSLINE1 + " " + lmsrApplication.TBL_BRANCH.ADDRESSLINE2;
                //this.isRelatedParty = lmsrAppllication.ISRELATEDPARTY == true ? "Yes" : "No";
                //this.recommendedInterestRate = lmsrAppllication.INTERESTRATE.ToString();
                this.dateCreated = lmsrApplication.DATETIMECREATED.ToShortDateString();
                this.rmCountry = context.TBL_COMPANY.Find(this.lmsrApplication.COMPANYID).TBL_COUNTRY.NAME;
                //this.misCode = this.lmsrAppllication.MISCODE;
                this.reviewType = "Annual";
                //this.preparedBy = this.lmsrAppllication.TBL_STAFF.FIRSTNAME + " " + this.lmsrAppllication.TBL_STAFF.LASTNAME;
                this.businessSectors = GetBusinessSectorsMarkupLMS();
                //this.exchangeRate = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault().EXCHANGERATE.ToString();
                this.obligorRiskRating = GetCustomerRiskRatingLMS();
                this.obligorClassification = GetObligorClassification();
                this.legalLendingLimit = (long)context.TBL_COMPANY.Find(lmsrApplication.COMPANYID).SINGLEOBLIGORLIMIT;
                this.exchangeRate = GetAllExchangeRatesLMS();

                this.groupExposure = GetGroupExposureMarkupLMS();
                this.approvals = GetApprovalsMarkup();
                this.currentDate = DateTime.Now.ToShortDateString();
                this.annualReviewDate = this.lmsrApplication.APPLICATIONDATE.AddYears(1).ToShortDateString();
                this.securityAnalysis = GetSecurityAnalysisMarkUP();
                //this.collateralCoverage = GetCollateralCoverageMarkupLOS();
                this.managementProfile = GetManagementProfileMarkup();
                this.allCustomerCollateralRemarks = GetAllCustomerCollateralsMarkup();
                this.ownership = GetOwnershipMarkup();
                this.groupFacilitySummary = GetGroupFacilitySummaryMarkupLOS();
                
                this.allCustomerFacilities = GetAllCustomerFacilitiesMarkup();

                
                this.conditionsPrecedentToDrawdown = FussCustomerConditionSubsequentHtml();
                this.transactionsDynamics = FussCustomerConditionDynamicsHtml(); //GetTransactionsDynamicsMarkup();

                // out ducument properties definition
                this.memoData = MemoMarkupHtml();
                this.facilityUpgradeSupportSchemeData = FacilityUpgradeSupportSchemeHtml();
                this.invoiceDiscountingData = InvoiceDiscountingHtml();
                this.cashCollaterizedData = CashCollaterizedHtml();
                this.staffcarLoansData = StaffCarLoansHtml();
                this.staffMortgageLoansData = StaffMortgageLoansHtml();
                this.staffPersonalLoansAGMData = StaffPersonalLoanAGMHtml();
                this.staffPersonalLoanData = StaffPersonalLoanHtml();
                this.temporaryOverdraftData = TemporaryOverdraftHtml();

                //FUSS
                this.fussCustomerInformationData = FussCustomerInformationHtml();
                this.fussSchoolFeesInformationData = FussSchoolFeesInformationHtml();
                this.fussCustomerFacilityData = FussCustomerFacilityHtml();
                this.fussCustomerAccountActivityData = FussCustomerAccountActivityHtml();
                this.fussCustomerAccountActivitySummaryData = FussCustomerAccountActivitySummaryHtml();
                this.fussCashFlowAnalysisData = FussCashFlowAnalysisHtml();
                this.fussSummaryNetCashFlowData = FussSummaryNetCashFlowHtml();
                this.fussCurrentRequestData = FussCurrentRequestHtml();
                this.fussBackgroungInformationData = FussBackgroungInformationHtml();
                this.fussChecklistEligibilityData = FussChecklistEligibilityHtml();
                this.fussCustomerConditionSubsequentData = FussCustomerConditionSubsequentHtml();
                this.fussCustomerConditionDynamicsData = FussCustomerConditionDynamicsHtml();

                //IDF
                this.idfCustomerInformationData = IdfCustomerInformationHtml();
                this.idfCustomerFacilityData = IdfCustomerFacilityHtml();
                this.idfCustomerAccountActivityData = IdfCustomerAccountActivityHtml();
                this.idfCurrentRequestData = IdfCurrentRequestHtml();
                this.idfBackgroungInformationData = IdfBackgroungInformationHtml();
                this.idfChecklistEligibilityData = IdfChecklistEligibilityHtml();
                this.idfDocumentationChecklistData = IdfDocumentationChecklistHtml();

                //CASH COLLATERIZED
                this.cashCollaterizedCustomerInformationData = IdfCustomerInformationHtml();
                this.cashCollaterizedCustomerFacilityData = IdfCustomerFacilityHtml();
                this.cashCollaterizedCustomerAccountActivityData = IdfCustomerAccountActivityHtml();
                this.cashCollaterizedCurrentRequestData = IdfCurrentRequestHtml();
                this.cashCollaterizedBackgroungInformationData = IdfBackgroungInformationHtml();
                this.cashCollaterizedChecklistEligibilityData = IdfChecklistEligibilityHtml();
                this.cashCollaterizedDocumentationChecklistData = IdfDocumentationChecklistHtml();

                //TOD 
                this.todHeaderData = TodHeaderHtml();
                this.todCustomerInformationData = TodCustomerInformationHtml();
                this.todCustomerFacilityData = TodCustomerFacilityHtml();
                this.todCustomerAccountActivityData = TodCustomerAccountActivityHtml();
                this.todCurrentRequestData = TodCurrentRequestHtml();
                this.todBackgroungInformationData = TodBackgroungInformationHtml();
                this.currentLMSFlowData = CurrentLMSFlowHtml();

                //this.originalDocumentNonCreditProgramData = NoncreditProgramCustomerInformationHtml();
                //this.originalDocumentCreditProgramData = CreditProgramCustomerInformationHtml();

                // cam
                var cam = ClassifiedAssetManagementReview(lmsrApplication.APPLICATIONREFERENCENUMBER);

                if (cam != null)
                {
                    this.securityType = cam.securityType;
                    this.securityDescription = cam.securityDescription;
                    this.securityFirstSellValue = cam.securityFirstSellValue.ToString();
                    this.securityLocation = cam.securityLocation;
                    this.securityOpenMarketValue = cam.securityOpenMarketValue.ToString();
                    this.securityPerfectionStatus = cam.securityPerfectionStatus.ToString();
                    this.securityValuationDate = cam.securityValuationDate.ToString();
                    this.shareHolders = cam.shareHolders;
                    this.signitories = cam.signitories;
                    this.directors = cam.directors;
                    this.isSecurity = cam.isResidential == true ? "Yes" : "No";
                    this.isOwnerOccupied = cam.isOwnerOccupied == true ? "Yes" : "No";
                    this.amountDisbursed = cam.amountDisbursed.ToString();
                    this.amountPaidSoFar = cam.amountPaidSoFar.ToString();
                    this.amountProposed = cam.amountProposed.ToString();
                }
            }

            if (this.customerIds?.Count > 0)
            {
                this.accountNumbers = AccountNumbersMarkup(this.customerIds?.Select(x => x.customerId).ToList());
            }

            this.approvalLevel = GetApprovalLevel();
            this.proposedConditions = GetProposedConditionsMarkup();
            //this.conditionsPrecedenceList = GetConditionsMarkUp();
            this.dynamicsList = GetDynamicsMarkUp();
            this.monitoringTriggers = MonitoringTriggersMarkup();


            //this.customerTurnover = CustomerTurnoverMarkup(); // lazy loaded

            return true;
        }

        public List<DropDownSelect> GetProposedConditions()
        {
            var result = new List<DropDownSelect>();
            if (lmsCamOperationIds.Contains(operationId))
            {
                var details = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).ToList();
                if (details != null && details.Count() > 0)
                {
                    foreach (var d in details)
                    {
                        foreach (var t in d.TBL_LMSR_TRANSACTION_DYNAMICS)
                        {
                            result.Add(new DropDownSelect { id = d.LOANREVIEWAPPLICATIONID, name = "TRANSACTION DYNAMICS: " + t.DYNAMICS });
                        }
                        foreach (var t in d.TBL_LMSR_CONDITION_PRECEDENT)
                        {
                            if (!t.ISSUBSEQUENT)
                            {
                                result.Add(new DropDownSelect { id = d.LOANREVIEWAPPLICATIONID, name = "CONDITION SUBSEQUENT: " + t.CONDITION });
                            }
                        }
                        foreach (var t in d.TBL_LMSR_CONDITION_PRECEDENT)
                        {
                            if (t.ISSUBSEQUENT)
                            {
                                result.Add(new DropDownSelect { id = d.LOANREVIEWAPPLICATIONID, name = "CONDITION SUBSEQUENT: " + t.CONDITION });
                            }
                        }
                    }
                }
            }
            else
            {
                if (operationId == (int)OperationsEnum.CreditAppraisal)
                {
                    var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).ToList();
                    if (details != null && details.Count() > 0)
                    {
                        foreach (var d in details)
                        {
                            result.Add(new DropDownSelect { id = d.LOANAPPLICATIONDETAILID, name = "TRANSACTION DYNAMICS: " + d.TRANSACTIONDYNAMICS });
                            result.Add(new DropDownSelect { id = d.LOANAPPLICATIONDETAILID, name = "CONDITION PRECEDENT: " + d.CONDITIONPRECIDENT });
                            result.Add(new DropDownSelect { id = d.LOANAPPLICATIONDETAILID, name = "CONDITION SUBSEQUENT: " + d.CONDITIONSUBSEQUENT });
                        }
                    }
                }
            }
            
            return result;
        }

        /*public List<DropDownSelect> GetConditionsPrecedentToDrawdown()
        {
            var result = new List<DropDownSelect>();
            if (this.loanApplication != null)
            {
                var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId && x.DELETED == false).ToList();
                var conditions = new List<ConditionPrecedentViewModel>();

                foreach (var f in details)
                {
                    conditions.AddRange(conditionsRepo.GetAllConditionPrecedent().Where(x => x.loanApplicationDetailId == f.LOANAPPLICATIONDETAILID));
                }

                foreach (var b in conditions)
                {
                    if (b.condition != null)
                    {
                        var detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == b.loanApplicationDetailId).FirstOrDefault();
                        result.Add(new DropDownSelect { typeId = b.loanApplicationDetailId, id = b.conditionId, name = b.condition, title = detail.TBL_PRODUCT1.PRODUCTNAME });
                    }
                }
            }
            else if (this.lmsrApplication != null)
            {
                var details = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID && x.DELETED == false).ToList();
                var conditions = new List<ConditionPrecedentViewModel>();

                foreach (var f in details)
                {
                    conditions.AddRange(conditionsRepo.GetAllConditionPrecedent().Where(x => x.loanApplicationDetailId == f.LOANREVIEWAPPLICATIONID));
                }

                foreach (var b in conditions)
                {
                    if (b.condition != null)
                    {
                        var detail = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANREVIEWAPPLICATIONID == b.loanApplicationDetailId).FirstOrDefault();
                        result.Add(new DropDownSelect { typeId = b.loanApplicationDetailId, id = b.conditionId, name = b.condition, title = detail.TBL_PRODUCT.PRODUCTNAME });
                    }
                }
            }
           
            return result;
        }*/
        public List<DropDownSelect> GetConditionsPrecedentToDrawdown()
        {
            var result = new List<DropDownSelect>();

            if (this.loanApplication != null)
            {
                var loanDetails = context.TBL_LOAN_APPLICATION_DETAIL
                    .Where(x => x.LOANAPPLICATIONID == targetId && x.DELETED == false)
                    .Select(x => new { x.LOANAPPLICATIONDETAILID, x.TBL_PRODUCT1.PRODUCTNAME })
                    .ToList();

                var detailIds = loanDetails.Select(x => x.LOANAPPLICATIONDETAILID).ToList();

                var conditions = conditionsRepo.GetAllConditionPrecedent()
                    .Where(x => detailIds.Contains(x.loanApplicationDetailId))
                    .ToList();

                foreach (var condition in conditions)
                {
                    if (condition.condition != null)
                    {
                        var detail = loanDetails.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == condition.loanApplicationDetailId);
                        if (detail != null)
                        {
                            result.Add(new DropDownSelect { typeId = condition.loanApplicationDetailId, id = condition.conditionId, name = condition.condition, title = detail.PRODUCTNAME });
                        }
                    }
                }
            }
            else if (this.lmsrApplication != null)
            {
                var lmsrDetails = context.TBL_LMSR_APPLICATION_DETAIL
                    .Where(x => x.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID && x.DELETED == false)
                    .Select(x => new { x.LOANREVIEWAPPLICATIONID, x.TBL_PRODUCT.PRODUCTNAME })
                    .ToList();

                var lmsrDetailIds = lmsrDetails.Select(x => x.LOANREVIEWAPPLICATIONID).ToList();

                var lmsrConditions = conditionsRepo.GetAllConditionPrecedent()
                    .Where(x => lmsrDetailIds.Contains(x.loanApplicationDetailId))
                    .ToList();

                foreach (var condition in lmsrConditions)
                {
                    if (condition.condition != null)
                    {
                        var detail = lmsrDetails.FirstOrDefault(x => x.LOANREVIEWAPPLICATIONID == condition.loanApplicationDetailId);
                        if (detail != null)
                        {
                            result.Add(new DropDownSelect { typeId = condition.loanApplicationDetailId, id = condition.conditionId, name = condition.condition, title = detail.PRODUCTNAME });
                        }
                    }
                }
            }

            return result;
        }


        public List<DropDownSelect> GetConditionsPrecedentToDrawdownFacility(int LOANAPPLICATIONDETAILID)
        {
            var result = new List<DropDownSelect>();
            var conditions = conditionsRepo.GetAllConditionPrecedent().Where(x => x.loanApplicationDetailId == LOANAPPLICATIONDETAILID);

            foreach (var d in conditions)
                {
                    if (d.condition != null)
                    {
                        var detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == d.loanApplicationDetailId).FirstOrDefault();
                        result.Add(new DropDownSelect { typeId = d.loanApplicationDetailId, id = d.conditionId, name = d.condition, title = detail.TBL_PRODUCT1.PRODUCTNAME });
                    }
                }
            
            return result;
        }

        private string GetConditionsPrecedentToDrawdownFacilityMarkup(int LOANAPPLICATIONDETAILID)
        {
            var result = String.Empty;
            var conditions = GetConditionsPrecedentToDrawdownFacility(LOANAPPLICATIONDETAILID).GroupBy(c => c.typeId).ToList(); // new
            if (conditions.Count() > 0) { 
                result = result + $@"<br />
                <h3><b>CONDITIONS PRECEDENT TO DRAWDOWN</b></h3>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>CONDITIONS PRECEDENT TO DRAWDOWN</b></th>
                    </tr>
                 ";

                foreach (var g in conditions)
                {
                    var n = 0;
                    var c = g.FirstOrDefault();
                    result += c.title;
                    foreach (var e in g)
                    {
                        n++;
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{e.name}</td>
                        </tr>
                        ";
                    }

                }
              result = result + $"</table><br/>";
            }
            return result;

        }

        public List<DropDownSelect> GetdrawdownTransactionsDynamics(int targetIds)
        {
            var result = new List<DropDownSelect>();

                var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetIds && x.DELETED == false).ToList();
                var allTransactions = this.transactionsRepo.GetAllTransactionDynamics().OrderBy(a => a.position);
                var transactions = new List<TransactionDynamicsViewModel>();

                foreach (var b in details)
                {
                    var transactionSelect = allTransactions.Where(x => x.loanApplicationDetailId == b.LOANAPPLICATIONDETAILID).ToList();
                    transactions.AddRange(transactionSelect);
                }

                foreach (var t in transactions)
                {
                    if (t.dynamics != null)
                    {
                        var detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == t.loanApplicationDetailId).FirstOrDefault();
                        result.Add(new DropDownSelect { typeId = (int)t.loanApplicationDetailId, name = t.dynamics, title = detail.TBL_PRODUCT1.PRODUCTNAME });
                    }
                }
            
            return result;
        }

        public List<DropDownSelect> GetTransactionsDynamics()
        {
            var result = new List<DropDownSelect>();

            var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId && x.DELETED == false).ToList();
            var allTransactions = this.transactionsRepo.GetAllTransactionDynamics().OrderBy(a => a.position);
            var transactions = new List<TransactionDynamicsViewModel>();

            foreach (var b in details)
            {
                var transactionSelect = allTransactions.Where(x => x.loanApplicationDetailId == b.LOANAPPLICATIONDETAILID).ToList();
                transactions.AddRange(transactionSelect);
            }

            foreach (var t in transactions)
            {
                if (t.dynamics != null)
                {
                    var detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == t.loanApplicationDetailId).FirstOrDefault();
                    result.Add(new DropDownSelect { typeId = (int)t.loanApplicationDetailId, name = t.dynamics, title = detail.TBL_PRODUCT1.PRODUCTNAME });
                }
            }

            return result;
        }


        public string GetObligorClassification()
        {
            if (string.IsNullOrEmpty(this.obligorRiskRating))
            {
                return null;
            }
            this.obligorRiskRating = obligorRiskRating.Trim();
            var classification = context.TBL_CUSTOMER_RISK_RATING.FirstOrDefault(r => r.RISKRATING.Trim() == obligorRiskRating)?.CLASSIFICATION;
            return classification;
        }

        public string GetManagementProfileMarkup()
        {
            return loanApplication?.LOANINFORMATION;
        }

        public string GetOwnershipMarkup()
        {
            return loanApplication?.OWNERSHIPSTRUCTURE;
        }

        private List<TotalFacilitiesSummaryViewModel> GetTotalFacilitiesNGNLOS2()
        {
            var totalSummary = new List<TotalFacilitiesSummaryViewModel>();
            var totalDirectFacilities = GetTotalDirectFacilitiesSummaryLOS((int)CurrencyEnum.NGN);
            if (totalDirectFacilities != null) totalSummary.Add(totalDirectFacilities);
            var totalContingentFacilities = GetTotalContingentFacilitiesSummaryLOS((int)CurrencyEnum.NGN);
            if (totalContingentFacilities != null) totalSummary.Add(totalContingentFacilities);
            //var totalImportFinanceFacilities
            var totalIFFSummaryNGN = GetTotalImportFinanceFacilitiesSummaryLOS((int)CurrencyEnum.NGN);
            if (totalIFFSummaryNGN != null) totalSummary.Add(totalIFFSummaryNGN);

            return totalSummary;
        }

        private List<TotalFacilitiesSummaryViewModel> GetTotalFacilitiesNGNLOS()
        {
            var totalSummary = new List<TotalFacilitiesSummaryViewModel>();
            var currencyId = (int)CurrencyEnum.NGN;

            var facilities = new[]
            {
                GetTotalDirectFacilitiesSummaryLOS(currencyId),
                GetTotalContingentFacilitiesSummaryLOS(currencyId),
                GetTotalImportFinanceFacilitiesSummaryLOS(currencyId)
             };

            totalSummary.AddRange(facilities.Where(f => f != null));

            return totalSummary;
        }

        private List<TotalFacilitiesSummaryViewModel> GetTotalForeignFacilitiesLOS2()
        {
            var totalSummary = new List<TotalFacilitiesSummaryViewModel>();
            var totalDirectFacilities = GetTotalDirectFacilitiesSummaryLOS((int)CurrencyEnum.USD);
            if (totalDirectFacilities != null) totalSummary.Add(totalDirectFacilities);
            var totalContingentFacilities = GetTotalContingentFacilitiesSummaryLOS((int)CurrencyEnum.USD);
            if (totalContingentFacilities != null) totalSummary.Add(totalContingentFacilities);
            //var totalImportFinanceFacilities
            var totalIFFSummaryFCY = GetTotalImportFinanceFacilitiesSummaryLOS((int)CurrencyEnum.USD);
            if (totalIFFSummaryFCY != null) totalSummary.Add(totalIFFSummaryFCY);
            return totalSummary;
        }

        private List<TotalFacilitiesSummaryViewModel> GetTotalForeignFacilitiesLOS()
        {
            var totalSummary = new List<TotalFacilitiesSummaryViewModel>();
            var currencyId = (int)CurrencyEnum.USD;

            var facilities = new[]
            {
                GetTotalDirectFacilitiesSummaryLOS(currencyId),
                GetTotalContingentFacilitiesSummaryLOS(currencyId),
                GetTotalImportFinanceFacilitiesSummaryLOS(currencyId)
            };

            totalSummary.AddRange(facilities.Where(f => f != null));

            return totalSummary;
        }

        private string getIsDirectorRelated()
        {

            var related = (from a in context.TBL_CUSTOMER_RELATED_PARTY
                           join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                           join c in context.TBL_COMPANY_DIRECTOR on a.COMPANYDIRECTORID equals c.COMPANYDIRECTORID
                           where a.CUSTOMERID == customerId && a.DELETED == false
                           select new CustomerRelatedPartyViewModel
                           {
                               customerName = b.FIRSTNAME + " " + b.MIDDLENAME + " " + b.LASTNAME,
                               directorName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                               relationshipType = a.RELATIONSHIPTYPE,
                               relatedPartyId = a.RELATEDPARTYID,
                               customerId = b.CUSTOMERID,
                               companyDirectorId = c.COMPANYDIRECTORID
                           }).ToList();
            if (related.Count > 0)
            {
                return "Yes";
            }
            return "No";
        }

        private string getIsDirectorRelatedLMS()
        {
            var related = (from a in context.TBL_CUSTOMER_RELATED_PARTY
                           join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                           join c in context.TBL_COMPANY_DIRECTOR on a.COMPANYDIRECTORID equals c.COMPANYDIRECTORID
                           where a.CUSTOMERID == customerId && a.DELETED == false
                           select new CustomerRelatedPartyViewModel
                           {
                               customerName = b.FIRSTNAME + " " + b.MIDDLENAME + " " + b.LASTNAME,
                               directorName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                               relationshipType = a.RELATIONSHIPTYPE,
                               relatedPartyId = a.RELATEDPARTYID,
                               customerId = b.CUSTOMERID,
                               companyDirectorId = c.COMPANYDIRECTORID
                           }).ToList();
            if (related.Count > 0)
            {
                return "Yes";
            }
            return "No";
        }

        public decimal getTotalLLLImpact()
        {
            var approvalAmount = GetApprovalAmount();
            return approvalAmount;
            //var totalSummary = GetTotalFacilitiesNGNLOS();
            //var totalSummary2 = GetTotalForeignFacilitiesLOS();
            //totalSummary.AddRange(totalSummary2);
            //var exposureLLL = (GetTotalGroupLendingLimit().totalLLLImpact - GetTotalGroupLendingLimit(true).totalLLLImpact);
            //var test = (totalSummary.Sum(f => f.totalLLLImpact));
            //var test2 = (totalSummary.Sum(f => f.totalLLLImpact) + exposureLLL);

            ////LLL = totalSummary.Sum(f => f.totalLLLImpact) + exposureLLL;
            //return (totalSummary.Sum(f => f.totalLLLImpact) + exposureLLL);
        }

        private decimal getTotalLLLImpactFCY()
        {
            var totalSummary = GetTotalForeignFacilitiesLOS();
            return totalSummary.Sum(f => f.totalLLLImpact);
        }

        private IQueryable<OperationStaffViewModel> GetAllStaffNames()
        {
            return this.context.TBL_STAFF.Select(s => new OperationStaffViewModel
            {
                id = s.STAFFID,
                staffId = s.STAFFID,
                name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME,
                role = s.TBL_STAFF_ROLE.STAFFROLENAME
                
            });
        }

        public IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId, int operationId, bool getAll)
        {
            var allstaff = this.GetAllStaffNames();
            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.FROMAPPROVALLEVELID != null && x.TARGETID == applicationId && x.OPERATIONID == operationId).ToList();

            if (getAll)
            {
                trail = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == applicationId && x.OPERATIONID == operationId).ToList();
            }

            var data = trail.Select(x => new ApprovalTrailViewModel
            {
                approvalTrailId = x.APPROVALTRAILID,
                comment = x.COMMENT,
                vote = x.VOTE,
                targetId = x.TARGETID,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? allstaff.FirstOrDefault(s => s.staffId == x.REQUESTSTAFFID)?.role : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = x.TOAPPROVALLEVELID ?? 0,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                loopedStaffId = x.LOOPEDSTAFFID,
                toStaffId = x.TOSTAFFID,
                approvalState = x.APPROVALSTATEID == null ? "N/A" : context.TBL_APPROVAL_STATE.Where(a => a.APPROVALSTATEID == x.APPROVALSTATEID).Select(a => a.APPROVALSTATE).FirstOrDefault(),
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            })?.ToList();

            foreach (var t in data)
            {
                if (t.fromApprovalLevelId == t.toApprovalLevelId)
                {
                    if (t.loopedStaffId > 0)
                    {
                        t.toStaffName = allstaff.FirstOrDefault(s => s.staffId == t.loopedStaffId)?.name;
                        t.toApprovalLevelName = allstaff.FirstOrDefault(s => s.staffId == t.loopedStaffId)?.role;
                    }
                    else
                    {
                        t.fromApprovalLevelName = allstaff.FirstOrDefault(s => s.staffId == t.requestStaffId)?.role;
                        t.toStaffName = t.toStaffId != null ? allstaff.FirstOrDefault(s => s.staffId == t.toStaffId)?.name : t.toStaffName;
                    }
                }

            }

            return data.OrderByDescending(d=>d.systemArrivalDateTime);
        }

        public IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrailDrawdown(int applicationId, int operationId)
        {
            var bookingIds = context.TBL_LOAN_BOOKING_REQUEST.Where(b => b.LOANAPPLICATIONDETAILID == applicationId).Select(b => b.LOAN_BOOKING_REQUESTID).ToList().Distinct();
            var allstaff = this.GetAllStaffNames();
            var trail = context.TBL_APPROVAL_TRAIL.Where(x=>x.OPERATIONID == operationId && x.FROMAPPROVALLEVELID !=null && bookingIds.Contains(x.TARGETID)).ToList();

            var data = trail.Select(x => new ApprovalTrailViewModel
            {
                approvalTrailId = x.APPROVALTRAILID,
                comment = x.COMMENT,
                vote = x.VOTE,
                targetId = x.TARGETID,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = x.TOAPPROVALLEVELID ?? 0,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            })?.ToList();

            return data.OrderByDescending(d=>d.systemArrivalDateTime);
        }


        public bool IsLLLViolated()
        {
            return false;
            //return ((getTotalLLLImpact() > legalLendingLimit) ? true : false);
        }

        private IEnumerable<MonitoringTriggersViewModel> GetApplicationMonitoringTriggers(int applicationId)
        {
            return context.TBL_LOAN_APPLICATN_DETL_MTRIG
                .Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId && x.DELETED == false)
                .Select(x => new MonitoringTriggersViewModel
                {
                    applicationDetailId = x.LOANAPPLICATIONDETAILID,
                    monitoringTriggerId = x.MONITORING_TRIGGERID,
                    monitoringTrigger = x.MONITORING_TRIGGER,
                    productCustomerName = x.TBL_LOAN_APPLICATION_DETAIL.TBL_PRODUCT.PRODUCTNAME + " -- " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_LOAN_APPLICATION_DETAIL.TBL_CUSTOMER.LASTNAME
                })
                .ToList();
        }

        private IEnumerable<MonitoringTriggersViewModel> GetApplicationMonitoringTriggersLms(int applicationId)
        {
            return context.TBL_LMSR_APPLICATN_DETL_MTRIG
                .Where(x => x.TBL_LMSR_APPLICATION_DETAIL.LOANAPPLICATIONID == applicationId)
                .Select(x => new MonitoringTriggersViewModel
                {
                    applicationDetailId = x.LOANREVIEWAPPLICATIONID,
                    monitoringTriggerId = x.MONITORING_TRIGGERID,
                    monitoringTrigger = x.MONITORING_TRIGGER,
                    productCustomerName = x.TBL_LMSR_APPLICATION_DETAIL.TBL_OPERATIONS.OPERATIONNAME
                })
                .ToList();
        }
        //markups

        public string GetDrawdownMemoHtml(int staffId, int targetId)
        {
            if (targetId == 0)
            {
                return null;
            }
            var bookingId = context.TBL_LOAN_BOOKING_REQUEST.Find(targetId);
            var loanApplicationId = new TBL_LOAN_APPLICATION_DETAIL();
            if (bookingId == null)
            {
                bookingId = (from b in context.TBL_LOAN_BOOKING_REQUEST join c in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID where b.LOANAPPLICATIONDETAILID == targetId select b).FirstOrDefault();
            }
            if (bookingId != null)
            {
                loanApplicationId = context.TBL_LOAN_APPLICATION_DETAIL.Find(bookingId.LOANAPPLICATIONDETAILID);
            }
            var appraisalOperation = context.TBL_LOAN_APPLICATION.Find(loanApplicationId.LOANAPPLICATIONID).OPERATIONID;

            var isInitialize = InitializeDrawdownMemoProperties(loanApplicationId.LOANAPPLICATIONDETAILID, appraisalOperation, targetId);
            
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/logo2x.png' alt='' width='245' height='52'></td>
                        
                    </tr>
                    <tr>
                        <td><b>Reference Number:</b></td>
                        <td>{applicationReferenceNumber}</td>
                    </tr>
                   ";
            result = result + $"</table>";
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>NAME OF CUSTOMER:</b></th>
                        <th><b>{customerName}</b></th>
                        <th><b>CURRENT/APG A/C NO:</b></th>
                        <th>{currentAccountNo}</th>
                    </tr>
                      
                    <tr>
                        <td>BRANCH:</td>
                        <td>{branchName}</td>
                        <td>MIS CODE:</td>
                        <td>{misCode}</td>
                    </tr>
                    <tr>
                        <td>FACILITY TYPE:</td>
                        <td>{facilityType}</td>
                        <td>INTEREST RATE:</td>
                        <td>{interestRate}</td>
                    </tr>
                    <tr>
                        <td>DRAWDOWN AMOUNT:</td>
                        <td>{drawdownAmount}</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>TENOR:</td>
                        <td>{approvedTenorString}</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>MORATORIUM:</td>
                        <td>{moratorium}</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>PRINCIPAL REPAYMENT:</td>
                        <td>{principalRepayment}</td>
                        <td>OTHER FEES (SPECIFY):</td>
                        <td>{otherFee}</td>
                    </tr>
                    <tr>
                        <td>INTEREST REPAYMENT:</td>
                        <td>{interestRepayment}</td>
                        <td>EFFECTIVE DATE:</td>
                        <td>{effectiveDate}</td>
                    </tr>
                 ";
            result = result + $"</table>";
            result = result + GetFees(loanApplicationId.LOANAPPLICATIONDETAILID) + GetTrancheDisbursementHtml() + GetRequestTypeHtml() + GetConditionsPrecedentToDrawdownFacilityMarkup(loanApplicationId.LOANAPPLICATIONDETAILID) + GetTransactionsDynamicsDrawdownMarkup(loanApplicationId.LOANAPPLICATIONID)+ GetIsLineFacilityMarkup(bookingId.LOANAPPLICATIONDETAILID) + GetDrawdownApprovalsMarkupLOS2(targetId);
            return result;
        }


        public string GetFees(int targetId)
        {

            var chargeFeeIds = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(O => O.LOANAPPLICATIONDETAILID == targetId).ToList();
            var result = String.Empty;
            int n = 0;
            result = result + $@"
                <br />
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                   ";
            if (chargeFeeIds.Count > 0)
            {
                foreach (var e in chargeFeeIds)
                {
                    n++;
                    var name = context.TBL_CHARGE_FEE_DETAIL.Where(O => O.CHARGEFEEID == e.CHARGEFEEID).FirstOrDefault()?.DESCRIPTION;
                    if (name != null)
                    {
                        result = result + $@"
                    <tr><td>{n}</td><td>{name.ToUpper()}:</td><td>{e.RECOMMENDED_FEERATEVALUE}</td></tr>";
                    }
                }
            }
            result = result + $"</table> <br />";
            return result;
        }

        public string GetTrancheDisbursementHtml()
        {
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <td><b>TRANCHE DISBURSEMENT:</b></td>
                        <td>APPROVED AMOUNT</td>
                        <td>{approvedAmount}</td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>AMOUNT UTILIZED</td>
                        <td>{amountUtilised}</td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>NEW REQUEST</td>
                        <td>{newRequest}</td>
                    </tr>";
            result = result + $"</table>";
            return result;
        }

        public string GetRequestTypeHtml()
        {
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <td><b>REQUEST TYPE</b></td>
                        <td>{requestType}</td>
                    </tr>
                   ";
            result = result + $"</table>";
            return result;
        }

        public string GetPrecedentConditionsHtml(int applicationDetailId)
        {
            var inPlace = "";
            var perfected = "";
            var deferred = "";

            int n = 0;
            var conditionsPre = GetConditionPrecedentByApplicationDetailId(applicationDetailId);
            var result = String.Empty;
            result = result + $@"
                <br />
                <h3><b>CONDITIONS PRECEDENT TO DRAWDOWN</b></h3>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>CONDITIONS</b></th>
                        <th><b>STATUS</b></th>
                        <th><b></b></th>
                        <th><b></b></th>
                    </tr>
                    <tr>
                        <td></td>
                        <td></td>
                        <td>In Place</td>
                        <td>Perfected</td>
                        <td>Deferred</td>
                    </tr>";
            foreach (var e in conditionsPre)
            {
                inPlace = context.TBL_CHECKLIST_STATUS.Where(x => x.CHECKLISTSTATUSID == e.checkListStatusId).Select(x => x.CHECKLISTSTATUSNAME).FirstOrDefault()==null? "": "Yes";
                perfected = context.TBL_CHECKLIST_STATUS.Where(x => x.CHECKLISTSTATUSID == e.checkListStatusId).Select(x => x.CHECKLISTSTATUSNAME).FirstOrDefault() == null ? "" : "Yes";
                deferred = context.TBL_CHECKLIST_STATUS.Where(x => x.CHECKLISTSTATUSID == e.checkListStatusId).Select(x => x.CHECKLISTSTATUSNAME).FirstOrDefault() == null ? "" : "Yes";
                n++;
                result = result + $@"
                    < tr>
                        <td>{n}</td>
                        <td>{e.condition}</td>
                        <td>{inPlace}</td>
                        <td>{perfected}</td>
                        <td>{deferred}</td>
                    </tr>
                ";
            }
                   
            result = result + $"</table> <br />";
            return result;
        }

        public string GetApprovalLevelsHtml()
        {
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>APPROVALS:</b></th>
                        <th><b></b></th>
                    </tr>
                    <tr>
                        <td>RELATIONSHIP OFFICER:</td>
                        <td>{relationshipOfficer}</td>
                    </tr>
                    <tr>
                        <td>RELATIONSHIP MANAGER:</td>
                        <td>{relationshipManager}</td>
                    </tr>
                    <tr>
                        <td>RISK MANAGEMENT:</td>
                        <td>{riskManagement}</td>
                    </tr>
                    <tr>
                        <td>LEGAL:</td>
                        <td>{legal}</td>
                    </tr>
                    <tr>
                        <td>TREASURY:</td>
                        <td>{treasury}</td>
                    </tr>
                    <tr>
                        <td>COO:</td>
                        <td>{coo}</td>
                    </tr>
                    <tr>
                        <td>CRM INTERNATIONAL:</td>
                        <td>{crmInternational}</td>
                    </tr>";
            result = result + $"</table>";
            return result;
        }

        public string GetOtherConditionsHtml()
        {
            var result = String.Empty;
            result = result + $@"
                <br />
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>OTHER CONDITIONS PRECEDENT TO DRAWDOWN AS APPROVED IN THE FAM</b></th>
                        <th><b>STATUS</b></th>
                        <th><b></b></th>
                        <th><b></b></th>
                    </tr>
                    <tr>
                        <td></td>
                        <td></td>
                        <td>In Place</td>
                        <td>Perfected</td>
                        <td>Deferred</td>
                    </tr>
                    <tr>
                        <td>1</td>
                        <td></td>
                        <td>{othersInPlace1}</td>
                        <td>{othersPerfected1}</td>
                        <td>{othersDeferred1}</td>
                    </tr>
                 ";
            result = result + $"</table>";
            return result;
        }

        private string GetProposedConditionsMarkup()
        {
            var conditions = GetProposedConditions(); // new

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=1000pxpx cellpadding=0 cellspacing=0>
                    < tr>
                        <th><b>S/N</b></th>
                        <th><b>Facility Type</b></th>
                    </tr>
                 ";
            foreach (var e in conditions)
            {
                n++;
                result = result + $@"
                    <tr>
                        <td>{n}</td>
                        <td>{e.name}</td>
                    </tr>
                ";
            }
            result = result + $"</table>";
            return result;

        }

        private string GetAllExchangeRates()
        {
            var result = String.Empty;
            var exchangeRates = context.TBL_CURRENCY_EXCHANGERATE.Where(c => c.DELETED == false).Take(3).ToList();
            foreach (var x in exchangeRates)
            {
                var rate = financeTransaction.GetExchangeRate(DateTime.Now, x.TBL_CURRENCY1.CURRENCYID, loanApplication.COMPANYID);
                if (rate?.sellingRate > 0)
                {
                    x.EXCHANGERATE = rate.sellingRate;
                }
                result = result + $@"
                        {x.TBL_CURRENCY1.CURRENCYCODE}: {x.EXCHANGERATE}   
                ";
            }
            context.SaveChanges();
            return result;
        }

        private string GetAllExchangeRatesLMS()
        {
            var result = String.Empty;
            var exchangeRates = context.TBL_CURRENCY_EXCHANGERATE.Where(c => c.DELETED == false).Take(3).ToList();
            foreach (var x in exchangeRates)
            {
                var rate = financeTransaction.GetExchangeRate(DateTime.Now, x.TBL_CURRENCY1.CURRENCYID, lmsrApplication.COMPANYID);
                if (rate?.sellingRate > 0)
                {
                    x.EXCHANGERATE = rate.sellingRate;
                }
                result = result + $@"
                        {x.TBL_CURRENCY1.CURRENCYCODE}: {x.EXCHANGERATE}   
                ";
            }
            context.SaveChanges();
            return result;
        }

        private string GetConditionsPrecedentToDrawdownMarkup()
        {
            var conditions = GetConditionsPrecedentToDrawdown().GroupBy(c => c.typeId); // new

            var result = String.Empty;
            foreach (var g in conditions)
            {
                var n = 0;
                var c = g.FirstOrDefault();
                result += c.title;
                result = result + $@"
                <table style='font face: arial; size:12px' border=1 align=center width=1000px cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>CONDITIONS PRECEDENT TO DRAWDOWN</b></th>
                    </tr>
                 ";
                foreach (var e in g)
                {
                    n++;
                    result = result + $@"
                    <tr>
                        <td>{n}</td>
                        <td>{e.name}</td>
                    </tr>
                    ";
                }
                result = result + $"</table>";
            }
            return result;

        }

        private string GetConditionsPrecedentToDrawdownLOSMarkup()
        {
            var conditions = GetConditionsPrecedentToDrawdown().GroupBy(c => c.typeId); // new

            var result = String.Empty;
            foreach (var g in conditions)
            {
                var n = 0;
                var c = g.FirstOrDefault();
                //result += c.title;
                result = result + $@"
                <table style='font face: arial; size:12px' border=1 align=center width=1000px cellpadding=0 cellspacing=0>
                <tr>
                        <th colspan=2><b>{c.title}</b></th>
                        <th></th>
                    </tr>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>CONDITIONS</b></th>
                    </tr>
                 ";
                foreach (var e in g)
                {
                    n++;
                    result = result + $@"
                    <tr>
                        <td>{n}</td>
                        <td>{e.name}</td>
                    </tr>
                    ";
                }
                result = result + $"</table>";
            }
            return result;

        }

        private string GetConditionsMarkUp()
        {
            var conditions = GetConditionsPrecedentToDrawdown().GroupBy(c => c.typeId); // new
            var result = String.Empty;
            var n = 0;

            foreach (var g in conditions)
            {
                var c = g.FirstOrDefault();
                result += c.title;
                foreach (var e in g)
                {
                    n++;
                    result = result + $@"
                        <br/>{n}&nbsp;&nbsp;&nbsp;&nbsp;
                        {e.name}
                    ";
                }

            }
            result = result + $"</table>";
            return result;
        }

        public string GetDynamicsMarkUp()
        {
            var transactions = GetTransactionsDynamics().GroupBy(t => t.typeId); // new
            var result = String.Empty;
            var n = 0;

            foreach (var group in transactions)
            {
                n++;
                var o = 0;
                var c = group.FirstOrDefault();
                result += c.title;
                foreach (var t in group)
                {
                    n++;
                    result = result + $@"
                        <br/>{n}&nbsp;&nbsp;&nbsp;&nbsp;
                        {t.name}
                    ";
                }

            }
            result = result + $"</table>";
            return result;
        }

        private string GetTransactionsDynamicsMarkup()
        {
            var transactions = GetTransactionsDynamics().GroupBy(t => t.typeId); // new
            
            var result = String.Empty;
            foreach (var group in transactions)
            {
                var n = 0;
                var c = group.FirstOrDefault();
                //result += c.title;
                result = result + $@"
                <table style='font face: arial; size:12px' border=1 align=center width=1000px cellpadding=0 cellspacing=0>
                     <tr>
                       <th colspan=2><b>{c.title}</b></th>
                       <th></th>
                       </tr>                    
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>TRANSACTIONS DYNAMICS</b></th>
                    </tr>
                 ";
                
                foreach (var t in group)
                {
                    n++;
                    result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.name}</td>
                        </tr>
                ";
                }
            result = result + $"</table>";
            }
            return result;
        }

        private string GetTransactionsDynamicsDrawdownMarkup(int loanApplicationId)
        {
            var transactions = GetdrawdownTransactionsDynamics(loanApplicationId).GroupBy(t => t.typeId); // new

            var result = String.Empty;
            if (transactions.Count() > 0)
            {
                result = result + $@"<br/><h3><b>TRANSACTIONS DYNAMICS</b></h3>";
                foreach (var group in transactions)
                {
                    var n = 0;
                    var c = group.FirstOrDefault();
                    //result += c.title;
                    result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <td colspan=2><b>{c.title}</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>TRANSACTIONS DYNAMICS</b></th>
                    </tr>
                 ";

                    foreach (var t in group)
                    {
                        n++;
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{t.name}</td>
                        </tr>
                ";
                    }
                    result = result + $"</table>";
                }
            }
            return result;
        }

        private string GetIsLineFacilityMarkup(int loanApplicationId)
        {
            var isLineFacility = context.TBL_LOAN_APPLICATION_DETAIL.Where(a=>a.LOANAPPLICATIONDETAILID == loanApplicationId && a.ISLINEFACILITY == true)?.FirstOrDefault();
            var result = String.Empty;
            var n = 0;
            if (isLineFacility != null)
            {
                var isLineFacilityProduct = context.TBL_PRODUCT.Where(p => p.PRODUCTID == isLineFacility.APPROVEDPRODUCTID).Select(p => p.PRODUCTNAME)?.FirstOrDefault();
                var approvedAmount = string.Format("{0:#,##.00}", Convert.ToDecimal(isLineFacility.APPROVEDAMOUNT));
                var createdDate = isLineFacility.DATETIMECREATED.ToString("dd-MM-yyyy");
                var isLineFacilityDetail = context.TBL_LOAN_BOOKING_REQUEST.Where(b=>b.LOANAPPLICATIONDETAILID == isLineFacility.LOANAPPLICATIONDETAILID && b.APPROVEDLINESTATUSID == null)?.ToList();
                
                result = result + $@"<br/><h3><b>LINE FACILITY DETAIL</b></h3>";
                result = result + $@"<table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                 <tr>
                        <td><b>Facility</b></td>
                        <td>{isLineFacilityProduct.ToUpper()}</td>
                        <td><b>Approved Amount</b></td>
                        <td>{approvedAmount}</td>
                        <td><b>Approved Date</b></td>
                        <td>{createdDate}</td>
                    </tr>";

                if (isLineFacilityDetail.Count() > 0)
                {
                     result = result + $@"
                     <tr>
                        <td>S/N</td>
                        <td>Customer</td>
                        <td>Amount Requested</td>
                        <td>Date Requested</td>
                        <td>Account Number</td>
                        <td>Account Name</td>
                     </tr>";

                    foreach (var group in isLineFacilityDetail)
                    {
                        var customer = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == group.CUSTOMERID).Select(c => c.FIRSTNAME + "" + c.MIDDLENAME + "" + c.LASTNAME)?.FirstOrDefault();
                        var approvedAmount2 = string.Format("{0:#,##.00}", Convert.ToDecimal(group.AMOUNT_REQUESTED));
                        var createdDate2 = group.DATETIMECREATED.ToString("dd-MM-yyyy");
                        var accountNumber = context.TBL_CASA.Where(a => a.CASAACCOUNTID == group.CASAACCOUNTID).Select(a => a.PRODUCTACCOUNTNUMBER)?.FirstOrDefault();
                        var accountName = context.TBL_CASA.Where(a => a.CASAACCOUNTID == group.CASAACCOUNTID).Select(a => a.PRODUCTACCOUNTNAME)?.FirstOrDefault();

                        n++;
                        result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{customer}</td>
                            <td>{approvedAmount2}</td>
                            <td>{createdDate2}</td>
                            <td>{accountNumber}</td>
                            <td>{accountName}</td>
                        </tr>";
                    }
                    
                }
            }
            result = result + $"</table><br/>";
            return result;
        }

        private string GetBusinessSectorsMarkupLOS()
        {
            var result = String.Empty;
            if (loanApplication.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.Single)
            {
                result += this.loanApplication.TBL_CUSTOMER.TBL_SUB_SECTOR?.TBL_SECTOR.NAME;
            }
            else
            {
                result += this.loanApplication.TBL_CUSTOMER_GROUP.TBL_CUSTOMER_GROUP_MAPPING.FirstOrDefault().TBL_CUSTOMER.TBL_SUB_SECTOR?.TBL_SECTOR.NAME;
            }
            //foreach (var loanDetail in this.loanApplication.TBL_LOAN_APPLICATION_DETAIL)
            //{
            //    result = result + loanDetail.TBL_SUB_SECTOR.TBL_SECTOR.NAME + "\n";
            //}
            return result;
        }

        private string GetBusinessSectorsMarkupLMS()
        {
            var result = String.Empty;
            string sectorName;
            var cust = context.TBL_CUSTOMER.Find(lmsrApplication.CUSTOMERID);
            sectorName = context.TBL_SUB_SECTOR.Find(cust.SUBSECTORID)?.TBL_SECTOR.NAME;
            result += sectorName;
            //foreach (var loanDetail in this.lmsrApplication.TBL_LMSR_APPLICATION_DETAIL)
            //{
            //    result = result + loanDetail.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_SUB_SECTOR.TBL_SECTOR.NAME + "\n";
            //}
            return result;
        }

        private string GetMccStamp()
        {
            return context.TBL_DIGITAL_STAMP.Where(d => d.STAMPNAME.ToLower().Contains("mcc") && d.DELETED == false).Select(d => d.DIGITALSTAMP).FirstOrDefault();
        }

        private string GetBccStamp()
        {
            return context.TBL_DIGITAL_STAMP.Where(d => d.STAMPNAME.ToLower().Contains("bcc") && d.DELETED == false).Select(d => d.DIGITALSTAMP).FirstOrDefault();
        }

        private string GetGroupFacilitySummaryMarkupLOS()
        {
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 align=center width=1000px cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>Facility</b></th>
                        <th><b>LLL Impact(NGN)</b></th>
                        <th><b>Currency</b></th>
                        <th><b>Approved Amount</b></th>
                        <th><b>Proposed Amount</b></th>
                        <th><b>Change</b></th>
                        <th><b>Tenor (Months)</b></th>
                    </tr>
                        {GetDirectFacilitiesMarkupLOS((int)CurrencyEnum.NGN)}
                        {GetDirectFacilitiesMarkupLOS((int)CurrencyEnum.USD)}
                        {GetTotalDirectFacilitiesMarkupLOS()}
                        {GetContingentFacilitiesMarkupLOS((int)CurrencyEnum.NGN)}
                        {GetContingentFacilitiesMarkupLOS((int)CurrencyEnum.USD)}
                        {GetTotalContingentFacilitiesMarkupLOS()}
                        {GetIFFMarkupLOS((int)CurrencyEnum.NGN)}
                        {GetIFFMarkupLOS((int)CurrencyEnum.USD)}
                        {GetTotalIFFMarkupLOS()}
                        {GetTotalFacilitiesMarkupLOS()}
                    <tr>
                        <td><b>Legal Lending Limit:</b></td>
                        <td>{String.Format("{0:0,0.00}", legalLendingLimit)}</td>
                    </tr>
                    <tr>
                        <td><b>LLL Impact of Proposed Facilities:</b></td>
                        <td>{String.Format("{0:0,0.00}", getTotalLLLImpact())}</td>
                    </tr>
                    <tr>
                        <td><b>Any LLL violation? (Yes / No):</b></td>
                        <td>{(IsLLLViolated() ? "Yes" : "No")}</td>
                    </tr>
                    <tr>
                        <td><b>Director-related? (Yes / No):</b></td>
                        <td>{getIsDirectorRelated()}</td>
                    </tr>
                    <tr class=esg>
                        <td><b>Environmental And Social Risk Summary:</b></td>
                        <td>{GetEnvironmentalSocialRiskMarkup()}</td>
                    </tr>
                    <tr class=green>
                        <td><b>Overall Green Category:</b></td>
                        <td>{GetGreenRatingDetailMarkup()}</td>
                        <td>{GetGreenRatingSummaryMarkup()}</td>
                    </tr>
                 ";
            result = result + $"</table>";
            return result;
        }

        private string GetSubstringBetween(string startString, string endString, string body)
        {
            int index = body.IndexOf(startString);
            if (index == -1)
            {
                return String.Empty;
            }
            int startIndex = index + startString.Length;
            int endIndex = body.IndexOf(endString, startIndex);
            string content = body.Substring(startIndex, endIndex - startIndex);
            return content;
        }

        public string UpdateEsg(string body)
        {
            var oldString = GetSubstringBetween("<tr class=esg>", "</tr>", body);
            var newString = String.Empty;
            newString = newString + $@"
                        <td><b>Environmental And Social Risk Summary:</b></td>
                        <td>{GetEnvironmentalSocialRiskMarkup()}</td>
                    ";
            if (string.IsNullOrEmpty(oldString))
            {
                return body;
            }
            body = body.Replace(oldString, newString);
            return body;
        }

        public string UpdateGreenRating(string body)
        {
            var oldString = GetSubstringBetween("<tr class=green>", "</tr>", body);
            var newString = String.Empty;
            newString = newString + $@"
                        <td><b>Overall Green Category:</b></td>
                        <td>{GetGreenRatingDetailMarkup()}</td>
                        <td>{GetGreenRatingSummaryMarkup()}</td>
                    ";
            if (string.IsNullOrEmpty(oldString))
            {
                return body;
            }
            body = body.Replace(oldString, newString);
            return body;
        }

        private string GetGroupFacilitySummaryFCYMarkupLOS()
        {
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=1000px cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>Facility</b></th>
                        <th><b>LLL Impact(NGN)</b></th>
                        <th><b>Currency</b></th>
                        <th><b>Approved Amount</b></th>
                        <th><b>Proposed Amount</b></th>
                        <th><b>Change</b></th>
                        <th><b>Tenor</b></th>
                    </tr>
                    <tr><td>Direct Facilities:</td></tr>
                        {GetForeignDirectFacilitiesMarkupLOS()}
                        {GetTotalForeignDirectFacilitiesMarkupLOS()}
                    <tr><td>Contingent Facilities:</td></tr>
                        {GetForeignContingentFacilitiesMarkupLOS()}
                        {GetTotalForeignContingentFacilitiesMarkupLOS()}
                        {GetTotalForeignFacilitiesMarkupLOS()}
                    <tr>
                        <td>Legal Lending Limit:</td>
                        <td>{String.Format("{0:0,0.00}", legalLendingLimit)} Naira</td>
                    </tr>
                    <tr>
                        <td>LLL Impact of Proposed Facilities:</td>
                        <td>{String.Format("{0:0,0.00}", getTotalLLLImpactFCY())}</td>
                    </tr>
                    <tr>
                        <td>Any LLL violation? (Yes / No):</td>
                        <td>{((getTotalLLLImpactFCY() > legalLendingLimit) ? "Yes" : "No")}</td>
                    </tr>
                    <tr>
                        <td>Director-related? (Yes / No):</td>
                        <td>{getIsDirectorRelated()}</td>
                    </tr>
                 ";
            result = result + $"</table>";
            return result;
        }

        private string GetDirectFacilitiesMarkupLOS(int currencyId)
        {
            var result = String.Empty;
            var exposures = new List<CurrentCustomerExposure>();
            var loanExposures = new List<CurrentCustomerExposure>();
            var directExposures = new List<CurrentCustomerExposure>();
            var overdraftExposures = new List<CurrentCustomerExposure>();
            var details = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var initialLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var renewalLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithIncreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithDecreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            if (currencyId == (int)CurrencyEnum.NGN)
            {
                //var overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                //                                                    && l.ISDISBURSED == true).ToList();
                //var loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && l.ISDISBURSED == true
                //                                    && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                //var loans = context.TBL_GLOBAL_EXPOSURE.Where(l => l.CUSTOMERID.Contains(loanApplication.TBL_CUSTOMER.CUSTOMERCODE.Trim()) && l.CURRENCYTYPE.Contains("LCY")
                //                                                    && l.EXPOSURETYPECODE.Contains(ExposureTypeEnum.Direct.ToString()) && !l.ADJFACILITYTYPE.Contains("LC") && !l.ADJFACILITYTYPE.Contains("TRADELOAN")).ToList();

                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("lcy")).ToList();
                directExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                loanExposures = directExposures.Where(e => e.adjFacilityTypeId != (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
                overdraftExposures = directExposures.Where(e => e.adjFacilityTypeId == (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();

                details = this.loanApplication?.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN &&
                                                                                     d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability &&
                                                                                     d.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }
            else
            {
                //var overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                //                                                    && l.ISDISBURSED == true).ToList();
                //var loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && l.ISDISBURSED == true
                //                                    && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                //var loans = context.TBL_GLOBAL_EXPOSURE.Where(l => l.CUSTOMERID.Contains(loanApplication.TBL_CUSTOMER.CUSTOMERCODE.Trim()) && l.CURRENCYTYPE.Contains("LCY")
                //                                                    && l.EXPOSURETYPECODE.Contains(ExposureTypeEnum.Direct.ToString()) && !l.ADJFACILITYTYPE.Contains("LC") && !l.ADJFACILITYTYPE.Contains("TRADELOAN")).ToList();

                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("fcy")).ToList();
                directExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                loanExposures = directExposures.Where(e => e.adjFacilityTypeId != (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
                overdraftExposures = directExposures.Where(e => e.adjFacilityTypeId == (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();

                if(this.isThirdPartyFacility == false)details = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN &&
                                                                                     d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability &&
                                                                                     d.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }

            //if (loans.Count() > 0 || overdrafts.Count() > 0 || details.Count() > 0)
            if (loanExposures.Count > 0 || overdraftExposures.Count > 0 || details?.Count() > 0)
            {
                if (currencyId == (int)CurrencyEnum.NGN)
                {
                    result = result + $@"<tr><td>Direct Facilities (NGN):</td></tr>";
                }
                else
                {
                    result = result + $@"<tr><td>Direct Facilities (FCY):</td></tr>";
                }
            }
            //var loanGroups = loans.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            var loanExposuresGroups = loanExposures.GroupBy(f => f.productCode.Trim());
            foreach (var group in loanExposuresGroups)
            {
                var currFacility = group.GroupBy(f => f.currency.Trim());
                foreach (var curr in currFacility)
                {
                    initialLoans = details.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Initial).ToList();
                    renewalLoans = details.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Renewal).ToList();
                    RenewalWithIncreaseLoans = details.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithIncrease).ToList();
                    RenewalWithDecreaseLoans = details.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithDecrease).ToList();
                    var facility = group.FirstOrDefault().facilityType;
                    var currency = curr.First().currency;
                    if (renewalLoans.Any())
                    {
                        var proposedAmountTest = renewalLoans?.Sum(p => p.PROPOSEDAMOUNT) ?? 0;
                        var proposedAmount = proposedAmountTest;
                        var proposedAmountTestForLLLImpact = (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var tenorTest = (renewalLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var currentAmount = curr.Sum(p => p.outstandings);
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;
                        result = result + $@"
                    <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                    }
                    else if (RenewalWithIncreaseLoans.Any())
                    {
                        var proposedAmountTest = (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var proposedAmountTestForLLLImpact = (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var tenorTest = (RenewalWithIncreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var proposedAmount = proposedAmountTest;
                        var currentAmount = curr.Sum(p => p.outstandings);
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;
                        result = result + $@"
                    <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                    }
                    else if (RenewalWithDecreaseLoans.Any())
                    {
                        var proposedAmountTestForLLLImpact = (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var proposedAmountTest = (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var tenorTest = (RenewalWithDecreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var proposedAmount = proposedAmountTest;
                        var currentAmount = curr.Sum(p => p.outstandings);
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;
                        result = result + $@"
                    <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                    }
                    else
                    {
                        var currentAmount = curr.Sum(p => p.outstandings);
                        var currentAmountForLLLImpact = curr.Sum(p => p.outstandingsLcy);
                        var proposedAmountTestForLLLImpact = (initialLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var proposedAmountTest = (initialLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var tenorTest = (initialLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;
                        result = result + $@"
                    <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                    }
                }
            }

            //var overdraftGroups = overdrafts.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            //foreach (var group in overdraftGroups)
            //{
            //    var facility = group.Key;
            //    var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
            //    var currentAmount = group.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT);
            //    //var currentAmount = group.Sum(p => p.OVERDRAFTLIMIT);
            //    var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.TBL_PRODUCT.PRODUCTID ==
            //                          group.FirstOrDefault().TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
            //    var tenorTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
            //                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
            //    var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
            //    var LLLImpact = (100 / 100) * proposedAmount;
            //    var change = proposedAmount - currentAmount;
            //    var tenor = tenorTest;

            //    result = result + $@"
            //        <tr>
            //            <td>{facility}</td>
            //            <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
            //            <td>{currency}</td>
            //            <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
            //            <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
            //            <td>{String.Format("{0:0,0.00}", change)}</td>
            //            <td>{(tenor / 30)}</td>
            //        </tr>
            //        ";
            //}

            var overdraftExposuresGroups = overdraftExposures.GroupBy(f => f.productCode.Trim());

            foreach (var group in overdraftExposuresGroups)
            {
                var currFacility = group.GroupBy(f => f.currency.Trim());
                foreach (var curr in currFacility)
                {
                    initialLoans = details.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                        f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Initial).ToList();
                    renewalLoans = details.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Renewal).ToList();
                    RenewalWithIncreaseLoans = details.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithIncrease).ToList();
                    RenewalWithDecreaseLoans = details.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithDecrease).ToList();
                    var facility = group.FirstOrDefault().facilityType;
                    var currency = curr.First().currency;
                    if (renewalLoans.Any())
                    {
                        var proposedAmountTestForLLLImpact = (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var proposedAmountTest = (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var tenorTest = (renewalLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var proposedAmount = proposedAmountTest;
                        var currentAmount = curr.Sum(p => p.approvedAmount);
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;

                        result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                     ";
                    }else if (RenewalWithIncreaseLoans.Any())
                    {
                        var proposedAmountTestForLLLImpact = (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var proposedAmountTest = (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var tenorTest = (RenewalWithIncreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var proposedAmount = proposedAmountTest;
                        var currentAmount = curr.Sum(p => p.approvedAmount);
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;

                            result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                     ";
                    }else if (RenewalWithDecreaseLoans.Any())
                    {
                        var proposedAmountTestForLLLImpact = (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var proposedAmountTest = (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var tenorTest = (RenewalWithDecreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var proposedAmount = proposedAmountTest;
                        var currentAmount = curr.Sum(p => p.approvedAmount);
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;

                        result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                     ";
                    }
                    else
                    {
                        var currentAmountForLLLImpact = curr.Sum(p => p.approvedAmountLcy);
                        var proposedAmountTestForLLLImpact = (initialLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var proposedAmountTest = (initialLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var tenorTest = (initialLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var currentAmount = curr.Sum(p => p.approvedAmount);
                        var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;

                        result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                     ";
                    }
                }
            }
            if(details == null) { return result; }
            foreach (var d in details)
            {
                //var loanFacilityExists = loans.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);
                //var overdraftFacilityExists = overdrafts.Exists(o => o.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

                var loanFacilityExists = loanExposures.Exists(l => l.productCode.Trim() == d.TBL_PRODUCT.PRODUCTCODE.Trim());
                var overdraftFacilityExists = overdraftExposures.Exists(o => o.productCode.Trim() == d.TBL_PRODUCT.PRODUCTCODE.Trim());
                if (!loanFacilityExists && !overdraftFacilityExists)
                {
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;

                    result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                     ";
                }
            }
            return result;
        }

        private string GetTotalDirectFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            var totalDirectsSummary = new List<TotalFacilitiesSummaryViewModel>();
            var totalDirectsSummaryNGN = GetTotalDirectFacilitiesSummaryLOS((int)CurrencyEnum.NGN);
            var totalDirectsSummaryFCY = GetTotalDirectFacilitiesSummaryLOS((int)CurrencyEnum.USD);
            totalDirectsSummary.Add(totalDirectsSummaryNGN);
            totalDirectsSummary.Add(totalDirectsSummaryFCY);
            if (totalDirectsSummary.Sum(d => d.numberOfLoans) > 0 || totalDirectsSummary.Sum(d => d.numberOfOverdrafts) > 0 || totalDirectsSummary.Sum(d => d.numberOfNewFacilities) > 0 )
            {
                result = result + $@"
                    <tr>
                        <td><b>Total Direct (NGN)<b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectsSummary.Sum(t => t.totalLLLImpact))}</b></td>
                        <td><b>Naira</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectsSummary.Sum(t => t.totalCurrentAmount))}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectsSummary.Sum(t => t.totalProposedAmount))}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectsSummary.Sum(t => t.totalChange))}</b></td>
                        <td><b>{(totalDirectsSummary.Sum(t => t.totalTenors) / 30)}</b></td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetContingentFacilitiesMarkupLOS(int currencyId)
        {
            var result = String.Empty;
            //var contingents = context.TBL_LOAN_CONTINGENT.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
            //                                                    && l.ISDISBURSED == true).ToList();
            //var appLoans = context.TBL_LOAN_APPLICATION.Where(l => l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).SelectMany(l => l.TBL_LOAN_APPLICATION_DETAIL).ToList();
            //var contingents = appLoans.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
            //                                    && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == (int)LoanProductTypeEnum.ContingentLiability).ToList();
            var exposures = new List<CurrentCustomerExposure>();
            var contingentExposures = new List<CurrentCustomerExposure>();
            var appDetails = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var initialLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var renewalLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithIncreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithDecreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            if (currencyId == (int)CurrencyEnum.NGN)
            {
                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("lcy")).ToList();
                contingentExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                if(this.isThirdPartyFacility ==false)appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID ==
                                                                                   (int)LoanProductTypeEnum.ContingentLiability).ToList();
            }
            else
            {
                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("fcy")).ToList();
                contingentExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                if (this.isThirdPartyFacility == false) appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN && d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID ==
                                                                                   (int)LoanProductTypeEnum.ContingentLiability).ToList();
            }
            if (contingentExposures.Count > 0 || appDetails.Count() > 0)
            {
                if (currencyId == (int)CurrencyEnum.NGN)
                {
                    result = result + $@"<tr><td>Contingent Facilities (NGN):</td></tr>";
                }
                else
                {
                    result = result + $@"<tr><td>Contingent Facilities (FCY):</td></tr>";
                }
            }
            var contingentsGroup = contingentExposures.GroupBy(f => f.productCode.Trim());
            if (contingentsGroup.Count() > 0)
            {
                foreach (var group in contingentsGroup)
                {
                    var currFacility = group.GroupBy(f => f.currency.Trim());
                    foreach (var curr in currFacility)
                    {
                        initialLoans = appDetails.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Initial).ToList();
                        renewalLoans = appDetails.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                                 f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Renewal).ToList();
                        RenewalWithIncreaseLoans = appDetails.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                                 f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithIncrease).ToList();
                        RenewalWithDecreaseLoans = appDetails.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                                 f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithDecrease).ToList();
                        var facility = group.FirstOrDefault().facilityType;
                        var currency = curr.First().currency;
                        if (renewalLoans.Any())
                        {
                            var currentAmount = curr.Sum(p => p.approvedAmount);
                            var proposedAmountTestForLLLImpact = (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                            var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                            var LLLImpact = proposedAmountForLLLImpact / 3;
                            var proposedAmountTest = renewalLoans?.Sum(p => p.PROPOSEDAMOUNT) ?? 0;
                            var tenorTest = (renewalLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                            var proposedAmount = proposedAmountTest;
                            var change = proposedAmount - currentAmount;
                            var tenor = tenorTest;
                            result = result + $@"
                            <tr>
                                <td>{facility}</td>
                                <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                                <td>{currency}</td>
                                <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                                <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                                <td>{String.Format("{0:0,0.00}", change)}</td>
                                <td>{(tenor / 30)}</td>
                            </tr>
                            ";
                        }
                        else if (RenewalWithIncreaseLoans.Any())
                        {
                            var currentAmount = curr.Sum(p => p.approvedAmount);
                            var proposedAmountTestForLLLImpact = (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                            var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                            var LLLImpact = proposedAmountForLLLImpact / 3;
                            var proposedAmountTest = (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                            var tenorTest = (RenewalWithIncreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                            var proposedAmount = proposedAmountTest;
                            var change = proposedAmount - currentAmount;
                            var tenor = tenorTest;
                            result = result + $@"
                            <tr>
                                <td>{facility}</td>
                                <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                                <td>{currency}</td>
                                <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                                <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                                <td>{String.Format("{0:0,0.00}", change)}</td>
                                <td>{(tenor / 30)}</td>
                            </tr>
                            ";
                        }
                        else if (RenewalWithDecreaseLoans.Any())
                        {
                            var currentAmount = curr.Sum(p => p.approvedAmount);
                            var proposedAmountTestForLLLImpact = (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                            var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                            var LLLImpact = proposedAmountForLLLImpact / 3;
                            var proposedAmountTest = (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                            var tenorTest = (RenewalWithDecreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                            var proposedAmount = proposedAmountTest;
                            var change = proposedAmount - currentAmount;
                            var tenor = tenorTest;
                            result = result + $@"
                            <tr>
                                <td>{facility}</td>
                                <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                                <td>{currency}</td>
                                <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                                <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                                <td>{String.Format("{0:0,0.00}", change)}</td>
                                <td>{(tenor / 30)}</td>
                            </tr>
                            ";
                        }
                        else
                        {
                            var currentAmount = curr.Sum(p => p.approvedAmount);
                            var currentAmountForLLLImpact = curr.Sum(p => p.approvedAmountLcy);
                            var proposedAmountTestForLLLImpact = (initialLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                            var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
                            var LLLImpact = proposedAmountForLLLImpact / 3;
                            var proposedAmountTest = (initialLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                            var tenorTest = (initialLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                            var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                            var change = proposedAmount - currentAmount;
                            var tenor = tenorTest;
                            result = result + $@"
                            <tr>
                                <td>{facility}</td>
                                <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                                <td>{currency}</td>
                                <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                                <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                                <td>{String.Format("{0:0,0.00}", change)}</td>
                                <td>{(tenor / 30)}</td>
                            </tr>
                            ";
                        }
                    }
                }
            }

            foreach (var d in appDetails)
            {
                //var contingentExists = contingents.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);
                var contingentExists = contingentExposures.Exists(l => l.productCode.Trim() == d.TBL_PRODUCT.PRODUCTCODE.Trim());

                if (!contingentExists)
                {
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
                    var LLLImpact = proposedAmount / 3;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;

                    result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                }
            }
            return result;
        }

        private string GetTotalContingentFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            var totalContingentSummary = new List<TotalFacilitiesSummaryViewModel>();
            var totalContingentSummaryNGN = GetTotalContingentFacilitiesSummaryLOS((int)CurrencyEnum.NGN);
            var totalContingentSummaryFCY = GetTotalContingentFacilitiesSummaryLOS((int)CurrencyEnum.USD);
            totalContingentSummary.Add(totalContingentSummaryNGN);
            totalContingentSummary.Add(totalContingentSummaryFCY);
            if (totalContingentSummary.Sum(c => c.numberOfContingents) > 0 || totalContingentSummary.Sum(c => c.numberOfNewFacilities) > 0)
            {
                result = result + $@"
                    <tr>
                        <td><b>Total Direct (NGN)<b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalContingentSummary.Sum(t => t.totalLLLImpact))}</b></td>
                        <td><b>Naira</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalContingentSummary.Sum(t => t.totalCurrentAmount))}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalContingentSummary.Sum(t => t.totalProposedAmount))}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalContingentSummary.Sum(t => t.totalChange))}</b></td>
                        <td><b>{(totalContingentSummary.Sum(t => t.totalTenors) / 30)}</b></td>
                    </tr>
                ";
            }
            return result;
        }
        
        private string GetIFFMarkupLOS(int currencyId)
        {
            var result = String.Empty;
            var IFFs = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var exposures = new List<CurrentCustomerExposure>();
            var lcs = new List<CurrentCustomerExposure>();
            var tradeLoans = new List<CurrentCustomerExposure>();
            var iFFExposures = new List<CurrentCustomerExposure>();
            //var IFFs = new List<TBL_LOAN>();
            var appDetails = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var initialLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var renewalLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithIncreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithDecreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            if (currencyId == (int)CurrencyEnum.NGN)
            {
                //IFFs = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                //                                && l.ISDISBURSED == true && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                //var appLoans = context.TBL_LOAN_APPLICATION.Where(l => l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).SelectMany(l => l.TBL_LOAN_APPLICATION_DETAIL).ToList();
                //IFFs = appLoans.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                //                                    && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("lcy")).ToList();
                lcs = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && e.adjFacilityTypeString.Contains("LC")).ToList();
                tradeLoans = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                iFFExposures.AddRange(lcs);
                iFFExposures.AddRange(tradeLoans);
                if (this.isThirdPartyFacility == false) appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                                                && d.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }
            else
            {
                //IFFs = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                //                                && l.ISDISBURSED == true && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                //var appLoans = context.TBL_LOAN_APPLICATION.Where(l => l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).SelectMany(l => l.TBL_LOAN_APPLICATION_DETAIL).ToList();
                //IFFs = appLoans.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                //                                    && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("fcy")).ToList();
                lcs = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && e.adjFacilityTypeString.Contains("LC")).ToList();
                tradeLoans = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                iFFExposures.AddRange(lcs);
                iFFExposures.AddRange(tradeLoans);
                if (this.isThirdPartyFacility == false) appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                && d.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }

            if (iFFExposures.Count > 0 || appDetails.Count() > 0)
            {
                if (currencyId == (int)CurrencyEnum.NGN)
                {
                    result = result + $@"<tr><td>Import Finance Facilities (NGN):</td></tr>";
                }
                else
                {
                    result = result + $@"<tr><td>Import Finance Facilities (FCY):</td></tr>";
                }
            }
            var loanGroups = iFFExposures.GroupBy(f => f.productCode.Trim());
            foreach (var group in loanGroups)
            {
                var currFacility = group.GroupBy(f => f.currency.Trim());
                foreach (var curr in currFacility)
                {
                    initialLoans = appDetails.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Initial).ToList();
                    renewalLoans = appDetails.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Renewal).ToList();
                    RenewalWithIncreaseLoans = appDetails.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithIncrease).ToList();
                    RenewalWithDecreaseLoans = appDetails.Where(f => curr.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithDecrease).ToList();
                    var facility = group.FirstOrDefault().facilityType;
                    var currency = curr.Key;
                    if (renewalLoans.Any())
                    {
                        var proposedAmountTest = renewalLoans?.Sum(p => p.PROPOSEDAMOUNT) ?? 0;
                        var proposedAmount = proposedAmountTest;
                        var proposedAmountTestForLLLImpact = (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var tenorTest = (renewalLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var currentAmount = curr.Sum(p => p.approvedAmount);
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;
                        result = result + $@"
                    <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                    }
                    else if (RenewalWithIncreaseLoans.Any())
                    {
                        var proposedAmountTest = (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var proposedAmountTestForLLLImpact = (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var tenorTest = (RenewalWithIncreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var proposedAmount = proposedAmountTest;
                        var currentAmount = curr.Sum(p => p.approvedAmount);
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;
                        result = result + $@"
                    <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                    }
                    else if (RenewalWithDecreaseLoans.Any())
                    {
                        var proposedAmountTestForLLLImpact = (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = proposedAmountTestForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var proposedAmountTest = (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var tenorTest = (RenewalWithDecreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var proposedAmount = proposedAmountTest;
                        var currentAmount = curr.Sum(p => p.approvedAmount);
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;
                        result = result + $@"
                    <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                    }
                    else
                    {
                        var currentAmount = curr.Sum(p => p.approvedAmount);
                        var currentAmountForLLLImpact = curr.Sum(p => p.approvedAmountLcy);
                        var proposedAmountTestForLLLImpact = (initialLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var proposedAmountTest = (initialLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var tenorTest = (initialLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                        var change = proposedAmount - currentAmount;
                        var tenor = tenorTest;
                        result = result + $@"
                    <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                    }
                }

                    //var currentAmount = curr.Sum(p => p.OUTSTANDINGPRINCIPAL) + curr.Sum(p => p.OUTSTANDINGINTEREST);
                    //var currentAmountForLLLImpact = curr.Sum(p => p.OUTSTANDINGPRINCIPAL * (decimal)p.EXCHANGERATE) + curr.Sum(p => p.OUTSTANDINGINTEREST * (decimal)p.EXCHANGERATE);
                    //var currentAmount = curr.Sum(p => p.approvedAmount);
                    //var currentAmountForLLLImpact = curr.Sum(p => p.approvedAmountLcy);
                    //var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().productCode.Trim() ==
                    //                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim())?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                    //var proposedAmountTestForLLLImpact = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().productCode.Trim() ==
                    //                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim())?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    //var tenorTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().productCode.Trim() ==
                    //                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim())?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    //var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                    //var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
                    //var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                    //var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                    //var tenor = tenorTest;
                    ////var tenor = curr.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

                    //result = result + $@"
                    // <tr>
                    //    <td>{facility}</td>
                    //    <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                    //    <td>{currency}</td>
                    //    <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                    //    <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                    //    <td>{String.Format("{0:0,0.00}", change)}</td>
                    //    <td>{(tenor / 30)}</td>
                    //</tr>
                    //";
            }

            // checks each loan detail.
            foreach (var d in appDetails)
            {
                //var IFFExists = IFFs.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);
                var IFFExists = iFFExposures.Exists(l => l.productCode.Trim() == d.TBL_PRODUCT.PRODUCTCODE.Trim());

                if (!IFFExists)
                {
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
                    var proposedAmountForLLL = d.PROPOSEDAMOUNT * (decimal)d.EXCHANGERATE;
                    var LLLImpact = (100 / 100) * proposedAmountForLLL;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;

                    result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                     ";
                }
            }
            return result;
        }

        private string GetTotalIFFMarkupLOS()
        {
            var result = String.Empty;
            var totalIFFSummary = new List<TotalFacilitiesSummaryViewModel>();
            var totalIFFSummaryNGN = GetTotalImportFinanceFacilitiesSummaryLOS((int)CurrencyEnum.NGN);
            var totalIFFSummaryFCY = GetTotalImportFinanceFacilitiesSummaryLOS((int)CurrencyEnum.USD);
            totalIFFSummary.Add(totalIFFSummaryNGN);
            totalIFFSummary.Add(totalIFFSummaryFCY);
            if (totalIFFSummary.Sum(t => t.numberOfImportFinanceFacilities) > 0 || totalIFFSummary.Sum(t => t.numberOfNewFacilities) > 0)
            {
                result = result + $@"
                     <tr>
                        <td><b>Total Import Finance Facilities</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalIFFSummary.Sum(t => t.totalLLLImpact))}</b></td>
                        <td><b>Naira</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalIFFSummary.Sum(t => t.totalCurrentAmount))}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalIFFSummary.Sum(t => t.totalProposedAmount))}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalIFFSummary.Sum(t => t.totalChange))}</b></td>
                        <td><b>{(totalIFFSummary.Sum(t => t.totalTenors) / 30)}</b></td>
                    </tr>
                ";
            }
            return result;
        }

        //public decimal GetApprovalAmount(bool isLMS = false)
        //{
        //    decimal obligorGFSProposedAmount = 0;
        //    var custCode = "";

        //    if (isLMS)
        //    {
        //        var currencyId = this.lmsrApplication.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault()?.CURRENCYID;
        //        var exchngeRat = context.TBL_CURRENCY_EXCHANGERATE.Where(x => x.CURRENCYID == currencyId).FirstOrDefault()?.EXCHANGERATE;
        //        obligorGFSProposedAmount = this.lmsrApplication.TBL_LMSR_APPLICATION_DETAIL.Sum(x => x.CUSTOMERPROPOSEDAMOUNT ?? x.APPROVEDAMOUNT) > 0 ? this.lmsrApplication.TBL_LMSR_APPLICATION_DETAIL.Sum(x => x.CUSTOMERPROPOSEDAMOUNT ?? x.APPROVEDAMOUNT) : (this.lmsrApplication.APPROVEDAMOUNT ?? 0);
        //        if (exchngeRat != null) obligorGFSProposedAmount = obligorGFSProposedAmount * (decimal)exchngeRat;
        //    }
        //    else
        //    {
        //        var totalSummary = GetTotalFacilitiesNGNLOS();
        //        var totalSumaryFCY = GetTotalForeignFacilitiesLOS();
        //        totalSummary.AddRange(totalSumaryFCY);
        //        obligorGFSProposedAmount = totalSummary.Sum(f => f.totalProposedAmount);
        //    }

        //    if (isLMS)
        //    {
        //        custCode = context.TBL_CUSTOMER.Find(this.lmsrApplication?.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID)?.CUSTOMERCODE;
        //    }
        //    else
        //    {
        //        custCode = context.TBL_CUSTOMER.Find(loanApplication?.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID)?.CUSTOMERCODE;
        //    }
        //    //var exposures = this.globalExposure.Where(e => e.customerCode != custCode).ToList();
        //    //var currentAmount = new decimal();
        //    //var approvedAmount = new decimal();
        //    //var amountForLLL = new decimal();
        //    //var LLLImpact = new decimal();
        //    //var directExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
        //    //var loanExposures = directExposures.Where(e => e.adjFacilityTypeId != (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
        //    //var overdraftExposures = directExposures.Where(e => e.adjFacilityTypeId == (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
        //    //var contingents = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
        //    //var lcs = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && e.adjFacilityTypeString.Contains("LC")).ToList();
        //    //var tradeLoans = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();

        //    foreach (var product in loanExposures)
        //    {
        //        currentAmount = product.outstandingsLcy;
        //        amountForLLL = currentAmount;
        //        LLLImpact += amountForLLL;
        //    }

        //    foreach (var product in overdraftExposures)
        //    {
        //        currentAmount = product.outstandingsLcy;
        //        approvedAmount = product.approvedAmountLcy;
        //        amountForLLL = approvedAmount;
        //        //LLLImpact += amountForLLL;
        //        amountForLLL = (currentAmount >= approvedAmount) ? currentAmount : approvedAmount;
        //    }

        //    foreach (var product in contingents)
        //    {
        //        currentAmount = product.outstandingsLcy;
        //        approvedAmount = product.approvedAmountLcy;
        //        amountForLLL = (currentAmount >= approvedAmount) ? currentAmount : approvedAmount;
        //        LLLImpact += amountForLLL;
        //        //approvedAmount = product.approvedAmountLcy;
        //        //amountForLLL = approvedAmount;
        //        //LLLImpact += amountForLLL;
        //    }

        //    foreach (var product in lcs)
        //    {
        //        currentAmount = product.outstandingsLcy;
        //        approvedAmount = product.approvedAmountLcy;
        //        amountForLLL = (currentAmount >= approvedAmount) ? currentAmount : approvedAmount;
        //        LLLImpact += amountForLLL;
        //        //approvedAmount = product.approvedAmountLcy;
        //        //amountForLLL = approvedAmount;
        //        //LLLImpact += amountForLLL;
        //        //LLLImpact += (amountForLLL / 3);
        //    }

        //    foreach (var product in tradeLoans)
        //    {
        //        currentAmount = product.outstandingsLcy;
        //        approvedAmount = product.approvedAmountLcy;
        //        amountForLLL = (currentAmount >= approvedAmount) ? currentAmount : approvedAmount;
        //        LLLImpact += amountForLLL;
        //        //approvedAmount = product.approvedAmountLcy;
        //        //amountForLLL = approvedAmount;
        //        //LLLImpact += amountForLLL;
        //    }

        //    var approvalAmount = obligorGFSProposedAmount + LLLImpact;
        //    return approvalAmount;
        //}

        public decimal GetApprovalAmount(bool isLMS = false)
        {
            decimal obligorGFSProposedAmount = 0m;
            string custCode;

            if (isLMS)
            {
                var lmsrDetails = this.lmsrApplication.TBL_LMSR_APPLICATION_DETAIL;
                var currencyId = lmsrDetails.FirstOrDefault()?.CURRENCYID;
                var exchangeRate = context.TBL_CURRENCY_EXCHANGERATE.FirstOrDefault(x => x.CURRENCYID == currencyId)?.EXCHANGERATE;

                obligorGFSProposedAmount = lmsrDetails.Sum(x => x.CUSTOMERPROPOSEDAMOUNT ?? x.APPROVEDAMOUNT);
                if (obligorGFSProposedAmount <= 0)
                {
                    obligorGFSProposedAmount = this.lmsrApplication.APPROVEDAMOUNT ?? 0;
                }
               
                obligorGFSProposedAmount *= (decimal)exchangeRate;
            }
            else
            {
                var totalSummary = GetTotalFacilitiesNGNLOS();
                totalSummary.AddRange(GetTotalForeignFacilitiesLOS());
                obligorGFSProposedAmount = totalSummary.Sum(f => f.totalProposedAmount);
            }

            custCode = isLMS
                ? context.TBL_CUSTOMER.Find(this.lmsrApplication?.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault()?.CUSTOMERID)?.CUSTOMERCODE
                : context.TBL_CUSTOMER.Find(loanApplication?.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault()?.CUSTOMERID)?.CUSTOMERCODE;

            var exposures = this.globalExposure.Where(e => e.customerCode != custCode).ToList();
            decimal LLLImpact = 0m;

            foreach (var exposure in exposures)
            {
                var currentAmount = exposure.outstandingsLcy;
                var approvedAmount = exposure.approvedAmountLcy;
                decimal amountForLLL;

                switch (exposure.exposureTypeId)
                {
                    case (int)ExposureTypeEnum.Direct:
                        if (exposure.adjFacilityTypeString.Contains("TRADE LOAN"))
                        {
                            amountForLLL = Math.Max(currentAmount, approvedAmount);
                            LLLImpact += amountForLLL;
                        }
                        else if (!exposure.adjFacilityTypeString.Contains("LC"))
                        {
                            amountForLLL = exposure.adjFacilityTypeId == (int)AdjustedFacilityTypeEnum.OVERDRAFT ? Math.Max(currentAmount, approvedAmount) : currentAmount;
                            LLLImpact += amountForLLL;
                        }
                        break;

                    case (int)ExposureTypeEnum.Contingent:
                        if (exposure.adjFacilityTypeString.Contains("LC"))
                        {
                            amountForLLL = Math.Max(currentAmount, approvedAmount);
                            LLLImpact += amountForLLL;
                        }
                        else if (!exposure.adjFacilityTypeString.Contains("TRADE LOAN"))
                        {
                            amountForLLL = Math.Max(currentAmount, approvedAmount);
                            LLLImpact += amountForLLL;
                        }
                        break;
                }
            }

            return obligorGFSProposedAmount + LLLImpact;
        }

        private string GetTotalFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            var totalSummary = GetTotalFacilitiesNGNLOS();
            var totalSumaryFCY = GetTotalForeignFacilitiesLOS();
            totalSummary.AddRange(totalSumaryFCY);
            if (totalSummary.Count() > 0)
            {
                result = result + $@"
                     <tr>
                        <td><b>Total Facilities</td>
                        <td><b>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalLLLImpact))}</b></td>
                        <td><b>Naira</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalCurrentAmount))}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalProposedAmount))}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalChange))}</b></td>
                        <td><b>{(totalSummary.Sum(f => f.totalTenors) / 30)}</b></td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetForeignDirectFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            //var loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN && l.ISDISBURSED == true
            //                                    && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            //var overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
            //                                                    && l.ISDISBURSED == true).ToList();
            //var appLoans = context.TBL_LOAN_APPLICATION.Where(l => l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).SelectMany(l => l.TBL_LOAN_APPLICATION_DETAIL).ToList();
            //var loans = appLoans.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
            //                                    && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability
            //                                    && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            var exposures = new List<CurrentCustomerExposure>();
            var loanExposures = new List<CurrentCustomerExposure>();
            var directExposures = new List<CurrentCustomerExposure>();
            var overdraftExposures = new List<CurrentCustomerExposure>();
            exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("fcy")).ToList();
            directExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
            loanExposures = directExposures.Where(e => e.adjFacilityTypeId != (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
            overdraftExposures = directExposures.Where(e => e.adjFacilityTypeId == (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
            var appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN &&
                                                                                 d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability &&
                                                                                 d.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            //if (loanExposures.Count > 0 || appDetails.Count() > 0)
            if (loanExposures.Count > 0 || overdraftExposures.Count > 0 || appDetails.Count() > 0)
            {
                result = result + $@"<tr><td>Direct Facilities (FCY):</td></tr>";
            }
            var loanGroups = loanExposures.GroupBy(f => f.productCode.Trim());
            if (loanGroups.Count() > 0)
            {
                foreach (var group in loanGroups)
                {
                    var currFacility = group.GroupBy(f => f.currencyCode);
                    foreach (var curr in currFacility)
                    {
                        var facility = group.FirstOrDefault().facilityType;
                        var currency = curr.Key;
                        var currentAmount = curr.Sum(p => p.outstandings);
                        //var currentAmount = curr.Sum(p => p.OUTSTANDINGPRINCIPAL) + curr.Sum(p => p.OUTSTANDINGINTEREST);
                        var currentAmountForLLLImpact = curr.Sum(p => p.outstandingsLcy);
                        //var currentAmountForLLLImpact = curr.Sum(p => p.OUTSTANDINGPRINCIPAL * (decimal)p.EXCHANGERATE) + curr.Sum(p => p.OUTSTANDINGINTEREST * (decimal)p.EXCHANGERATE);
                        var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().productCode.Trim() ==
                                                  f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim())?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var proposedAmountTestForLLLImpact = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().productCode.Trim() ==
                                                  f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim())?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var tenorTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim())?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                        var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                        var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                        var tenor = tenorTest;
                        //var tenor = curr.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

                        result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                    }
                }
            }

            var overdraftExposuresGroups = overdraftExposures.GroupBy(f => f.productCode.Trim());
            foreach (var group in overdraftExposuresGroups)
            {
                var currFacility = group.GroupBy(f => f.currencyCode);
                foreach (var curr in currFacility)
                {
                    var facility = group.FirstOrDefault().facilityType;
                    var currency = curr.Key;
                    var currentAmount = curr.Sum(p => p.approvedAmount);
                    //var currentAmount = curr.Sum(p => p.OUTSTANDINGPRINCIPAL) + curr.Sum(p => p.OUTSTANDINGINTEREST);
                    var currentAmountForLLLImpact = curr.Sum(p => p.approvedAmountLcy);
                    //var currentAmountForLLLImpact = curr.Sum(p => p.OUTSTANDINGPRINCIPAL * (decimal)p.EXCHANGERATE) + curr.Sum(p => p.OUTSTANDINGINTEREST * (decimal)p.EXCHANGERATE);
                    var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().productCode.Trim() ==
                                              f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim())?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                    var proposedAmountTestForLLLImpact = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().productCode.Trim() ==
                                              f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim())?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYCODE.Trim() == curr.First().currencyCode.Trim())?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                    var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
                    var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                    var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                    var tenor = tenorTest;
                    //var tenor = curr.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

                    result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                }
            }

            //var overdraftGroups = overdrafts.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            //if (loanGroups.Count() > 0)
            //{
            //    foreach (var group in overdraftGroups)
            //    {
            //        var currFacility = group.GroupBy(f => f.CURRENCYID);
            //        foreach (var curr in currFacility)
            //        {
            //            var facility = group.Key;
            //            var currency = curr.First().TBL_CURRENCY.CURRENCYNAME;
            //            var currentAmount = curr.Sum(p => p.OVERDRAFTLIMIT);
            //            var currentAmountForLLLImpact = curr.Sum(p => p.OVERDRAFTLIMIT * (decimal)p.EXCHANGERATE);
            //            var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
            //                                      f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
            //            var proposedAmountTestForLLLImpact = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
            //                                      f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
            //            var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
            //            var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
            //            var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
            //            var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
            //            var tenor = curr.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

            //            result = result + $@"
            //         <tr>
            //            <td>{facility}</td>
            //            <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
            //            <td>{currency}</td>
            //            <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
            //            <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
            //            <td>{String.Format("{0:0,0.00}", change)}</td>
            //            <td>{(tenor / 30)}</td>
            //        </tr>
            //        ";
            //        }
            //    }
            //}

            foreach (var d in appDetails)
            {
                //var loanFacilityExists = loans.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);
                //var overdraftFacilityExists = overdrafts.Exists(o => o.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

                var loanFacilityExists = loanExposures.Exists(l => l.productCode.Trim() == d.TBL_PRODUCT.PRODUCTCODE.Trim());
                var overdraftFacilityExists = overdraftExposures.Exists(o => o.productCode.Trim() == d.TBL_PRODUCT.PRODUCTCODE.Trim());
                if (!loanFacilityExists && !overdraftFacilityExists)
                {
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
                    var proposedAmountTestForLLLImpact = d.PROPOSEDAMOUNT * (decimal)d.EXCHANGERATE;
                    var LLLImpact = (100 / 100) * proposedAmountTestForLLLImpact;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;

                    result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                }
            }
            return result;
        }

        private string GetTotalForeignDirectFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            var totalDirectSummary = GetTotalDirectFacilitiesSummaryLOS((int)CurrencyEnum.USD);
            if (totalDirectSummary.numberOfLoans > 0 || totalDirectSummary.numberOfOverdrafts > 0 || totalDirectSummary.numberOfNewFacilities > 0)
            {
                result = result + $@"
                     <tr>
                        <td><b>Total Directs (FCY)<b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectSummary.totalLLLImpact)}</b></td>
                        <td><b>{totalDirectSummary.currency}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectSummary.totalCurrentAmount)}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectSummary.totalProposedAmount)}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectSummary.totalChange)}</b></td>
                        <td><b>{(totalDirectSummary.totalTenors / 30)}</b></td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetForeignContingentFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            //var contingents = context.TBL_LOAN_CONTINGENT.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
            //                                                    && l.ISDISBURSED == true).ToList();
            //var appLoans = context.TBL_LOAN_APPLICATION.Where(l => l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).SelectMany(l => l.TBL_LOAN_APPLICATION_DETAIL).ToList();
            //var contingents = appLoans.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
            //                                    && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == (int)LoanProductTypeEnum.ContingentLiability).ToList();
            var exposures = new List<CurrentCustomerExposure>();
            var contingentExposures = new List<CurrentCustomerExposure>();
            exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("fcy")).ToList();
            contingentExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
            var appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN && d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID ==
                                                                                   (int)LoanProductTypeEnum.ContingentLiability);
            if (contingentExposures.Count() > 0 || appDetails.Count() > 0)
            {
                result = result + $@"<tr><td>Contingent Facilities (FCY):</td></tr>";
            }

            var contingentsGroup = contingentExposures.GroupBy(f => f.productCode.Trim());
            if (contingentsGroup.Count() > 0)
            {
                foreach (var group in contingentsGroup)
                {
                    var facility = group.FirstOrDefault().facilityType;
                    var currency = group.First().currency;
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.TBL_PRODUCT.PRODUCTCODE.Trim() ==
                                             group.FirstOrDefault().productCode.Trim() && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                    var tenorTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.TBL_PRODUCT.PRODUCTCODE.Trim() ==
                                             group.FirstOrDefault().productCode.Trim() && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                    var LLLImpact = proposedAmount / 3;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;

                    result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                }
            }
            //var contingentsGroup = contingents.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            //foreach (var group in contingentsGroup)
            //{
            //    var currFacility = group.GroupBy(f => f.CURRENCYID);
            //    foreach (var curr in currFacility)
            //    {
            //        var facility = group.Key;
            //        var currency = curr.First().TBL_CURRENCY.CURRENCYNAME;
            //        //var currentAmount = curr.Sum(p => p.CONTINGENTAMOUNT);
            //        //var currentAmountForLLLImpact = curr.Sum(p => p.CONTINGENTAMOUNT * (decimal)p.EXCHANGERATE);
            //        var currentAmount = curr.Sum(p => p.APPROVEDAMOUNT);
            //        var currentAmountForLLLImpact = curr.Sum(p => p.APPROVEDAMOUNT * (decimal)p.EXCHANGERATE);
            //        var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
            //                                  f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
            //        var proposedAmountTestForLLLImpact = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
            //                                  f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
            //        var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
            //        var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
            //        var LLLImpact = proposedAmountForLLLImpact / 3;
            //        var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
            //        var tenor = curr.Sum(p => p.APPROVEDTENOR);
            //        //var tenor = curr.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

            //        result = result + $@"
            //         <tr>
            //            <td>{facility}</td>
            //            <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
            //            <td>{currency}</td>
            //            <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
            //            <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
            //            <td>{String.Format("{0:0,0.00}", change)}</td>
            //            <td>{(tenor / 30)}</td>
            //        </tr>
            //        ";
            //    }
            //}

            foreach (var d in appDetails)
            {
                var contingentExists = contingentExposures.Exists(l => l.productCode.Trim() == d.TBL_PRODUCT.PRODUCTCODE.Trim());
                //var contingentExists = contingents.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

                if (!contingentExists)
                {
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
                    var LLLImpact = proposedAmount / 3;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;

                    result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
                }
            }
            return result;
        }

        private string GetTotalForeignContingentFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            var totalContingentSummary = GetTotalContingentFacilitiesSummaryLOS((int)CurrencyEnum.USD);
            if (totalContingentSummary.numberOfContingents > 0 || totalContingentSummary.numberOfNewFacilities > 0)
            {
                result = result + $@"
                     <tr>
                        <td><b>Total Contingents (FCY)</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalContingentSummary.totalLLLImpact)}</b></td>
                        <td><b>{totalContingentSummary.currency}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalContingentSummary.totalCurrentAmount)}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalContingentSummary.totalProposedAmount)}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalContingentSummary.totalChange)}</b></td>
                        <td><b>{(totalContingentSummary.totalTenors / 30)}</b></td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetTotalForeignFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            var totalSummary = GetTotalForeignFacilitiesLOS();
            if (totalSummary.Count() > 0)
            {
                result = result + $@"
                     <tr>
                        <td><b>Total Facilities</td>
                        <td><b>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalLLLImpact))}</b></td>
                        <td><b>{totalSummary.FirstOrDefault()?.currency}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalCurrentAmount))}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalProposedAmount))}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalChange))}</b></td>
                        <td><b>{(totalSummary.Sum(f => f.totalTenors) / 30)}</b></td>
                    </tr>
                ";
            }
            return result;
        }

        private TotalFacilitiesSummaryViewModel GetTotalDirectFacilitiesSummaryLOS(int currencyId)
        {
            var directSummary = new TotalFacilitiesSummaryViewModel();
            int numberOfNewFacilities = 0;
            var exposures = new List<CurrentCustomerExposure>();
            var loanExposures = new List<CurrentCustomerExposure>();
            var directExposures = new List<CurrentCustomerExposure>();
            var overdraftExposures = new List<CurrentCustomerExposure>();
            var initialLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var renewalLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithIncreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithDecreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            //var loans = new List<TBL_LOAN>();
            //var loans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            //var overdrafts = new List<TBL_LOAN_REVOLVING>();
            //var appLoans = context.TBL_LOAN_APPLICATION.Where(l => l.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.CAMInProgress).SelectMany(l => l.TBL_LOAN_APPLICATION_DETAIL).ToList();
            var appDetails = new List<TBL_LOAN_APPLICATION_DETAIL>();
            if (currencyId == (int)CurrencyEnum.NGN)
            {
                //loans = appLoans.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                //                                    && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability
                //                                    && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("lcy")).ToList();
                directExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                loanExposures = directExposures.Where(e => e.adjFacilityTypeId != (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
                overdraftExposures = directExposures.Where(e => e.adjFacilityTypeId == (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
                //loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && l.ISDISBURSED == true
                //                                && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                //overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                //                                                    && l.ISDISBURSED == true).ToList();
                //if(this.isThirdPartyFacility ==false)appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN &&
                //                                                                     d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability &&
                //                                                                     d.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();

                if (!this.isThirdPartyFacility)
                {
                    var ngnCurrencyId = (int)CurrencyEnum.NGN;
                    var nonContingentProductTypeId = (int)LoanProductTypeEnum.ContingentLiability;
                    var nonImportFinanceProductClassId = (int)ProductClassEnum.ImportFinanceFacilities;

                    appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL
                        .Where(d => d.TBL_CURRENCY.CURRENCYID == ngnCurrencyId &&
                                    d.TBL_PRODUCT.PRODUCTTYPEID != nonContingentProductTypeId &&
                                    d.TBL_PRODUCT.PRODUCTCLASSID != nonImportFinanceProductClassId)
                    .ToList();
                }
            }
            else
            {
                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("fcy")).ToList();
                //loans = appLoans.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                //                                    && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability
                //                                    && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                directExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                loanExposures = directExposures.Where(e => e.adjFacilityTypeId != (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
                overdraftExposures = directExposures.Where(e => e.adjFacilityTypeId == (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
                //loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN && l.ISDISBURSED == true
                //                                && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                //overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                //                                                   && l.ISDISBURSED == true).ToList();
                if(this.isThirdPartyFacility == false)appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN &&
                                                                                     d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability &&
                                                                                     d.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }
            directSummary.numberOfLoans = loanExposures.Count;
            directSummary.numberOfOverdrafts = overdraftExposures.Count;
            //var loanGroups = loans.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            //foreach (var group in loanGroups)
            //{
            //    var currency = "Naira";
            //    var currentAmount = group.Sum(p => p.APPROVEDAMOUNT * (decimal)p.EXCHANGERATE);
            //    //var currentAmount = group.Sum(p => p.OUTSTANDINGPRINCIPAL * (decimal)p.EXCHANGERATE) + group.Sum(p => p.OUTSTANDINGINTEREST * (decimal)p.EXCHANGERATE);
            //    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
            //                             (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
            //                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
            //                              :
            //                              (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
            //                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
            //    var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
            //                             (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
            //                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0
            //                              :
            //                              (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
            //                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
            //    var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
            //    var LLLImpact = (100 / 100) * proposedAmount;
            //    var change = proposedAmount - currentAmount;
            //    var tenor = tenorTest;
            //    directSummary.totalLLLImpact += LLLImpact;
            //    directSummary.currency = currency;
            //    directSummary.totalCurrentAmount += currentAmount;
            //    directSummary.totalProposedAmount += proposedAmount;
            //    directSummary.totalChange += change;
            //    directSummary.totalTenors += tenor;
            //}

            var loanExposuresGroups = loanExposures.GroupBy(f => f.productName);
            foreach(var group in loanExposuresGroups)
            {
                initialLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Initial).ToList();
                renewalLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Renewal).ToList();
                RenewalWithIncreaseLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithIncrease).ToList();
                RenewalWithDecreaseLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithDecrease).ToList();
                var facility = group.FirstOrDefault().facilityType;
                var currency = "Naira";
                if (renewalLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.outstandingsLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (renewalLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    directSummary.totalLLLImpact += LLLImpact;
                    directSummary.currency = currency;
                    directSummary.totalCurrentAmount += currentAmount;
                    directSummary.totalProposedAmount += proposedAmount;
                    directSummary.totalChange += change;
                    directSummary.totalTenors += tenor;
                }
                else if (RenewalWithIncreaseLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.outstandingsLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (RenewalWithIncreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    directSummary.totalLLLImpact += LLLImpact;
                    directSummary.currency = currency;
                    directSummary.totalCurrentAmount += currentAmount;
                    directSummary.totalProposedAmount += proposedAmount;
                    directSummary.totalChange += change;
                    directSummary.totalTenors += tenor;
                }
                else if (RenewalWithDecreaseLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.outstandingsLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (RenewalWithDecreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    directSummary.totalLLLImpact += LLLImpact;
                    directSummary.currency = currency;
                    directSummary.totalCurrentAmount += currentAmount;
                    directSummary.totalProposedAmount += proposedAmount;
                    directSummary.totalChange += change;
                    directSummary.totalTenors += tenor;
                }
                else
                {
                    var currentAmount = group.Sum(p => p.outstandingsLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (initialLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (initialLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (initialLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    directSummary.totalLLLImpact += LLLImpact;
                    directSummary.currency = currency;
                    directSummary.totalCurrentAmount += currentAmount;
                    directSummary.totalProposedAmount += proposedAmount;
                    directSummary.totalChange += change;
                    directSummary.totalTenors += tenor;
                }
            }

            var overdraftExposuresGroups = overdraftExposures.GroupBy(f => f.productName);
            foreach (var group in overdraftExposuresGroups)
            {
                initialLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Initial).ToList();
                renewalLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Renewal).ToList();
                RenewalWithIncreaseLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithIncrease).ToList();
                RenewalWithDecreaseLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithDecrease).ToList();
                var facility = group.FirstOrDefault().facilityType;
                var currency = "Naira";
                if (renewalLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (renewalLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    directSummary.totalLLLImpact += LLLImpact;
                    directSummary.currency = currency;
                    directSummary.totalCurrentAmount += currentAmount;
                    directSummary.totalProposedAmount += proposedAmount;
                    directSummary.totalChange += change;
                    directSummary.totalTenors += tenor;
                }
                else if (RenewalWithIncreaseLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (RenewalWithIncreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    directSummary.totalLLLImpact += LLLImpact;
                    directSummary.currency = currency;
                    directSummary.totalCurrentAmount += currentAmount;
                    directSummary.totalProposedAmount += proposedAmount;
                    directSummary.totalChange += change;
                    directSummary.totalTenors += tenor;
                }
                else if (RenewalWithDecreaseLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (RenewalWithDecreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    directSummary.totalLLLImpact += LLLImpact;
                    directSummary.currency = currency;
                    directSummary.totalCurrentAmount += currentAmount;
                    directSummary.totalProposedAmount += proposedAmount;
                    directSummary.totalChange += change;
                    directSummary.totalTenors += tenor;
                }
                else
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (initialLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (initialLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (initialLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    directSummary.totalLLLImpact += LLLImpact;
                    directSummary.currency = currency;
                    directSummary.totalCurrentAmount += currentAmount;
                    directSummary.totalProposedAmount += proposedAmount;
                    directSummary.totalChange += change;
                    directSummary.totalTenors += tenor;
                }
            }
            //var overdraftGroups = overdrafts.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            //foreach (var group in overdraftGroups)
            //{
            //    var facility = group.Key;
            //    var currency = "Naira";
            //    var currentAmount = group.Sum(p => p.OVERDRAFTLIMIT);
            //    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
            //                             (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
            //                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
            //                              :
            //                              (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
            //                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
            //    var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
            //                             (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
            //                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0
            //                              :
            //                              (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
            //                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
            //    var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
            //    var LLLImpact = (100 / 100) * proposedAmount;
            //    var change = proposedAmount - currentAmount;
            //    var tenor = tenorTest;
            //    directSummary.totalLLLImpact += LLLImpact;
            //    directSummary.currency = currency;
            //    directSummary.totalCurrentAmount += currentAmount;
            //    directSummary.totalProposedAmount += proposedAmount;
            //    directSummary.totalChange += change;
            //    directSummary.totalTenors += tenor;
            //}

            foreach (var d in appDetails)
            {
                var loanFacilityExists = loanExposures.Exists(l => l.productCode.Trim() == d.TBL_PRODUCT.PRODUCTCODE.Trim());
                var overdraftFacilityExists = overdraftExposures.Exists(o => o.productCode.Trim() == d.TBL_PRODUCT.PRODUCTCODE.Trim());
                //var loanFacilityExists = loans.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);
                //var overdraftFacilityExists = overdrafts.Exists(o => o.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

                if (!loanFacilityExists && !overdraftFacilityExists)
                {
                    ++numberOfNewFacilities;
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = "Naira";
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT * (decimal)d.EXCHANGERATE;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;
                    directSummary.totalLLLImpact += LLLImpact;
                    directSummary.currency = currency;
                    directSummary.totalCurrentAmount += currentAmount;
                    directSummary.totalProposedAmount += proposedAmount;
                    directSummary.totalChange += change;
                    directSummary.totalTenors += tenor;
                }
            }
            directSummary.numberOfNewFacilities = numberOfNewFacilities;
            return directSummary;
        }

        private TotalFacilitiesSummaryViewModel GetTotalContingentFacilitiesSummaryLOS(int currencyId)
        {
            var contingentsSummary = new TotalFacilitiesSummaryViewModel();
            int numberOfNewFacilities = 0;
            //var contingents = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var exposures = new List<CurrentCustomerExposure>();
            var contingentExposures = new List<CurrentCustomerExposure>();
            //var contingents = new List<TBL_LOAN_CONTINGENT>();
            var appDetails = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var initialLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var renewalLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithIncreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithDecreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            if (currencyId == (int)CurrencyEnum.NGN)
            {
                //var appLoans = context.TBL_LOAN_APPLICATION.Where(l => l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).SelectMany(l => l.TBL_LOAN_APPLICATION_DETAIL).ToList();
                //contingents = appLoans.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                //                                    && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == (int)LoanProductTypeEnum.ContingentLiability).ToList();
                //contingents = context.TBL_LOAN_CONTINGENT.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                //                                                && l.ISDISBURSED == true).ToList();
                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("lcy")).ToList();
                contingentExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                if (this.isThirdPartyFacility == false) appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN &&
                              d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == (int)LoanProductTypeEnum.ContingentLiability).ToList();
            }
            else
            {
                //var appLoans = context.TBL_LOAN_APPLICATION.Where(l => l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).SelectMany(l => l.TBL_LOAN_APPLICATION_DETAIL).ToList();
                //contingents = appLoans.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                //                                    && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == (int)LoanProductTypeEnum.ContingentLiability).ToList();
                //contingents = context.TBL_LOAN_CONTINGENT.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                //                                                && l.ISDISBURSED == true).ToList();
                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("fcy")).ToList();
                contingentExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                if (this.isThirdPartyFacility == false) appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN &&
                              d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == (int)LoanProductTypeEnum.ContingentLiability).ToList();
            }
            contingentsSummary.numberOfContingents = contingentExposures.Count;
            var loanGroups = contingentExposures.GroupBy(f => f.productName);
            foreach (var group in loanGroups)
            {
                initialLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Initial).ToList();
                renewalLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Renewal).ToList();
                RenewalWithIncreaseLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithIncrease).ToList();
                RenewalWithDecreaseLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithDecrease).ToList();
                var facility = group.FirstOrDefault().facilityType;
                if (renewalLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var currency = "Naira";
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (renewalLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0
                                              :
                                              (renewalLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = proposedAmount / 3;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    contingentsSummary.totalLLLImpact += LLLImpact;
                    contingentsSummary.currency = currency;
                    contingentsSummary.totalCurrentAmount += currentAmount;
                    contingentsSummary.totalProposedAmount += proposedAmount;
                    contingentsSummary.totalChange += change;
                    contingentsSummary.totalTenors += tenor;
                }
                else if (RenewalWithIncreaseLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var currency = "Naira";
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (RenewalWithIncreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0
                                              :
                                              (RenewalWithIncreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = proposedAmount / 3;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    contingentsSummary.totalLLLImpact += LLLImpact;
                    contingentsSummary.currency = currency;
                    contingentsSummary.totalCurrentAmount += currentAmount;
                    contingentsSummary.totalProposedAmount += proposedAmount;
                    contingentsSummary.totalChange += change;
                    contingentsSummary.totalTenors += tenor;
                }
                else if (RenewalWithDecreaseLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var currency = "Naira";
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (RenewalWithDecreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0
                                              :
                                              (RenewalWithDecreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = proposedAmount / 3;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    contingentsSummary.totalLLLImpact += LLLImpact;
                    contingentsSummary.currency = currency;
                    contingentsSummary.totalCurrentAmount += currentAmount;
                    contingentsSummary.totalProposedAmount += proposedAmount;
                    contingentsSummary.totalChange += change;
                    contingentsSummary.totalTenors += tenor;
                }
                else
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var currency = "Naira";
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (initialLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (initialLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (initialLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0
                                              :
                                              (initialLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                    var LLLImpact = proposedAmount / 3;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    contingentsSummary.totalLLLImpact += LLLImpact;
                    contingentsSummary.currency = currency;
                    contingentsSummary.totalCurrentAmount += currentAmount;
                    contingentsSummary.totalProposedAmount += proposedAmount;
                    contingentsSummary.totalChange += change;
                    contingentsSummary.totalTenors += tenor;
                }
            }

            foreach (var d in appDetails)
            {
                //var contingentsFacilityExists = contingents.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);
                var contingentsFacilityExists = contingentExposures.Exists(l => l.productCode.Trim() == d.TBL_PRODUCT.PRODUCTCODE.Trim());

                if (!contingentsFacilityExists)
                {
                    ++numberOfNewFacilities;
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = "Naira";
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT * (decimal)d.EXCHANGERATE;
                    var LLLImpact = proposedAmount / 3;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;
                    contingentsSummary.totalLLLImpact += LLLImpact;
                    contingentsSummary.currency = currency;
                    contingentsSummary.totalCurrentAmount += currentAmount;
                    contingentsSummary.totalProposedAmount += proposedAmount;
                    contingentsSummary.totalChange += change;
                    contingentsSummary.totalTenors += tenor;
                }
            }
            contingentsSummary.numberOfNewFacilities = numberOfNewFacilities;
            return contingentsSummary;
        }

        private TotalFacilitiesSummaryViewModel GetTotalImportFinanceFacilitiesSummaryLOS(int currencyId)
        {   //not yet implemented, just code for dummy data
            var IFFSummary = new TotalFacilitiesSummaryViewModel();
            int numberOfNewFacilities = 0;
            //var IFFs = new List<TBL_LOAN>();
            //var IFFs = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var exposures = new List<CurrentCustomerExposure>();
            var lcs = new List<CurrentCustomerExposure>();
            var tradeLoans = new List<CurrentCustomerExposure>();
            var iFFExposures = new List<CurrentCustomerExposure>();
            var appDetails = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var initialLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var renewalLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithIncreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            var RenewalWithDecreaseLoans = new List<TBL_LOAN_APPLICATION_DETAIL>();
            if (currencyId == (int)CurrencyEnum.NGN)
            {
                //IFFs = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                //                                && l.ISDISBURSED == true && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                //var appLoans = context.TBL_LOAN_APPLICATION.Where(l => l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).SelectMany(l => l.TBL_LOAN_APPLICATION_DETAIL).ToList();
                //IFFs = appLoans.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                //                                    && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("lcy")).ToList();
                lcs = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && e.adjFacilityTypeString.Contains("LC")).ToList();
                tradeLoans = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                iFFExposures.AddRange(lcs);
                iFFExposures.AddRange(tradeLoans);
                if (this.isThirdPartyFacility == false) appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN 
                                                && d.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }
            else
            {
                //IFFs = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                //                                && l.ISDISBURSED == true && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                //var appLoans = context.TBL_LOAN_APPLICATION.Where(l => l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).SelectMany(l => l.TBL_LOAN_APPLICATION_DETAIL).ToList();
                //IFFs = appLoans.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                //                                    && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                exposures = GetExposures(true).Where(e => e.currencyType.ToLower().Contains("fcy")).ToList();
                lcs = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && e.adjFacilityTypeString.Contains("LC")).ToList();
                tradeLoans = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
                iFFExposures.AddRange(lcs);
                iFFExposures.AddRange(tradeLoans);
                if (this.isThirdPartyFacility == false) appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                && d.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }
            IFFSummary.numberOfImportFinanceFacilities = iFFExposures.Count;
            var loanGroups = iFFExposures.GroupBy(f => f.productName);
            foreach (var group in loanGroups)
            {
                var currency = "Naira";
                
                    initialLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                         f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Initial).ToList();
                    renewalLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.Renewal).ToList();
                    RenewalWithIncreaseLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithIncrease).ToList();
                    RenewalWithDecreaseLoans = appDetails.Where(f => group.FirstOrDefault().productCode.Trim() ==
                                             f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.LOANDETAILREVIEWTYPEID == (int)LoanDetailReviewTypeEnum.RenewalWithDecrease).ToList();
                var facility = group.FirstOrDefault().facilityType;
                if (renewalLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (renewalLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (renewalLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    IFFSummary.totalLLLImpact += LLLImpact;
                    IFFSummary.currency = currency;
                    IFFSummary.totalCurrentAmount += currentAmount;
                    IFFSummary.totalProposedAmount += proposedAmount;
                    IFFSummary.totalChange += change;
                    IFFSummary.totalTenors += tenor;
                }
                else if (RenewalWithIncreaseLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (RenewalWithIncreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (RenewalWithIncreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    IFFSummary.totalLLLImpact += LLLImpact;
                    IFFSummary.currency = currency;
                    IFFSummary.totalCurrentAmount += currentAmount;
                    IFFSummary.totalProposedAmount += proposedAmount;
                    IFFSummary.totalChange += change;
                    IFFSummary.totalTenors += tenor;
                }
                else if (RenewalWithDecreaseLoans.Any())
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (RenewalWithDecreaseLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (RenewalWithDecreaseLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = proposedAmountTest;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    IFFSummary.totalLLLImpact += LLLImpact;
                    IFFSummary.currency = currency;
                    IFFSummary.totalCurrentAmount += currentAmount;
                    IFFSummary.totalProposedAmount += proposedAmount;
                    IFFSummary.totalChange += change;
                    IFFSummary.totalTenors += tenor;
                }
                else
                {
                    var currentAmount = group.Sum(p => p.approvedAmountLcy);
                    var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                             (initialLoans?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                              :
                                              (initialLoans?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (initialLoans?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = tenorTest;
                    IFFSummary.totalLLLImpact += LLLImpact;
                    IFFSummary.currency = currency;
                    IFFSummary.totalCurrentAmount += currentAmount;
                    IFFSummary.totalProposedAmount += proposedAmount;
                    IFFSummary.totalChange += change;
                    IFFSummary.totalTenors += tenor;
                }
                //var currentAmount = group.Sum(p => p.OUTSTANDINGPRINCIPAL * (decimal)p.EXCHANGERATE) + group.Sum(p => p.OUTSTANDINGINTEREST * (decimal)p.EXCHANGERATE);
                //var currentAmount = group.Sum(p => p.approvedAmountLcy);
                //var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                //                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().productCode.Trim() ==
                //                           f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                //                           :
                //                           (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().productCode.Trim() ==
                //                           f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                //var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                //                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().productCode.Trim() ==
                //                          f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0
                //                          :
                //                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().productCode.Trim() ==
                //                          f.TBL_PRODUCT.PRODUCTCODE.Trim() && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                //var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                //var LLLImpact = (100 / 100) * proposedAmount;
                //var change = proposedAmount - currentAmount;
                //var tenor = tenorTest;
            }

            foreach (var d in appDetails)
            {
                //var IFFExists = iFFExposures.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);
                var IFFExists = iFFExposures.Exists(l => l.productCode.Trim() == d.TBL_PRODUCT.PRODUCTCODE.Trim());

                if (!IFFExists)
                {
                    ++numberOfNewFacilities;
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = "Naira";
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT * (decimal)d.EXCHANGERATE;
                    var LLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;
                    IFFSummary.totalLLLImpact += LLLImpact;
                    IFFSummary.currency = currency;
                    IFFSummary.totalCurrentAmount += currentAmount;
                    IFFSummary.totalProposedAmount += proposedAmount;
                    IFFSummary.totalChange += change;
                    IFFSummary.totalTenors += tenor;
                }
            }
            IFFSummary.numberOfNewFacilities = numberOfNewFacilities;
            return IFFSummary;
        }

        private string GetGroupExposureMarkup()
        {
            var result = String.Empty;
            var exposures = new List<CurrentCustomerExposure>();
            exposures = GetExposures();
            var directExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
            var loanExposures = directExposures.Where(e => e.adjFacilityTypeId != (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
            var overdraftExposures = directExposures.Where(e => e.adjFacilityTypeId == (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
            var contingents = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
            var lcs = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && e.adjFacilityTypeString.Contains("LC")).ToList();
            var tradeLoans = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
            var exposureGroupsByCustomer = exposures.GroupBy(e => e.customerCode);
            var n = 0;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=1000px align=center cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>Related Obligors(domestic)</b></th>
                        <th><b>Facility Name</b></th>
                        <th><b>LLL Impact (NGN)</b></th>
                        <th><b>Currency</b></th>
                        <th><b>Approved Amount</b></th>
                        <th><b>Outstanding Exposure</b></th>
                        <th><b>[O/S] Ccy</b></th>
                        <th><b>End Date</b></th>
                    </tr>
                ";
            if (loanExposures.Count > 0 || overdraftExposures.Count > 0)
            {
                result = result + $@"<tr><td>Direct Facilities:</td></tr>";
                if (loanExposures.Count > 0)
                {
                    var customers = loanExposures.GroupBy(e => e.customerCode);
                    foreach (var cust in customers)
                    {
                        var directsGroup = cust.GroupBy(f => f.productCode.Trim());

                        foreach (var product in directsGroup)
                        {
                            var facility = product.FirstOrDefault().facilityType;
                            var currency = "Naira";
                            //var currency = product.currency;
                            var currentAmount = product.Sum(p => p.outstandings);
                            var approvedAmount = product.Sum(p => p.approvedAmount);
                            var currentAmountForLLL = product.Sum(p => p.outstandingsLcy);
                            var approvedAmountForLLL = product.Sum(p => p.approvedAmountLcy);
                            var amountForLLL = currentAmountForLLL;
                            var LLLImpact = amountForLLL;

                            result = result + $@"
                     <tr>
                        <td>{product.FirstOrDefault().customerName}</td>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", approvedAmountForLLL)}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmountForLLL)}</td>
                        <td>{currency}</td>
                        <td>{product.Max(p => p.maturityDate)}</td>
                    </tr>
                    ";
                        }
                    }
                }

                if (overdraftExposures.Count > 0)
                {
                    var customers = overdraftExposures.GroupBy(e => e.customerCode);
                    foreach (var cust in customers)
                    {
                        var overdraftsGroup = cust.GroupBy(f => f.productCode.Trim());

                        foreach (var product in overdraftsGroup)
                        {
                            var facility = product.FirstOrDefault().facilityType;
                            var currency = "Naira";
                            var currentAmount = product.Sum(p => p.outstandings);
                            var approvedAmount = product.Sum(p => p.approvedAmount);
                            var currentAmountForLLL = product.Sum(p => p.outstandingsLcy);
                            var approvedAmountForLLL = product.Sum(p => p.approvedAmountLcy);
                            var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                            var LLLImpact = amountForLLL;

                            result = result + $@"
                     <tr>
                        <td>{product.FirstOrDefault().customerName}</td>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", approvedAmountForLLL)}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmountForLLL)}</td>
                        <td>{currency}</td>
                        <td>{product.Max(p => p.maturityDate)}</td>
                    </tr>
                    ";
                        }
                    }
                }

            }

            if (contingents.Count() > 0)
            {
                result = result + $@"<tr><td>Contingent Facilities:</td></tr>";
                var customers = contingents.GroupBy(e => e.customerCode);
                foreach (var cust in customers)
                {
                    var contingentsGroup = cust.GroupBy(f => f.productCode.Trim());
                    foreach (var product in contingentsGroup)
                    {
                        var facility = product.FirstOrDefault().facilityType;
                        var currency = "Naira";
                        var currentAmount = product.Sum(p => p.outstandings);
                        var approvedAmount = product.Sum(p => p.approvedAmount);
                        var currentAmountForLLL = product.Sum(p => p.outstandingsLcy);
                        var approvedAmountForLLL = product.Sum(p => p.approvedAmountLcy);
                        var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                        var LLLImpact = (amountForLLL / 3);
                        //var tenor = curr.Sum(p => p.APPROVEDTENOR);

                        result = result + $@"
                     <tr>
                        <td>{product.FirstOrDefault().customerName}</td>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", approvedAmountForLLL)}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmountForLLL)}</td>
                        <td>{currency}</td>
                        <td>{product.Max(p => p.maturityDate)}</td>
                    </tr>
                    ";
                    }

                    //foreach (var product in cust)
                    //{
                    //    var facility = product.facilityType;
                    //    var currency = product.currency;
                    //    var currentAmount = product.outstandings;
                    //    var approvedAmount = product.approvedAmount;
                    //    var currentAmountForLLL = product.outstandingsLcy;
                    //    var approvedAmountForLLL = product.approvedAmountLcy;
                    //    var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                    //    var LLLImpact = (amountForLLL / 3);

                    //    result = result + $@"
                    // <tr>
                    //    <td>{product.customerName}</td>
                    //    <td>{facility}</td>
                    //    <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                    //    <td>{currency}</td>
                    //    <td>{String.Format("{0:0,0.00}", approvedAmount)}</td>
                    //    <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                    //    <td>{currency}</td>
                    //    <td>{product.maturityDate}</td>
                    //</tr>
                    //";
                    //}
                }
            }

            if (tradeLoans.Count() > 0 || lcs.Count() > 0)
            {
                result = result + $@"<tr><td>(Import Finance Facilities)</td></tr>";
                if (lcs.Count() > 0)
                {
                    result = result + $@"<tr><td>(Contingent)</td></tr>";
                    var customers = lcs.GroupBy(e => e.customerCode);
                    foreach (var cust in customers)
                    {
                        var contingentsGroup = cust.GroupBy(f => f.productCode.Trim());
                        foreach (var product in contingentsGroup)
                        {
                            var facility = product.FirstOrDefault().facilityType;
                            var currency = "Naira";
                            var currentAmount = product.Sum(p => p.outstandings);
                            var approvedAmount = product.Sum(p => p.approvedAmount);
                            var currentAmountForLLL = product.Sum(p => p.outstandingsLcy);
                            var approvedAmountForLLL = product.Sum(p => p.approvedAmountLcy);
                            var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                            var LLLImpact = amountForLLL;
                            //var tenor = curr.Sum(p => p.APPROVEDTENOR);

                            result = result + $@"
                             <tr>
                                <td>{product.FirstOrDefault().customerName}</td>
                                <td>{facility}</td>
                                <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                                <td>{currency}</td>
                                <td>{String.Format("{0:0,0.00}", approvedAmountForLLL)}</td>
                                <td>{String.Format("{0:0,0.00}", currentAmountForLLL)}</td>
                                <td>{currency}</td>
                                <td>{product.Max(p => p.maturityDate)}</td>
                            </tr>
                            ";
                        }

                        //foreach (var product in cust)
                        //{
                        //    var facility = product.facilityType;
                        //    var currency = product.currency;
                        //    var currentAmount = product.outstandings;
                        //    var approvedAmount = product.approvedAmount;
                        //    var currentAmountForLLL = product.outstandingsLcy;
                        //    var approvedAmountForLLL = product.approvedAmountLcy;
                        //    var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                        //    var LLLImpact = amountForLLL;
                        //    //var LLLImpact = (amountForLLL / 3);
                        //    //var tenor = curr.Sum(p => p.APPROVEDTENOR);

                        //    result = result + $@"
                        //     <tr>
                        //        <td>{product.customerName}</td>
                        //        <td>{facility}</td>
                        //        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        //        <td>{currency}</td>
                        //        <td>{String.Format("{0:0,0.00}", approvedAmount)}</td>
                        //        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        //        <td>{currency}</td>
                        //        <td>{product.maturityDate}</td>
                        //    </tr>
                        //    ";
                        //}
                    }
                }

                if (tradeLoans.Count() > 0)
                {
                    result = result + $@"<tr><td>(Direct)</td></tr>";
                    var customers = tradeLoans.GroupBy(e => e.customerCode);
                    foreach (var cust in customers)
                    {
                        var directsGroup = cust.GroupBy(f => f.productCode.Trim());
                        foreach (var product in directsGroup)
                        {
                            var facility = product.FirstOrDefault().facilityType;
                            var currency = "Naira";
                            var currentAmount = product.Sum(p => p.outstandings);
                            var approvedAmount = product.Sum(p => p.approvedAmount);
                            var currentAmountForLLL = product.Sum(p => p.outstandingsLcy);
                            var approvedAmountForLLL = product.Sum(p => p.approvedAmountLcy);
                            var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                            var LLLImpact = amountForLLL;
                            //var tenor = curr.Sum(p => p.APPROVEDTENOR);

                            result = result + $@"
                             <tr>
                                <td>{product.FirstOrDefault().customerName}</td>
                                <td>{facility}</td>
                                <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                                <td>{currency}</td>
                                <td>{String.Format("{0:0,0.00}", approvedAmountForLLL)}</td>
                                <td>{String.Format("{0:0,0.00}", currentAmountForLLL)}</td>
                                <td>{currency}</td>
                                <td>{product.Max(p => p.maturityDate)}</td>
                            </tr>
                            ";
                        }

                        //foreach (var product in cust)
                        //{
                        //    var facility = product.facilityType;
                        //    var currency = product.currency;
                        //    var currentAmount = product.outstandings;
                        //    var approvedAmount = product.approvedAmount;
                        //    var currentAmountForLLL = product.outstandingsLcy;
                        //    var approvedAmountForLLL = product.approvedAmountLcy;
                        //    var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                        //    var LLLImpact = amountForLLL;
                        //    //var tenor = curr.Sum(p => p.APPROVEDTENOR);

                        //    result = result + $@"
                        //     <tr>
                        //        <td>{product.customerName}</td>
                        //        <td>{facility}</td>
                        //        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        //        <td>{currency}</td>
                        //        <td>{String.Format("{0:0,0.00}", approvedAmount)}</td>
                        //        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        //        <td>{currency}</td>
                        //        <td>{product.maturityDate}</td>
                        //    </tr>
                        //    ";
                        //}
                    }
                }
            }
            //foreach (var customerGroups in exposureGroupsByCustomer)
            //{
            //    var customerName = customerGroups.Key;
            //    var facilities = customerGroups.GroupBy(c => c.facilityType.Trim());
            //    foreach (var facility in facilities)
            //    {
            //        ++n;
            //        result = result + $@"
            //         <tr>
            //            <td>{customerName}</td>
            //            <td>{facility.Key}</td>
            //            <td>{facility.FirstOrDefault()?.currency}</td>
            //            <td>{String.Format("{0:0,0.00}", facility.Sum(f => f.approvedAmount))}</td>
            //            <td>{String.Format("{0:0,0.00}", facility.Sum(f => f.outstandings))}</td>
            //            <td>{facility.Max(f => f.bookingDate).ToShortDateString()}</td>
            //            <td>{facility.Max(f => f.maturityDate).ToShortDateString()}</td>
            //        </tr>
            //    ";
            //    }
            //}
            result = result + $@"
                    {GetTotalGroupExposureMarkup()}
                ";
            result = result + $"</table>";
            return result;
        }

        public List<CurrentCustomerExposure> GetExposures(bool isForGFS = false)
        {
            var exposures = new List<CurrentCustomerExposure>();
            exposures = this.globalExposure;
            if (isForGFS)
            {
                //var custCode = context.TBL_CUSTOMER.Find(customerId).CUSTOMERCODE;
                var custCode = context.TBL_CUSTOMER.Find(loanApplication?.TBL_LOAN_APPLICATION_DETAIL?.FirstOrDefault().CUSTOMERID)?.CUSTOMERCODE;
                var exposure = exposures.Where(e => e.customerCode == custCode).ToList();
                return exposure;
            }
            return exposures;
        }

        private List<CurrentCustomerExposure> GetGloabalExposures()
        {
            var exposures = new List<CurrentCustomerExposure>();
            var customerIds = new List<CustomerExposure>();
            int customerId;
            if (loanApplication.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.Single)
            {
                customerIds.Add(new CustomerExposure { customerId = (int)loanApplication.CUSTOMERID });
                exposures = GetCustomerExposure(customerIds, loanApplication.COMPANYID);
                customerId = (int)loanApplication.CUSTOMERID;
            }
            else
            {
                customerIds.Add(new CustomerExposure { customerId = (int)loanApplication.CUSTOMERGROUPID });
                //customerIds.Add(new CustomerExposure { customerId = (int)loanApplication.TBL_CUSTOMER_GROUP.TBL_CUSTOMER_GROUP_MAPPING.FirstOrDefault().CUSTOMERID });
                exposures = GetCustomerExposure(customerIds, loanApplication.COMPANYID);
                customerId = (int)loanApplication.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID;
                //exposures = GetGroupExposurebyCustomerId((int)this.loanApplication.CUSTOMERGROUPID, this.loanApplication.COMPANYID);
            }
            return exposures;
        }

        public List<CurrentCustomerExposure> GetExposuresLMS()
        {
            var exposures = new List<CurrentCustomerExposure>();
            var customerIds = new List<CustomerExposure>();
            if (lmsrApplication.CUSTOMERGROUPID > 0)
            {
                customerIds.Add(new CustomerExposure { customerId = (int)lmsrApplication.CUSTOMERGROUPID });
                exposures = GetCustomerExposureLMS(customerIds, lmsrApplication.COMPANYID);
            }
            else
            {
                customerIds.Add(new CustomerExposure { customerId = (int)lmsrApplication.CUSTOMERID });
                exposures = GetCustomerExposureLMS(customerIds, lmsrApplication.COMPANYID);
            }
            return exposures;
        }

        private TotalFacilitiesSummaryViewModel GetTotalGroupLendingLimit(bool isForGFS = false)
        {
            var exposures = new List<CurrentCustomerExposure>();
            var exposure = new TotalFacilitiesSummaryViewModel();
            //var customerIds = new List<CustomerExposure>();
            exposures = GetExposures(isForGFS);
            var currentAmount = new decimal();
            var approvedAmount = new decimal();
            var amountForLLL = new decimal();
            var LLLImpact = new decimal();
            var directExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
            var loanExposures = directExposures.Where(e => e.adjFacilityTypeId != (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
            var overdraftExposures = directExposures.Where(e => e.adjFacilityTypeId == (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
            var contingents = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
            var lcs = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && e.adjFacilityTypeString.Contains("LC")).ToList();
            var tradeLoans = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
            foreach (var product in loanExposures)
            {
                currentAmount = product.outstandingsLcy;
                amountForLLL = currentAmount;
                LLLImpact += amountForLLL;
            }

            foreach (var product in overdraftExposures)
            {
                currentAmount = product.outstandingsLcy;
                approvedAmount = product.approvedAmountLcy;
                amountForLLL = (currentAmount >= approvedAmount) ? currentAmount : approvedAmount;
                LLLImpact += amountForLLL;
            }

            foreach (var product in contingents)
            {
                currentAmount = product.outstandingsLcy;
                approvedAmount = product.approvedAmountLcy;
                amountForLLL = (currentAmount >= approvedAmount) ? currentAmount : approvedAmount;
                LLLImpact += (amountForLLL / 3);
            }

            foreach (var product in lcs)
            {
                currentAmount = product.outstandingsLcy;
                approvedAmount = product.approvedAmountLcy;
                amountForLLL = (currentAmount >= approvedAmount) ? currentAmount : approvedAmount;
                LLLImpact += amountForLLL;
                //LLLImpact += (amountForLLL / 3);
            }

            foreach (var product in tradeLoans)
            {
                currentAmount = product.outstandingsLcy;
                approvedAmount = product.approvedAmountLcy;
                amountForLLL = (currentAmount >= approvedAmount) ? currentAmount : approvedAmount;
                LLLImpact += amountForLLL;
            }

            exposure.totalLLLImpact = LLLImpact;
            return exposure;

        }
        private string GetTotalGroupExposureMarkup()
        {
            var result = String.Empty;
            var exposures = new List<CurrentCustomerExposure>();
            var exposure = GetTotalGroupLendingLimit();
            //var customerIds = new List<CustomerExposure>();
            exposures = GetExposures();
            CurrentCustomerExposure totalExposure;
            
            totalExposure = new CurrentCustomerExposure()
            {
                facilityType = "TOTAL",
                outstandings = exposures.Sum(t => t.outstandingsLcy),
                approvedAmount = exposures.Sum(t => t.approvedAmountLcy),
                //outstandings = exposures.Sum(t => t.outstandings),
                //approvedAmount = exposures.Sum(t => t.approvedAmount),
            };
            result = result + $@"
                     <tr>
                        <td><b>{totalExposure.facilityType}</b></td>
                        <td>&nbsp;</td>
                        <td><b>{String.Format("{0:0,0.00}", exposure.totalLLLImpact)}</b></td>
                        <td><b>Naira</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalExposure.approvedAmount)}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalExposure.outstandings)}</b></td>
                        <td><b>Naira</b></td>
                        <td>&nbsp;</td>
                    </tr>
                ";

            return result;
        }

        private int GetCurrentOperationId()
        {
            //if (this.loanApplication?.OPERATIONID > 0 && this.loanApplication != null)
            if (this.lmsrApplication != null)
            {
                return this.lmsrApplication.OPERATIONID;
            }else if(this.loanApplication != null)
            {
                return this.loanApplication.OPERATIONID;
            }
            else
            {
                return this.operationId;
            }
        }

        private string GetGenericApprovalsMarkup(int targetIdForWorkFlow, bool getAll = false)
        {
            var appraisals = GetAppraisalMemorandumTrail(targetIdForWorkFlow, GetCurrentOperationId(), getAll).OrderBy(a => a.approvalTrailId).ToList();
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=1000px align=center cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>Role</b></th>
                        <th><b>Name</b></th>
                        <th><b>Decision</b></th>
                        <th><b>Comment</b></th>
                        <th><b>Date</b></th>
                    </tr>
                    ";
            foreach (var trail in appraisals)
            {
                result = result + $@"
                    <tr>
                        <td>{trail.fromApprovalLevelName.ToUpper()}</td>
                        <td>{trail.fromStaffName}</td>
                        <td>{GetDecision(trail.approvalStatusId)}</td>
                        <td>{trail.comment}</td>
                        <td>{trail.systemArrivalDateTime}</td>
                    </tr>
                ";
            }

            result = result + $"</table>";
            return result;

        }

        private string GetApprovalsMarkup(bool getAll = false)
        {
            var appraisals = GetAppraisalMemorandumTrail(this.targetId, GetCurrentOperationId(), getAll).OrderBy(a => a.approvalTrailId).ToList();
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=1000px align=center cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>Role</b></th>
                        <th><b>Name</b></th>
                        <th><b>Decision</b></th>
                        <th><b>Comment</b></th>
                        <th><b>Date</b></th>
                    </tr>
                    ";
            foreach (var trail in appraisals)
            {
                result = result + $@"
                    <tr>
                        <td>{trail.fromApprovalLevelName.ToUpper()}</td>
                        <td>{trail.fromStaffName}</td>
                        <td>{GetDecision(trail.approvalStatusId)}</td>
                        <td>{trail.comment}</td>
                        <td>{trail.systemArrivalDateTime}</td>
                    </tr>
                ";
            }

            result = result + $"</table>";
            return result;

        }


        private string GetLMSApprovalsMarkup(int targetId, int operationId)
        {
            var appraisals = GenericLMSApprovalTrail(targetId, operationId).OrderBy(a => a.approvalTrailId).ToList();
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=1000px align=center cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>Role</b></th>
                        <th><b>Name</b></th>
                        <th><b>Decision</b></th>
                        <th><b>Comment</b></th>
                        <th><b>Date</b></th>
                    </tr>
                    ";
            foreach (var trail in appraisals)
            {
                result = result + $@"
                    <tr>
                        <td>{trail.fromApprovalLevelName.ToUpper()}</td>
                        <td>{trail.fromStaffName}</td>
                        <td>{GetDecision(trail.approvalStatusId)}</td>
                        <td>{trail.comment}</td>
                        <td>{trail.systemArrivalDateTime}</td>
                    </tr>
                ";
            }

            result = result + $"</table>";
            return result;

        }


        //private string GetApprovalsMarkupForAllLOS(int targetId)
        //{
        //    var appraisals = GetAppraisalMemorandumTrail(targetId, GetCurrentOperationId(), true).OrderBy(a => a.approvalTrailId).ToList();
        //    var result = String.Empty;
        //    result = result + $@"
        //        <br/><h3><b>APPROVALS</b></h3>
        //        <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
        //            <tr>
        //                <th><b>Role</b></th>
        //                <th><b>Name</b></th>
        //                <th><b>Decision</b></th>
        //                <th><b>Comment</b></th>
        //                <th><b>Date</b></th>
        //            </tr>
        //            ";
        //    foreach (var trail in appraisals)
        //    {
        //        result = result + $@"
        //            <tr>
        //                <td>{trail.fromApprovalLevelName.ToUpper()}</td>
        //                <td>{trail.fromStaffName}</td>
        //                <td>{GetDecision(trail.vote)}</td>
        //                <td>{trail.comment}</td>
        //                <td>{trail.systemArrivalDateTime}</td>
        //            </tr>
        //        ";
        //    }

        //    result = result + $"</table>";
        //    return result;

        //}


        public IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrailDrawdownMemoReversal(int applicationId, bool getAll = true)
        {
            int[] operations = { (int)OperationsEnum.CreditCardDrawdownRequest,(int)OperationsEnum.IndividualDrawdownRequest,
                (int)OperationsEnum.CorporateDrawdownRequest,(int)OperationsEnum.CommercialLoanBooking,(int)OperationsEnum.ContigentLoanBooking,
                (int)OperationsEnum.RevolvingLoanBooking,(int)OperationsEnum.TermLoanBooking,(int)OperationsEnum.ForeignExchangeLoanBooking,
                (int)OperationsEnum.RevolvingTranchDisbursement
            };
            var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;
            var allstaff = this.GetAllStaffNames();

           
            var trail = context.TBL_APPROVAL_TRAIL.Where(x => operations.Contains(x.OPERATIONID) && x.TARGETID == applicationId).ToList();

            if (getAll)
            {
                trail = context.TBL_APPROVAL_TRAIL.Where(x => operations.Contains(x.OPERATIONID) && x.TARGETID == applicationId).ToList();
            }

            var data = trail.Select(x => new ApprovalTrailViewModel
            {
                approvalTrailId = x.APPROVALTRAILID,
                comment = x.COMMENT,
                targetId = x.TARGETID,
                operationId = x.OPERATIONID,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? allstaff.FirstOrDefault(r => r.staffId == x.REQUESTSTAFFID).role : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                loopedStaffId = x.LOOPEDSTAFFID,
                toStaffId = x.TOSTAFFID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                vote = x.VOTE,
                //applicationId = application.LOANAPPLICATIONID,
                commentStage = "Drawdown",
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            })?.OrderByDescending(x => x.systemArrivalDateTime).ToList();

            //data.AddRange(GetOfferLetterTrail(applicationId));
            var bookingId = context.TBL_LOAN_BOOKING_REQUEST.Find(applicationId);
            var loanApplicationId = context.TBL_LOAN_APPLICATION_DETAIL.Find(bookingId.LOANAPPLICATIONDETAILID).LOANAPPLICATIONID;
            var appraisalOperation = context.TBL_LOAN_APPLICATION.Find(loanApplicationId).OPERATIONID;

            if (getAll)
            {
                data.AddRange(GetNonAppraisalTrail(loanApplicationId, (short)OperationsEnum.OfferLetterApproval, "Offer Letter"));
                data.AddRange(GetNonAppraisalTrail(loanApplicationId, (short)OperationsEnum.LoanAvailment, "Availment"));
                data.AddRange(GetNonAppraisalTrail(loanApplicationId, appraisalOperation, "Credit Appraisal"));
            }
            foreach (var t in data)
            {
                if (t.fromApprovalLevelId == t.toApprovalLevelId)
                {
                    if (t.loopedStaffId > 0)
                    {
                        t.toStaffName = allstaff.FirstOrDefault(s => s.staffId == t.loopedStaffId)?.name;
                        t.toApprovalLevelName = allstaff.FirstOrDefault(s => s.staffId == t.loopedStaffId)?.role;
                    }
                    else
                    {
                        t.fromApprovalLevelName = allstaff.FirstOrDefault(s => s.staffId == t.requestStaffId)?.role;
                        t.toStaffName = t.toStaffId != null ? allstaff.FirstOrDefault(s => s.staffId == t.toStaffId)?.name : t.toStaffName;
                    }
                }

            }

            data.OrderByDescending(d => d.systemArrivalDateTime);

            return data;
        }


        public IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrailMemo(int applicationId, int operationId, bool getAll = true)
        {

            var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;

            var allstaff = this.GetAllStaffNames();

            var application = context.TBL_LOAN_APPLICATION_DETAIL.Find(applicationId);

            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == application.TBL_LOAN_APPLICATION.OPERATIONID && x.TARGETID == application.LOANAPPLICATIONID).ToList();

            if (getAll)
            {
                trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == application.TBL_LOAN_APPLICATION.OPERATIONID && x.TARGETID == application.LOANAPPLICATIONID).ToList();
            }

            var data = trail.Select(x => new ApprovalTrailViewModel
            {
                approvalTrailId = x.APPROVALTRAILID,
                comment = x.COMMENT,
                targetId = x.TARGETID,
                operationId = x.OPERATIONID,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                //applicationId = application.LOANAPPLICATIONID,
                commentStage = "Credit Appaisal",
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            })?.OrderByDescending(x => x.systemArrivalDateTime).ToList();

            //data.AddRange(GetOfferLetterTrail(applicationId));
            if (getAll)
            {
                data.AddRange(GetNonAppraisalTrail(application.LOANAPPLICATIONID, (short)OperationsEnum.OfferLetterApproval, "Offer Letter"));
                data.AddRange(GetNonAppraisalTrail(application.LOANAPPLICATIONID, (short)OperationsEnum.LoanAvailment, "Availment"));

                foreach (var t in data.ToList())
                {
                    var facilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == application.LOANAPPLICATIONID).ToList();
                    foreach (var f in facilities)
                    {
                        var request = context.TBL_LOAN_BOOKING_REQUEST.Where(x => x.LOANAPPLICATIONDETAILID == f.LOANAPPLICATIONDETAILID);
                        foreach (var r in request)
                        {
                            if (r?.OPERATIONID != null)
                            {
                                data.AddRange(GetNonAppraisalTrail(r.LOAN_BOOKING_REQUESTID, r.OPERATIONID ?? 0, "Drawdown"));
                            }
                            data.AddRange(GetNonAppraisalTrail(r.LOAN_BOOKING_REQUESTID, (short)OperationsEnum.TermLoanBooking, "Booking"));
                            data.AddRange(GetNonAppraisalTrail(r.LOAN_BOOKING_REQUESTID, (short)OperationsEnum.RevolvingLoanBooking, "Booking"));
                            data.AddRange(GetNonAppraisalTrail(r.LOAN_BOOKING_REQUESTID, (short)OperationsEnum.ContigentLoanBooking, "Booking"));
                        }
                    }
                };
            }

            //for Filtering multiple occuring levels
            var data2 = data.ToList();
            var testData = data.ToList();
            foreach (var t in testData)
            {
                var firstTrailForLevel = testData.OrderBy(x => x.approvalTrailId).FirstOrDefault(x => x.fromApprovalLevelId == t.fromApprovalLevelId);
                var multipleTrails = testData.Where(d => d.fromApprovalLevelId == firstTrailForLevel.fromApprovalLevelId && d.approvalTrailId != firstTrailForLevel.approvalTrailId).ToList();
                foreach (var tr in multipleTrails)
                {
                    data2.GroupBy(d => d.systemArrivalDateTime == tr.systemArrivalDateTime);
                }
            }
            data = data2;

            data.OrderByDescending(d => d.systemArrivalDateTime);

            return data;
        }

        private IEnumerable<ApprovalTrailViewModel> GetNonAppraisalTrail(int applicationId, int operationid, string commentStage)
        {
            var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;

            var allstaff = this.GetAllStaffNames();

            //var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationid && x.TARGETID == applicationId && x.FROMAPPROVALLEVELID != null).ToList();
            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationid && x.TARGETID == applicationId).ToList();

            var data = trail.Select(x => new ApprovalTrailViewModel
            {
                approvalTrailId = x.APPROVALTRAILID,
                comment = x.COMMENT,
                targetId = x.TARGETID,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                toStaffId = x.TOSTAFFID,
                loopedStaffId = x.LOOPEDSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                commentStage = commentStage,
                vote = x.VOTE,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            })?.OrderBy(x => x.systemArrivalDateTime).ToList();


            var initiation = data.FirstOrDefault();
            if (initiation?.fromApprovalLevelId == null)
            {
                //data.Remove(initiation);
                data = data.OrderByDescending(d => d.approvalTrailId).ToList();
            }
            data.OrderByDescending(d => d.systemArrivalDateTime);
            return data;
            /* var staffRoles = context.TBL_STAFF_ROLE.ToList();
             var staffs = from s in context.TBL_STAFF select s;
             var allstaff = this.GetAllStaffNames();
             var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.OPERATIONID == operationid && x.TARGETID == applicationId && x.FROMAPPROVALLEVELID != null).ToList();

             var data = trail.Select(x => new ApprovalTrailViewModel
             {
                 approvalTrailId = x.APPROVALTRAILID,
                 comment = x.COMMENT,
                 targetId = x.TARGETID,
                 arrivalDate = x.ARRIVALDATE,
                 systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                 responseDate = x.RESPONSEDATE,
                 systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                 responseStaffId = x.RESPONSESTAFFID,
                 requestStaffId = x.REQUESTSTAFFID,
                 fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                 fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == x.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                 toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                 toApprovalLevelId = x.TOAPPROVALLEVELID,
                 approvalStateId = x.APPROVALSTATEID,
                 approvalStatusId = x.APPROVALSTATUSID,
                 approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                 approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                 commentStage = commentStage,
                 vote = x.VOTE,
                 toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                 fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
             })?.ToList();


             data.OrderByDescending(d => d.systemArrivalDateTime);
             return data;*/
        }

        private string GetDrawdownApprovalsMarkupLOS()
        {
            var appraisals = GetAppraisalMemorandumTrailDrawdown(this.targetId, GetCurrentOperationId()).OrderBy(a => a.approvalTrailId);
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>APPROVALS:</b></th>
                        <th><b></b></th>
                    </tr>
                    <tr>
                        <th><b>Role</b></th>
                        <th><b>Name</b></th>
                        <th><b>Comment</b></th>
                        <th><b>Date</b></th>
                    </tr>
                    ";
            foreach (var trail in appraisals)
            {
                result = result + $@"
                    <tr>
                        <td>{trail.fromApprovalLevelName.ToUpper()}</td>
                        <td>{trail.fromStaffName}</td>
                        <td>{trail.comment}</td>
                        <td>{trail.systemArrivalDateTime}</td>
                    </tr>
                ";
            }

            result = result + $"</table><br/>";
            return result;

        }


        private string GetDrawdownApprovalsMarkupLOS2(int targetId)
        {

            var appraisals = GetAppraisalMemorandumTrailDrawdownMemoReversal(targetId).OrderByDescending(a => a.systemArrivalDateTime);
            var result = String.Empty;
            result = result + $@"
                <br/>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>APPROVALS:</b></th>
                        <th><b></b></th>
                    </tr>
                    <tr>
                        <th><b>Role</b></th>
                        <th><b>Name</b></th>
                        <th><b>Comment</b></th>
                        <th><b>Decision</b></th>
                       <th><b>Date</b></th>
                    </tr>
                    ";
            foreach (var trail in appraisals)
            {
                result = result + $@"
                    <tr>
                        <td>{trail.fromApprovalLevelName.ToUpper()}</td>
                        <td>{trail.fromStaffName}</td>
                        <td>{trail.comment}</td>
                        <td>{GetDecision(trail.approvalStatusId)}</td>
                        <td>{trail.systemArrivalDateTime}</td>
                    </tr>
                ";
            }

            result = result + $"</table><br/>";
            return result;

        }


        private string GetDecision(short? vote)
        {
            if (vote == 1) return "Accepted";
            if (vote == 2) return "Accepted";
            if (vote == 3) return "Declined";
            if (vote == 4) return "Accepted";
            if (vote == 5) return "Referred";
            return String.Empty;
        }

        private string GetAllCustomerFacilitiesMarkup()
        {
            var result = String.Empty;
            if (this.customerFacilities != null)
            {
                result += $@"
                        <ul>
                        ";
                foreach (var f in this.customerFacilities)
                {
                    result += $@"
                            <li>{f.TBL_PRODUCT.PRODUCTNAME + " " + f.TBL_CURRENCY.CURRENCYCODE + String.Format("{0:0,0.00}", f.APPROVEDAMOUNT)}</li>
                        ";
                }
                result += $@"
                        </ul>
                        ";
            }
            else
            {
                result = result + GetAllCustomerFacilitiesLMSMarkup();
            }
            return result;
        }

        private string GetAllCustomerFacilitiesLMSMarkup()
        {
            var result = String.Empty;
            if (this.customerFacilitiesLms != null)
            {
                result += $@"
                        <ul>
                        ";
                foreach (var f in this.customerFacilitiesLms)
                {
                    result += $@"
                            <li>{f.TBL_PRODUCT.PRODUCTNAME + " " + String.Format("{0:0,0.00}", f.APPROVEDAMOUNT)}</li>
                        ";
                }
                result += $@"
                        </ul>
                        ";
            }
            return result;
        }
        private string GetAllCustomerCollateralsMarkupLMS()
        {
            var result = String.Empty;
            var remark = string.Empty;
            if (this.lmsrApplication != null)
            {
                var customerCollaterals = collateralRepo.GetCustomerCollateral(this.lmsrApplication.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID, this.lmsrApplication.LOANAPPLICATIONID, this.lmsrApplication.COMPANYID);

                result += $@"
                        <ul>
                        ";
                foreach (var cc in customerCollaterals)
                {
                    result += $@"
                            <li>{cc.collateralSummary}</li>
                        ";
                }
                result += $@"
                        </ul>
                        ";
            }
            return result;
        }


        private string GetAllCustomerCollateralsMarkup()
        {
            var result = String.Empty;
            var remark = string.Empty;
            if (this.lmsrApplication == null)
            {
                var customerCollaterals = collateralRepo.GetCustomerCollateral(this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().CUSTOMERID, this.loanApplication.LOANAPPLICATIONID, this.loanApplication.COMPANYID);

                result += $@"
                        <ul>
                        ";
                foreach (var cc in customerCollaterals)
                {
                    result += $@"
                            <li>{cc.collateralSummary}</li>
                        ";
                }
                result += $@"
                        </ul>
                        ";
            }
            else
            {
                result = result + GetAllCustomerCollateralsMarkupLMS();
            }
            return result;
        }

        private string GetSecurityAnalysisMarkUP()
        {
            var result = String.Empty;
            
                result += $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Security / Support</b></th>
                    </tr>
                    <tr>
                    <td>{GetAllCustomerFacilitiesMarkup()}</td>
                    <td>{GetAllCustomerCollateralsMarkup()}</td>
                    
                    </tr>
                    <tr></tr>
                </table>

                
            ";
            
            return result;
        }

        private string GetCollateralCoverageMarkupLOS()
        {
            var custFacilitiesAmount = new decimal();
            var collaterals = collateralRepo.GetProposedCustomerCollateralByCustomerId(this.customerId, true);
            var result = String.Empty;
            if (collaterals.Count() < 1) return result;
            //decimal actualCollateralCoverageSum = 0;
            //actualCollateralCoverageSum = collaterals.Where(c => c.customerId != this.customerId).Sum(c => c.actualCollateralCoverage);
            //custFacilitiesAmount += actualCollateralCoverageSum;
            decimal totalCollateralValue = 0;
            var currencies = context.TBL_CURRENCY.ToList();
            var baseCurrency = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == loanApplication.COMPANYID).CURRENCYID;
            var baseCurrencyCode = currencies.FirstOrDefault(cu => cu.CURRENCYID == baseCurrency).CURRENCYCODE;
            var collateralGroup = collaterals.GroupBy(c => c.loanApplicationDetailId).ToList();
            var collateralGroup2 = collaterals.GroupBy(c => c.collateralId).ToList();
            foreach (var g in collateralGroup)
            {
                var facility = context.TBL_LOAN_APPLICATION_DETAIL.Find(g.Key);
                custFacilitiesAmount += facility.APPROVEDAMOUNT * (decimal)facility.EXCHANGERATE;
            }

            if(custFacilitiesAmount == 0)
            {
                custFacilitiesAmount = 1;
            }
            int n = 0;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=1000px cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>DESCRIPTION/SUMMARY</b></th>
                        <th><b>COLLATERAL VALUE(OMV)</b></th>
                        <th><b>COLLATERAL VALUE(FSV)</b></th>
                    </tr>
                    ";

            foreach (var d in collateralGroup2)
            { ++n;
                var c = d.FirstOrDefault();
                var currCode = currencies.FirstOrDefault(cu => cu.CURRENCYID == c.currencyId).CURRENCYCODE;
                //var rate = financeTransaction.GetExchangeRate(DateTime.Now, (short)c.currencyId, loanApplication.COMPANYID);
                //var baseCollateralValue = c.collateralValue * (decimal)rate.sellingRate;
                totalCollateralValue += c.fsv;
                result = result + $@"
                    <tr>
                        <td>{n}</td>
                        <td>{c.collateralSummary}</td>
                        <td>{baseCurrencyCode + " " + String.Format("{0:0,0.00}", c.omv)}</td>
                        <td>{baseCurrencyCode + " " + String.Format("{0:0,0.00}", c.fsv)}</td>
                    </tr>
                ";
            }
            result = result + $@"
                <tr>
                    <td>&nbsp;</td>
                    <td><b>TOTAL</b></td>
                    <td>&nbsp;</td>
                    <td>{baseCurrencyCode + " " + String.Format("{0:0,0.00}", totalCollateralValue)}</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td><b>TOTAL FACILITY AMOUNT</b></td>
                    <td>&nbsp;</td>
                    <td>{baseCurrencyCode + " " + String.Format("{0:0,0.00}", (custFacilitiesAmount))}</td>
                </tr>
                <tr>
                    <td>&nbsp;</td>
                    <td><b>NET COVERAGE</b></td>
                    <td>&nbsp;</td>
                    <td>{String.Format("{0:0,0.00}", (totalCollateralValue/custFacilitiesAmount) * 100)} %</td>
                </tr>
            ";

            result = result + $"</table>";
            return result;
        }
        
        private string GetCustomerRiskRating()
        {
            var result = String.Empty;
            if (loanApplication.LOANAPPLICATIONTYPEID == (int)LoanTypeEnum.Single)
            {
                var rating = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == customerId).CUSTOMERRATING;
                result += rating;
            }
            else
            {
                var rating = context.TBL_CUSTOMER_GROUP.FirstOrDefault(c => c.CUSTOMERGROUPID == loanApplication.CUSTOMERGROUPID).RISKRATINGID;
                result += rating;
            }
            
            return result;
        }
        //lms
        private string GetDirectFacilitiesMarkupLMS()
        {
            var result = String.Empty;
            foreach (var facility in context.TBL_LMSR_APPLICATION_DETAIL.Where(l => l.CUSTOMERID == lmsrApplication.CUSTOMERID && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != 
            (int)LoanProductTypeEnum.ContingentLiability && l.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN))
            {
                result = result + $@"
                    <tr>
                        <td>{facility.TBL_PRODUCT.PRODUCTNAME}</td>
                        <td>{(100 / 100) * facility.APPROVEDAMOUNT}</td>
                        <td>{facility.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYNAME}</td>
                        <td>{facility.APPROVEDAMOUNT}</td>
                        <td>{facility.PROPOSEDAMOUNT}</td>
                        <td>{facility.PROPOSEDAMOUNT - facility.APPROVEDAMOUNT}</td>
                        <td>{facility.APPROVEDTENOR}</td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetCustomerRiskRatingLMS()
        {
            var result = String.Empty;
            if (lmsrApplication.CUSTOMERID > 0)
            {
                var rating = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == lmsrApplication.CUSTOMERID).CUSTOMERRATING;
                result += rating;
            }
            else
            {
                var rating = context.TBL_CUSTOMER_GROUP.FirstOrDefault(c => c.CUSTOMERGROUPID == lmsrApplication.CUSTOMERGROUPID).RISKRATINGID;
                result += rating;
            }

            return result;
        }

        private string GetTotalDirectFacilitiesMarkupLMS()
        {
            var result = String.Empty;
            var directs = context.TBL_LMSR_APPLICATION_DETAIL.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != 
            (int)LoanProductTypeEnum.ContingentLiability && l.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN);

            result = result + $@"
                <tr>
                    <td>{directs.Count()}</td>
                    <td>{(100 / 100) * directs.Sum(l => l.APPROVEDAMOUNT)}</td>
                    <td>&nbsp;</td>
                    <td>{directs.Sum(l => l.APPROVEDAMOUNT)}</td>
                    <td>{directs.Sum(l => l.PROPOSEDAMOUNT)}</td>
                    <td>{directs.Sum(l => l.PROPOSEDAMOUNT) - directs.Sum(l => l.APPROVEDAMOUNT)}</td>
                    <td>{directs.Sum(l => l.APPROVEDTENOR)}</td>
                </tr>
            ";
            return result;
        }

        private string GetContingentFacilitiesMarkupLMS()
        {
            var result = String.Empty;
            var contingents = context.TBL_LMSR_APPLICATION_DETAIL.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID ==
            (int)LoanProductTypeEnum.ContingentLiability && l.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN);

            foreach (var facility in contingents)
            {
                result = result + $@"
                    <tr>
                        <td>{facility.TBL_PRODUCT.PRODUCTNAME}</td>
                        <td>{(100 / 100) * facility.APPROVEDAMOUNT}</td>
                        <td>{facility.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYNAME}</td>
                        <td>{facility.APPROVEDAMOUNT}</td>
                        <td>{facility.PROPOSEDAMOUNT}</td>
                        <td>{facility.PROPOSEDAMOUNT - facility.APPROVEDAMOUNT}</td>
                        <td>{facility.APPROVEDTENOR}</td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetTotalContingentFacilitiesMarkupLMS()
        {
            var result = String.Empty;
            var contingents = context.TBL_LMSR_APPLICATION_DETAIL.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID ==
            (int)LoanProductTypeEnum.ContingentLiability && l.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN);
            if (contingents.Count() > 0)
            {
                result = result + $@"
                    <tr>
                        <td>{contingents.Count()}</td>
                        <td>{(100 / 100) * contingents.Sum(l => l.APPROVEDAMOUNT)}</td>
                        <td>&nbsp;</td>
                        <td>{contingents.Sum(l => l.APPROVEDAMOUNT)}</td>
                        <td>{contingents.Sum(l => l.PROPOSEDAMOUNT)}</td>
                        <td>{contingents.Sum(l => l.PROPOSEDAMOUNT) - contingents.Sum(l => l.APPROVEDAMOUNT)}</td>
                        <td>{contingents.Sum(l => l.APPROVEDTENOR)}</td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetForeignDirectFacilitiesMarkupLMS()
        {
            var result = String.Empty;
            var foreignDirects = context.TBL_LMSR_APPLICATION_DETAIL.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID !=
            (int)LoanProductTypeEnum.ContingentLiability && l.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN);
            foreach (var facility in foreignDirects)
            {
                result = result + $@"
                    <tr>
                        <td>{facility.TBL_PRODUCT.PRODUCTNAME}</td>
                        <td>{(100 / 100) * facility.APPROVEDAMOUNT}</td>
                        <td>{facility.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYNAME}</td>
                        <td>{facility.APPROVEDAMOUNT}</td>
                        <td>{facility.PROPOSEDAMOUNT}</td>
                        <td>{facility.PROPOSEDAMOUNT - facility.APPROVEDAMOUNT}</td>
                        <td>{facility.APPROVEDTENOR}</td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetTotalForeignDirectFacilitiesMarkupLMS()
        {
            var result = String.Empty;
            var foreignDirects = context.TBL_LMSR_APPLICATION_DETAIL.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID !=
            (int)LoanProductTypeEnum.ContingentLiability && l.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN);

            if (foreignDirects.Count() > 0)
            {
                result = result + $@"
                    <tr>
                        <td>{foreignDirects.Count()}</td>
                        <td>{(100 / 100) * foreignDirects.Sum(l => l.APPROVEDAMOUNT)}</td>
                        <td>&nbsp;</td>
                        <td>{foreignDirects.Sum(l => l.APPROVEDAMOUNT)}</td>
                        <td>{foreignDirects.Sum(l => l.PROPOSEDAMOUNT)}</td>
                        <td>{foreignDirects.Sum(l => l.PROPOSEDAMOUNT) - foreignDirects.Sum(l => l.APPROVEDAMOUNT)}</td>
                        <td>{foreignDirects.Sum(l => l.APPROVEDTENOR)}</td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetForeignContingentFacilitiesMarkupLMS()
        {
            var result = String.Empty;
            var foreignContingents = context.TBL_LMSR_APPLICATION_DETAIL.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID ==
            (int)LoanProductTypeEnum.ContingentLiability && l.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN);
            foreach (var facility in foreignContingents)
            {
                result = result + $@"
                    <tr>
                        <td>{facility.TBL_PRODUCT.PRODUCTNAME}</td>
                        <td>{(100 / 100) * facility.APPROVEDAMOUNT}</td>
                        <td>{facility.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYNAME}</td>
                        <td>{facility.APPROVEDAMOUNT}</td>
                        <td>{facility.PROPOSEDAMOUNT}</td>
                        <td>{facility.PROPOSEDAMOUNT - facility.APPROVEDAMOUNT}</td>
                        <td>{facility.APPROVEDTENOR}</td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetTotalForeignContingentFacilitiesMarkupLMS()
        {
            var result = String.Empty;
            var foreignContingents = context.TBL_LMSR_APPLICATION_DETAIL.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == 
            (int)LoanProductTypeEnum.ContingentLiability && l.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN);
            if (foreignContingents.Count() > 0)
            {
                result = result + $@"
                    <tr>
                        <td>{foreignContingents.Count()}</td>
                        <td>{(100 / 100) * foreignContingents.Sum(l => l.APPROVEDAMOUNT)}</td>
                        <td>&nbsp;</td>
                        <td>{foreignContingents.Sum(l => l.APPROVEDAMOUNT)}</td>
                        <td>{foreignContingents.Sum(l => l.PROPOSEDAMOUNT)}</td>
                        <td>{foreignContingents.Sum(l => l.PROPOSEDAMOUNT) - foreignContingents.Sum(l => l.APPROVEDAMOUNT)}</td>
                        <td>{foreignContingents.Sum(l => l.APPROVEDTENOR)}</td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetGroupExposureMarkupLMS()
        {
            var result = String.Empty;
            var exposures = GetExposuresLMS();
            //var exposures = GetGroupExposurebyCustomerId((int)lmsrApplication.CUSTOMERID, this.lmsrApplication.COMPANYID);
            var directExposures = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
            var loanExposures = directExposures.Where(e => e.adjFacilityTypeId != (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
            var overdraftExposures = directExposures.Where(e => e.adjFacilityTypeId == (int)AdjustedFacilityTypeEnum.OVERDRAFT).ToList();
            var contingents = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && !e.adjFacilityTypeString.Contains("LC") && !e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
            var lcs = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Contingent && e.adjFacilityTypeString.Contains("LC")).ToList();
            var tradeLoans = exposures.Where(e => e.exposureTypeId == (int)ExposureTypeEnum.Direct && e.adjFacilityTypeString.Contains("TRADE LOAN")).ToList();
            var exposureGroupsByCustomer = exposures.GroupBy(e => e.customerCode);
            var n = 0;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=1000px align=center cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>Related Obligors(domestic)</b></th>
                        <th><b>Facility Name</b></th>
                        <th><b>LLL Impact (NGN)</b></th>
                        <th><b>Currency</b></th>
                        <th><b>Approved Amount</b></th>
                        <th><b>Outstanding Exposure</b></th>
                        <th><b>[O/S] Ccy</b></th>
                        <th><b>End Date</b></th>
                    </tr>
                ";
            if (loanExposures.Count > 0 || overdraftExposures.Count > 0)
            {
                result = result + $@"<tr><td>Direct Facilities:</td></tr>";
                if (loanExposures.Count > 0)
                {
                    var customers = loanExposures.GroupBy(e => e.customerCode);
                    foreach (var cust in customers)
                    {
                        var directsGroup = cust.GroupBy(f => f.productCode.Trim());

                        foreach (var product in directsGroup)
                        {
                            var facility = product.FirstOrDefault().facilityType;
                            var currency = "Naira";
                            //var currency = product.currency;
                            var currentAmount = product.Sum(p => p.outstandings);
                            var approvedAmount = product.Sum(p => p.approvedAmount);
                            var currentAmountForLLL = product.Sum(p => p.outstandingsLcy);
                            var approvedAmountForLLL = product.Sum(p => p.approvedAmountLcy);
                            var amountForLLL = currentAmountForLLL;
                            var LLLImpact = amountForLLL;

                            result = result + $@"
                     <tr>
                        <td>{product.FirstOrDefault().customerName}</td>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", approvedAmountForLLL)}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmountForLLL)}</td>
                        <td>{currency}</td>
                        <td>{product.Max(p => p.maturityDate)}</td>
                    </tr>
                    <tr><td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td><tr>
                    ";
                        }
                    }
                }

                if (overdraftExposures.Count > 0)
                {
                    var customers = overdraftExposures.GroupBy(e => e.customerCode);
                    foreach (var cust in customers)
                    {
                        var overdraftsGroup = cust.GroupBy(f => f.productCode.Trim());

                        foreach (var product in overdraftsGroup)
                        {
                            var facility = product.FirstOrDefault().facilityType;
                            var currency = "Naira";
                            var currentAmount = product.Sum(p => p.outstandings);
                            var approvedAmount = product.Sum(p => p.approvedAmount);
                            var currentAmountForLLL = product.Sum(p => p.outstandingsLcy);
                            var approvedAmountForLLL = product.Sum(p => p.approvedAmountLcy);
                            var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                            var LLLImpact = amountForLLL;

                            result = result + $@"
                     <tr>
                        <td>{product.FirstOrDefault().customerName}</td>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", approvedAmountForLLL)}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmountForLLL)}</td>
                        <td>{currency}</td>
                        <td>{product.Max(p => p.maturityDate)}</td>
                    </tr>
                    ";
                        }
                    }
                }

            }

            if (contingents.Count() > 0)
            {
                result = result + $@"<tr><td>Contingent Facilities:</td></tr>";
                var customers = contingents.GroupBy(e => e.customerCode);
                foreach (var cust in customers)
                {
                    var contingentsGroup = cust.GroupBy(f => f.productCode.Trim());
                    foreach (var product in contingentsGroup)
                    {
                        var facility = product.FirstOrDefault().facilityType;
                        var currency = "Naira";
                        var currentAmount = product.Sum(p => p.outstandings);
                        var approvedAmount = product.Sum(p => p.approvedAmount);
                        var currentAmountForLLL = product.Sum(p => p.outstandingsLcy);
                        var approvedAmountForLLL = product.Sum(p => p.approvedAmountLcy);
                        var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                        var LLLImpact = (amountForLLL / 3);
                        //var tenor = curr.Sum(p => p.APPROVEDTENOR);

                        result = result + $@"
                     <tr>
                        <td>{product.FirstOrDefault().customerName}</td>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", approvedAmountForLLL)}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmountForLLL)}</td>
                        <td>{currency}</td>
                        <td>{product.Max(p => p.maturityDate)}</td>
                    </tr>
                    ";
                    }

                    //foreach (var product in cust)
                    //{
                    //    var facility = product.facilityType;
                    //    var currency = product.currency;
                    //    var currentAmount = product.outstandings;
                    //    var approvedAmount = product.approvedAmount;
                    //    var currentAmountForLLL = product.outstandingsLcy;
                    //    var approvedAmountForLLL = product.approvedAmountLcy;
                    //    var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                    //    var LLLImpact = (amountForLLL / 3);

                    //    result = result + $@"
                    // <tr>
                    //    <td>{product.customerName}</td>
                    //    <td>{facility}</td>
                    //    <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                    //    <td>{currency}</td>
                    //    <td>{String.Format("{0:0,0.00}", approvedAmount)}</td>
                    //    <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                    //    <td>{currency}</td>
                    //    <td>{product.maturityDate}</td>
                    //</tr>
                    //";
                    //}
                }
            }

            if (tradeLoans.Count() > 0 || lcs.Count() > 0)
            {
                result = result + $@"<tr><td>(Import Finance Facilities)</td></tr>";
                if (lcs.Count() > 0)
                {
                    result = result + $@"<tr><td>(Contingent)</td></tr>";
                    var customers = lcs.GroupBy(e => e.customerCode);
                    foreach (var cust in customers)
                    {
                        var contingentsGroup = cust.GroupBy(f => f.productCode.Trim());
                        foreach (var product in contingentsGroup)
                        {
                            var facility = product.FirstOrDefault().facilityType;
                            var currency = "Naira";
                            var currentAmount = product.Sum(p => p.outstandings);
                            var approvedAmount = product.Sum(p => p.approvedAmount);
                            var currentAmountForLLL = product.Sum(p => p.outstandingsLcy);
                            var approvedAmountForLLL = product.Sum(p => p.approvedAmountLcy);
                            var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                            var LLLImpact = amountForLLL;
                            //var tenor = curr.Sum(p => p.APPROVEDTENOR);

                            result = result + $@"
                             <tr>
                                <td>{product.FirstOrDefault().customerName}</td>
                                <td>{facility}</td>
                                <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                                <td>{currency}</td>
                                <td>{String.Format("{0:0,0.00}", approvedAmountForLLL)}</td>
                                <td>{String.Format("{0:0,0.00}", currentAmountForLLL)}</td>
                                <td>{currency}</td>
                                <td>{product.Max(p => p.maturityDate)}</td>
                            </tr>
                            ";
                        }

                        //foreach (var product in cust)
                        //{
                        //    var facility = product.facilityType;
                        //    var currency = product.currency;
                        //    var currentAmount = product.outstandings;
                        //    var approvedAmount = product.approvedAmount;
                        //    var currentAmountForLLL = product.outstandingsLcy;
                        //    var approvedAmountForLLL = product.approvedAmountLcy;
                        //    var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                        //    var LLLImpact = amountForLLL;
                        //    //var LLLImpact = (amountForLLL / 3);
                        //    //var tenor = curr.Sum(p => p.APPROVEDTENOR);

                        //    result = result + $@"
                        //     <tr>
                        //        <td>{product.customerName}</td>
                        //        <td>{facility}</td>
                        //        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        //        <td>{currency}</td>
                        //        <td>{String.Format("{0:0,0.00}", approvedAmount)}</td>
                        //        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        //        <td>{currency}</td>
                        //        <td>{product.maturityDate}</td>
                        //    </tr>
                        //    ";
                        //}
                    }
                }

                if (tradeLoans.Count() > 0)
                {
                    result = result + $@"<tr><td>(Direct)</td></tr>";
                    var customers = tradeLoans.GroupBy(e => e.customerCode);
                    foreach (var cust in customers)
                    {
                        var directsGroup = cust.GroupBy(f => f.productCode.Trim());
                        foreach (var product in directsGroup)
                        {
                            var facility = product.FirstOrDefault().facilityType;
                            var currency = "Naira";
                            var currentAmount = product.Sum(p => p.outstandings);
                            var approvedAmount = product.Sum(p => p.approvedAmount);
                            var currentAmountForLLL = product.Sum(p => p.outstandingsLcy);
                            var approvedAmountForLLL = product.Sum(p => p.approvedAmountLcy);
                            var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                            var LLLImpact = amountForLLL;
                            //var tenor = curr.Sum(p => p.APPROVEDTENOR);

                            result = result + $@"
                             <tr>
                                <td>{product.FirstOrDefault().customerName}</td>
                                <td>{facility}</td>
                                <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                                <td>{currency}</td>
                                <td>{String.Format("{0:0,0.00}", approvedAmountForLLL)}</td>
                                <td>{String.Format("{0:0,0.00}", currentAmountForLLL)}</td>
                                <td>{currency}</td>
                                <td>{product.Max(p => p.maturityDate)}</td>
                            </tr>
                            ";
                        }

                        //foreach (var product in cust)
                        //{
                        //    var facility = product.facilityType;
                        //    var currency = product.currency;
                        //    var currentAmount = product.outstandings;
                        //    var approvedAmount = product.approvedAmount;
                        //    var currentAmountForLLL = product.outstandingsLcy;
                        //    var approvedAmountForLLL = product.approvedAmountLcy;
                        //    var amountForLLL = (currentAmountForLLL >= approvedAmountForLLL) ? currentAmountForLLL : approvedAmountForLLL;
                        //    var LLLImpact = amountForLLL;
                        //    //var tenor = curr.Sum(p => p.APPROVEDTENOR);

                        //    result = result + $@"
                        //     <tr>
                        //        <td>{product.customerName}</td>
                        //        <td>{facility}</td>
                        //        <td>{String.Format("{0:0,0.00}", LLLImpact)}</td>
                        //        <td>{currency}</td>
                        //        <td>{String.Format("{0:0,0.00}", approvedAmount)}</td>
                        //        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        //        <td>{currency}</td>
                        //        <td>{product.maturityDate}</td>
                        //    </tr>
                        //    ";
                        //}
                    }
                }
            }
            //foreach (var customerGroups in exposureGroupsByCustomer)
            //{
            //    var customerName = customerGroups.Key;
            //    var facilities = customerGroups.GroupBy(c => c.facilityType.Trim());
            //    foreach (var facility in facilities)
            //    {
            //        ++n;
            //        result = result + $@"
            //         <tr>
            //            <td>{customerName}</td>
            //            <td>{facility.Key}</td>
            //            <td>{facility.FirstOrDefault()?.currency}</td>
            //            <td>{String.Format("{0:0,0.00}", facility.Sum(f => f.approvedAmount))}</td>
            //            <td>{String.Format("{0:0,0.00}", facility.Sum(f => f.outstandings))}</td>
            //            <td>{facility.Max(f => f.bookingDate).ToShortDateString()}</td>
            //            <td>{facility.Max(f => f.maturityDate).ToShortDateString()}</td>
            //        </tr>
            //    ";
            //    }
            //}
            result = result + $@"
                    {GetTotalGroupExposureMarkup()}
                ";
            result = result + $"</table>";
            return result;
        }

        // execute 
        public string Replace(string content) // placeholders replace
        {
            content = content.Replace(customerNameHolder, customerName);
            content = content.Replace(branchNameHolder, branchName);
            content = content.Replace(companyLogoHolder, companyLogo);
            content = content.Replace(customerExposureHolder, customerExposure);
            content = content.Replace(recommendedInterestRateHolder, recommendedInterestRate);
            content = content.Replace(isRelatedPartyHolder, isRelatedParty);
            content = content.Replace(dateCreatedHolder, dateCreated);
            content = content.Replace(locationNameHolder, locationName);
            content = content.Replace(managementProfileHolder, managementProfile);
            content = content.Replace(ownershipHolder, ownership);
            content = content.Replace(approvalLevelHolder, approvalLevel);
            content = content.Replace(accountNumbersHolder, accountNumbers);
            content = content.Replace(proposedConditionsHolder, proposedConditions);
            content = content.Replace(conditionsPrecedenceListHolder, conditionsPrecedenceList);
            content = content.Replace(dynamicsListHolder, dynamicsList);
            content = content.Replace(conditionsPrecedentToDrawdownHolder, conditionsPrecedentToDrawdown);
            content = content.Replace(transactionsDynamicsHolder, transactionsDynamics);
            content = content.Replace(monitoringTriggersHolder, monitoringTriggers);
            content = content.Replace(environmentalSocialRiskHolder, environmentalSocialRisk);
            //content = content.Replace(environmentalAndSocialSummaryHolder, environmentalAndSocialSummary);
            content = content.Replace(rmCountryHolder, rmCountry);
            content = content.Replace(misCodeHolder, misCode);
            content = content.Replace(reviewTypeHolder, reviewType);
            content = content.Replace(preparedByHolder, preparedBy);
            content = content.Replace(businessSectorsHolder, businessSectors);
            content = content.Replace(exchangeRateHolder, exchangeRate);
            content = content.Replace(groupFacilitySummaryHolder, groupFacilitySummary);
            content = content.Replace(mccStampHolder, mccDigitalStamp);
            content = content.Replace(bccStampHolder, bccDigitalStamp);
            //content = content.Replace(groupFacilitySummaryFcyHolder, groupFacilitySummaryFcy);
            //content = content.Replace(directFacilitiesHolder, directFacilities);
            //content = content.Replace(totalDirectsHolder, totalDirectFacilities);
            //content = content.Replace(contingentFacilitiesHolder, contingentFacilities);
            //content = content.Replace(totalContingentsHolder, totalContingentFacilities);
            //content = content.Replace(importFinanceFacilitiesHolder, importFinanceFacilities);
            //content = content.Replace(totalImportFinanceFacilitiesHolder, totalImportFinanceFacilities);
            //content = content.Replace(foreignDirectFacilitiesHolder, foreignDirectFacilities);
            //content = content.Replace(totalForeignDirectsHolder, totalForeignDirectFacilities);
            //content = content.Replace(foreignContingentFacilitiesHolder, foreignContingentFacilities);
            //content = content.Replace(totalForeignContingentsHolder, totalForeignContingentFacilities);
            //content = content.Replace(foreignImportFinanceFacilitiesHolder, foreignImportFinanceFacilities);
            //content = content.Replace(totalForeignImportFinanceFacilitiesHolder, totalForeignImportFinanceFacilities);
            //content = content.Replace(totalFacilitiesHolder, totalFacilities);
            content = content.Replace(groupExposureHolder, groupExposure);
            content = content.Replace(approvalsHolder, approvals);
            content = content.Replace(currentDateHolder, currentDate);
            content = content.Replace(annualReviewDateHolder, annualReviewDate);
            content = content.Replace(securityAnalysisHolder, securityAnalysis);
            content = content.Replace(allCustomerCollateralRemarksHolder, allCustomerCollateralRemarks);
            content = content.Replace(collateralCoverageHolder, collateralCoverage);
            content = content.Replace(allCustomerFacilitiesHolder, allCustomerFacilities);
            content = content.Replace(obligorRiskRatingHolder, obligorRiskRating);
            content = content.Replace(obligorClassificationHolder, obligorClassification);
            //content = content.Replace(totalGroupExposureHolder, totalGroupExposure);

            if (content.Contains(customerTurnoverHolder))
            {
                customerTurnover = CustomerTurnoverMarkup();
                content = content.Replace(customerTurnoverHolder, customerTurnover);
            }

            // lms cam only
            content = content.Replace(securityTypeHolder, securityType);
            content = content.Replace(securityDescriptionHolder, securityDescription);
            content = content.Replace(securityFirstSellValueHolder, securityFirstSellValue);
            content = content.Replace(securityLocationHolder, securityLocation);
            content = content.Replace(securityOpenMarketValueHolder, securityOpenMarketValue);
            content = content.Replace(securityPerfectionStatusHolder, securityPerfectionStatus);
            content = content.Replace(securityValuationDateHolder, securityValuationDate);
            content = content.Replace(shareHoldersHolder, shareHolders);
            content = content.Replace(signitoriesHolder, signitories);
            content = content.Replace(directorsHolder, directors);
            content = content.Replace(isSecurityHolder, isSecurity);
            content = content.Replace(isOwnerOccupiedHolder, isOwnerOccupied);
            content = content.Replace(amountDisbursedHolder, amountDisbursed);
            content = content.Replace(amountPaidSoFarHolder, amountPaidSoFar);
            content = content.Replace(amountProposedHolder, amountProposed);

            // for output document          
            content = content.Replace(memoHolder, memoData);
            content = content.Replace(exceptionMemoHolder, exceptionMemoData);
            content = content.Replace(facilityUpgradeSupportSchemeHolder, facilityUpgradeSupportSchemeData);
            content = content.Replace(invoiceDiscountingDataHolder, invoiceDiscountingData);
            content = content.Replace(cashCollaterizedDataHolder, cashCollaterizedData);
            content = content.Replace(staffCarLoansDataHolder, staffcarLoansData);
            content = content.Replace(staffMortgageLoansDataHolder, staffMortgageLoansData);
            content = content.Replace(staffPersonalLoansAGMDataHolder, staffPersonalLoansAGMData);
            content = content.Replace(staffPersonalLoanDataHolder, staffPersonalLoanData);
            content = content.Replace(temporaryOverdraftHolder, temporaryOverdraftData); 
            content = content.Replace(documentatonDeferralWaiverDataHolder, documentatonDeferralWaiverData);
            //content = content.Replace(OfferLetterForBondsAndGuaranteesDataHolder, OfferLetterForBondsAndGuaranteesData);

            // for FUSS document          
            content = content.Replace(fussCustomerInformationHolder, fussCustomerInformationData);
            content = content.Replace(fussSchoolFeesInformationHolder, fussSchoolFeesInformationData);
            content = content.Replace(fussCustomerFacilityHolder, fussCustomerFacilityData);
            content = content.Replace(fussCustomerAccountActivityHolder, fussCustomerAccountActivityData);
            content = content.Replace(fussCustomerAccountActivitySummaryHolder, fussCustomerAccountActivitySummaryData);
            content = content.Replace(fussCashFlowAnalysisHolder, fussCashFlowAnalysisData);
            content = content.Replace(fussSummaryNetCashFlowHolder, fussSummaryNetCashFlowData);
            content = content.Replace(fussCurrentRequestHolder, fussCurrentRequestData);
            content = content.Replace(fussBackgroungInformationHolder, fussBackgroungInformationData);
            content = content.Replace(fussChecklistEligibilitytHolder, fussChecklistEligibilityData);
            content = content.Replace(fussCustomerConditionSubsequentHolder, fussCustomerConditionSubsequentData);
            content = content.Replace(fussCustomerConditionDynamicsHolder, fussCustomerConditionDynamicsData);

            // for IDF document          
            content = content.Replace(idfCustomerInformationHolder, idfCustomerInformationData);
            content = content.Replace(idfCustomerFacilityHolder, idfCustomerFacilityData);
            content = content.Replace(idfCustomerAccountActivityHolder, idfCustomerAccountActivityData);
            content = content.Replace(idfCurrentRequestHolder, idfCurrentRequestData);
            content = content.Replace(idfBackgroungInformationHolder, idfBackgroungInformationData);
            content = content.Replace(idfChecklistEligibilitytHolder, idfChecklistEligibilityData);
            content = content.Replace(idfDocumentationChecklistHolder, idfDocumentationChecklistData);

            // for CASH COLLATERIZED document          
            content = content.Replace(cashCollaterizedCustomerInformationHolder, cashCollaterizedCustomerInformationData);
            content = content.Replace(cashCollaterizedCustomerFacilityHolder, cashCollaterizedCustomerFacilityData);
            content = content.Replace(cashCollaterizedCustomerAccountActivityHolder, cashCollaterizedCustomerAccountActivityData);
            content = content.Replace(cashCollaterizedCurrentRequestHolder, cashCollaterizedCurrentRequestData);
            content = content.Replace(cashCollaterizedBackgroungInformationHolder, cashCollaterizedBackgroungInformationData);
            content = content.Replace(cashCollaterizedChecklistEligibilitytHolder, cashCollaterizedChecklistEligibilityData);
            content = content.Replace(cashCollaterizedDocumentationChecklistHolder, cashCollaterizedDocumentationChecklistData);

            //for TOD document
            content = content.Replace(todHeaderHolder, todHeaderData);
            content = content.Replace(todCustomerInformationHolder, todCustomerInformationData);
            content = content.Replace(todCustomerAccountActivityHolder, todCustomerAccountActivityData);
            content = content.Replace(todCustomerFacilityHolder, todCustomerFacilityData); 
             content = content.Replace(todCurrentRequestHolder, todCurrentRequestData);
            content = content.Replace(todBackgroungInformationHolder, todBackgroungInformationData);
            content = content.Replace(currentLMSFlowHolder, currentLMSFlowData);

            content = content.Replace(originalDocumentNonCreditProgramHolder, originalDocumentNonCreditProgramData);
            content = content.Replace(originalDocumentCreditProgramHolder, originalDocumentCreditProgramData);

            content = content.Replace(recoveryAnalysisHolder, recoveryAnalysisData);
            content = content.Replace(recoveryAnalysisFirmNameHolder, recoveryAnalysisFirmNameData);
            content = content.Replace(recoveryAnalysisAddressHolder, recoveryAnalysisAddressData);
            content = content.Replace(recoveryAnalysisDateHolder, recoveryAnalysisDateData);

            return content;
        }

        // support methods // interface getter

        public List<CurrentCustomerExposure> GetCustomerExposure(List<CustomerExposure> customerIds, int companyId) // not used!
        {
            return expRepo.GetCurrentCustomerExposure(customerIds, loanApplication.LOANAPPLICATIONTYPEID, companyId); // old ify impl
        }

        public List<CurrentCustomerExposure> GetCustomerExposureLMS(List<CustomerExposure> customerIds, int companyId) // not used!
        {
            int loanType;
            if (lmsrApplication.CUSTOMERID > 0)
            {
                loanType = (int)LoanTypeEnum.Single;
            }
            else
            {
                loanType = (int)LoanTypeEnum.CustomerGroup;
            }
            return expRepo.GetCurrentCustomerExposure(customerIds, loanType, companyId); // old ify impl
        }

        // html markup

        private string CustomerExposureMarkup()
        {
            //if (this.loanApplication.LOANAPPLICATIONTYPEID != (int)LoanTypeEnum.Single)
            //{
                return null;
            //}
            //// var exposures = GetCustomerExposure(customerIds, companyId); // old maurer impl
            //var exposures = GetCurrentSingleCustomerExposures(); // new

            //var result = String.Empty;
            //var n = 0;
            //result = result + $@"
            //    <table style='font face: arial; size:12px' border=1>
            //        <tr>
            //            <th><b>S/N</b></th>
            //            <th><b>Facility Type</b></th>
            //            <th><b>Existing Limit</b></th>
            //            <th><b>Proposed Limit</b></th>
            //            <th><b>Change</b></th>
            //            <th><b>Outstandings</b></th>
            //            <th><b>Past Due Obligations Principal</b></th>
            //            <th><b>Past Due Obligations Interest</b></th>
            //            <th><b>Review Date</b></th>
            //        </tr>
            //     ";
            //foreach (var e in exposures)
            //{
            //    n++;
            //    result = result + $@"
            //        <tr>
            //            <td>{n}</td>
            //            <td>{e.facilityType}</td>
            //            <td>{String.Format("{0:n}", e.existingLimit)}</td>
            //            <td>{String.Format("{0:n}", e.proposedLimit)}</td>
            //            <td>{String.Format("{0:n}", e.change)}</td>
            //            <td>{String.Format("{0:n}", e.outstandings)}</td>
            //            <td>{String.Format("{0:n}", e.pastDueObligationsPrincipal)}</td>
            //            <td>{String.Format("{0:n}", e.PastDueObligationsInterest)}</td>
            //            <td>{e.reviewDate.ToShortDateString()}</td>
            //        </tr>
            //    ";
            //}
            //result = result + $"</table>";
            //return result;

            /*
            int number = 1234567890;
            Convert.ToDecimal(number).ToString("#,##0.00");

            You will get the result 1,234,567,890.00.
            */
        }

        // account numbers
        public List<String> GetAccountNumbers(List<int> customerIds)
        {
            return context.TBL_CASA.Where(x => customerIds.Contains(x.CUSTOMERID)).Select(x => x.PRODUCTACCOUNTNUMBER).ToList();
        }

        private string AccountNumbersMarkup(List<int> customerIds)
        {
            var list = GetAccountNumbers(customerIds);
            return string.Join(",", list);
        }

        // approval level
        public string GetApprovalLevel()
        {
            string levelName = "N/A";
            var trail = context.TBL_APPROVAL_TRAIL.FirstOrDefault(x => x.OPERATIONID == operationId
                && x.TARGETID == targetId
                && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
            );
            if (trail != null) levelName = trail.TBL_APPROVAL_LEVEL.LEVELNAME;
            return levelName;
        }

        // monitoring triggers
        public IEnumerable<MonitoringTriggersViewModel> GetMonitoringTriggers()
        {
            if (operationId == (int)OperationsEnum.CreditAppraisal) return GetApplicationMonitoringTriggers(targetId);
            return GetApplicationMonitoringTriggersLms(targetId);
        }

        private string MonitoringTriggersMarkup()
        {
            var result = String.Empty;
            var triggers = GetMonitoringTriggers();

            var n = 0;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>Facility</b></th>
                        <th><b>Monitoring Trigger</b></th>
                    </tr>
                 ";
            foreach (var t in triggers)
            {
                n++;
                result = result + $@"
                    <tr>
                        <td>{n}</td>
                        <td>{t.productCustomerName}</td>
                        <td>{t.monitoringTrigger}</td>
                    </tr>
                ";
            }
            result = result + $"</table>";
            return result;
        }

        // Environmental & Social Risk Assessment

        /*public IEnumerable<ESGChecklistSummaryViewModel> GetEnvironmentalSocialRisk()
        {
            return context.TBL_ESG_CHECKLIST_SUMMARY
                .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == this.targetId) 
                , s => s.LOANAPPLICATIONDETAILID, d => d.LOANAPPLICATIONDETAILID, (s, d) => new { s, d }).Where(x => x.s.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist)
                .Select(x => new ESGChecklistSummaryViewModel
                {
                    loanApplicationDetailId = x.s.LOANAPPLICATIONDETAILID,
                    comment = x.s.COMMENT_,
                    ratingId = x.s.RATINGID,
                    productCustomerName = x.d.TBL_PRODUCT.PRODUCTNAME + " -- " + x.d.TBL_CUSTOMER.FIRSTNAME + " " + x.d.TBL_CUSTOMER.MIDDLENAME + " " + x.d.TBL_CUSTOMER.LASTNAME
                }).ToList();
        }*/

        public IEnumerable<ESGChecklistSummaryViewModel> GetEnvironmentalSocialRisk()
        {
            var loanDetails = context.TBL_LOAN_APPLICATION_DETAIL
                .Where(x => x.LOANAPPLICATIONID == this.targetId)
                .Select(d => new { d.LOANAPPLICATIONDETAILID, d.TBL_PRODUCT.PRODUCTNAME, d.TBL_CUSTOMER.FIRSTNAME, d.TBL_CUSTOMER.MIDDLENAME, d.TBL_CUSTOMER.LASTNAME });

            var summaries = context.TBL_ESG_CHECKLIST_SUMMARY
                .Where(s => s.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist)
                .Join(loanDetails,
                      s => s.LOANAPPLICATIONDETAILID,
                      d => d.LOANAPPLICATIONDETAILID,
                      (s, d) => new { s.LOANAPPLICATIONDETAILID, s.COMMENT_, s.RATINGID, d.PRODUCTNAME, d.FIRSTNAME, d.MIDDLENAME, d.LASTNAME })
                .AsEnumerable() // Brings data to memory for further operations
                .Select(x => new ESGChecklistSummaryViewModel
                {
                    loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                    comment = x.COMMENT_,
                    ratingId = x.RATINGID,
                    productCustomerName = String.Concat(x.PRODUCTNAME, " -- ", x.FIRSTNAME, " ", x.MIDDLENAME, " ", x.LASTNAME)
                }).ToList();

            return summaries;
        }


        public IEnumerable<ESGChecklistSummaryViewModel> GetGreenLoanIdentificationDetails()
        {
            var greenDetails = (from g in context.TBL_ESG_CHECKLIST_DETAIL
                               join d in context.TBL_ESG_CHECKLIST_DEFINITION on g.ESGCHECKLISTDEFINITIONID equals d.ESGCHECKLISTDEFINITIONID
                               join yes in context.TBL_ESG_CHECKLIST_SCORES on d.YESCHECKLISTSCORESID equals yes.CHECKLISTSCORESID into yesscore
                               join no in context.TBL_ESG_CHECKLIST_SCORES on d.NOCHECKLISTSCORESID equals no.CHECKLISTSCORESID into noscore
                               let ys = yesscore.Any(ye => ye.SCORE == g.CHECKLISTSTATUSID)
                               from y in yesscore.DefaultIfEmpty()
                               from n in noscore.DefaultIfEmpty()
                               where g.CHECKLIST_TYPEID == (int)CheckListTypeEnum.GreenRating
                               && g.LOANAPPLICATIONDETAILID == this.targetId
                               && g.DELETED == false
                               && d.DELETED == false
                               select new ESGChecklistSummaryViewModel
                               {
                                   loanApplicationId = g.LOANAPPLICATIONDETAILID,
                                   grade = (ys) ? y.GRADE : n.GRADE,
                                   score = (ys) ? y.CHECKLISTSCORESID : n.CHECKLISTSCORESID,
                                   ratingId = g.CHECKLISTSTATUSID
                               }).ToList();
            return greenDetails;
        }

        public IEnumerable<ESGChecklistSummaryViewModel> GetGreenRatingSummary()
        {
            return context.TBL_ESG_CHECKLIST_SUMMARY
                .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == this.targetId)
                , s => s.LOANAPPLICATIONDETAILID, d => d.LOANAPPLICATIONID, (s, d) => new { s, d }).Where(x => x.s.CHECKLIST_TYPEID == (int)CheckListTypeEnum.GreenRating)
                .Select(x => new ESGChecklistSummaryViewModel
                {
                    loanApplicationDetailId = x.s.LOANAPPLICATIONDETAILID,
                    comment = x.s.COMMENT_,
                    ratingId = x.s.RATINGID,
                    productCustomerName = x.d.TBL_PRODUCT.PRODUCTNAME + " -- " + x.d.TBL_CUSTOMER.FIRSTNAME + " " + x.d.TBL_CUSTOMER.MIDDLENAME + " " + x.d.TBL_CUSTOMER.LASTNAME
                }).ToList();
        }

        private string GetGreenRatingSummaryMarkup() // TODO RATINGIS
        {
            var result = String.Empty;
            var summary = GetGreenRatingSummary().FirstOrDefault();

            var n = 0;
            result = result + $@"{ summary?.comment }";
            return result;
        }

        private string GetGreenRatingDetailMarkup()
        {
            var result = String.Empty;
            var greenDetails = GetGreenLoanIdentificationDetails().Where(g => g.ratingId != 6);
            var greenSummary = greenDetails.GroupBy(d => d.score).Select(d => d.FirstOrDefault()).ToList();
            result += $@"
                        <ul>
                        ";
            foreach (var s in greenSummary)
            {
                result += $@"<li>{ s.grade }</li>";
            }
            result += $@"
                        </ul>
                        ";
            return result;
        }

        private string GetEnvironmentalSocialRiskMarkup() // TODO RATINGIS
        {
            var result = String.Empty;
            var summary = GetEnvironmentalSocialRisk().FirstOrDefault();

            var n = 0;
            //result = result + $@"
            //    <table style='font face: arial; size:12px' border=1>
            //        <tr>
            //            <th><b>S/N</b></th>
            //            <th><b>Facility</b></th>
            //            <th><b>Summary</b></th>
            //            <th><b>Rating</b></th>
            //        </tr>
            //     ";
            //foreach (var s in summary)
            //{
            //    n++;
            //    result = result + $@"
            //        <tr>
            //            <td>{n}</td>
            //            <td>{s.productCustomerName}</td>
            //            <td>{s.comment}</td>
            //            <td>{GetESGRating(s.ratingId)}</td>
            //        </tr>
            //    ";
            //}
            //result = result + $"</table>";
            result = result + $@"{ GetESGRating(summary?.ratingId)}";
            return result;
        }
       
        private string GetESGRating(int? ratingId)
        {
            if (ratingId == null) return "N/A";
            if (ratingId == 1) return "A";
            if (ratingId == 5) return "B";
            if (ratingId == 6) return "C";
            if (ratingId == 7) return "Low";
            if (ratingId == 8) return "Medium";
            if (ratingId == 9) return "High";
            return "N/A";
        }

        //private string GetEnvironmentalAndSocialSummaryMarkUp()
        //{
        //    var summary = GetEnvironmentalSocialRisk();
        //    return;
        //}

        // Customer exposure

        public List<CurrentCustomerExposure> GetCurrentSingleCustomerExposures()
        {
            //List<CustomerProduct> details = new List<CustomerProduct>();
            //IEnumerable<CurrentCustomerExposure> exposure = null;
            ////IQueryable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();

            //if (operationId == (int)OperationsEnum.CreditAppraisal)
            //    details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerProduct { CUSTOMERID = x.CUSTOMERID, PRODUCTID = x.APPROVEDPRODUCTID }).ToList();
            //else
            //    details = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerProduct { CUSTOMERID = x.CUSTOMERID, PRODUCTID = x.PRODUCTID }).ToList();

            
            //var customerCode = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == loanApplication.CUSTOMERID).CUSTOMERCODE.Trim();
            ////var customCode = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == item.customerId).Select(x => x.CUSTOMERCODE).FirstOrDefault();

            //exposure = (from a in context.TBL_GLOBAL_EXPOSURE
            //            where a.CUSTOMERID.Contains(customerCode)
            //            select new CurrentCustomerExposure
            //            {
            //                customerName = a.CUSTOMERNAME,
            //                customerCode = a.CUSTOMERID.Trim(),
            //                facilityType = a.ADJFACILITYTYPE,
            //                approvedAmount = a.LOANAMOUNYTCY ?? 0,
            //                approvedAmountLcy = a.LOANAMOUNYLCY ?? 0,
            //                currency = a.CURRENCYNAME,
            //                exposureTypeCodeString = a.EXPOSURETYPECODE,
            //                adjFacilityTypeString = a.ADJFACILITYTYPE,
            //                adjFacilityTypeCode = a.ADJFACILITYTYPEid,
            //                productCode = a.PRODUCTCODE,
            //                productIdString = a.PRODUCTID,
            //                productName = a.PRODUCTNAME,
            //                tenorString = a.TENOR,
            //                //existingLimit = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
            //                //proposedLimit = a.LOANAMOUNYLCY ?? 0,
            //                outstandings = a.PRINCIPALOUTSTANDINGBALTCY ?? 0,
            //                outstandingsLcy = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
            //                pastDueObligationsPrincipal = a.TOTALUNPAIDOBLIGATION ?? 0,
            //                reviewDate = DateTime.Now,
            //                bookingDate = a.BOOKINGDATE ,
            //                //maturityDateString = a.MATURITYDATE,
            //                maturityDate = a.MATURITYDATE,
            //                loanStatus = a.CBNCLASSIFICATION,
            //                referenceNumber = a.REFERENCENUMBER,
            //            }).ToList();

            //if (exposure.Count() > 0)
            //{
            //    foreach(var e in exposure)
            //    {
            //        e.exposureTypeId = int.Parse(String.IsNullOrEmpty(e.exposureTypeCodeString) ? "0" : e.exposureTypeCodeString);
            //        e.tenor = int.Parse(String.IsNullOrEmpty(e.tenorString) ? "0" : e.tenorString);
            //        //e.productId = int.Parse(e.productIdString);
            //        e.exposureTypeCode = int.Parse(String.IsNullOrEmpty(e.exposureTypeCodeString) ? "0" : e.exposureTypeCodeString);
            //        e.adjFacilityTypeId = int.Parse(String.IsNullOrEmpty(e.adjFacilityTypeCode) ? "0" : e.adjFacilityTypeCode);
            //    }
            //    exposures.AddRange(exposure);
            //}
            //foreach (var detail in details)
            //{
            //    exposure = context.TBL_LOAN
            //        .Where(x => x.CUSTOMERID == detail.CUSTOMERID && x.PRODUCTID == detail.PRODUCTID && x.LOANSTATUSID == (int)LoanStatusEnum.Active)
            //        .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
            //        .Select(g => new CurrentCustomerExposure
            //        {
            //            facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
            //            existingLimit = g.Sum(x => x.PRINCIPALAMOUNT),
            //            proposedLimit = g.Sum(x => x.OUTSTANDINGPRINCIPAL),
            //            recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
            //            PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
            //            pastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
            //            reviewDate = DateTime.Now,
            //            prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
            //            loanStatus = "Running"
            //        });

            //    if (exposure.Count() > 0) exposures.AddRange(exposure);

            //    // Same for revolving and contegent facility ...

            //    exposure = context.TBL_LOAN_REVOLVING
            //        .Where(x => x.CUSTOMERID == detail.CUSTOMERID && x.PRODUCTID == detail.PRODUCTID && x.LOANSTATUSID == (int)LoanStatusEnum.Active)
            //        .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
            //        .Select(g => new CurrentCustomerExposure
            //        {
            //            facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
            //            existingLimit = g.Sum(x => x.OVERDRAFTLIMIT),
            //            proposedLimit = g.Sum(x => x.OVERDRAFTLIMIT),
            //            recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
            //            PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
            //            pastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
            //            reviewDate = DateTime.Now,
            //            prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
            //            loanStatus = "Running"
            //        });

            //    if (exposure.Count() > 0) exposures.AddRange(exposure);

            //    //exposure = from a in context.TBL_LOAN_APPLICATION_DETAIL
            //    //           where a.CUSTOMERID == detail.CUSTOMERID && a.APPROVEDPRODUCTID == detail.PRODUCTID && (a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved || a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
            //    //           select new CurrentCustomerExposure
            //    //           {
            //    //               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
            //    //               existingLimit = 0,
            //    //               proposedLimit = a.PROPOSEDAMOUNT,
            //    //               recommendedLimit = a.APPROVEDAMOUNT,
            //    //               PastDueObligationsInterest = 0,
            //    //               PastDueObligationsPrincipal = 0,
            //    //               reviewDate = DateTime.Now,
            //    //               prudentialGuideline = "Processing",
            //    //               loanStatus = "Processing"
            //    //           };

            //    //if (exposure.Count() > 0) exposures.AddRange(exposure);

            //}

            //exposures.Add(new CurrentCustomerExposure
            //{
            //    facilityType = "TOTAL",
            //    existingLimit = exposures.Sum(t => t.existingLimit),
            //    proposedLimit = exposures.Sum(t => t.proposedLimit),
            //    recommendedLimit = exposures.Sum(t => t.recommendedLimit),
            //    PastDueObligationsInterest = exposures.Sum(t => t.PastDueObligationsInterest),
            //    pastDueObligationsPrincipal = exposures.Sum(t => t.pastDueObligationsPrincipal),
            //    reviewDate = DateTime.Now,
            //    prudentialGuideline = String.Empty,
            //    loanStatus = String.Empty,
            //});

            return exposures;
        }

        //public List<CurrentCustomerExposure> GetGroupExposurebyCustomerId(int customerId, int companyId)
        //{
        //    var exposures = groupRepo.GetGroupExposureByCustomerId(customerId, companyId);
        //    return exposures;
        //}

        public decimal GetCustomerTotalOutstandingBalanceForLoans(int customerId)
        {
            var loanData = context.TBL_LOAN.FirstOrDefault(x => x.CUSTOMERID == customerId);

            decimal loanBalance = 0;

            if (loanData != null)
            {
                var balance = (from a in context.TBL_LOAN where a.CUSTOMERID == customerId select a.OUTSTANDINGPRINCIPAL).Sum();
                loanBalance = balance;
            }

            return loanBalance;
        }

        public decimal GetCustomerTotalOutstandingBalanceForOverdrafts(int customerId)
        {
            var overdraftData = context.TBL_LOAN_REVOLVING.FirstOrDefault(x => x.CUSTOMERID == customerId);

            decimal overdraftBalance = 0;

            if (overdraftData != null)
            {
                var balance = (from a in context.TBL_LOAN_REVOLVING where a.CUSTOMERID == customerId select a.OVERDRAFTLIMIT).Sum();
                overdraftBalance = balance;
            }

            return overdraftBalance;
        }

        public decimal GetCustomerTotalOutstandingBalanceForContingents(int customerId)
        {
            var contingentData = context.TBL_LOAN_CONTINGENT.FirstOrDefault(x => x.CUSTOMERID == customerId);

            decimal contingentBalance = 0;

            if (contingentData != null)
            {
                var balance = (from a in context.TBL_LOAN_CONTINGENT where a.CUSTOMERID == customerId select a.CONTINGENTAMOUNT).Sum();
                contingentBalance = balance;
            }

            return contingentBalance;
        }

        //Classified Assets Management
        public ClassifiedAssetManagementViewModel ClassifiedAssetManagementReview(string applicationReferenceNumber)
        {
            string listOfshareHolders = String.Empty;
            string listOfSignitories = String.Empty;
            string listOfDirectors = String.Empty;

            var cam = (from a in context.TBL_LMSR_APPLICATION
                       join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                       join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                       join d in context.TBL_LOAN on b.LOANID equals d.TERMLOANID
                       where a.APPLICATIONREFERENCENUMBER == applicationReferenceNumber && b.LOANSYSTEMTYPEID==(int)LoanSystemTypeEnum.TermDisbursedFacility
                       select new ClassifiedAssetManagementViewModel
                       {
                           customerId = c.CUSTOMERID,
                           loanId = d.TERMLOANID,
                           accountNumber = context.TBL_CASA.Where(o => o.CASAACCOUNTID == d.CASAACCOUNTID).Select(o => o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                           amountDisbursed = d.PRINCIPALAMOUNT,
                           amountPaidSoFar = d.PRINCIPALAMOUNT-d.OUTSTANDINGPRINCIPAL, 
                           amountProposed = b.PROPOSEDAMOUNT,
                           branchAddress = context.TBL_BRANCH.Where(o=>o.BRANCHID==d.BRANCHID).Select(o=>o.ADDRESSLINE1 + " " + o.ADDRESSLINE2).FirstOrDefault(),
                           branchManager = "", //
                           branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == d.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                           customerName = c.FIRSTNAME + " " + c.MAIDENNAME + " " + c.LASTNAME,
                           dateClassified = context.TBL_LOAN_CAMSOL.Where(o => o.LOANID == d.TERMLOANID).Select(o=>o.DATE).FirstOrDefault() , 
                           dateFacilityWasGranted = d.EFFECTIVEDATE, 
                           facilityType = context.TBL_PRODUCT.Where(o => o.PRODUCTID == d.PRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                           facilityAmountGranted = b.APPROVEDAMOUNT,
                           incumbentAccountOfficer = context.TBL_STAFF.Where(o=>o.STAFFID==d.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                           nameOfInitialAccountOfficer = context.TBL_STAFF.Where(o => o.STAFFID == context.TBL_STAFF_ACCOUNT_HISTORY.Where(y => y.TARGETID == d.TERMLOANID).OrderByDescending(y => y.DATETIMECREATED).Select(y => o.STAFFID).FirstOrDefault()).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),

                           interestOverdue = d.PASTDUEINTEREST + d.INTERESTONPASTDUEINTEREST + d.INTERESTONPASTDUEPRINCIPAL, 
                           pricipalOutstanding = d.OUTSTANDINGPRINCIPAL + d.PASTDUEPRINCIPAL,
                           //proposedRepaymentTenor = (d.MATURITYDATE - d.EFFECTIVEDATE).TotalDays,
                           totalPaidAndProposed = (d.PRINCIPALAMOUNT - d.OUTSTANDINGPRINCIPAL) - b.PROPOSEDAMOUNT, 
                           totalOutstanding = 0 //

                       }).FirstOrDefault();

            if (cam == null) return null;

            var securty = (from x in context.TBL_LOAN_COLLATERAL_MAPPING
                          join b in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals b.COLLATERALCUSTOMERID
                          join c in context.TBL_COLLATERAL_IMMOVE_PROPERTY on b.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                          where x.LOANID == cam.loanId
                          select new { x, b, c }).FirstOrDefault();

            var shareHolders = (from d in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                             where d.CUSTOMERID == cam.customerId && d.COMPANYDIRECTORTYPEID == (int)CustomerCompanyDirectorTypeEnum.Shareholder
                             select d.FIRSTNAME + " " + d.MIDDLENAME + " " + d.SURNAME).ToList();

            foreach (var x in shareHolders)
                listOfshareHolders = listOfshareHolders + x + ", ";
            var signatories = (from d in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                             where d.CUSTOMERID == cam.customerId && d.COMPANYDIRECTORTYPEID == (int)CustomerCompanyDirectorTypeEnum.AccountSignatory
                             select d.CUSTOMERBVN).ToList();

            foreach (var x in signatories)
                listOfSignitories = listOfSignitories + x + ", ";


            var director = (from d in context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                where d.CUSTOMERID == cam.customerId && d.COMPANYDIRECTORTYPEID == (int)CustomerCompanyDirectorTypeEnum.BoardMember && d.COMPANYDIRECTORTYPEID==(int)CustomerCompanyDirectorTypeEnum.BoardMemberShareholder
                               select d.FIRSTNAME + " " + d.MIDDLENAME + " " + d.SURNAME).ToList();

            foreach (var x in director)
                listOfDirectors = listOfDirectors + x + ", ";

            if (securty != null)
            {
                cam.securityType = context.TBL_COLLATERAL_TYPE.Where(o => o.COLLATERALTYPEID == securty.b.COLLATERALTYPEID).Select(o => o.COLLATERALTYPENAME).FirstOrDefault();
                cam.isResidential = securty.c.ISRESIDENTIAL;
                cam.securityDescription = securty.c.PROPERTYNAME;
                cam.securityFirstSellValue = securty.c.FORCEDSALEVALUE;
                cam.securityLocation = securty.c.PROPERTYADDRESS;
                cam.securityOpenMarketValue = securty.c.OPENMARKETVALUE;
                cam.isOwnerOccupied = securty.c.ISOWNEROCCUPIED;
                cam.securityPerfectionStatus = securty.c.PERFECTIONSTATUSID;
                cam.securityValuationDate = securty.c.LASTVALUATIONDATE;
            }
            cam.shareHolders = listOfshareHolders.TrimEnd(',');
            cam.signitories = listOfSignitories.TrimEnd(',');
            cam.directors = listOfDirectors;

            return cam;
        }

        // customer turnover
        //public IEnumerable<CustomersTurnoverViewModel> GetCustomerTurnover()
        //{
        //    List<int> ids = new List<int>();
        //    foreach (var exposure in customerIds) ids.Add(exposure.customerId);
        //    List<CustomersTurnoverViewModel> turnover = new List<CustomersTurnoverViewModel>();
        //    turnover = loan.GetCustomerTurnover(ids, lmsCamOperationIds.Contains(operationId));
        //    return turnover;
        //}

        private string CustomerTurnoverMarkup()
        {
            var result = String.Empty;
            //var turnover = GetCustomerTurnover();
            //var n = 0;
            //result = result + $@"
            //    <table style='font face: arial; size:12px' border=1>
            //        <tr>
            //            <th><b>S/N</b></th>
            //            <th><b>Account ID</b></th>
            //            <th><b>Scheme Type</b></th>
            //            <th><b>Min Debit Balance</b></th>
            //            <th><b>Max Debit Balance</b></th>
            //            <th><b>Min Creit Balance</b></th>
            //            <th><b>Max Credit Balance</b></th>
            //            <th><b>Debit Turnover</b></th>
            //            <th><b>Credit Turnover</b></th>
            //        </tr>
            //     ";
            //foreach (var t in turnover)
            //{
            //    n++;
            //    result = result + $@"
            //        <tr>
            //            <td>{n}</td>
            //            <td>{t.accountId}</td>
            //            <td>{t.schemeType}</td>
            //            <td>{t.minimumDebitBalance}</td>
            //            <td>{t.maximumDebitBalance}</td>
            //            <td>{t.minimumCreitBalance}</td>
            //            <td>{t.maximumCreditBalance}</td>
            //            <td>{t.debitTurnover}</td>
            //            <td>{t.creditTurnover}</td>
            //        </tr>
            //    ";
            //}
            //result = result + $"</table>";
            return result;
        }

        public string GenericMemoMarkupHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br />
                <h3><b>MEMO</b></h3>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>
                    <tr>
                        <td><b>Date</b></td>
                        <td>{DateTime.UtcNow}</td>
                    </tr>
                    <tr>
                        <td><b>To:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>From:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>Location:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>Subject:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>No. Of Pages:</b></td>
                        <td></td>
                    </tr>
                 ";
            result = result + $"</table>";
            result = result + $@" 
                    <p></p>
                    <p><b>1. BACKGROUND</b></p>
                    <p></p>
                    <p><b>2. COLLATERAL</b></p>
                    <p></p>
                    <p><b>3. ACCOUNT STATUS/ANALYSIS</b></p>
                    <p></p>
                    <p><b>4. ISSUES</b></p>
                    <p></p>
                    <p><b>5. CURRENT UPDATES</b></p>
                    <p></p>
                    <p><b>6. REQUEST/RECOMMENDATION</b></p>
                    <p></p>
                    <p><b>7. JUSTIFICATION</b></p>";
            return result;
        }

        public string MemoMarkupHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br />
                <h3><b>MEMO</b></h3>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>
                    <tr>
                        <td><b>Date</b></td>
                        <td>{DateTime.UtcNow}</td>
                    </tr>
                    <tr>
                        <td><b>To:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>From:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>Location:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>Subject:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>No. Of Pages:</b></td>
                        <td></td>
                    </tr>
                 ";
            result = result + $"</table>";
            result = result + $@" 
                    <p></p>
                    <p><b>1. BACKGROUND</b></p>
                    <p></p>
                    <p><b>2. COLLATERAL</b></p>
                    <p>{GetAllCustomerCollateralsMarkup()}</p>
                    <p><b>3. ACCOUNT STATUS/ANALYSIS</b></p>
                    <p>{FussCustomerAccountActivityHtml()}</p>
                    <p><b>4. ISSUES</b></p>
                    <p></p>
                    <p><b>5. CURRENT UPDATES</b></p>
                    <p></p>
                    <p><b>6. REQUEST/RECOMMENDATION</b></p>
                    <p></p>
                    <p><b>7. JUSTIFICATION</b></p>";
            return result;
        }
        public string FacilityUpgradeSupportSchemeHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br />
                <h3><b>CREDIT PROGRAM SHEET (FACILITY UPGRADE SUPPORT SCHEME)</b></h3>
                <br />
                <h4><b>Customer Information</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td>Borrower</td>
                        <td colspan='3'>------------------------------------------</td>
                    </tr>
                   <tr>
                        <td>Location</td>
                        <td>------------------------------------------</td>
                        <td>Customer Risk Rating</td>
                        <td>-------------------</td>
                    </tr> 
                     <tr>
                        <td>Business</td>
                        <td>------------------------------------------</td>
                        <td>Classification</td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>Account Number</td>
                        <td>------------------------------------------</td>
                        <td>Account Opening Date</td>
                        <td>-------------------</td>
                    </tr> 
                     <tr>
                        <td>Incorporation Date</td>
                        <td>------------------------------------------</td>
                        <td>Biz Commencement Date</td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>Principal Promoters</td>
                        <td colspane='3'>------------------------------------------</td>
                        
                    </tr> 
                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>School Fees Information</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <td>Total No of Pupils</td>
                        <td>----------------------</td>
                        <td>No of Staff</td>
                        <td>-------------------</td>
                    </tr> 
                   <tr>
                        <td>Next School Reopening Date </td>
                        <td colspan='3'>------------------------------------------</td>
                    </tr> 
                      <tr>
                        <td>Proposed Facility Repayment Date </td>
                        <td colspan='3'>------------------------------------------</td>
                    </tr>
                   <tr>
                        <td>Expected Amount Collectible before next Maturing Loan Obligation  </td>
                        <td colspan='3'>-------------</td>
                    </tr>
                     <tr>
                        <td>Maturity Amount at Due Date </td>
                        <td colspan='3'>------------------------------------------</td>
                    </tr>
                   <tr>
                        <td>Monthly Salary Payments and Other Expenses </td>
                        <td colspan='3'>------------------------------------------</td>
                    </tr>
                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Customer Facilities as @ xx/xx/xxxx</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility</b></th>
                        <th><b>Amount (‘000)</b></th>
                        <th><b>Maturity</b></th>
                        <th><b>Security</b></th>
                        <th><b>Performing</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Account Activity with Current (Major) Banker per period of 6 months</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th colspan='2'><b>Period (e.g. Jan 08 to Mar 09)</b></th>
                        <th><b>Debits</b></th>
                        <th><b>Credits</b></th>
                        <th><b>Returned Cheque</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                       <td colspan='2'>Current Book Balance</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'>Average Monthly Credit Turnover</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'><b>Other Bankers/Age</b></td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'>Existing Facility Type/Maturity</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'>Security/Support</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Cash flow Analysis/Projections</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     
                   <tr>
                        <td></td>
                        <td colspan='5'>Year 1(indicate year)-Most Recent</td>
                        <td colspan='5'>Year 2 (indicate year)-Projections</td>
                       
                    </tr> 
                      <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td>1st Term </td>
                        <td>2nd Term </td>
                        <td>3rd Term</td>
                        <td></td>
                        <td></td>
                        <td>1st Term </td>
                        <td>2nd Term </td>
                        <td>3rd Term</td>
                    </tr> 
                     <tr>
                        <td>INFLOWS(A)</td>
                        <td>Amount per student</td>
                        <td>Total amount paid per term</td>
                        <td>(‘000) </td>
                        <td>(‘000)</td>
                        <td>(‘000)</td>
                        <td>Amount per student</td>
                        <td>Total amount paid per term</td>
                        <td>(‘000) </td>
                        <td>(‘000)</td>
                        <td>(‘000)</td>
                    </tr> 
                     <tr>
                        <td>Crèche (no of students)</td>
                        <td>xx</td>
                        <td>No of students * Amt per student(xx)</td>
                        <td>xxx</td>
                        <td>xxx</td>
                        <td>xx</td>
                        <td>xx</td>
                        <td>No of students * Amt per student(xx)</td>
                        <td>xxx</td>
                        <td>xxx</td>
                        <td>xx</td>
                    </tr> 
                    <tr>
                        <td>Nursery (no of students)</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                    <tr>
                        <td>Primary (no of students)</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                     <tr>
                        <td>JSS (no of students)</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                     <tr>
                        <td>SSS (no of students)</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                     <tr>
                        <td>Sub total </td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                      <tr>
                        <td>OUTFLOWS(B)</td>
                        <td>No of months</td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td>No of months</td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td>Salaries(Nxx/month)</td>
                        <td>4</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                        <td>4</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                    </tr> 
                     <tr>
                        <td>stationeries (Nxx/month)</td>
                        <td>3</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>3</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr>  
                      <tr>
                        <td>Loan repayment (principal & interest)</td>
                        <td></td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td></td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                    <tr>
                        <td>Miscellaneous expense (Nxx/month)</td>
                        <td>3</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>3</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                    <tr>
                        <td>Sub total </td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                    </tr> 
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Net Cashflow(A-B)</td>
                        <td></td>
                        <td></td>
                        <td>xxxxxx</td>
                        <td>xxxxxx</td>
                        <td>xxxxxx</td>
                        <td></td>
                        <td></td>
                        <td>xxxxxx</td>
                        <td>xxxxxx</td>
                        <td>xxxxxx</td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + "<br/>";
            result = result + "<em>(Net cash flow is the amount available for repayment of loan obligation)</em>";
            result = result + $@"
                <br />
                <h4><b>Summary Net Cash Flow </b></h4>
                <table style='font face: arial; size:12px' border=1 width=700 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Year</b></th>
                        <th><b></b></th>
                        <th><b>Net Cash Flow</b></th>        
                    </tr> 
                   <tr>
                        <td rolspan='3'>Year 1 (indicate year)</td>
                        <td>1st Term</td>
                        <td></td>
                   </tr> 
                   <tr>
                        <td></td>
                        <td>2nd Term</td>
                        <td></td>
                   </tr>  
                  <tr>
                        <td></td>
                        <td>3rd Term </td>
                        <td></td>
                   </tr>
                   <tr>
                        <td rolspan='3'>Year 1 (indicate year)</td>
                        <td>1st Term</td>
                        <td></td>
                   </tr>
                   <tr>
                        <td></td>
                        <td>2nd Term</td>
                        <td></td>
                   </tr>  
                  <tr>
                        <td></td>
                        <td>3rd Term </td>
                        <td></td>
                   </tr>
                   <tr>
                        <td colspan='2'>Total</td>
                        <td></td>                     
                   </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>CURRENT REQUEST:</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Facility Type:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Facility Amount:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Purpose:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Tenor:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Repayment Plan</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Price:</strong></td>
                        <td><table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Interest Rate:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Management Fees:</strong></td>
                        <td></td>
                        </tr>
                         <tr>
                        <td><strong>COT</strong></td>
                        <td></td>
                        </tr>
                        </table></td>
                        </tr>
                        <tr>
                        <td><strong>Security/Support:</strong> </td>
                        <td><ul><li>Legal mortgage on school property or any other property acceptable to Access Bank (FSV must be at least 120% of total loan amount),</li> 
                                <li>Letter of undertaking to lodge all school fees with Access Bank (supported by customized deposit slips), </li>
                                <li>Letter of authority permitting Access Bank to offset loan repayments from accounts with good funds. </li>
                                <li>Personal Guarantee of key promoter(s). </li>
                                </ul>
                        </td>
                        </tr>
                        </table>
                        </td>
                    </tr> 

                     <tr>
                        <td><strong>BACKGROUND INFORMATION ON THE OBLIGOR</strong> (including the mitigation of all risks analyzed in the credit program as well as any identified risk peculiar to the obligor).</td>
                    </tr>
                     <tr>
                        <td><strong>ATTESTATION:</strong><br/> 
                            I, ……………………………...attest to the integrity of the Promoter................................having known him/her for at least ........years. 
	
                            <br/><strong>Signature & Date</strong>	
                        </td>
                    </tr> 
                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>CONCURRENCES:</b></h4> <br />";
            //result = result + GetApprovalsMarkupLOS();
            
            result = result + $@"
                <br />
                <h4><b>CHECKLIST / ELIGIBILITY</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     
                   <tr>
                        <td colspan='4'><strong>TARGET MARKET SCREENING CRITERIA</strong></td>                     
                    </tr> 
                    <tr>
                        <td></td>
                        <td><strong>Required</strong></td>
                        <td><strong>Actual</strong></td>
                        <td><strong>Exception (Y/N)</strong></td>
                    </tr> 
                   <tr>
                        <td>Minimum years in business</td>
                        <td>5</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Obligor Risk Rating</td>
                        <td>3</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Minimum number of years of relationship with Access Bank </td>
                        <td>1 year;</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>School is approved by ministry of education</td>
                        <td>Yes</td>
                        <td></td>
                        <td>No deviation allowed</td>
                    </tr>  
                    <tr>
                        <td>Minimum annual profitability from relationship</td>
                        <td>N250,000</td>
                        <td></td>
                        <td></td>
                    </tr>  
                      <tr>
                        <td>School is located in approved cities</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td>School is not a startup</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td colspan='4'><strong>RISK ACCEPTANCE CRITERIA</strong></td>
                    </tr> 
                <tr>
                        <td>Tenor of short-term booking</td>
                        <td><=120 days</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Facility Maximum Amount</td>
                        <td>N50m</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Personal Guarantee of key promoter</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Written domiciliation of school fees</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>No of Staff</td>
                        <td>>30</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>No of Student Enrollment</td>
                        <td>>250</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Good CBN checking</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr>  
                      <tr>
                        <td>*Monthly collections must be thrice the monthly loan obligation</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td colspan='4'>*Quarterly profit must cover full year loan obligation *Yearly profit must cover the entire facility amount and interest
                       </td>
                     </tr> 
                     <tr>
                        <td colspan='4'><strong>DOCUMENTATION CHECKLIST</strong></td>
                        
                    </tr> 
                    <tr>
                        <td>Loan Application Form</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Approved Credit Program Memo</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Offer Letter</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Letter of Domiciliation</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Operating License</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Credit Checks Reports</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Statements of accounts </td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>  
                      <tr>
                        <td>Financial Statements / Annual Reports</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td>Other Documents:</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                   ";
            result = result + $"</table>";
            result = result + $@"
                 <br />";
            return result;
        }
        public string FussCustomerInformationHtml()
        {
            TBL_CUSTOMER customer = new TBL_CUSTOMER();
            string address = string.Empty;
            string accountNumber = string.Empty;
            var customerId = 0;

            //var currentApplicationId = this.loanApplication?.LOANAPPLICATIONID ?? 0;
            //if (this.loanApplication == null)
            //{
            //    var d = context.TBL_LMSR_APPLICATION_DETAIL.Where(a=>a.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID && a.DELETED == false).FirstOrDefault();
            //    currentApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
            //                         (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
            //                         (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault();

            //}

            if (this.loanApplication == null)
            {
                customerId = context.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).Select(d=>d.CUSTOMERID).FirstOrDefault();

            }
            else
            {
                customerId = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.loanApplication.LOANAPPLICATIONID).Select(d => d.CUSTOMERID).FirstOrDefault();

            }


            if (this.isThirdPartyFacility == false)
            {
                customer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == customerId).FirstOrDefault();
                 address = context.TBL_CUSTOMER_ADDRESS.Where(a => a.CUSTOMERID == customer.CUSTOMERID).Select(a => a.ADDRESS).FirstOrDefault();
                 accountNumber = context.TBL_CASA.Where(c => c.CUSTOMERID == customer.CUSTOMERID).Select(c => c.PRODUCTACCOUNTNUMBER).FirstOrDefault();
            }
            else
            {
                 customer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == this.customerRecord.CUSTOMERID).FirstOrDefault();
                 address = context.TBL_CUSTOMER_ADDRESS.Where(a => a.CUSTOMERID == this.customerRecord.CUSTOMERID).Select(a => a.ADDRESS).FirstOrDefault();
                 accountNumber = context.TBL_CASA.Where(c => c.CUSTOMERID == this.customerRecord.CUSTOMERID).Select(c => c.PRODUCTACCOUNTNUMBER).FirstOrDefault();
            }
            
           
            var riskRating = context.TBL_CUSTOMER_RISK_RATING.Find(customer.RISKRATINGID);

            var result = String.Empty;
            
            result = result + $@"
                <br />
                <h3><b>CREDIT PROGRAM SHEET (FACILITY UPGRADE SUPPORT SCHEME)</b></h3>
                <br />
                <h4><b>Customer Information</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>
                    <tr>
                        <td>Borrower</td>
                        <td colspan='3'>{customer?.FIRSTNAME} {customer?.MIDDLENAME} {customer?.LASTNAME}</td>
                    </tr>
                   <tr>
                        <td>Location</td>
                        <td>{address}</td>
                        <td>Customer Risk Rating</td>
                        <td>{customer?.CUSTOMERRATING}</td>
                    </tr> 
                     <tr>
                        <td>Business</td>
                        <td>{customer.OCCUPATION}</td>
                        <td>Classification</td>
                        <td>{riskRating?.CLASSIFICATION}</td>
                    </tr>
                   <tr>
                        <td>Account Number</td>
                        <td>{currentAccountNo}</td>
                        <td>Account Opening Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                    </tr> 
                     <tr>
                        <td>Incorporation Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                        <td>Biz Commencement Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                    </tr>
                   <tr>
                        <td>Principal Promoters</td>
                        <td colspane='3'>N/A</td>
                        
                    </tr> 
                   
                 ";
            result = result + $"</table><br />";
            return result;
        }
        public string FussSchoolFeesInformationHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br />
                <h4><b>School Fees Information</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <td>Total No of Pupils</td>
                        <td>----------------------</td>
                        <td>No of Staff</td>
                        <td>-------------------</td>
                    </tr> 
                   <tr>
                        <td>Next School Reopening Date </td>
                        <td colspan='3'>------------------------------------------</td>
                    </tr> 
                      <tr>
                        <td>Proposed Facility Repayment Date </td>
                        <td colspan='3'>------------------------------------------</td>
                    </tr>
                   <tr>
                        <td>Expected Amount Collectible before next Maturing Loan Obligation  </td>
                        <td colspan='3'>-------------</td>
                    </tr>
                     <tr>
                        <td>Maturity Amount at Due Date </td>
                        <td colspan='3'>------------------------------------------</td>
                    </tr>
                   <tr>
                        <td>Monthly Salary Payments and Other Expenses </td>
                        <td colspan='3'>------------------------------------------</td>
                    </tr>
                   
                 ";
            result = result + $"</table>";
            result = result + $@"<br />";
            return result;
        }
        public string FussCustomerFacilityHtml()
        {
            var result = String.Empty;
            result = result + $@"
                <br />
                <h4><b>Customer Facilities as @ {DateTime.UtcNow.ToString("dd-MM-yyyy")}</b></h4>";
            result = result + GetSecurityAnalysisMarkUP();
            result = result + $@"<br />";
            return result;
        }
        public string FussCustomerAccountActivityHtml()
        {
            var currentApplicationId = this.loanApplication != null ? this.loanApplication.LOANAPPLICATIONID : 0;
            if (this.loanApplication == null)
            {
                if(this.isThirdPartyFacility == false)
                {
                    var d = context.TBL_LMSR_APPLICATION_DETAIL.Where(a => a.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID && a.DELETED == false).FirstOrDefault();
                    currentApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                         (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
                                         (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault();
                }

            }

            var customerId = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == currentApplicationId).FirstOrDefault();
            var accountActivity = GetCustomerTransactions(customerId?.CUSTOMERID ?? this.customerRecord.CUSTOMERID, currentApplicationId, false);
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br />
                <h4><b>Account Activity with Current (Major) Banker per period of 6 months</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Account Number</b></th>
                        <th><b>Product Account Name</b></th>
                        <th><b>Period</b></th>
                        <th><b>Max Debit Balance</b></th>
                        <th><b>Min Debit Balance</b></th>
                        <th><b>Max Credit Balance</b></th>
                        <th><b>Min Credit Balance</b></th>
                        <th><b>Debit Turnover</b></th>
                        <th><b>Credit Turnover</b></th>
                        <th><b>Month</b></th>
                        <th><b>Year</b></th>
                    </tr>";
            foreach (var f in accountActivity)
            {
                result = result + $@"
                        <tr>
                        <td> {f.accountNumber}</td>
                        <td> {f.productAccountName}</td>
                        <td> {f.period}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.max_Debit_Balance))}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.min_Debit_Balance))}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.max_Credit_Balance))}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.min_Credit_Balance))}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.debit_Turnover))}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.credit_Turnover))}</td>
                        <td> {f.month}</td>
                        <td> {f.year}</td>
                    </tr>";

            }

            result = result + $"</table>";
            result = result + $@"
                 <br />";
            return result;
        }
        public string FussCustomerAccountActivitySummaryHtml()
        {
            var result = String.Empty;
            result = result + $@" 
                   <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                       <td colspan='2'>Current Book Balance</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'>Average Monthly Credit Turnover</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'><b>Other Bankers/Age</b></td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                 <br />";
            return result;
        }
        public string FussCashFlowAnalysisHtml()
        {
            var result = String.Empty;
            result = result + $@"
                <br />
                <h4><b>Cash flow Analysis/Projections</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     
                   <tr>
                        <td></td>
                        <td colspan='5'>Year 1(indicate year)-Most Recent</td>
                        <td colspan='5'>Year 2 (indicate year)-Projections</td>
                       
                    </tr> 
                      <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td>1st Term </td>
                        <td>2nd Term </td>
                        <td>3rd Term</td>
                        <td></td>
                        <td></td>
                        <td>1st Term </td>
                        <td>2nd Term </td>
                        <td>3rd Term</td>
                    </tr> 
                     <tr>
                        <td>INFLOWS(A)</td>
                        <td>Amount per student</td>
                        <td>Total amount paid per term</td>
                        <td>(‘000) </td>
                        <td>(‘000)</td>
                        <td>(‘000)</td>
                        <td>Amount per student</td>
                        <td>Total amount paid per term</td>
                        <td>(‘000) </td>
                        <td>(‘000)</td>
                        <td>(‘000)</td>
                    </tr> 
                     <tr>
                        <td>Crèche (no of students)</td>
                        <td>xx</td>
                        <td>No of students * Amt per student(xx)</td>
                        <td>xxx</td>
                        <td>xxx</td>
                        <td>xx</td>
                        <td>xx</td>
                        <td>No of students * Amt per student(xx)</td>
                        <td>xxx</td>
                        <td>xxx</td>
                        <td>xx</td>
                    </tr> 
                    <tr>
                        <td>Nursery (no of students)</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                    <tr>
                        <td>Primary (no of students)</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                     <tr>
                        <td>JSS (no of students)</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                     <tr>
                        <td>SSS (no of students)</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                     <tr>
                        <td>Sub total </td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                      <tr>
                        <td>OUTFLOWS(B)</td>
                        <td>No of months</td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td>No of months</td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td>Salaries(Nxx/month)</td>
                        <td>4</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                        <td>4</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                        <td>4*Nxx/month</td>
                    </tr> 
                     <tr>
                        <td>stationeries (Nxx/month)</td>
                        <td>3</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>3</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr>  
                      <tr>
                        <td>Loan repayment (principal & interest)</td>
                        <td></td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td></td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                    <tr>
                        <td>Miscellaneous expense (Nxx/month)</td>
                        <td>3</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>3</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                        <td>√</td>
                    </tr> 
                    <tr>
                        <td>Sub total </td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                        <td>xxxx</td>
                    </tr> 
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Net Cashflow(A-B)</td>
                        <td></td>
                        <td></td>
                        <td>xxxxxx</td>
                        <td>xxxxxx</td>
                        <td>xxxxxx</td>
                        <td></td>
                        <td></td>
                        <td>xxxxxx</td>
                        <td>xxxxxx</td>
                        <td>xxxxxx</td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + "<br/>";
            return result;
        }
        public string FussSummaryNetCashFlowHtml()
        {
            var result = String.Empty;
            result = result + "<br/>";
            result = result + "<em>(Net cash flow is the amount available for repayment of loan obligation)</em>";
            result = result + $@"
                <br />
                <h4><b>Summary Net Cash Flow </b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Year</b></th>
                        <th><b></b></th>
                        <th><b>Net Cash Flow</b></th>        
                    </tr> 
                   <tr>
                        <td rolspan='3'>Year 1 (indicate year)</td>
                        <td>1st Term</td>
                        <td></td>
                   </tr> 
                   <tr>
                        <td></td>
                        <td>2nd Term</td>
                        <td></td>
                   </tr>  
                  <tr>
                        <td></td>
                        <td>3rd Term </td>
                        <td></td>
                   </tr>
                   <tr>
                        <td rolspan='3'>Year 1 (indicate year)</td>
                        <td>1st Term</td>
                        <td></td>
                   </tr>
                   <tr>
                        <td></td>
                        <td>2nd Term</td>
                        <td></td>
                   </tr>  
                  <tr>
                        <td></td>
                        <td>3rd Term </td>
                        <td></td>
                   </tr>
                   <tr>
                        <td colspan='2'>Total</td>
                        <td></td>                     
                   </tr> 
                 ";
            result = result + $"</table>";
            return result;
        }
        public string FussCurrentRequestHtml()
        {
            var result = String.Empty;

            if (this.isThirdPartyFacility == true) { return string.Empty; }
            if (this.loanApplication != null)
            {
                var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.loanApplication.LOANAPPLICATIONID).FirstOrDefault();
                var repaymentTerm = context.TBL_REPAYMENT_TERM.Find(details.REPAYMENTSCHEDULEID);
                var id = details.LOANAPPLICATIONDETAILID;

                result = result + $@"
                <br />
                <h4><b>CURRENT REQUEST:</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>LOANS & SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Facility Type:</strong></td>
                        <td>{GetAllCustomerFacilitiesMarkup()} {GetAllCustomerFacilitiesLMSMarkup()}</td>
                        </tr>
                        <tr>
                        <td><strong>Facility Amount:</strong></td>
                        <td>{approvedAmount}</td>
                        </tr>
                        <tr>
                        <td><strong>Purpose:</strong></td>
                        <td>{details?.LOANPURPOSE}</td>
                        </tr>
                        <tr>
                        <td><strong>Tenor:</strong></td>
                        <td>{approvedTenorString}</td>
                        </tr>
                        <tr>
                        <td><strong>Repayment Plan</strong></td>
                        <td>{repaymentTerm?.REPAYMENTTERMDETAIL}</td>
                        </tr>
                        <tr>
                        <td><strong>Price:</strong></td>
                        <td><table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Interest Rate:</strong></td>
                        <td>{interestRate}</td>
                        </tr>
                        <tr>
                        <td><strong>Fees:</strong></td>
                        <td>{GetFees(id)}</td>
                        </tr>
                         <tr>
                        <td><strong>COT</strong></td>
                        <td>N/A</td>
                        </tr>
                        </table></td>
                        </tr>
                        <tr>
                        <td><strong>Security/Support:</strong> </td>
                        <td>{GetAllCustomerCollateralsMarkup()} {GetAllCustomerCollateralsMarkupLMS()}
                        </td>
                        </tr>
                        </table>
                        </td>
                    </tr> 
                 ";
                result = result + $"</table>";
                result = result + $@"
                 <br />";
            }
            else
            {
                var ids = context.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).FirstOrDefault().LOANREVIEWAPPLICATIONID;
                var details = context.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).FirstOrDefault();

                result = result + $@"
                <br />
                <h4><b>CURRENT REQUEST:</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>LOANS & SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Facility Type:</strong></td>
                        <td>{GetAllCustomerFacilitiesMarkup()} {GetAllCustomerFacilitiesLMSMarkup()}</td>
                        </tr>
                        <tr>
                        <td><strong>Facility Amount:</strong></td>
                        <td>{approvedAmount}</td>
                        </tr>
                        <tr>
                        <td><strong>Purpose:</strong></td>
                        <td>{details?.REVIEWDETAILS}</td>
                        </tr>
                        <tr>
                        <td><strong>Tenor:</strong></td>
                        <td>{approvedTenorString}</td>
                        </tr>
                        <tr>
                        <td><strong>Repayment Plan</strong></td>
                        <td>Nil</td>
                        </tr>
                        <tr>
                        <td><strong>Price:</strong></td>
                        <td><table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Interest Rate:</strong></td>
                        <td>{interestRate}</td>
                        </tr>
                        <tr>
                        <td><strong>Fees:</strong></td>
                        <td>{GetFees(ids)}</td>
                        </tr>
                         <tr>
                        <td><strong>COT</strong></td>
                        <td>N/A</td>
                        </tr>
                        </table></td>
                        </tr>
                        <tr>
                        <td><strong>Security/Support:</strong> </td>
                        <td>{GetAllCustomerCollateralsMarkup()} {GetAllCustomerCollateralsMarkupLMS()}
                        </td>
                        </tr>
                        </table>
                        </td>
                    </tr> 
                 ";
                result = result + $"</table>";
                result = result + $@"
                 <br />";
            }

            return result;
        }
        public string FussBackgroungInformationHtml()
        {
            var result = String.Empty;
            
            result = result + $@"
                <br />
                <h4><b>BACKGROUND INFORMATION:</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><strong>BACKGROUND INFORMATION ON THE OBLIGOR</strong> (including the mitigation of all risks analyzed in the credit program as well as any identified risk peculiar to the obligor).</td>
                    </tr>
                 ";
            result = result + $"</table>";
            result = result + $@"
                 <br />";
            result = result + $@"
                <br />
               
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <td><strong>ATTESTATION:</strong><br/> 
                            I, ……………………………...attest to the integrity of the Promoter................................having known him/her for at least ........years. 
	
                            <br/><strong>Signature & Date</strong>	
                        </td>
                    </tr> 
                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                 <br />";
            return result;
        }
        public string FussChecklistEligibilityHtml()
        {
            var currentApplicationDetailId = 0;
            if (this.isThirdPartyFacility) { return string.Empty; }
            if (this.loanApplication != null)
            {
                currentApplicationDetailId = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().LOANAPPLICATIONDETAILID;
            }
            else
            {
                var d = context.TBL_LMSR_APPLICATION_DETAIL.Where(a => a.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID && a.DELETED == false).FirstOrDefault();
                currentApplicationDetailId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select l.LOANAPPLICATIONDETAILID).FirstOrDefault() :
                                     (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select l.LOANAPPLICATIONDETAILID).FirstOrDefault() :
                                     (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select l.LOANAPPLICATIONDETAILID).FirstOrDefault();

            }
            var racs = (from r in context.TBL_RAC_DETAIL
                        join rd in context.TBL_RAC_DEFINITION on r.RACDEFINITIONID equals rd.RACDEFINITIONID
                        join ri in context.TBL_RAC_ITEM on rd.RACITEMID equals ri.RACITEMID
                        where r.TARGETID == currentApplicationDetailId
                        select new ProductRacItem
                        {
                            criteria = ri.CRITERIA,
                            value = r.ACTUALVALUE,
                        }).ToList();
            var n = 0;
            var result = String.Empty;
            result = result + $@"
                <br />
                <h4><b>CHECKLIST / ELIGIBILITY</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td><strong>S/N</strong></td>
                        <td><strong>Required</strong></td>
                        <td><strong>Input Option</strong></td>
                    </tr>";
            foreach (var f in racs)
            {
                n++;
                result = result + $@"
                        <tr>
                        <td> {n}</td>
                        <td> {f.criteria}</td>
                        <td> {GetRacValue(f.value)}</td>
                    </tr>";

            }
                   
            result = result + $"</table>";
            result = result + $@"
                 <br />";
            return result;
        }
        private string GetRacValue(string rac)
        {
            if (rac.Length > 1)
            {
                return rac.ToString();
            }
            else { 
                if(int.Parse(rac) == 1)
                {
                    return "YES";
                }

                return "NO";
            }
            
        }
        public string FussCustomerConditionSubsequentHtml()
        {
            IEnumerable<OfferLetterConditionPrecidentViewModel> ConditionSubsequent = new List<OfferLetterConditionPrecidentViewModel>();
            var result = String.Empty;
            var n = 0;
            if (this.lmsrApplication == null)
            {
                result = result + GetConditionsPrecedentToDrawdownLOSMarkup();
            }
            else
            {
                ConditionSubsequent = GetLoanApplicationConditionSubsequentLMS(this.lmsrApplication.LOANAPPLICATIONID);
                result = result + $@"
                <br />
                <h4><b>CONDITIONS</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>S/N</b></th>
                        <th><b>Product Name</b></th>
                        <th><b>Condition Precedent</b></th>
                    </tr>";
                foreach (var f in ConditionSubsequent)
                {
                    n++;
                    result = result + $@"
                        <tr>
                        <td> {n}</td>
                        <td> {f.productName}</td>
                        <td> {f.conditionPrecident}</td>
                    </tr>";

                }

                result = result + $"</table>";
            }
            result = result + $@"
                 <br />";
            return result;
        }
        
        public string GenericConditionSubsequentHtml()
        {
            var result = String.Empty;
            
            result = result + $@"
            <br />
            <h4><b>CONDITIONS</b></h4>
            <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                    <th><b>S/N</b></th>
                    <th><b>Product Name</b></th>
                    <th><b>Condition Precedent</b></th>
                </tr>";

            result = result + $"</table>";
            result = result + $@"
                 <br />";
            return result;
        }
        public string GenericConditionDynamicsHtml()
        {
            var result = String.Empty;
            result = result + $@"
            <br />
            <h4><b>TRANSACTION DYNAMICS</b></h4>
            <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                    <th><b>S/N</b></th>
                    <th><b>Product Name</b></th>
                    <th><b>Transaction Dynamics</b></th>
                </tr>";
            result = result + $"</table>";
            result = result + $@"
                 <br />";
            return result;
        }
        public string FussCustomerConditionDynamicsHtml()
        {
            IEnumerable<TransactionDynamicsViewModel> ConditionSubsequent = new List<TransactionDynamicsViewModel>();
            var result = String.Empty;
            var n = 0;
            if (this.lmsrApplication == null)
            {
                result = result + GetTransactionsDynamicsMarkup();
            }
            else
            {
                ConditionSubsequent = Los_ConditionDynamicsLMS(this.lmsrApplication.LOANAPPLICATIONID);
                result = result + $@"
                <br />
                <h4><b>TRANSACTION DYNAMICS</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>S/N</b></th>
                        <th><b>Product Name</b></th>
                        <th><b>Transaction Dynamics</b></th>
                    </tr>";
                foreach (var f in ConditionSubsequent)
                {
                    n++;
                    result = result + $@"
                        <tr>
                        <td> {n}</td>
                        <td> {f.productName}</td>
                        <td> {f.dynamics}</td>
                    </tr>";

                }

                result = result + $"</table>";
            }
            result = result + $@"
                 <br />";
            return result;
        }

        public List<CustomerTransactionsViewModels> GetCustomerTransactions(int customerId, int applicationId, bool isLms = false)
        {
            var first = (from a in context.TBL_LOAN_APPLICATION_TRANS
                         where a.CUSTOMERID == customerId && a.LOANAPPLICATIONID == applicationId && a.ISLMS == isLms
                         select new CustomerTransactionsViewModels
                         {
                             cust_Id = a.CUSTOMERTRANSACTIONID.ToString(),
                             period = a.PERIOD,
                             productName = a.PRODUCTNAME,
                             accountNumber = a.ACCOUNTNUMBER,
                             max_Credit_Balance = a.MAXIMUMCREDITBALANCE,
                             max_Debit_Balance = a.MAXIMUMDEBITBALANCE,
                             min_Credit_Balance = a.MINIMUMCREDITBALANCE,
                             min_Debit_Balance = a.MINIMUMDEBITBALANCE,
                             credit_Turnover = a.CREDITTURNOVER,
                             debit_Turnover = a.DEBITTURNOVER,
                             month = a.MONTH,
                             year = a.YEAR,
                             productAccountName = context.TBL_CASA.Where(o => o.CUSTOMERID == customerId).Select(o => o.PRODUCTACCOUNTNAME).FirstOrDefault(),
                         }).OrderByDescending(m => m.year).ThenByDescending(b => b.month).ToList();


            return first;
        }
        public string InvoiceDiscountingHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4><br/>
                <h3><b>CREDIT PROGRAM SHEET (INVOICE DISCOUNTING CREDIT PROGRAM)</b></h3>
                <br />
                <h4><b>Customer Information</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td>Borrower</td>
                        <td colspan='3'>------------------------------------------</td>
                    </tr>
                   <tr>
                        <td>Location</td>
                        <td>------------------------------------------</td>
                        <td>Customer Risk Rating</td>
                        <td>-------------------</td>
                    </tr> 
                     <tr>
                        <td>Business</td>
                        <td>------------------------------------------</td>
                        <td>Classification</td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>Account Number</td>
                        <td>------------------------------------------</td>
                        <td>Account Opening Date</td>
                        <td>-------------------</td>
                    </tr> 
                     <tr>
                        <td>Incorporation Date</td>
                        <td>------------------------------------------</td>
                        <td>Biz Commencement Date</td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>Principal Promoters</td>
                        <td colspane='3'>------------------------------------------</td>
                        
                    </tr> 

                    <tr>
                        <td>Contract Employer</td>
                        <td colspane='3'>------------------------------------------</td>
                    </tr> 
                     <tr>
                        <td>No of Payments from Principal in the last 3 months</td>
                        <td colspane='3'>------------------------------------------</td>
                    </tr>
                   <tr>
                        <td>Alternate Contract Employer</td>
                       <td colspane='3'>------------------------------------------</td>
                    </tr> 
                     <tr>
                        <td>No of Payments from Principal in the last 3 months</td>
                        <td colspane='3'>------------------------------------------</td>
                    </tr>
                   <tr>
                        <td>Discount Value (50%)</td>
                        <td colspane='3'>------------------------------------------</td>
                        
                    </tr> 
                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Customer Facilities as @ xx/xx/xxxx</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility</b></th>
                        <th><b>Amount (‘000)</b></th>
                        <th><b>Maturity</b></th>
                        <th><b>Security</b></th>
                        <th><b>Performing</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>6 Months Activity with Current (Major) Banker</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th colspan='2'><b>Month</b></th>
                        <th><b>Debits</b></th>
                        <th><b>Credits</b></th>
                        <th><b>Returned Cheque</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                       <td colspan='2'>Current Book Balance</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'>Average Monthly Credit Turnover</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'><b>Other Bankers/Age</b></td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'>Existing Facility Type/Maturity</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'>Security/Support</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>CURRENT REQUEST:</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Facility Type:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Facility Amount:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Purpose:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Tenor:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Repayment Plan</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Price:</strong></td>
                        <td><table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Interest Rate:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Management Fees:</strong></td>
                        <td></td>
                        </tr>
                         <tr>
                        <td><strong>COT</strong></td>
                        <td></td>
                        </tr>
                        </table></td>
                        </tr>
                        <tr>
                        <td><strong>Security/Support:</strong> </td>
                        <td><ul><li>Deed of assignment of receivables. </li> 
                                <li>Domiciliation of Payment Mandate.</li>
                                <li>Letter of authority permitting Access Bank to offset loan repayments from accounts with good funds.</li>
                                <li>Personal Guarantee of key promoter(s).</li>
                                <li>Execution of All Asset Debentures. </li>
                                <li>Other  form of security acceptable to Access Bank Plc.</li>
                                </ul>
                        </td>
                        </tr>
                        </table>
                        </td>
                    </tr> 

                     <tr>
                        <td><strong>BACKGROUND INFORMATION ON THE OBLIGOR</strong> (including the mitigation of all risks analyzed in the credit program as well as any identified risk peculiar to the obligor).</td>
                    </tr>
                     <tr>
                        <td><strong>ATTESTATION:</strong><br/> 
                            I, ……………………………...attest to the integrity of the Promoter................................having known him/her for at least ........years. 
	
                            <br/><strong>Signature & Date</strong>	
                        </td>
                    </tr> 
                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />";
            
            result = result + $@"
                <br />
                <h4><b>CHECKLIST / ELIGIBILITY</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     
                   <tr>
                        <td colspan='4'><strong>TARGET MARKET SCREENING CRITERIA</strong></td>                     
                    </tr> 
                    <tr>
                        <td></td>
                        <td><strong>Required</strong></td>
                        <td><strong>Actual</strong></td>
                        <td><strong>Exception (Y/N)</strong></td>
                    </tr> 
                   <tr>
                        <td>Minimum years in business</td>
                        <td>5</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Obligor Risk Rating</td>
                        <td>2</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Minimum number of years of relationship with Access Bank </td>
                        <td>2 year</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Minimum Asset Base </td>
                        <td>N10bn</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>History of honoring obligation when due</td>
                        <td>Favorable/Unfavorable</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Industry</td>
                        <td>Oil & Gas; Telecoms; </td>
                        <td></td>
                        <td></td>
                    </tr>                       
                     <tr>
                        <td colspan='4'><strong>RISK ACCEPTANCE CRITERIA</strong></td>
                    </tr> 
                    <tr>
                        <td>Minimum years in business</td>
                        <td>5</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Obligor Risk Rating</td>
                        <td>3</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Customer’s minimum annual sales</td>
                        <td>N300m</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Minimum number of years of relationship with Access Bank OR Principal</td>
                        <td>Access Bank: 1 year; Principal: 2 years
                        </td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Satisfactory Trade or Bank Checking</td>
                        <td>>yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Principal is Bank’s customer</td>
                        <td>>Yes</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Principal is on Approved List</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr>  
                                     
                     <tr>
                        <td colspan='4'></td>
                        
                    </tr> 
                    <tr>
                        <td>Acceptance of Domiciliation Agreements</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Principal  minimum annual sales</td>
                        <td>N5bn</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td colspan='4'><strong>Risk Acceptance Criteria (Obligor)</strong></td>             
                    </tr>
                    <tr>
                        <td></td>
                        <td><strong>Required</strong></td>
                        <td><strong>Actual</strong></td>
                        <td><strong>Exception (Y/N)</strong></td>
                    </tr> 
                    <tr>
                        <td>Minimum years in business</td>
                        <td>5</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Obligor Risk Rating</td>
                        <td>3</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Customer’s minimum annual sales</td>
                        <td>N300m</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Minimum number of years of relationship with Access Bank OR Principal</td>
                        <td>Access Bank: 1 year; Principal: 2 years
                        </td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Satisfactory Trade or Bank Checking</td>
                        <td>>yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Principal is Bank’s customer</td>
                        <td>>Yes</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Principal is on Approved List</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr>
                     <tr>
                        <td colspan='4'></td>
                       
                    </tr> 
                     <tr>
                        <td>Facility Maximum Tenor</td>
                        <td>365 days</td>
                        <td></td>
                        <td></td>
                    </tr>
                     <tr>
                        <td>Cumulative tenor of booking</td>
                        <td><=135 days</td>
                        <td></td>
                        <td>No Deviation Allowed</td>
                    </tr>
                     <tr>
                        <td>Facility Maximum Amount</td>
                        <td>N50m</td>
                        <td></td>
                        <td>No Deviation Allowed</td>
                    </tr>
                     <tr>
                        <td>Personal Guarantee of key promoter</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr>
                     <tr>
                        <td>Deed of Assignment of receivables </td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr>
                     <tr>
                        <td>Invoice Discounting Facility Agreement</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>Written domiciliation</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr>
                   ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th colspan='4'><strong>Documentation Checklist</strong></th>                     
                    </tr> 
                   <tr>
                        <td>Deed of assignment for 100% contracts proceeds to Access Bank (where obtainable)</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Approved contract OR purchase order, stating bank A/C details.</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>  
                   <tr>
                        <td>Vendor’s letter to Principal requesting domiciliation of contract proceeds to Access Bank, stating that the instruction cannot be varied without the express consent of Access Bank.</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Domiciliation letter OR Proof that domiciliation has worked in the past (in cases where the principal is unwilling to accept domiciliation)</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>  
                   
                    <tr>
                        <td>A copy of the final invoice(s) and/or an original waybill, stamped ‘received’ by the Principal indicating the bank details. </td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>A formal client’s request for draw-down, stating relevant account details.</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Evidence of other receivables domiciled to us other the one being discounted</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
               <br />";
            return result;
        }
        public string IdfCustomerInformationHtml()
        {
            var customerId = 0;
            TBL_CUSTOMER customer = new TBL_CUSTOMER();
            string address = string.Empty;
            string accountNumber = string.Empty;

            //var currentApplicationId = this.loanApplication?.LOANAPPLICATIONID;
            //if (this.loanApplication == null)
            //{
            //    var d = context.TBL_LMSR_APPLICATION_DETAIL.Where(a => a.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID && a.DELETED == false).FirstOrDefault();
            //    currentApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
            //                         (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
            //                         (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault();

            //}

            if (this.loanApplication != null)
            {
                customerId = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.loanApplication.LOANAPPLICATIONID).Select(d=>d.CUSTOMERID).FirstOrDefault();

            }
            else
            {
                customerId = context.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).Select(d => d.CUSTOMERID).FirstOrDefault();
            }

            if (this.isThirdPartyFacility == false)
            {
                customer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == customerId).FirstOrDefault();
                address = context.TBL_CUSTOMER_ADDRESS.Where(a => a.CUSTOMERID == customer.CUSTOMERID).Select(a => a.ADDRESS).FirstOrDefault();
                accountNumber = context.TBL_CASA.Where(c => c.CUSTOMERID == customer.CUSTOMERID).Select(c => c.PRODUCTACCOUNTNUMBER).FirstOrDefault();
            }
            else
            {
                customer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == this.customerRecord.CUSTOMERID).FirstOrDefault();
                address = context.TBL_CUSTOMER_ADDRESS.Where(a => a.CUSTOMERID == this.customerRecord.CUSTOMERID).Select(a => a.ADDRESS).FirstOrDefault();
                accountNumber = context.TBL_CASA.Where(c => c.CUSTOMERID == this.customerRecord.CUSTOMERID).Select(c => c.PRODUCTACCOUNTNUMBER).FirstOrDefault();
            }
            var riskRating = context.TBL_CUSTOMER_RISK_RATING.Find(customer.RISKRATINGID);

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4><br/>
                <h3><b>CREDIT PROGRAM SHEET (INVOICE DISCOUNTING CREDIT PROGRAM)</b></h3>
                <br />
                <h4><b>Customer Information</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>
                    <tr>
                        <td>Borrower</td>
                        <td colspan='3'>{customer?.FIRSTNAME} {customer?.MIDDLENAME} {customer?.LASTNAME}</td>
                    </tr>
                   <tr>
                        <td>Location</td>
                        <td>{address}-</td>
                        <td>Customer Risk Rating</td>
                        <td>{customer?.CUSTOMERRATING}</td>
                    </tr> 
                     <tr>
                        <td>Business</td>
                        <td>{customer.OCCUPATION}</td>
                        <td>Classification</td>
                        <td>{riskRating?.CLASSIFICATION}</td>
                    </tr>
                   <tr>
                        <td>Account Number</td>
                        <td>{currentAccountNo}</td>
                        <td>Account Opening Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                    </tr> 
                     <tr>
                        <td>Incorporation Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                        <td>Biz Commencement Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                    </tr>
                   <tr>
                        <td>Principal Promoters</td>
                        <td colspane='3'>N/A</td>
                        
                    </tr> 

                    <tr>
                        <td>Contract Employer</td>
                        <td colspane='3'>N/A</td>
                    </tr> 
                     <tr>
                        <td>No of Payments from Principal in the last 3 months</td>
                        <td colspane='3'>N/A</td>
                    </tr>
                   <tr>
                        <td>Alternate Contract Employer</td>
                       <td colspane='3'>N/A</td>
                    </tr> 
                     <tr>
                        <td>No of Payments from Principal in the last 3 months</td>
                        <td colspane='3'>N/A</td>
                    </tr>
                   <tr>
                        <td>Discount Value (50%)</td>
                        <td colspane='3'>N/A</td>
                        
                    </tr> 
                   
                 ";
            result = result + $"</table>";
            return result;
        }

        public string cashCollaterizedCustomerInformationHtml()
        {
            var customerId = 0;
            TBL_CUSTOMER customer = new TBL_CUSTOMER();
            string address = string.Empty;
            string accountNumber = string.Empty;

            //var currentApplicationId = this.loanApplication.LOANAPPLICATIONID;
            //if (this.loanApplication == null)
            //{
            //    var d = context.TBL_LMSR_APPLICATION_DETAIL.Where(a => a.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID && a.DELETED == false).FirstOrDefault();
            //    currentApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
            //                         (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
            //                         (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault();

            //}

            if (this.loanApplication != null)
            {
                customerId = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.loanApplication.LOANAPPLICATIONID).Select(d => d.CUSTOMERID).FirstOrDefault();

            }
            else
            {
                customerId = context.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).Select(d => d.CUSTOMERID).FirstOrDefault();

            }

             customer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == customerId).FirstOrDefault();
             address = context.TBL_CUSTOMER_ADDRESS.Where(a => a.CUSTOMERID == customer.CUSTOMERID).Select(a => a.ADDRESS).FirstOrDefault();
             accountNumber = context.TBL_CASA.Where(c => c.CUSTOMERID == customer.CUSTOMERID).Select(c => c.PRODUCTACCOUNTNUMBER).FirstOrDefault();
            var riskRating = context.TBL_CUSTOMER_RISK_RATING.Find(customer.RISKRATINGID);

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4><br/>
                <h3><b>CREDIT PROGRAM SHEET (CASH COLLATERIZED)</b></h3>
                <br />
                <h4><b>Customer Information</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>
                    <tr>
                        <td>Borrower</td>
                        <td colspan='3'>{customer?.FIRSTNAME} {customer?.MIDDLENAME} {customer?.LASTNAME}</td>
                    </tr>
                   <tr>
                        <td>Location</td>
                        <td>{address}-</td>
                        <td>Customer Risk Rating</td>
                        <td>{customer?.CUSTOMERRATING}</td>
                    </tr> 
                     <tr>
                        <td>Business</td>
                        <td>{customer.OCCUPATION}</td>
                        <td>Classification</td>
                        <td>{riskRating?.CLASSIFICATION}</td>
                    </tr>
                   <tr>
                        <td>Account Number</td>
                        <td>{currentAccountNo}</td>
                        <td>Account Opening Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                    </tr> 
                     <tr>
                        <td>Incorporation Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                        <td>Biz Commencement Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                    </tr>
                   <tr>
                        <td>Principal Promoters</td>
                        <td colspane='3'>N/A</td>
                        
                    </tr> 

                    <tr>
                        <td>Contract Employer</td>
                        <td colspane='3'>N/A</td>
                    </tr> 
                     <tr>
                        <td>No of Payments from Principal in the last 3 months</td>
                        <td colspane='3'>N/A</td>
                    </tr>
                   <tr>
                        <td>Alternate Contract Employer</td>
                       <td colspane='3'>N/A</td>
                    </tr> 
                     <tr>
                        <td>No of Payments from Principal in the last 3 months</td>
                        <td colspane='3'>N/A</td>
                    </tr>
                   <tr>
                        <td>Discount Value (50%)</td>
                        <td colspane='3'>N/A</td>
                        
                    </tr> 
                   
                 ";
            result = result + $"</table>";
            return result;
        }

        public string NoncreditProgramCustomerInformationHtml()
        {
            var customerId = 0;
            //var currentApplicationId = this.loanApplication?.LOANAPPLICATIONID;
            //if (this.loanApplication == null)
            //{
            //    var d = context.TBL_LMSR_APPLICATION_DETAIL.Where(a => a.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID && a.DELETED == false).FirstOrDefault();
            //    currentApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
            //        (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ExternalFacility) ? (from p in context.TBL_LOAN_EXTERNAL join c in context.TBL_LMSR_APPLICATION_DETAIL on p.EXTERNALLOANID equals c.LOANID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select c.LOANID).FirstOrDefault() :
            //                         (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
            //                         (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault();

            //}
            if (this.loanApplication != null)
            {
                customerId = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.loanApplication.LOANAPPLICATIONID).Select(d=>d.CUSTOMERID).FirstOrDefault();
            }
            else
            {
                customerId = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).Select(d => d.CUSTOMERID).FirstOrDefault();
            }

                
            var customer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == customerId)?.FirstOrDefault();

                if (customer == null && this.customerRecord != null)
                {
                    customer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == this.customerRecord.CUSTOMERID).FirstOrDefault();
                }

                 var address = context.TBL_CUSTOMER_ADDRESS.Where(a => a.CUSTOMERID == customer.CUSTOMERID).Select(a => a.ADDRESS).FirstOrDefault();
                 var accountNumber = context.TBL_CASA.Where(c => c.CUSTOMERID == customer.CUSTOMERID).Select(c => c.PRODUCTACCOUNTNUMBER).FirstOrDefault();
                 var riskRating = context.TBL_CUSTOMER_RISK_RATING.Find(customer.RISKRATINGID);
            
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4><br/>
                <h3><b>ORIGINAL DOCUMENT (NON CREDIT PROGRAM)</b></h3>
                <br />
                <h4><b>Customer Information</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>
                    <tr>
                        <td>Borrower</td>
                        <td colspan='3'>{customer?.FIRSTNAME} {customer?.MIDDLENAME} {customer?.LASTNAME}</td>
                    </tr>
                   <tr>
                        <td>Location</td>
                        <td>{address}-</td>
                        <td>Customer Risk Rating</td>
                        <td>{customer?.CUSTOMERRATING}</td>
                    </tr> 
                     <tr>
                        <td>Business</td>
                        <td>{customer?.OCCUPATION}</td>
                        <td>Classification</td>
                        <td>{riskRating?.CLASSIFICATION}</td>
                    </tr>
                   <tr>
                        <td>Account Number</td>
                        <td>{currentAccountNo}</td>
                        <td>Account Opening Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                    </tr> 
                     <tr>
                        <td>Incorporation Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                        <td>Biz Commencement Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                    </tr>
                   <tr>
                        <td>Principal Promoters</td>
                        <td colspane='3'>N/A</td>
                        
                    </tr> 

                    <tr>
                        <td>Contract Employer</td>
                        <td colspane='3'>N/A</td>
                    </tr> 
                     <tr>
                        <td>No of Payments from Principal in the last 3 months</td>
                        <td colspane='3'>N/A</td>
                    </tr>
                   <tr>
                        <td>Alternate Contract Employer</td>
                       <td colspane='3'>N/A</td>
                    </tr> 
                     <tr>
                        <td>No of Payments from Principal in the last 3 months</td>
                        <td colspane='3'>N/A</td>
                    </tr>
                   <tr>
                        <td>Discount Value (50%)</td>
                        <td colspane='3'>N/A</td>
                        
                    </tr> 
                   
                 ";
            result = result + $"</table>";
            return result;
        }
        public string CreditProgramCustomerInformationHtml()
        {
            var customerId = 0;
            //var currentApplicationId = this.loanApplication.LOANAPPLICATIONID;
            //if (this.loanApplication == null)
            //{
            //    var d = context.TBL_LMSR_APPLICATION_DETAIL.Where(a => a.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID && a.DELETED == false).FirstOrDefault();
            //    currentApplicationId = (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility) ? (from p in context.TBL_LOAN join c in context.TBL_LMSR_APPLICATION_DETAIL on p.TERMLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
            //                         (d.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability) ? (from p in context.TBL_LOAN_CONTINGENT join c in context.TBL_LMSR_APPLICATION_DETAIL on p.CONTINGENTLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault() :
            //                         (from p in context.TBL_LOAN_REVOLVING join c in context.TBL_LMSR_APPLICATION_DETAIL on p.REVOLVINGLOANID equals c.LOANID join l in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID join aa in context.TBL_LOAN_APPLICATION on l.LOANAPPLICATIONID equals aa.LOANAPPLICATIONID where c.LOANREVIEWAPPLICATIONID == d.LOANREVIEWAPPLICATIONID select aa.LOANAPPLICATIONID).FirstOrDefault();

            //}
            if (this.loanApplication != null)
            {
                customerId = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.loanApplication.LOANAPPLICATIONID).Select(d => d.CUSTOMERID).FirstOrDefault();
            }
            else
            {
                customerId = context.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).Select(d => d.CUSTOMERID).FirstOrDefault();
            }
            var customer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == customerId).FirstOrDefault();
            var address = context.TBL_CUSTOMER_ADDRESS.Where(a => a.CUSTOMERID == customer.CUSTOMERID).Select(a => a.ADDRESS).FirstOrDefault();
            var accountNumber = context.TBL_CASA.Where(c => c.CUSTOMERID == customer.CUSTOMERID).Select(c => c.PRODUCTACCOUNTNUMBER).FirstOrDefault();
            var riskRating = context.TBL_CUSTOMER_RISK_RATING.Find(customer.RISKRATINGID);

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4><br/>
                <h3><b>ORIGINAL DOCUMENT (CREDIT PROGRAM)</b></h3>
                <br />
                <h4><b>Customer Information</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>
                    <tr>
                        <td>Borrower</td>
                        <td colspan='3'>{customer?.FIRSTNAME} {customer?.MIDDLENAME} {customer?.LASTNAME}</td>
                    </tr>
                   <tr>
                        <td>Location</td>
                        <td>{address}-</td>
                        <td>Customer Risk Rating</td>
                        <td>{customer?.CUSTOMERRATING}</td>
                    </tr> 
                     <tr>
                        <td>Business</td>
                        <td>{customer?.OCCUPATION}</td>
                        <td>Classification</td>
                        <td>{riskRating?.CLASSIFICATION}</td>
                    </tr>
                   <tr>
                        <td>Account Number</td>
                        <td>{currentAccountNo}</td>
                        <td>Account Opening Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                    </tr> 
                     <tr>
                        <td>Incorporation Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                        <td>Biz Commencement Date</td>
                        <td>{customer.DATEOFBIRTH?.ToString("dd-MM-yyyy")}</td>
                    </tr>
                   <tr>
                        <td>Principal Promoters</td>
                        <td colspane='3'>N/A</td>
                        
                    </tr> 

                    <tr>
                        <td>Contract Employer</td>
                        <td colspane='3'>N/A</td>
                    </tr> 
                     <tr>
                        <td>No of Payments from Principal in the last 3 months</td>
                        <td colspane='3'>N/A</td>
                    </tr>
                   <tr>
                        <td>Alternate Contract Employer</td>
                       <td colspane='3'>N/A</td>
                    </tr> 
                     <tr>
                        <td>No of Payments from Principal in the last 3 months</td>
                        <td colspane='3'>N/A</td>
                    </tr>
                   <tr>
                        <td>Discount Value (50%)</td>
                        <td colspane='3'>N/A</td>
                        
                    </tr> 
                   
                 ";
            result = result + $"</table>";
            return result;
        }

        public string IdfCustomerFacilityHtml()
        {
            var result = String.Empty;
            result = result + FussCustomerFacilityHtml();
            return result;
        }
        public string IdfCustomerAccountActivityHtml()
        {
            var result = String.Empty;
            result = result + FussCustomerAccountActivityHtml();
            return result;
        }
        public string IdfCurrentRequestHtml()
        {
            var result = String.Empty;
            result = result + FussCurrentRequestHtml();
            return result;
        }
        public string IdfBackgroungInformationHtml()
        {
            var result = String.Empty;
            result = result + FussBackgroungInformationHtml();
            return result;
        }
        public string IdfChecklistEligibilityHtml()
        {
            var result = String.Empty;
            result = result + FussChecklistEligibilityHtml();
            return result;
        }
        public string IdfDocumentationChecklistHtml()
        {
            var result = String.Empty;
            result = result + $@"
                <br />
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th colspan='4'><strong>Documentation Checklist</strong></th>                     
                    </tr> 
                   <tr>
                        <td>Deed of assignment for 100% contracts proceeds to Access Bank (where obtainable)</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Approved contract OR purchase order, stating bank A/C details.</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>  
                   <tr>
                        <td>Vendor’s letter to Principal requesting domiciliation of contract proceeds to Access Bank, stating that the instruction cannot be varied without the express consent of Access Bank.</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Domiciliation letter OR Proof that domiciliation has worked in the past (in cases where the principal is unwilling to accept domiciliation)</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>  
                   
                    <tr>
                        <td>A copy of the final invoice(s) and/or an original waybill, stamped ‘received’ by the Principal indicating the bank details. </td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>A formal client’s request for draw-down, stating relevant account details.</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Evidence of other receivables domiciled to us other the one being discounted</td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
               <br />";
            return result;
        }

        public string CashCollaterizedHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4><br/>
                <h3><b>CREDIT PROGRAM SHEET (CASH COLLATERIZED)</b></h3>
                <br />
                <h4><b>Customer Information</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td>Borrower</td>
                        <td colspan='3'>------------------------------------------</td>
                    </tr>
                   <tr>
                        <td>Location</td>
                        <td>------------------------------------------</td>
                        <td>Customer Risk Rating</td>
                        <td>-------------------</td>
                    </tr> 
                     <tr>
                        <td>Business</td>
                        <td>------------------------------------------</td>
                        <td>Classification</td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>Account Number</td>
                        <td>------------------------------------------</td>
                        <td>Account Opening Date</td>
                        <td>-------------------</td>
                    </tr> 
                     <tr>
                        <td>Incorporation Date</td>
                        <td>------------------------------------------</td>
                        <td>Biz Commencement Date</td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>Principal Promoters</td>
                        <td colspane='3'>------------------------------------------</td>
                        
                    </tr>    
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Customer Facilities as @ xx/xx/xxxx</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility</b></th>
                        <th><b>Amount (‘000)</b></th>
                        <th><b>Maturity</b></th>
                        <th><b>Security</b></th>
                        <th><b>Performing</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>12 Months Activity with Current (Major) Banker</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th colspan='2'><b>Month</b></th>
                        <th><b>Debits</b></th>
                        <th><b>Credits</b></th>
                        <th><b>Returned Cheque</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                       <td colspan='2'>Current Book Balance</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'>Average Monthly Credit Turnover</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'><b>Other Bankers/Age</b></td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'>Existing Facility Type/Maturity</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                     <tr>
                       <td colspan='2'>Security/Support</td>
                        <td colspan='3'>------------------------------------------------------</td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>CURRENT REQUEST:</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Facility Type:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Facility Amount:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Purpose:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Tenor:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Repayment Plan</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Price:</strong></td>
                        <td><table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Interest Rate:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Management Fees:</strong></td>
                        <td></td>
                        </tr>
                         <tr>
                        <td><strong>COT</strong></td>
                        <td></td>
                        </tr>
                        </table></td>
                        </tr>
                        <tr>
                        <td><strong>Security/Support:</strong> </td>
                        <td><ul><li>Cash Collateral in the currency of obligation. In cases where cash collateral is in a currency other than obligation currency, facility shall not exceed 90% of cash collateral. </li> 
                                <li>At maturity of facility, cash collateral should be liquidated into the account to clean up any shortfall in account position.</li>           
                                </ul>
                        </td>
                        </tr>
                        </table>
                        </td>
                    </tr> 

                     <tr>
                        <td><strong>BACKGROUND INFORMATION ON THE OBLIGOR</strong> (including the mitigation of all risks analyzed in the credit program as well as any identified risk peculiar to the obligor).</td>
                    </tr>
                     <tr>
                        <td><strong>ATTESTATION:</strong><br/> 
                            I, ……………………………...attest to the integrity of the Promoter................................having known him/her for at least ........years. 
	
                            <br/><strong>Signature & Date</strong>	
                        </td>
                    </tr> 
                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />";
            
            result = result + $@"
                <br />
                <h4><b>CHECKLIST / ELIGIBILITY</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     
                   <tr>
                        <td colspan='4'><strong>TARGET MARKET SCREENING CRITERIA</strong></td>                     
                    </tr> 
                    <tr>
                        <td></td>
                        <td><strong>Required</strong></td>
                        <td><strong>Actual</strong></td>
                        <td><strong>Exception (Y/N)</strong></td>
                    </tr> 
                   <tr>
                        <td>Minimum years in business</td>
                        <td>5</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Current KYC doc</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>100% cash collateral or </td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Satisfactory Trade or Bank Checking/CBN CRMS system</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr>                                      
                     <tr>
                        <td colspan='4'><strong>RISK ACCEPTANCE CRITERIA</strong></td>
                    </tr> 
                    <tr>
                        <td>Facility Maximum Tenor</td>
                        <td>5 Years</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Facility tenor not longer than cash collateral tenor</td>
                        <td>Yes</td>
                        <td></td>
                        <td>No Deviation Allowed</td>
                    </tr>                                                   
                     <tr>
                        <td colspan='4'><strong>RECOMMENDATION:</strong></td>
                        
                    </tr> 
                    <tr>
                        <td colspan='4'>Based on the foregoing, we hereby recommend………………………..</td>                   
                    </tr>                                    
                   ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th colspan='4'><strong>DOCUMENTATION CHECKLIST</strong></th>                     
                    </tr> 
                   <tr>
                        <td>Offer Letter</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Board Resolution accepting offer by persons authorized by the Board of Directors</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr>  
                   <tr>
                        <td>Letter of Lien/Set Off</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Other Documents as may be required for each product</td>
                        <td>In Place</td>
                        <td></td>
                        <td></td>
                    </tr>  
                   
                    <tr>
                        <td>Overdrafts: 90% of cash collateral (where interest and fees are paid upfront), otherwise 80%</td>
                        <td>90%</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Bonds & Guarantees: 100% of cash collateral provided all fees are paid.</td>
                        <td>100%</td>
                        <td></td>
                        <td></td>
                    </tr>                   
                 ";
            result = result + $"</table>";
            result = result + $@"
               <br />";
            return result;
        }
       
        public string TemporaryOverdraftHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <h3><b>MEMO</b></h3>
                <br />
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td colspan=2><strong>TEMPORARY OVERDRAFT (TOD)</strong></td>
                        <td></td>
                        <td><strong>Date:</strong></td>
                        <td>{DateTime.UtcNow}</td>                
                    </tr>
                     <tr>
                        <td><strong>Unit:</strong></td>
                        <td>Originating Unit</td>
                        <td><strong>Prepared By:</strong></td>
                        <td>Account Officer</td>                
                    </tr>
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
               
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>        
                   <tr>
                        <td>Name of Customer:</td>
                        <td>XXXXXXXX</td>
                    </tr> 
                    <tr>
                        <td>Nature of Business:</td>
                        <td>XXXXXXXX</td>
                    </tr>  
                   <tr>
                        <td>Promoter/M.D. of Company:</td>
                        <td>XXXXXXXX</td>
                    </tr> 
                    <tr>
                        <td>Account No:</td>
                        <td>XXXXXXXX</td>
                    </tr> 
                     <tr>
                        <td>Book Balance:</td>
                        <td>XXXXXXXX</td>
                    </tr> 
                     <tr>
                        <td>Available Balance:</td>
                        <td>XXXXXXXX</td>
                    </tr> 
                     <tr>
                        <td>Unavailable Balance: </td>
                        <td>XXXXXXXX</td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     
                   <tr>
                        <td><strong>New Request:</strong></td>
                        <td>New</td>
                        <td><strong>Amount:</strong></td>
                        <td>N XXXXXXXX TOD</td>
                    </tr> 
                    <tr>
                        <td><strong>Interest Rate:</strong></td>
                        <td>X p.a.</td>
                        <td><strong>Tenor:</strong></td>
                        <td>Maximum of 30 days in a year</td>
                    </tr> 

                   <tr>
                        <td><strong>Fee:</strong></td>
                        <td>As approved</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td><strong>Reason for this request:</strong></td>
                        <td colspan=3></td>
                    </tr> 
                    
                 ";
            result = result + $"</table>";          
            result = result + $@"
                <br />
                <h4><b>3 MONTHS ACTIVITY:</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                   <tr>
                        <td>Month</td>
                        <td>Debits</td>
                        <td>Credits</td>
                    </tr> 
                   <tr>
                        <td>Jul</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Aug</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Sept</td>
                        <td></td>
                        <td></td>
                    </tr>  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4>Collateral/Support/Justification:</h4>
                <p><ul><li>List the supporting documents and their perfection status.</li></ul></p>
                <br />";
           

            result = result + $@"
                <br />
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                   <tr>
                        <td COLSPAN=2>ATTESTATION: I hereby undertake to sponsor the TOD based on my expert knowledge of the customer and his business, and state that I would be personally responsible in ensuring repayment in line with approved terms.</td>
                        
                    </tr> 
                    <tr>
                        <td><strong>Signature & Date<strong></td>
                        <td>__________________</td>
                    </tr>                  
                 ";
            result = result + $"</table>";    
            return result;
            
        }
        public string TodHeaderHtml()
        {
            string staff = string.Empty;
            string branch = string.Empty;
            if (this.isThirdPartyFacility == false)
            {
                if (this.loanApplication != null)
                {
                    staff = context.TBL_STAFF.Where(s => s.STAFFID == this.loanApplication.OWNEDBY).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                    branch = context.TBL_BRANCH.Where(s => s.BRANCHID == this.loanApplication.BRANCHID).Select(s => s.BRANCHNAME).FirstOrDefault();
                }
                else
                {
                    staff = context.TBL_STAFF.Where(s => s.STAFFID == this.lmsrApplication.CREATEDBY).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                    branch = context.TBL_BRANCH.Where(s => s.BRANCHID == this.lmsrApplication.BRANCHID).Select(s => s.BRANCHNAME).FirstOrDefault();
                }
            
            }
            else
            {
                if (this.loanApplication != null)
                {
                    staff = context.TBL_STAFF.Where(s => s.STAFFID == this.loanApplication.OWNEDBY).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                    branch = context.TBL_BRANCH.Where(s => s.BRANCHID == this.loanApplication.BRANCHID).Select(s => s.BRANCHNAME).FirstOrDefault();
                }
                else
                {
                    staff = context.TBL_STAFF.Where(s => s.STAFFID == this.lmsrApplication.CREATEDBY).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                    branch = context.TBL_BRANCH.Where(s => s.BRANCHID == this.lmsrApplication.BRANCHID).Select(s => s.BRANCHNAME).FirstOrDefault();
                }
            }
            
            
            var result = String.Empty;
            result = result + $@"
                <h3><b>MEMO</b></h3>
                <br />
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td colspan=2><strong>TEMPORARY OVERDRAFT (TOD)</strong></td>
                        <td></td>
                        <td><strong>Date:</strong></td>
                        <td>{DateTime.UtcNow.ToString("dd-mm-yyyy")}</td>                
                    </tr>
                     <tr>
                        <td><strong>Unit:</strong></td>
                        <td>{branch}</td>
                        <td><strong>Prepared By:</strong></td>
                        <td>{staff}</td>                
                    </tr>
                 ";
            result = result + $"</table>";
            return result;

        }
        public string TodCustomerInformationHtml()
        {
            int customerId = 0;
            if (this.isThirdPartyFacility == false)
            {
                if (this.loanApplication != null)
                {
                    var app = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.loanApplication.LOANAPPLICATIONID).FirstOrDefault();
                    customerId = app.CUSTOMERID;
                }
                else
                {
                    var app = context.TBL_LMSR_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).FirstOrDefault();
                    customerId = app.CUSTOMERID;
                }
            }
            else { customerId = this.customerRecord.CUSTOMERID; }

            var customer = context.TBL_CUSTOMER.Where(a => a.CUSTOMERID == customerId).FirstOrDefault();
            var address = context.TBL_CUSTOMER_ADDRESS.Where(a => a.CUSTOMERID == customer.CUSTOMERID).Select(a => a.ADDRESS).FirstOrDefault();
            var accountNumber = context.TBL_CASA.Where(c => c.CUSTOMERID == customer.CUSTOMERID).Select(c => c.PRODUCTACCOUNTNUMBER).FirstOrDefault();
            var availableBalance = context.TBL_CASA.Where(c => c.CUSTOMERID == customer.CUSTOMERID).Select(c => c.AVAILABLEBALANCE).FirstOrDefault();
            var bookBalance = context.TBL_CASA.Where(c => c.CUSTOMERID == customer.CUSTOMERID).Select(c => c.LEDGERBALANCE).FirstOrDefault();
            var unAvailableBalance = bookBalance - availableBalance;
            var availableBalanceFormat = string.Format("{0:#,##.00}", Convert.ToDecimal(availableBalance));
            var bookBalanceFormat = string.Format("{0:#,##.00}", Convert.ToDecimal(bookBalance));
            var unAvailableBalanceFormat = string.Format("{0:#,##.00}", Convert.ToDecimal(unAvailableBalance));

            var result = String.Empty;
                result = result + $@"
                <br />
               
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>  
                        <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>
                   <tr>
                        <td>Name of Customer:</td>
                        <td>{customer?.FIRSTNAME} {customer?.MIDDLENAME} {customer?.LASTNAME}</td>
                    </tr> 
                    <tr>
                        <td>Nature of Business:</td>
                        <td>{customer.OCCUPATION}</td>
                    </tr>  
                   <tr>
                        <td>Promoter/M.D. of Company:</td>
                        <td>N/A</td>
                    </tr> 
                    <tr>
                        <td>Account No:</td>
                        <td>{accountNumber}</td>
                    </tr> 
                     <tr>
                        <td>Book Balance:</td>
                        <td>{bookBalanceFormat}</td>
                    </tr> 
                     <tr>
                        <td>Available Balance:</td>
                        <td>{availableBalanceFormat}</td>
                    </tr> 
                     <tr>
                        <td>Unavailable Balance: </td>
                        <td>{unAvailableBalanceFormat}</td>
                    </tr> 
                 ";
            result = result + $"</table>";
            return result;

        }
        public string TodCurrentRequestHtml()
        {
            var result = String.Empty;
            
            result = result + FussCurrentRequestHtml();
            return result;

        }
        public string TodCustomerAccountActivityHtml()
        {
            var result = String.Empty;
            result = result + FussCustomerAccountActivityHtml();
            return result;

        }
        public string TodCustomerFacilityHtml()
        {
            var result = String.Empty;
            
            result = result + FussCustomerFacilityHtml();
            return result;

        }
        public string TodBackgroungInformationHtml()
        {
            var result = String.Empty;
            result = result + $@"
                <br />
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                   <tr>
                        <td COLSPAN=2>ATTESTATION: I hereby undertake to sponsor the TOD based on my expert knowledge of the customer and his business, and state that I would be personally responsible in ensuring repayment in line with approved terms.</td>
                        
                    </tr> 
                    <tr>
                        <td><strong>Signature & Date<strong></td>
                        <td>__________________</td>
                    </tr>                  
                 ";
            result = result + $"</table>";
            return result;

        }
        public string CurrentLMSFlowHtml()
        {
            var currentOperation = context.TBL_OPERATIONS.Where(l => l.OPERATIONID == this.lmsrApplication.OPERATIONID).Select(l => l.OPERATIONNAME).FirstOrDefault();
            var lmsReviewDetails = context.TBL_LMSR_APPLICATION_DETAIL.Where(r => r.LOANAPPLICATIONID == this.lmsrApplication.LOANAPPLICATIONID).Select(r => r.REVIEWDETAILS).FirstOrDefault();
            
            var result = String.Empty;
            if (this.lmsrApplication != null)
            {
                result = result + $@"
                <br />
                <h4><strong>CURRENT PROCESS DETAILS</strong></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                   <tr>
                        <td>Process Type:</td>
                         <td>{currentOperation}</td>
                        <td>Review Detail:</td>
                         <td>{lmsReviewDetails}</td>
                    </tr> 
                 ";
                result = result + $"</table>";
            }
            return result;

        }
        public string StaffCarLoansHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4>
                <h3><b>Staff Car Loan Scheme – Facility Approval Memo</b></h3>
                <br />
                <h3><b>Staff Information</b></h3>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td>Staff’s Name</td> 
                        <td>------------------</td>
                        <td>Level/ Designation: </td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>Supervisor’s Name:</td>
                        <td>-----------------</td>
                        <td>Level/ Designation</td>
                        <td>-------------------</td>
                    </tr> 
                     <tr>
                        <td>Borrower’s Residential Address</td>
                        <td colspan=3>-----------------</td>
                       
                    </tr>
                   <tr>
                        <td>Branch / Unit:</td>
                        <td>-----</td>
                        <td>Group: </td>
                        <td>Division: </td>                     
                    </tr> 
                     <tr>
                        <td>Date of Employment:</td>
                        <td>---------------------</td>
                        <td>Employment Status</td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>No. Of years in Access Bank:</td>
                        <td>----------------</td>
                        <td>Total No. of years in Employment</td>
                        <td>---------------</td>
                        
                    </tr>    
                    <tr>
                        <td>Account Number:</td>
                        <td>-------------</td>
                        <td>Loan Tenor: ---Five (5) Years</td>
                        <td>--------------</td>
                    </tr>
                   <tr>
                        <td>Car Amount:</td>
                        <td>₦------------------------</td>
                        <td>(Amount in words</td>
                        <td>-----------------------)</td>
                        
                    </tr>    
                     <tr>
                        <td>Car Loan Limit:</td>
                        <td>------------------</td>
                        <td>(Amount in words</td>
                        <td>₦-------------------)</td>
                    </tr>
                   <tr>
                        <td>Facility Amount:</td>
                        <td>₦-----------------------</td>
                        <td>(Amount in words</td>
                        <td>-----------------------)</td>
                        
                    </tr>    
                     <tr>
                        <td><strong>Sex:</strong></td>
                        <td><input type='checkbox' name='Male' value='Male'> Male
                            <input type='checkbox' name='Female' value='Female'> Female</td>
                        <td>Email Address:</td>
                        <td>-----------------</td>
                    </tr>
                   <tr>
                        <td></td>
                        <td></td>
                        <td valign='top'>Office</td>
                        <td valign='top'>Personal</td>
                        
                    </tr>  
                    <tr>
                        <td>Telephone numbers:</td>
                        <td>-----------------</td>
                        <td>-----------------</td>
                        <td>-----------------</td>
                        
                    </tr> 
                     <tr>
                        <td></td>
                        <td valign='top'>Office</td>
                        <td valign='top'>Home</td>
                        <td valign='top'>Mobile</td>
                        
                    </tr>  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>To be completed by HR Only</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td>Staff Rating (Key Talent, A*, A, B, C, D):</td>
                        <td>---------------</td>
                        <td colspan=2>(indicate OK or NOT OK)</td>
                    </tr> 
                    <tr>
                        <td>Basic Salary:</td>
                        <td>---------------------------------</td>
                        <td>Debt Service Ratio (including new loan)</td>
                        <td>-------------%</td>               
                    </tr>  
                   <tr>
                       <td>Status of Employment ((Un)Confirmed):</td>
                        <td>----------------------------</td>
                        <td><strong>Age:</strong></td>
                        <td>---------------</td>               
                    </tr> 
                    <tr>
                       <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td valign='top'>(Indicate OK or NOT OK)</td>
                    </tr> 
                    <tr>
                       <td colspan=3>Disciplinary case (Warning Letter / Suspension in the past six (6) months):</td>
                        <td>Yes<input type='checkbox' name='Yes' value='Yes'> 
                            No<input type='checkbox' name='No' value='No'></td>
                    </tr> 
                    <tr>
                        <td>Fast Track:</td>
                        <td>Yes<input type='checkbox' name='Yes' value='Yes'> 
                            No<input type='checkbox' name='No' value='No'></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Existing Facilities with Access Bank as at: -------------------------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Outstanding Amount (‘000)</b></th>
                        <th><b>Maturity Date</b></th>
                        <th><b>Security</b></th>
                        <th><b>Monthly Repayment</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Loan History (Facilities already Paid Down only) as at: --------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Approved Amount</b></th>
                        <th><b>Date of Last Repayment</b></th>
                        <th><b>Security</b></th>
                   </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Analysis of Six (6) Months Bank Statement</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Month</b></th>
                        <th><b>Debit</b></th>
                        <th><b>Credit</b></th>
                        <th><b>EOM Balance</b></th>
                        <th><b>Returned Cheques</b></th>
                   </tr> 
                   <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>   
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td><strong>Total</strong></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td><strong>Average</strong></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Existing Facilities with Other Banks as at: -------------------------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Outstanding Amount (‘000)</b></th>
                        <th><b>Maturity Date</b></th>
                        <th><b>Security</b></th>
                        <th><b>Monthly Repayment</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>                     
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>CURRENT REQUEST:</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Facility Type:</strong></td>
                        <td>Car Loan</td>
                        </tr>
                        <tr>
                        <td><strong>Facility Amount:</strong></td>
                        <td>NGN</td>
                        </tr>
                        <tr>
                        <td><strong>Purpose:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Tenor:</strong></td>
                        <td>5 years (60 months)</td>
                        </tr>
                        <tr>
                        <td><strong>Repayment Plan</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Interest Rate:</strong></td>
                        <td>5% p.a.</td>
                        </tr>
                        <tr>
                        <td><strong>Positive Credit Check Reports:</strong> </td>
                        <td>CRMS and two (2) other Credit Bureaus
                        </td>
                        </tr>
                        <tr>
                        <td><strong>Disbursement:</strong> </td>
                        <td><ul><li>Original Car particulars and spare key Staff’s salaries, allowances and other emoluments. </li> 
                                <li>Comprehensive Insurance Cover on Car financed with The Bank noted as First Loss Payee.</li>           
                                </ul>
                        </td>
                        </tr>
                        </table>
                        </td>
                    </tr> 

                     <tr>
                        <td><strong>Background Information On The Obligor:</strong> </td>
                    </tr>
                    <tr>
                        <td><strong>Current Request:</strong> </td>
                    </tr>
                    <tr>
                        <td><strong>Rationale for Current Request:</strong> </td>
                    </tr>
                     <tr>
                        <td><strong><hr>Conditions Precedent To Drawdown:</strong><br/> 
                           Conditions precedent to drawdown shall include but shall not be limited to the following
                            <ul>                           
                                <li>Duly approved Staff Car Loan FAM.</li>
                                <li> Duly accepted Offer Letter</li>
                                <li> Pro-forma invoice obtained from an acceptable vendor and duly endorsed by designated officer in Procurement Unit.</li>
                                <li>Signed letter of set-off, letter of undertaking and irrevocable transfer of ownership.</li>
                                <li>Positive Credit Check Report from CRMS and two (2) Credit Bureaus.</li>
                                <li> Where required, Equitable Contribution must be in place prior to loan drawdown</li>
                            </ul>
                        </td>
                    </tr> 
                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Risk Acceptance Criteria (RAC) – Staff Car Loans</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Description</b></th>
                        <th><b>Required</b></th>
                        <th><b>Actual</b></th>
                        <th><b>Exception (Y/N)</b></th>
                    </tr> 
                   <tr>
                        <td>Minimum years as Access Bank staff </td>
                        <td>Upon confirmation</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Maximum Loan Amount</td>
                        <td>4 times staff annual basic salary subject to 33.33 DSR.</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Global Limit</td>
                        <td>Aggregate value of all staff loan types shall not exceed 2.5% of the Bank’s total loan portfolio.</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Employment Status</td>
                        <td>Confirmed</td>
                        <td></td>
                        <td></td>
                    </tr>   
                     <tr>
                        <td>Qualifying Grade</td>
                        <td>ET and above</td>
                        <td></td>
                        <td></td>
                    </tr>  
                   <tr>
                        <td>Minimum Appraisal Rating</td>
                        <td>“B”</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Overall Debt Service Ratio (DSR)</td>
                        <td>33.33% of staff’s basic income</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td>Good Credit Checks</td>
                        <td>From CRMS & 2 other Credit Bureaus</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Maximum Tenor</td>
                        <td>60 Months</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Car Loan within the last 60 months</td>
                        <td>None</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Nature of employment</td>
                        <td>Professional staff</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Line ED’s Approval</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";

            result = result + $@"
                <br />
                ";
            return result;
        }
        public string StaffMortgageLoansHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4>
                <h3><b>Staff Mortgage Loan Scheme – Facility Approval Memo</b></h3>
                <br />
                <h3><b>Staff Information</b></h3>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td>Staff’s Name</td> 
                        <td>------------------</td>
                        <td>Level/ Designation: </td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>Supervisor’s Name:</td>
                        <td>-----------------</td>
                        <td>Level/ Designation</td>
                        <td>-------------------</td>
                    </tr> 
                     <tr>
                        <td>Borrower’s Residential Address</td>
                        <td colspan=3>--------------------</td>
                       
                    </tr>
                   <tr>
                        <td>Branch / Unit:</td>
                        <td>--------------</td>
                        <td>Group:---------------</td>
                        <td>Division:-------------</td>
                    </tr> 
                     <tr>
                        <td>Date of Employment:</td>
                        <td>-------------------</td>
                        <td>Employment Status</td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>No. Of years in Access Bank:</td>
                        <td>-------------------</td>
                        <td>Total No. of years in Employment</td>
                        <td>--------------------</td>
                        
                    </tr>    
                    <tr>
                        <td>Account Number:</td>
                        <td>----------------------</td>
                        <td>Loan Tenor (max 10 years):</td>
                        <td>----------------Years</td>
                    </tr>
                   <tr>
                        <td>Property Value:</td>
                        <td>₦------------------------</td>
                        <td>(Amount in words</td>
                        <td>-----------------------)</td>
                        
                    </tr>    
                     <tr>
                        <td>Mortgage Entitlement:</td>
                        <td>₦--------------------------</td>
                        <td>(Amount in words</td>
                        <td>-------------------)</td>
                    </tr>
                   <tr>
                        <td>Facility Amount:</td>
                        <td>₦-----------------------</td>
                        <td>(Amount in words</td>
                        <td>-----------------------)</td>
                        
                    </tr>  
                    <tr>
                        <td>Agency Fee @ 5%:</td>
                        <td>₦-----------------------</td>
                        <td>(Amount in words</td>
                        <td>-----------------------)</td>
                        
                    </tr>  
                     <tr>
                        <td><strong>Sex:</strong></td>
                        <td><input type='checkbox' name='Male' value='Male'> Male
                            <input type='checkbox' name='Female' value='Female'> Female</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>Telephone numbers:</td>
                        <td>-----------------------</td>
                        <td>-----------------------</td>
                        <td>-----------------------</td>
                        
                    </tr> 
                     <tr>
                        <td></td>
                        <td valign='top'>Office</td>
                        <td valign='top'>Home</td>
                        <td valign='top'>Mobile</td>
                        
                    </tr> 
                     <tr>
                        <td>eMail Address:</td>
                        <td>-----------------------</td>
                        <td></td>
                        <td>-----------------------</td>
                        
                    </tr> 
                   <tr>
                        <td></td>
                        <td valign='top'>Office</td>
                        <td></td>
                        <td valign='top'>Personal</td>
                        
                    </tr>  
                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>To be completed by HR Only</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td>Staff Rating (Key Talent, A*, A, B, C, D):</td>
                        <td>---------------</td>
                        <td colspan=2>(indicate OK or NOT OK)</td>
                    </tr> 
                    <tr>
                        <td>Basic Salary:</td>
                        <td>---------------------------------</td>
                        <td>Debt Service Ratio (including new loan)</td>
                        <td>-------------%</td>               
                    </tr>  
                   <tr>
                       <td>Status of Employment ((Un)Confirmed):</td>
                        <td>----------------------------</td>
                        <td><strong>Age:</strong></td>
                        <td>---------------</td>               
                    </tr> 
                    <tr>
                       <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td valign='top'>(Indicate OK or NOT OK)</td>
                    </tr> 
                    <tr>
                       <td colspan=3>Disciplinary case (Warning Letter / Suspension in the past six (6) months):</td>
                        <td>Yes<input type='checkbox' name='Yes' value='Yes'> 
                            No<input type='checkbox' name='No' value='No'></td>
                    </tr> 
                    <tr>
                        <td>Fast Track:</td>
                        <td>Yes<input type='checkbox' name='Yes' value='Yes'> 
                            No<input type='checkbox' name='No' value='No'></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Existing Facilities with Access Bank as at: -------------------------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Outstanding Amount (‘000)</b></th>
                        <th><b>Maturity Date</b></th>
                        <th><b>Security</b></th>
                        <th><b>Monthly Repayment</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Loan History (Facilities already Paid Down only) as at: --------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Approved Amount</b></th>
                        <th><b>Date of Last Repayment</b></th>
                        <th><b>Security</b></th>
                   </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Analysis of Six (6) Months Bank Statement</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Month</b></th>
                        <th><b>Debit</b></th>
                        <th><b>Credit</b></th>
                        <th><b>EOM Balance</b></th>
                        <th><b>Returned Cheques</b></th>
                   </tr> 
                   <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>   
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td><strong>Total</strong></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td><strong>Average</strong></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Existing Facilities with Other Banks as at: -------------------------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Outstanding Amount (‘000)</b></th>
                        <th><b>Maturity Date</b></th>
                        <th><b>Security</b></th>
                        <th><b>Monthly Repayment</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>                     
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Facility Type:</strong></td>
                        <td>Mortgage Loan</td>
                        </tr>
                        <tr>
                        <td><strong>Facility Amount:</strong></td>
                        <td>NGN</td>
                        </tr>
                        <tr>
                        <td><strong>Purpose:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Tenor:</strong></td>
                        <td>10 years (120 months)</td>
                        </tr>
                        <tr>
                        <td><strong>Repayment Plan</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Interest Rate:</strong></td>
                        <td>5% per annum in line with policy.</td>
                        </tr>
                        <tr>
                        <td><strong>Positive Credit Check Reports:</strong> </td>
                        <td>Must be obtained from CRMS and two (2) other Credit Bureaus
                        </td>
                        </tr>
                        <tr>
                        <td><strong>Security/Support:</strong> </td>
                        <td>
                        </td>
                        </tr>
                        </table>
                        </td>
                    </tr> 

                     <tr>
                        <td><strong>Background Information On The Obligor:</strong> </td>
                    </tr>
                    <tr>
                        <td><strong>Current Request:</strong> </td>
                    </tr>
                    <tr>
                        <td><strong>Rationale for Current Request:</strong> </td>
                    </tr>
                     <tr>
                        <td><strong><hr>Conditions Precedent To Drawdown:</strong><br/> 
                           Conditions precedent to drawdown shall include but shall not be limited to the following
                            <ul>                           
                                <li>Duly approved Staff Mortgage Loan Facility Approval Memo (FAM).</li>
                                <li>Duly accepted Offer Letter</li>
                                <li>All mortgage documents required to place Legal mortgage on the property financed.</li>
                                <li>Signed letter of set-off</li>
                                <li>Positive Credit Check Report from CRMS and two (2) other Credit Bureaus.</li>
                                <li>Drawdown memo must be approved by CRM – PBD, The Legal Team in CRM, Drawdown Team and Treasury.</li>
                            </ul>
                        </td>
                    </tr>                   
            ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Risk Acceptance Criteria – Staff Mortgage Loan</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Description</b></th>
                        <th><b>Required</b></th>
                        <th><b>Actual</b></th>
                        <th><b>Exception (Y/N)</b></th>
                    </tr> 
                   <tr>
                        <td>Minimum years as Access Bank staff </td>
                        <td>24 Months</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Maximum Loan Amount</td>
                        <td>10 times annual staff Housing Allowance subject to 33.33% Debt Service ratio and retirement age of 60</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Global Limit</td>
                        <td>Aggregate value of all staff loan types shall not exceed 2.5% of the Bank’s total loan portfolio.</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td>Qualifying Age</td>
                        <td>55 years or lower (facility maturity shall not exceed retirement age – presently 60 years).</td>
                        <td></td>
                        <td></td>
                    </tr>  
                   <tr>
                        <td>Employment Status</td>
                        <td>Confirmed</td>
                        <td></td>
                        <td></td>
                    </tr>   
                    <tr>
                        <td>Qualifying Grade</td>
                        <td>SM and Above</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Minimum Appraisal Rating</td>
                        <td>“B”</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Overall Debt Service Ratio (DSR)</td>
                        <td>33.33% of staff’s basic income</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td>Good Credit Checks</td>
                        <td>Good Credit Checks from CRMS & 2 Others</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Maximum Tenor</td>
                        <td>120 Months subject to the number of years as an employee and the retirement age of 60.</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Mortgage Loan within the last 120 months</td>
                        <td>None</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Nature of employment</td>
                        <td>Professional staff</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Line ED’s Approval</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                ";
            result = result + $@"
                <br />
                <h3><b>BOARD / BCC (for AGM & above):</b></h3>                           
                 ";       
            return result;
        }
        public string StaffPersonalLoanAGMHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4>
                <h3><b>Staff Personal Loan Scheme – Facility Approval Memo</b></h3>
                <br />
                <h4><b>Staff Information</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td>Staff’s Name</td> 
                        <td>-----------------------------</td>
                        <td>Level/ Designation: </td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>Supervisor’s Name:</td>
                        <td>------------------------------</td>
                        <td>Level/ Designation</td>
                        <td>-------------------</td>
                    </tr> 
                     <tr>
                        <td>Borrower’s Residential Address</td>
                        <td colspan=3>---------------------</td>                    
                    </tr>
                   <tr>
                        <td>Branch / Unit:</td>
                        <td>-------------</td>
                        <td>Group:-------------</td>
                        <td>Division:-----------</td>
                    </tr> 
                     <tr>
                        <td>Date of Employment:</td>
                        <td>-----------------------------------</td>
                        <td>Employment Status</td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>No. Of years in Access Bank:</td>
                        <td>-----------------------</td>
                        <td>Total No. of years in Employment</td>
                        <td>-----------------------</td>
                        
                    </tr>    
                    <tr>
                        <td>Account Number:</td>
                        <td>----------------------</td>
                        <td>Loan Tenor:</td>
                        <td>----------------</td>
                    </tr>
                   <tr>
                        <td>Facility Entitlement:</td>
                        <td>₦------------------------</td>
                        <td>(Amount in words</td>
                        <td>-----------------------)</td>
                        
                    </tr>                       
                    <tr>
                        <td>Facility Amount:</td>
                        <td>₦-----------------------</td>
                        <td>(Amount in words</td>
                        <td>-----------------------)</td>                      
                    </tr>                    
                     <tr>
                        <td><strong>Sex:</strong></td>
                        <td><input type='checkbox' name='Male' value='Male'> Male
                            <input type='checkbox' name='Female' value='Female'> Female</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>Telephone numbers:</td>
                        <td>-----------------------</td>
                        <td>-----------------------</td>
                        <td>-----------------------</td>                     
                    </tr> 
                     <tr>
                        <td></td>
                        <td valign='top'>Office</td>
                        <td valign='top'>Home</td>
                        <td valign='top'>Mobile</td>                     
                    </tr> 
                     <tr>
                        <td>eMail Address:</td>
                        <td>-----------------------</td>
                        <td></td>
                        <td>-----------------------</td>
                        
                    </tr> 
                   <tr>
                        <td></td>
                        <td valign='top'>Office</td>
                        <td></td>
                        <td valign='top'>Personal</td>
                        
                    </tr>  
                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>To be completed by HR Only</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td>Staff Rating (Key Talent, A*, A, B, C, D):</td>
                        <td>---------------</td>
                        <td colspan=2>(indicate OK or NOT OK)</td>
                    </tr> 
                    <tr>
                        <td>Basic Salary:</td>
                        <td>-----------------</td>
                        <td>Debt Service Ratio (including new loan)</td>
                        <td>-------------%</td>               
                    </tr>  
                   <tr>
                       <td>Status of Employment ((Un)Confirmed):</td>
                        <td>------------</td>
                        <td><strong>Age:</strong></td>
                        <td>---------------</td>               
                    </tr> 
                    <tr>
                       <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td valign='top'>(Indicate OK or NOT OK)</td>
                    </tr> 
                    <tr>
                       <td colspan=3>Disciplinary case (Warning Letter / Suspension in the past six (6) months):</td>
                        <td>Yes<input type='checkbox' name='Yes' value='Yes'> 
                            No<input type='checkbox' name='No' value='No'></td>
                    </tr> 
                    <tr>
                        <td>Fast Track:</td>
                        <td>Yes<input type='checkbox' name='Yes' value='Yes'> 
                            No<input type='checkbox' name='No' value='No'></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Existing Facilities with Access Bank as at: -------------------------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Outstanding Amount (‘000)</b></th>
                        <th><b>Maturity Date</b></th>
                        <th><b>Security</b></th>
                        <th><b>Monthly Repayment</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Loan History (Facilities already Paid Down only) as at: --------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Approved Amount</b></th>
                        <th><b>Date of Last Repayment</b></th>
                        <th><b>Security</b></th>
                   </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Analysis of Six (6) Months Bank Statement</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Month</b></th>
                        <th><b>Debit</b></th>
                        <th><b>Credit</b></th>
                        <th><b>EOM Balance</b></th>
                        <th><b>Returned Cheques</b></th>
                   </tr> 
                   <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>   
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td><strong>Total</strong></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td><strong>Average</strong></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Existing Facilities with Other Banks as at: -------------------------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Outstanding Amount (‘000)</b></th>
                        <th><b>Maturity Date</b></th>
                        <th><b>Security</b></th>
                        <th><b>Monthly Repayment</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>                     
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Facility Type:</strong></td>
                        <td>Personal Loan</td>
                        </tr>
                        <tr>
                        <td><strong>Facility Amount:</strong></td>
                        <td>NGN</td>
                        </tr>
                        <tr>
                        <td><strong>Purpose:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Tenor:</strong> (2 years max for ET to SM and 4 years max for AGM and above):</td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Repayment Plan</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Interest Rate:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Positive Credit Check Reports:</strong> </td>
                        <td>To be obtained from CRMS and two other Credit Bureaus
                        </td>
                        </tr>
                        <tr>
                        <td><strong>Security/Support:</strong> </td>
                        <td><ul>
                                <li>Staff salaries, allowances and other emoluments.</li>
                                <li>Shares/ Stocks of **blue chip companies. Value of the collateral shall be determined by applying 30% discount on the
                                    ‘market value’ of such shares/ stocks. Such collateral value must be adequate to cover full recovery of The Bank’s principal and also provide minimum of 12 months interest payment cover. The assessed market value of such shares shall be based on average value of such shares over a twelve (12) month period as indicated on the Stock Exchange Daily Official List (SEDOL) and shall also reflect the most current market perception of expected future performance of such shares on the Stock Exchange.
                                    In addition, The Bank reserves the right to dispose of shares/ stocks pledged as collateral for Staff Loans should the value of such shares / stocks depreciate by over 30% and apply proceeds of such sale to liquidate the applicable loan(s). But before exercising this right, the staff shall be required to beefup the value of the collateral securing the loan to the level it was prior to the diminution in the values of the underlying shares/ stocks within forty-eight (48) hours.</li>
                                <li>Cash backed (where applicable).</li>
                            </ul>
                         </td>
                        </tr>
                        </table>
                        </td>
                    </tr> 

                     <tr>
                        <td><strong>Background Information On The Obligor:</strong> </td>
                    </tr>
                    <tr>
                        <td><strong>Current Request:</strong> </td>
                    </tr>
                    <tr>
                        <td><strong>Rationale for Current Request:</strong> </td>
                    </tr>
                     <tr>
                        <td><strong><hr>Conditions Precedent To Drawdown:</strong><br/> 
                           Conditions precedent to drawdown shall include, but not limited to the following:
                            <ul>                           
                                <li>Duly completed and signed Staff Loan Application Form.</li>
                                <li>Duly approved Staff Personal Loan Facility Approval Memo (FAM).</li>
                                <li>Duly accepted Offer Letter</li>
                                <li>Where shares / stocks are used as collateral, evidence that such shares / stocks have been transferred to Marina Securities Ltd or any other stock broking firm approved by The Bank.</li>
                                <li>Signed letter of set-off</li>
                                <li>Positive Credit Check Report from CRMS and two (2) other Credit Bureaus.</li>
                                <li>Drawdown memo must be approved by CRM – PBD, The Legal Team in Credit Admin and Portfolio Management Group, Credit Admin (Drawdown) and Treasury.</li>
                            </ul>
                        </td>
                    </tr>                   
            ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Risk Acceptance Criteria – Staff Personal Loan</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Description</b></th>
                        <th><b>Required</b></th>
                        <th><b>Actual</b></th>
                        <th><b>Exception (Y/N)</b></th>
                    </tr> 
                   <tr>
                        <td>Minimum years in The Bank </td>
                        <td>Upon confirmation</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Maximum Loan Amount</td>
                        <td><ul>
                            <li><strong>ET - SM:</strong> Up to staff’s annual basic salary for two years subject to DSR not exceeding onethird of staff’s basic monthly salary.</li>
                            <li><strong>AGM AND Above:</strong> Up to staff’s annual basic salary for four years subject to DSR not exceeding one-third of staff’s basic monthly / yearly salary.</li>
                        </ul>
                        </td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Global Limit</td>
                        <td>Aggregate value of all staff loan types shall not exceed 2.5% of the Bank’s total loan portfolio.</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td>Security – Shares/ Stocks of blue chip companies.</td>
                        <td>100% secured with shares of blue chips per Guideline in the Product Paper and The CPG.</td>
                        <td></td>
                        <td></td>
                    </tr>  
                   <tr>
                        <td>Security – Cash-backed</td>
                        <td>100% cash backed</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Employment Status</td>
                        <td>Confirmed</td>
                        <td></td>
                        <td></td>
                    </tr>   
                    <tr>
                        <td>Qualifying Grade</td>
                        <td>ET and Above</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Minimum Appraisal Rating</td>
                        <td>“B”</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Overall Debt Service Ratio (DSR)</td>
                        <td>33.33% of staff’s basic income</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td>Good Credit Checks</td>
                        <td>To be obtained from CRMS and two(2) other credit bureaus</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Maximum Tenor</td>
                        <td><ul>
                             <li><strong>ET - SM:</strong> Up to 24 months depending on loan amount. Where loan amount is less than 50% of staff’s basic annual salary, tenor shall not exceed one year.</li>
                             <li><strong>AGM AND Above:</strong> Up to 48 months.</li>
                            </ul>
                        </td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Mortgage Loan within the last 120 months</td>
                        <td>None</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Nature of employment</td>
                        <td>Full time employee</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Line ED’s Approval</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                ";
            result = result + $@"
                <br />
                <h3><b>BOARD / BCC (for AGM & above):</b></h3>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th></th>
                        <th></th>
                        <th></th>
                    </tr> 
                  
                 ";
            result = result + $"</table>";
            return result;
        }
        public string StaffPersonalLoanHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4>
                <h3><b>Staff Personal Loan Scheme – Facility Approval Memo</b></h3>
                <br />
                <h4><b>Staff Information</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td>Staff’s Name</td> 
                        <td>----------------------</td>
                        <td>Level/ Designation: </td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>Supervisor’s Name:</td>
                        <td>---------------------</td>
                        <td>Level/ Designation</td>
                        <td>-------------------</td>
                    </tr> 
                     <tr>
                        <td>Borrower’s Residential Address</td>
                        <td colspan=3>--------------------</td>
                       
                    </tr>
                   <tr>
                        <td>Branch / Unit:</td>
                        <td>-------------</td>
                        <td>Group:--------</td>
                        <td>Division:--------</td>
                    </tr> 
                     <tr>
                        <td>Date of Employment:</td>
                        <td>--------------------</td>
                        <td>Employment Status</td>
                        <td>-------------------</td>
                    </tr>
                   <tr>
                        <td>No. Of years in Access Bank:</td>
                        <td>-----------------------</td>
                        <td>Total No. of years in Employment</td>
                        <td>-----------------------</td>
                        
                    </tr>    
                    <tr>
                        <td>Account Number:</td>
                        <td>----------------------</td>
                        <td>Loan Tenor:</td>
                        <td>----------------</td>
                    </tr>
                   <tr>
                        <td>Facility Entitlement:</td>
                        <td>₦------------------------</td>
                        <td>(Amount in words</td>
                        <td>-----------------------)</td>
                        
                    </tr>                       
                    <tr>
                        <td>Facility Amount:</td>
                        <td>₦-----------------------</td>
                        <td>(Amount in words</td>
                        <td>-----------------------)</td>                      
                    </tr>                    
                     <tr>
                        <td><strong>Sex:</strong></td>
                        <td><input type='checkbox' name='Male' value='Male'> Male
                            <input type='checkbox' name='Female' value='Female'> Female</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>Telephone numbers:</td>
                        <td>-----------------------</td>
                        <td>-----------------------</td>
                        <td>-----------------------</td>                     
                    </tr> 
                     <tr>
                        <td></td>
                        <td valign='top'>Office</td>
                        <td valign='top'>Home</td>
                        <td valign='top'>Mobile</td>                     
                    </tr> 
                     <tr>
                        <td>eMail Address:</td>
                        <td>-----------------------</td>
                        <td></td>
                        <td>-----------------------</td>
                        
                    </tr> 
                   <tr>
                        <td></td>
                        <td valign='top'>Office</td>
                        <td></td>
                        <td valign='top'>Personal</td>
                        
                    </tr>  
                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>To be completed by HR Only</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td>Staff Rating (Key Talent, A*, A, B, C, D):</td>
                        <td>---------------</td>
                        <td colspan=2>(indicate OK or NOT OK)</td>
                    </tr> 
                    <tr>
                        <td>Basic Salary:</td>
                        <td>---------------------------------</td>
                        <td>Debt Service Ratio (including new loan)</td>
                        <td>-------------%</td>               
                    </tr>  
                   <tr>
                       <td>Status of Employment ((Un)Confirmed):</td>
                        <td>----------------------------</td>
                        <td><strong>Age:</strong></td>
                        <td>---------------</td>               
                    </tr> 
                    <tr>
                       <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td valign='top'>(Indicate OK or NOT OK)</td>
                    </tr> 
                    <tr>
                       <td colspan=3>Disciplinary case (Warning Letter / Suspension in the past six (6) months):</td>
                        <td>Yes<input type='checkbox' name='Yes' value='Yes'> 
                            No<input type='checkbox' name='No' value='No'></td>
                    </tr> 
                    <tr>
                        <td>Fast Track:</td>
                        <td>Yes<input type='checkbox' name='Yes' value='Yes'> 
                            No<input type='checkbox' name='No' value='No'></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Existing Facilities with Access Bank as at: -------------------------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Outstanding Amount (‘000)</b></th>
                        <th><b>Maturity Date</b></th>
                        <th><b>Security</b></th>
                        <th><b>Monthly Repayment</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Loan History (Facilities already Paid Down only) as at: --------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Approved Amount</b></th>
                        <th><b>Date of Last Repayment</b></th>
                        <th><b>Security</b></th>
                   </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>  
                   <tr>
                       <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Analysis of Six (6) Months Bank Statement</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Month</b></th>
                        <th><b>Debit</b></th>
                        <th><b>Credit</b></th>
                        <th><b>EOM Balance</b></th>
                        <th><b>Returned Cheques</b></th>
                   </tr> 
                   <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>   
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td><strong>Total</strong></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td><strong>Average</strong></td>
                        <td></td>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>                   
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Staff Existing Facilities with Other Banks as at: -------------------------------- (dd/mmm/yyyy)</b></h4>
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Outstanding Amount (‘000)</b></th>
                        <th><b>Maturity Date</b></th>
                        <th><b>Security</b></th>
                        <th><b>Monthly Repayment</b></th>
                    </tr> 
                   <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr> 
                    <tr>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                        <td>---------------</td>
                    </tr>                     
                    <tr>
                       <td colspan='5'><strong><hr>Total<hr></strong></td>
                     </tr>                  
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                        <tr>
                        <td><strong>Facility Type:</strong></td>
                        <td>Personal Loan</td>
                        </tr>
                        <tr>
                        <td><strong>Facility Amount:</strong></td>
                        <td>NGN</td>
                        </tr>
                        <tr>
                        <td><strong>Purpose:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Tenor:</strong> (2 years max for ET to SM and 4 years max for AGM and above):</td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Repayment Plan</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Interest Rate:</strong></td>
                        <td></td>
                        </tr>
                        <tr>
                        <td><strong>Positive Credit Check Reports:</strong> </td>
                        <td>CRMS and two other Credit Bureaus
                        </td>
                        </tr>
                        <tr>
                        <td><strong>Security/Support:</strong> </td>
                        <td>
                         </td>
                        </tr>
                        </table>
                        </td>
                    </tr> 

                     <tr>
                        <td><strong>Background Information On The Obligor:</strong> </td>
                    </tr>
                    <tr>
                        <td><strong>Current Request:</strong> </td>
                    </tr>
                    <tr>
                        <td><strong>Rationale for Current Request:</strong> </td>
                    </tr>
                     <tr>
                        <td><strong><hr>Conditions Precedent To Drawdown:</strong><br/> 
                           Conditions precedent to drawdown shall include, but not limited to the following:
                            <ul>                           
                                <li>Duly approved Staff Personal Loan Facility Approval Memo (FAM).</li>
                                <li>Duly accepted Offer Letter</li>
                                <li>Cash security inform of Shares/Investment acceptable.</li>
                                <li>Signed letter of set-off</li>
                                <li>Positive Credit Check Report from CRMS and two (2) other Credit Bureaus.</li>
                                <li>Drawdown memo must be approved by CRM – PBD, The Legal Team in Credit Admin and Portfolio Management Group, Credit Admin (Drawdown) and Treasury.</li>
                            </ul>
                        </td>
                    </tr>                   
            ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>Risk Acceptance Criteria – Staff Personal Loan</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Description</b></th>
                        <th><b>Required</b></th>
                        <th><b>Actual</b></th>
                        <th><b>Exception (Y/N)</b></th>
                    </tr> 
                   <tr>
                        <td>Minimum years in The Bank </td>
                        <td>Upon confirmation</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Maximum Loan Amount</td>
                        <td><ul>
                            <li><strong>ET - SM:</strong> Up to staff’s annual basic salary for two years subject to DSR not exceeding onethird of staff’s basic monthly salary.</li>
                            <li><strong>AGM AND Above:</strong> Up to staff’s annual basic salary for four years subject to DSR not exceeding one-third of staff’s basic monthly / yearly salary.</li>
                        </ul>
                        </td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Global Limit</td>
                        <td>Aggregate value of all staff loan types shall not exceed 2.5% of the Bank’s total loan portfolio.</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td>Security – Shares/ Stocks of blue chip companies.</td>
                        <td>100% secured with shares of blue chips per Guideline in the Product Paper and The CPG.</td>
                        <td></td>
                        <td></td>
                    </tr>  
                   <tr>
                        <td>Security – Cash-backed</td>
                        <td>100% cash backed</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Employment Status</td>
                        <td>Confirmed</td>
                        <td></td>
                        <td></td>
                    </tr>   
                    <tr>
                        <td>Qualifying Grade</td>
                        <td>ET and Above</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Minimum Appraisal Rating</td>
                        <td>“B”</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Overall Debt Service Ratio (DSR)</td>
                        <td>33.33% of staff’s basic income</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td>Good Credit Checks</td>
                        <td>To be obtained from CRMS and two(2) other credit bureaus</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Maximum Tenor</td>
                        <td><ul>
                             <li><strong>ET - SM:</strong> Up to 24 months depending on loan amount. Where loan amount is less than 50% of staff’s basic annual salary, tenor shall not exceed one year.</li>
                             <li><strong>AGM AND Above:</strong> Up to 48 months.</li>
                            </ul>
                        </td>
                        <td></td>
                        <td></td>
                    </tr> 
                    
                    <tr>
                        <td>Nature of employment</td>
                        <td>Full time employee</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Line ED’s Approval</td>
                        <td>Yes</td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                ";
            result = result + $@"
                <br />
                <h3><b>BOARD / BCC (for AGM & above):</b></h3>
                
                 ";   
            return result;
        }
        public string DocumentationDeferralWaiverFormHtml(int staffId, int operationId, int targetId)
        {
            var isInitialize = InitializeDrawdownMemoProperties(targetId,operationId);

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr></table> ";   
                   
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4>
                <h3><b>DOCUMENTATION DEFERRAL/WAIVER FORM</b></h3>
                <br />
               
                <table style='font face: arial; size:12px' border=0 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td><strong>BORROWER:</strong></td> 
                        <td>{customerName}</td>
                        <td><strong>DATE:</strong> </td>
                        <td>{currentDate}</td>
                    </tr>
                   <tr>
                        <td><strong>FACILITY TYPE::</strong></td> 
                        <td>{facilityType}</td>
                        <td><strong>BRANCH:</strong> </td>
                        <td>{branchName}</td>
                    </tr>
                    <tr>
                        <td><strong>FACILITY AMOUNT:</strong></td> 
                        <td>{approvedAmount}</td>
                        <td><strong>FINAL APPROVAL:</strong><br><em>(AS PER CPG)</em></td>  
                        <td></td>
                    </tr>
                   <tr>
                        <td><strong>PREPARED BY:</strong></td> 
                        <td colspan=3>{preparedBy}</td>                                   
                    </tr>
                      
                   
                 ";
            result = result + $"</table>";
            var condition = GetDeferralMemoChecklistAwaitingApproval(staffId, targetId, operationId).ToList();
            var appId = context.TBL_LOAN_APPLICATION_DETAIL.Find(targetId);
            var precedent = GetConditionPrecedentByApplicationDetailId(appId.LOANAPPLICATIONDETAILID).ToList();
            result = result + $@"
                <br />              
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><em><strong>Documents/Conditions precedent to draw down as approved in the FAM</strong></em>:</td>
                        <td><strong><em>Description of Document</strong></em></td>
                        <td><strong><em>Reason for Deferral/waiver</strong></em></td>
                        <td><strong><em>No of days</em></strong></td>
                        <td><strong><em>Number of times deferred</strong></em></td>
                    </tr>                         
                 ";
            foreach(var d in condition) {
                result = result + $@"
                  <tr>
                        <td>{d.condition}</td>
                         <td>{d.loanInformation}</td>
                        <td>{d.reason}</td>
                        <td>{d.deferralDuration}</td>
                        <td>{d.numberOfTimesDeferred}</td>    
                        
                    </tr> 
                ";
            }
            result = result + $"</table>";

            result = result + $@" </br></br></br>
                <table style='font face: arial; size:12px' border=1 width=900px align=left cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>Role</b></th>
                        <th><b>Name</b></th>
                        <th><b>Decision</b></th>
                        <th><b>Comment</b></th>
                        <th><b>Date</b></th>
                    </tr>
                    ";
            foreach (var pre in precedent)
            {
                var trail = GetDeferralnAprroval(operationId, pre.loanConditionId);
                if (trail != null)
                {
                    result = result + $@"
                    <tr>
                        <td>{trail.fromApprovalLevelName.ToUpper()}</td>
                        <td>{trail.fromStaffName}</td>
                        <td>{GetDecision(trail.approvalStatusId)}</td>
                        <td>{trail.comment}</td>
                        <td>{trail.systemArrivalDateTime}</td>
                    </tr>";
                }
            }

            result = result + $"</table>";

            //result = result + $@"
            //        <br/>
            //     <p><strong><em> RELATIONSHIP OFFICER: &nbsp; &nbsp;</em></strong>{relationshipOfficerName}</p>
            //    <br/>";
            //foreach (var pre in precedent)
            //{
            //    var approvals = GetDeferralnAprroval(operationId, pre.loanConditionId);
            //        result = result + $@"
            //     <p><strong><em>{approvals?.fromApprovalLevelName}&nbsp;&nbsp;</em></strong>{approvals?.fromStaffName}</p><br/>";
            //}
            return result;
        }
        private IEnumerable<ChecklistApprovalViewModel> GetChecklistAwaitingApproval(int staffId, int operationId)
        {
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, operationId).ToList();

            var dataLOS = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                           join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           join c in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals c.LOANCONDITIONID
                           join atrail in context.TBL_APPROVAL_TRAIL on c.LOANCONDITIONID equals atrail.TARGETID
                           where c.ISLMS == false
                           && ((atrail.OPERATIONID == (int)OperationsEnum.DefferedChecklistApproval) || (atrail.OPERATIONID == (int)OperationsEnum.WaivedChecklistApproval))
                               && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                               && atrail.RESPONSESTAFFID == null
                               && atrail.LOOPEDSTAFFID == null
                           orderby a.DATETIMECREATED descending
                           select new ChecklistApprovalViewModel()
                           {
                               customerName = a.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                               customerId = a.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID : a.TBL_CUSTOMER.CUSTOMERID,
                               proposedAmount = a.APPROVEDAMOUNT,
                               approvalStatus = atrail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                               deferredDate = b.DEFEREDDATE,
                               dateTimeCreated = b.DATETIMECREATED,
                               condition = b.CONDITION,
                               conditionId = b.LOANCONDITIONID,
                               loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                               applicationReferenceNumber = a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                               checklistStatus = b.TBL_CHECKLIST_STATUS.CHECKLISTSTATUSNAME,
                               dateCreated = b.DATETIMECREATED,
                               operationId = atrail.OPERATIONID,
                               //Loan Information
                               relationshipOfficerName = a.TBL_LOAN_APPLICATION.TBL_STAFF.FIRSTNAME + " " + a.TBL_LOAN_APPLICATION.TBL_STAFF.FIRSTNAME,
                               relationshipManagerName = a.TBL_LOAN_APPLICATION.TBL_STAFF1.FIRSTNAME + " " + a.TBL_LOAN_APPLICATION.TBL_STAFF1.FIRSTNAME,
                               applicationAmount = a.TBL_LOAN_APPLICATION.APPLICATIONAMOUNT,
                               applicationTenor = a.PROPOSEDTENOR,
                               applicationDate = a.TBL_LOAN_APPLICATION.APPLICATIONDATE,
                               isInvestmentGrade = a.TBL_LOAN_APPLICATION.ISINVESTMENTGRADE,
                               isPoliticallyExposed = a.TBL_LOAN_APPLICATION.ISPOLITICALLYEXPOSED,
                               isRelatedParty = a.TBL_LOAN_APPLICATION.ISRELATEDPARTY,
                               approvalStatusId = a.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID,
                               applicationStatusId = a.TBL_LOAN_APPLICATION.APPROVALSTATUSID,
                               submittedForAppraisal = a.TBL_LOAN_APPLICATION.SUBMITTEDFORAPPRAISAL,
                               loanInformation = a.LOANPURPOSE,
                               isLms = c.ISLMS,
                               reason = c.DEFERRALREASON,
                               deferredDateOnFinalApproval = c.DEFEREDDATEONFINALAPPROVAL,
                               dateApproved = c.DATEAPPROVED == null ? c.DATETIMECREATED : c.DATEAPPROVED,
                           }).ToList();

            foreach (var x in dataLOS)
            {
                x.deferralDuration = x.deferredDateOnFinalApproval != null ? (x.deferredDateOnFinalApproval - x.dateApproved).Value.Days : 0;

            }

            var dataLMS = (from a in context.TBL_LMSR_APPLICATION_DETAIL
                           join b in context.TBL_LMSR_CONDITION_PRECEDENT on a.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                           join c in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals c.LOANCONDITIONID
                           join atrail in context.TBL_APPROVAL_TRAIL on c.LOANCONDITIONID equals atrail.TARGETID
                           where c.ISLMS == true
                            && ((atrail.OPERATIONID == (int)OperationsEnum.DefferedChecklistApproval) || (atrail.OPERATIONID == (int)OperationsEnum.WaivedChecklistApproval))
                               && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                               && atrail.RESPONSESTAFFID == null
                               && atrail.LOOPEDSTAFFID == null
                           orderby a.DATETIMECREATED descending
                           select new ChecklistApprovalViewModel()
                           {
                               customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                               proposedAmount = a.APPROVEDAMOUNT,
                               approvalStatus = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == b.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                               deferredDate = b.DEFEREDDATE,
                               dateTimeCreated = b.DATETIMECREATED,
                               condition = b.CONDITION,
                               conditionId = b.LOANCONDITIONID,
                               loanApplicationId = a.LOANAPPLICATIONID,
                               applicationReferenceNumber = a.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER,
                               checklistStatus = context.TBL_CHECKLIST_STATUS.Where(o => o.CHECKLISTSTATUSID == b.CHECKLISTSTATUSID).Select(o => o.CHECKLISTSTATUSNAME).FirstOrDefault(),
                               dateCreated = b.DATETIMECREATED,
                               relationshipOfficerName = context.TBL_STAFF.Where(o=>o.STAFFID ==a.CREATEDBY).Select(o=>o.FIRSTNAME).FirstOrDefault()+" "+ context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.MIDDLENAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.LASTNAME).FirstOrDefault(),
                               relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.FIRSTNAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.MIDDLENAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.LASTNAME).FirstOrDefault(),
                               applicationAmount = 0,//a.TBL_LOAN_APPLICATION.APPLICATIONAMOUNT,
                               applicationTenor = 0,//a.PROPOSEDTENOR,
                               applicationDate = a.TBL_LMSR_APPLICATION.APPLICATIONDATE,
                               isInvestmentGrade = false,//a.TBL_LOAN_APPLICATION.ISINVESTMENTGRADE,
                               isPoliticallyExposed = false,//a.TBL_LOAN_APPLICATION.ISPOLITICALLYEXPOSED,
                               isRelatedParty = false,//a.TBL_LOAN_APPLICATION.ISRELATEDPARTY,
                               approvalStatusId = 0,//a.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID,
                               applicationStatusId = 0,//a.TBL_LOAN_APPLICATION.APPROVALSTATUSID,
                               submittedForAppraisal = true,//a.TBL_LOAN_APPLICATION.SUBMITTEDFORAPPRAISAL,
                               loanInformation = "",//a.LOANPURPOSE
                               isLms = c.ISLMS,
                               deferredDateOnFinalApproval = c.DEFEREDDATEONFINALAPPROVAL,
                               dateApproved = c.DATEAPPROVED == null ? c.DATETIMECREATED : c.DATEAPPROVED,
                           }).ToList();

            foreach (var x in dataLMS)
            {
                x.deferralDuration = x.deferredDateOnFinalApproval != null ? (x.deferredDateOnFinalApproval - x.dateApproved).Value.Days : 0;
            }

            return dataLOS.Union(dataLMS);
        }


        //fordeferrals memo only 

        private IEnumerable<ChecklistApprovalViewModel> GetDeferralMemoChecklistAwaitingApproval(int staffId, int targetId, int operationId)
        {
            
                var ids = _genSetup.GetStaffApprovalLevelIds(staffId, operationId).ToList();

                var dataLOS = (from a in context.TBL_LOAN_APPLICATION_DETAIL
                               join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                               join c in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals c.LOANCONDITIONID
                               join atrail in context.TBL_APPROVAL_TRAIL on c.LOANCONDITIONID equals atrail.TARGETID
                               where c.ISLMS == false
                               //&& ((atrail.OPERATIONID == (int)OperationsEnum.DefferedChecklistApproval) || (atrail.OPERATIONID == (int)OperationsEnum.WaivedChecklistApproval))
                                   && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                   && a.LOANAPPLICATIONDETAILID == targetId
                               orderby a.DATETIMECREATED descending
                               select new ChecklistApprovalViewModel()
                               {
                                   approvalTrailId = atrail.APPROVALTRAILID,
                                   loanConditionId = b.LOANCONDITIONID,
                                   customerName = a.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.GROUPNAME : a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                   customerId = a.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup ? a.TBL_LOAN_APPLICATION.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID : a.TBL_CUSTOMER.CUSTOMERID,
                                   proposedAmount = a.APPROVEDAMOUNT,
                                   approvalStatus = atrail.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                   deferredDate = c.DEFERREDDATE,
                                   deferredDateOnFinalApproval = c.DEFEREDDATEONFINALAPPROVAL,
                                   dateApproved = c.DATEAPPROVED == null ? c.DATETIMECREATED : c.DATEAPPROVED,
                                   dateTimeCreated = c.DATETIMECREATED,
                                   condition = b.CONDITION,
                                   conditionId = b.LOANCONDITIONID,
                                   loanApplicationId = b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID,
                                   applicationReferenceNumber = a.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                   checklistStatus = b.TBL_CHECKLIST_STATUS.CHECKLISTSTATUSNAME,
                                   dateCreated = b.DATETIMECREATED,
                                   operationId = atrail.OPERATIONID,
                                   //Loan Information
                                   relationshipOfficerName = a.TBL_LOAN_APPLICATION.TBL_STAFF.FIRSTNAME + " " + a.TBL_LOAN_APPLICATION.TBL_STAFF.FIRSTNAME,
                                   relationshipManagerName = a.TBL_LOAN_APPLICATION.TBL_STAFF1.FIRSTNAME + " " + a.TBL_LOAN_APPLICATION.TBL_STAFF1.FIRSTNAME,
                                   applicationAmount = a.TBL_LOAN_APPLICATION.APPLICATIONAMOUNT,
                                   applicationTenor = a.PROPOSEDTENOR,
                                   applicationDate = a.TBL_LOAN_APPLICATION.APPLICATIONDATE,
                                   isInvestmentGrade = a.TBL_LOAN_APPLICATION.ISINVESTMENTGRADE,
                                   isPoliticallyExposed = a.TBL_LOAN_APPLICATION.ISPOLITICALLYEXPOSED,
                                   isRelatedParty = a.TBL_LOAN_APPLICATION.ISRELATEDPARTY,
                                   approvalStatusId = a.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID,
                                   applicationStatusId = a.TBL_LOAN_APPLICATION.APPROVALSTATUSID,
                                   submittedForAppraisal = a.TBL_LOAN_APPLICATION.SUBMITTEDFORAPPRAISAL,
                                   loanInformation = a.LOANPURPOSE,
                                   isLms = c.ISLMS == true,
                                   reason = c.DEFERRALREASON
                               }).ToList();

                foreach (var x in dataLOS)
                {
                    x.deferralDuration = x.deferredDateOnFinalApproval != null ? (x.deferredDateOnFinalApproval - x.dateApproved).Value.Days : 0;
                    x.numberOfTimesDeferred = context.TBL_LOAN_CONDITION_DEFERRAL.Where(xx => xx.LOANCONDITIONID == x.loanConditionId).Select(xx => xx.LOANCONDITIONID).Count();
                }

                var dataLMS = (from a in context.TBL_LMSR_APPLICATION_DETAIL
                               join b in context.TBL_LMSR_CONDITION_PRECEDENT on a.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                               join c in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals c.LOANCONDITIONID
                               join atrail in context.TBL_APPROVAL_TRAIL on c.LOANCONDITIONID equals atrail.TARGETID
                               where c.ISLMS == true
                               // && ((atrail.OPERATIONID == (int)OperationsEnum.DefferedChecklistApproval) || (atrail.OPERATIONID == (int)OperationsEnum.WaivedChecklistApproval))
                                   && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                   && a.LOANREVIEWAPPLICATIONID == targetId
                               orderby a.DATETIMECREATED descending
                               select new ChecklistApprovalViewModel()
                               {
                                   deferredDateOnFinalApproval = c.DEFEREDDATEONFINALAPPROVAL,
                                   dateApproved = c.DATEAPPROVED == null ? c.DATETIMECREATED : c.DATEAPPROVED,
                                   approvalTrailId = atrail.APPROVALTRAILID,
                                   loanConditionId = b.LOANCONDITIONID,
                                   customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                   proposedAmount = a.APPROVEDAMOUNT,
                                   approvalStatus = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == b.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                                   deferredDate = c.DEFERREDDATE,
                                   dateTimeCreated = c.DATETIMECREATED,
                                   condition = b.CONDITION,
                                   conditionId = b.LOANCONDITIONID,
                                   loanApplicationId = a.LOANAPPLICATIONID,
                                   applicationReferenceNumber = a.TBL_LMSR_APPLICATION.APPLICATIONREFERENCENUMBER,
                                   checklistStatus = context.TBL_CHECKLIST_STATUS.Where(o => o.CHECKLISTSTATUSID == b.CHECKLISTSTATUSID).Select(o => o.CHECKLISTSTATUSNAME).FirstOrDefault(),
                                   dateCreated = b.DATETIMECREATED,
                                   relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.FIRSTNAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.MIDDLENAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.LASTNAME).FirstOrDefault(),
                                   relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.FIRSTNAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.MIDDLENAME).FirstOrDefault() + " " + context.TBL_STAFF.Where(o => o.STAFFID == a.CREATEDBY).Select(o => o.LASTNAME).FirstOrDefault(),
                                   applicationAmount = 0,//a.TBL_LOAN_APPLICATION.APPLICATIONAMOUNT,
                                   applicationTenor = 0,//a.PROPOSEDTENOR,
                                   applicationDate = a.TBL_LMSR_APPLICATION.APPLICATIONDATE,
                                   isInvestmentGrade = false,//a.TBL_LOAN_APPLICATION.ISINVESTMENTGRADE,
                                   isPoliticallyExposed = false,//a.TBL_LOAN_APPLICATION.ISPOLITICALLYEXPOSED,
                                   isRelatedParty = false,//a.TBL_LOAN_APPLICATION.ISRELATEDPARTY,
                                   approvalStatusId = 0,//a.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID,
                                   applicationStatusId = 0,//a.TBL_LOAN_APPLICATION.APPROVALSTATUSID,
                                   submittedForAppraisal = true,//a.TBL_LOAN_APPLICATION.SUBMITTEDFORAPPRAISAL,
                                   loanInformation = "",//a.LOANPURPOSE
                                   isLms = c.ISLMS == true
                               }).ToList();

            foreach (var x in dataLMS)
            {
                x.deferralDuration = x.deferredDateOnFinalApproval != null ? (x.deferredDateOnFinalApproval - x.dateApproved).Value.Days : 0;
                x.numberOfTimesDeferred = context.TBL_LOAN_CONDITION_DEFERRAL.Where(xx => xx.LOANCONDITIONID == x.loanConditionId).Select(xx => xx.LOANCONDITIONID).Count();
            }

            var data = dataLOS.Union(dataLMS);
                var result = data.GroupBy(r => r.loanConditionId)
                                   .Select(p => p.OrderByDescending(r => r.approvalTrailId).FirstOrDefault()).ToList();
                return result;
            
        }

        public IEnumerable<ApprovalTrailViewModel> GetDeferralnAprrovalTrail(int operationId, int targetId)
        {

            var allstaff = this.GetAllStaffNames();
            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.FROMAPPROVALLEVELID != null && x.OPERATIONID== operationId && x.TARGETID == targetId);
            var data = trail.Select(x => new ApprovalTrailViewModel
            {
                approvalTrailId = x.APPROVALTRAILID,
                comment = x.COMMENT,
                vote = x.VOTE,
                targetId = x.TARGETID,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = (int)x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            }).ToList();

            return data.OrderByDescending(d=>d.systemArrivalDateTime);
        }
        public ApprovalTrailViewModel GetDeferralnAprroval(int operationId, int targetId)
        {

            var allstaff = this.GetAllStaffNames();
            var staffs = context.TBL_STAFF.ToList();
            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.FROMAPPROVALLEVELID != null && x.OPERATIONID == operationId && x.TARGETID == targetId);
            var data = trail.Select(x => new ApprovalTrailViewModel
            {
                approvalTrailId = x.APPROVALTRAILID,
                comment = x.COMMENT,
                vote = x.VOTE,
                targetId = x.TARGETID,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = (int)x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            }).FirstOrDefault();

            return data;
        }
        public IEnumerable<ConditionPrecedentViewModel> GetConditionPrecedentByApplicationDetailId(int applicationDetailId)
        {

            var trail = context.TBL_LOAN_CONDITION_PRECEDENT.Where(x => x.LOANAPPLICATIONDETAILID == applicationDetailId);
            var data = trail.Select(x => new ConditionPrecedentViewModel
            {
                loanConditionId = x.LOANCONDITIONID,
                loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                condition = x.CONDITION
            }).ToList();

            return data;
        }

        
        public string CashBackMemoMarkupHtml(int staffId, int operationId, int targetId)
        {
            
            var isInitialize = InitializeCashBackMemoProperties(operationId, targetId);
            var staffBranch = context.TBL_STAFF.Where(s => s.STAFFID == staffId).Select(s => s.BRANCHID).FirstOrDefault();
            var branchName = context.TBL_BRANCH.Where(b => b.BRANCHID == staffBranch).Select(b => b.BRANCHNAME).FirstOrDefault();
            var flowChange = context.TBL_LOAN_APPLICATN_FLOW_CHANGE.Where(o => o.OPERATIONID == operationId).FirstOrDefault();
            var cashbackSection = context.TBL_CASHBACK.Where(x => x.LOANAPPLICATIONDETAILID == targetId).FirstOrDefault();

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br />
                <h3><b>MEMO</b></h3> <br />
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>                   
                    <tr>
                        <td><b>Date:</b></td>
                        <td>{DateTime.Now}</td>
                    </tr>
                    <tr>
                        <td><b>From:</b></td>
                        <td>{relationshipOfficerName.ToUpper()}</td>
                    </tr>
                    <tr>
                        <td><b>To:</b></td>
                        <td>THE UNDRELISTED</td>
                    </tr>
                    <tr>
                        <td><b>Location:</b></td>
                        <td>{branchName?.ToUpper()}</td>
                    </tr>
                    <tr>
                        <td><b>Subject:</b></td>
                        <td>APPROVAL TO ISSUE {flowChange?.PLACEHOLDER.ToUpper()} {facilityType?.ToUpper()} TO {customerName?.ToUpper()}</td>
                    </tr>
                 ";
            result = result + $"</table>";
            result = result + $@" 
                    <p></p>
                    <p><b>BACKGROUND</b></p>
                    <p>{cashbackSection?.BACKGROUND}</p>
                    <p><b>ISSUES</b></p>
                    <p>{cashbackSection?.ISSUES}</p>
                    <p><b>REQUEST</b></p>
                    <p>{cashbackSection?.REQUEST}</p>
                <p align='center'><b>APPROVAL LOG</b></p>
                <p><b>APPROVAL TO ISSUE {flowChange?.PLACEHOLDER.ToUpper()} {facilityType?.ToUpper()} TO {customerName?.ToUpper()}</b></p>
                <p><b>{flowChange?.PLACEHOLDER.ToUpper()} MEMO</b></p>
            ";
            result = result + GetCashBackApprovalsMarkupLOS(this.loanApplication.LOANAPPLICATIONID, operationId);
            return result;
        }

        private string GetCashBackApprovalsMarkupLOS(int targetId, int operationId)
        {
            var appraisals = GetAppraisalMemorandumTrail(targetId, operationId,false).OrderBy(a => a.approvalTrailId);
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <th><b>APPROVAL NAME</b></th>
                        <th><b>DESIGNATION</b></th>
                        <th><b>COMMENT</b></th>
                        <th><b>ROUTING STATUS</b></th>
                        <th><b>DATE APPROVED</b></th>
                    </tr>
                    ";
            foreach (var trail in appraisals)
            {
                result = result + $@"
                    <tr>
                        <td>{trail.fromStaffName}</td>
                        <td>{trail.fromApprovalLevelName.ToUpper()}</td>
                        <td>{trail.comment}</td>
                        <td>{GetDecision(trail.approvalStatusId)}</td>
                        <td>{trail.systemArrivalDateTime}</td>
                    </tr>
                ";
            }

            result = result + $"</table>";
            return result;

        }

        public string GetCallMemoMarkup(int id)
        {
            var data = context.TBL_CALL_MEMO.Find(id);
            var staff = context.TBL_STAFF.Where(s => s.STAFFID == data.CREATEDBY).Select(s => s).FirstOrDefault();
            var branch = context.TBL_BRANCH.Where(c => c.BRANCHID == staff.BRANCHID).Select(c => c.BRANCHNAME).FirstOrDefault();
            var customer = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == data.CUSTOMERID).Select(c=>c).FirstOrDefault();
            var customerName = customer.FIRSTNAME + " " + customer?.MIDDLENAME + " " + customer?.LASTNAME;
            var nextDateTime = data.NEXTCALLDATE;
            var date = nextDateTime?.ToString("yyyy-MM-dd");
            var nextCallTime = data.NEXTCALLTIME;
            var time = nextCallTime?.ToString("hh:mm:ss");

            var result = String.Empty;
            result = result + $@"
                <table border=1 width=750 cellpadding=5 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>                    
                    <tr>
                      <td><strong>Date</strong></td>
                      <td>{data.DATECREATED}</td>  
                    </tr>
                    <tr>
                      <td><strong>Participants at the meeting</strong></td>
                      <td>{data.PARTICIPANTS}</td>  
                    </tr>
                    <tr>
                      <td><strong>Location of the meeting</strong></td>
                      <td>{data.LOCATION}</td>  
                    </tr>
                    <tr>
                      <td><strong>Customer</strong></td>
                      <td>{customerName.ToUpper()}</td>  
                    </tr>
                     <tr>
                      <td><strong>Time</strong></td>
                      <td>{data.CALLTIME}</td>  
                    </tr>
                    <br/>
                    ";
            result = result + $"</table>";
            result = result + $"<br/>";
            result = result + $"<p><strong>CUSTOMER BACKGROUND</strong><br/>{data.BACKGROUND}";
            result = result + $"</p><br/>";
            result = result + $"<p><strong>RECENT UPDATE</strong><br/>{data.RECENTUPDATE}";
            result = result + $"</p><br/>";
            result = result + $"<p><strong>PURPOSE OF THE MEETING</strong><br/>{data.PURPOSE}";
            result = result + $"</p><br/>";
            result = result + $"<p><strong>MEETING HIGHLIGHTS</strong><br/>{data.DISCUSION}";
            result = result + $"</p><br/>";
            result = result + $"<p><strong>ACTION PLAN</strong><br/>{data.ACTION}";
            result = result + $"</p><br/>";
            result = result + $"<p><strong>NEXT CALL DATE AND TIME</strong><br/>{date} {time}";
            result = result + $"</p><br/>";
            result = result + $"<strong>NAME OF INITIATOR: </strong>{staff.FIRSTNAME} {staff.MIDDLENAME} {staff.LASTNAME}";
            result = result + $"<br/>";
            result = result + $"<strong>BUSINESS UNIT: </strong>{branch}";
            return result;

        }

        public int count = 0;
        public IEnumerable<OfferLetterConditionPrecidentViewModel> GetLoanApplicationConditionSubsequentLMS(int? loanApplicationId)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var conditionSubsequentData = (from a in context.TBL_LMSR_APPLICATION
                                           join c in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                           join b in context.TBL_LMSR_CONDITION_PRECEDENT on c.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                                           where a.LOANAPPLICATIONID == (int)loanApplicationId
                                           select new OfferLetterConditionPrecidentViewModel()
                                           {
                                               conditionPrecident = b.CONDITION,
                                               loanApplicationId = a.LOANAPPLICATIONID,
                                               isExternal = b.ISEXTERNAL,
                                               productName = c.TBL_PRODUCT.PRODUCTNAME
                                           }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();

            var conditionPrecedentDeferralData = (from a in context.TBL_LMSR_APPLICATION
                                                  join c in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                                  join b in context.TBL_LMSR_CONDITION_PRECEDENT on c.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                                                  join d in context.TBL_LOAN_CONDITION_DEFERRAL on b.LOANCONDITIONID equals d.LOANCONDITIONID
                                                  where a.LOANAPPLICATIONID == (int)loanApplicationId
                                                  select new OfferLetterConditionPrecidentViewModel()
                                                  {
                                                      conditionPrecident = b.CONDITION,
                                                      loanApplicationId = a.LOANAPPLICATIONID,
                                                      isExternal = b.ISEXTERNAL,
                                                      productName = c.TBL_PRODUCT.PRODUCTNAME
                                                  }).GroupBy(x => x.conditionPrecident).Select(y => y.FirstOrDefault()).ToList();


            var forDebugging = conditionSubsequentData.ToList().Union(conditionPrecedentDeferralData.ToList());
            return conditionSubsequentData;
        }
        public List<TransactionDynamicsViewModel> Los_ConditionDynamicsLMS(int? loanApplicationId)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
            count = 1;
            var transactionDynamicsDetails = (from a in context.TBL_LMSR_TRANSACTION_DYNAMICS
                                              join b in context.TBL_LMSR_APPLICATION_DETAIL on a.LOANREVIEWAPPLICATIONID equals b.LOANREVIEWAPPLICATIONID
                                              join c in context.TBL_LMSR_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                                              //where c.LOANAPPLICATIONID == (int)loanApplicationId
                                              where c.LOANAPPLICATIONID == (int)loanApplicationId
                                              select new TransactionDynamicsViewModel()
                                              {
                                                  SN = +count,
                                                  dynamics = a.DYNAMICS,
                                                  productName = b.TBL_PRODUCT.PRODUCTNAME
                                              }).Distinct().ToList();

            return transactionDynamicsDetails;
        }

        private string GetOutstandingLoans(int accreditedConsultantId, string referenceId)
        {
            var exposureData = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join ln in context.TBL_GLOBAL_EXPOSURE on lr.LOANREFERENCE equals ln.REFERENCENUMBER
                                where
                                lr.ISFULLYRECOVERED == false
                                && lr.ACCREDITEDCONSULTANT == accreditedConsultantId
                                && lr.REFERENCEID == referenceId
                                && lr.DELETED == false

                                orderby ln.ID descending
                                select new GlobalExposureApplicationViewModel
                                {
                                    totalAmountRecovery = (decimal)lr.TOTALAMOUNTRECOVERY,
                                    customerCode = ln.CUSTOMERID,
                                    loanTypeName = "",
                                    customerName = ln.CUSTOMERNAME,
                                    branchName = ln.BRANCHNAME,
                                    loanReferenceNumber = ln.REFERENCENUMBER
                                }).ToList();
            foreach (var xx in exposureData)
            {
                var customerid = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == xx.customerCode).Select(x => x.CUSTOMERID).FirstOrDefault();
                xx.customerAddresses = context.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == customerid).Select(x => x.ADDRESS).ToList();
            }

            var dataLoan = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                join ln in context.TBL_LOAN on lr.LOANID equals ln.TERMLOANID
                                join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                where
                                lr.ISFULLYRECOVERED == false
                                && lr.ACCREDITEDCONSULTANT == accreditedConsultantId
                                && pr.EXCLUDEFROMLITIGATION == false
                                && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                && lr.REFERENCEID == referenceId
                                && lr.DELETED == false

                                select new GlobalExposureApplicationViewModel
                                {
                                    totalAmountRecovery = (decimal)lr.TOTALAMOUNTRECOVERY,
                                    customerCode = cu.CUSTOMERCODE,
                                    loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                    customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                    customerAddresses = context.TBL_CUSTOMER_ADDRESS.Where(a => a.CUSTOMERID == cu.CUSTOMERID).Select(a => a.ADDRESS).ToList(),
                                    branchName = br.BRANCHNAME,
                                    loanReferenceNumber = ln.LOANREFERENCENUMBER
                                }).ToList();
            
            var dataRevolvingLoan = (from lr in context.TBL_LOAN_RECOVERY_ASSIGNMENT
                                         join ln in context.TBL_LOAN_REVOLVING on lr.LOANID equals ln.REVOLVINGLOANID
                                         join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on ln.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                         join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                         join cu in context.TBL_CUSTOMER on ln.CUSTOMERID equals cu.CUSTOMERID
                                         join pr in context.TBL_PRODUCT on ln.PRODUCTID equals pr.PRODUCTID
                                         join st in context.TBL_STAFF on ln.RELATIONSHIPOFFICERID equals st.STAFFID
                                         join stm in context.TBL_STAFF on ln.RELATIONSHIPMANAGERID equals stm.STAFFID
                                         where
                                         lr.ISFULLYRECOVERED == false
                                         && lr.ACCREDITEDCONSULTANT == accreditedConsultantId
                                         && pr.EXCLUDEFROMLITIGATION == false
                                         && ln.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                         && lr.REFERENCEID == referenceId
                                         && lr.DELETED == false

                                         select new GlobalExposureApplicationViewModel
                                         {
                                             totalAmountRecovery = (decimal)lr.TOTALAMOUNTRECOVERY,
                                             customerCode = cu.CUSTOMERCODE,
                                             loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                             customerName = cu.LASTNAME + " " + cu.FIRSTNAME + " " + cu.MIDDLENAME,
                                             customerAddresses = context.TBL_CUSTOMER_ADDRESS.Where(a=>a.CUSTOMERID == cu.CUSTOMERID).Select(a=>a.ADDRESS).ToList(),
                                             branchName = br.BRANCHNAME,
                                             loanReferenceNumber = ln.LOANREFERENCENUMBER
                                         }).ToList();


                    var data = dataLoan.Union(dataRevolvingLoan).Union(exposureData);
                     foreach(var rec in data)
                    {
                        foreach(var address in rec.customerAddresses)
                        {
                            rec.address = rec.address + " " + address;
                        }
                    }

            var consult = context.TBL_ACCREDITEDCONSULTANT.Find(accreditedConsultantId);
            this.recoveryAnalysisFirmNameData = consult.FIRMNAME;
            this.recoveryAnalysisAddressData = consult.ADDRESS;
            this.recoveryAnalysisDateData = DateTime.Now.ToString("dd-MM-yyyy");

            int i = 0;
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>Reference Number</b></th>
                        <th><b>Name Of Customer</b></th>
                        <th><b>Address/GSM No</b></th>
                        <th><b>Outstanding Exposure</b></th>
                        <th><b>Branch</b></th>
                    </tr>
                    ";
            foreach (var trail in data)
            {
                i++;
                result = result + $@"
                    <tr>
                        <td>{i}</td>
                        <td>{trail.loanReferenceNumber}</td>
                        <td>{trail.customerName.ToUpper()}</td>
                        <td>{trail.address}</td>
                        <td>{trail.totalAmountRecovery}</td>
                        <td>{trail.branchName}</td>
                    </tr>
                ";
            }

            result = result + $"</table>";
            return result;

        }


        public bool InitRecoveryDate(int accreditedConsultantId, string referenceId)
        {
            this.recoveryAnalysisData = GetOutstandingLoans(accreditedConsultantId,referenceId);

            return true;
        }

        public string ExceptionMemoMarkupHtml(int customerId, int loanApplicationId, int company)
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br />
                <h3><b>MEMO</b></h3>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>
                    <tr>
                        <td><b>Date</b></td>
                        <td>{DateTime.UtcNow}</td>
                    </tr>
                    <tr>
                        <td><b>To:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>From:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>Location:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>Subject:</b></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td><b>No. Of Pages:</b></td>
                        <td></td>
                    </tr>
                 ";
            result = result + $"</table>";
            result = result + $@" 
                    <p></p>
                    <p><b>1. BACKGROUND</b></p>
                    <p></p>
                    <p><b>2. COLLATERAL</b></p>
                    <p>{GetAllExceptionCustomerCollateralsMarkup(customerId, loanApplicationId, company)}</p>
                    <p><b>3. ACCOUNT STATUS/ANALYSIS</b></p>
                    <p>{ExceptionCustomerAccountActivityHtml(customerId, loanApplicationId)}</p>
                    <p><b>4. ISSUES</b></p>
                    <p></p>
                    <p><b>5. CURRENT UPDATES</b></p>
                    <p></p>
                    <p><b>6. REQUEST/RECOMMENDATION</b></p>
                    <p></p>
                    <p><b>7. JUSTIFICATION</b></p>";
            return result;
        }


        public bool InitForExceptionalLoans(int operationId, int targetId) // feeder
        {
            this.targetId = targetId;
            this.operationId = operationId;
                var exceptionalLoan = context.TBL_EXCEPTIONAL_LOAN_APPLICATION.Find(targetId);
                var exceptionalLoanDetail = context.TBL_EXCEPTIONAL_LOAN_APPL_DETAIL.Where(x=>x.EXCEPTIONALLOANAPPLICATIONID == exceptionalLoan.EXCEPTIONALLOANAPPLICATIONID).Select(x=>x).FirstOrDefault();
                this.interestRate = exceptionalLoanDetail.PROPOSEDINTERESTRATE;
                this.customerRecord = context.TBL_CUSTOMER.Find(exceptionalLoanDetail.CUSTOMERID);
                if (customerRecord != null) this.customerName = customerRecord.FIRSTNAME + " " + customerRecord.MIDDLENAME + " " + customerRecord.LASTNAME;
                var customerFacilitiesLms = context.TBL_EXCEPTIONAL_LOAN_APPL_DETAIL.Where(f => f.DELETED == false && f.CUSTOMERID == customerRecord.CUSTOMERID).ToList();
                this.tenor = exceptionalLoanDetail.PROPOSEDTENOR;
                this.branchName = context.TBL_BRANCH.Find(exceptionalLoan.BRANCHID).BRANCHNAME;
                this.locationName = context.TBL_BRANCH.Find(exceptionalLoan.BRANCHID).ADDRESSLINE1 + " " + context.TBL_BRANCH.Find(exceptionalLoan.BRANCHID).ADDRESSLINE2;
                this.dateCreated = exceptionalLoan.DATETIMECREATED.ToShortDateString();
                this.rmCountry = context.TBL_COMPANY.Find(exceptionalLoan.COMPANYID).TBL_COUNTRY.NAME;
                
                this.reviewType = "Annual";
                this.businessSectors = GetExceptionBusinessSectorsMarkup(customerRecord.CUSTOMERID);
                this.approvals = GetExceptionalApprovalsMarkup(exceptionalLoanDetail.EXCEPTIONALLOANAPPLDETAILID, operationId);
                this.currentDate = DateTime.Now.ToShortDateString();
               this.exceptionMemoData = ExceptionMemoMarkupHtml(this.customerRecord.CUSTOMERID, exceptionalLoan.EXCEPTIONALLOANAPPLICATIONID, exceptionalLoan.COMPANYID);

            if (this.customerIds?.Count > 0)
            {
                this.accountNumbers = AccountNumbersMarkup(this.customerIds?.Select(x => x.customerId).ToList());
            }

            this.approvalLevel = GetApprovalLevel();

            return true;
        }

        private string GetAllExceptionCustomerCollateralsMarkup(int customerId, int loanApplicationId, int company)
        {
            var result = String.Empty;
            var remark = string.Empty;
            
                var customerCollaterals = GetExceptionCustomerCollateral(customerId, loanApplicationId, company);

                result += $@"
                        <ul>
                        ";
                foreach (var cc in customerCollaterals)
                {
                    result += $@"
                            <li>{cc.collateralSummary}</li>
                        ";
                }
                result += $@"
                        </ul>
                        ";
            
            return result;
        }

        public IEnumerable<CollateralViewModel> GetExceptionCustomerCollateral(int customerId, int? applicationId, int companyId)
        {
            var typeIds = new List<int>();
            var company = context.TBL_COMPANY.Find(companyId);
            var baseCurrencyId = company.TBL_CURRENCY.CURRENCYID;
            bool disAllowCollateral = false;
            bool isForiegnCurrencyFacility = false;
            var productIds = new List<short>();
            if (applicationId != null && applicationId != 0)
            {
                
                    productIds = context.TBL_EXCEPTIONAL_LOAN_APPL_DETAIL
                    .Where(x => x.EXCEPTIONALLOANAPPLICATIONID == applicationId)
                    .Select(x => x.PROPOSEDPRODUCTID)
                    .Distinct().ToList();
                    isForiegnCurrencyFacility = context.TBL_EXCEPTIONAL_LOAN_APPL_DETAIL.Where(x => x.CURRENCYID != company.CURRENCYID).Any();
                

                typeIds = context.TBL_PRODUCT_COLLATERALTYPE.Where(x => productIds.Contains(x.PRODUCTID))
                   .Select(x => x.COLLATERALTYPEID)
                   .Distinct().ToList();

            }

            var collaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false && x.CUSTOMERID == customerId)
                .GroupJoin(
                    context.TBL_LOAN_COLLATERAL_MAPPING,
                    c => c.COLLATERALCUSTOMERID,
                    lc => lc.COLLATERALCUSTOMERID,
                    (c, lc) => new { c, m = lc }
                )
                .SelectMany
                (
                    x => x.m.DefaultIfEmpty(),
                    (c, m) => new CollateralViewModel
                    {
                        collateralId = c.c.COLLATERALCUSTOMERID,
                        collateralTypeId = c.c.COLLATERALTYPEID,
                        collateralSubTypeId = c.c.COLLATERALSUBTYPEID,
                        customerId = c.c.CUSTOMERID,
                        customerCode = c.c.CUSTOMERCODE,
                        customerName = c.c.TBL_CUSTOMER.FIRSTNAME + c.c.TBL_CUSTOMER.MIDDLENAME + c.c.TBL_CUSTOMER.LASTNAME,
                        currencyId = c.c.CURRENCYID,
                        currencyCode = c.c.TBL_CURRENCY.CURRENCYCODE,
                        baseCurrencyId = company.CURRENCYID,
                        baseCurrencyCode = (c.c.CURRENCYID == baseCurrencyId) ? "" : company.TBL_CURRENCY.CURRENCYCODE,//so that only fcy will show
                        currency = c.c.TBL_CURRENCY.CURRENCYNAME,
                        disAllowCollateral = disAllowCollateral && c.c.CURRENCYID == company.CURRENCYID, // facilityCurrency != baseCurrency && collateralCurrency == baseCurrency
                        collateralTypeName = c.c.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                        collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(r => r.COLLATERALSUBTYPEID == c.c.COLLATERALSUBTYPEID).Select(q => q.COLLATERALSUBTYPENAME).FirstOrDefault(),
                        collateralCode = c.c.COLLATERALCODE,
                        collateralValue = c.c.COLLATERALVALUE,
                        camRefNumber = c.c.CAMREFNUMBER,
                        allowSharing = c.c.ALLOWSHARING,
                        isLocationBased = c.c.ISLOCATIONBASED ?? false,
                        valuationCycle = c.c.VALUATIONCYCLE,
                        haircut = c.c.HAIRCUT,
                        approvalStatusName = c.c.APPROVALSTATUS,
                        allowApplicationMapping = typeIds.Contains((short)c.c.COLLATERALTYPEID),
                        requireInsurancePolicy = c.c.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
                        exchangeRate = c.c.EXCHANGERATE,
                        collateralReleaseStatusId = c.c.COLLATERALRELEASESTATUSID,
                        collateralReleaseStatusName = c.c.COLLATERALRELEASESTATUSID == null ? context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == (int)CollateralReleaseStatus.InVault).FirstOrDefault().COLLATERALRELEASESTATUSNAME : context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == c.c.COLLATERALRELEASESTATUSID).FirstOrDefault().COLLATERALRELEASESTATUSNAME,
                        accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == customerId).ACCOUNTNUMBER,
                        collateralUsageStatus = c.c.COLLATERALUSAGESTATUSID,
                        loanApplicationId = applicationId, //c.c.LOANAPPLICATIONID,
                        collateralSummary = c.c.COLLATERALSUMMARY,
                        isMapped = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.COLLATERALCUSTOMERID == c.c.COLLATERALCUSTOMERID && o.DELETED == false).Any(),
                        isProposed = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == c.c.COLLATERALCUSTOMERID && o.DELETED == false).Any(),
                        companyId = companyId,//remark = c.c.
                        validTill = c.c.VALIDTILL,
                    })
                    .ToList()
                    .GroupBy(x => x.collateralId).Select(g => g.First());

            return collaterals.OrderByDescending(x => x.collateralId);
        }

        private string GetExceptionBusinessSectorsMarkup(int customerId)
        {
            var result = String.Empty;
            string sectorName;
            var cust = context.TBL_CUSTOMER.Find(customerId);
            sectorName = context.TBL_SUB_SECTOR.Find(cust.SUBSECTORID)?.TBL_SECTOR.NAME;
            result += sectorName;
            return result;
        }

        private string GetExceptionalApprovalsMarkup(int targetId, int operationId)
        {
            var appraisals = GetAppraisalMemorandumTrail(targetId, operationId,false).OrderBy(a => a.approvalTrailId).ToList();
            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=1000px align=center cellpadding=0 cellspacing=0>
                    <tr>
                        <th><b>Role</b></th>
                        <th><b>Name</b></th>
                        <th><b>Decision</b></th>
                        <th><b>Comment</b></th>
                        <th><b>Date</b></th>
                    </tr>
                    ";
            foreach (var trail in appraisals)
            {
                result = result + $@"
                    <tr>
                        <td>{trail.fromApprovalLevelName.ToUpper()}</td>
                        <td>{trail.fromStaffName}</td>
                        <td>{GetDecision(trail.approvalStatusId)}</td>
                        <td>{trail.comment}</td>
                        <td>{trail.systemArrivalDateTime}</td>
                    </tr>
                ";
            }

            result = result + $"</table>";
            return result;

        }

        public string ExceptionCustomerAccountActivityHtml(int customerId, int loanApplicationId)
        {
            var accountActivity = GetCustomerTransactions(customerId, loanApplicationId, false);
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br />
                <h4><b>Account Activity with Current (Major) Banker per period of 6 months</b></h4>
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th><b>Account Number</b></th>
                        <th><b>Product Account Name</b></th>
                        <th><b>Period</b></th>
                        <th><b>Max Debit Balance</b></th>
                        <th><b>Min Debit Balance</b></th>
                        <th><b>Max Credit Balance</b></th>
                        <th><b>Min Credit Balance</b></th>
                        <th><b>Debit Turnover</b></th>
                        <th><b>Credit Turnover</b></th>
                        <th><b>Month</b></th>
                        <th><b>Year</b></th>
                    </tr>";
            foreach (var f in accountActivity)
            {
                result = result + $@"
                        <tr>
                        <td> {f.accountNumber}</td>
                        <td> {f.productAccountName}</td>
                        <td> {f.period}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.max_Debit_Balance))}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.min_Debit_Balance))}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.max_Credit_Balance))}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.min_Credit_Balance))}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.debit_Turnover))}</td>
                        <td> {string.Format("{0:#,##.00}", Convert.ToDecimal(f.credit_Turnover))}</td>
                        <td> {f.month}</td>
                        <td> {f.year}</td>
                    </tr>";

            }

            result = result + $"</table>";
            result = result + $@"
                 <br />";
            return result;
        }


        public IEnumerable<ApprovalTrailViewModel> GenericLMSApprovalTrail(int targetId, int operationId)
        {
            var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;

            var allstaff = this.GetAllStaffNames();
            var data = (from x in context.TBL_APPROVAL_TRAIL
                        where
                        x.OPERATIONID == operationId
                        && x.TARGETID == targetId
                        //&& x.FROMAPPROVALLEVELID != null
                        select new ApprovalTrailViewModel
                        {
                            approvalTrailId = x.APPROVALTRAILID,
                            comment = x.COMMENT,
                            targetId = x.TARGETID,
                            vote = x.VOTE,
                            arrivalDate = x.ARRIVALDATE,
                            systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                            responseDate = x.RESPONSEDATE,
                            systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                            responseStaffId = x.RESPONSESTAFFID,
                            requestStaffId = x.REQUESTSTAFFID,
                            fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                            fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                            toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                            toApprovalLevelId = (int)x.TOAPPROVALLEVELID,
                            approvalStateId = x.APPROVALSTATEID,
                            approvalStatusId = x.APPROVALSTATUSID,
                            commentStage = "Credit Appaisal",
                            approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                            approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                            toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                            fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
                        }).OrderBy(x => x.approvalTrailId).ToList();

            var initiation = data.FirstOrDefault();
            if (initiation?.fromApprovalLevelId == null)
            {
                //data.Remove(initiation);
                data = data.OrderByDescending(d => d.approvalTrailId).ToList();
            }

            var applicationDetail = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => x).FirstOrDefault();
            var reviewDetail = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANREVIEWAPPLICATIONID == applicationDetail.LOANREVIEWAPPLICATIONID).Select(x => x).FirstOrDefault();
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.LoanReviewApprovalAvailment, "Availment"));
            if (reviewDetail != null)
            {
                data.AddRange(GetNonAppraisalTrail(reviewDetail.LOANREVIEWOPERATIONID, reviewDetail.OPERATIONTYPEID, "Credit Operations"));
            }
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.LoanReviewDrawdownForExtension, "Loan Review Drawdown"));
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.ContingentReviewDrawdownForExtension, "Contingent Review Drawdown"));
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.OverdraftReviewDrawdownForExtension, "Overdraft Review Drawdown"));

            data.OrderByDescending(d => d.approvalTrailId);
            foreach (var d in data)
            {

                if (d.fromApprovalLevelId == d.toApprovalLevelId)
                {
                    if (d.loopedStaffId > 0)
                    {
                        d.toApprovalLevelName = staffs.FirstOrDefault(s => s.STAFFID == d.loopedStaffId).TBL_STAFF_ROLE.STAFFROLENAME;
                    }
                    else
                    {
                        d.fromApprovalLevelName = staffs.FirstOrDefault(s => s.STAFFID == d.requestStaffId).TBL_STAFF_ROLE.STAFFROLENAME;
                    }
                }
            }

            return data;

            /*var staffRoles = context.TBL_STAFF_ROLE.ToList();
            var staffs = from s in context.TBL_STAFF select s;

            var allstaff = this.GetAllStaffNames();
            var data = (from x in context.TBL_APPROVAL_TRAIL
                         where
                         x.OPERATIONID == operationId 
                         && x.TARGETID == targetId
                         //&& x.FROMAPPROVALLEVELID != null
              select new ApprovalTrailViewModel
              {
                approvalTrailId = x.APPROVALTRAILID,
                comment = x.COMMENT,
                targetId = x.TARGETID,
                vote = x.VOTE,
                arrivalDate = x.ARRIVALDATE,
                systemArrivalDateTime = x.SYSTEMARRIVALDATETIME,
                responseDate = x.RESPONSEDATE,
                systemResponseDateTime = x.SYSTEMRESPONSEDATETIME,
                responseStaffId = x.RESPONSESTAFFID,
                requestStaffId = x.REQUESTSTAFFID,
                fromApprovalLevelId = x.FROMAPPROVALLEVELID,
                fromApprovalLevelName = x.FROMAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelName = x.TOAPPROVALLEVELID == null ? "N/A" : context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == x.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                toApprovalLevelId = (int)x.TOAPPROVALLEVELID,
                approvalStateId = x.APPROVALSTATEID,
                approvalStatusId = x.APPROVALSTATUSID,
                commentStage = "Credit Appaisal",
                approvalState = x.TBL_APPROVAL_STATE.APPROVALSTATE,
                approvalStatus = x.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                toStaffName = allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.RESPONSESTAFFID).name,
                fromStaffName = allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID) == null ? "N/A" : allstaff.FirstOrDefault(s => s.id == x.REQUESTSTAFFID).name,
            }).OrderBy(x => x.approvalTrailId).ToList();

            var initiation = data.FirstOrDefault();
            if (initiation?.fromApprovalLevelId == null)
            {
                //data.Remove(initiation);
                data = data.OrderByDescending(d => d.approvalTrailId).ToList();
            }

            var applicationDetail = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => x).FirstOrDefault();
            var reviewDetail = context.TBL_LOAN_REVIEW_OPERATION.Where(x => x.LOANREVIEWAPPLICATIONID == applicationDetail.LOANREVIEWAPPLICATIONID).Select(x => x).FirstOrDefault();
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.LoanReviewApprovalAvailment, "Availment"));
            if (reviewDetail != null)
            {
                data.AddRange(GetNonAppraisalTrail(reviewDetail.LOANREVIEWOPERATIONID, reviewDetail.OPERATIONTYPEID, "Credit Operations"));
            }
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.LoanReviewDrawdownForExtension, "Loan Review Drawdown"));
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.ContingentReviewDrawdownForExtension, "Contingent Review Drawdown"));
            data.AddRange(GetNonAppraisalTrail(targetId, (short)OperationsEnum.OverdraftReviewDrawdownForExtension, "Overdraft Review Drawdown"));

            data.OrderByDescending(d => d.approvalTrailId);
            foreach (var d in data)
            {

                if (d.fromApprovalLevelId == d.toApprovalLevelId)
                {
                    if (d.loopedStaffId > 0)
                    {
                        d.toApprovalLevelName = staffs.FirstOrDefault(s => s.STAFFID == d.loopedStaffId).TBL_STAFF_ROLE.STAFFROLENAME;
                    }
                    else
                    {
                        d.fromApprovalLevelName = staffs.FirstOrDefault(s => s.STAFFID == d.requestStaffId).TBL_STAFF_ROLE.STAFFROLENAME;
                    }
                }
            }

            return data;*/
        }


    }
}




/*
    Obligor Risk Rating:
    Industry Risk Rating:
    Review Type – Annual/Interim/Initial

*/
