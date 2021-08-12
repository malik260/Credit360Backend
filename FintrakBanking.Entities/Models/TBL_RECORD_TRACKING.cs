using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_RECORD_TRACKING")]
    public partial class TBL_RECORD_TRACKING
    {
        [Key]
        public int RECORDID { get; set; }
        public string STATUSMESSAGE { get; set; }
        public DateTime CURRENTDATE { get; set; }
    }
}
