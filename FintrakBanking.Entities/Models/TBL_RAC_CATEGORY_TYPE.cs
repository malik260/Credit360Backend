using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_RAC_CATEGORY_TYPE")]
    public partial class TBL_RAC_CATEGORY_TYPE
    {
        [Key]
        public int RACCATEGORYTYPEID { get; set; }
        [Required]
        public int RACCATEGORYID { get; set; }
        public string RACCATEGORYTYPE { get; set; }
    }
}
