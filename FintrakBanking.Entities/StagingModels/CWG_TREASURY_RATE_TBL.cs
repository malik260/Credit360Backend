namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class CWG_TREASURY_RATE_TBL
    {
        public int id { get; set; }

        [StringLength(50)]
        public string PRODUCT { get; set; }

        [StringLength(50)]
        public string CURRENCY { get; set; }

        public double? OFFER_RATE { get; set; }

        public double? BID_RATE { get; set; }

        public DateTime? DATE { get; set; }
    }
}
