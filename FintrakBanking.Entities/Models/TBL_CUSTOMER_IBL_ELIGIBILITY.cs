using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    public class TBL_CUSTOMER_IBL_ELIGIBILITY
    {
        [Key]
        public int ELIGIBILITYID { get; set; }
        public int CUSTOMERID { get; set; }
        public string RESPONSEDESCRIPTION { get; set; }
        public decimal MAXIMUMAMOUNT { get; set; }
        public decimal MINIMUMAMOUNT { get; set; }
        public bool ISELIGIBLE { get; set; }
        public string FULLDESCRIPTION { get; set; }
        public string ACCOUNTNUMBER { get; set; }
        public string AMOUNT { get; set; }
        public bool ISIBLREQUEST { get; set; }
        public string PHONENUMBER { get; set; }
    }
}
