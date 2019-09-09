using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class DocumentUploadViewModel : GeneralEntity
    {
        public int documentUploadId { get; set; }

        public string fileName { get; set; }

        public string fileExtension { get; set; }

        public int fileSize { get; set; }

        public string fileSizeUnit { get; set; }

        public byte[] fileData { get; set; }

        public int? companyId { get; set; }

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
    }

    public class CustomerDocumentSearchViewModel
    {
        public string customerName { get; set; }

        public IEnumerable<DocumentUploadViewModel> documents { get; set; }
    }
}