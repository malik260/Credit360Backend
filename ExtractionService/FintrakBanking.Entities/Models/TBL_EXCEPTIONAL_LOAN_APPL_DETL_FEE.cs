using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_EXCEPTIONAL_LOAN_APPL_DETL_FEE")]
    public class TBL_EXCEPTIONAL_LOAN_APPL_DETL_FEE
    {
        [Key]
        public int LOANCHARGEFEEID { get; set; }

        public int EXCEPTIONALLOANAPPLDETAILID { get; set; }

        public int CHARGEFEEID { get; set; }

        public bool HASCONSESSION { get; set; }

        public string CONSESSIONREASON { get; set; }

        public short APPROVALSTATUSID { get; set; }

        public decimal DEFAULT_FEERATEVALUE { get; set; }

        public decimal RECOMMENDED_FEERATEVALUE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

        public virtual TBL_APPROVAL_STATUS TBL_APPROVAL_STATUS { get; set; }

        public virtual TBL_CHARGE_FEE TBL_CHARGE_FEE { get; set; }

        public virtual TBL_EXCEPTIONAL_LOAN_APPL_DETAIL TBL_EXCEPTIONAL_LOAN_APPL_DETAIL { get; set; }

    }
}
