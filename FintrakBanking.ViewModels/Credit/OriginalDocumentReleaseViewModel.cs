using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class OriginalDocumentReleaseViewModel : GeneralEntity
    {
        public int originalDocumentReleaseId { get; set; }
        public int originalDocumentApprovalId { get; set; }
        public int documentUploadId { get; set; }
        public int approvalStatusId { get; set; }
        public string approvalStatus { get; set; }
        public string customerName { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string documentReferenceNumber { get; set; }
        public string createdByName { get; set; }
        public int companyId { get; set; }
        public DateTime? docDateTimeCreated { get; set; }
        public string documentDescription { get; set; }
        public int operationId { get; set; }
        public string comment { get; set; }
        public string documentCategoryName { get; set; }
        public string documentTypeName { get; set; }
        public int? docSubmissionOperationId { get; set; }
        public DateTime? approvalDate { get; set; }
        public string collateralCode { get; set; }
    }
}
