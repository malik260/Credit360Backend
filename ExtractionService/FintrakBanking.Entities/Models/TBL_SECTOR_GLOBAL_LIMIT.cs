using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_SECTOR_GLOBAL_LIMIT")]
    public partial class TBL_SECTOR_GLOBAL_LIMIT
    {
        [Key]
        public int ID { get; set; }
        public string CBNSECTOR { get; set; }
        public string CBNSECTORID { get; set; }
        public DateTime? DATE { get; set; }
        public decimal? TOTALEXPOSURELCY { get; set; }
        public decimal? EXPOSURES { get; set; }
        public decimal? SECTORLIMIT { get; set; }
    }
}
