namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_DETL_CON")]
    public partial class TBL_LOAN_APPLICATION_DETL_CON
    {
        [Key]
        public int LOANAPPLICATIONCONSULTANTID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int ACCREDITEDCONSULTANTID { get; set; }

        [Required]
        //[StringLength(1000)]
        public string DESCRIPTION { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        //public virtual TBL_LOAN_APPLICATION_DETL_STA TBL_LOAN_APPLICATION_DETL_STA { get; set; }

    }
}
