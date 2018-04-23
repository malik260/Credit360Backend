namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_STAFF_ACCOUNT_HISTORY")]
    public partial class TBL_STAFF_ACCOUNT_HISTORY
    {
        [Key]
        public int STAFFACCOUNTHISTORYID { get; set; }

        public int STAFFID { get; set; }

        [Column(TypeName = "date")]
        public DateTime STARTDATE { get; set; }

        [Column(TypeName = "date")]
        public DateTime ENDDATE { get; set; }

        public int NEWSTAFFID { get; set; }

        public short PRODUCTTYPEID { get; set; }

        public int TARGETID { get; set; }

        [Required]
        [StringLength(2000)]
        public string REASONFORCHANGE { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_PRODUCT_TYPE TBL_PRODUCT_TYPE { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }
    }
}
