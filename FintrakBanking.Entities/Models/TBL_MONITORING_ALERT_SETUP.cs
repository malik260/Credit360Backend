namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_MONITORING_ALERT_SETUP")]
    public partial class TBL_MONITORING_ALERT_SETUP
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MONITORING_ITEMID { get; set; }

        [Required]
        [StringLength(100)]
        public string MONITORING_ITEM_NAME { get; set; }

        [Required]
        [StringLength(5000)]
        public string MESSAGE_TEMPLATE { get; set; }

        public short MESSAGETYPEID { get; set; }

        [Required]
        [StringLength(200)]
        public string MESSAGE_TITLE { get; set; }

        public int NOTIFICATION_PERIOD1 { get; set; }

        [StringLength(500)]
        public string RECIPIENTEMAILS1 { get; set; }

        public int? NOTIFICATION_PERIOD2 { get; set; }

        [StringLength(500)]
        public string RECIPIENTEMAILS2 { get; set; }

        public int? NOTIFICATION_PERIOD3 { get; set; }

        [StringLength(500)]
        public string RECIPIENTEMAILS3 { get; set; }

        public virtual TBL_MESSAGE_LOG_TYPE TBL_MESSAGE_LOG_TYPE { get; set; }
    }
}
