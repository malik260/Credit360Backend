namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_CURRENCY_EXCHANGERATE")]
    public partial class TBL_CURRENCY_EXCHANGERATE
    {
        [Key]
        public short CURRENCYRATEID { get; set; }

        public short CURRENCYID { get; set; }

        public short RATECODEID { get; set; }

        [Column(TypeName = "date")]
        public DateTime DATE { get; set; }

        public double EXCHANGERATE { get; set; }

        public short BASECURRENCYID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY1 { get; set; }

        public virtual TBL_CURRENCY_RATECODE TBL_CURRENCY_RATECODE { get; set; }
    }
}
