namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_FS_Ratio_Detail")]
    public partial class tbl_Customer_FS_Ratio_Detail
    {
        [Key]
        public int RatioDetailId { get; set; }

        public short RatioCaptionId { get; set; }

        public int FSCaptionId { get; set; }

        public short DivisorTypeId { get; set; }

        public double Multiplier { get; set; }

        [StringLength(50)]
        public string Description { get; set; }

        public short ValueTypeId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Customer_FS_Caption tbl_Customer_FS_Caption { get; set; }

        public virtual tbl_Customer_FS_Ratio_Caption tbl_Customer_FS_Ratio_Caption { get; set; }

        public virtual tbl_Customer_FS_Ratio_DivisorType tbl_Customer_FS_Ratio_DivisorType { get; set; }

        public virtual tbl_Customer_FS_Ratio_ValueType tbl_Customer_FS_Ratio_ValueType { get; set; }
    }
}
