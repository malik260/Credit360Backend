using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_APPROVED_TRADE_CYCLE")]
    public class TBL_APPROVED_TRADE_CYCLE
    {
        [Key]
        public int APPROVEDTRADECYCLEID { get; set; }
        public int APPROVEDTRADECYCLEDAYS { get; set; }
    }
}