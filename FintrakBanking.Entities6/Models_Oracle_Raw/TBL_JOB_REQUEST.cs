namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("FINTRAKBANKING.TBL_JOB_REQUEST")]
    public partial class TBL_JOB_REQUEST
    {
        public TBL_JOB_REQUEST()
        {
            TBL_JOB_REQUEST_DETAIL = new HashSet<TBL_JOB_REQUEST_DETAIL>();
            TBL_JOB_REQUEST_DOCUMENT_MAPPN = new HashSet<TBL_JOB_REQUEST_DOCUMENT_MAPPN>();
            TBL_JOB_REQUEST_MESSAGE = new HashSet<TBL_JOB_REQUEST_MESSAGE>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int JOBREQUESTID { get; set; }

        [Required]
        [StringLength(50)]
        public string JOBREQUESTCODE { get; set; }

        public int JOBTYPEID { get; set; }

        public int SENDERSTAFFID { get; set; }

        public int? RECEIVERSTAFFID { get; set; }

        public int? REASSIGNEDTO { get; set; }

        public int ISREASSIGNED { get; set; }

        public int ISACKNOWLEDGED { get; set; }

        public int TARGETID { get; set; }

        public int DEPARTMENTID { get; set; }

        public int DEPARTMENTUNITID { get; set; }

        public int OPERATIONSID { get; set; }

        public int REQUESTSTATUSID { get; set; }

        public int? JOB_STATUS_FEEDBACKID { get; set; }

        [StringLength(400)]
        public string JOB_TITLE { get; set; }

        [StringLength(2000)]
        public string SENDERCOMMENT { get; set; }

        [StringLength(2000)]
        public string RESPONSECOMMENT { get; set; }

        public DateTime SYSTEMARRIVALDATE { get; set; }

        public DateTime? SYSTEMREASSIGNEDDATE { get; set; }

        public DateTime? SYSTEMRESPONSEDATE { get; set; }

        public DateTime? SYSTEMACKNOWLEDGEMENTDATE { get; set; }

        public DateTime? ACKNOWLEDGEMENTDATE { get; set; }

        public DateTime? RESPONSEDATE { get; set; }

        public DateTime? REASSIGNEDDATE { get; set; }

        public DateTime? ARRIVALDATE { get; set; }

        public virtual TBL_DEPARTMENT TBL_DEPARTMENT { get; set; }

        public virtual TBL_DEPARTMENT_UNIT TBL_DEPARTMENT_UNIT { get; set; }

        public virtual ICollection<TBL_JOB_REQUEST_DETAIL> TBL_JOB_REQUEST_DETAIL { get; set; }

        public virtual ICollection<TBL_JOB_REQUEST_DOCUMENT_MAPPN> TBL_JOB_REQUEST_DOCUMENT_MAPPN { get; set; }

        public virtual ICollection<TBL_JOB_REQUEST_MESSAGE> TBL_JOB_REQUEST_MESSAGE { get; set; }

        public virtual TBL_JOB_REQUEST_STATUS_FEEDBAK TBL_JOB_REQUEST_STATUS_FEEDBAK { get; set; }

        public virtual TBL_JOB_REQUEST_STATUS TBL_JOB_REQUEST_STATUS { get; set; }

        public virtual TBL_JOB_TYPE TBL_JOB_TYPE { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }
    }
}
