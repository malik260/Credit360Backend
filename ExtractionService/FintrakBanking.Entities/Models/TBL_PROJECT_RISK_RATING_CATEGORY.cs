using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_PROJECT_RISK_RATING_CATEGORY")]
    public partial class TBL_PROJECT_RISK_RATING_CATEGORY
    {
        [Key]
        public int CATEGORYID { get; set; }
        public string CATEGORYNAME { get; set; }
    }
}
