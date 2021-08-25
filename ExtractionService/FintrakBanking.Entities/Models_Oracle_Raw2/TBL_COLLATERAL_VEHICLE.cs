namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_COLLATERAL_VEHICLE")]
    public partial class TBL_COLLATERAL_VEHICLE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int COLLATERALVEHICLEID { get; set; }

        public int COLLATERALCUSTOMERID { get; set; }

        [Required]
        [StringLength(100)]
        public string VEHICLETYPE { get; set; }

        [StringLength(10)]
        public string VEHICLESTATUS { get; set; }

        [Required]
        [StringLength(50)]
        public string VEHICLEMAKE { get; set; }

        [Required]
        [StringLength(50)]
        public string MODELNAME { get; set; }

        [Required]
        [StringLength(50)]
        public string REGISTRATIONNUMBER { get; set; }

        [StringLength(50)]
        public string SERIALNUMBER { get; set; }

        [Required]
        [StringLength(50)]
        public string CHASISNUMBER { get; set; }

        [Required]
        [StringLength(50)]
        public string ENGINENUMBER { get; set; }

        [Required]
        [StringLength(250)]
        public string NAMEOFOWNER { get; set; }

        [Required]
        [StringLength(250)]
        public string REGISTRATIONCOMPANY { get; set; }

        public decimal? RESALEVALUE { get; set; }

        public DateTime? VALUATIONDATE { get; set; }

        public decimal? LASTVALUATIONAMOUNT { get; set; }

        public decimal INVOICEVALUE { get; set; }

        [StringLength(500)]
        public string REMARK { get; set; }

        public DateTime MANUFACTUREDDATE { get; set; }

        public virtual TBL_COLLATERAL_CUSTOMER TBL_COLLATERAL_CUSTOMER { get; set; }
    }
}
