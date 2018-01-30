namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_COLLATERAL_STOCK")]
    public partial class TBL_TEMP_COLLATERAL_STOCK
    {
        public int? COLLATERALSTOCKID { get; set; }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COLLATERALCUSTOMERID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short COLLATERALSUBTYPEID { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(250)]
        public string COMPANYNAME { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SHAREQUANTITY { get; set; }

        [Key]
        [Column(Order = 4, TypeName = "money")]
        public decimal MARKETPRICE { get; set; }

        [Key]
        [Column(Order = 5, TypeName = "money")]
        public decimal AMOUNT { get; set; }

        [Key]
        [Column(Order = 6, TypeName = "money")]
        public decimal SHARESSECURITYVALUE { get; set; }

        [Key]
        [Column(Order = 7, TypeName = "money")]
        public decimal SHAREVALUEAMOUNTTOUSE { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
