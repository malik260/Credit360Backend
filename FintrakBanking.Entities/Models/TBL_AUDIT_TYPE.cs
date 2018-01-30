namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_AUDIT_TYPE")]
    public partial class TBL_AUDIT_TYPE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short AUDITTYPEID { get; set; }

        [StringLength(100)]
        public string AUDITTYPENAME { get; set; }
    }
}
