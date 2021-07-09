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
        public short CURRENCYID { get; set; }
        public decimal EXPOSURE { get; set; }
        public decimal APPROVEDAMOUNT { get; set; }
        public short PRODUCTID { get; set; }
        public decimal IMPACT { get; set; }      
        public int CREATEDBY { get; set; }
        public int? LASTUPDATEDBY { get; set; }
        public bool DELETED { get; set; }
        public int? DELETEDBY { get; set; }
        public DateTime? DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public int? LOANAPPLICATIONID { get; set; }
        public int? CUSTOMERID { get; set; }
        public int? TENOR { get; set; }
       




    }
}
