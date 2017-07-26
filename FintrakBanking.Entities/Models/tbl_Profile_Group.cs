namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Profile_Group")]
    public partial class tbl_Profile_Group
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Profile_Group()
        {
            tbl_Profile_Group_Activity = new HashSet<tbl_Profile_Group_Activity>();
            tbl_Profile_UserGroup = new HashSet<tbl_Profile_UserGroup>();
        }

        [Key]
        public short GroupId { get; set; }

        [Required]
        [StringLength(100)]
        public string GroupName { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Profile_Group_Activity> tbl_Profile_Group_Activity { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Profile_UserGroup> tbl_Profile_UserGroup { get; set; }
    }
}
