namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Currency_Rate")]
    public partial class tbl_Currency_Rate
    {
        [Key]
        public short CurrencyRateId { get; set; }

        public short CurrencyId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        public double BuyingRate { get; set; }

        public double SellingRate { get; set; }

        public short BaseCurrencyId { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }

        public virtual tbl_Currency tbl_Currency1 { get; set; }
    }
}
