namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_PreciousMetal")]
    public partial class tbl_Collateral_PreciousMetal
    {
        [Key]
        public int PreciousMetalId { get; set; }

        public int ColleralCustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string PreciousMetal { get; set; }

        [Required]
        [StringLength(50)]
        public string MetalType { get; set; }

        [Required]
        [StringLength(10)]
        public string WeighInGms { get; set; }

        [Column(TypeName = "money")]
        public decimal? ValuationAmount { get; set; }

        public double? UnitRate { get; set; }

        [StringLength(100)]
        public string PreciousMetalForm { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

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
