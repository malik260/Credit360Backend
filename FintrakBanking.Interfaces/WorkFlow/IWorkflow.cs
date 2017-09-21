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

        string Comment { set; }

        decimal Amount { set; }

        int StatusId { get; set; }

        int NextLevelId { set; }

        bool EmailNotification { set; }

        bool SmsNotification { set; }

        bool ExternalInitialization { set; }

        string Message { get; }

        bool Saved { get; }
    }
}
