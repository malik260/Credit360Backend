using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class CustomerProductFeeViewModel : GeneralEntity
    {
        public string productCode;
        public int productFeeId;

        public int customerProductFeeId { get; set; }

        public int customerId { get; set; }

        public short productId { get; set; }
        public string product { get; set; }

        public int chargeFeeId { get; set; }
        public string chargeFee { get; set; }

        public decimal rateValue { get; set; }

        public decimal? dependentAmount { get; set; }

    }
}
