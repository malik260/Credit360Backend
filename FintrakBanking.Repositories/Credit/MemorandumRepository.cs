using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
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
        private ILoanRepository loan;
        private ICustomerGroupRepository groupRepo;
        private ITransactionDynamicsRepository transactionsRepo;
        private IConditionPrecedentRepository conditionsRepo;

        public MemorandumRepository(
            FinTrakBankingContext context, 
            IAppraisalMemorandumRepository memo, 
            ILoanRepository loan,     
            ICustomerGroupRepository groupRepo, 
            ITransactionDynamicsRepository transactionsRepo,
            IConditionPrecedentRepository conditionsRepo
            )
        {
            this.context = context;
            this.memo = memo;
            this.loan = loan;
            this.groupRepo = groupRepo;
            this.transactionsRepo = transactionsRepo;
            this.conditionsRepo = conditionsRepo;
        }

        // init
        private int targetId;
        private int operationId;
        List<CustomerExposure> customerIds; // init

        // field variables
        TBL_LOAN_APPLICATION loanApplication = null; 
        TBL_LMSR_APPLICATION lmsrApplication = null;
        private List<int> lmsCamOperationIds = new List<int> { 46, 71, 79 };
        private long legalLendingLimit = 10000000000;

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


        // init
        public bool Init(int operationId, int targetId) // feeder
        {
            this.targetId = targetId;
            this.operationId = operationId;

            if (operationId == (int)OperationsEnum.CAM) // LOS 
            {
                if (loanApplication == null)
                {
                    this.loanApplication = context.TBL_LOAN_APPLICATION.Find(targetId);
                    this.customerIds = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerExposure { customerId = x.CUSTOMERID }).Distinct().ToList();
                    this.customerExposure = CustomerExposureMarkup();
                }

                //string customerName = String.Empty;
                if (loanApplication.CUSTOMERGROUPID != null) this.customerName = loanApplication.TBL_CUSTOMER_GROUP.GROUPNAME;
                if (loanApplication.CUSTOMERID != null) this.customerName = loanApplication.TBL_CUSTOMER.FIRSTNAME + " " + loanApplication.TBL_CUSTOMER.MIDDLENAME + " " + loanApplication.TBL_CUSTOMER.LASTNAME;

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
                this.exchangeRate = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault().EXCHANGERATE.ToString();
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
                //this.totalGroupExposure = GetTotalGroupExposureMarkupLOS();





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

        public List<DropDownSelect> GetProposedConditions()
        {
            var result = new List<DropDownSelect>();
            if (operationId == (int)OperationsEnum.CAM)
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
            if (operationId == (int)OperationsEnum.CAM)
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
            if (operationId == (int)OperationsEnum.CAM)
            {

                var detail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).ToList();
                var details = this.transactionsRepo.GetAllTransactionDynamics().Where(x => x.loanApplicationDetailId == (int)detail.FirstOrDefault()?.LOANAPPLICATIONDETAILID)
                                .OrderBy(a => a.position);
                foreach (var d in details)
                {
                    if (d.dynamics != null)
                    {
                        result.Add(new DropDownSelect { id = d.productId, name = d.dynamics });
                    }
                }
            }
            return result;
        }

        private List<TotalFacilitiesSummaryViewModel> GetTotalFacilitiesLOS()
        {
            var totalSummary = new List<TotalFacilitiesSummaryViewModel>();
            var totalDirectFacilities = GetTotalDirectFacilitiesSummaryLOS((int)CurrencyEnum.NGN);
            if (totalDirectFacilities != null) totalSummary.Add(totalDirectFacilities);
            var totalContingentFacilities = GetTotalContingentFacilitiesSummaryLOS((int)CurrencyEnum.NGN);
            if (totalContingentFacilities != null) totalSummary.Add(totalContingentFacilities);
            //var totalImportFinanceFacilities
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
            var totalSummary = GetTotalFacilitiesLOS();
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



        //markups
        private string GetProposedConditionsMarkup()
        {
            var conditions = GetProposedConditions(); // new

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table align=center border=1 width=1000px cellpadding=15 cellspacing=0>
                    < tr>
                        <th>S/N</th>
                        <th>Facility Type</th>
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
            result = result + $"</table align=center>";
            return result;

        }

        private string GetConditionsPrecedentToDrawdownMarkup()
        {
            var conditions = GetConditionsPrecedentToDrawdown(); // new

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table align=center border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th>S/N</th>
                        <th>CONDITIONS PRECEDENT TO DRAWDOWN</th>
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
            result = result + $"</table align=center>";
            return result;

        }

        private string GetTransactionsDynamicsMarkup()
        {
            var transactions = GetTransactionsDynamics(); // new

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table align=center border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th>S/N</th>
                        <th>TRANSACTIONS DYNAMICS</th>
                    </tr>
                 ";
            foreach (var e in transactions)
            {
                n++;
                result = result + $@"
                        <tr>
                            <td>{n}</td>
                            <td>{e.name}</td>
                        </tr>
                ";
            }
            result = result + $"</table align=center>";
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
                <table align=center border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th>Facility</th>
                        <th>ILLImpact</th>
                        <th>Currency</th>
                        <th>Current Amount</th>
                        <th>Proposed Amount</th>
                        <th>Change</th>
                        <th>Tenor (Months)</th>
                    </tr>
                    <tr><td colspan=7 >Direct Facilities:</td></tr>
                        {GetDirectFacilitiesMarkupLOS()}
                    <tr><td colspan=7>Total Direct</td></tr>
                        {GetTotalDirectFacilitiesMarkupLOS()}
                    <tr><td colspan=7>Contingent Facilities:</td></tr>
                        {GetContingentFacilitiesMarkupLOS()}
                    <tr><td colspan=7>Total Contingents</td></tr>
                        {GetTotalContingentFacilitiesMarkupLOS()}
                    <tr><td colspan=7>Total Facilities:</td></tr>
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
                        <td>{((getTotalLLLImpact() > legalLendingLimit) ? "Yes" : "No")}</td>
                    </tr>
                    <tr>
                        <td>Director-related? (Yes / No):</td>
                        <td>{getIsDirectorRelated()}</td>
                    </tr>
                 ";
            result = result + $"</table align=center>";
            return result;
        }

        private string GetGroupFacilitySummaryFCYMarkupLOS()
        {
            var result = String.Empty;
            result = result + $@"
                <table align=center border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th>Facility</th>
                        <th>ILLImpact</th>
                        <th>Currency</th>
                        <th>Current Amount</th>
                        <th>Proposed Amount</th>
                        <th>Change</th>
                        <th>Tenor</th>
                    </tr>
                    <tr><td colspan=7>Direct Facilities:</td></tr>
                        {GetForeignDirectFacilitiesMarkupLOS()}
                    <tr><td colspan=7>Total Direct</td></tr>
                        {GetTotalForeignDirectFacilitiesMarkupLOS()}
                    <tr><td colspan=7>Contingent Facilities:</td></tr>
                        {GetForeignContingentFacilitiesMarkupLOS()}
                    <tr><td colspan=7>Total Contingents</td></tr>
                        {GetTotalForeignContingentFacilitiesMarkupLOS()}
                    <tr><td colspan=7>Total Facilities:</td></tr>
                        {GetTotalForeignFacilitiesMarkupLOS()}
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
                        <td>{((getTotalLLLImpactFCY() > legalLendingLimit) ? "Yes" : "No")}</td>
                    </tr>
                    <tr>
                        <td>Director-related? (Yes / No):</td>
                        <td>{getIsDirectorRelated()}</td>
                    </tr>
                 ";
            result = result + $"</table align=center>";
            return result;
        }

        private string GetDirectFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            var loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && l.ISDISBURSED == true).ToList();
            var overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                                                                && l.ISDISBURSED == true).ToList();
            var details = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && 
                                                                                 d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability).ToList();

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
                var proposedAmount = (proposedAmountTest > 0)? proposedAmountTest + currentAmount : 0;
                var lLLImpact = (100 / 100) * proposedAmount;
                var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                var tenor = tenorTest;

                result = result + $@"
                    <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", lLLImpact)}</td>
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
                var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : 0;
                var lLLImpact = (100 / 100) * proposedAmount;
                var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                var tenor = tenorTest;

                result = result + $@"
                    <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", lLLImpact)}</td>
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
                var loanFacilityExists = loans.Exists(l => l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);
                var overdraftFacilityExists = overdrafts.Exists(o => o.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);

                if (!loanFacilityExists && !overdraftFacilityExists)
                {
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
                    var lLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;

                    result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", lLLImpact)}</td>
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
                        <td>&nbsp;</td>
                        <td>{String.Format("{0:0,0.00}", totalDirectsSummary.totalLLLImpact)}</td>
                        <td>{totalDirectsSummary.currency}</td>
                        <td>{String.Format("{0:0,0.00}", totalDirectsSummary.totalCurrentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", totalDirectsSummary.totalProposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", totalDirectsSummary.totalChange)}</td>
                        <td>{(totalDirectsSummary.totalTenors / 30)}</td>
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
            var contingentsGroup = contingents.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in contingentsGroup)
            {
                var facility = group.Key;
                var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
                var currentAmount = group.Sum(p => p.CONTINGENTAMOUNT);
                var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.TBL_PRODUCT.PRODUCTID ==
                                         group.FirstOrDefault().TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                var tenorTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.TBL_PRODUCT.PRODUCTID ==
                                         group.FirstOrDefault().TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : 0;
                var lLLImpact = (1 / 3) * proposedAmount;
                var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                var tenor = tenorTest;

                result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", lLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
            }

            foreach (var d in this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID ==
                                                                                    (int)LoanProductTypeEnum.ContingentLiability))
            {
                var contingentExists = contingents.Exists(l => l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);

                if (!contingentExists)
                {
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
                    var lLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;

                    result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", lLLImpact)}</td>
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
                        <td>&nbsp;</td>
                        <td>{String.Format("{0:0,0.00}", totalContingentSummary.totalLLLImpact)}</td>
                        <td>{totalContingentSummary.currency}</td>
                        <td>{String.Format("{0:0,0.00}", totalContingentSummary.totalCurrentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", totalContingentSummary.totalProposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", totalContingentSummary.totalChange)}</td>
                        <td>{(totalContingentSummary.totalTenors / 30)}</td>
                    </tr>
                ";
            }
            return result;
        }
        
        private string GetTotalFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            var totalSummary = GetTotalFacilitiesLOS();
            if (totalSummary.Count() > 0)
            {
                result = result + $@"
                     <tr>
                        <td>&nbsp;</td>
                        <td>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalLLLImpact))}</td>
                        <td>{totalSummary.FirstOrDefault()?.currency}</td>
                        <td>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalCurrentAmount))}</td>
                        <td>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalProposedAmount))}</td>
                        <td>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalChange))}</td>
                        <td>{(totalSummary.Sum(f => f.totalTenors) / 30)}</td>
                    </tr>
                ";
            }
            return result;
        }

        private string GetForeignDirectFacilitiesMarkupLOS()
        {
            var result = String.Empty;
            var loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN && l.ISDISBURSED == true).ToList();
            var overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                                 && l.ISDISBURSED == true).ToList();

            var loanGroups = loans.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in loanGroups)
            {
                var facility = group.Key;
                var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
                var currentAmount = group.Sum(p => p.OUTSTANDINGPRINCIPAL) + group.Sum(p => p.OUTSTANDINGINTEREST);
                var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : 0;
                var lLLImpact = (100 / 100) * proposedAmount;
                var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                var tenor = group.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

                result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", lLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
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
                                      group.FirstOrDefault().TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : 0;
                var lLLImpact = (100 / 100) * proposedAmount;
                var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                var tenor = group.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);

                result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", lLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
            }

            foreach (var d in this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN))
            {
                var loanFacilityExists = loans.Exists(l => l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);
                var overdraftFacilityExists = overdrafts.Exists(o => o.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);

                if (!loanFacilityExists || !overdraftFacilityExists)
                {
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
                    var lLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;

                    result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", lLLImpact)}</td>
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
                        <td>{totalDirectSummary.numberOfLoans + totalDirectSummary.numberOfOverdrafts + totalDirectSummary.numberOfNewFacilities}</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>{String.Format("{0:0,0.00}", totalDirectSummary.totalCurrentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", totalDirectSummary.totalProposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", totalDirectSummary.totalChange)}</td>
                        <td>{(totalDirectSummary.totalTenors / 30)}</td>
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
            var contingentsGroup = contingents.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in contingentsGroup)
            {
                var facility = group.Key;
                var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
                var currentAmount = group.Sum(p => p.CONTINGENTAMOUNT);
                var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.TBL_PRODUCT.PRODUCTID ==
                                         group.FirstOrDefault().TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                var tenorTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.TBL_PRODUCT.PRODUCTID ==
                                         group.FirstOrDefault().TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : 0;
                var lLLImpact = (1 / 3) * proposedAmount;
                var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                var tenor = tenorTest;

                result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", lLLImpact)}</td>
                        <td>{currency}</td>
                        <td>{String.Format("{0:0,0.00}", currentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", proposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", change)}</td>
                        <td>{(tenor / 30)}</td>
                    </tr>
                    ";
            }

            foreach (var d in this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN && d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID ==
                                                                                    (int)LoanProductTypeEnum.ContingentLiability))
            {
                var contingentExists = contingents.Exists(l => l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);

                if (!contingentExists)
                {
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
                    var lLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;

                    result = result + $@"
                     <tr>
                        <td>{facility}</td>
                        <td>{String.Format("{0:0,0.00}", lLLImpact)}</td>
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
                        <td>&nbsp;</td>
                        <td>{String.Format("{0:0,0.00}", totalContingentSummary.totalLLLImpact)}</td>
                        <td>{totalContingentSummary.currency}</td>
                        <td>{String.Format("{0:0,0.00}", totalContingentSummary.totalCurrentAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", totalContingentSummary.totalProposedAmount)}</td>
                        <td>{String.Format("{0:0,0.00}", totalContingentSummary.totalChange)}</td>
                        <td>{(totalContingentSummary.totalTenors / 30)}</td>
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
                        <td>&nbsp;</td>
                        <td>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalLLLImpact))}</td>
                        <td>{totalSummary.FirstOrDefault()?.currency}</td>
                        <td>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalCurrentAmount))}</td>
                        <td>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalProposedAmount))}</td>
                        <td>{String.Format("{0:0,0.00}", totalSummary.Sum(f => f.totalChange))}</td>
                        <td>{(totalSummary.Sum(f => f.totalTenors) / 30)}</td>
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
                loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN && l.ISDISBURSED == true).ToList();
                overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN
                                                                && l.ISDISBURSED == true).ToList();
                appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN).ToList();
            }
            else
            {
                loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN && l.ISDISBURSED == true).ToList();
                overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN
                                                                && l.ISDISBURSED == true).ToList();
                appDetails = this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN).ToList();
            }
            directSummary.numberOfLoans = loans.Count();
            directSummary.numberOfOverdrafts = overdrafts.Count();
            var loanGroups = loans.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in loanGroups)
            {
                var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
                var currentAmount = group.Sum(p => p.OUTSTANDINGPRINCIPAL) + group.Sum(p => p.OUTSTANDINGINTEREST);
                var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : 0;
                var LLLImpact = (100 / 100) * proposedAmount;
                var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
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
                var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
                var currentAmount = group.Sum(p => p.OVERDRAFTLIMIT);
                var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : 0;
                var LLLImpact = (100 / 100) * proposedAmount;
                var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
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
                var loanFacilityExists = loans.Exists(l => l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);
                var overdraftFacilityExists = overdrafts.Exists(o => o.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);

                if (!loanFacilityExists && !overdraftFacilityExists)
                {
                    ++numberOfNewFacilities;
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
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
                var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
                var currentAmount = group.Sum(p => p.CONTINGENTAMOUNT);
                var proposedAmountTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                var tenorTest = (currencyId == (int)CurrencyEnum.NGN) ?
                                         (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0
                                          :
                                          (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID != (int)CurrencyEnum.NGN)?.Sum(p => p.APPROVEDTENOR)) ?? 0;
                var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : 0;
                var LLLImpact = (100 / 100) * proposedAmount;
                var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
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
                var contingentsFacilityExists = contingents.Exists(l => l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);

                if (!contingentsFacilityExists)
                {
                    ++numberOfNewFacilities;
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
                    var LLLImpact = (100 / 100) * proposedAmount;
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

        private TotalFacilitiesSummaryViewModel GetTotalImportFinanceFacilitiesSummaryLOS()
        {   //not yet implemented, just code for dummy data
            var directSummary = new TotalFacilitiesSummaryViewModel();
            int numberOfNewFacilities = 0;
            var loans = context.TBL_LOAN.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN).ToList();
            var overdrafts = context.TBL_LOAN_REVOLVING.Where(l => l.CUSTOMERID == loanApplication.CUSTOMERID && l.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN).ToList();
            directSummary.numberOfLoans = loans.Count();
            directSummary.numberOfOverdrafts = overdrafts.Count();
            var loanGroups = loans.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in loanGroups)
            {
                var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
                var currentAmount = group.Sum(p => p.OUTSTANDINGPRINCIPAL) + group.Sum(p => p.OUTSTANDINGINTEREST);
                var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => group.FirstOrDefault().TBL_PRODUCT.PRODUCTID ==
                                          f.TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : 0;
                var lLLImpact = (100 / 100) * proposedAmount;
                var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                var tenor = group.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);
                directSummary.totalCurrentAmount += currentAmount;
                directSummary.totalProposedAmount += proposedAmount;
                directSummary.totalChange += change;
                directSummary.totalTenors += tenor;
            }

            var overdraftGroups = overdrafts.GroupBy(f => f.TBL_PRODUCT.PRODUCTNAME);
            foreach (var group in overdraftGroups)
            {
                var facility = group.Key;
                var currency = group.First().TBL_CURRENCY.CURRENCYNAME;
                var currentAmount = group.Sum(p => p.OVERDRAFTLIMIT);
                var proposedAmountTest = (this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.TBL_PRODUCT.PRODUCTID ==
                                      group.FirstOrDefault().TBL_PRODUCT.PRODUCTID && f.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN)?.Sum(p => p.PROPOSEDAMOUNT)) ?? 0;
                var proposedAmount = (proposedAmountTest > 0) ? proposedAmountTest + currentAmount : 0;
                var lLLImpact = (100 / 100) * proposedAmount;
                var change = (proposedAmount > 0) ? proposedAmount - currentAmount : 0;
                var tenor = group.Sum(p => p.TBL_LOAN_APPLICATION_DETAIL.APPROVEDTENOR);
                directSummary.totalCurrentAmount += currentAmount;
                directSummary.totalProposedAmount += proposedAmount;
                directSummary.totalChange += change;
                directSummary.totalTenors += tenor;
            }

            foreach (var d in this.loanApplication.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.TBL_CURRENCY.CURRENCYID == (int)CurrencyEnum.NGN))
            {
                var loanFacilityExists = loans.Exists(l => l.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);
                var overdraftFacilityExists = overdrafts.Exists(o => o.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID == d.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPEID);

                if (!loanFacilityExists || !overdraftFacilityExists)
                {
                    ++numberOfNewFacilities;
                    var facility = d.TBL_PRODUCT.PRODUCTNAME;
                    var currency = d.TBL_CURRENCY.CURRENCYNAME;
                    var currentAmount = 0;
                    var proposedAmount = d.PROPOSEDAMOUNT;
                    var lLLImpact = (100 / 100) * proposedAmount;
                    var change = proposedAmount - currentAmount;
                    var tenor = d.APPROVEDTENOR;
                    directSummary.totalCurrentAmount += currentAmount;
                    directSummary.totalProposedAmount += proposedAmount;
                    directSummary.totalChange += change;
                    directSummary.totalTenors += tenor;
                }
            }
            directSummary.numberOfNewFacilities = numberOfNewFacilities;
            return directSummary;
        }

        private string GetGroupExposureMarkup()
        {
            var result = String.Empty;
            var exposures = GetGroupExposurebyCustomerId((int)loanApplication.CUSTOMERID, 1);
            var exposureGroupsByCustomer = exposures.GroupBy(e => e.customerName);
            var n = 0;
            result = result + $@"
                <table align=center border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th>Customer Name</th>
                        <th>Facility Type</th>
                        <th>Approved Amount</th>
                        <th>Currency</th>
                        <th>Current Amount</th>
                        <th>Maturity</th>
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
                        <td>{String.Format("{0:0,0.00}", facility.Sum(f => f.approvedAmount))}</td>
                        <td>{facility.FirstOrDefault()?.currency}</td>
                        <td>{String.Format("{0:0,0.00}", facility.Sum(f => f.outstandings))}</td>
                        <td>{facility.Max(f => f.maturityDate).ToShortDateString()}</td>
                    </tr>
                ";
                }
            }
            result = result + $@"
                    <tr>
                        <td>Total:</td>
                    </tr>
                    {GetTotalGroupExposureMarkup()}
                ";
            result = result + $"</table align=center>";
            return result;
        }

        private string GetTotalGroupExposureMarkup()
        {
            var result = String.Empty;
            var exposures = GetGroupExposurebyCustomerId((int)loanApplication.CUSTOMERID, 1);
            CurrentCustomerExposure totalExposure;
            
            totalExposure = new CurrentCustomerExposure()
            {
                facilityType = "TOTAL",
                outstandings = exposures.Sum(t => t.outstandings),
                approvedAmount = exposures.Sum(t => t.approvedAmount),
            };
            
            result = result + $@"
                     <tr>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>{String.Format("{0:0,0.00}", totalExposure.approvedAmount)}</td>
                        <td>{exposures.FirstOrDefault()?.currency}</td>
                        <td>{String.Format("{0:0,0.00}", totalExposure.outstandings)}</td>
                        <td>&nbsp;</td>
                    </tr>
                ";

            return result;
        }

        private string GetApprovalsMarkupLOS()
        {
            var appraisals = GetAppraisalMemorandumTrail(this.targetId).ToList();
            var result = String.Empty;
            var rm = appraisals.Find(a => a.fromApprovalLevelName.ToLower().Contains("relationship manager"));
            var gh = appraisals.Find(a => a.fromApprovalLevelName.ToLower().Contains("group head"));
            var gdmd = appraisals.Find(a => a.fromApprovalLevelName.ToLower().Contains("gdmd"));
            var gmd = appraisals.Find(a => a.fromApprovalLevelName.ToLower().Contains("gmd"));
            var gmcc = appraisals.Find(a => a.fromApprovalLevelName.ToLower().Contains("group mcc"));
            var bcc = appraisals.Find(a => a.fromApprovalLevelName.ToLower().Contains("bcc"));
            var bod = appraisals.Find(a => a.fromApprovalLevelName.ToLower().Contains("board of directors"));
            result = result + $@"
                <table align=center border=1 width=1200 cellpadding=15 cellspacing=0>
                    <tr>
                        <th>Level</th>
                        <th>Name</th>
                        <th>Comment</th>
                        <th>Date</th>
                    </tr>
            
                    <tr>
                        <td>{rm?.fromApprovalLevelName.ToUpper()}</td>
                        <td>{rm?.fromStaffName}</td>
                        <td>{rm?.comment}</td>
                        <td>{rm?.systemArrivalDateTime}</td>
                    </tr>
                    <tr>
                        <td>{gh?.fromApprovalLevelName.ToUpper()}</td>
                        <td>{gh?.fromStaffName}</td>
                        <td>{gh?.comment}</td>
                        <td>{gh?.systemArrivalDateTime}</td>
                    </tr>
                    <tr>
                        <td><b><i>Required Concurrences</i></b></td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>Group Relationship Manager (if applicable)</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>Legal</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>Country, CRM</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>Country MD</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>Country MCC</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>CRO, Subsidiaries</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td><b><i>Credit Approval</i></b></td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>Head, African Subsidiaries</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                        <td>&nbsp;</td>
                    </tr>
                    <tr>
                        <td>{(gdmd != null ? gdmd.fromApprovalLevelName.ToUpper() : "GDMD")}</td>
                        <td>{gdmd?.fromStaffName}</td>
                        <td>{gdmd?.comment}</td>
                        <td>{gdmd?.systemArrivalDateTime}</td>
                    </tr>
                    <tr>
                        <td>{(gmd != null ? gmd?.fromApprovalLevelName.ToUpper() : "GMD")}</td>
                        <td>{gmd?.fromStaffName}</td>
                        <td>{gmd?.comment}</td>
                        <td>{gmd?.systemArrivalDateTime}</td>
                    </tr>
                    <tr>
                        <td>{(gmcc != null ? gmcc.fromApprovalLevelName.ToUpper() : "Group MCC")}</td>
                        <td>{gmcc?.fromStaffName}</td>
                        <td>{gmcc?.comment}</td>
                        <td>{gmcc?.systemArrivalDateTime}</td>
                    </tr>
                    <tr>
                        <td>{(bcc != null ? bcc.fromApprovalLevelName.ToUpper() : "BCC")}</td>
                        <td>{bcc?.fromStaffName}</td>
                        <td>{bcc?.comment}</>
                        <td>{bcc?.systemArrivalDateTime}</td>
                    </tr>
                    <tr>
                        <td>{(bod != null ? bod.fromApprovalLevelName.ToUpper() : "Board Of Directors")}</td>
                        <td>{bod?.fromStaffName}</td>
                        <td>{bod?.comment}</td>
                        <td>{bod?.systemArrivalDateTime}</td>
                    </tr>

";
            result = result + $"</table align=center>";
            return result;

        }

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
            var exposures = GetGroupExposurebyCustomerId((int)lmsrApplication.CUSTOMERID, 1);
            var n = 0;
            result = result + $@"
                <table align=center border=1>
                    <tr>
                        <th>S/N</th>
                        <th>Customer Name</th>
                        <th>Facility Type</th>
                        <th>Application Reference Number</th>
                        <th>Outstanding</th>
                        <th>Existing Limit</th>
                        <th>Loan Status</th>
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
            result = result + $"</table align=center>";
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
            content = content.Replace(rmCountryHolder, rmCountry);
            content = content.Replace(misCodeHolder, misCode);
            content = content.Replace(reviewTypeHolder, reviewType);
            content = content.Replace(preparedByHolder, preparedBy);
            content = content.Replace(businessSectorsHolder, businessSectors);
            content = content.Replace(exchangeRateHolder, exchangeRate);
            content = content.Replace(groupFacilitySummaryHolder, groupFacilitySummary);
            content = content.Replace(groupFacilitySummaryFcyHolder, groupFacilitySummaryFcy);
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

            return content;
        }

        // support methods // interface getter

        public List<CurrentCustomerExposure> GetCustomerExposure(List<CustomerExposure> customerIds, int companyId) // not used!
        {
            return loan.GetCurrentCustomerExposure(customerIds, companyId); // old maurer impl
        }

        // html markup

        private string CustomerExposureMarkup()
        {
            // var exposures = GetCustomerExposure(customerIds, companyId); // old maurer impl
            var exposures = GetCurrentCustomerExposure(); // new

            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table align=center border=1>
                    <tr>
                        <th>S/N</th>
                        <th>Facility Type</th>
                        <th>Existing Limit</th>
                        <th>Proposed Limit</th>
                        <th>Change</th>
                        <th>Outstandings</th>
                        <th>Past Due Obligations Principal</th>
                        <th>Past Due Obligations Interest</th>
                        <th>Review Date</th>
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
            result = result + $"</table align=center>";
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
            if (operationId == (int)OperationsEnum.CAM) return memo.GetApplicationMonitoringTriggers(targetId);
            return memo.GetApplicationMonitoringTriggersLms(targetId);
        }

        private string MonitoringTriggersMarkup()
        {
            var result = String.Empty;
            var triggers = GetMonitoringTriggers();

            var n = 0;
            result = result + $@"
                <table align=center border=1>
                    <tr>
                        <th>S/N</th>
                        <th>Facility</th>
                        <th>Monitoring Trigger</th>
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
            result = result + $"</table align=center>";
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
                    comment = x.s.COMMENT_,
                    ratingId = x.s.RATINGID,
                    productCustomerName = x.d.TBL_PRODUCT.PRODUCTNAME + " -- " + x.d.TBL_CUSTOMER.FIRSTNAME + " " + x.d.TBL_CUSTOMER.MIDDLENAME + " " + x.d.TBL_CUSTOMER.LASTNAME
                });
        }

        private string GetEnvironmentalSocialRiskMarkup() // TODO RATINGIS
        {
            var result = String.Empty;
            var summary = GetEnvironmentalSocialRisk();

            var n = 0;
            result = result + $@"
                <table align=center border=1>
                    <tr>
                        <th>S/N</th>
                        <th>Facility</th>
                        <th>Summary</th>
                        <th>Rating</th>
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
            result = result + $"</table align=center>";
            return result;
        }

        private string GetESGRating(int ratingId)
        {
            if (ratingId == 7) return "Low";
            if (ratingId == 8) return "Medium";
            if (ratingId == 9) return "High";
            return "N/A";
        }

        // Customer exposure

        public List<CurrentCustomerExposure> GetCurrentCustomerExposure()
        {
            List<CustomerProduct> details = new List<CustomerProduct>();
            IQueryable<CurrentCustomerExposure> exposure = null;
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();

            if (operationId == (int)OperationsEnum.CAM)
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
            //    <table align=center border=1>
            //        <tr>
            //            <th>S/N</th>
            //            <th>Account ID</th>
            //            <th>Scheme Type</th>
            //            <th>Min Debit Balance</th>
            //            <th>Max Debit Balance</th>
            //            <th>Min Creit Balance</th>
            //            <th>Max Credit Balance</th>
            //            <th>Debit Turnover</th>
            //            <th>Credit Turnover</th>
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
            //result = result + $"</table align=center>";
            return result;
        }
    }
}

/*
    Obligor Risk Rating:
    Industry Risk Rating:
    Review Type – Annual/Interim/Initial

*/
