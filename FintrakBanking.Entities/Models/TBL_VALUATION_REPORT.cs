using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_VALUATION_REPORT")]
    public partial class TBL_VALUATION_REPORT
    {
        [Key]
        public int VALUATIONREPORTID { get; set; }

        public int COLLATERALVALUATIONID { get; set; }

        public int VALUERID { get; set; }

        //[Required]
        //[StringLength(500)]
        public string VALUERCOMMENT { get; set; }

        [Required]
        //[StringLength(20)]
        public string ACCOUNTNUMBER { get; set; }

        public decimal? VALUATIONFEE { get; set; }

        [Required]
        ////[StringLength(20)]
        public decimal? WHT { get; set; }
        public decimal? WHTAMOUNT { get; set; }

        public int COMPANYID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int? DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int APPROVALSTATUSID { get; set; }
    }
}
