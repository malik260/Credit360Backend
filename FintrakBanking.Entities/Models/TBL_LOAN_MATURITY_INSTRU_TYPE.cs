namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_LOAN_MATURITY_INSTRU_TYPE")]
    public partial class TBL_LOAN_MATURITY_INSTRU_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_LOAN_MATURITY_INSTRU_TYPE()
        {
            TBL_LOAN_MATURITY_INSTRUCTION = new HashSet<TBL_LOAN_MATURITY_INSTRUCTION>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short INSTRUCTIONTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string INSTRUCTIONTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN_MATURITY_INSTRUCTION> TBL_LOAN_MATURITY_INSTRUCTION { get; set; }
    }
}
