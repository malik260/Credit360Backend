using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class ConditionPrecedentViewModel : GeneralEntity
    {
        public int conditionId { get; set; }
        public string condition { get; set; }
        public bool? isExternal { get; set; }
        public int loanApplicationId { get; set; }

        public string staffName { get; set; }
    }
}
