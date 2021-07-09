namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_JOB_REQUEST")]
    public partial class TBL_JOB_REQUEST
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TBL_JOB_REQUEST()
        {
            TBL_JOB_REQUEST_DETAIL = new HashSet<TBL_JOB_REQUEST_DETAIL>();
            TBL_JOB_REQUEST_DOCUMENT_MAPPN = new HashSet<TBL_JOB_REQUEST_DOCUMENT_MAPPN>();
            TBL_JOB_REQUEST_MESSAGE = new HashSet<TBL_JOB_REQUEST_MESSAGE>();
        }

        [Key]
        public int JOBREQUESTID { get; set; }

        [Required]
        [StringLength(50)]
        public string JOBREQUESTCODE { get; set; }

        public short JOBTYPEID { get; set; }

        public int SENDERSTAFFID { get; set; }

        public int? RECEIVERSTAFFID { get; set; }

        public int? REASSIGNEDTO { get; set; }

        public bool ISREASSIGNED { get; set; }

        public bool ISACKNOWLEDGED { get; set; }

        public int TARGETID { get; set; }

        public short DEPARTMENTID { get; set; }

        public short DEPARTMENTUNITID { get; set; }

        public int OPERATIONSID { get; set; }

        public short REQUESTSTATUSID { get; set; }

        public short? JOB_STATUS_FEEDBACKID { get; set; }

        [StringLength(400)]
        public string JOB_TITLE { get; set; }

        [StringLength(2000)]
        public string SENDERCOMMENT { get; set; }

        [StringLength(2000)]
        public string RESPONSECOMMENT { get; set; }

        //[Column(TypeName = "date")]
        public DateTime ARRIVALDATE { get; set; }

        public DateTime SYSTEMARRIVALDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? REASSIGNEDDATE { get; set; }

        public DateTime? SYSTEMREASSIGNEDDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? RESPONSEDATE { get; set; }

        public DateTime? SYSTEMRESPONSEDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? ACKNOWLEDGEMENTDATE { get; set; }

        public DateTime? SYSTEMACKNOWLEDGEMENTDATE { get; set; }

        public virtual TBL_DEPARTMENT TBL_DEPARTMENT { get; set; }

        public virtual TBL_DEPARTMENT_UNIT TBL_DEPARTMENT_UNIT { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST_DETAIL> TBL_JOB_REQUEST_DETAIL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST_DOCUMENT_MAPPN> TBL_JOB_REQUEST_DOCUMENT_MAPPN { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TBL_JOB_REQUEST_MESSAGE> TBL_JOB_REQUEST_MESSAGE { get; set; }

        public virtual TBL_JOB_REQUEST_STATUS TBL_JOB_REQUEST_STATUS { get; set; }

        public virtual TBL_JOB_REQUEST_STATUS_FEEDBAK TBL_JOB_REQUEST_STATUS_FEEDBAK { get; set; }

        public virtual TBL_JOB_TYPE TBL_JOB_TYPE { get; set; }

        public virtual TBL_OPERATIONS TBL_OPERATIONS { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_STAFF TBL_STAFF1 { get; set; }

        public virtual TBL_STAFF TBL_STAFF2 { get; set; }
    }
}
