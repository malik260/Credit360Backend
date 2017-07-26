namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.CollateralDeposit_CashType")]
    public partial class CollateralDeposit_CashType
    {
        public short? CollateralDepositCashTypeId { get; set; }

        [StringLength(100)]
        public string CollateralDepositCashTypeName { get; set; }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        [Key]
        [Column(Order = 2)]
        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }
    }
}
