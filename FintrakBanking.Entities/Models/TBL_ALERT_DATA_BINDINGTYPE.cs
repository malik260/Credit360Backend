using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_DATA_BINDINGTYPE")]
    public partial class TBL_ALERT_DATA_BINDINGTYPE
    {
        [Key]
        public int ALERTBINDINGTYPEID { get; set; }
        public string BINDINGTYPE { get; set; }
        public string BINDINGRULE { get; set; }
        public string TARGET { get; set; }


    }
}
