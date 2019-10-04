using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_SETUP")]
  public partial class TBL_ALERT_SETUP
    {
        [Key]
        public int ALERTSETUPID { get; set; }
        public int TITLEID { get; set; }
        public int LEVELGROUPMAPPINGID { get; set; }
        public int FREQUENCYID { get; set; }
        public int CONDITIONID { get; set; }

    }
}
