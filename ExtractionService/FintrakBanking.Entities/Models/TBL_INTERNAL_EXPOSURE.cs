using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_INTERNAL_EXPOSURE")]
  public partial class TBL_INTERNAL_EXPOSURE
    {
        [Key]
        public int INTERNALEXPOSUREID { get; set; }
        public string CUSTOMERNAME { get; set; }
        public string REFERENCENUMBER { get; set; }
        public string ACCOUNTNUMBER { get; set; }
        public string ACCOUNTOFFICERCODE { get; set; }
        public string ACCOUNTOFFICERNAME { get; set; }
        public string PRODUCTNAME { get; set; }
        public string FACILITYTYPE { get; set; }
        public string MATURITYDATE { get; set; }
        public string LOCATION { get; set; }
        public string BRANCHNAME { get; set; }
        public string BRANCHCODE { get; set; }
        public DateTime? EXPIRYDATE { get; set; }
        public DateTime? MEETINGDATE { get; set; }

    }
}
