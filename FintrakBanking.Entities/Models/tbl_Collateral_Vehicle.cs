namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Vehicle")]
    public partial class tbl_Collateral_Vehicle
    {
        [Key]
        public int VehicleTypeID { get; set; }

        public int ColleralCustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string VehicleType { get; set; }

<<<<<<< HEAD
        [Required]
        [StringLength(10)]
        public string NewORUsed { get; set; }
=======
        public short CollateralSubTypeId { get; set; }

        [StringLength(10)]
        public string VehicleStatus { get; set; }
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba

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
        public string RegnNo { get; set; }

        [StringLength(50)]
        public string SerialNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string ChasisNo { get; set; }

        [Required]
        [StringLength(50)]
        public string EngineNo { get; set; }

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

<<<<<<< HEAD
        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }
=======
        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }
>>>>>>> 1234ffcc6749be1304beaae4f63e4587aa7a4aba
    }
}
