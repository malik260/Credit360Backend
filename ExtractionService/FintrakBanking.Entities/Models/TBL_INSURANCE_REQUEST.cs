using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_INSURANCE_REQUEST")]
    public partial class TBL_INSURANCE_REQUEST
    {
        [Key]
        public int INSURANCEREQUESTID { get; set; }
        public int REQUESTNUMBER { get; set; }
        public string REQUESTREASON { get; set; }
        public string REQUESTCOMMENT { get; set; }
        public short? APPROVALSTATUSID { get; set; }
        public int COLLATERALCUSTOMERID { get; set; }
        public int? CREATEDBY { get; set; }
        public DateTime? DATETIMECREATED { get; set; }
        public DateTime? DATETIMEUPDATED { get; set; }
        public DateTime? DATETIMEDELETED { get; set; }
        public int? LASTUPDATEDBY { get; set; }
    }
}
