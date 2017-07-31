namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Policy")]
    public partial class tbl_Collateral_Policy
    {
        [Key]
        public int CollateralInsurancePolicyId { get; set; }

        public int CollateralCustomerId { get; set; }

        public short CollateralSubTypeId { get; set; }

        public bool IsOwnedByCustomer { get; set; }

        [Required]
        [StringLength(50)]
        public string InsurancePolicyNumber { get; set; }

        [Column(TypeName = "money")]
        public decimal PremiumAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal PolicyAmount { get; set; }

        [Required]
        [StringLength(10)]
        public string InsuranceCompanyName { get; set; }

        [Required]
        [StringLength(300)]
        public string InsurerAddress { get; set; }

        public DateTime PolicyStartDate { get; set; }

        public DateTime AssignDate { get; set; }

        public short? RenewalFrequencyTypeId { get; set; }

        [StringLength(500)]
        public string InsurerDetails { get; set; }

        public DateTime PolicyRenewalDate { get; set; }

        [StringLength(500)]
        public string Remark { get; set; }

        public virtual tbl_Frequency_Type tbl_Frequency_Type { get; set; }

        public virtual tbl_Collateral_Customer tbl_Collateral_Customer { get; set; }
    }
}
