namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_TYPE")]
    public partial class TBL_LOAN_APPLICATION_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_APPLICATION_TYPE()
        {
            TBL_LOAN_APPLICATION = new HashSet<TBL_LOAN_APPLICATION>();
            TBL_LOAN_APPLICATION_ARCHIVE = new HashSet<TBL_LOAN_APPLICATION_ARCHIVE>();
            TBL_LOAN_PRELIMINARY_EVALUATN = new HashSet<TBL_LOAN_PRELIMINARY_EVALUATN>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short LOANAPPLICATIONTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string LOANAPPLICATIONTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION> TBL_LOAN_APPLICATION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_APPLICATION_ARCHIVE> TBL_LOAN_APPLICATION_ARCHIVE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_PRELIMINARY_EVALUATN> TBL_LOAN_PRELIMINARY_EVALUATN { get; set; }
    }
}
