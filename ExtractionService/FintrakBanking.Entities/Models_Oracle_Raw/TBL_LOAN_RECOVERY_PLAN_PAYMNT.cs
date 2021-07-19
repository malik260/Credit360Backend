namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_RECOVERY_PLAN_PAYMNT")]
    public partial class TBL_LOAN_RECOVERY_PLAN_PAYMNT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RECOVERYPLANPAYMENTID { get; set; }

        public int RECOVERYPLANID { get; set; }

        public decimal PAYMENTAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime? PAYMENTDATE { get; set; }

        public virtual TBL_LOAN_RECOVERY_PLAN TBL_LOAN_RECOVERY_PLAN { get; set; }
    }
}
