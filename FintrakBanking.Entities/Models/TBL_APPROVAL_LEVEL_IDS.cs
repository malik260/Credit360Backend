using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_APPROVAL_LEVEL_IDS")]
    public class TBL_APPROVAL_LEVEL_IDS
    {
        [Key]
        public int MATCHINGID { get; set; }
        public int LOCALAPPROVALLEVELID { get; set; }
        public int REMOTEAPPROVALLEVELID { get; set; }
        public string COUNTRYCODE { get; set; }
        public string STAFFROLECODE { get; set; }
        public string STAFFNT { get; set; }

    }
}
