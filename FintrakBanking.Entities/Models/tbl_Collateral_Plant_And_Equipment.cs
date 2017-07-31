namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Plant_And_Equipment")]
    public partial class tbl_Collateral_Plant_And_Equipment
    {
        [Key]
        public int CollateralMachineDetailId { get; set; }

        public int CollateralCustomerId { get; set; }

        public short CollateralSubTypeId { get; set; }

        [Required]
        [StringLength(200)]
        public string MachineName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(50)]
        public string MachineNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string ManufacturerName { get; set; }

        [Required]
        [StringLength(5)]
        public string YearOfManufacture { get; set; }

        [Required]
        [StringLength(5)]
        public string YearOfPurchase { get; set; }

        public short ValueBaseTypeId { get; set; }

        [StringLength(300)]
        public string MachineCondition { get; set; }

        [Required]
        [StringLength(200)]
        public string MachineryLocation { get; set; }

        [Column(TypeName = "money")]
        public decimal ReplacementValue { get; set; }

        [StringLength(50)]
        public string EquipmentSize { get; set; }

        [StringLength(150)]
        public string IntendedUse { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }

        public virtual tbl_Collateral_Type_Sub tbl_Collateral_Type_Sub { get; set; }

        public virtual tbl_Collateral_Valuebase_Type tbl_Collateral_Valuebase_Type { get; set; }
    }
}
