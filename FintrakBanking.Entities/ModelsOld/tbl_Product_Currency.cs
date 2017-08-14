namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Product_Currency")]
    public partial class tbl_Product_Currency
    {
        [Key]
        public int ProductCurrencyId { get; set; }

        public short ProductId { get; set; }

        public short CurrencyId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }

        public virtual tbl_Product tbl_Product { get; set; }
    }
}
