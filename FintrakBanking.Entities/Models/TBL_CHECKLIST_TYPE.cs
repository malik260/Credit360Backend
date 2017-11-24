namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CHECKLIST_TYPE")]
    public partial class TBL_CHECKLIST_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CHECKLIST_TYPE()
        {
            TBL_CHECKLIST_DEFINITION = new HashSet<TBL_CHECKLIST_DEFINITION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CHECKLIST_TYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string CHECKLIST_TYPE_NAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHECKLIST_DEFINITION> TBL_CHECKLIST_DEFINITION { get; set; }
    }
}
