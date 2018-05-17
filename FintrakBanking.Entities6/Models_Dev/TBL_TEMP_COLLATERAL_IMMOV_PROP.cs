namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_TEMP_COLLATERAL_IMMOV_PROP")]
    public partial class TBL_TEMP_COLLATERAL_IMMOV_PROP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TEMPCOLLATERALPROPERTYID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

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

        public double? LONGITUDE { get; set; }

        public double? LATITUDE { get; set; }

        public byte PERFECTIONSTATUSID { get; set; }

        [StringLength(200)]
        public string PERFECTIONSTATUSREASON { get; set; }

        public virtual TBL_CITY TBL_CITY { get; set; }

        public virtual TBL_COLLATERAL_PERFECTN_STAT TBL_COLLATERAL_PERFECTN_STAT { get; set; }
    }
}
