namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Profile_Priviledge")]
    public partial class tbl_Profile_Priviledge
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_Profile_Priviledge()
        {
            tbl_Profile_Priviledge_Activity = new HashSet<tbl_Profile_Priviledge_Activity>();
        }

        [Key]
        public short PriviledgeId { get; set; }

        [Required]
        [StringLength(50)]
        public string PriviledgeName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_Profile_Priviledge_Activity> tbl_Profile_Priviledge_Activity { get; set; }
    }
}
