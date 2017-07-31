namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Documents")]
    public partial class tbl_Collateral_Documents
    {
        [Key]
        public long DocumentId { get; set; }

        public int CollateralCustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentCategory { get; set; }

        [Required]
        [StringLength(500)]
        public string DocumentRef { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentCode { get; set; }

        [StringLength(100)]
        public string DocumentType { get; set; }

        public bool IsMandatory { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }
    }
}
