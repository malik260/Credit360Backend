namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_SeniorityOfClaims")]
    public partial class tbl_Collateral_SeniorityOfClaims
    {
        [Key]
        public short CollateralSeniorityOfClaimId { get; set; }

        [Required]
        [StringLength(50)]
        public string SeniorityOfClaims { get; set; }

        [StringLength(250)]
        public string Description { get; set; }
    }
}
