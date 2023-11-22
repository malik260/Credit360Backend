
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_FACILITY_STAMP_DUTY")]
    public class TBL_FACILITY_STAMP_DUTY
    {
        [Key]
        public int FACILITYSTAMPDUTYID { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }
        public int COLLATERALCUSTOMERID { get; set; }
        public int CURRENTSTATUS { get; set; }
        public string OSDC { get; set; }
        public DateTime DATETIMECREATED { get; set; }
    }
}