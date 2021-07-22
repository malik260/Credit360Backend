using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.StagingModels
{
    [Table("STG_TREASURY_RATE_TBL")]
    public  class STG_TREASURY_RATE_TBL
    {
        [Key]
        public int PRODUCTCURRENCYID { get; set; }
        public string PRODUCT { get; set; }
        public string CURRENCY { get; set; }

        public decimal OFFER_RATE { get; set; }
        public decimal BID_RATE { get; set; }
        public DateTime DATE { get; set; }
    }
}
