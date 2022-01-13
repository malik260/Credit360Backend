using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.SupportUtility
{
    public class SupportUtilityViewModel : GeneralEntity
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
        public int fromApprovalLevelId { get; set; }
        public int toApprovalLevelId { get; set; }
        public int approvalStateId { get; set; }
        public short approvalStatusId { get; set; }
        public int operationId { get; set; }
        public string productName { get; set; }
        public int customerId { get; set; }
        public DateTime? applicationDate { get; set; }
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
        public DateTime transactionDate { get; set; }
        public object responsestaffName { get; set; }
        public object toStaffName { get; set; }
        public object toApprovalLevelName { get; set; }
        public object fromStaffName { get; set; }
    }

    public class ExpectedWorkflowViewModel : GeneralEntity
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

    public class StaffSupportUtilityViewModel
    {
        public int staffId { get; set; }
        public int DepartmentId  { get; set; }
        public string email { get; set; }
        public string gender { get; set; }
        public string FullName { get { return $"{FirstName} {MiddleName} {LastName}"; } }
        public string StaffFullName { get { return this.FirstName + " " + this.MiddleName + " " + this.LastName; } }
        public string Phone { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string StaffCode { get; set; }
        public int staffRoleId { get; set; }
        public string staffRoleName { get; set; }
        public int supervisorStafFId { get; set; }
        public int? supervisorStaffId { get; set; }
        public string supervisorStafFName { get; set; }
        public string supervisorStaffName { get; set; }
        public string misCode { get; set; }
        public short customerSensitivityLevelId { get; set; }
        public short? BranchId { get; set; }
        public int? jobTitleId { get; set; }
        public int? StateId { get; set; }
        public int? MisinfoId { get; set; }
        public int? businessUnitId { get; set; }
        public string SensitivityLevel { get; set; }
        public string businessUnitName { get; set; }
        public bool deleted { get; set; }
        public int? updatedById { get; set; }
        public string updatedBy { get; set; }
        public string BranchName { get; set; }
        public string deletedby { get; set; }
        public DateTime? timeUpdated { get; set; }
        public DateTime? timeDeleted { get; set; }
        public int? mainStaff { get; set; }
        public int? tempStaff { get; set; }
        public bool isCurrent { get; set; }
        public short ApprovalStatusId { get; set; }
        public DateTime? dateTimeCreated { get; set; }
        public DateTime? dateTimeUpdated { get; set; }
        public string approvalStatusName { get; set; }
        public string createdByName { get; set; }
    }

}

