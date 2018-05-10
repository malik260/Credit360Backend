namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_APPROVAL_STATE")]
    public partial class TBL_APPROVAL_STATE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_APPROVAL_STATE()
        {
            TBL_APPROVAL_TRAIL = new HashSet<TBL_APPROVAL_TRAIL>();
        }

        [Key]
        public short APPROVALSTATEID { get; set; }

        [StringLength(50)]
        public string APPROVALSTATE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }
    }
}
