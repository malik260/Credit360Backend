namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_DISBURSEMENT")]
    public partial class TBL_LOAN_DISBURSEMENT
    {
        [Key]
        public int LOANDISBURSEMENTID { get; set; }

        public int TERMLOANID { get; set; }

        [Column(TypeName = "money")]
        public decimal AMOUNTDISBURSED { get; set; }

        [Required]
        //[StringLength(1000)]
        public string NARRATION { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_LOAN TBL_LOAN { get; set; }
    }
}
