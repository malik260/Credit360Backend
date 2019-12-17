namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_COLLATRL2")]
    public partial class TBL_LOAN_APPLICATION_COLLATRL2
    {
        [Key]
        public int COLLATERALBASICDETAILID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int? LOANAPPLICATIONDETAILID { get; set; }

        [Required]
        //[StringLength(4000)]
        public string COLLATERALDETAIL { get; set; }

        //[Column(TypeName = "money")]
        public decimal COLLATERALVALUE { get; set; }

        //[Column(TypeName = "money")]
        public decimal STAMPEDTOCOVERAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }
    }
}
