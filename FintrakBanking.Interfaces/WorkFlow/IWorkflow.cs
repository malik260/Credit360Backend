using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.WorkFlow
{
    public interface IWorkflow
    {
        Task<bool> LogActivity();
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
        int NextLevelId { set; }
        bool EmailNotification { set; }
        bool Vote { set; }
        bool InvestmentGrade { set; }
<<<<<<< HEAD
        bool PoliticallyExposed { set; }
=======
>>>>>>> 6c46ad2ef3ec373556311170ac7660692789c95b
        bool SmsNotification { set; }
        bool ExternalInitialization { set; }
        string Message { get; }
        bool Saved { get; }
    }
}
