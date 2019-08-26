using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class BulkDisbursementSetupPackageViewModel : GeneralEntity
    {
        public int disbursementPackageId { get; set; }
        public int groupCustomerId { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string packageDescription { get; set; }
        public string customerName { get; set; }
    }

    public class BulkDisbursementSetupSchemeViewModel : GeneralEntity
    {
        public string facilityName;
        public string disbursementPackageName;

        public int disbursementPackageId { get; set; }
        public int disburseSchemeId { get; set; }
        public int productId { get; set; }
        public int tenor { get; set; }
        public int scheduleMethodId { get; set; }
        public int interestRate { get; set; }
        public int productPriceIndexId { get; set; }
        public bool includeProductFees { get; set; }
        public int approvalStatusId { get; set; }
        public string scheduleName { get; set; }
        public string schemeName { get; set; }
    }

    public class BulkDisbursementSetupSchemeFeesViewModel : GeneralEntity
    {
        public string schemeName;

        public int schemeFeeId { get; set; }
        public int disburseSchemeId { get; set; }
        public int chargeFeeId { get; set; }
        public bool hasConcession { get; set; }
        public int approvalStatusId { get; set; }
    }


}
