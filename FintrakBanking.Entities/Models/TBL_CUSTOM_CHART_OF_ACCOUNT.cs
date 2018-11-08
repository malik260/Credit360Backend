namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOM_CHART_OF_ACCOUNT")]
    public partial class TBL_CUSTOM_CHART_OF_ACCOUNT
    {
        [Key]
        public int CUSTOMACCOUNTID { get; set; }

        public string ACCOUNTID { get; set; }

        public string ACCOUNTNAME { get; set; }

        public string CURRENCYCODE { get; set; }

        public string PLACEHOLDERID { get; set; }

        public bool ISNOSTROACCOUNT { get; set; }

        public bool ISBRANCHSPECIFIC { get; set; }

    }
}
