namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class STG_PRICE_INDEX_RATE
    {
        public int ID { get; set; }

        [Column(TypeName = "date")]
        public DateTime? PRICEDATE { get; set; }

        public double? BID_RATE { get; set; }

        [StringLength(250)]
        public string PRICEINDEX { get; set; }

        public double? OFFER_RATE { get; set; }

        [StringLength(20)]
        public string CURRENCY { get; set; }
    }
}
