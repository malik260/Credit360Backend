namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_STOCK_PRICE")]
    public partial class TBL_STOCK_PRICE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int STOCKPRICEID { get; set; }

        public int STOCKID { get; set; }

        public decimal STOCKPRICE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public DateTime? DATE_ { get; set; }

        public virtual TBL_STOCK_COMPANY TBL_STOCK_COMPANY { get; set; }
    }
}
