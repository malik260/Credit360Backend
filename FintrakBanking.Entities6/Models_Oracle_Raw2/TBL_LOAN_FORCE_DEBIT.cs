namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_LOAN_FORCE_DEBIT")]
    public partial class TBL_LOAN_FORCE_DEBIT
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FORCEDEBITID { get; set; }

        public int LOANID { get; set; }

        public int PRODUCTTYPEID { get; set; }

        public int TRANSACTIONTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string FORCEDEBITCODE { get; set; }

        [StringLength(50)]
        public string PARENT_FORCEDEBITCODE { get; set; }

        [Required]
        [StringLength(800)]
        public string DESCRIPTION { get; set; }

        public decimal DEBITAMOUNT { get; set; }

        public decimal CREDITAMOUNT { get; set; }

        public DateTime? DATE_ { get; set; }

        public virtual TBL_LOAN_TRANSACTION_TYPE TBL_LOAN_TRANSACTION_TYPE { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }
    }
}
