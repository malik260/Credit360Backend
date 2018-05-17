namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_MESSAGE_LOG")]
    public partial class TBL_MESSAGE_LOG
    {
        [Key]
        public int MESSAGEID { get; set; }

        [Required]
        [StringLength(1000)]
        public string FROMADDRESS { get; set; }

        [Required]
        [StringLength(1000)]
        public string TOADDRESS { get; set; }

        [Required]
        public string MESSAGEBODY { get; set; }

        [Required]
        [StringLength(1000)]
        public string MESSAGESUBJECT { get; set; }

        public short MESSAGESTATUSID { get; set; }

        public short MESSAGETYPEID { get; set; }

        public DateTime DATETIMERECEIVED { get; set; }

        public DateTime SENDONDATETIME { get; set; }

        public DateTime? DATETIMESENT { get; set; }

        [StringLength(2000)]
        public string GATEWAYRESPONSE { get; set; }

        public int? OPERATIONID { get; set; }

        public int? TARGETID { get; set; }

        public virtual TBL_MESSAGE_LOG_STATUS TBL_MESSAGE_LOG_STATUS { get; set; }

        public virtual TBL_MESSAGE_LOG_TYPE TBL_MESSAGE_LOG_TYPE { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }
    }
}
