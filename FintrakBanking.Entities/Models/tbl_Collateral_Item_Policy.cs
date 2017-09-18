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

<<<<<<< HEAD
=======
        [Column(TypeName = "money")]
        public decimal SumInsured { get; set; }

>>>>>>> 68cbb529932abeba6893c5306c87e679fa0c36a8
        [Required]
        [StringLength(50)]
        public string PolicyReferenceNumber { get; set; }

        [Required]
        [StringLength(250)]
        public string InsuranceCompanyName { get; set; }

        [Column(TypeName = "money")]
        public decimal SumInsured { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }
    }
}
