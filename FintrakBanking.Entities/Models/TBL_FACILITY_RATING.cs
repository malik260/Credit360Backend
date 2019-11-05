using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_FACILITY_RATING")]
    public partial class TBL_FACILITY_RATING
    {
        [Key]
        public int FACILITYRATINGID { get; set; }

        public int LOANAPPLICATIONDETAILID { get; set; }
        public string CUSTOMERCODE { get; set; }

        public string PROBABILITYOFDEFAULT { get; set; }
        public string REMARK { get; set; }

        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }

        public DateTime? DATETIMEUPDATED { get; set; }

        public bool DELETED { get; set; }

        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
    }
}
