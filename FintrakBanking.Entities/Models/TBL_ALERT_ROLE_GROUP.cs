using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_ROLE_GROUP")]
    public partial class TBL_ALERT_ROLE_GROUP
    {
        [Key]
        public int ALERTLEVELGROUPID { get; set; }
        public string LEVELGROUPNAME { get; set; }
        public string DESCRIPTION { get; set; }
    }
}
