namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.tbl_Temp_Product_Charge_Fee")]
    public partial class tbl_Temp_Product_Charge_Fee
    {
        [Key]
        public int ProductFeeId { get; set; }

        public short ProductId { get; set; }

        public int CompanyId { get; set; }

        public int ChargeFeeId { get; set; }

        [Column(TypeName = "money")]
        public decimal RateValue { get; set; }

        [Column(TypeName = "money")]
        public decimal? DependentAmount { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Charge_Fee tbl_Charge_Fee { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        public virtual tbl_Temp_Product tbl_Temp_Product { get; set; }
    }
}
