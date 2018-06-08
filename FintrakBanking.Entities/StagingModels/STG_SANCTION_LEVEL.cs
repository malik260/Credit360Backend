using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.StagingModels
{
  public partial  class STG_SANCTION_LEVEL
    {
        [StringLength(10)]
        public string SANCTION_LEVEL { get; set; }
        [StringLength(255)]
        public string REF_DESC { get; set; }
        [StringLength(2)]
        public string DEL_FLG { get; set; }
        [StringLength(2)]
        public string BANK_ID { get; set; }
    }
}
