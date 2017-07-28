namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Locations")]
    public partial class tbl_Collateral_Locations
    {
        [Key]
        public int CollateralLocationId { get; set; }

        public int CollateralTypeId { get; set; }

        public int CountryId { get; set; }

        public int CityId { get; set; }

        public int CompanyId { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_City tbl_City { get; set; }

        public virtual tbl_Collateral_Type tbl_Collateral_Type { get; set; }
    }
}
