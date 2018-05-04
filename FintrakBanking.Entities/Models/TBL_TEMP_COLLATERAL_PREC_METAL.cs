namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_COLLATERAL_PREC_METAL")]
    public partial class TBL_TEMP_COLLATERAL_PREC_METAL
    {
        [Key]
        public int TEMPCOLLATERALPRECIOUSMETALID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(100)]
        public string PRECIOUSMETALNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string METALTYPE { get; set; }

        [Required]
        [StringLength(10)]
        public string WEIGHTINGRAMMES { get; set; }

        [Column(TypeName = "money")]
        public decimal? VALUATIONAMOUNT { get; set; }

        public double? UNITRATE { get; set; }

        [StringLength(100)]
        public string PRECIOUSMETALFORM { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }
    }
}
