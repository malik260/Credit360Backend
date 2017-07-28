namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Misc_Notes")]
    public partial class tbl_Collateral_Misc_Notes
    {
        [Key]
        public int Misc_NoteId { get; set; }

        public int? MiscellaneousId { get; set; }

        [Required]
        [StringLength(50)]
        public string ColumnName { get; set; }

        [StringLength(250)]
        public string ColumnValue { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Collateral_Miscellaneous tbl_Collateral_Miscellaneous { get; set; }
    }
}
