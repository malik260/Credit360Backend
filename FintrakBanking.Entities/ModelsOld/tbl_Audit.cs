namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Audit")]
    public partial class tbl_Audit
    {
        [Key]
        public long AuditId { get; set; }

        public short AuditTypeId { get; set; }

        public int StaffId { get; set; }

        public DateTime SystemDateTime { get; set; }

        [Column(TypeName = "date")]
        public DateTime ApplicationDate { get; set; }

        [Required]
        [StringLength(4000)]
        public string Detail { get; set; }

        public short BranchId { get; set; }

        [StringLength(100)]
        public string IPAddress { get; set; }

        [StringLength(300)]
        public string Url { get; set; }

        public int TargetId { get; set; }

        public virtual tbl_Branch tbl_Branch { get; set; }

        public virtual tbl_Staff tbl_Staff { get; set; }
    }
}
