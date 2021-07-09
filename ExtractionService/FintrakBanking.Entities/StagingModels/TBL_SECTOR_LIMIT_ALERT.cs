using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.StagingModels
{
    [Table("TBL_SECTOR_LIMIT_ALERT")]
    public partial class TBL_SECTOR_LIMIT_ALERT
    {
        [Key]
        public int ID { get; set; }
        public string SECTOR { get; set; }
        public decimal? BBD { get; set; }
        public decimal? CBD { get; set; }
        public decimal? CIBD { get; set; }
        public decimal? RBD { get; set; }
        public decimal? EXPOSURE { get; set; }
        public decimal? BANK { get; set; }
    }
}
