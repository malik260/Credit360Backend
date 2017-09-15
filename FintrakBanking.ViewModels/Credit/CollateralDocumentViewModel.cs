
using System;

namespace FintrakBanking.ViewModels.Credit
{
    public class CollateralDocumentViewModel : GeneralEntity
    {
        public int documentId { get; set; }
        public int collateralId { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public byte[] fileData { get; set; }
        public string documentTitle { get; set; }
    }
}