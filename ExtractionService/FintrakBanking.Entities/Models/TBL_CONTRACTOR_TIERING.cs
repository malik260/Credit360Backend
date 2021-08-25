using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CONTRACTOR_TIERING")]
    public partial class TBL_CONTRACTOR_TIERING
    {
        [Key]
        public int CONTRACTORTIERID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public int CUSTOMERID { get; set; }
        public int CONTRACTORCRITERIAID { get; set; }
        public decimal ACTUALVALUE { get; set; }
        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
