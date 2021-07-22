namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CASA_ACCOUNTSTATUS")]
    public partial class TBL_CASA_ACCOUNTSTATUS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CASA_ACCOUNTSTATUS()
        {
            TBL_CASA = new HashSet<TBL_CASA>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short ACCOUNTSTATUSID { get; set; }

        [Required]
        //[StringLength(50)]
        public string ACCOUNTSTATUSNAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CASA> TBL_CASA { get; set; }
    }
}
