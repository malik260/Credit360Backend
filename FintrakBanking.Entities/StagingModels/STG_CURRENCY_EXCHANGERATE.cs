namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STG_CURRENCY_EXCHANGERATE
    {
        [Key]
        public short CURRENCYRATEID { get; set; }

        [Required]
        [StringLength(50)]
        public string CURRENCYCODE { get; set; }

        [Required]
        [StringLength(50)]
        public string RATECODE { get; set; }

        [Column(TypeName = "date")]
        public DateTime DATE { get; set; }

        public double EXCHANGERATE { get; set; }

        [Required]
        [StringLength(50)]
        public string BASECURRENCY { get; set; }
    }
}
