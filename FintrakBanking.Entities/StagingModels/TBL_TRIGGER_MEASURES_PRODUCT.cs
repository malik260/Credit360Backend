using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.StagingModels
{
    [Table("TBL_TRIGGER_MEASURES_PRODUCT")]
    public partial class TBL_TRIGGER_MEASURES_PRODUCT
    {
        [Key]
        public int ID { get; set; }
        public DateTime DATE { get; set; }
        public string PRODUCTNAME { get; set; }
        public decimal DPD { get; set; }
        public decimal NPL { get; set; }
        public decimal DISBURSEMENT { get; set; }
        public decimal? LIQUIDATION { get; set; }
    }
}
