namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_REVIEW_OPRATN_IREG_SC")]
    public partial class TBL_LOAN_REVIEW_OPRATN_IREG_SC
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IRREGULARSCHEDULEINPUTID { get; set; }

        public int LOANREVIEWOPERATIONID { get; set; }

        public decimal PAYMENTAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? PAYMENTDATE { get; set; }

        public virtual TBL_LOAN_REVIEW_OPERATION TBL_LOAN_REVIEW_OPERATION { get; set; }
    }
}
