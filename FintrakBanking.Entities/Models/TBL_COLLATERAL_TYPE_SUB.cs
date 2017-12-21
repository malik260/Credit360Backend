namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_TYPE_SUB")]
    public partial class TBL_COLLATERAL_TYPE_SUB
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_COLLATERAL_TYPE_SUB()
        {
            TBL_TEMP_COLLATERAL_CASA = new HashSet<TBL_TEMP_COLLATERAL_CASA>();
            TBL_TEMP_COLLATERAL_DEPOSIT = new HashSet<TBL_TEMP_COLLATERAL_DEPOSIT>();
            TBL_TEMP_COLLATERAL_IMMOVE_PRP = new HashSet<TBL_TEMP_COLLATERAL_IMMOVE_PRP>();
            TBL_TEMP_COLLATERAL_PLANT_EQUP = new HashSet<TBL_TEMP_COLLATERAL_PLANT_EQUP>();
        }

        [Key]
        public short COLLATERALSUBTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string COLLATERALSUBTYPENAME { get; set; }

        public int COLLATERALTYPEID { get; set; }

        public double HAIRCUT { get; set; }

        public int REVALUATIONDURATION { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_COLLATERAL_TYPE TBL_COLLATERAL_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_CASA> TBL_TEMP_COLLATERAL_CASA { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_DEPOSIT> TBL_TEMP_COLLATERAL_DEPOSIT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_IMMOVE_PRP> TBL_TEMP_COLLATERAL_IMMOVE_PRP { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_PLANT_EQUP> TBL_TEMP_COLLATERAL_PLANT_EQUP { get; set; }
    }
}
