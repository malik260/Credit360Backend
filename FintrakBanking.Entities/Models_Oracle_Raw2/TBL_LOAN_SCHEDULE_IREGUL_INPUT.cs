namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_SCHEDULE_IREGUL_INPUT")]
    public partial class TBL_LOAN_SCHEDULE_IREGUL_INPUT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int IRREGULARSCHEDULEINPUTID { get; set; }

        public int LOANID { get; set; }

        public decimal PAYMENTAMOUNT { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime PAYMENTDATE { get; set; }
    }
}
