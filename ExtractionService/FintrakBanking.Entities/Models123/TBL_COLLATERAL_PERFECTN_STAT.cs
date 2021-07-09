namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_PERFECTN_STAT")]
    public partial class TBL_COLLATERAL_PERFECTN_STAT
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_COLLATERAL_PERFECTN_STAT()
        {
            TBL_COLLATERAL_IMMOVE_PROPERTY = new HashSet<TBL_COLLATERAL_IMMOVE_PROPERTY>();
            TBL_TEMP_COLLATERAL_IMMOV_PROP = new HashSet<TBL_TEMP_COLLATERAL_IMMOV_PROP>();
        }

        [Key]
        public byte PERFECTIONSTATUSID { get; set; }

        [Required]
        [StringLength(50)]
        public string PERFECTIONSTATUSNAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_IMMOVE_PROPERTY> TBL_COLLATERAL_IMMOVE_PROPERTY { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_IMMOV_PROP> TBL_TEMP_COLLATERAL_IMMOV_PROP { get; set; }
    }
}
