using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_ROLE_GRP_MAPPING")]
    public partial class TBL_ALERT_ROLE_GRP_MAPPING
    {
        [Key]
        public int ALERTLEVELGROUPMAPID { get; set; }
        public string LEVELCODE { get; set; }
        public int LEVELGROUPID { get; set; }
    }
}
