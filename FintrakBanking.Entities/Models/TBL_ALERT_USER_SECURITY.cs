using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_USER_SECURITY")]
    public partial class TBL_ALERT_USER_SECURITY
    {
        [Key]
        public int ID { get; set; }
        public string STAFFID { get; set; }
        public string LOGINID { get; set; }
        public string NAME { get; set; }
        public string EMAIL { get; set; }
        public DateTime DATEEMPLOYED { get; set; }
        public string USERCODE { get; set; }
        public string USERNAME { get; set; }
        public string ACCOUNTOFFICERNAME { get; set; }
        public string TEAMCODE { get; set; }
        public string TEAMNAME { get; set; }
        public string BRANCHCODE { get; set; }
        public string BRANCHNAME { get; set; }
        public string REGIONCODE { get; set; }
        public string REGIONNAME { get; set; }
        public string GROUPCODE { get; set; }
        public string GROUPNAME { get; set; }
        public string GROUPHEADNAME { get; set; }
        public string DIVISIONCODE { get; set; }
        public string DIVISIONNAME { get; set; }
    }
}
