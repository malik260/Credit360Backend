namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_COLLATERAL_PREC_METAL")]
    public partial class TBL_TEMP_COLLATERAL_PREC_METAL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
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

        public decimal? VALUATIONAMOUNT { get; set; }

        public decimal? UNITRATE { get; set; }

        [StringLength(100)]
        public string PRECIOUSMETALFORM { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }
    }
}
