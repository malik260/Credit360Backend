using FintrakBanking.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    
    public class BulkDisbursementSetupSchemeViewModel : GeneralEntity
    {
        public string currencyName;

        public string customerName { get; set; }
        public string customerCode { get; set; }
        public int loanApplicationId { get; set; }
        public decimal approvedAmount { get; set; }

        public int disburseSchemeId { get; set; }
        public int currencyId { get; set; }
        public string facilityName { get; set; }
        public int loanApplicationDetailId { get; set; }
        public string schemeCode { get; set; }
        public int productId { get; set; }
        public int tenor { get; set; }
        public int scheduleMethodId { get; set; }
        public float interestRate { get; set; }
        public int productPriceIndexId { get; set; }
        public int approvalStatusId { get; set; }
        public string scheduleName { get; set; }
        public string schemeName { get; set; }
        public string applicationReferenceNumber { get; set; }
        public int interestFrequencyTypeId { get; set; }
        public int principalFrequencyTypeId { get; set; }
        public int scheduleDayCountConventionId { get; set; }
        // public TBL_LOAN_APPLICATION_DETAIL LOANAPPLICATIONDETAIL { get; set; }
    }

    
}
