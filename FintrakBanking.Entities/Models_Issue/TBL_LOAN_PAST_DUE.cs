namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_PAST_DUE")]
    public partial class TBL_LOAN_PAST_DUE
    {
        [Key]
        public int PASTDUEID { get; set; }

        public int LOANID { get; set; }

        public short PRODUCTTYPEID { get; set; }

        //[Column(TypeName = "date")]
        public DateTime DATE { get; set; }

        public byte TRANSACTIONTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string PASTDUECODE { get; set; }

        [StringLength(50)]
        public string PARENT_PASTDUECODE { get; set; }

        [Required]
        [StringLength(800)]
        public string DESCRIPTION { get; set; }

        //[Column(TypeName = "money")]
        public decimal DEBITAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal CREDITAMOUNT { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        public virtual TBL_LOAN_TRANSACTION_TYPE TBL_LOAN_TRANSACTION_TYPE { get; set; }
    }
}
