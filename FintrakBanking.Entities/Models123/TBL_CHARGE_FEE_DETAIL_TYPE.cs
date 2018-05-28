namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CHARGE_FEE_DETAIL_TYPE")]
    public partial class TBL_CHARGE_FEE_DETAIL_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_CHARGE_FEE_DETAIL_TYPE()
        {
            TBL_CHARGE_FEE_DETAIL = new HashSet<TBL_CHARGE_FEE_DETAIL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short DETAILTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string DETAILTYPENAME { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_CHARGE_FEE_DETAIL> TBL_CHARGE_FEE_DETAIL { get; set; }
    }
}
