namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_FS_Caption_Detail")]
    public partial class tbl_Customer_FS_Caption_Detail
    {
        [Key]
        public int FSDetailId { get; set; }

        public int CustomerId { get; set; }

        public int FSCaptionId { get; set; }

        [Column(TypeName = "date")]
        public DateTime FSDate { get; set; }

        [Column(TypeName = "money")]
        public decimal Amount { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Customer_FS_Caption tbl_Customer_FS_Caption { get; set; }
    }
}
