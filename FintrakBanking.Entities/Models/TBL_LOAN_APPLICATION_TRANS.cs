namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_TRANS")]
    public partial class TBL_LOAN_APPLICATION_TRANS
    {
        [Key]
        public short CUSTOMERTRANSACTIONID { get; set; }
        public short LOANAPPLICATIONID { get; set; }
        public short CUSTOMERID { get; set; }
        public string CUSTOMERCODE { get; set; }
        public string ACCOUNTNUMBER { get; set; }
        public string PERIOD { get; set; }
        public string PRODUCTNAME { get; set; }
        public decimal? MINIMUMDEBITBALANCE { get; set; }
        public decimal? MAXIMUMDEBITBALANCE { get; set; }
        public decimal? MINIMUMCREDITBALANCE { get; set; }
        public decimal? MAXIMUMCREDITBALANCE { get; set; }
        public decimal? DEBITTURNOVER { get; set; }
        public decimal? CREDITTURNOVER { get; set; }
        public string SMSALERT { get; set; }
        public decimal? AMC { get; set; }
        public decimal? VAT { get; set; }
        public decimal? MANAGEMENTFEE { get; set; }
        public decimal? COMMITMENTFEE { get; set; }
        public decimal? CONTINGENTLIABILITYCOMM { get; set; }
        public decimal? LC_COMMISSION { get; set; }
        public short CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
    }
}