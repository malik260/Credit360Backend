using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    class RepaymentTermViewModel : GeneralEntity
    {
        public int repaymentTermId { get; set; }

        public string RepaymentTermIdDetail { get; set; }

        public bool DELETED { get; set; }
    }
}
