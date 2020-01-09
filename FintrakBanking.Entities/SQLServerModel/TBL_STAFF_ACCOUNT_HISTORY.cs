namespace FintrakBanking.Entities.SQLServerModel
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

        public short ACCOUNTYPEID { get; set; }

        public int TARGETID { get; set; }

        [Required]
        //[StringLength(2000)]
        public string REASONFORCHANGE { get; set; }

        public short APPROVALSTATUSID { get; set; }

        //[StringLength(200)]
        public string FIELD1 { get; set; }

        //[StringLength(200)]
        public string FIELD2 { get; set; }

        //[StringLength(200)]
        public string FIELD3 { get; set; }

        //[StringLength(200)]
        public string FIELD4 { get; set; }

        //[StringLength(200)]
        public string FIELD5 { get; set; }

        //[StringLength(200)]
        public string FIELD6 { get; set; }

        //[StringLength(200)]
        public string FIELD7 { get; set; }

        //[StringLength(200)]
        public string FIELD8 { get; set; }

        //[StringLength(200)]
        public string FIELD9 { get; set; }

        //[StringLength(200)]
        public string FIELD10 { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }

        public virtual TBL_STAFF_ACCOUNT_HISTORY_TYPE TBL_STAFF_ACCOUNT_HISTORY_TYPE { get; set; }
    }
}
