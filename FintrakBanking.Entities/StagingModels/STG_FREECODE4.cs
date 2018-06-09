using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.StagingModels
{
   public partial class STG_FREECODE4
    {
        [StringLength(10)]
        public string FREECODE4 { get; set; }
        [StringLength(255)]
        public string REF_DESC { get; set; }
        [StringLength(2)]
        public string DEL_FLG { get; set; }
        [StringLength(2)]
        public string BANK_ID { get; set; }
    }
}
