using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.StagingModels
{
    [Table("STG_USER_MIS")]
   public partial class STG_USER_MIS
    {
        [Key]
        public int USERMISID { get; set; }
        public string PROFITCENTERDEFINITIONCODE { get; set; }
        public string PROFITCENTERMISCODE { get; set; }
        public string LOGINID { get; set; }
    }
}
