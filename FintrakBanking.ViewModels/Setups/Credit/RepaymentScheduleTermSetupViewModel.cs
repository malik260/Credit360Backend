using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
     public class RepaymentScheduleTermSetupViewModel : GeneralEntity
    {
        public int repaymentScheduleId { get; set; }

        public string repaymentScheduleDetail { get; set; }

        public bool DELETED { get; set; }
    }
}
