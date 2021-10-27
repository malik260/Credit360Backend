using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_EXCEPTIONAL_LOAN_APPL_DETL_SYN")]
    public class TBL_EXCEPTIONAL_LOAN_APPL_DETL_SYN
    {
        [Key]
        public int SYNDICATIONID { get; set; }

        public int EXCEPTIONALLOANAPPLDETAILID { get; set; }

        [Required]
        public string BANKCODE { get; set; }

        [Required]
        public string BANKNAME { get; set; }

        public decimal AMOUNTCONTRIBUTED { get; set; }

        public int PARTY_TYPEID { get; set; }

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
