namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("treasury.TBL_STOCK_PRICE")]
    public partial class TBL_STOCK_PRICE
    {
        [Key]
        public int STOCKPRICEID { get; set; }

        public int STOCKID { get; set; }

        [Column(TypeName = "date")]
        public DateTime DATE { get; set; }

        [Column(TypeName = "money")]
        public decimal STOCKPRICE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_STOCK_COMPANY TBL_STOCK_COMPANY { get; set; }
    }
}
