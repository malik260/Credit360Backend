using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
   
    [Table("TBL_GLOBAL_EXPOSURE_MANUAL")]
    public partial class TBL_GLOBAL_EXPOSURE_MANUAL
    {
        [Key]
        public int EXPOSUREMANUALID { get; set; }
        public decimal CURRENCYID { get; set; }
        public decimal EXPOSURE { get; set; }
        public decimal LEGALLENDINGLIMIT { get; set; }
        public decimal APPROVEDAMOUNT { get; set; }
        public int PRODUCTID { get; set; }
        public int CUSTOMERID { get; set; }
        public int LOANAPPLICATIONID { get; set; }
        public int EXPOSURECURRENCYID { get; set; }
        public string FACILITYNAME { get; set; }
        public decimal IMPACT { get; set; }


    }
}
