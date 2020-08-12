using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_LMS_FACILITY_MODIFICATION")]
    public partial class TBL_LMS_FACILITY_MODIFICATION
    {
        [Key]
        public int FACILITYMODIFICATIONID { get; set; }
        public int LOANAPPLICATIONDETAILID { get; set; }
        public int APPROVEDTENOR { get; set; }
        public short APPROVEDPRODUCTID { get; set; }
        public int TENORMODEID { get; set; }
        public short SUBSECTORID { get; set; }
        public int PRODUCTCLASSID { get; set; }
        public int? PRODUCTCLASSPROCESSID { get; set; }
        public decimal APPROVEDAMOUNT { get; set; }
        public double APPROVEDINTERESTRATE { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public string REVIEWDETAILS { get; set; }
        
        public int CREATEDBY { get; set; }
        public DateTime DATETIMECREATED { get; set; }
    }
}
