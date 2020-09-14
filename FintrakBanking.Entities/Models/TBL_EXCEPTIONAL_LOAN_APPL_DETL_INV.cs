using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_EXCEPTIONAL_LOAN_APPL_DETL_INV")]
    public class TBL_EXCEPTIONAL_LOAN_APPL_DETL_INV
    {
        [Key]
        public int INVOICEID { get; set; }

        public int EXCEPTIONALLOANAPPLDETAILID { get; set; }

        public int PRINCIPALID { get; set; }

        [Required]
        public string INVOICENO { get; set; }

        public DateTime INVOICE_DATE { get; set; }

        public decimal INVOICE_AMOUNT { get; set; }

        public short INVOICE_CURRENCYID { get; set; }

        [Required]
        public string CONTRACTNO { get; set; }

        public DateTime CONTRACT_STARTDATE { get; set; }

        public DateTime CONTRACT_ENDDATE { get; set; }

        public string PURCHASEORDERNUMBER { get; set; }

        public short? APPROVALSTATUSID { get; set; }

        public string APPROVAL_COMMENT { get; set; }

        public int? APPROVEDBY { get; set; }

        public DateTime? APPROVEDDATETIME { get; set; }

        public string CERTIFICATENO { get; set; }

        public string ENTRYSHEETNUMBER { get; set; }

        public bool REVALIDATED { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_EXCEPTIONAL_LOAN_APPL_DETAIL TBL_EXCEPTIONAL_LOAN_APPL_DETAIL { get; set; }

    }
}
