using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.StagingModels
{
    [Table("STG_SUBSIDIARIES")]
    public class STG_SUBSIDIARIES
    {
        [Key]
        public int SUBSIDIARYID { get; set; }
        public string SUBSIDIARYNAME { get; set; }
        public int COUNTRYID { get; set; }
    }
}
