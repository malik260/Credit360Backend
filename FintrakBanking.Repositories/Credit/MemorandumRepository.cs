using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class MemorandumRepository : IMemorandumRepository
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

        public MemorandumRepository(
            FinTrakBankingContext context, 
            IAppraisalMemorandumRepository memo, 
            ILoanRepository loanRepo,     
            IFinanceTransactionRepository financeTransaction,
            ICustomerGroupRepository groupRepo, 
            ITransactionDynamicsRepository transactionsRepo,
            IConditionPrecedentRepository conditionsRepo,
            ICustomerCollateralRepository collateralRepo
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
        }

        // init
        private int targetId;
        private int operationId;
        List<CustomerExposure> customerIds; // init

        // field variables
        TBL_LOAN_APPLICATION loanApplication = null;
        TBL_LOAN_APPLICATION_DETAIL loanApplicationDetail = null;
        TBL_LMSR_APPLICATION lmsrApplication = null;
        List<TBL_LOAN_APPLICATION_DETAIL> customerFacilities = null;
        int customerId;
        private List<int> lmsCamOperationIds = new List<int> { 46, 71, 79 };
        private long legalLendingLimit = 200000000000;

        // place holders
        private readonly string customerNameHolder = "@{{CustomerName}}";
        private readonly string branchNameHolder = "@{{Branch}}";
        private readonly string locationNameHolder = "@{{Location}}";
        private readonly string customerExposureHolder = "@{{CustomerExposure}}";
        private readonly string recommendedInterestRateHolder = "@{{RecommendedInterest}}";
        private readonly string isRelatedPartyHolder = "@{{IsRelatedParty}}";
        private readonly string dateCreatedHolder = "@{{DateCreated}}";
        private readonly string accountNumbersHolder = "@{{AccountNumbers}}";
        private readonly string approvalLevelHolder = "@{{ApprovalLevel}}";
        private readonly string environmentalSocialRiskHolder = "@{{EnvironmentalSocialRisk}}";
        private readonly string monitoringTriggersHolder = "@{{MonitoringTriggers}}";
        private readonly string proposedConditionsHolder = "@{{ProposedConditions}}";
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
        private readonly string groupFacilitySummaryFcyHolder = "@{{GroupFacilitySummaryFcy}}";
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
        private readonly string facilityUpgradeSupportSchemeHolder = "@{{facilityUpgradeSupportSchemeData}}";
        private readonly string invoiceDiscountingDataHolder = "@{{invoiceDiscountingData}}";
        private readonly string cashCollaterizedDataHolder = "@{{cashCollaterizedData}}";
        private readonly string temporaryOverdraftHolder = "@{{temporaryOverdraftData}}";
        private readonly string staffCarLoansDataHolder = "@{{staffCarLoansData}}";
        private readonly string staffMortgageLoansDataHolder = "@{{staffMortgageLoansData}}";
        private readonly string staffPersonalLoansAGMDataHolder = "@{{staffPersonalLoansAGMData}}";
        private readonly string staffPersonalLoanDataHolder = "@{{staffPersonalLoanData}}";

        // properties to have getter methods for interfacing
        private string customerName;
        private string branchName;
        private string locationName;
        private string customerExposure;
        private string recommendedInterestRate;
        private string isRelatedParty;
        private string dateCreated;
        private string accountNumbers;
        private string approvalLevel;
        private string environmentalSocialRisk;
        private string monitoringTriggers;
        private string proposedConditions;
        private string conditionsPrecedentToDrawdown;
        private string transactionsDynamics;
        private string rmCountry;
        private string misCode;
        private string reviewType;
        private string preparedBy;
        private string businessSectors;
        private string exchangeRate;
        private string groupFacilitySummary;
        private string groupFacilitySummaryFcy;
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
        private string currentDate;
        private string annualReviewDate;
        private string securityAnalysis;
        private string allCustomerCollateralRemarks;
        private string collateralCoverage;
        private string allCustomerFacilities;
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
        private int tenor;
        private int? moratorium;
        private string principalRepayment;
        private string interestRepayment;
        private double interestRate;
        private string processingFee;
        private string managementFee;
        private string commitmentFee;
        private string otherFee;
        private string effectiveDate;
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
        private string facilityUpgradeSupportSchemeData;
        private string invoiceDiscountingData;
        private string cashCollaterizedData;
        private string staffcarLoansData;
        private string staffMortgageLoansData;
        private string staffPersonalLoansAGMData;
        private string staffPersonalLoanData;
        private string temporaryOverdraftData;

        // init
        public bool Init(int operationId, int targetId, bool isDrawdwon = false) // feeder
        {
            if (isDrawdwon)
            {
                return InitializeDrawdownMemoProperties(operationId, targetId);
            }

            this.targetId = targetId;
            this.operationId = operationId;
            if (operationId == (int)OperationsEnum.CreditAppraisal) // LOS 
            {
                if (loanApplication == null)
                {
                    this.loanApplication = context.TBL_LOAN_APPLICATION.Find(targetId);
                    this.customerIds = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerExposure { customerId = x.CUSTOMERID }).Distinct().ToList();
                    this.customerExposure = CustomerExposureMarkup();
                }

                //string customerName = String.Empty;
                if (loanApplication.CUSTOMERGROUPID != null)
                {
                    this.customerName = loanApplication.TBL_CUSTOMER_GROUP.GROUPNAME;
                    this.customerId = (int)loanApplication.CUSTOMERGROUPID;
                }
                if (loanApplication.CUSTOMERID != null)
                {
                    this.customerName = loanApplication.TBL_CUSTOMER.FIRSTNAME + " " + loanApplication.TBL_CUSTOMER.MIDDLENAME + " " + loanApplication.TBL_CUSTOMER.LASTNAME;
                    this.customerId = (int)loanApplication.CUSTOMERID;
                }

                this.customerFacilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.DELETED == false && f.CUSTOMERID == this.customerId).ToList();
                this.branchName = loanApplication.TBL_BRANCH.BRANCHNAME;
                this.locationName = loanApplication.TBL_BRANCH.ADDRESSLINE1 + " " + loanApplication.TBL_BRANCH.ADDRESSLINE2;
                this.isRelatedParty = loanApplication.ISRELATEDPARTY == true ? "Yes" : "No";
                this.recommendedInterestRate = loanApplication.INTERESTRATE.ToString();
                this.dateCreated = loanApplication.DATETIMECREATED.ToShortDateString();
                this.environmentalSocialRisk = GetEnvironmentalSocialRiskMarkup();
                this.conditionsPrecedentToDrawdown = GetConditionsPrecedentToDrawdownMarkup();
                this.transactionsDynamics = GetTransactionsDynamicsMarkup();
                this.rmCountry = this.loanApplication.TBL_BRANCH.TBL_STATE.TBL_COUNTRY.NAME;
                this.misCode = this.loanApplication.MISCODE;
                this.reviewType = "Initial";
                this.preparedBy = this.loanApplication.TBL_STAFF.FIRSTNAME + " " + this.loanApplication.TBL_STAFF.LASTNAME;
                this.businessSectors = GetBusinessSectorsMarkupLOS();
                this.exchangeRate = GetAllExchangeRates();
                this.groupFacilitySummary = GetGroupFacilitySummaryMarkupLOS();
                this.groupFacilitySummaryFcy = GetGroupFacilitySummaryFCYMarkupLOS();
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
                this.approvals = GetApprovalsMarkupLOS();
                this.currentDate = DateTime.Now.ToShortDateString();
                this.annualReviewDate = this.loanApplication.APPLICATIONDATE.AddYears(1).ToShortDateString();
                this.securityAnalysis = this.GetSecurityAnalysisMarkUP();
                this.collateralCoverage = GetCollateralCoverageMarkupLOS();
                this.allCustomerCollateralRemarks = GetAllCustomerCollateralsMarkup();
                this.allCustomerFacilities = GetAllCustomerFacilitiesMarkup();
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
                
    }

            if (lmsCamOperationIds.Contains(operationId)) // LMS
            {
                if (lmsrApplication == null)
                {
                    this.lmsrApplication = context.TBL_LMSR_APPLICATION.Find(targetId);
                    this.customerIds = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerExposure { customerId = x.CUSTOMERID }).Distinct().ToList();
                    this.customerExposure = CustomerExposureMarkup();
                }

                //string customerName = String.Empty;
                // if (lmsrAppllication.CUSTOMERGROUPID != null) this.customerName = lmsrAppllication.TBL_CUSTOMER_GROUP.GROUPNAME;
                if (lmsrApplication.CUSTOMERID != null) this.customerName = lmsrApplication.TBL_CUSTOMER.FIRSTNAME + " " + lmsrApplication.TBL_CUSTOMER.MIDDLENAME + " " + lmsrApplication.TBL_CUSTOMER.LASTNAME;

                this.branchName = lmsrApplication.TBL_BRANCH.BRANCHNAME;
                this.locationName = lmsrApplication.TBL_BRANCH.ADDRESSLINE1 + " " + lmsrApplication.TBL_BRANCH.ADDRESSLINE2;
                //this.isRelatedParty = lmsrAppllication.ISRELATEDPARTY == true ? "Yes" : "No";
                //this.recommendedInterestRate = lmsrAppllication.INTERESTRATE.ToString();
                this.dateCreated = lmsrApplication.DATETIMECREATED.ToShortDateString();
                this.rmCountry = this.lmsrApplication.TBL_BRANCH.TBL_STATE.TBL_COUNTRY.NAME;
                //this.misCode = this.lmsrAppllication.MISCODE;
                this.reviewType = "Annual";
                //this.preparedBy = this.lmsrAppllication.TBL_STAFF.FIRSTNAME + " " + this.lmsrAppllication.TBL_STAFF.LASTNAME;
                this.businessSectors = GetBusinessSectorsMarkupLMS();
                //this.exchangeRate = context.TBL_LMSR_APPLICATION_DETAIL.FirstOrDefault().EXCHANGERATE.ToString();
                this.groupFacilitySummary = GetGroupFacilitySummaryMarkupLOS();
                this.groupFacilitySummaryFcy = GetGroupFacilitySummaryFCYMarkupLOS();//this.directFacilities = GetDirectFacilitiesMarkupLMS();
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
                this.groupExposure = GetGroupExposureMarkupLMS();
                this.approvals = GetApprovalsMarkupLOS();
                this.currentDate = DateTime.Now.ToShortDateString();
                this.annualReviewDate = this.loanApplication.APPLICATIONDATE.AddYears(1).ToShortDateString();
                this.securityAnalysis = this.GetSecurityAnalysisMarkUP();
                this.collateralCoverage = GetCollateralCoverageMarkupLOS();

                // out ducument properties definition
                /*this.memoData = MemoMarkupHtml();
                this.facilityUpgradeSupportSchemeData = Fa;
                this.invoiceDiscountingData;
                this.cashCollaterizedData;
                this.staffcarLoansData;
                this.staffMortgageLoansData;
                this.staffPersonalLoansAGMData;
                this.staffPersonalLoanData;*/

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

            this.accountNumbers = AccountNumbersMarkup(this.customerIds.Select(x => x.customerId).ToList());
            this.approvalLevel = GetApprovalLevel();
            this.proposedConditions = GetProposedConditionsMarkup();
            this.monitoringTriggers = MonitoringTriggersMarkup();
            //this.customerTurnover = CustomerTurnoverMarkup(); // lazy loaded

            return true;
        }

        private bool InitializeDrawdownMemoProperties(int operationId, int targetId) // feeder
        {
            this.targetId = targetId;
            this.operationId = operationId;

            //if (operationId == (int) OperationsEnum.CreditAppraisal) // LOS 
            //{
                if (loanApplicationDetail == null)
                {
                    this.loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(targetId);
                    this.loanApplication = loanApplicationDetail.TBL_LOAN_APPLICATION;
                    //this.customerIds = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerExposure { customerId = x.CUSTOMERID }).Distinct().ToList();
                    //this.customerExposure = CustomerExposureMarkup();
                }

                string customerName = String.Empty;
                if (loanApplication.CUSTOMERGROUPID != null) this.customerName = loanApplication.TBL_CUSTOMER_GROUP.GROUPNAME;
                if (loanApplication.CUSTOMERID != null) this.customerName = loanApplication.TBL_CUSTOMER.FIRSTNAME + " " + loanApplication.TBL_CUSTOMER.MIDDLENAME + " " + loanApplication.TBL_CUSTOMER.LASTNAME;

                this.branchName = loanApplication.TBL_BRANCH.BRANCHNAME;
                this.currentAccountNo = context.TBL_CASA.Where(O => O.CASAACCOUNTID == loanApplicationDetail.CASAACCOUNTID).Select(O => O.PRODUCTACCOUNTNUMBER).FirstOrDefault();
                this.facilityType = context.TBL_PRODUCT.Where(O => O.PRODUCTID == loanApplicationDetail.APPROVEDPRODUCTID).Select(O => O.PRODUCTNAME).FirstOrDefault();
                this.drawdownAmount = loanApplicationDetail.APPROVEDAMOUNT.ToString("#,##.00");
                this.tenor = loanApplicationDetail.APPROVEDTENOR;
                this.moratorium = loanApplicationDetail.MORATORIUMDURATION;
                this.principalRepayment = "";
                this.interestRepayment = loanApplicationDetail.REPAYMENTTERMS;
                this.interestRate = loanApplicationDetail.APPROVEDINTERESTRATE;
                this.processingFee = ""; //context.TBL_LOAN_APPLICATION_DETL_FEE.Where(O => O.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID).Select(O => O.TBL_CHARGE_FEE).FirstOrDefault();
                this.managementFee = "";
                this.commitmentFee = "";
                this.otherFee = "";
                this.effectiveDate = "";
                this.misCode = loanApplicationDetail.TBL_LOAN_APPLICATION.MISCODE;

                approvedAmount = loanApplicationDetail.APPROVEDAMOUNT.ToString("#,##.00");
                amountUtilised = "0.00";

                newRequest = context.TBL_LOAN_BOOKING_REQUEST.Where(O => O.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID).FirstOrDefault() == null ? "0.00" : context.TBL_LOAN_BOOKING_REQUEST.Where(O => O.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID).FirstOrDefault().AMOUNT_REQUESTED.ToString("#,##.00");

                requestType = "";

                inPlace1 = "";
                perfected1 = "";
                deferred1 = "";
                inPlace2 = "";
                perfected2 = "";
                deferred2 = "";
                inPlace3 = "";
                perfected3 = "";
                deferred3 = "";
                inPlace4 = "";
                perfected4 = "";
                deferred4 = "";
                inPlace5 = "";
                perfected5 = "";
                deferred5 = "";
                inPlace6 = "";
                perfected6 = "";
                deferred6 = "";
                inPlace7 = "";
                perfected7 = "";
                deferred7 = "";

                relationshipOfficer = "";
                relationshipManager = "";
                riskManagement = "";
                legal = "";
                treasury = "";
                coo = "";
                crmInternational = "";

                othersInPlace1 = "";
                othersPerfected1 = "";
                othersDeferred1 = "";
                othersInPlace2 = "";
                othersPerfected2 = "";
                othersDeferred2 = "";
                othersInPlace3 = "";
                othersPerfected3 = "";
                othersDeferred3 = "";
                othersInPlace4 = "";
                othersPerfected4 = "";
                othersDeferred4 = "";
                othersInPlace5 = "";
                othersPerfected5 = "";
                othersDeferred5 = "";
                othersInPlace6 = "";
                othersPerfected6 = "";
                othersDeferred6 = "";
                othersInPlace7 = "";
                othersPerfected7 = "";
                othersDeferred7 = "";

            //}

            return true;
        }

        
        public List<DropDownSelect> GetProposedConditions()
        {
            var result = new List<DropDownSelect>();
            if (operationId == (int)OperationsEnum.CreditAppraisal)
            {
                var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId);
                foreach (var d in details)
                {
                    result.Add(new DropDownSelect { id = d.LOANAPPLICATIONDETAILID, name = "TRANSACTION DYNAMICS: " + d.TRANSACTIONDYNAMICS });
                    result.Add(new DropDownSelect { id = d.LOANAPPLICATIONDETAILID, name = "CONDITION PRECEDENT: " + d.CONDITIONPRECIDENT });
                    result.Add(new DropDownSelect { id = d.LOANAPPLICATIONDETAILID, name = "CONDITION SUBSEQUENT: " + d.CONDITIONSUBSEQUENT });
                }
            }
            return result;
        }

        public List<DropDownSelect> GetConditionsPrecedentToDrawdown()
        {
            var result = new List<DropDownSelect>();
            if (operationId == (int)OperationsEnum.CreditAppraisal)
            {
                var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).ToList();
                var detail = this.conditionsRepo.GetAllConditionPrecedent().Where(x => x.loanApplicationDetailId == details.FirstOrDefault()?.LOANAPPLICATIONDETAILID);
                foreach (var d in detail)
                {
                    if (d.condition != null)
                    {
                        result.Add(new DropDownSelect { id = d.conditionId, name = d.condition });
                    }
                }
            }
            return result;
        }

        public List<DropDownSelect> GetTransactionsDynamics()
        {
            var result = new List<DropDownSelect>();
            if (operationId == (int)OperationsEnum.CreditAppraisal)
            {

                var details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).ToList();
                var allTransactions = this.transactionsRepo.GetAllTransactionDynamics().OrderBy(a => a.position);
                var transactions = new List<TransactionDynamicsViewModel>();
                foreach (var d in details)
                {
                    var transactionSelect = allTransactions.Where(x => x.loanApplicationDetailId == d.LOANAPPLICATIONDETAILID).ToList();
                    transactions.AddRange(transactionSelect);
                }
                foreach (var t in transactions)
                {
                    if (t.dynamics != null)
                    {
                        result.Add(new DropDownSelect { typeId = (int)t.loanApplicationDetailId, name = t.dynamics });
                    }
                }
            }
            return result;
        }

        private List<TotalFacilitiesSummaryViewModel> GetTotalFacilitiesNGNLOS()
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

        private List<TotalFacilitiesSummaryViewModel> GetTotalForeignFacilitiesLOS()
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

        private string getIsDirectorRelated()
        {
            if (this.loanApplication.ISRELATEDPARTY)
            {
                return "Yes";
            }
            return "No";
        }

        private string getIsDirectorRelatedLMS()
        {
            return "";
        }

        private decimal getTotalLLLImpact()
        {
            var totalSummary = GetTotalFacilitiesNGNLOS();
            var totalSummary2 = GetTotalForeignFacilitiesLOS();
            totalSummary.AddRange(totalSummary2);
            return totalSummary.Sum(f => f.totalLLLImpact);
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
                name = s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME
            });
        }

        public IEnumerable<ApprovalTrailViewModel> GetAppraisalMemorandumTrail(int applicationId)
        {

            //int[] operations = { (int)OperationsEnum.TermLoanBooking, (int)OperationsEnum.CAM, (int)OperationsEnum.InterestPastDueLoanRepayment,
            //        (int)OperationsEnum.RevolvingLoanBooking, (int)OperationsEnum.ContigentLoanBooking,(int)OperationsEnum.OfferLetterApproval,
            //    (int)OperationsEnum.LoanAvailment,(int)OperationsEnum.LoanTrancheBookingRequest,(int)OperationsEnum.BondsAndGuarantees,
            //        (int)OperationsEnum.CommercialLoanBooking,(int)OperationsEnum.ForeignExchangeLoanBooking,(int)OperationsEnum.LoanAndOverdraftRequestBooking
            //    ,(int)OperationsEnum.ContigentLoanBooking,(int)OperationsEnum.CustomerInformationApproval};

            var allstaff = this.GetAllStaffNames();

            var trail = context.TBL_APPROVAL_TRAIL.Where(x => x.FROMAPPROVALLEVELID != null && x.TARGETID == applicationId);

            //if (getAll)
            //{
            //    trail = context.TBL_APPROVAL_TRAIL.Where(x => x.FROMAPPROVALLEVELID != null && x.TARGETID == applicationId);
            //}

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

            return data;
        }

        public bool IsLLLViolated()
        {
            return ((getTotalLLLImpact() > legalLendingLimit) ? true : false);
        }


        //markups
        
        public string GetDrawdownMemoHtml(int staffId, int operationId, int targetId)
        {
            var isInitialize = InitializeDrawdownMemoProperties(operationId, targetId);

            var result = String.Empty;
            result = result + $@"
                <table border=1 width=900 cellpadding=15 cellspacing=0>
                    <tr>
                        <th><b>NAME OF CUSTOMER:</b></th>
                        <th><b></b></th>
                        <th><b>CURRENT/APG A/C NO:</b></th>
                        <th><b></b></th>
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
                        <td>PROCESSING FEE:</td>
                        <td>{processingFee}</td>
                    </tr>
                    <tr>
                        <td>TENOR:</td>
                        <td>{tenor}</td>
                        <td>MGT FEE:</td>
                        <td>{managementFee}</td>
                    </tr>
                    <tr>
                        <td>MORATORIUM:</td>
                        <td>{moratorium}</td>
                        <td>COMMITMENT FEE:</td>
                        <td>{commitmentFee}</td>
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
            result = result + GetTrancheDisbursementHtml() + GetRequestTypeHtml() + GetPrecedentConditionsHtml() + GetApprovalLevelsHtml() + GetOtherConditionsHtml();
            return result;
        }

        public string GetTrancheDisbursementHtml()
        {
             //< tr >
             //           < th >< b ></ b ></ th >
             //           < th >< b ></ b ></ th >
             //           < th >< b ></ b ></ th >
             //       </ tr >
            var result = String.Empty;
            result = result + $@"
                <table border=1 width=900 cellpadding=15 cellspacing=0>
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
             //< tr >
             //           < th >< b ></ b ></ th >
             //           < th >< b ></ b ></ th >
             //       </ tr >

            var result = String.Empty;
            result = result + $@"
                <table border=1 width=900 cellpadding=15 cellspacing=0>
                    <tr>
                        <td><b>REQUEST TYPE</b></td>
                        <td>{requestType}</td>
                    </tr>
                   ";
            result = result + $"</table>";
            return result;
        }

        public string GetPrecedentConditionsHtml()
        {
            var result = String.Empty;
            result = result + $@"
                <br />
                <h3><b>CONDITIONS PRECEDENT TO DRAWDOWN</b></h3>
                <table border=1 width=900 cellpadding=15 cellspacing=0>
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
                    </tr>
                    <tr>
                        <td>1</td>
                        <td>Request letter for the facility</td>
                        <td>{inPlace1}</td>
                        <td>{perfected1}</td>
                        <td>{deferred1}</td>
                    </tr>
                    <tr>
                        <td>2</td>
                        <td>Accepted offer letter</td>
                        <td>{inPlace2}</td>
                        <td>{perfected2}</td>
                        <td>{deferred2}</td>
                    </tr>
                    <tr>
                        <td>3</td>
                        <td>Board resolution accepting the facility (for corporate customers)</td>
                        <td>{inPlace3}</td>
                        <td>{perfected3}</td>
                        <td>{deferred3}</td>
                    </tr>
                    <tr>
                        <td>4</td>
                        <td>Positive CRMS/Credit check</td>
                        <td>{inPlace4}</td>
                        <td>{perfected4}</td>
                        <td>{deferred4}</td>
                    </tr>
                    <tr>
                        <td>5</td>
                        <td>Evidence of payroll mandate (for corporate customers)</td>
                        <td>{inPlace5}</td>
                        <td>{perfected5}</td>
                        <td>{deferred5}</td>
                    </tr>
                    <tr>
                        <td>6</td>
                        <td>Status of Perfection of Mortgage/Debenture</td>
                        <td>{inPlace6}</td>
                        <td>{perfected6}</td>
                        <td>{deferred6}</td>
                    </tr>
                    <tr>
                        <td>7</td>
                        <td>Other conditions as specified on the approval memo/FAM (please see overleaf)</td>
                        <td>{inPlace7}</td>
                        <td>{perfected7}</td>
                        <td>{deferred7}</td>
                    </tr>
                 ";
            result = result + $"</table>";
            return result;
        }

        public string GetApprovalLevelsHtml()
        {
            var result = String.Empty;
            result = result + $@"
                <table border=1 width=900 cellpadding=15 cellspacing=0>
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
                <table border=1 width=900 cellpadding=15 cellspacing=0>
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
                    <tr>
                        <td>2</td>
                        <td></td>
                        <td>{othersInPlace2}</td>
                        <td>{othersPerfected2}</td>
                        <td>{othersDeferred2}</td>
                    </tr>
                    <tr>
                        <td>3</td>
                        <td></td>
                        <td>{othersInPlace3}</td>
                        <td>{othersPerfected3}</td>
                        <td>{othersDeferred3}</td>
                    </tr>
                    <tr>
                        <td>4</td>
                        <td></td>
                        <td>{othersInPlace4}</td>
                        <td>{othersPerfected4}</td>
                        <td>{othersDeferred4}</td>
                    </tr>
                    <tr>
                        <td>5</td>
                        <td></td>
                        <td>{othersInPlace5}</td>
                        <td>{othersPerfected5}</td>
                        <td>{othersDeferred5}</td>
                    </tr>
                    <tr>
                        <td>6</td>
                        <td></td>
                        <td>{othersInPlace6}</td>
                        <td>{othersPerfected6}</td>
                        <td>{othersDeferred6}</td>
                    </tr>
                    <tr>
                        <td>7</td>
                        <td></td>
                        <td>{othersInPlace7}</td>
                        <td>{othersPerfected7}</td>
                        <td>{othersDeferred7}</td>
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
                <table border=1 width=1000px cellpadding=15 cellspacing=0>
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
            var exchangeRates = context.TBL_CURRENCY_EXCHANGERATE.ToList();
            foreach (var x in exchangeRates)
            {
                result = result + $@"
                        {x.TBL_CURRENCY1.CURRENCYCODE}: {x.EXCHANGERATE}   
                ";
            }
            return result;
        }
        private string GetConditionsPrecedentToDrawdownMarkup()
        {
            var conditions = GetConditionsPrecedentToDrawdown(); // new

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>CONDITIONS PRECEDENT TO DRAWDOWN</b></th>
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

        private string GetTransactionsDynamicsMarkup()
        {
            var transactions = GetTransactionsDynamics().GroupBy(t => t.typeId); // new

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>TRANSACTIONS DYNAMICS</b></th>
                    </tr>
                 ";
            foreach (var group in transactions)
            {
                n++;
                var o = 0;
                foreach (var t in group)
                {
                    o++;
                    result = result + $@"
                        <tr>
                            <td>{o}</td>
                            <td>{t.name}</td>
                        </tr>
                ";
                }
                
            }
            result = result + $"</table>";
            return result;
        }
        
        private string GetBusinessSectorsMarkupLOS()
        {
            var result = String.Empty;
            foreach (var loanDetail in this.loanApplication.TBL_LOAN_APPLICATION_DETAIL)
            {
                result = result + loanDetail.TBL_SUB_SECTOR.TBL_SECTOR.NAME + "\n";
            }
            return result;
        }

        private string GetBusinessSectorsMarkupLMS()
        {
            var result = String.Empty;
            foreach (var loanDetail in this.lmsrApplication.TBL_LMSR_APPLICATION_DETAIL)
            {
                result = result + loanDetail.TBL_PRODUCT.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().TBL_SUB_SECTOR.TBL_SECTOR.NAME + "\n";
            }
            return result;
        }

        private string GetGroupFacilitySummaryMarkupLOS()
        {
            var result = String.Empty;
            result = result + $@"
                <table border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th><b>Facility</b></th>
                        <th><b>LLL Impact</b></th>
                        <th><b>Currency</b></th>
                        <th><b>Current Amount</b></th>
                        <th><b>Proposed Amount</b></th>
                        <th><b>Change</b></th>
                        <th><b>Tenor (Months)</b></th>
                    </tr>
                        {GetDirectFacilitiesMarkupLOS()}
                        {GetTotalDirectFacilitiesMarkupLOS()}
                        {GetForeignDirectFacilitiesMarkupLOS()}
                        {GetTotalForeignDirectFacilitiesMarkupLOS()}
                        {GetContingentFacilitiesMarkupLOS()}
                        {GetTotalContingentFacilitiesMarkupLOS()}
                        {GetForeignContingentFacilitiesMarkupLOS()}
                        {GetTotalForeignContingentFacilitiesMarkupLOS()}
                        {GetIFFMarkupLOS((int)CurrencyEnum.NGN)}
                        {GetIFFMarkupLOS((int)CurrencyEnum.USD)}
                        {GetTotalIFFMarkupLOS()}
                        {GetTotalFacilitiesMarkupLOS()}
                    <tr>
                        <td>Legal Lending Limit:</td>
                        <td>{String.Format("{0:0,0.00}", legalLendingLimit)}</td>
                    </tr>
                    <tr>
                        <td>LLL Impact of Proposed Facilities:</td>
                        <td>{String.Format("{0:0,0.00}", getTotalLLLImpact())}</td>
                    </tr>
                    <tr>
                        <td>Any LLL violation? (Yes / No):</td>
                        <td>{(IsLLLViolated() ? "Yes" : "No")}</td>
                    </tr>
                    <tr>
                        <td>Director-related? (Yes / No):</td>
                        <td>{getIsDirectorRelated()}</td>
                    </tr>
                    <tr>
                        <td>Environmental And Social Risk Summary:</td>
                        <td>{GetEnvironmentalSocialRiskMarkup()}</td>
                    </tr>
                 ";
            result = result + $"</table>";
            return result;
        }

        private string GetGroupFacilitySummaryFCYMarkupLOS()
        {
            var result = String.Empty;
            result = result + $@"
                <table border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th><b>Facility</b></th>
                        <th><b>LLL Impact</b></th>
                        <th><b>Currency</b></th>
                        <th><b>Current Amount</b></th>
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

        private string GetDirectFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            var loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && l.ISDISBURSED == true
                                                && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            var overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                                                                && l.ISDISBURSED == true).ToList();
            var details = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && 
                                                                                 d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability &&
                                                                                 d.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            if (loans.Count() > 0 || overdrafts.Count() > 0 || details.Count() > 0)
            {
                result = result + $@"<tr><td>Direct Facilities (NGN):</td></tr>";
            }
            var loanGroups = loans.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in loanGroups)
            {
                var facility = group.Key;
                var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
                var currentAmount = group.Sum(p => p.OUTSTANDINGPRINCIPAL) + group.Sum(p => p.OUTSTANDINGINTEREST);
                // checks each loan detail.
                var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID == 
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                var tenorTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                var proposedAmount = (proposedAmountTest > 0)? proposedAmountTest + currentAmount : currentAmount;
                var LLLImpact = (100 / 100) * proposedAmount;
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
                        <td>{(tenor/30)}</td>
                    </tr>
                    ";
            }

            var overdraftGroups = overdrafts.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in overdraftGroups)
            {
                var facility = group.Key;
                var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
                var currentAmount = group.Sum(p => p.OVERDRAFTLIMIT);
                var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.TBL_PRODUCT.PRODUCTID ==
                                      group.FirstOrDefault().TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                var tenorTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                var LLLImpact = (100 / 100) * proposedAmount;
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

            foreach (var d in details)
            {
                var loanFacilityExists = loans.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);
                var overdraftFacilityExists = overdrafts.Exists(o => o.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

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
            var totalDirectsSummary = GetTotalDirectFacilitiesSummaryLOS((int)CurrencyEnum.NGN);
            if (totalDirectsSummary.numberOfLoans > 0 || totalDirectsSummary.numberOfOverdrafts > 0 || totalDirectsSummary.numberOfNewFacilities > 0 )
            {
                result = result + $@"
                    <tr>
                        <td><b>Total Direct (NGN)<b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectsSummary.totalLLLImpact)}</b></td>
                        <td><b>{totalDirectsSummary.currency}</td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectsSummary.totalCurrentAmount)}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectsSummary.totalProposedAmount)}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalDirectsSummary.totalChange)}</b></td>
                        <td><b>{(totalDirectsSummary.totalTenors / 30)}</b></td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetContingentFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            var contingents = context.TBL_LOAN_CONTINGENT.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                                                                && l.ISDISBURSED == true).ToList();
            var appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID ==
                                                                                   (int)LoanProductTypeEnum.ContingentLiability);
            if (contingents.Count() > 0 || appDetails.Count() > 0)
            {
                result = result + $@"<tr><td>Contingent Facilities (NGN):</td></tr>";
            }
            var contingentsGroup = contingents.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            if (contingentsGroup.Count() > 0)
            {
                foreach (var group in contingentsGroup)
                {
                    var facility = group.Key;
                    var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = group.Sum(p => p.CONTINGENTAMOUNT);
                    var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.TBL_PRODUCT.PRODUCTID ==
                                             group.FirstOrDefault().TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                    var tenorTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.TBL_PRODUCT.PRODUCTID ==
                                             group.FirstOrDefault().TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
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

            foreach (var d in appDetails)
            {
                var contingentExists = contingents.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

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
            var totalContingentSummary = GetTotalContingentFacilitiesSummaryLOS((int)CurrencyEnum.NGN);
            if (totalContingentSummary.numberOfContingents > 0 || totalContingentSummary.numberOfNewFacilities > 0)
            {
                result = result + $@"
                    <tr>
                        <td><b>Total Contingents (NGN)</b></td>
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
        
        private string GetIFFMarkupLOS(int currencyId)
        {
            var result = String.Empty;
            var IFFs = new List<TBL_LOAN>();
            var appDetails = new List<TBL_LOAN_APPLICATION_DETAIL>();
            if (currencyId == (int)CurrencyEnum.NGN)
            {
                IFFs = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                                                && l.ISDISBURSED == true && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                                                && d.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }
            else
            {
                IFFs = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                && l.ISDISBURSED == true && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                && d.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }

            if (IFFs.Count() > 0 || appDetails.Count() > 0)
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
            var loanGroups = IFFs.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in loanGroups)
            {
                var currFacility = group.GroupBy(f => f.CURRENCYID);
                foreach (var curr in currFacility)
                {
                    var facility = group.Key;
                    var currency = curr.First().TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = curr.Sum(p => p.OUTSTANDINGPRINCIPAL) + curr.Sum(p => p.OUTSTANDINGINTEREST);
                    var currentAmountForLLLImpact = curr.Sum(p => p.OUTSTANDINGPRINCIPAL * (decimal)p.EXCHANGERATE) + curr.Sum(p => p.OUTSTANDINGINTEREST * (decimal)p.EXCHANGERATE);
                    var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
                                             f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                    var proposedAmountTestForLLLImpact = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
                                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var tenorTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
                                        f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                    var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                    var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
                    var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                    var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                    var tenor = curr.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

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

            // checks each loan detail.
            foreach (var d in appDetails)
            {
                var IFFExists = IFFs.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

                if (!IFFExists)
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
            var loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN && l.ISDISBURSED == true
                                                && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            var overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                                && l.ISDISBURSED == true).ToList();
            var appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN &&
                                                                                 d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability &&
                                                                                 d.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            if (loans.Count() > 0 || overdrafts.Count() > 0 || appDetails.Count() > 0)
            {
                result = result + $@"<tr><td>Direct Facilities (FCY):</td></tr>";
            }
            var loanGroups = loans.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            if (loanGroups.Count() > 0)
            {
                foreach (var group in loanGroups)
                {
                    var currFacility = group.GroupBy(f => f.CURRENCYID);
                    foreach (var curr in currFacility)
                    {
                        var facility = group.Key;
                        var currency = curr.First().TBL_CURRENCY.CURRENCYNAME;
                        var currentAmount = curr.Sum(p => p.OUTSTANDINGPRINCIPAL) + curr.Sum(p => p.OUTSTANDINGINTEREST);
                        var currentAmountForLLLImpact = curr.Sum(p => p.OUTSTANDINGPRINCIPAL * (decimal)p.EXCHANGERATE) + curr.Sum(p => p.OUTSTANDINGINTEREST * (decimal)p.EXCHANGERATE);
                        var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
                                                  f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var proposedAmountTestForLLLImpact = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
                                                  f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                        var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                        var tenor = curr.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

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

            var overdraftGroups = overdrafts.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            if (loanGroups.Count() > 0)
            {
                foreach (var group in overdraftGroups)
                {
                    var currFacility = group.GroupBy(f => f.CURRENCYID);
                    foreach (var curr in currFacility)
                    {
                        var facility = group.Key;
                        var currency = curr.First().TBL_CURRENCY.CURRENCYNAME;
                        var currentAmount = curr.Sum(p => p.OVERDRAFTLIMIT);
                        var currentAmountForLLLImpact = curr.Sum(p => p.OVERDRAFTLIMIT * (decimal)p.EXCHANGERATE);
                        var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
                                                  f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                        var proposedAmountTestForLLLImpact = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
                                                  f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                        var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                        var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
                        var LLLImpact = (100 / 100) * proposedAmountForLLLImpact;
                        var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                        var tenor = curr.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

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

            foreach (var d in appDetails)
            {
                var loanFacilityExists = loans.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);
                var overdraftFacilityExists = overdrafts.Exists(o => o.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

                if (!loanFacilityExists || !overdraftFacilityExists)
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
            var contingents = context.TBL_LOAN_CONTINGENT.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                                && l.ISDISBURSED == true).ToList();
            var appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN && d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID ==
                                                                                   (int)LoanProductTypeEnum.ContingentLiability);
            if (contingents.Count() > 0 || appDetails.Count() > 0)
            {
                result = result + $@"<tr><td>Contingent Facilities (FCY):</td></tr>";
            }
            var contingentsGroup = contingents.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in contingentsGroup)
            {
                var currFacility = group.GroupBy(f => f.CURRENCYID);
                foreach (var curr in currFacility)
                {
                    var facility = group.Key;
                    var currency = curr.First().TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = curr.Sum(p => p.CONTINGENTAMOUNT);
                    var currentAmountForLLLImpact = curr.Sum(p => p.CONTINGENTAMOUNT * (decimal)p.EXCHANGERATE);
                    var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
                                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                    var proposedAmountTestForLLLImpact = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => curr.First().TBL_PRODUCT.PRODUCTID ==
                                              f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == curr.First().CURRENCYID)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                    var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : currentAmount;
                    var proposedAmountForLLLImpact = (proposedAmountTestForLLLImpact > 0) ? proposedAmountTestForLLLImpact + currentAmountForLLLImpact : currentAmountForLLLImpact;
                    var LLLImpact = proposedAmountForLLLImpact / 3;
                    var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                    var tenor = curr.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

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

            foreach (var d in appDetails)
            {
                var contingentExists = contingents.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

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
            var loans = new List<TBL_LOAN>();
            var overdrafts = new List<TBL_LOAN_REVOLVING>();
            var appDetails = new List<TBL_LOAN_APPLICATION_DETAIL>();
            if (currencyId == (int)CurrencyEnum.NGN)
            {
                loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && l.ISDISBURSED == true
                                                && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                                                                    && l.ISDISBURSED == true).ToList();
                appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN &&
                                                                                     d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability &&
                                                                                     d.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }
            else
            {
                loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN && l.ISDISBURSED == true
                                                && l.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                                    && l.ISDISBURSED == true).ToList();
                appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN &&
                                                                                     d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability &&
                                                                                     d.TBL_PRODUCT.PRODUCTCLASSID != (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }
            directSummary.numberOfLoans = loans.Count();
            directSummary.numberOfOverdrafts = overdrafts.Count();
            var loanGroups = loans.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in loanGroups)
            {
                var currency = "Naira";
                //var currentAmount = group.Sum(p => p.OUTSTANDINGPRINCIPAL * (decimal)p.EXCHANGERATE) + group.Sum(p => p.OUTSTANDINGINTEREST * (decimal)p.EXCHANGERATE);
                var currentAmount = group.Sum(p => p.OUTSTANDINGPRINCIPAL * (decimal)p.EXCHANGERATE);
                var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
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

            var overdraftGroups = overdrafts.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in overdraftGroups)
            {
                var facility = group.Key;
                var currency = "Naira";
                var currentAmount = group.Sum(p => p.OVERDRAFTLIMIT);
                var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
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

            foreach (var d in appDetails)
            {
                var loanFacilityExists = loans.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);
                var overdraftFacilityExists = overdrafts.Exists(o => o.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

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
            var contingents = new List<TBL_LOAN_CONTINGENT>();
            var appDetails = new List<TBL_LOAN_APPLICATION_DETAIL>();
            if (currencyId == (int)CurrencyEnum.NGN)
            {
                contingents = context.TBL_LOAN_CONTINGENT.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                                                                && l.ISDISBURSED == true).ToList();
                appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN &&
                              d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == (int)LoanProductTypeEnum.ContingentLiability).ToList();
            }
            else
            {
                contingents = context.TBL_LOAN_CONTINGENT.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                                && l.ISDISBURSED == true).ToList();
                appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN &&
                              d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == (int)LoanProductTypeEnum.ContingentLiability).ToList();
            }
            contingentsSummary.numberOfContingents = contingents.Count();
            var loanGroups = contingents.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in loanGroups)
            {
                var currency = "Naira";
                var currentAmount = group.Sum(p => p.CONTINGENTAMOUNT);
                var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
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

            foreach (var d in appDetails)
            {
                var contingentsFacilityExists = contingents.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

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
            var IFFs = new List<TBL_LOAN>();
            var appDetails = new List<TBL_LOAN_APPLICATION_DETAIL>();
            if (currencyId == (int)CurrencyEnum.NGN)
            {
                IFFs = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                                                && l.ISDISBURSED == true && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN 
                                                && d.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }
            else
            {
                IFFs = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                && l.ISDISBURSED == true && l.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
                appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                && d.TBL_PRODUCT.PRODUCTCLASSID == (int)ProductClassEnum.ImportFinanceFacilities).ToList();
            }
            IFFSummary.numberOfImportFinanceFacilities = IFFs.Count();
            var loanGroups = IFFs.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in loanGroups)
            {
                var currency = "Naira";
                //var currentAmount = group.Sum(p => p.OUTSTANDINGPRINCIPAL * (decimal)p.EXCHANGERATE) + group.Sum(p => p.OUTSTANDINGINTEREST * (decimal)p.EXCHANGERATE);
                var currentAmount = group.Sum(p => p.OUTSTANDINGPRINCIPAL * (decimal)p.EXCHANGERATE);
                var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT * (decimal)p.EXCHANGERATE)) ?? 0;
                var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
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

            foreach (var d in appDetails)
            {
                var IFFExists = IFFs.Exists(l => l.TBL_PRODUCT.PRODUCTID == d.TBL_PRODUCT.PRODUCTID);

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
            var exposures = GetGroupExposurebyCustomerId(this.customerId, this.loanApplication.COMPANYID);
            var exposureGroupsByCustomer = exposures.GroupBy(e => e.customerName);
            var n = 0;
            result = result + $@"
                <table border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th><b>Customer Name</b></th>
                        <th><b>Facility Type</b></th>
                        <th><b>Currency</b></th>
                        <th><b>Approved Amount</b></th>
                        <th><b>Current Amount</b></th>
                        <th><b>Maturity</b></th>
                    </tr>
                ";
            foreach (var customerGroups in exposureGroupsByCustomer)
            {
                var customerName = customerGroups.Key;
                var facilities = customerGroups.GroupBy(c => c.facilityType);
                foreach (var facility in facilities)
                {
                    ++n;
                    result = result + $@"
                     <tr>
                        <td>{customerName}</td>
                        <td>{facility.Key}</td>
                        <td>{facility.FirstOrDefault()?.currency}</td>
                        <td>{String.Format("{0:0,0.00}", facility.Sum(f => f.approvedAmount))}</td>
                        <td>{String.Format("{0:0,0.00}", facility.Sum(f => f.outstandings))}</td>
                        <td>{facility.Max(f => f.maturityDate).ToShortDateString()}</td>
                    </tr>
                ";
                }
            }
            result = result + $@"
                    {GetTotalGroupExposureMarkup()}
                ";
            result = result + $"</table>";
            return result;
        }

        private string GetTotalGroupExposureMarkup()
        {
            var result = String.Empty;
            var exposures = GetGroupExposurebyCustomerId(this.customerId, this.loanApplication.COMPANYID);
            CurrentCustomerExposure totalExposure;
            
            totalExposure = new CurrentCustomerExposure()
            {
                facilityType = "TOTAL",
                outstandings = exposures.Sum(t => t.outstandings),
                approvedAmount = exposures.Sum(t => t.approvedAmount),
            };
            
            result = result + $@"
                     <tr>
                        <td><b>{totalExposure.facilityType}</b></td>
                        <td>&nbsp;</td>
                        <td><b>{exposures.FirstOrDefault()?.currency}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalExposure.approvedAmount)}</b></td>
                        <td><b>{String.Format("{0:0,0.00}", totalExposure.outstandings)}</b></td>
                        <td>&nbsp;</td>
                    </tr>
                ";

            return result;
        }

        private string GetApprovalsMarkupLOS()
        {
            var appraisals = GetAppraisalMemorandumTrail(this.targetId).OrderBy(a => a.approvalTrailId);
            var result = String.Empty;
            result = result + $@"
                <table border=1 width=1200 cellpadding=15 cellspacing=0>
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
                        <td>{GetDecision(trail.vote)}</td>
                        <td>{trail.comment}</td>
                        <td>{trail.systemArrivalDateTime}</td>
                    </tr>
                ";
            }

            result = result + $"</table>";
            return result;

        }

        private string GetDecision(short? vote)
        {
            if (vote == 1) return "Decline";
            if (vote == 2) return "Accept";
            if (vote == 3) return "Decline";
            if (vote == 4) return "Accept";
            return String.Empty;
        }

        private string GetAllCustomerFacilitiesMarkup()
        {
            var result = String.Empty;
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
            return result;
        }

        private string GetAllCustomerCollateralsMarkup()
        {
            var result = String.Empty;
            var remark = string.Empty;
            var customerCollaterals = collateralRepo.GetCustomerCollateral(this.customerId, this.loanApplication.LOANAPPLICATIONID, this.loanApplication.COMPANYID);
            
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

        private string GetSecurityAnalysisMarkUP()
        {
            var result = String.Empty;
            result += $@"
                <table border=1 width=600 cellpadding=15 cellspacing=0>
                    <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Security / Support</b></th>
                    </tr>
                    <tr>
                    <td>{GetAllCustomerFacilitiesMarkup()}</td>
                    <td>{GetAllCustomerCollateralsMarkup()}</td>
                    </tr>
                </table>
            ";
            return result;
        }

        private string GetCollateralCoverageMarkupLOS()
        {
            var custFacilitiesAmount = new decimal();
            var collaterals = collateralRepo.GetProposedCustomerCollateralByCustomerId(customerId, false);
            var result = String.Empty;
            if (collaterals.Count() < 1) return result;
            decimal totalCollateralValue = 0;
            var currencies = context.TBL_CURRENCY.ToList();
            var baseCurrency = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == loanApplication.COMPANYID).CURRENCYID;
            var baseCurrencyCode = currencies.FirstOrDefault(cu => cu.CURRENCYID == baseCurrency).CURRENCYCODE;
            var collateralGroup = collaterals.GroupBy(c => c.loanApplicationDetailId);
            var collateralGroup2 = collaterals.GroupBy(c => c.collateralId);
            foreach (var g in collateralGroup)
            {
                var facility = context.TBL_LOAN_APPLICATION_DETAIL.Find(g.Key);
                custFacilitiesAmount += facility.APPROVEDAMOUNT * (decimal)facility.EXCHANGERATE;
            }
            int n = 0;
            result = result + $@"
                <table border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>DESCRIPTION/SUMMARY</b></th>
                        <th><b>COLLATERAL VALUE</b></th>
                        <th><b>COLLATERAL VALUE(LCY)</b></th>
                    </tr>
                    ";

            foreach (var d in collateralGroup2)
            { ++n;
                var c = d.FirstOrDefault();
                var currCode = currencies.FirstOrDefault(cu => cu.CURRENCYID == c.currencyId).CURRENCYCODE;
                var rate = financeTransaction.GetExchangeRate(DateTime.Now, (short)c.currencyId, loanApplication.COMPANYID);
                var baseCollateralValue = c.collateralValue * (decimal)rate.sellingRate;
                totalCollateralValue += baseCollateralValue;
                result = result + $@"
                    <tr>
                        <td>{n}</td>
                        <td>{c.collateralSummary}</td>
                        <td>{currCode + " " + String.Format("{0:0,0.00}", c.collateralValue)}</td>
                        <td>{baseCurrencyCode + " " + String.Format("{0:0,0.00}", baseCollateralValue)}</td>
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
            var exposures = GetGroupExposurebyCustomerId((int)lmsrApplication.CUSTOMERID, this.lmsrApplication.COMPANYID);
            var n = 0;
            result = result + $@"
                <table border=1>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>Customer Name</b></th>
                        <th><b>Facility Type</b></th>
                        <th><b>Application Reference Number</b></th>
                        <th><b>Outstanding</b></th>
                        <th><b>Existing Limit</b></th>
                        <th><b>Loan Status</b></th>
                    </tr>
                 ";
            foreach (var exposure in exposures)
            {
                ++n;
                result = result + $@"
                    <tr>
                        <td>{n}</td>
                        <td>{exposure.customerName}</td>
                        <td>{exposure.facilityType}</td>
                        <td>{exposure.referenceNumber}</td>
                        <td>{exposure.outstandings}</td>
                        <td>{exposure.existingLimit}</td
                        <td>{exposure.loanStatus}</td>
                    </tr>
                ";
            }
            result = result + $"</table>";
            return result;
        }

        // execute 
        public string Replace(string content) // placeholders replace
        {
            content = content.Replace(customerNameHolder, customerName);
            content = content.Replace(branchNameHolder, branchName);
            content = content.Replace(customerExposureHolder, customerExposure);
            content = content.Replace(recommendedInterestRateHolder, recommendedInterestRate);
            content = content.Replace(isRelatedPartyHolder, isRelatedParty);
            content = content.Replace(dateCreatedHolder, dateCreated);
            content = content.Replace(locationNameHolder, locationName);
            content = content.Replace(approvalLevelHolder, approvalLevel);
            content = content.Replace(accountNumbersHolder, accountNumbers);
            content = content.Replace(proposedConditionsHolder, proposedConditions);
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
            content = content.Replace(facilityUpgradeSupportSchemeHolder, facilityUpgradeSupportSchemeData);
            content = content.Replace(invoiceDiscountingDataHolder, invoiceDiscountingData);
            content = content.Replace(cashCollaterizedDataHolder, cashCollaterizedData);
            content = content.Replace(staffCarLoansDataHolder, staffcarLoansData);
            content = content.Replace(staffMortgageLoansDataHolder, staffMortgageLoansData);
            content = content.Replace(staffPersonalLoansAGMDataHolder, staffPersonalLoansAGMData);
            content = content.Replace(staffPersonalLoanDataHolder, staffPersonalLoanData);
            content = content.Replace(temporaryOverdraftHolder, temporaryOverdraftData);


            return content;
        }

        // support methods // interface getter

        public List<CurrentCustomerExposure> GetCustomerExposure(List<CustomerExposure> customerIds, int companyId) // not used!
        {
            return loanRepo.GetCurrentCustomerExposure(customerIds, companyId); // old maurer impl
        }

        // html markup

        private string CustomerExposureMarkup()
        {
            // var exposures = GetCustomerExposure(customerIds, companyId); // old maurer impl
            var exposures = GetCurrentCustomerExposure(); // new

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table border=1>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>Facility Type</b></th>
                        <th><b>Existing Limit</b></th>
                        <th><b>Proposed Limit</b></th>
                        <th><b>Change</b></th>
                        <th><b>Outstandings</b></th>
                        <th><b>Past Due Obligations Principal</b></th>
                        <th><b>Past Due Obligations Interest</b></th>
                        <th><b>Review Date</b></th>
                    </tr>
                 ";
            foreach (var e in exposures)
            {
                n++;
                result = result + $@"
                    <tr>
                        <td>{n}</td>
                        <td>{e.facilityType}</td>
                        <td>{String.Format("{0:n}", e.existingLimit)}</td>
                        <td>{String.Format("{0:n}", e.proposedLimit)}</td>
                        <td>{String.Format("{0:n}", e.change)}</td>
                        <td>{String.Format("{0:n}", e.outstandings)}</td>
                        <td>{String.Format("{0:n}", e.PastDueObligationsPrincipal)}</td>
                        <td>{String.Format("{0:n}", e.PastDueObligationsInterest)}</td>
                        <td>{e.reviewDate.ToShortDateString()}</td>
                    </tr>
                ";
            }
            result = result + $"</table>";
            return result;

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
            if (operationId == (int)OperationsEnum.CreditAppraisal) return memo.GetApplicationMonitoringTriggers(targetId);
            return memo.GetApplicationMonitoringTriggersLms(targetId);
        }

        private string MonitoringTriggersMarkup()
        {
            var result = String.Empty;
            var triggers = GetMonitoringTriggers();

            var n = 0;
            result = result + $@"
                <table border=1>
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

        public IEnumerable<ESGChecklistSummaryViewModel> GetEnvironmentalSocialRisk()
        {
            return context.TBL_ESG_CHECKLIST_SUMMARY
                .Join(context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == this.targetId) 
                , s => s.LOANAPPLICATIONDETAILID, d => d.LOANAPPLICATIONDETAILID, (s, d) => new { s, d })
                .Select(x => new ESGChecklistSummaryViewModel
                {
                    loanApplicationDetailId = x.s.LOANAPPLICATIONDETAILID,
                    comment = x.s.COMMENT_,
                    ratingId = x.s.RATINGID,
                    productCustomerName = x.d.TBL_PRODUCT.PRODUCTNAME + " -- " + x.d.TBL_CUSTOMER.FIRSTNAME + " " + x.d.TBL_CUSTOMER.MIDDLENAME + " " + x.d.TBL_CUSTOMER.LASTNAME
                }).ToList();
        }

        private string GetEnvironmentalSocialRiskMarkup() // TODO RATINGIS
        {
            var result = String.Empty;
            var summary = GetEnvironmentalSocialRisk();

            var n = 0;
            result = result + $@"
                <table border=1>
                    <tr>
                        <th><b>S/N</b></th>
                        <th><b>Facility</b></th>
                        <th><b>Summary</b></th>
                        <th><b>Rating</b></th>
                    </tr>
                 ";
            foreach (var s in summary)
            {
                n++;
                result = result + $@"
                    <tr>
                        <td>{n}</td>
                        <td>{s.productCustomerName}</td>
                        <td>{s.comment}</td>
                        <td>{GetESGRating(s.ratingId)}</td>
                    </tr>
                ";
            }
            result = result + $"</table>";
            return result;
        }
       
        private string GetESGRating(int ratingId)
        {
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

        public List<CurrentCustomerExposure> GetCurrentCustomerExposure()
        {
            List<CustomerProduct> details = new List<CustomerProduct>();
            IQueryable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();

            if (operationId == (int)OperationsEnum.CreditAppraisal)
                details = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerProduct { CUSTOMERID = x.CUSTOMERID, PRODUCTID = x.APPROVEDPRODUCTID }).ToList();
            else
                details = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerProduct { CUSTOMERID = x.CUSTOMERID, PRODUCTID = x.PRODUCTID }).ToList();

            foreach (var detail in details)
            {
                exposure = context.TBL_LOAN
                    .Where(x => x.CUSTOMERID == detail.CUSTOMERID && x.PRODUCTID == detail.PRODUCTID && x.LOANSTATUSID == (int)LoanStatusEnum.Active)
                    .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
                    .Select(g => new CurrentCustomerExposure
                    {
                        facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
                        existingLimit = g.Sum(x => x.PRINCIPALAMOUNT),
                        proposedLimit = g.Sum(x => x.OUTSTANDINGPRINCIPAL),
                        recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                        PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
                        PastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
                        reviewDate = DateTime.Now,
                        prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
                        loanStatus = "Running"
                    });

                if (exposure.Count() > 0) exposures.AddRange(exposure);

                // Same for revolving and contegent facility ...

                exposure = context.TBL_LOAN_REVOLVING
                    .Where(x => x.CUSTOMERID == detail.CUSTOMERID && x.PRODUCTID == detail.PRODUCTID && x.LOANSTATUSID == (int)LoanStatusEnum.Active)
                    .GroupBy(x => new { x.CUSTOMERID, x.PRODUCTID })
                    .Select(g => new CurrentCustomerExposure
                    {
                        facilityType = g.FirstOrDefault().TBL_PRODUCT.PRODUCTNAME,
                        existingLimit = g.Sum(x => x.OVERDRAFTLIMIT),
                        proposedLimit = g.Sum(x => x.OVERDRAFTLIMIT),
                        recommendedLimit = g.FirstOrDefault().TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                        PastDueObligationsInterest = g.Sum(x => x.PASTDUEINTEREST),
                        PastDueObligationsPrincipal = g.Sum(x => x.PASTDUEPRINCIPAL),
                        reviewDate = DateTime.Now,
                        prudentialGuideline = g.FirstOrDefault().TBL_LOAN_PRUDENTIALGUIDELINE2.STATUSNAME, // ?
                        loanStatus = "Running"
                    });

                if (exposure.Count() > 0) exposures.AddRange(exposure);

                //exposure = from a in context.TBL_LOAN_APPLICATION_DETAIL
                //           where a.CUSTOMERID == detail.CUSTOMERID && a.APPROVEDPRODUCTID == detail.PRODUCTID && (a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved || a.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved)
                //           select new CurrentCustomerExposure
                //           {
                //               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
                //               existingLimit = 0,
                //               proposedLimit = a.PROPOSEDAMOUNT,
                //               recommendedLimit = a.APPROVEDAMOUNT,
                //               PastDueObligationsInterest = 0,
                //               PastDueObligationsPrincipal = 0,
                //               reviewDate = DateTime.Now,
                //               prudentialGuideline = "Processing",
                //               loanStatus = "Processing"
                //           };

                //if (exposure.Count() > 0) exposures.AddRange(exposure);

            }

            exposures.Add(new CurrentCustomerExposure
            {
                facilityType = "TOTAL",
                existingLimit = exposures.Sum(t => t.existingLimit),
                proposedLimit = exposures.Sum(t => t.proposedLimit),
                recommendedLimit = exposures.Sum(t => t.recommendedLimit),
                PastDueObligationsInterest = exposures.Sum(t => t.PastDueObligationsInterest),
                PastDueObligationsPrincipal = exposures.Sum(t => t.PastDueObligationsPrincipal),
                reviewDate = DateTime.Now,
                prudentialGuideline = String.Empty,
                loanStatus = String.Empty,
            });

            return exposures;
        }

        public List<CurrentCustomerExposure> GetGroupExposurebyCustomerId(int customerId, int companyId)
        {
            var exposures = groupRepo.GetGroupExposureByCustomerId(customerId, companyId);
            return exposures;
        }

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
            //    <table border=1>
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

        public string MemoMarkupHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br />
                <h3><b>MEMO</b></h3>
                <table border=1 width=900 cellpadding=15 cellspacing=0>
                    <tr>
                        <td><b>Date</b></td>
                        <td></td>
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

        public string FacilityUpgradeSupportSchemeHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br />
                <h3><b>CREDIT PROGRAM SHEET (FACILITY UPGRADE SUPPORT SCHEME)</b></h3>
                <br />
                <h4><b>Customer Information</b></h4>
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     
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
                <table border=1 width=700 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                        <td><table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <h4><b>CONCURRENCES:</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th></th>
                        <th><b>NAME</b></th>
                        <th><b>SIGNATURE & DATE</b></th>
                    </tr> 
                   <tr>
                        <td>ACCOUNT OFFICER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>TEAM LEAD /REL. MANAGER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>RETAIL SALES MANAGER</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>ZONAL HEAD</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>PRODUCT MGT (HEAD)</td>
                        <td></td>
                        <td></td>
                    </tr> 
                  <tr>
                        <td>GH, PRODUCT & CHANNELS MGT</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>CREDIT RISK MANAGEMENT </td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td><strong>APPROVAL:</strong></td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>CHECKLIST / ELIGIBILITY</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     
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
                <br />
                <h4><b>CONCURRENCES:</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th></th>
                        <th><b>NAME</b></th>
                        <th><b>SIGNATURE & DATE</b></th>
                    </tr> 
                   <tr>
                        <td>ACCOUNT OFFICER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>TEAM LEAD /REL. MANAGER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>PRODUCT MGT GROUP</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>CORPORATE COUNSEL</td>
                        <td></td>
                        <td></td>
                    </tr>  
                   
                    <tr>
                        <td>CREDIT RISK MANAGEMENT </td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            return result;
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                        <td><table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <br />
                <h4><b>CONCURRENCES:</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th></th>
                        <th><b>NAME</b></th>
                        <th><b>SIGNATURE & DATE</b></th>
                    </tr> 
                   <tr>
                        <td>ACCOUNT OFFICER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>TEAM LEAD /REL. MANAGER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>RETAIL SALES MANAGER</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>ZONAL HEAD</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>PRODUCT MGT (HEAD)</td>
                        <td></td>
                        <td></td>
                    </tr> 
                  <tr>
                        <td>GH, PRODUCT & CHANNELS MGT</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>CREDIT RISK MANAGEMENT </td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td><strong>APPROVAL:</strong></td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>CHECKLIST / ELIGIBILITY</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <br />
                <h4><b>Concurrences:</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th></th>
                        <th><b>NAME</b></th>
                        <th><b>SIGNATURE & DATE</b></th>
                    </tr> 
                   <tr>
                        <td>ACCOUNT OFFICER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>TEAM LEAD /REL. MANAGER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>PRODUCT MGT GROUP</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>CORPORATE COUNSEL</td>
                        <td></td>
                        <td></td>
                    </tr>  
                   
                    <tr>
                        <td>CREDIT RISK MANAGEMENT </td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                        <td><table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <br />
                <h4><b>CONCURRENCES:</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th></th>
                        <th><b>NAME</b></th>
                        <th><b>SIGNATURE & DATE</b></th>
                    </tr> 
                   <tr>
                        <td>ACCOUNT OFFICER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>TEAM LEAD /REL. MANAGER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>RETAIL SALES MANAGER</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>ZONAL HEAD</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>PRODUCT MGT (HEAD)</td>
                        <td></td>
                        <td></td>
                    </tr> 
                  <tr>
                        <td>GH, PRODUCT & CHANNELS MGT</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>CREDIT RISK MANAGEMENT </td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td><strong>APPROVAL:</strong></td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h4><b>CHECKLIST / ELIGIBILITY</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <br />
                <h4><b>APPROVALS:</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th></th>
                        <th><b>NAME</b></th>
                        <th><b>SIGNATURE & DATE</b></th>
                    </tr> 
                   <tr>
                        <td>RELATIONSHIP OFFICER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>RELATIONSHIP MANAGER </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>GROUP HEAD</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>CREDIT RISK MANAGEMENT </td>
                        <td></td>
                        <td></td>
                    </tr>  
                   
                    <tr>
                        <td>APPROVAL </td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            return result;
        }

        public string TemporaryOverdraftHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <h3><b>MEMO</b></h3>
                <br />
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                    <tr>
                        <td colspan=2><strong>TEMPORARY OVERDRAFT (TOD)</strong></td>
                        <td></td>
                        <td><strong>Date:</strong></td>
                        <td>Request Date</td>                
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
               
                <table border=0 width=900 cellpadding=10 cellspacing=0>        
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
                     
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <br />
                <table border=0 width=900 cellpadding=10 cellspacing=0>
                     
                   <tr>
                        <td colspan='2'><strong>APPROVAL INFORMATION</strong></td>                     
                    </tr> 
                    <tr>
                        <td><strong>Name</strong></td>
                        <td><strong>SIGNATURE</strong></td>
                    </tr> 
                   <tr>
                        <td><strong>Relationship Officer.</strong></td>
                        <td>_______________</td>
                    </tr> 
                    <tr>
                        <td><strong>Relationship Manager.</strong></td>
                        <td>_______________</td>
                    </tr> 
                     <tr>
                        <td><strong>Zonal/Group Head</strong></td>
                        <td>_______________</td>
                    </tr> 
                    <tr>
                        <td><strong>Zonal/Group Head</strong></td>
                        <td>_______________</td>
                    </tr> 
                     <tr>
                        <td><strong>Credit Risk Mgt.</strong></td>
                        <td>_______________</td>
                    </tr> 
                    <tr>
                        <td><strong>APPROVAL	Executive Director</strong></td>
                        <td>_______________</td>
                    </tr>  
                    <tr>
                        <td>GDMD</td>
                        <td>_______________</td>
                    </tr>  
                    <tr>
                        <td>GMD</td>
                        <td>_______________</td>
                    </tr>  
                   ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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

        public string StaffCarLoansHtml()
        {
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <br /><h4><b>Access Bank Plc RC 125384</b></h4>
                <h3><b>Staff Car Loan Scheme – Facility Approval Memo</b></h3>
                <br />
                <h3><b>Staff Information</b></h3>
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
                    
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <h4><b>Approval Information:</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th></th>
                        <th><b>NAME</b></th>
                        <th><b>SIGNATURE & DATE</b></th>
                    </tr> 
                   <tr>
                        <td>Staff / Applicant </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Staff’s Group Head </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>HR Officer</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Group Head, CRM – PBD </td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Group Head, Credit Admin & Portfolio Management</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Approval: Group Head, Human Resources </td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
                    
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <h4><b>Approval Information:</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th></th>
                        <th><b>NAME</b></th>
                        <th><b>SIGNATURE & DATE</b></th>
                    </tr> 
                   <tr>
                        <td>Staff / Applicant </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Line Supervisor/ Group Head</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Group Head, Human Resources</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Group Head, CRM – PBD</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Group Head, Credit Admin & Portfolio Management</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td><strong>Approval:</strong> Line ED </td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td><strong>Approval:</strong> GMD </td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
                    
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <h4><b>Approval Information:</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th></th>
                        <th><b>NAME</b></th>
                        <th><b>SIGNATURE & DATE</b></th>
                    </tr> 
                   <tr>
                        <td>Staff / Applicant </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Line Supervisor/ Group Head</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Group Head, Human Resources</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Group Head, CRM – PBD</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Group Head, Credit Admin & Portfolio Management</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td><strong>Approval:</strong> GMD </td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h3><b>BOARD / BCC (for AGM & above):</b></h3>
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
                    
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <table border=0 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                    
                   <tr>
                        <td><b>PRINCIPAL TERMS & CONDITIONS INCLUDING SECURITY/SUPPORT:</b></td>
                    </tr> 
                     <tr>
                        <td>
                        <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <table border=1 width=900 cellpadding=10 cellspacing=0>
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
                <h4><b>Approval Information:</b></h4>
                <table border=1 width=900 cellpadding=10 cellspacing=0>
                     <tr>
                        <th></th>
                        <th><b>NAME</b></th>
                        <th><b>SIGNATURE & DATE</b></th>
                    </tr> 
                   <tr>
                        <td>Staff / Applicant </td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>Staff’s Group Head</td>
                        <td></td>
                        <td></td>
                    </tr> 
                   <tr>
                        <td>HR Officer</td>
                        <td></td>
                        <td></td>
                    </tr> 
                    <tr>
                        <td>Group Head, CRM – PBD</td>
                        <td></td>
                        <td></td>
                    </tr>  
                    <tr>
                        <td>Group Head, Credit Admin & Portfolio Management</td>
                        <td></td>
                        <td></td>
                    </tr> 
                     <tr>
                        <td><strong>Approval:</strong> Group Head, Human Resources </td>
                        <td></td>
                        <td></td>
                    </tr> 
                 ";
            result = result + $"</table>";
            result = result + $@"
                <br />
                <h3><b>BOARD / BCC (for AGM & above):</b></h3>
                
                 ";   
            return result;
        }
    }
}




/*
    Obligor Risk Rating:
    Industry Risk Rating:
    Review Type – Annual/Interim/Initial

*/
