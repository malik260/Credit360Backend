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
        public bool isExternal { get; set; }
        public bool corporate { get; set; }
        public bool retail { get; set; }
        public short? productId { get; set; }
        public int loanApplicationId { get; set; }
        public string staffName { get; set; }
        public bool isSubsequent { get; set; }
        public int loanApplicationDetailId { get; set; }
    }
}



