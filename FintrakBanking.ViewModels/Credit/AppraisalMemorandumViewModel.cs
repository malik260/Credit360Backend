using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class AppraisalMemorandumViewModel : GeneralEntity
    {
        public int appraisalMemorandumId { get; set; }
        public int loanApplicationId { get; set; }
        public string camRef { get; set; }
        public bool isCompleted { get; set; }
        public bool riskRated { get; set; }
        public string camDocumentation { get; set; }
        public string loanDetails { get; set; }
    }
}
