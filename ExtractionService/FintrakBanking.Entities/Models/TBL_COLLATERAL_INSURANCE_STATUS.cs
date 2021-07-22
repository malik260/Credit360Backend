using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATERAL_INSURANCE_STATUS")]
    public class TBL_COLLATERAL_INSURANCE_STATUS
    {
        [Key]
        public int INSURANCESTATUSID { get; set; }
        public string INSURANCESTATUS { get; set; }
        public bool DELETED { get; set; }

    }
}
