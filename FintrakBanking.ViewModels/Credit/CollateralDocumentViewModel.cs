
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

        public int collateralCustomerId { get; set; }
        public string visitationRemark { get; set; }
        public DateTime lastVisitaionDate { get; set; }
        public int CollateralVisitationID { get; set; }

    }
}