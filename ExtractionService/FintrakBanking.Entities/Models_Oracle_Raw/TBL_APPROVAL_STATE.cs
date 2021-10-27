namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_APPROVAL_STATE")]
    public partial class TBL_APPROVAL_STATE
    {
        public TBL_APPROVAL_STATE()
        {
            TBL_APPROVAL_TRAIL = new HashSet<TBL_APPROVAL_TRAIL>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPROVALSTATEID { get; set; }

        [StringLength(50)]
        public string APPROVALSTATE { get; set; }

        public virtual ICollection<TBL_APPROVAL_TRAIL> TBL_APPROVAL_TRAIL { get; set; }
    }
}
