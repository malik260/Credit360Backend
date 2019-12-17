namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_CONTINGENT_ARCHIVE")]
    public partial class TBL_LOAN_CONTINGENT_ARCHIVE
    {
        [Key]
        public int CONTINGENTLOANARCHIVEID { get; set; }

        public DateTime ARCHIVEDATE { get; set; }

        //[StringLength(50)]
        public string ARCHIVEBATCHCODE { get; set; }

        public int CONTINGENTLOANID { get; set; }

        public short LOANSYSTEMTYPEID { get; set; }

        public int CUSTOMERID { get; set; }

        public short PRODUCTID { get; set; }

        public int COMPANYID { get; set; }

        public int CASAACCOUNTID { get; set; }

        public short BRANCHID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }

        public bool ISTENORED { get; set; }

        public bool ISBANKFORMAT { get; set; }

        public short CURRENCYID { get; set; }

        public double EXCHANGERATE { get; set; }

        [Required]
        //[StringLength(50)]
        public string LOANREFERENCENUMBER { get; set; }

        //[StringLength(50)]
        public string RELATED_LOAN_REFERENCE_NUMBER { get; set; }

        public short SUBSECTORID { get; set; }

        public int RELATIONSHIPOFFICERID { get; set; }

        public int RELATIONSHIPMANAGERID { get; set; }

        //[StringLength(50)]
        public string MISCODE { get; set; }

        //[StringLength(50)]
        public string TEAMMISCODE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime EFFECTIVEDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime MATURITYDATE { get; set; }

        //[Column(TypeName = "date")]
        public DateTime BOOKINGDATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal CONTINGENTAMOUNT { get; set; }

        public int APPROVALSTATUSID { get; set; }

        public int? APPROVEDBY { get; set; }

        //[StringLength(500)]
        public string APPROVERCOMMENT { get; set; }

        public DateTime? DATEAPPROVED { get; set; }

        public short LOANSTATUSID { get; set; }

        public bool ISDISBURSED { get; set; }

        //[StringLength(50)]
        public string DISBURSEDBY { get; set; }

        //[StringLength(500)]
        public string DISBURSERCOMMENT { get; set; }

        //[Column(TypeName = "date")]
        public DateTime? DISBURSEDATE { get; set; }

        public int? OPERATIONID { get; set; }

        public bool DISCHARGELETTER { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

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
        //[StringLength(50)]
        public string CRMSCODE { get; set; }
        public short? CRMSREPAYMENTAGREEMENTID { get; set; }

        public string LEGALCONTINGENTCODE { get; set; }

        public DateTime? CRMSDATE { get; set; }

        public int? LOAN_BOOKING_REQUESTID { get; set; }

    }
}
