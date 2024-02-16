using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class DocumentUploadViewModel : GeneralEntity
    {
        public string deletedByName;
        public string countryCode { get; set; }

        public int documentUploadId { get; set; }

        public string fileName { get; set; }

        public string fileExtension { get; set; }

        public int fileSize { get; set; }

        public string fileSizeUnit { get; set; }

        public byte[] fileData { get; set; }

        public new int? companyId { get; set; }

        public DateTime? issueDate { get; set; }

        public DateTime? expiryDate { get; set; }

        public string physicalFilenumber { get; set; }

        public string physicalLocation { get; set; }

        public int documentUsageId { get; set; }

        public int targetId { get; set; }

        public string targetCode { get; set; }

        public string targetReferenceNumber { get; set; }

        public string documentCode { get; set; }

        public string documentTitle { get; set; }

        public string customerCode { get; set; }

        public int documentCategoryId { get; set; }

        public int documentTypeId { get; set; }

        public int? approvalStatusId { get; set; }

        public int? documentStatusId { get; set; }

        public bool isPrimaryDocument { get; set; }
        public string documentTypeName { get; set; }
        public string documentCategoryName { get; set; }
        public bool owner { get; set; }
        public string uploadedBy { get; set; }
        public string customerName { get; set; }
        public bool isOriginalCopy { get; set; }

        public int? source { get; set; }

        public int? edmsDocumentId { get; set; }

        public string fileSizeString { get {
                string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
                if (fileSize == 0)
                    return "0" + suf[0];
                long bytes = Math.Abs(fileSize);
                int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
                double num = Math.Round(bytes / Math.Pow(1024, place), 1);
                return (Math.Sign(fileSize) * num).ToString() + suf[place];
            } }

        public int operationId { get; set; }
        public int customerId { get; set; }
        public int customerGroupId { get; set; }
        public bool overwrite { get; set; }
        public int customerCreditBureauId { get; set; }
        public bool isBulk { get; set; }
        public string csdc { get; set; }
    }

    public class CustomerDocumentSearchViewModel
    {
        public string customerName { get; set; }

        public IEnumerable<DocumentUploadViewModel> documents { get; set; }
    }

    public class RecoveryReportingDocumentViewModel
    {
        public int loanRecoveryReportingDocumentId { get; set; }
        public string description { get; set; }
        public int operationId { get; set; }
        public bool overwrite { get; set; }
        public int targetId { get; set; }
        public string referenceId { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public int fileSize { get; set; }
        public short userBranchId { get; set; }
        public string userIPAddress { get; set; }
        public string applicationUrl { get; set; }
        public int createdBy { get; set; }
        public int companyId { get; set; }
        public string uploadedBy { get; set; }
        public byte[] fileData { get; set; }
        public DateTime dateTimeCreated { get; set; }
    }

    public class DocumentUploadViewModelResut
    {
        public string success;
        public int result { get; set; }

        public string message { get; set; }
    }

    public class DeferredDocumentsViewModel
    {
        public int deferredDodId { get; set; }
        public int documentCategoryId { get; set; }
        public int documentTypeId { get; set; }
        public int loanApplicationId { get; set; }
        public DateTime dueDate { get; set; }
        public int createdBy { get; set; }
        public DateTime datetimeCreated { get; set; }
        public bool submitted { get; set; }
        public DateTime datetimeSubmited { get; set; }
        public int lastUpdatedBy { get; set; }
        public DateTime datetimeUpdated { get; set; }
        public bool deleted { get; set; }
        public int deletedBy { get; set; }
        public DateTime datetimeDeleted { get; set; }
        public int tenor { get; set; }
        public string applicationUrl { get; set; }
        public string documentCategoryName { get; set; }
        public string documentTypeName { get; set; }
        public string applicationReferenceNumber { get; set; }
        public short facilityTypeId { get; set; }
        public string facilityTypeName { get; set; }
    }

}