using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Media
{
    public class DocumentArchiveViewModel : GeneralEntity
    {
        public int documentArchiveId { get; set; }

        public int? documentId { get; set; }

        public string customerCode { get; set; }

        public int? targetId { get; set; }

        public string targetCode { get; set; }

        public string targetReferenceNumber { get; set; }

        public string documentCode { get; set; }

        public string documentTitle { get; set; }

        public string physicalFileNumber { get; set; }

        public string physicalLocation { get; set; }

        public DateTime? expiryDate { get; set; }

        public int? documentStatusId { get; set; }

        public bool isPrimaryDocument { get; set; }

        public int? documentCategoryId { get; set; }

        public int? documentTypeId { get; set; }

        public string fileName { get; set; }

        public string fileExtension { get; set; }

        public int? fileSize { get; set; }

        public string fileSizeUnit { get; set; }

        public string fileData  { get; set; }

        public new int? companyId { get; set; }

        public int? approvalStatusId { get; set; }

    }
}