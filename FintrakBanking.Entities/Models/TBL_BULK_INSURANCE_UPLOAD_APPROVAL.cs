using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("TBL_BULK_INSURANCE_UPLOAD_APPROVAL")]
    public class TBL_BULK_INSURANCE_UPLOAD_APPROVAL
    {
        [Key]
        public int BULKINSURANCEUPLOADAPPROVALID { get; set; }
        public string BATCHCODE { get; set; }
        public int APPROVALSTATUSID { get; set; }
        public int OPERATIONID { get; set; }
        public DateTime REQUESTDATE { get; set; }
        public int CREATEDBY { get; set; }
    }
}
