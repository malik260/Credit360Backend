using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_CUSTOMER_RATIO_CATEGORY")]
    public partial class TBL_CUSTOMER_RATIO_CATEGORY
    {
        [Key]
        public int CATEGORYID { get; set; }
        public string CATEGORYNAME { get; set; }
        public bool SHOWATFAM { get; set; }

    }
}
