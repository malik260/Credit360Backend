using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_NMRC_LOAN_SCHEDULE_PERIODIC")]

    public class TBL_NMRC_LOAN_SCHEDULE_PERIODIC
    {
        [Key]
        public int PERIODICSCHEDULEID { get; set; }

        public int LOANID { get; set; }

        public int PAYMENTNUMBER { get; set; }

        //[Column(TypeName = "date")]
        public DateTime PAYMENTDATE { get; set; }

        //[Column(TypeName = "money")]
        public decimal STARTPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal PERIODPAYMENTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal PERIODINTERESTAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal PERIODPRINCIPALAMOUNT { get; set; }

        //[Column(TypeName = "money")]
        public decimal ENDPRINCIPALAMOUNT { get; set; }

        public double INTERESTRATE { get; set; }

        public double? PREVIOUSINTERESTRATE { get; set; }

        public decimal PREVIOUSINTERESTAMOUNT { get; set; }

        public decimal PREVIOUSPRINCIPALAMOUNT { get; set; }

        public decimal AMORTISEDSTARTPRINCIPALAMOUNT { get; set; }

        public decimal AMORTISEDPERIODPAYMENTAMOUNT { get; set; }

        public decimal AMORTISEDPERIODINTERESTAMOUNT { get; set; }

        public decimal AMORTISEDPERIODPRINCIPALAMOUNT { get; set; }

        public decimal AMORTISEDENDPRINCIPALAMOUNT { get; set; }

        public double EFFECTIVEINTERESTRATE { get; set; }

        public double? PREVIOUSEFFECTIVEINTERESTRATE { get; set; }

        public int CREATEDBY { get; set; }
        public int? EXTERNALLOANID { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

    }
}
