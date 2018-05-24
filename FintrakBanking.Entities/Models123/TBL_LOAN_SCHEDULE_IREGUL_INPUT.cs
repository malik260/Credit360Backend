namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_SCHEDULE_IREGUL_INPUT")]
    public partial class TBL_LOAN_SCHEDULE_IREGUL_INPUT
    {
        [Key]
        public int IRREGULARSCHEDULEINPUTID { get; set; }

        public int LOANID { get; set; }

        [Column(TypeName = "date")]
        public DateTime PAYMENTDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal PAYMENTAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }
    }
}
