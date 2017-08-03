namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Product_CollateralType")]
    public partial class tbl_Temp_Product_CollateralType
    {
        [Key]
        public int ProductCollateralTypeId { get; set; }

        public short ProductId { get; set; }

        public int CollateralTypeId { get; set; }

        public int CompanyId { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Company tbl_Company1 { get; set; }

        public virtual tbl_Collateral_Type tbl_Collateral_Type { get; set; }

        public virtual tbl_Collateral_Type tbl_Collateral_Type1 { get; set; }

        public virtual tbl_Temp_Product tbl_Temp_Product { get; set; }
    }
}
