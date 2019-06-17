using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATERAL_USAGE_STATUS")]
    public class TBL_COLLATERAL_USAGE_STATUS
    {
        [Key]
        public int COLLATERALUSAGESTATUSID { get; set; }
        public string USAGESTATUSNAME { get; set; }
    }
}
