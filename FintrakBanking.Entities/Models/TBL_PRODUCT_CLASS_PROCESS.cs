namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PRODUCT_CLASS_PROCESS")]
    public partial class TBL_PRODUCT_CLASS_PROCESS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_PRODUCT_CLASS_PROCESS()
        {
            TBL_PRODUCT_CLASS = new HashSet<TBL_PRODUCT_CLASS>();
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short PRODUCT_CLASS_PROCESSID { get; set; }

        [Required]
        [StringLength(100)]
        public string PRODUCT_CLASS_PROCESS_NAME { get; set; }

        public bool USE_AMOUNT_LIMIT { get; set; }

        //[Column(TypeName = "money")]
        public decimal? MAXIMUM_AMOUNT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_PRODUCT_CLASS> TBL_PRODUCT_CLASS { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }
    }
}
