namespace FintrakBaking.BranchUpdateService.Fintrak_Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_APPROVAL_STATE")]
    public partial class TBL_APPROVAL_STATE
    {
        [Key]
        public short APPROVALSTATEID { get; set; }

        [StringLength(50)]
        public string APPROVALSTATE { get; set; }
    }
}
