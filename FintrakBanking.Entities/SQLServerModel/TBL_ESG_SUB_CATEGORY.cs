namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_ESG_SUB_CATEGORY")]
    public partial class TBL_ESG_SUB_CATEGORY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_ESG_SUB_CATEGORY()
        {
            TBL_ESG_CHECKLIST_DEFINITION = new HashSet<TBL_ESG_CHECKLIST_DEFINITION>();
        }

        [Key]
        public int ESGSUBCATEGORYID { get; set; }

        [Required]
        //[StringLength(700)]
        public string ESGSUBCATEGORYNAME { get; set; }

        public int ESGCATEGORYID { get; set; }

        public virtual TBL_ESG_CATEGORY TBL_ESG_CATEGORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_ESG_CHECKLIST_DEFINITION> TBL_ESG_CHECKLIST_DEFINITION { get; set; }
    }
}
