namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("temp.TBL_TEMP_COLLATERAL_VEHICLE")]
    public partial class TBL_TEMP_COLLATERAL_VEHICLE
    {
        [Key]
        public int TEMPCOLLATERALVEHICLEID { get; set; }

        public int TEMPCOLLATERALCUSTOMERID { get; set; }

        [Required]
        //[StringLength(100)]
        public string VEHICLETYPE { get; set; }

        //[StringLength(10)]
        public string VEHICLESTATUS { get; set; }

        [Required]
        //[StringLength(50)]
        public string VEHICLEMAKE { get; set; }

        [Required]
        //[StringLength(50)]
        public string MODELNAME { get; set; }

        [Required]
        //[StringLength(50)]
        public string MANUFACTUREDDATE { get; set; }

        [Required]
        //[StringLength(50)]
        public string REGISTRATIONNUMBER { get; set; }

        //[StringLength(50)]
        public string SERIALNUMBER { get; set; }

        [Required]
        //[StringLength(50)]
        public string CHASISNUMBER { get; set; }

        [Required]
        //[StringLength(50)]
        public string ENGINENUMBER { get; set; }

        [Required]
        //[StringLength(250)]
        public string NAMEOFOWNER { get; set; }

        [Required]
        //[StringLength(250)]
        public string REGISTRATIONCOMPANY { get; set; }

        [Column(TypeName = "money")]
        public decimal? RESALEVALUE { get; set; }

        public DateTime? VALUATIONDATE { get; set; }

        [Column(TypeName = "money")]
        public decimal? LASTVALUATIONAMOUNT { get; set; }

        [Column(TypeName = "money")]
        public decimal INVOICEVALUE { get; set; }

        //[StringLength(500)]
        public string REMARK { get; set; }
    }
}
