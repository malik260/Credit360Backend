using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_USER_CLAIMS")]

    public class TBL_USER_CLAIMS
    {
        [Key]
        public int USERCLAIMID { get; set; }
        public string TOKEN { get; set; }
        public bool ISACTIVE { get; set; }
        public int USERID { get; set; }
        public DateTime DATETIMECREATED { get; set; }

    }
}
