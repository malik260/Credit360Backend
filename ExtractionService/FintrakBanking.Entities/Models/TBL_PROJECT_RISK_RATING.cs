using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_PROJECT_RISK_RATING")]
    public partial class TBL_PROJECT_RISK_RATING
    {
        [Key]
        public int PROJECTRISKRATINGID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }
        public int? LOANBOOKINGREQUESTID { get; set; }
        public int CATEGORYID { get; set; }
        public int CATEGORYVALUE { get; set; }
        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public string PROJECTLOCATION { get; set; }
        public string PROJECTDETAILS { get; set; }


    }
}
