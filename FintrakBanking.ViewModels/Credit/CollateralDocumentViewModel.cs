
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
        public DateTime lastVisitaionDate { get; set; }
        public DateTime? nextVisitationDate { get; set; }
        public string visitationRemark { get; set; }
        public int? CollateralVisitationID { get; set; }
        public bool isPrimaryDocument { get; set; }
        public int? targetId { get; set; }
        public int? documentTypeId { get; set; }
        public string documentType { get; set; }
        public string collateralCode { get; set; }
        public decimal ContingentAmount { get; set; }
        public int? collateralReleaseStatusId { get; set; }
        public string collateralReleaseStatusName { get; set; }
        public string doneBy { get; set; }
        public string collateralSummary { get; set; }
        public string isPrimaryDocumentValue { get; set; }
    }

    public class CollateralVisitationDocumentViewModel : GeneralEntity
    {
        public int documentId { get; set; }
        public int collateralId { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public byte[] fileData { get; set; }
        public string documentTitle { get; set; }
        public int? collateralCustomerId { get; set; }
        public DateTime lastVisitaionDate { get; set; }
        public string visitationRemark { get; set; }
        public int? CollateralVisitationID { get; set; }
    }
}