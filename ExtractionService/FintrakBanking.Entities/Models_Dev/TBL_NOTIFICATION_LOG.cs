namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_NOTIFICATION_LOG")]
    public partial class TBL_NOTIFICATION_LOG
    {
        [Key]
        public long NOTIFICATIONID { get; set; }

        public int STAFFID { get; set; }

        [Required]
        [StringLength(1000)]
        public string MESSAGE { get; set; }

        [Required]
        [StringLength(50)]
        public string ACTIONURL { get; set; }

        public bool ISACTIVE { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}
