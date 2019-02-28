namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_COLLATERAL_PROMISSORY")]
    public partial class TBL_COLLATERAL_PROMISSORY
    {
        [Key]
        public int COLLATERALPROMISSORYID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        public string PROMISSORYNOTEID { get; set; }

        //[Column(TypeName = "money")]
        public decimal PROMISSORYVALUE { get; set; }

        //[Column(TypeName = "money")]
        public DateTime EFFECTIVEDATE { get; set; }


        public DateTime MATURITYDATE { get; set; }

       // public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
