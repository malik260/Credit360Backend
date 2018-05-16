namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_REGION")]
    public partial class TBL_REGION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_REGION()
        {
            TBL_STATE = new HashSet<TBL_STATE>();
        }

        [Key]
        public int REGIONID { get; set; }

        public int COUNTRYID { get; set; }

        [Required]
        [StringLength(100)]
        public string REGIONNAME { get; set; }

        public virtual TBL_COUNTRY TBL_COUNTRY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_STATE> TBL_STATE { get; set; }
    }
}
