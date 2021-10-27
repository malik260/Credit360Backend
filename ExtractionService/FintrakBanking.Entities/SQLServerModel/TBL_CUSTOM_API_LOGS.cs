namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("custom.TBL_CUSTOM_API_LOGS")]
    public partial class TBL_CUSTOM_API_LOGS
    {
        [Key]
        public int APILOGID { get; set; }

        [Required]
        //[StringLength(500)]
        public string APIURL { get; set; }

        public short LOGTYPEID { get; set; }

        [Required]
        //[StringLength(50)]
        public string REFERENCENUMBER { get; set; }

        [Required]
        public string REQUESTMESSAGE { get; set; }

        [Required]
        public string RESPONSEMESSAGE { get; set; }

        public DateTime REQUESTDATETIME { get; set; }

        public DateTime RESPONSEDATETIME { get; set; }
    }
}
