namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_ESG_CHECKLIST_SUMMARY")]
    public partial class TBL_ESG_CHECKLIST_SUMMARY
    {
        [Key]
        public int ESGCHECKLISTSUMMARYID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        [Required]
        //[StringLength(2000)]
        public string COMMENT { get; set; }

        public short RATINGID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }
    }
}
