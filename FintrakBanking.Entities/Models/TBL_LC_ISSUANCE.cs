namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LC_ISSUANCE")]
    public partial class TBL_LC_ISSUANCE
    {
        public TBL_LC_ISSUANCE()
        {
            TBL_LC_DOCUMENT = new HashSet<TBL_LC_DOCUMENT>();
            TBL_LC_CONDITION = new HashSet<TBL_LC_CONDITION>();
            TBL_LC_SHIPPING = new HashSet<TBL_LC_SHIPPING>();
            TBL_LC_USSANCE = new HashSet<TBL_LC_USSANCE>();

        }

        [Key]
        public int LCISSUANCEID { get; set; }

        public string LCREFERENCENUMBER { get; set; }

        [Required]
        public string BENEFICIARYNAME { get; set; }

        public decimal TOTALAPPROVEDAMOUNT { get; set; }

        public int LETTEROFCREDITTYPEID { get; set; }

        public bool? ISDRAFTREQUIRED { get; set; }

        [Required]
        public string BENEFICIARYADDRESS { get; set; }

        [Required]
        public string BENEFICIARYEMAIL { get; set; }

        public int CUSTOMERID { get; set; }

        public int FUNDSOURCEID { get; set; }

        public int FUNDSOURCEDETAILS { get; set; }

        public string FORMMNUMBER { get; set; }

        public string BENEFICIARYPHONENUMBER { get; set; }

        public string BENEFICIARYBANK { get; set; }

        public int CURRENCYID { get; set; }

        public string PROFORMAINVOICEID { get; set; }

        public decimal AVAILABLEAMOUNT { get; set; }
        public decimal LETTEROFCREDITAMOUNT { get; set; }

        public DateTime LETTEROFCREDITEXPIRYDATE { get; set; }

        public DateTime INVOICEDATE { get; set; }

        public DateTime INVOICEDUEDATE { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public int? CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public int? APPROVEDBY { get; set; }

        public bool? APPROVED { get; set; }

        public int? APPROVALSTATUSID { get; set; }

        public short? LCUSSANCESTATUSID { get; set; }

        public short? LCUSSANCEAPPROVALSTATUSID { get; set; }

        public short? APPLICATIONSTATUSID { get; set; }

        public int? FINALAPPROVAL_LEVELID { get; set; }

        public int? LCUSSANCEFINALAPPROVAL_LEVELID { get; set; }

        public DateTime? LCUSSANCEAPPROVEDDATE { get; set; }

        public DateTime? DATEACTEDON { get; set; }

        public int? ACTEDONBY { get; set; }

        public DateTime? APPROVEDDATE { get; set; }


        public int? TOTALAPPROVEDAMOUNTCURRENCYID { get; set; }

        public int? AVAILABLEAMOUNTCURRENCYID { get; set; }

        public bool? CASHBUILDUPAVAILABLE { get; set; }

        public string CASHBUILDUPREFERENCETYPE { get; set; }

        public string CASHBUILDUPREFERENCENUMBER { get; set; }

        public decimal? PERCENTAGETOCOVER { get; set; }

        public decimal? LCTOLERANCEPERCENTAGE { get; set; }

        public decimal? LCTOLERANCEVALUE { get; set; }
        public decimal? TOTALUSANCEAMOUNTLOCAL { get; set; }

        public decimal RELEASEDAMOUNT { get; set; }
        public int TRANSACTIONCYCLE { get; set; }


        public int? OPERATIONID { get; set; }

        public virtual ICollection<TBL_LC_DOCUMENT> TBL_LC_DOCUMENT { get; set; }

        public virtual ICollection<TBL_LC_CONDITION> TBL_LC_CONDITION { get; set; }

        public virtual ICollection<TBL_LC_SHIPPING> TBL_LC_SHIPPING { get; set; }

        public virtual ICollection<TBL_LC_USSANCE> TBL_LC_USSANCE { get; set; }

        public virtual TBL_CUSTOMER TBL_CUSTOMER { get; set; }

    }
}