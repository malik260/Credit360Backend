namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PROFILE_ACTIVITY")]
    public partial class TBL_PROFILE_ACTIVITY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PROFILE_ACTIVITY()
        {
            TBL_PROFILE_ADDITIONALACTIVITY = new HashSet<TBL_PROFILE_ADDITIONALACTIVITY>();
            TBL_TEMP_PROFILE_ADTN_ACTIVITY = new HashSet<TBL_TEMP_PROFILE_ADTN_ACTIVITY>();
            TBL_PROFILE_GROUP_ACTIVITY = new HashSet<TBL_PROFILE_GROUP_ACTIVITY>();
            TBL_PROFILE_PRIVILEDGE_ACTIVIT = new HashSet<TBL_PROFILE_PRIVILEDGE_ACTIVIT>();
            TBL_PROFILE_STAFF_ROLE_ADT_ACT = new HashSet<TBL_PROFILE_STAFF_ROLE_ADT_ACT>();
            TBL_TEMP_PROFILE_STAFF_ROLE_AA = new HashSet<TBL_TEMP_PROFILE_STAFF_ROLE_AA>();
        }

        [Key]
        public int ACTIVITYID { get; set; }

        public int ACTIVITYPARENTID { get; set; }

        [Required]
        [StringLength(100)]
        public string ACTIVITYNAME { get; set; }

        public virtual TBL_PROFILE_ACTIVITY_PARENT TBL_PROFILE_ACTIVITY_PARENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PROFILE_ADDITIONALACTIVITY> TBL_PROFILE_ADDITIONALACTIVITY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PROFILE_ADTN_ACTIVITY> TBL_TEMP_PROFILE_ADTN_ACTIVITY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PROFILE_GROUP_ACTIVITY> TBL_PROFILE_GROUP_ACTIVITY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PROFILE_PRIVILEDGE_ACTIVIT> TBL_PROFILE_PRIVILEDGE_ACTIVIT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PROFILE_STAFF_ROLE_ADT_ACT> TBL_PROFILE_STAFF_ROLE_ADT_ACT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PROFILE_STAFF_ROLE_AA> TBL_TEMP_PROFILE_STAFF_ROLE_AA { get; set; }
    }
}
