using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_BINDING_METHODS")]
    public partial class TBL_ALERT_BINDING_METHODS
    {
        [Key]
        public int BINDINGMEHTODID { get; set; }
        public string METHODTITLE { get; set; }
        public string METHODTNAME { get; set; }
        public DateTime DATETIMECREATED { get; set; }
        public int CREATEDBY { get; set; }
    }
}
