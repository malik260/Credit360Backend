using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_ALERT_GROUP_EMAIL")]
    public partial class TBL_ALERT_GROUP_EMAIL
    { 
    [Key]
    public int GROUPEMAILID { get; set; }
    public string GROUPCODE { get; set; }
    public string GROUPNAME { get; set; }
    public string GROUPEMAIL { get; set; }
    
    }
}
