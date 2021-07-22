namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_VALUEBASE_TYPE")]
    public partial class TBL_COLLATERAL_VALUEBASE_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_COLLATERAL_VALUEBASE_TYPE()
        {
            TBL_COLLATERAL_PLANT_AND_EQUIP = new HashSet<TBL_COLLATERAL_PLANT_AND_EQUIP>();
            TBL_TEMP_COLLATERAL_PLANT_EQUP = new HashSet<TBL_TEMP_COLLATERAL_PLANT_EQUP>();
        }

        [Key]
        public short COLLATERALVALUEBASETYPEID { get; set; }

        [Required]
        //[StringLength(100)]
        public string VALUEBASETYPENAME { get; set; }

        public int COLLATERALTYPEID { get; set; }

        //[StringLength(500)]
        public string REMARK { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_COLLATERAL_PLANT_AND_EQUIP> TBL_COLLATERAL_PLANT_AND_EQUIP { get; set; }

        public virtual TBL_COLLATERAL_TYPE TBL_COLLATERAL_TYPE { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_TEMP_COLLATERAL_PLANT_EQUP> TBL_TEMP_COLLATERAL_PLANT_EQUP { get; set; }
    }
}
