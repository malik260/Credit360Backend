namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CURRENCY_RATE")]
    public partial class TBL_CURRENCY_RATE
    {
        [Key]
        public short CURRENCYRATEID { get; set; }

        public short CURRENCYID { get; set; }

        [Column(TypeName = "date")]
        public DateTime DATE { get; set; }

        public double BUYINGRATE { get; set; }

        public double SELLINGRATE { get; set; }

        public short BASECURRENCYID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY1 { get; set; }
    }
}
