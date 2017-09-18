namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Item_Policy")]
    public partial class tbl_Collateral_Item_Policy
    {
        [Key]
        public int PolicyId { get; set; }

        public int CollateralCustomerId { get; set; }

        [Column(TypeName = "money")]
        public decimal SumInsured { get; set; }

        [Required]
        [StringLength(50)]
        public string PolicyReferenceNumber { get; set; }

        [Required]
        [StringLength(250)]
        public string InsuranceCompanyName { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
