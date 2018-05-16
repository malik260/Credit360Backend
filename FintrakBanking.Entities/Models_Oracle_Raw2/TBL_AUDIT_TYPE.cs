namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_AUDIT_TYPE")]
    public partial class TBL_AUDIT_TYPE
    {
        public TBL_AUDIT_TYPE()
        {
            TBL_AUDIT = new HashSet<TBL_AUDIT>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int AUDITTYPEID { get; set; }

        [StringLength(100)]
        public string AUDITTYPENAME { get; set; }

        public virtual ICollection<TBL_AUDIT> TBL_AUDIT { get; set; }
    }
}
