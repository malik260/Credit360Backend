using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class MemorandumRepository : IMemorandumRepository
    {
        // dependencies
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private ILoanRepository loan;

        public MemorandumRepository(FinTrakBankingContext context, IGeneralSetupRepository general, ILoanRepository loan)
        {
            this.context = context;
            this.general = general;
            this.loan = loan;
        }

        // init
        private int targetId;
        private int operationId;
        List<CustomerExposure> customerIds; // init

        // field variables
        TBL_LOAN_APPLICATION loanAppllication = null;
        TBL_LMSR_APPLICATION lmsrAppllication = null;
        private List<int> lmsCamOperationIds = new List<int> { 46, 71, 79 };

        // place holders
        public readonly string customerNameHolder = "@{{CustomerName}}";
        private readonly string branchNameHolder = "@{{BranchName}}";
        private readonly string locationNameHolder = "@{{LocationName}}";
        private readonly string customerExposureHolder = "@{{CustomerExposure}}";
        private readonly string recommendedInterestRateHolder = "@{{RecommendedInterest}}";
        private readonly string isRelatedPartyHolder = "@{{IsRelatedParty}}";
        private readonly string dateCreatedHolder = "@{{DateCreated}}";

        private readonly string accountNumbersHolder = "@{{AccountNumbers}}";
        private readonly string approvalLevelHolder = "@{{ApprovalLevel}}";
        private readonly string facilitySummaryHolder = "@{{FacilitySummary}}";
        private readonly string environmentalSocialRiskHolder = "@{{EnvironmentalSocialRisk}}";
        private readonly string monitoringTriggersHolder = "@{{MonitoringTriggers}}";

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
        private string facilitySummary;
        private string environmentalSocialRisk;
        private string monitoringTriggers;

        // init
        public bool Init(int operationId, int targetId)
        {
            this.targetId = targetId;
            this.operationId = operationId;

            if (operationId == (int)OperationsEnum.CAM) // LOS 
            {
                if (loanAppllication == null)
                {
                    this.loanAppllication = context.TBL_LOAN_APPLICATION.Find(targetId);
                    this.customerIds = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerExposure { customerId = x.CUSTOMERID }).Distinct().ToList();
                    this.customerExposure = CustomerExposureMarkup(this.customerIds, loanAppllication.COMPANYID);
                }

                //string customerName = String.Empty;
                if (loanAppllication.CUSTOMERGROUPID != null) this.customerName = loanAppllication.TBL_CUSTOMER_GROUP.GROUPNAME;
                if (loanAppllication.CUSTOMERID != null) this.customerName = loanAppllication.TBL_CUSTOMER.FIRSTNAME + " " + loanAppllication.TBL_CUSTOMER.MIDDLENAME + " " + loanAppllication.TBL_CUSTOMER.LASTNAME;

                this.branchName = loanAppllication.TBL_BRANCH.BRANCHNAME;
                this.locationName = loanAppllication.TBL_BRANCH.BRANCHNAME;
                this.isRelatedParty = loanAppllication.ISRELATEDPARTY == true ? "Yes" : "No";
                this.recommendedInterestRate = loanAppllication.INTERESTRATE.ToString();
                this.dateCreated = loanAppllication.DATETIMECREATED.ToShortDateString();

            }

            if (lmsCamOperationIds.Contains(operationId)) // LMS
            {
                if (lmsrAppllication == null)
                {
                    this.lmsrAppllication = context.TBL_LMSR_APPLICATION.Find(targetId);
                    this.customerIds = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).Select(x => new CustomerExposure { customerId = x.CUSTOMERID }).Distinct().ToList();
                    this.customerExposure = CustomerExposureMarkup(this.customerIds, loanAppllication.COMPANYID);
                }

                //string customerName = String.Empty;
                // if (lmsrAppllication.CUSTOMERGROUPID != null) this.customerName = lmsrAppllication.TBL_CUSTOMER_GROUP.GROUPNAME;
                if (lmsrAppllication.CUSTOMERID != null) this.customerName = lmsrAppllication.TBL_CUSTOMER.FIRSTNAME + " " + lmsrAppllication.TBL_CUSTOMER.MIDDLENAME + " " + lmsrAppllication.TBL_CUSTOMER.LASTNAME;

                this.branchName = lmsrAppllication.TBL_BRANCH.BRANCHNAME;
                this.locationName = lmsrAppllication.TBL_BRANCH.BRANCHNAME;
                //this.isRelatedParty = lmsrAppllication.ISRELATEDPARTY == true ? "Yes" : "No";
                //this.recommendedInterestRate = lmsrAppllication.INTERESTRATE.ToString();
                this.dateCreated = lmsrAppllication.DATETIMECREATED.ToShortDateString();

            }

            return true;
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
            content = content.Replace(branchNameHolder, branchName);
            content = content.Replace(locationNameHolder, locationName);
            content = content.Replace(isRelatedPartyHolder, isRelatedParty);
            content = content.Replace(recommendedInterestRateHolder, recommendedInterestRate);
            content = content.Replace(dateCreatedHolder, dateCreated);

            return content;
        }

        // support methods // interface getter

        public List<CurrentCustomerExposure> GetCustomerExposure(List<CustomerExposure> customerIds, int companyId)
        {
            return loan.GetCurrentCustomerExposure(customerIds, companyId);
        }

        // html markup

        private string CustomerExposureMarkup(List<CustomerExposure> customerIds, int companyId)
        {
            var exposures = GetCustomerExposure(customerIds, companyId);
            var result = String.Empty;
            var n = 0;
            result = result + $@"
                <table border=1>
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
            result = result + $"</table>";
            return result;

            /*
            int number = 1234567890;
            Convert.ToDecimal(number).ToString("#,##0.00");

            You will get the result 1,234,567,890.00.
            */
        }
    }
}

/*
    Facility Summary
    All Account Numbers:
    Monitoring Triggers
    Approval Level:
    Environmental & Social Risk Assessment

    Obligor Risk Rating:
    Industry Risk Rating:
    Review Type – Annual/Interim/Initial
*/
