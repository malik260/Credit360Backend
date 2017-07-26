using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Credit
{
    public class CreditAssessmentMemoViewModel : GenaralEntity
    {
        public int loanApplicationId { get; set; }
        public string camref { get; set; }
        public string camdocumentation { get; set; }
    }
    public class CreditTemplateViewModel
    {
        public string creditTemplate { get; set; }
        public int productClassId { get; set; }
        public int approvalLevelId { get; set; }
    }

}
