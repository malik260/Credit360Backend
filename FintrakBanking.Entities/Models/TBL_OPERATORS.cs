using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_OPERATORS")]
    public class TBL_OPERATORS
    {
        [Key]
        public int OPERATORID { get; set; }
        public string OPERATOR { get; set; }
        public string DESCRIPTION { get; set; }
    }
}