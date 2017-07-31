namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("finance.tbl_Charges")]
    public partial class tbl_Charges
    {
        [Key]
        public int ChargeId { get; set; }

        [StringLength(50)]
        public string ChargeName { get; set; }

        public int OperationId { get; set; }

        public decimal SetValue { get; set; }

        public int GLAccountId { get; set; }

        public int Frequency { get; set; }

        public bool ApplyVAT { get; set; }

        public bool ApplyWHT { get; set; }

        public int CompanyId { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        public virtual tbl_Operations tbl_Operations { get; set; }

        public virtual tbl_Chart_Of_Account tbl_Chart_Of_Account { get; set; }
    }
}
