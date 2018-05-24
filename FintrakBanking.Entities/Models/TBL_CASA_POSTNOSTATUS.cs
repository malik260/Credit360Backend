namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CASA_POSTNOSTATUS")]
    public partial class TBL_CASA_POSTNOSTATUS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CASA_POSTNOSTATUS()
        {
            TBL_CASA = new HashSet<TBL_CASA>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short POSTNOSTATUSID { get; set; }

        [Required]
        [StringLength(50)]
        public string POSTNOSTATUSNAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CASA> TBL_CASA { get; set; }
    }
}
