namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_TERM_SHEET")]
    public partial class TBL_TERM_SHEET
    {
        [Key]
        public int TERMSHEETID { get; set; }

        public int? CUSTOMERID { get; set; }

        public string TERMSHEETCODE { get; set; }

        [Required]
        //[StringLength(500)]
        public string BORROWER { get; set; }

        public decimal FACILITYAMOUNT { get; set; }

        public int FACILITYTYPE { get; set; }

        [Required]
        //[StringLength(1000)]
        public string PURPOSE { get; set; }

        public int TENOR { get; set; }

        [Required]
        //[StringLength(1000)]
        public string PERMITTEDACCOUNT { get; set; }

        [Required]
        //[StringLength(1000)]
        public string DEBTSERVICERESERVEACCOUNT { get; set; }

        [Required]
        //[StringLength(1000)]
        public string CANCELLATION { get; set; }

        [Required]
        //[StringLength(1000)]
        public string PRINCIPALREPAYMENT { get; set; }

        [Required]
        //[StringLength(1000)]
        public string INTERESTPAYMENT { get; set; }

        [Required]
        //[StringLength(1000)]
        public string COMPUTATIONOFINTEREST { get; set; }

        [Required]
        //[StringLength(1000)]
        public string REPAYMENTSOURCE { get; set; }

        [Required]
        //[StringLength(1000)]
        public string AVAILABILITY { get; set; }

        public int CURRENCYOFDISBURSEMENT { get; set; }

        [Required]
        //[StringLength(1000)]
        public string DOCUMENTATION { get; set; }

        [Required]
        //[StringLength(1000)]
        public string DRAWDOWN { get; set; }

        [Required]
        //[StringLength(1000)]
        public string EARLYREPAYMENTOFPRINCIPAL { get; set; }

        public decimal INTERESTRATE { get; set; }

        [Required]
        //[StringLength(1000)]
        public string PRICING { get; set; }

        public decimal MANAGEMENTFEES { get; set; }

        public decimal FACILITYFEE { get; set; }

        public decimal PROCESSINGFEE { get; set; }

        [Required]
        //[StringLength(1000)]
        public string SECURITYCONDITION { get; set; }

        [Required]
        //[StringLength(1000)]
        public string TRANSACTIONDYNAMICS { get; set; }

        [Required]
        //[StringLength(1000)]
        public string CONDITIONSPRECEDENTTOUTILISATION { get; set; }

        [Required]
        //[StringLength(1000)]
        public string OTHERCONDITION { get; set; }

        [Required]
        //[StringLength(1000)]
        public string TAXES { get; set; }

        [Required]
        //[StringLength(1000)]
        public string PRESENTATIONSANDWARRANTEES { get; set; }

        [Required]
        //[StringLength(1000)]
        public string COVENANTS { get; set; }

        [Required]
        //[StringLength(1000)]
        public string EVENTSOFDEFAULT { get; set; }

        [Required]
        //[StringLength(1000)]
        public string TRANSFERABILITY { get; set; }

        [Required]
        //[StringLength(1000)]
        public string GOVERNINGLAWANDJURISDICTION { get; set; }

        public bool DELETED { get; set; }

        //public string DESCRIPTION { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime? DATETIMECREATED { get; set; }

        public int LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public int DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

    }
}