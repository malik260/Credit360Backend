namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ESG_CHECKLIST_SUMMARY")]
    public partial class TBL_ESG_CHECKLIST_SUMMARY
    {
        [Key]
        public int ESGCHECKLISTSUMMARYID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }
        public int CHECKLIST_TYPEID { get; set; }

        public string COMMENT_ { get; set; }

        public int RATINGID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }
        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

    }
}
