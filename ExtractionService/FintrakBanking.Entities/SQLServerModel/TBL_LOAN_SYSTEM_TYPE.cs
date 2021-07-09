namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_SYSTEM_TYPE")]
    public partial class TBL_LOAN_SYSTEM_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_SYSTEM_TYPE()
        {
            TBL_LOAN = new HashSet<TBL_LOAN>();
            TBL_LOAN_ARCHIVE = new HashSet<TBL_LOAN_ARCHIVE>();
            TBL_LOAN_CAMSOL = new HashSet<TBL_LOAN_CAMSOL>();
            TBL_LOAN_COLLATERAL_MAPPING = new HashSet<TBL_LOAN_COLLATERAL_MAPPING>();
            TBL_LOAN_CONTINGENT = new HashSet<TBL_LOAN_CONTINGENT>();
            TBL_LOAN_COVENANT_DETAIL = new HashSet<TBL_LOAN_COVENANT_DETAIL>();
            TBL_LOAN_FEE = new HashSet<TBL_LOAN_FEE>();
            TBL_LOAN_FEE_ARCHIVE = new HashSet<TBL_LOAN_FEE_ARCHIVE>();
            TBL_LOAN_HISTORY = new HashSet<TBL_LOAN_HISTORY>();
            TBL_LOAN_MATURITY_INSTRUCTION = new HashSet<TBL_LOAN_MATURITY_INSTRUCTION>();
            TBL_LOAN_MONITORING_TRIGGER = new HashSet<TBL_LOAN_MONITORING_TRIGGER>();
            TBL_LOAN_REVOLVING = new HashSet<TBL_LOAN_REVOLVING>();
            TBL_LOAN_REVOLVING_ARCHIVE = new HashSet<TBL_LOAN_REVOLVING_ARCHIVE>();
            TBL_LOAN_REVOLVING_HISTORY = new HashSet<TBL_LOAN_REVOLVING_HISTORY>();
            TBL_TEMP_LOAN_CAMSOL = new HashSet<TBL_TEMP_LOAN_CAMSOL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LOANSYSTEMTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string LOANSYSTEMTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_ARCHIVE> TBL_LOAN_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CAMSOL> TBL_LOAN_CAMSOL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_COLLATERAL_MAPPING> TBL_LOAN_COLLATERAL_MAPPING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_CONTINGENT> TBL_LOAN_CONTINGENT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_COVENANT_DETAIL> TBL_LOAN_COVENANT_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_FEE> TBL_LOAN_FEE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_FEE_ARCHIVE> TBL_LOAN_FEE_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_HISTORY> TBL_LOAN_HISTORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_MATURITY_INSTRUCTION> TBL_LOAN_MATURITY_INSTRUCTION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_MONITORING_TRIGGER> TBL_LOAN_MONITORING_TRIGGER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING> TBL_LOAN_REVOLVING { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_ARCHIVE> TBL_LOAN_REVOLVING_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_REVOLVING_HISTORY> TBL_LOAN_REVOLVING_HISTORY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_LOAN_CAMSOL> TBL_TEMP_LOAN_CAMSOL { get; set; }
    }
}
