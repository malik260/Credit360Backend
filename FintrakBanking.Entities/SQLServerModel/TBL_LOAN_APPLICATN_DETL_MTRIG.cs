namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_APPLICATN_DETL_MTRIG")]
    public partial class TBL_LOAN_APPLICATN_DETL_MTRIG
    {
        [Key]
        public int LOAN_MONITORING_TRIGGERID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int? MONITORING_TRIGGERID { get; set; }

        [Required]
        //[StringLength(800)]
        public string MONITORING_TRIGGER { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_MONITORING_TRIG_SETUP TBL_LOAN_MONITORING_TRIG_SETUP { get; set; }
    }
}
