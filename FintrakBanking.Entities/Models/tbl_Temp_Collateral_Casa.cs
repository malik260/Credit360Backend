namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Collateral_Casa")]
    public partial class tbl_Temp_Collateral_Casa
    {
        [Key]
        public int CollateralCasaId { get; set; }

        public int CollateralCustomerId { get; set; }

        public short? CollateralSubTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; }

        public bool IsOwnedByCustomer { get; set; }

        public short? CashTypeId { get; set; }

        [Column(TypeName = "money")]
        public decimal AvailableBalance { get; set; }

        [Column(TypeName = "money")]
        public decimal ExistingLienAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal LienAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal SecurityValue { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public virtual tbl_Collateral_Type_Sub tbl_Collateral_Type_Sub { get; set; }

        public virtual tbl_Temp_Collateral_Customer tbl_Temp_Collateral_Customer { get; set; }
    }
}
