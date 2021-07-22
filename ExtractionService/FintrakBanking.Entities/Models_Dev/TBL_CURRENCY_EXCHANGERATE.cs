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
        public short CURRENCYRATEID { get; set; }

        public short CURRENCYID { get; set; }

        public short RATECODEID { get; set; }

        public double EXCHANGERATE { get; set; }

        public short BASECURRENCYID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        [StringLength(700), Column(name: "DATE_")]
        public DateTime DATE { get; set; }

        public virtual TBL_CURRENCY TBL_CURRENCY { get; set; }

        public virtual TBL_CURRENCY_RATECODE TBL_CURRENCY_RATECODE { get; set; }
    }
}
