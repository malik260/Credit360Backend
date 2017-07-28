namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Gaurantee")]
    public partial class tbl_Collateral_Gaurantee
    {
        [Key]
        public int CollateralGauranteeId { get; set; }

        public int CollateralCustomerId { get; set; }

        public short CollateralSubTypeId { get; set; }

        public bool? IsOwnedByCustomer { get; set; }

        [Required]
        [StringLength(50)]
        public string InstitutionName { get; set; }

        [Required]
        [StringLength(250)]
        public string GuarantorAddress { get; set; }

        [StringLength(50)]
        public string GuarantorReferenceNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string GuaranteeType { get; set; }

        [Column(TypeName = "money")]
        public decimal GuaranteeValue { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }
    }
}
