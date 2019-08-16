using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class CollateralInsuranceRequestViewModel : GeneralEntity
    {
        public int collateralInsuranceRequestId { get; set; }

        public int requestNumber { get; set; }

        public string requestReason { get; set; }

        public string requestComment { get; set; }

        public int collateralCustomerId { get; set; }

    }
}
