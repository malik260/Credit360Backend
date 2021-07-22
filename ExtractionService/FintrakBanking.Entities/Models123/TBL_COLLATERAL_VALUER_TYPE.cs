namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_VALUER_TYPE")]
    public partial class TBL_COLLATERAL_VALUER_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_COLLATERAL_VALUER_TYPE()
        {
            TBL_COLLATERAL_VALUER = new HashSet<TBL_COLLATERAL_VALUER>();
        }

        [Key]
        public short COLLATERALVALUERTYPEID { get; set; }

        [StringLength(200)]
        public string VALUERTYPENAME { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_VALUER> TBL_COLLATERAL_VALUER { get; set; }
    }
}
