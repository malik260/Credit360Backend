namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_SECTOR")]
    public partial class TBL_SECTOR
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_SECTOR()
        {
            TBL_SUB_SECTOR = new HashSet<TBL_SUB_SECTOR>();
        }

        [Key]
        public short SECTORID { get; set; }

        [Required]
        [StringLength(200)]
        public string NAME { get; set; }

        [StringLength(10)]
        public string CODE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_SUB_SECTOR> TBL_SUB_SECTOR { get; set; }
    }
}
