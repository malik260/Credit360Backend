namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_PRODUCT_PRICE_INDEX_DAILY")]
    public partial class TBL_PRODUCT_PRICE_INDEX_DAILY
    {
        [Key]
        public int DAILYPRODUCTPRICEINDEXID { get; set; }

        public short PRODUCTPRICEINDEXID { get; set; }

        [Column(TypeName = "date")]
        public DateTime DATE { get; set; }

        public double PRICEINDEXRATE { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_PRODUCT_PRICE_INDEX TBL_PRODUCT_PRICE_INDEX { get; set; }
    }
}
