
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
        public DateTime DATETIMEUPDATED { get; set;}
        public bool ISSHARED { get; set; }
        public bool DELETED { get; set; }
        public decimal CUSTOMERPERCENTAGE { get; set; }
        public decimal BANKPERCENTAGE { get; set; }
        public string CSDC { get; set; }
        public string ASDC { get; set; }
        public string CONTRACTCODE { get; set; }
        public bool COLLECTED { get; set; }
    }
}