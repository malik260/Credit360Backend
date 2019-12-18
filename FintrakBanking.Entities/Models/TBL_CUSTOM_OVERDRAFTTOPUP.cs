namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOM_OVERDRAFTTOPUP")]
    public partial class TBL_CUSTOM_OVERDRAFTTOPUP
    {
        [Key]
        public int OVERDRAFTTOPUPID { get; set; }

        //[StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        //[StringLength(50)]
        public string SANCTIONREFERENCENUMBER { get; set; }

        //[StringLength(50)]
        public string SANCTIONLEVEL { get; set; }

        //[StringLength(50)]
        public string SANCTIONAUTHORIZER { get; set; }

        //[StringLength(50)]
        public string REVIEWEDDATE { get; set; }

        //[StringLength(50)]
        public string SANCTIONLIMIT { get; set; }

        //[StringLength(50)]
        public string EXPIRYDATE { get; set; }

        //[StringLength(50)]
        public string SANCTIONDATE { get; set; }

        //[StringLength(50)]
        public string APIURL { get; set; }

        //[StringLength(50)]
        public string APPLICATIONDATE { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public bool CONSUMED { get; set; }

        public DateTime? DATETIMECONSUMED { get; set; }
    }
}
