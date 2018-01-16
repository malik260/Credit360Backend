namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("finance.TBL_FINANCIAL_STATEMENT_TYPE")]
    public partial class TBL_FINANCIAL_STATEMENT_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_FINANCIAL_STATEMENT_TYPE()
        {
            TBL_CUSTOMER_FS_CAPTION = new HashSet<TBL_CUSTOMER_FS_CAPTION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short FSTYPEID { get; set; }

        [Required]
        [StringLength(150)]
        public string FSTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CUSTOMER_FS_CAPTION> TBL_CUSTOMER_FS_CAPTION { get; set; }
    }
}
