namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_APPLICATION_DETL_EDU")]
    public partial class TBL_LOAN_APPLICATION_DETL_EDU
    {
        [Key]
        public int EDUCATIONID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int NUMBER_OF_STUDENTS { get; set; }

        [Column(TypeName = "money")]
        public decimal AVERAGE_SCHOOL_FEES { get; set; }

        [Column(TypeName = "money")]
        public decimal TOTAL_PREVIOUS_TERM_SCHOL_FEES { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }
    }
}
