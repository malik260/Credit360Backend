using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    public partial class TBL_LOAN_DISBURSEMENT
    {
        [Key]
        public int LOANDISBURSEMENTID { get; set; } 

        public int TERMLOANID { get; set; }

        [StringLength(50)]
        public string ACCOUNTNUMBER { get; set; }

        public decimal AMOUNTDISBURSED { get; set; }

        public int CREATEDBY { get; set; }

        public int? LASTUPDATEDBY { get; set; }

        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }

        public DateTime? DATETIMEDELETED { get; set; }
    }
}
