
using System.ComponentModel.DataAnnotations;

namespace FintrakBanking.ExtractionService.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class STG_FREECODE1
    {

       
        [StringLength(10)]
        public string FREECODE1 { get; set; }
        
        [StringLength(10)]
        public string REF_DESC { get; set; }
        
        [StringLength(2)]
        public string DEL_FLG { get; set; }
       
        [StringLength(2)]
        public string BANK_ID { get; set; }
    }
}
