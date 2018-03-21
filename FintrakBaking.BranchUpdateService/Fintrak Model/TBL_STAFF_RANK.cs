namespace FintrakBaking.BranchUpdateService.Fintrak_Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_STAFF_RANK")]
    public partial class TBL_STAFF_RANK
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_STAFF_RANK()
        {
            TBL_STAFF = new HashSet<TBL_STAFF>();
        }

        [Key]
        public int RANKID { get; set; }

        [StringLength(50)]
        public string RANKCODE { get; set; }

        [Required]
        [StringLength(50)]
        public string RANKNAME { get; set; }

        public int COMPANYID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STAFF> TBL_STAFF { get; set; }
    }
}
