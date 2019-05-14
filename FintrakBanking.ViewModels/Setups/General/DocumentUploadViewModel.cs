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

        public string fileSize { get; set; }

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

    }
}