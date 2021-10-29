using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class OriginalDocumentReleaseViewModel : GeneralEntity
    {
        public DateTime? dateRecieved { get; set; }

        public int originalDocumentReleaseId { get; set; }
        public int originalDocumentApprovalId { get; set; }
        public int documentUploadId { get; set; }
        public short approvalStatusId { get; set; }
        public string approvalStatus { get; set; }
        public string customerName { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string documentReferenceNumber { get; set; }
        public string createdByName { get; set; }
        //public int companyId { get; set; }
        public DateTime? docDateTimeCreated { get; set; }
        public string documentDescription { get; set; }
        public int operationId { get; set; }
        public string comment { get; set; }
        public string documentCategoryName { get; set; }
        public string documentTypeName { get; set; }
        public int? docSubmissionOperationId { get; set; }
        public DateTime? approvalDate { get; set; }
        public string collateralCode { get; set; }
        public int? collateralCustomerId { get; set; }
        public int loanApplicationId { get; set; }
        public string description { get; set; }
        public string approvalStatusName { get; set; }
        public DateTime arrivalDate { get; set; }
        public string responsiblePerson { get; set; }
        public int approvalTrailId { get; set; }
        public int collateralId { get; set; }
        public int? loopedStaffId { get; set; }
        public string divisionCode { get; set; }
        public string divisionShortCode { get; set; }

        public short? perfectionStatusId { get; set; }
        public short? litigationStatusId { get; set; }
        public bool? isOnAmconList { get; set; }
        public int userId { get; set; }
        public int customerId { get; set; }
        public string isAmconList { get; set; }
        public string perfectionStatus { get; set; }
        public string litigationStatus { get; set; }
        public int numberOfTimesApprove { get; set; }
        public string currentApprovalLevel { get; set; }
        public decimal facilityAmount { get; set; }
        public string customerAccount { get; set; }
        public int targetId { get; set; }
        public string relationshipOfficerName { get; set; }
        public string relationshipManagerName { get; set; }
        public string referenceNumber { get; set; }
        public string branchName { get; set; }
        public string customerCode { get; set; }
        public string productName { get; set; }
        public decimal applicationAmount { get; set; }
        public double interestRate { get; set; }
        public DateTime applicationDate { get; set; }
        public string currency { get; set; }
        public string fileName { get; set; }
        public DateTime drawdownInitiationDate { get; set; }
    }

    public class OutStandingDocumentViewModel
    {
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public string facilityType { get; set; }
        public decimal facilityAmount { get; set; }
        public string reference { get; set; }
        public string accountOfficerName { get; set; }
        public string relationshipManager { get; set; }
        public string groupHead { get; set; }
        public string sbu { get; set; }
        public string condition { get; set; }
        public int numberOfDays { get; set; }
        public int createdBy { get; set; }
        public int customerId { get; set; }
        public DateTime? dateOnFinalApproval { get; set; }
        public DateTime? deferredDate { get; set; }
    }
}
