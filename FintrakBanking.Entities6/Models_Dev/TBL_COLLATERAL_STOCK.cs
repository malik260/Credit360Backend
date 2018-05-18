namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COLLATERAL_STOCK")]
    public partial class TBL_COLLATERAL_STOCK
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COLLATERALSTOCKID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(250)]
        public string COMPANYNAME { get; set; }

        public int SHAREQUANTITY { get; set; }

        public decimal MARKETPRICE { get; set; }

        public decimal AMOUNT { get; set; }

        public decimal SHARESSECURITYVALUE { get; set; }

        public decimal SHAREVALUEAMOUNTTOUSE { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
