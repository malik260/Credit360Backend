namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_ESG_TYPE")]
    public partial class TBL_ESG_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_ESG_TYPE()
        {
            TBL_ESG_CHECKLIST_DETAIL = new HashSet<TBL_ESG_CHECKLIST_DETAIL>();
        }

        [Key]
        public short ESGTYPEID { get; set; }

        [Required]
        //[StringLength(100)]
        public string ESGTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_ESG_CHECKLIST_DETAIL> TBL_ESG_CHECKLIST_DETAIL { get; set; }
    }
}
