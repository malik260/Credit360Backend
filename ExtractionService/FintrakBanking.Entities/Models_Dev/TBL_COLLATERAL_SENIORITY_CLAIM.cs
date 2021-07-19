namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COLLATERAL_SENIORITY_CLAIM")]
    public partial class TBL_COLLATERAL_SENIORITY_CLAIM
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short COLLATERALSENIORITYOFCLAIMID { get; set; }

        [Required]
        [StringLength(50)]
        public string SENIORITYOFCLAIMS { get; set; }

        [StringLength(250)]
        public string DESCRIPTION { get; set; }
    }
}
