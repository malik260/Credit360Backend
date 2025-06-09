using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_NMRC_CONDITION_PRECEDENTS")]
    public class TBL_NMRC_CONDITIONPRECEDENTS
    {
        public int Id { get; set; }
        public string ConditionPrecedent { get; set; }
        public int Deffered { get; set; }
        public int InPlace { get; set; }
        public int NotInPlace { get; set; }
    }
}
