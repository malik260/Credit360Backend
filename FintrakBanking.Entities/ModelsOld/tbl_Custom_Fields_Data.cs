namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Custom_Fields_Data")]
    public partial class tbl_Custom_Fields_Data
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Custom_Fields_Data()
        {
            tbl_Custom_Field_Data_Upload = new HashSet<tbl_Custom_Field_Data_Upload>();
        }

        [Key]
        public int CustomFieldsDataId { get; set; }

        public int CustomFieldId { get; set; }

        [Required]
        public string DataDetails { get; set; }

        public int CreatedBy { get; set; }

        public int OwnerId { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int ApprovalStatus { get; set; }

        public DateTime? DateActedOn { get; set; }

        public int? ActedOnBy { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Custom_Field_Data_Upload> tbl_Custom_Field_Data_Upload { get; set; }

        public virtual tbl_Custom_Fields tbl_Custom_Fields { get; set; }

        public virtual tbl_Customer tbl_Customer { get; set; }

        public virtual tbl_Customer tbl_Customer1 { get; set; }
    }
}
