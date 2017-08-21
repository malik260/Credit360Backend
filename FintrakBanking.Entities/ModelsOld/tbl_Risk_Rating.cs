namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Risk_Rating")]
    public partial class tbl_Risk_Rating
    {
        [Key]
        public int RiskRatingId { get; set; }

        [StringLength(10)]
        public string Rates { get; set; }

        public decimal? MaxRange { get; set; }

        public decimal? MinRange { get; set; }

        public decimal? AdvicedRate { get; set; }

        [StringLength(10)]
        public string RatesDescription { get; set; }

        public short ProductId { get; set; }

        public int CompanyId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }
    }
}
