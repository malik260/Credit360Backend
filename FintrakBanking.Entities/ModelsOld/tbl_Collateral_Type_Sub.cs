namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.tbl_Collateral_Type_Sub")]
    public partial class tbl_Collateral_Type_Sub
    {
        [Key]
        public short CollateralSubTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string CollateralSubTypeName { get; set; }

        public int CollateralTypeId { get; set; }

        public double Haircut { get; set; }

        public int RevaluationDuration { get; set; }

        public virtual tbl_Collateral_Type tbl_Collateral_Type { get; set; }
    }
}
