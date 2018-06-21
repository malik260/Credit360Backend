namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_CUSTOM_API_LOGS")]
    public partial class TBL_CUSTOM_API_LOGS
    {
        [Key]
        public int APILOGID { get; set; }

        public string APIURL { get; set; }

        public int LOGTYPEID { get; set; }

        public DateTime REQUESTDATETIME { get; set; }

        public DateTime RESPONSEDATETIME { get; set; }

        public string REQUESTMESSAGE { get; set; }

        public string RESPONSEMESSAGE { get; set; }

        public string REFERENCENUMBER { get; set; }
    }
}
