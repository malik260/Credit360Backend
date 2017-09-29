using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class ProductChargeFeeViewModel: GeneralEntity
    {
        public int productFeeId { get; set; }

        public short productId { get; set; }

        public int chargeFeeId { get; set; }

        public decimal rateValue { get; set; }

        public decimal? dependentAmount { get; set; }
    }
}
