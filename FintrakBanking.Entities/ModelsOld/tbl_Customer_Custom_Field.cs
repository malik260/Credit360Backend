namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_Custom_Field")]
    public partial class tbl_Customer_Custom_Field
    {
        [Key]
        public int CustomerCustomFieldId { get; set; }

        public int? CustomerId { get; set; }

        public int DisplayOrder { get; set; }

        [Required]
        [StringLength(50)]
        public string Label { get; set; }

        [StringLength(250)]
        public string Value { get; set; }

        public bool? ShowByDefault { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        public DateTime? DateTimeCreated { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }
    }
}
