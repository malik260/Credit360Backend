using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FintrakBanking.Entities.Models
{
    [Table("TBL_PSR_ANALYSIS")]
    public partial class TBL_PSR_ANALYSIS
    {
        [Key]
        public int PSRANALYSISID { get; set; }

        public decimal? VALUEOFCOLLATERAL { get; set; }

        public decimal? IPC { get; set; }
        public decimal? PMU { get; set; }
        public decimal? AMOUNTDISBURSED { get; set; }
        public decimal? AMOUNTREQUESTED { get; set; }
        public int PROJECTSITEREPORTID { get; set; }
        public bool DELETED { get; set; }
        public int CREATEDBY { get; set; }
        public int DELETEDBY { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
    }
}
