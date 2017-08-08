namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("finance.tbl_Charge_Range")]
    public partial class tbl_Charge_Range
    {
        [Key]
        public int ChargeRangeId { get; set; }

        [Column(TypeName = "money")]
        public decimal? Minimum { get; set; }

        [Column(TypeName = "money")]
        public decimal? Maximum { get; set; }

        public bool? MinimumAndAbove { get; set; }

        public bool? MaximumAndBelow { get; set; }

        public double? Rate { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amount { get; set; }

        public int ChargeFeeId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Charge_Fee tbl_Charge_Fee { get; set; }
    }
}
