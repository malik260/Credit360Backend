using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class CallMemoTypeViewModel
    {
        public int CallLimitTypeId { get; set; }

        public string Name { get; set; }
    }
    public class CallMemoLoanSearchViewModel
    {
        public int loanApplicationId { get; set; }
        public int? customerId { get; set; }
        public string customerName { get; set; }
        public decimal principalAmount { get; set; }
        public string loanReferenceNo { get; set; }
        public int operationId { get; set; }
        public string email { get; set; } //
        public string customerCode { get; set; }

    }
    public class CallMemoViewModel : GeneralEntity
    {
        public int? loopedStaffId;
        public int approvalStatusId;
        public int approvalTrailId;
        public string approvalStatusName;
        public string fromApprovalLevelName;
        public string toApprovalLevelName;

        public int callMemoId { get; set; }
        public int? approvalLevelId { get; set; }
        public int? loanApplicationId { get; set; }

        public string loanReferenceNo { get; set; }

        //public int staffId { get; set; }

        public string customerName { get; set; }

        //public short CallMemoTypeId { get; set; }

        //public string CallMemoType { get; set; }

        public DateTime memoDate { get; set; }

        public DateTime? nextCallDate { get; set; }

        public string purpose { get; set; }

        public string discusion { get; set; }

        public string action { get; set; }

        public string participants { get; set; }
        public string location { get; set; }
        public string background { get; set; }
        public string recentUpdate { get; set; }
        public string cc { get; set; }
        public int operationId { get; set; }
       
        public string approvalStatus { get; set; }
        public int customerId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string searchName { get; set; }
        public DateTime callTime { get; set; }
        public DateTime? nextCallTime { get; set; }
        public string comment { get; set; }
    }
    public class CallLimitViewModel : GeneralEntity
    {
        public int CallLimitId { get; set; }

        public int JobTitleId { get; set; }

        public string JobTitleName { get; set;}

        public decimal MaximumAmount { get; set; }

        public decimal MinimumAmount { get; set; }

        public short FrequencyId { get; set; }

        public string FrequencyName { get; set; }

        public int CallLimitTypeId { get; set; }

        public string CallLimitTypeName { get; set; }
    }
}
