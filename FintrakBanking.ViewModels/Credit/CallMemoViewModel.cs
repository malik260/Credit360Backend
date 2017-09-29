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
    }
    public class CallMemoViewModel : GeneralEntity
    {
        public int CallMemoId { get; set; }

        public int LoanApplicationId { get; set; }

        public string LoanReferenceNo { get; set; }

        public int StaffId { get; set; }

        public string CustomerName { get; set; }

        public DateTime MemoDate { get; set; }

        public DateTime? NextCallDate { get; set; }

        public string Purpose { get; set; }

        public string Discusion { get; set; }

        public string Summary { get; set; }

        public string Action { get; set; }

        public string Recommendation { get; set; }
    }
    public class CallLimitViewModel : GeneralEntity
    {
        public int CallLimitId { get; set; }

        public int JobTitleId { get; set; }

        public string JobTitleName { get; set;}

        public decimal CallLimit { get; set; }

        public short FrequencyId { get; set; }

        public string FrequencyName { get; set; }

        public int CallLimitTypeId { get; set; }

        public string CallLimitTypeName { get; set; }
    }
}
