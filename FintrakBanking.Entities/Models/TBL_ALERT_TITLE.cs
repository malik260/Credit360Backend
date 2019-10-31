using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_TITLE")]
    public partial class TBL_ALERT_TITLE
    {
        [Key]
        public int ALERTTITLEID { get; set; }
        public string TITLE { get; set; }
        public string TEMPLATE { get; set; }
        public string BUSINESSOWNER { get; set; }
        public string SENDERNAME { get; set; }
        public string SENDEREMAIL { get; set; }
        public string TEMPLATETYPE { get; set; }
        //public int BINDINGTYPE { get; set; }
        //public DateTime LASTSENTDATE { get; set; }
        //public int SOURECE { get; set; }

    }
}
