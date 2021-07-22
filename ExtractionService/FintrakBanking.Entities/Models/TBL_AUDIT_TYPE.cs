namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_AUDIT_TYPE")]
    public partial class TBL_AUDIT_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_AUDIT_TYPE()
        {
            TBL_AUDIT = new HashSet<TBL_AUDIT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short AUDITTYPEID { get; set; }

        //[StringLength(100)]
        public string AUDITTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_AUDIT> TBL_AUDIT { get; set; }
    }
}
