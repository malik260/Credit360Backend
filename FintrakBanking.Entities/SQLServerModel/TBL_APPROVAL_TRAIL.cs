namespace FintrakBanking.Entities.SQLServerModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.TBL_APPROVAL_TRAIL")]
    public partial class TBL_APPROVAL_TRAIL
    {
        [Key]
        public int APPROVALTRAILID { get; set; }

        public int TARGETID { get; set; }

        [Column(TypeName = "date")]
        public DateTime ARRIVALDATE { get; set; }

        public DateTime SYSTEMARRIVALDATETIME { get; set; }

        [Column(TypeName = "date")]
        public DateTime? RESPONSEDATE { get; set; }

        public DateTime? SYSTEMRESPONSEDATETIME { get; set; }

        public DateTime? SLADATETIME { get; set; }

        public int REQUESTSTAFFID { get; set; }

        public int? RESPONSESTAFFID { get; set; }

        public int? TOSTAFFID { get; set; }

        public int? RELIEVEDSTAFFID { get; set; }

        public int COMPANYID { get; set; }

        public int? FROMAPPROVALLEVELID { get; set; }

        public int? TOAPPROVALLEVELID { get; set; }

        public short APPROVALSTATEID { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public int OPERATIONID { get; set; }

        //[StringLength(700)]
        public string COMMENT { get; set; }

        public short? VOTE { get; set; }

        public virtual TBL_APPROVAL_LEVEL TBL_APPROVAL_LEVEL { get; set; }

        public virtual TBL_APPROVAL_LEVEL TBL_APPROVAL_LEVEL1 { get; set; }

        public virtual TBL_APPROVAL_STATE TBL_APPROVAL_STATE { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_APPROVAL_VOTE_OPTION TBL_APPROVAL_VOTE_OPTION { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }

        public virtual TBL_STAFF TBL_STAFF2 { get; set; }

        public virtual TBL_STAFF TBL_STAFF3 { get; set; }
    }
}
