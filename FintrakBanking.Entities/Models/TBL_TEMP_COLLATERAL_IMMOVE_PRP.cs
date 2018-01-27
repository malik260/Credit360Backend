namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_COLLATERAL_IMMOVE_PRP")]
    public partial class TBL_TEMP_COLLATERAL_IMMOVE_PRP
    {
        [Key]
        public int COLLATERALPROPERTYID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        public short COLLATERALSUBTYPEID { get; set; }

        [Required]
        [StringLength(50)]
        public string PROPERTYNAME { get; set; }

        public int CITYID { get; set; }

        public short COUNTRYID { get; set; }

        public DateTime? CONSTRUCTIONDATE { get; set; }

        [Required]
        [StringLength(500)]
        public string PROPERTYADDRESS { get; set; }

        public DateTime DATEOFACQUISITION { get; set; }

        public DateTime LASTVALUATIONDATE { get; set; }

        public short? VALUERID { get; set; }

        [StringLength(100)]
        public string VALUERREFERENCENUMBER { get; set; }

        public short PROPERTYVALUEBASETYPEID { get; set; }

        [Column(TypeName = "money")]
        public decimal? OPENMARKETVALUE { get; set; }

        [Column(TypeName = "money")]
        public decimal COLLATERALVALUE { get; set; }

        [Column(TypeName = "money")]
        public decimal? FORCEDSALEVALUE { get; set; }

        [StringLength(10)]
        public string STAMPTOCOVER { get; set; }

        [StringLength(50)]
        public string VALUATIONSOURCE { get; set; }

        [Column(TypeName = "money")]
        public decimal ORIGINALVALUE { get; set; }

        [Column(TypeName = "money")]
        public decimal AVAILABLEVALUE { get; set; }

        [Column(TypeName = "money")]
        public decimal? SECURITYVALUE { get; set; }

        [Column(TypeName = "money")]
        public decimal? COLLATERALUSABLEAMOUNT { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        [StringLength(100)]
        public string NEARESTLANDMARK { get; set; }

        [StringLength(100)]
        public string NEARESTBUSSTOP { get; set; }

        public decimal? LONGITUDE { get; set; }

        public decimal? LATITUDE { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_COLLATERAL_TYPE_SUB TBL_COLLATERAL_TYPE_SUB { get; set; }

        public virtual TBL_TEMP_COLLATERAL_CUSTOMER TBL_TEMP_COLLATERAL_CUSTOMER { get; set; }
    }
}
