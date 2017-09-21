namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Immovable_Property")]
    public partial class tbl_Collateral_Immovable_Property
    {
        [Key]
        public int CollateralPropertyId { get; set; }

        public int CollateralCustomerId { get; set; }

        [Required]
        [StringLength(50)]
        public string PropertyName { get; set; }

        public int CityId { get; set; }

        public short CountryId { get; set; }

        public DateTime? ConstructionDate { get; set; }

        [Required]
        [StringLength(500)]
        public string PropertyAddress { get; set; }

        public DateTime DateOfAcquisition { get; set; }

        public DateTime LastValuationDate { get; set; }

        public short? ValuerId { get; set; }

        [StringLength(100)]
        public string ValuerReferenceNumber { get; set; }

        public short PropertyValueBaseTypeId { get; set; }

        [Column(TypeName = "money")]
        public decimal? OpenMarketValue { get; set; }

        [Column(TypeName = "money")]
        public decimal CollateralValue { get; set; }

        [Column(TypeName = "money")]
        public decimal? ForcedSaleValue { get; set; }

        [StringLength(10)]
        public string StampToCover { get; set; }

        [StringLength(50)]
        public string ValuationSource { get; set; }

        [Column(TypeName = "money")]
        public decimal OriginalValue { get; set; }

        [Column(TypeName = "money")]
        public decimal AvailableValue { get; set; }

        [Column(TypeName = "money")]
        public decimal SecurityValue { get; set; }

        [Column(TypeName = "money")]
        public decimal? CollateralUsableAmount { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        [StringLength(250)]
        public string NearestLandMark { get; set; }

        [StringLength(250)]
        public string NearestBusStop { get; set; }

        public decimal? Longitude { get; set; }

        public decimal? Latitude { get; set; }

        public virtual tbl_City tbl_City { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }

        public virtual tbl_Collateral_Immovable_Property tbl_Collateral_Immovable_Property1 { get; set; }

        public virtual tbl_Collateral_Immovable_Property tbl_Collateral_Immovable_Property2 { get; set; }
    }
}
