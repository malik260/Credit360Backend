using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_EXCEPTIONAL_LOAN_APPL_DETL_BG")]
    public class TBL_EXCEPTIONAL_LOAN_APPL_DETL_BG
    {
        [Key]
        public int BONDID { get; set; }

        public int EXCEPTIONALLOANAPPLDETAILID { get; set; }

        public int? PRINCIPALID { get; set; }
        public string PRINCIPALNAME { get; set; }

        public decimal AMOUNT { get; set; }

        public short CURRENCYID { get; set; }

        public DateTime CONTRACT_STARTDATE { get; set; }

        public DateTime CONTRACT_ENDDATE { get; set; }

        public bool ISTENORED { get; set; }

        public bool ISBANKFORMAT { get; set; }

        public short? APPROVALSTATUSID { get; set; }

        public string APPROVAL_COMMENT { get; set; }

        public string REFERENCENO { get; set; }

        public int? CASAACCOUNTID { get; set; }

        public int? APPROVEDBY { get; set; }

        public DateTime? APPROVEDDATETIME { get; set; }

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
