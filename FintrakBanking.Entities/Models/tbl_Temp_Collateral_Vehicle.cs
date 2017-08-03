namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Collateral_Vehicle")]
    public partial class tbl_Temp_Collateral_Vehicle
    {
        [Key]
        public int CollateralVehicleId { get; set; }

        public int CollateralCustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string VehicleType { get; set; }

        public short CollateralSubTypeId { get; set; }

        [StringLength(10)]
        public string VehicleStatus { get; set; }

        [Required]
        [StringLength(50)]
        public string VehicleMake { get; set; }

        [Required]
        [StringLength(50)]
        public string ModelName { get; set; }

        [Required]
        [StringLength(5)]
        public string ManufacturedDate { get; set; }

        [Required]
        [StringLength(50)]
        public string RegistrationNumber { get; set; }

        [StringLength(50)]
        public string SerialNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string ChasisNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string EngineNumber { get; set; }

        [Required]
        [StringLength(250)]
        public string NameOfOwner { get; set; }

        [Required]
        [StringLength(250)]
        public string RegistrationCompany { get; set; }

        [Column(TypeName = "money")]
        public decimal? ResaleValue { get; set; }

        public DateTime? ValuationDate { get; set; }

        [Column(TypeName = "money")]
        public decimal? LastValuationAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal InvoiceValue { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }
    }
}
