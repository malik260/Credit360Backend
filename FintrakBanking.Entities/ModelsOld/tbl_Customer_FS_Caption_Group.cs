namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Customer_FS_Caption_Group")]
    public partial class tbl_Customer_FS_Caption_Group
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Customer_FS_Caption_Group()
        {
            tbl_Customer = new HashSet<tbl_Customer>();
            tbl_Customer_FS_Caption = new HashSet<tbl_Customer_FS_Caption>();
        }

        [Key]
        public short FSCaptionGroupId { get; set; }

        [Required]
        [StringLength(50)]
        public string FSCaptionGroupName { get; set; }

        public int CompanyId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Company tbl_Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer> tbl_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Customer_FS_Caption> tbl_Customer_FS_Caption { get; set; }
    }
}
