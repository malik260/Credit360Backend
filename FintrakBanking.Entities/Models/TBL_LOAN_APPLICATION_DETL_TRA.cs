namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_DETL_TRA")]
    public partial class TBL_LOAN_APPLICATION_DETL_TRA
    {
        [Key]
        public int TRADDERID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public int MARKETID { get; set; }

        //[Column(TypeName = "money")]
        public decimal AVERAGE_MONTHLY_TURNOVER { get; set; }

        //[StringLength(300)]
        public string SOLDITEMS { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_MARKET TBL_LOAN_MARKET { get; set; }
    }
}
