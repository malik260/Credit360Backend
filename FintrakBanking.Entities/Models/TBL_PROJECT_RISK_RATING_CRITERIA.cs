using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_PROJECT_RISK_RATING_CRITERIA")]
    public partial class TBL_PROJECT_RISK_RATING_CRITERIA
    {
        [Key]
        public int PROJECTRISKRATINGCRITERIAID { get; set; }
        public string CRITERIA { get; set; }
        public decimal CRITERIAVALUE { get; set; }
        public int PROJECTRISKRATINGCATEGORYID { get; set; }
    }
}
