namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Deposit")]
    public partial class tbl_Collateral_Deposit
    {
        [Key]
        public int TermDepositTranAccId { get; set; }

        public int ColleralCustomerId { get; set; }

        [StringLength(50)]
        public string AccountType { get; set; }

        [StringLength(50)]
        public string AccountNo { get; set; }

        [Column(TypeName = "money")]
        public decimal? AccountBalance { get; set; }

        [StringLength(5)]
        public string Contribution { get; set; }

        public DateTime? MaturityDate { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }
    }
}
