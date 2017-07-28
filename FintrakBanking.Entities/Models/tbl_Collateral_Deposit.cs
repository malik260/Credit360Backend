namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Deposit")]
    public partial class tbl_Collateral_Deposit
    {
        [Key]
        public int CollateralDepositId { get; set; }

        public int CollateralCustomerId { get; set; }

        public short? CollateralSubTypeId { get; set; }

        [StringLength(50)]
        public string DealReferenceNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountType { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; }

        [Column(TypeName = "money")]
        public decimal ExistingLienAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal LienAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AvailableBalance { get; set; }

        [Column(TypeName = "money")]
        public decimal SecurityValue { get; set; }

        public DateTime MaturityDate { get; set; }

        [Column(TypeName = "money")]
        public decimal MaturityAmount { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }

        public virtual tbl_Collateral_Type_Sub tbl_Collateral_Type_Sub { get; set; }
    }
}
