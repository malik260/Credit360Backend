namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_MONITORING_SETUP")]
    public partial class TBL_MONITORING_SETUP
    {
        [Key]
        public int MONITORING_ITEMID { get; set; }

        [Required]
        [StringLength(100)]
        public string MONITORING_ITEM_NAME { get; set; }

        [Required]
        [StringLength(5000)]
        public string MESSAGE_TEMPLATE { get; set; }

        public short MESSAGETYPEID { get; set; }

        public int NOTIFICATION_PERIOD { get; set; }

        public short PRODUCTID { get; set; }

        public virtual TBL_MESSAGE_LOG_TYPE TBL_MESSAGE_LOG_TYPE { get; set; }

        public virtual TBL_PRODUCT TBL_PRODUCT { get; set; }
    }
}
