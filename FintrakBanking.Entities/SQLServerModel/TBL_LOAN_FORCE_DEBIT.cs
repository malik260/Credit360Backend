namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_FORCE_DEBIT")]
    public partial class TBL_LOAN_FORCE_DEBIT
    {
        [Key]
        public int FORCEDEBITID { get; set; }

        public int LOANID { get; set; }

        public short PRODUCTTYPEID { get; set; }

        [Column(TypeName = "date")]
        public DateTime DATE { get; set; }

        public byte TRANSACTIONTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string FORCEDEBITCODE { get; set; }

        //[StringLength(50)]
        public string PARENT_FORCEDEBITCODE { get; set; }

        [Required]
        //[StringLength(800)]
        public string DESCRIPTION { get; set; }

        [Column(TypeName = "money")]
        public decimal DEBITAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal CREDITAMOUNT { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        public virtual TBL_LOAN_TRANSACTION_TYPE TBL_LOAN_TRANSACTION_TYPE { get; set; }
    }
}
