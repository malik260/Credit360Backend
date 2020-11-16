using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class ValuationPrerequisiteViewModel : GeneralEntity
    {
        public int valuationPrerequisiteId { get; set; }
        public int collateralValuationId { get; set; }//
        public int collateralCustomerId { get; set; }
        public int valuationRequestTypeId { get; set; }
        public string valuationComment { get; set; }
        public string valuationRequestType { get; set; }
        public int? operationId { get; set; }
        public int customerId { get; set; }
        public string collateralCode { get; set; }
        public string collateralType { get; set; }
        public int? approvalStatusId { get; set; }
        public int? approvalTrailId { get; set; }
        public int? loopedStaffId { get; set; }
        public string approvalStatus { get; set; }
        public string comment { get; set; }
        public int valuerId { get; set; }
        public string divisionShortCode { get; set; }
        public string divisionCode { get; set; }

        public decimal? omv { get; set; }
        public decimal? fsv { get; set; }

        public decimal? valuationFee { get; set; }
        public int valuationReportId { get; set; }
        public string valuer { get; set; } //
        public string accountNumber { get; set; }
        public decimal? wht { get; set; }
        public decimal? whtAmount { get; set; }

        public string customerName { get; set; }
        public decimal collateralValue { get; set; }
        public decimal facilityAmount { get; set; }
        public string valuationReason { get; set; }
        public string valuationName { get; set; }
        public string approvalComment { get; set; }
        public string referenceNumber { get; set; }

        public int? currentApprovalLevelId { get; set; }
        
        public string currentApprovalLevel { get; set; }
        public string responsiblePerson { get; set; }
        public DateTime arrivalDate { get; set; }
        public string customerAccount { get; set; }
        public string createdByName { get; set; }
        public int targetId { get; set; }
        public string narration { get; set; }
    }
}
