namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_OVERRIDE_DETAIL")]
    public partial class TBL_OVERRIDE_DETAIL
    {
        [Key]
        public int OVERRIDE_DETAILID { get; set; }

        [Required]
        //[StringLength(50)]
        public string CUSTOMERCODE { get; set; }

        public short OVERRIDE_ITEMID { get; set; }

        public bool ISUSED { get; set; }

        public short APPROVALSTATUSID { get; set; }

        //[StringLength(50)]
        public string SOURCE_REFERENCE_NUMBER { get; set; }

        [Required]
        //[StringLength(2000)]
        public string REASON { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUSED { get; set; }

        public int? USEDBY { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_OVERRIDE_ITEM TBL_OVERRIDE_ITEM { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}
