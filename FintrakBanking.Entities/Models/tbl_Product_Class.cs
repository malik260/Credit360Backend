namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PRODUCT_CLASS")]
    public partial class TBL_PRODUCT_CLASS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PRODUCT_CLASS()
        {
            TBL_APPROVAL_GROUP_MAPPING = new HashSet<TBL_APPROVAL_GROUP_MAPPING>();
            TBL_PRODUCT = new HashSet<TBL_PRODUCT>();
            TBL_CREDIT_TEMPLATE = new HashSet<TBL_CREDIT_TEMPLATE>();
            TBL_LOAN_PRELIMINARY_EVALUATION = new HashSet<TBL_LOAN_PRELIMINARY_EVALUATION>();
            TBL_TEMP_PRODUCT = new HashSet<TBL_TEMP_PRODUCT>();
        }

        [Key]
        public short PRODUCTCLASSID { get; set; }

        [Required]
        [StringLength(50)]
        public string PRODUCTCLASSNAME { get; set; }

        public short PRODUCTCLASSTYPEID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_APPROVAL_GROUP_MAPPING> TBL_APPROVAL_GROUP_MAPPING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT> TBL_PRODUCT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CREDIT_TEMPLATE> TBL_CREDIT_TEMPLATE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_PRELIMINARY_EVALUATION> TBL_LOAN_PRELIMINARY_EVALUATION { get; set; }

        public virtual TBL_PRODUCT_CLASS_TYPE TBL_PRODUCT_CLASS_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_PRODUCT> TBL_TEMP_PRODUCT { get; set; }
    }
}
