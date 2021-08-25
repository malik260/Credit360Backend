using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CONTRACTOR_CRITERIA")]
    public partial class TBL_CONTRACTOR_CRITERIA
    {
        [Key]
        public int CRITERIAID { get; set; }
        public string CRITERIA { get; set; }
        public decimal TIERONE { get; set; }
        public decimal TIERTWO { get; set; }
        public decimal TIERTHREE { get; set; }
    }
}
