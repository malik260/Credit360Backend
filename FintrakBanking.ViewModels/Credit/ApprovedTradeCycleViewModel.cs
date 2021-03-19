using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class ApprovedTradeCycleViewModel : GeneralEntity
    {
        public int approvedTradeCycleId { get; set; }
        public int approvedTradeCycleDays { get; set; }
    }
}
