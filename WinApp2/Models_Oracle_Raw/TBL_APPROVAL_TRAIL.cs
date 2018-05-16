namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_APPROVAL_TRAIL")]
    public partial class TBL_APPROVAL_TRAIL
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int APPROVALTRAILID { get; set; }

        public int TARGETID { get; set; }

        public DateTime SYSTEMARRIVALDATETIME { get; set; }

        public DateTime? SYSTEMRESPONSEDATETIME { get; set; }

        public int REQUESTSTAFFID { get; set; }

        public int? RESPONSESTAFFID { get; set; }

        public int? TOSTAFFID { get; set; }

        public int? RELIEVEDSTAFFID { get; set; }

        public int COMPANYID { get; set; }

        public int? FROMAPPROVALLEVELID { get; set; }

        public int? TOAPPROVALLEVELID { get; set; }

        public int APPROVALSTATEID { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int OPERATIONID { get; set; }

        [StringLength(700)]
        public string COMMENT_ { get; set; }

        public int? VOTE { get; set; }

        public DateTime? ARRIVALDATE { get; set; }

        public DateTime? RESPONSEDATE { get; set; }

        public virtual TBL_APPROVAL_LEVEL TBL_APPROVAL_LEVEL { get; set; }

        public virtual TBL_APPROVAL_STATE TBL_APPROVAL_STATE { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_APPROVAL_VOTE_OPTION TBL_APPROVAL_VOTE_OPTION { get; set; }

        public virtual TBL_COMPANY TBL_COMPANY { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}
