namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_COLLATERAL_PLANT_EQUP")]
    public partial class TBL_TEMP_COLLATERAL_PLANT_EQUP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALMACHINEDETAILID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(200)]
        public string MACHINENAME { get; set; }

        [StringLength(500)]
        public string DESCRIPTION { get; set; }

        [Required]
        [StringLength(50)]
        public string MACHINENUMBER { get; set; }

        [Required]
        [StringLength(50)]
        public string MANUFACTURERNAME { get; set; }

        [Required]
        [StringLength(5)]
        public string YEAROFMANUFACTURE { get; set; }

        [Required]
        [StringLength(5)]
        public string YEAROFPURCHASE { get; set; }

        public int VALUEBASETYPEID { get; set; }

        [StringLength(300)]
        public string MACHINECONDITION { get; set; }

        [Required]
        [StringLength(200)]
        public string MACHINERYLOCATION { get; set; }

        public decimal REPLACEMENTVALUE { get; set; }

        [StringLength(50)]
        public string EQUIPMENTSIZE { get; set; }

        [StringLength(150)]
        public string INTENDEDUSE { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public virtual TBL_COLLATERAL_VALUEBASE_TYPE TBL_COLLATERAL_VALUEBASE_TYPE { get; set; }
    }
}
