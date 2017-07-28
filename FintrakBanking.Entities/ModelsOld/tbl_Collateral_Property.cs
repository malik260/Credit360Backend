namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Property")]
    public partial class tbl_Collateral_Property
    {
        [Key]
        public int PropertyTypeId { get; set; }

        public int ColleralCustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string PropertyType { get; set; }

        public int CityId { get; set; }

        public short CountryId { get; set; }

        [Required]
        [StringLength(500)]
        public string PropertyAddress { get; set; }

        public DateTime? ConstructionDate { get; set; }

        public DateTime? PurchaseDate { get; set; }

        [StringLength(100)]
        public string ZoneClassification { get; set; }

        public byte? propertyValueBaseTypeId { get; set; }

        [Column(TypeName = "money")]
        public decimal? MarketValue { get; set; }

        [Column(TypeName = "money")]
        public decimal? GovtValue { get; set; }

        [Column(TypeName = "money")]
        public decimal? PropertyIndexValue { get; set; }

        public double? Haircut { get; set; }

        public DateTime? LastValuationDate { get; set; }

        [StringLength(150)]
        public string ValuationSource { get; set; }

        [Column(TypeName = "money")]
        public decimal? ValuationAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal? OtherLendersChargeAmount { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_City tbl_City { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }

        public virtual tbl_PropertyValue_Basetype tbl_PropertyValue_Basetype { get; set; }
    }
}
