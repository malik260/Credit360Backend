namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_CASA_AccountStatus")]
    public partial class tbl_CASA_AccountStatus
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_CASA_AccountStatus()
        {
            tbl_CASA = new HashSet<tbl_CASA>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short AccountStatusId { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountStatusName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_CASA> tbl_CASA { get; set; }
    }
}
