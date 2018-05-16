namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_FEE_AMORTISATION_TYPE")]
    public partial class TBL_FEE_AMORTISATION_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_FEE_AMORTISATION_TYPE()
        {
            TBL_CHARGE_FEE = new HashSet<TBL_CHARGE_FEE>();
            
            TBL_TEMP_CHARGE_FEE = new HashSet<TBL_TEMP_CHARGE_FEE>();
            
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short FEEAMORTISATIONTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string FEEAMORTISATIONTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHARGE_FEE> TBL_CHARGE_FEE { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_CHARGE_FEE> TBL_TEMP_CHARGE_FEE { get; set; }
       
    }
}
