using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.credit
{
    public class LcCashBuildUpPlanViewModel : GeneralEntity
    {
        public int lcCashBuildUpPlanId { get; set; }
        public int lcIssuanceId { get; set; }
        public decimal amount { get; set; }
        public int currencyId { get; set; }
        public int cashBuildUpReferenceTypeId { get; set; }
        public string cashBuildUpReferenceTypeName { get; set; }
        public string lcReferenceNumber { get; set; }
        public int collectionCasaAccountId { get; set; }
        public int daysInterval { get; set; }
        public DateTime? planDate { get; set; }

    }
}
