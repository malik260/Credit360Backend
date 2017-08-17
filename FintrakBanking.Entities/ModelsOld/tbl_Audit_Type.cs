namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_Audit_Type")]
    public partial class tbl_Audit_Type
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short AuditTypeId { get; set; }

        [StringLength(100)]
        public string AuditTypeName { get; set; }
    }
}
