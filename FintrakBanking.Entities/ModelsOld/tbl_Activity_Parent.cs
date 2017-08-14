namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Activity_Parent")]
    public partial class tbl_Activity_Parent
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Activity_Parent()
        {
            tbl_Profile_Activity = new HashSet<tbl_Profile_Activity>();
        }

        [Key]
        public int ActivityParentId { get; set; }

        [Required]
        [StringLength(200)]
        public string ActivityParentName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Profile_Activity> tbl_Profile_Activity { get; set; }
    }
}
