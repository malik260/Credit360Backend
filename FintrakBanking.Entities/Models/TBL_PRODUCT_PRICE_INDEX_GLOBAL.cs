namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_PRODUCT_PRICE_INDEX_GLOBAL")]
    public partial class TBL_PRODUCT_PRICE_INDEX_GLOBAL
    {
        [Key]
        public short PRODUCTPRICEINDEXGLOBALID { get; set; }
        public short PRODUCTPRICEINDEXID { get; set; }
        public double OLDRATE { get; set; }
        public double NEWRATE { get; set; }
        public DateTime EFFECTIVEDATE { get; set; }
        public short APPROVALSTATUSID { get; set; }
        public bool HASBEENAPPLIED { get; set; }
        public bool ISMARKETINDUCED { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
