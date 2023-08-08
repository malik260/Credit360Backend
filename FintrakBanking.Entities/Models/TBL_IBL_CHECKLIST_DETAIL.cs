using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_IBL_CHECKLIST_DETAIL")]
    public class TBL_IBL_CHECKLIST_DETAIL
    {
        [Key]
        public int IBLCHECKLISTDETAILID { get; set; }
        public int CUSTOMERID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public int IBLCHECKLISTID { get; set; }
        public int OPTIONID { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public int LASTUPDATEDBY { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
    }
}