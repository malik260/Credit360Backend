using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_COLLATERAL_VALUATION")]
    public partial class TBL_COLLATERAL_VALUATION
    {
        [Key]
        public int COLLATERALVALUATIONID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        public string VALUATIONNAME { get; set; }

        public string VALUATIONREASON { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int DELETED { get; set; }

        public string NARRATION { get; set; }
    }
}
