namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Custom_Fields")]
    public partial class tbl_Custom_Fields
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Custom_Fields()
        {
            tbl_Custom_Fields_Data = new HashSet<tbl_Custom_Fields_Data>();
        }

        [Key]
        public int CustomFieldId { get; set; }

        public int CompanyId { get; set; }

        public int HostPageId { get; set; }

        [Required]
        [StringLength(50)]
        public string LabelName { get; set; }

        [Required]
        [StringLength(20)]
        public string ControlKey { get; set; }

        [Required]
        [StringLength(20)]
        public string ControlType { get; set; }

        public bool Required { get; set; }

        public int ItemOrder { get; set; }

        public bool IsUpload { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public int ApprovalStatus { get; set; }

        public DateTime? DateActedOn { get; set; }

        public int? ActedOnBy { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Custom_Fields_Data> tbl_Custom_Fields_Data { get; set; }
    }
}
