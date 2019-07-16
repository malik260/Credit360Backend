using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_VALUATION_REQUESTTYPE")]
    public partial class TBL_VALUATION_REQUESTTYPE
    {
        [Key]
        public int VALUATIONREQUESTTYPEID { get; set; }

        [Required]
        [StringLength(100)]
        public string VALUATIONREQUESTTYPE { get; set; }


        public int DELETED { get; set; }
    }
    
}
