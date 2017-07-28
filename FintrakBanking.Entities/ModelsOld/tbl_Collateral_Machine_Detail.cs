namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Machine_Detail")]
    public partial class tbl_Collateral_Machine_Detail
    {
        [Key]
        public int CollateralMachineDetailId { get; set; }

        public int CollateralCustomerId { get; set; }

        [Required]
        [StringLength(200)]
        public string MachineName { get; set; }

        [StringLength(50)]
        public string Manufacturer { get; set; }

        [Required]
        [StringLength(5)]
        public string ManufacturedYear { get; set; }

        [Required]
        [StringLength(5)]
        public string PurchasedYear { get; set; }

        public short MachineValueBaseTypeId { get; set; }

        [Required]
        [StringLength(200)]
        public string MachineryLocation { get; set; }

        [Column(TypeName = "money")]
        public decimal? ReplacementValue { get; set; }

        [Column(TypeName = "money")]
        public decimal? ThirdPartyChargeAmount { get; set; }

        [StringLength(300)]
        public string MachineryCondition { get; set; }

        [StringLength(150)]
        public string IntendedUse { get; set; }

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

        public virtual tbl_Collateral_ValueBase_Type tbl_Collateral_ValueBase_Type { get; set; }
    }
}
