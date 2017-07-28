namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("finance.tbl_Chart_Of_Account_Currency")]
    public partial class tbl_Chart_Of_Account_Currency
    {
        [Key]
        public int GLAccountCurrencyId { get; set; }

        public int GLAccountId { get; set; }

        public short CurrencyId { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public int CreatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public virtual tbl_Currency tbl_Currency { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account { get; set; }
    }
}
