namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Profile_Activity")]
    public partial class tbl_Profile_Activity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Profile_Activity()
        {
            tbl_Profile_AdditionalActivity = new HashSet<tbl_Profile_AdditionalActivity>();
            tbl_Profile_Group_Activity = new HashSet<tbl_Profile_Group_Activity>();
            tbl_Profile_Priviledge_Activity = new HashSet<tbl_Profile_Priviledge_Activity>();
        }

        [Key]
        public int ActivityId { get; set; }

        public int ActivityParentId { get; set; }

        [Required]
        [StringLength(100)]
        public string ActivityName { get; set; }

        public virtual tbl_Profile_Activity_Parent tbl_Profile_Activity_Parent { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Profile_AdditionalActivity> tbl_Profile_AdditionalActivity { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Profile_Group_Activity> tbl_Profile_Group_Activity { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Profile_Priviledge_Activity> tbl_Profile_Priviledge_Activity { get; set; }
    }
}
