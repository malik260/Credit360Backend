namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ACCREDITEDCONSULTANT_TYPE")]
    public partial class TBL_ACCREDITEDCONSULTANT_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_ACCREDITEDCONSULTANT_TYPE()
        {
            TBL_ACCREDITEDCONSULTANT = new HashSet<TBL_ACCREDITEDCONSULTANT>();
        }

        [Key]
        public int ACCREDITEDCONSULTANTID { get; set; }

        [Required]
        //[StringLength(100)]
        public string NAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_ACCREDITEDCONSULTANT> TBL_ACCREDITEDCONSULTANT { get; set; }
    }
}
