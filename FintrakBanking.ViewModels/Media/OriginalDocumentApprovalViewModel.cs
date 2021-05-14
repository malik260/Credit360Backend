using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Media
{
    public class OriginalDocumentApprovalViewModel : GeneralEntity
    {
        public int originalDocumentApprovalId { get; set; }

        public int loanApplicationId { get; set; }

        public string description { get; set; }
        public bool isOriginalTitleDocument { get; set; }
        public string isOriginalTitleDocumentString { get; set; }

        public short approvalStatusId { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string referenceNumber { get; set; }
        public string approvalStatusName { get; set; }
        public string customerName { get; set; }
        public string customerCode { get; set; }
        public int customerId { get; set; }
        public int customerID { get; set; }
        public int? businessUnit { get; set; }
        public string branchName { get; set; }
        public DateTime applicationDate { get; set; }
        public decimal applicationAmount { get; set; }
        public double interestRate { get; set; }
        public string productName { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public int operationId { get; set; }
        public string comment { get; set; }
        public DateTime? approvalDate { get; set; }
        public bool atInitiator { get; set; }
        public string createdByName { get; set; }
        public int collateralCustomerId { get; set; }
        public decimal collateralValue { get; set; }
        public string collateralType { get; set; }

        public decimal exposureValue { get; set; }
        public string collateralCode { get; set; }
        public int collateralTypeId { get; set; }
        public DateTime arrivalDate { get; set; }
        public string responsiblePerson { get; set; }
        public int approvalTrailId { get; set; }
        public string currentApprovalLevel { get; set; }
        public int? loopedStaffId { get; set; }
        public string fintrakReferenceId { get; set; }
        public string facilityType { get; set; }
        public decimal facilityAmount { get; set; }
        public DateTime bookingDate { get; set; }
        public int daysOverdue { get; set; }
        public string accountOfficer { get; set; }
        public string division { get; set; }
        public string groupHead { get; set; }
        public string team { get; set; }
        public string sla { get; set; }
        public string businessUnitId { get; set; }
    }
}