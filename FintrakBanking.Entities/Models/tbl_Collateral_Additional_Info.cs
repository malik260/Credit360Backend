namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Additional_Info")]
    public partial class tbl_Collateral_Additional_Info
    {
        [Key]
        public int DocumentNumberId { get; set; }

        public int CustomerCollateralId { get; set; }

        [Required]
        [StringLength(50)]
        public string DocumentNumber { get; set; }

        public decimal Worth { get; set; }

        public bool IsBankAccount { get; set; }
    }
}
