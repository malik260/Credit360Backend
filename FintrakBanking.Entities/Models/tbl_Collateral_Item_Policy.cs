namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("credit.TBL_COLLATERAL_ITEM_POLICY")]
    public partial class TBL_COLLATERAL_ITEM_POLICY
    {
        [Key]
        public int POLICYID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(50)]
        public string POLICYREFERENCENUMBER { get; set; }

        [Required]
        [StringLength(250)]
        public string INSURANCECOMPANYNAME { get; set; }

        [Column(TypeName = "money")]
        public decimal SUMINSURED { get; set; }

        public DateTime STARTDATE { get; set; }

        public DateTime ENDDATE { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
