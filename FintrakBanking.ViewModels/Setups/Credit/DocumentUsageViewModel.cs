using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class DocumentUsageViewModel : GeneralEntity
    {
        public int documentUsageId { get; set; }

        public int documentUploadId { get; set; }

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
        public string documentCategory { get; set; }
        public string customerName { get; set; }
        public string fileName { get; set; }
        public string fileType { get; set; }
    }
}