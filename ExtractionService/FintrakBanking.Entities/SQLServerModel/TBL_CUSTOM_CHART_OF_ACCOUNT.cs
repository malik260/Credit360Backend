namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("custom.TBL_CUSTOM_CHART_OF_ACCOUNT")]
    public partial class TBL_CUSTOM_CHART_OF_ACCOUNT
    {
        [Key]
        public int CUSTOMACCOUNTID { get; set; }

        [Required]
        //[StringLength(50)]
        public string ACCOUNTID { get; set; }

        [Required]
        //[StringLength(400)]
        public string ACCOUNTNAME { get; set; }

        [Required]
        //[StringLength(10)]
        public string CURRENCYCODE { get; set; }

        [Required]
        //[StringLength(200)]
        public string PLACEHOLDERID { get; set; }
    }
}
