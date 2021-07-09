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
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALPRECIOUSMETALID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(100)]
        public string PRECIOUSMETALNAME { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(50)]
        public string METALTYPE { get; set; }

        [Key]
        [Column(Order = 4)]
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
