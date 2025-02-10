using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("st_NmrcEligibility")]
    public class StNmrcEligibility
    {
        public int Id { get; set; }
        public int Category { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public int? DocUpload { get; set; }

    }
}
