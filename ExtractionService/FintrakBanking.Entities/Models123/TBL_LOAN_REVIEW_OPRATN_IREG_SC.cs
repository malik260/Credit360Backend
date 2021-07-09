namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_REVIEW_OPRATN_IREG_SC")]
    public partial class TBL_LOAN_REVIEW_OPRATN_IREG_SC
    {
        [Key]
        public int IRREGULARSCHEDULEINPUTID { get; set; }

        public int LOANREVIEWOPERATIONID { get; set; }

        [Column(TypeName = "date")]
        public DateTime PAYMENTDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal PAYMENTAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public virtual TBL_LOAN_REVIEW_OPERATION TBL_LOAN_REVIEW_OPERATION { get; set; }
    }
}
