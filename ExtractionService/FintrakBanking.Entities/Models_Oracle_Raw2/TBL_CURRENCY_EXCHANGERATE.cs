namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_CURRENCY_EXCHANGERATE")]
    public partial class TBL_CURRENCY_EXCHANGERATE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CURRENCYRATEID { get; set; }

        public int CURRENCYID { get; set; }

        public int RATECODEID { get; set; }

        public decimal EXCHANGERATE { get; set; }

        public int BASECURRENCYID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETED { get; set; }

        public DateTime? DATE_ { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY1 { get; set; }

        public virtual TBL_CURRENCY_RATECODE TBL_CURRENCY_RATECODE { get; set; }
    }
}
