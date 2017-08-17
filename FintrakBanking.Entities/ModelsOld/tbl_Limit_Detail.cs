namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Limit_Detail")]
    public partial class tbl_Limit_Detail
    {
        [Key]
        public int LimitDetailId { get; set; }

        public int LimitTypeId { get; set; }

        public int LimitId { get; set; }

        public int TargetId { get; set; }

        [Column(TypeName = "money")]
        public decimal MinimumValue { get; set; }

        [Column(TypeName = "money")]
        public decimal MaximumValue { get; set; }

        public short LimitFrequencyTypeId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Frequency_Type tbl_Frequency_Type { get; set; }

        public virtual tbl_Limit tbl_Limit { get; set; }

        public virtual tbl_Limit_Type tbl_Limit_Type { get; set; }
    }
}
