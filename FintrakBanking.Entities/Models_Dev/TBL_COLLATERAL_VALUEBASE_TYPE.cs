namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COLLATERAL_VALUEBASE_TYPE")]
    public partial class TBL_COLLATERAL_VALUEBASE_TYPE
    {
        public TBL_COLLATERAL_VALUEBASE_TYPE()
        {
            TBL_COLLATERAL_PLANT_AND_EQUIP = new HashSet<TBL_COLLATERAL_PLANT_AND_EQUIP>();
            TBL_TEMP_COLLATERAL_PLANT_EQUP = new HashSet<TBL_TEMP_COLLATERAL_PLANT_EQUP>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short COLLATERALVALUEBASETYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string VALUEBASETYPENAME { get; set; }

        public int COLLATERALTYPEID { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual ICollection<TBL_COLLATERAL_PLANT_AND_EQUIP> TBL_COLLATERAL_PLANT_AND_EQUIP { get; set; }

        public virtual TBL_COLLATERAL_TYPE TBL_COLLATERAL_TYPE { get; set; }

        public virtual ICollection<TBL_TEMP_COLLATERAL_PLANT_EQUP> TBL_TEMP_COLLATERAL_PLANT_EQUP { get; set; }
    }
}
