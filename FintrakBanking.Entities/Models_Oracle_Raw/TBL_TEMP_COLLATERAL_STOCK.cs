namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_COLLATERAL_STOCK")]
    public partial class TBL_TEMP_COLLATERAL_STOCK
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALSTOCKID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(250)]
        public string COMPANYNAME { get; set; }

        public int SHAREQUANTITY { get; set; }

        public decimal MARKETPRICE { get; set; }

        public decimal AMOUNT { get; set; }

        public decimal SHARESSECURITYVALUE { get; set; }

        public decimal SHAREVALUEAMOUNTTOUSE { get; set; }
    }
}
