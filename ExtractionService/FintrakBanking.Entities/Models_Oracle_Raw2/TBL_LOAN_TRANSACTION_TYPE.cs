namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_TRANSACTION_TYPE")]
    public partial class TBL_LOAN_TRANSACTION_TYPE
    {
        public TBL_LOAN_TRANSACTION_TYPE()
        {
            TBL_DAILY_ACCRUAL = new HashSet<TBL_DAILY_ACCRUAL>();
            TBL_LOAN_FORCE_DEBIT = new HashSet<TBL_LOAN_FORCE_DEBIT>();
            TBL_LOAN_PAST_DUE = new HashSet<TBL_LOAN_PAST_DUE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TRANSACTIONTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string TRANSACTIONTYPENAME { get; set; }

        public virtual ICollection<TBL_DAILY_ACCRUAL> TBL_DAILY_ACCRUAL { get; set; }

        public virtual ICollection<TBL_LOAN_FORCE_DEBIT> TBL_LOAN_FORCE_DEBIT { get; set; }

        public virtual ICollection<TBL_LOAN_PAST_DUE> TBL_LOAN_PAST_DUE { get; set; }
    }
}
