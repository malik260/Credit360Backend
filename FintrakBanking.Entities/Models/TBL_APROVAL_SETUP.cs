using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_APROVAL_SETUP")]
    public partial class TBL_APROVAL_SETUP
    {
        [Key]
        public int APPROVALSETUPID { get; set; }
        public bool USEROUNDROBIN { get; set; }
        public bool ISRETAILONLYROUNDROBIN { get; set; }
    }
}
