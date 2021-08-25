namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKSTAGING.STG_PRICE_INDEX_RATE")]
    public partial class STG_PRICE_INDEX_RATE
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public decimal ID { get; set; }

        public DateTime? PRICEDATE { get; set; }

        [Column(TypeName = "float")]
        public decimal? BID_RATE { get; set; }

        [StringLength(255)]
        public string PRICEINDEX { get; set; }

        [Column(TypeName = "float")]
        public decimal? OFFER_RATE { get; set; }

        [StringLength(20)]
        public string CURRENCY { get; set; }
    }
}
