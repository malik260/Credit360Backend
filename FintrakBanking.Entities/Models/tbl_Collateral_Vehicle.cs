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

        [Required]
        [StringLength(10)]
        public string NewORUsed { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [Required]
        [StringLength(5)]
        public string Year { get; set; }

        [Required]
        [StringLength(50)]
        public string RegnNo { get; set; }

        [Required]
        [StringLength(50)]
        public string ChasisNo { get; set; }

        [Required]
        [StringLength(50)]
        public string EngineNo { get; set; }

        [Required]
        [StringLength(250)]
        public string Owner { get; set; }

        [Required]
        [StringLength(250)]
        public string RegAuthority { get; set; }

        [Column(TypeName = "money")]
        public decimal? ResaleValue { get; set; }

        public DateTime? ValuationDate { get; set; }

        [Column(TypeName = "money")]
        public decimal? ValuationAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal InvoiceValue { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }
    }
}
