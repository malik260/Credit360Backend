using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.Interfaces.WorkFlow
{
    public interface IWorkflow
    {
        int OperationId { set; }
        int? ProductClassId { set; }
        int? ProductId { set; }
        int StaffId { set; }
        int? ToStaffId { set; }
        int TargetId { set; }
        int CompanyId { set; }
        int Tenor { set; }
        string Comment { set; }
        decimal Amount { set; }
        int StatusId { get; set; }
        int GroupStatusId { get; }
        int NewState { get; }
        int? NextLevelId { get; set; }
        int? FinalLevel { set; }
        bool EmailNotification { set; }
        short? Vote { set; }
        bool InvestmentGrade { set; }
        bool Untenored { set; }
        bool Disputed { set; }
        bool PoliticallyExposed { set; }
        bool SmsNotification { set; }
        bool ExternalInitialization { set; }
        bool DeferredExecution { set; }
        bool KeepPending { set; }
        bool Saved { get; }
        float? InterestRateConcession { set; }
        float? FeeRateConcession { set; }

        AlertPlaceholders Placeholders { set; }
        WorkflowResponse Response { get; set; }

        bool LogActivity();
        void NextProcess(int companyId, int staffId, int operationId, int targetId, int? productClassId, string comment, bool external, bool deferred);

        bool LogForApproval(ApprovalViewModel model); // <- this property is deprecated!!!
    }

    public class WorkflowResponse
    {
        public WorkflowResponse () { success = false; }
        public int statusId { get; set; }
        public int stateId { get; set; }
        public string statusName { get; set; }
        public int? nextLevelId { get; set; }
        public string nextLevelName { get; set; }
        public int? nextPersonId { get; set; }
        public string nextPersonName { get; set; }

        public string nextOperationName { get; set; }
        public bool success { get; set; }
    }

    public class AlertPlaceholders
    {
        public string customerName { get; set; }
        public string referenceNumber { get; set; }
        public string operationName { get; set; }
        public string branchName { get; set; }
        public string locationName { get; set; }
    }

}