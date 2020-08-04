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
        public string APPLICATIONREFERENCENUMBER { get; set; }
        public int CUSTOMERID { get; set; }
        public string CUSTOMERTIER { get; set; }
        public string CONTRACTEMPLOYER { get; set; }
        public string COMPLEXITY { get; set; }
        public string LOCATION { get; set; }
        public decimal PROJECTVALUE { get; set; }
        public string IMPORTCONTENT { get; set; }
        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
    }
}
