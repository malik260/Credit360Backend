using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CODE_TRACKER")]
    public class TBL_CODE_TRACKER
    {
        [Key]
        public int CODEID { get; set; }
        public int OSDC { get; set; }
        public int ASDC { get; set; }
        public int CSDC { get; set; }
        public DateTime CURRENTDATE { get; set; }
    }
}