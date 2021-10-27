namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_JOB_REQUEST_DETAIL")]
    public partial class TBL_JOB_REQUEST_DETAIL
    {
        [Key]
        public int JOBREQUEST_DETAILID { get; set; }

        public int JOBREQUESTID { get; set; }

        public short JOB_SUB_TYPEID { get; set; }

        public int? JOB_SUB_TYPE_CLASSID { get; set; }

        public int? ACCREDITEDCONSULTANTID { get; set; }

        //[StringLength(2000)]
        public string DESCRIPTION { get; set; }

        public string DESCRIPTION2 { get; set; }

        //[Column(TypeName = "money")]
        public decimal? AMOUNT { get; set; }

        //[StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        public bool ACCREDITEDCONSULTANTPAID { get; set; }

        public int? CURRENCYID { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public bool CUSTOMERORBUSINESSCHARGED { get; set; }

        public bool DEBITBUSINESS { get; set; }

        public bool? TRANSACTIONREVERSED { get; set; }
        public int? CUSTOMERCASAACCOUNTID { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_ACCREDITEDCONSULTANT TBL_ACCREDITEDCONSULTANT { get; set; }

        public virtual TBL_JOB_REQUEST TBL_JOB_REQUEST { get; set; }

        public virtual TBL_JOB_TYPE_SUB TBL_JOB_TYPE_SUB { get; set; }

        public virtual TBL_JOB_TYPE_SUB_CLASS TBL_JOB_TYPE_SUB_CLASS { get; set; }
    }
}
