namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CURRENCY_RATECODE")]
    public partial class TBL_CURRENCY_RATECODE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CURRENCY_RATECODE()
        {
            TBL_CURRENCY_EXCHANGERATE = new HashSet<TBL_CURRENCY_EXCHANGERATE>();
            TBL_LOAN = new HashSet<TBL_LOAN>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short RATECODEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string RATECODE { get; set; }

        [Required]
        //[StringLength(100)]
        public string RATECODEDESCRIPTION { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CURRENCY_EXCHANGERATE> TBL_CURRENCY_EXCHANGERATE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_LOAN> TBL_LOAN { get; set; }
    }
}
