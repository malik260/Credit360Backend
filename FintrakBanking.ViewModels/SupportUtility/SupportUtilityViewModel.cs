using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.SupportUtility
{
   public class SupportUtilityViewModel: GeneralEntity
   {
        public int supportIssueTypeId { get; set; }
        public string description { get; set; }
        public int? tag { get; set; }
   }

    public class WorkflowSupportUtilityViewModel : GeneralEntity
    {
        public string searchString { get; set; }
        public int approvalTrailId { get; set; }
        public int targetId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public DateTime arrivalDate { get; set; }
        public DateTime systemArrivalDate { get; set; }
        public DateTime responseDate { get; set; }
        public DateTime systemResponseDate { get; set; }
        public int requestStaffId { get; set; }
        public int responseStaffId { get; set; }
        public int tostaffId { get; set; }
        public int relievedStaffId { get; set; }
        public int fromApprovalLevelId {get; set; }
        public int toApprovalLevelId { get; set; }
        public int approvalStateId { get; set; }
        public short approvalStatusId { get; set; }
        public int operationId { get; set; }
        public string productName { get; set; }
        public int customerId { get; set; }
        public DateTime applicationDate { get; set; }
        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string customerName { get; set; }
        public string customerCode { get; set; }
        public string middleName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int? customerGroupId { get; set; }
        public short loanTypeId { get; set; }
        public decimal applicationAmount { get; set; }
        public decimal approvedAmount { get; set; }
        public short? productClassId { get; set; }
        public short productId { get; set; }
        public bool isRelatedParty { get; set; }
        public bool customerInfoValidated { get; set; }
        public bool isPoliticallyExposed { get; set; }
        public bool isInvestmentGrade { get; set; }
        public string approvalStatus { get; set; }
        public short applicationStatusId { get; set; }
        public string applicationStatus { get; set; }
        public string customerGroupName { get; set; }
        public string loanTypeName { get; set; }
        public string oprationName { get; set; }
    }

    public class ExpectedWorkflowViewModel: GeneralEntity
    {
        public string operationName { get; set; }
        public string productClassName { get; set; }
        public string groupName { get; set; }
        public short? productClassId { get; set; }
        public short? productId { get; set; }
        public int position { get; set; }
        public string levelName { get; set; }
        public int approvalLevelId { get; set; }
        public bool canApprove { get; set; }
        public int? approvalBusinessRuleId { get; set; }
        public decimal maximumAmount { get; set; }
        public string businessRule { get; set; }
        public string productName { get; set; }
    }
}
