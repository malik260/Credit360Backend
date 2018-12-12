
namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TBL_LOAN_APPLICATION_TRANS2")]
    public partial class TBL_LOAN_APPLICATION_TRANS2
    {

        [Key]
        public int CUSTOMERTRANSACTIONID2 { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public int CUSTOMERID { get; set; }
        public string CUSTOMERCODE { get; set; }
        public string ACCOUNTNUMBER { get; set; }
        public string PERIOD { get; set; }
        public string PRODUCTNAME { get; set; }
        public decimal? FLOATCHARGE { get; set; }
        public decimal? INTEREST { get; set; }
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int? MONTH { get; set; }
        public int? YEAR { get; set; }
        public bool ISLMS { get; set; }

    }
}
