namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PROFILE_ACTIVITY_PARENT")]
    public partial class TBL_PROFILE_ACTIVITY_PARENT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PROFILE_ACTIVITY_PARENT()
        {
            TBL_PROFILE_ACTIVITY = new HashSet<TBL_PROFILE_ACTIVITY>();
        }

        [Key]
        public int ACTIVITYPARENTID { get; set; }

        [Required]
        [StringLength(200)]
        public string ACTIVITYPARENTNAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PROFILE_ACTIVITY> TBL_PROFILE_ACTIVITY { get; set; }
    }
}
