namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COLLATERAL_IMMOVE_PROPERTY")]
    public partial class TBL_COLLATERAL_IMMOVE_PROPERTY
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COLLATERALPROPERTYID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(50)]
        public string PROPERTYNAME { get; set; }

        public int CITYID { get; set; }

        public int COUNTRYID { get; set; }

        public DateTime? CONSTRUCTIONDATE { get; set; }

        [Required]
        [StringLength(500)]
        public string PROPERTYADDRESS { get; set; }

        public DateTime DATEOFACQUISITION { get; set; }

        public DateTime LASTVALUATIONDATE { get; set; }

        public int? VALUERID { get; set; }

        [StringLength(100)]
        public string VALUERREFERENCENUMBER { get; set; }

        public int PROPERTYVALUEBASETYPEID { get; set; }

        public decimal? OPENMARKETVALUE { get; set; }

        public decimal? FORCEDSALEVALUE { get; set; }

        [StringLength(10)]
        public string STAMPTOCOVER { get; set; }

        public decimal? SECURITYVALUE { get; set; }

        public decimal? COLLATERALUSABLEAMOUNT { get; set; }

        public decimal? VALUATIONAMOUNT { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        [StringLength(250)]
        public string NEARESTLANDMARK { get; set; }

        [StringLength(250)]
        public string NEARESTBUSSTOP { get; set; }

        public decimal? LONGITUDE { get; set; }

        public decimal? LATITUDE { get; set; }

        public int PERFECTIONSTATUSID { get; set; }

        [StringLength(200)]
        public string PERFECTIONSTATUSREASON { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }

        public virtual TBL_COLLATERAL_PERFECTN_STAT TBL_COLLATERAL_PERFECTN_STAT { get; set; }
    }
}
