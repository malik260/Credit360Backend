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
        public string currentApprovalLevel { get; set; }
        public int collateralId { get; set; }
        public int? loopedStaffId { get; set; }

        public short? perfectionStatusId { get; set; }
        public short? litigationStatusId { get; set; }
        public bool? isOnAmconList { get; set; }
        public int userId { get; set; }
        public int customerId { get; set; }

    }
}
