namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Marketable_Security")]
    public partial class tbl_Collateral_Marketable_Security
    {
        [Key]
        public int CollateralMarketableSecurityId { get; set; }

        public int CollateralCustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string SecurityType { get; set; }

        [Required]
        [StringLength(20)]
        public string SecurityCode { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(150)]
        public string IssuerName { get; set; }

        [StringLength(50)]
        public string IssuerReferenceNumber { get; set; }

        [Column(TypeName = "money")]
        public decimal? UnitValue { get; set; }

        public int? NumberOfUnits { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }
    }
}
