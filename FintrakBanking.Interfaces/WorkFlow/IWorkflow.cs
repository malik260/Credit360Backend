using FintrakBanking.ViewModels.WorkFlow;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.WorkFlow
{
    public interface IWorkflow
    {
        bool LogActivity();

        int OperationId { set; }
        int? ProductClassId { set; }
        int? ProductId { set; }
        int StaffId { set; }
        int TargetId { set; }
        int CompanyId { set; }
        int Tenor { set; }
        string Comment { set; }
        decimal Amount { set; }
        int StatusId { get; set; }
        int NewState { get; }
        int? NextLevelId { get; set; }
        bool EmailNotification { set; }
        bool Vote { set; }
        bool InvestmentGrade { set; }
        bool PoliticallyExposed { set; }
        bool SmsNotification { set; }
        bool ExternalInitialization { set; }
        bool DeferredExecution { set; }
        bool KeepPending { set; }
        string Message { get; }
        bool Saved { get; }
        
        bool LogForApproval(ApprovalViewModel model);
    }
}