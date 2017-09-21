namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_AccreditedConsultant_Type")]
    public partial class tbl_AccreditedConsultant_Type
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_AccreditedConsultant_Type()
        {
            tbl_AccreditedConsultant = new HashSet<tbl_AccreditedConsultant>();
        }

        [Key]
        public int AccreditedConsultantId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_AccreditedConsultant> tbl_AccreditedConsultant { get; set; }
    }
}
