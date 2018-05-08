using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBaking.DataMigration
{
  public  class BranchViewModel 
    {
        public short branchId { get; set; }
        public int companyId { get; set; }
        public string branchName { get; set; }
        public string branchCode { get; set; }
        public int regionId { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public int? stateId { get; set; }
        public int? cityId { get; set; }
        public bool deleted { get; set; }
        public DateTime datetimeCreated { get; set; }
        public string comment { get; set; }
        public bool nplLimitExceeded { get; set; }
        public decimal npl_Limit { get; set; }
        public int deletedBy { get; set; }
        public DateTime? datetimeDeleted { get; set; }
        public int? lastUpdatedBy { get; set; }
    }
}
