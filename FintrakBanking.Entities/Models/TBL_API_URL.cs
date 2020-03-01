namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_API_URL")]
    public partial class TBL_API_URL
    {
        [Key]
        public int APIURLID { get; set; }

        public string TYPENAME { get; set; }
        public string URL { get; set; }
        public string APIKEY { get; set; }
        public string SOURCE { get; set; }
        public string USERID { get; set; }

        //public int CREATEDBY { get; set; }

        //public DateTime DATETIMECREATED { get; set; }

        //public int? LASTUPDATEDBY { get; set; }

        //public DateTime? DATETIMEUPDATED { get; set; }

    }
}
