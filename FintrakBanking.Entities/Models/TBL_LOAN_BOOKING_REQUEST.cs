namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_BOOKING_REQUEST")]
    public partial class TBL_LOAN_BOOKING_REQUEST
    {
        [Key]
        public int LOAN_BOOKING_REQUESTID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        //[Column(TypeName = "money")]
        public decimal AMOUNT_REQUESTED { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public bool? ISUSED { get; set; }
        public int? CUSTOMERID { get; set; }

        public int? CASAACCOUNTID { get; set; }

        public int? CASAACCOUNTID2 { get; set; }
        public short PRODUCTID { get; set; }
        //[StringLength(50)]
        public string CRMSCODE { get; set; }

        public int? OPERATIONID { get; set; }

        public bool? SECUREDBYCOLLATERAL { get; set; }
        public int? CRMSCOLLATERALTYPEID { get; set; }
        public int? MORATORIUMDURATION { get; set; }

        public int? CRMSFUNDINGSOURCEID { get; set; }

        public int? CRMSREPAYMENTSOURCEID { get; set; }
        //[StringLength(50)]
        public string CRMSFUNDINGSOURCECATEGORY { get; set; }
        //[StringLength(50)]
        public string CRMS_ECCI_NUMBER { get; set; }
        public short? CRMSREPAYMENTAGREEMENTID { get; set; }
        public bool? CRMSVALIDATED { get; set; }

        public DateTime? CRMSDATE { get; set; }
        public int? TENOR { get; set; }
        public short? APPROVEDLINESTATUSID { get; set; }

        public bool? ISMAINTAINEDLINE { get; set; }

        public bool? TAKEFEEONCE { get; set; }

        public bool ISDISBURSED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_STAFF TBL_STAFF { get; set; }

        public virtual TBL_LOAN_APPLICATION_DETAIL TBL_LOAN_APPLICATION_DETAIL { get; set; }
    }
}
