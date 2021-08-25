namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_APPLICATION_COLT2_LOG")]
    public partial class TBL_LOAN_APPLICATION_COLT2_LOG
    {
        [Key]
        public int LOGCOLLATERALBASICDETAILID { get; set; }

        public int COLLATERALBASICDETAILID { get; set; }

        public int LOANAPPLICATIONID { get; set; }

        public int? LOANAPPLICATIONDETAILID { get; set; }

        [Required]
        //[StringLength(4000)]
        public string COLLATERALDETAIL { get; set; }

        [Column(TypeName = "money")]
        public decimal COLLATERALVALUE { get; set; }

        [Column(TypeName = "money")]
        public decimal STAMPEDTOCOVERAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime SYSTEMDATETIME { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION { get; set; }

        public virtual TBL_LOAN_APPLICATION TBL_LOAN_APPLICATION1 { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL1 { get; set; }
    }
}
