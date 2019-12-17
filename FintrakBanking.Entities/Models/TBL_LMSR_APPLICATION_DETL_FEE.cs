using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LMSR_APPLICATION_DETL_FEE")]
    public partial class TBL_LMSR_APPLICATION_DETL_FEE
    {
        [Key]
        public int LOANCHARGEFEEID { get; set; }

        public int LOANREVIEWAPPLICATIONID { get; set; }

        public int CHARGEFEEID { get; set; }

        public bool HASCONSESSION { get; set; }

        //[StringLength(3000)]
        public string CONSESSIONREASON { get; set; }

        public short APPROVALSTATUSID { get; set; }

        //[Column(TypeName = "money")]
        public decimal DEFAULT_FEERATEVALUE { get; set; }

        //[Column(TypeName = "money")]
        public decimal RECOMMENDED_FEERATEVALUE { get; set; }

        public int CREATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }

      
        public virtual TBL_LMSR_APPLICATION_DETAIL TBL_LMSR_APPLICATION_DETAIL { get; set; }

     
    }
}
