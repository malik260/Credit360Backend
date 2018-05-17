namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("finance.TBL_FINANCIAL_STATEMENT_CAPTN")]
    public partial class TBL_FINANCIAL_STATEMENT_CAPTN
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_FINANCIAL_STATEMENT_CAPTN()
        {
            TBL_CHART_OF_ACCOUNT = new HashSet<TBL_CHART_OF_ACCOUNT>();
            TBL_TEMP_CHART_OF_ACCOUNT = new HashSet<TBL_TEMP_CHART_OF_ACCOUNT>();
        }

        [Key]
        public short FSCAPTIONID { get; set; }

        [Required]
        [StringLength(50)]
        public string FSCAPTIONCODE { get; set; }

        [Required]
        [StringLength(200)]
        public string FSCAPTION { get; set; }

        public int POSITION { get; set; }

        [StringLength(50)]
        public string REFNOTE { get; set; }

        [Required]
        [StringLength(20)]
        public string FINTYPE { get; set; }

        public short? ACCOUNTCATEGORYID { get; set; }

        public int? PARENTID { get; set; }

        public bool ISTOTALLINE { get; set; }

        [StringLength(50)]
        public string CAPTIONCOLOR { get; set; }

        public double? MULTIPLIER { get; set; }

        public virtual TBL_ACCOUNT_CATEGORY TBL_ACCOUNT_CATEGORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHART_OF_ACCOUNT> TBL_CHART_OF_ACCOUNT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CHART_OF_ACCOUNT> TBL_TEMP_CHART_OF_ACCOUNT { get; set; }
    }
}
