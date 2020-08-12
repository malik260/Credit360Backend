using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CONTRACTOR_CRITERIA_OPTION")]
    public partial class TBL_CONTRACTOR_CRITERIA_OPTION
    {
        [Key]
        public int OPTIONID { get; set; }
        public int CRITERIAID { get; set; }
        public string OPTIONNAME { get; set; }
        public decimal OPTIONVALUE { get; set; }
    }
}
