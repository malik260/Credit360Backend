namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PROFILE_PRIVILEDGE")]
    public partial class TBL_PROFILE_PRIVILEDGE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PROFILE_PRIVILEDGE()
        {
            TBL_PROFILE_PRIVILEDGE_ACTIVIT = new HashSet<TBL_PROFILE_PRIVILEDGE_ACTIVIT>();
        }

        [Key]
        public short PRIVILEDGEID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRIVILEDGENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PROFILE_PRIVILEDGE_ACTIVIT> TBL_PROFILE_PRIVILEDGE_ACTIVIT { get; set; }
    }
}
