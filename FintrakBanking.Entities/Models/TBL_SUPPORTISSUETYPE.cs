using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_SUPPORTISSUETYPE")]
    public class TBL_SUPPORTISSUETYPE
    {
        [Key]
        public int SUPPORTISSUETYPEID { get; set; }
        public string DESCRIPTION {get; set;}
        public int? TAG {get; set;}
    }
}
