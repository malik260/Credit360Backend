namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_ACCOUNT_CATEGORY")]
    public partial class TBL_ACCOUNT_CATEGORY
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_ACCOUNT_CATEGORY()
        {
            
            TBL_ACCOUNT_TYPE = new HashSet<TBL_ACCOUNT_TYPE>();
            TBL_FINANCIAL_STATEMENT_CAPTN = new HashSet<TBL_FINANCIAL_STATEMENT_CAPTN>();
            
        }

        [Key]
        public short ACCOUNTCATEGORYID { get; set; }

        [Required]
        [StringLength(50)]
        public string ACCOUNTCATEGORYNAME { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_ACCOUNT_TYPE> TBL_ACCOUNT_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_FINANCIAL_STATEMENT_CAPTN> TBL_FINANCIAL_STATEMENT_CAPTN { get; set; }

    }
}
