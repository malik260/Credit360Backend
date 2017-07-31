namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Notification_Log")]
    public partial class tbl_Notification_Log
    {
        [Key]
        public long NotificationId { get; set; }

        public int StaffId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        [Required]
        [StringLength(50)]
        public string ActionUrl { get; set; }

        public bool IsActive { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }
    }
}
