namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_GAURANTEE")]
    public partial class TBL_COLLATERAL_GAURANTEE
    {
        [Key]
        public int COLLATERALGAURANTEEID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(50)]
        public string INSTITUTIONNAME { get; set; }

        [Required]
        [StringLength(250)]
        public string GUARANTORADDRESS { get; set; }

        [Column(TypeName = "money")]
        public decimal GUARANTEEVALUE { get; set; }

        public DateTime STARTDATE { get; set; }

        public DateTime? ENDDATE { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
