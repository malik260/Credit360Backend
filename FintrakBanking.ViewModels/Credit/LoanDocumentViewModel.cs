
using System;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanDocumentViewModel : GeneralEntity
    {
        public int documentId { get; set; }
        public string loanApplicationNumber { get; set; }
        public string loanReferenceNumber { get; set; }
        public string documentTitle { get; set; }
        public short documentTypeId { get; set; }
        public byte[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public DateTime systemDateTime { get; set; }
        public string physicalFileNumber { get; set; }
        public string physicalLocation { get; set; }
        public int? SourceId { get; set; }
        public int? databaseTable { get; set; }
        public int? conditionId { get; set; }
        public int? loanApplicationId { get; set; }
        public int? checkListDefinitionId { get; set; }
        public int? loanDetailId { get; set; }
        public int? collateralCustomerId { get; set; }
        public string documentCode { get; set; }
        public string jobRequestCode { get; set; }
        public string customerCode { get; set; }
        public int? customerId { get; set; }
        public string staffCode { get; set; }

        public int? operationReviewId { get; set; }

        //Other references
        public int customerCreditBureauId { get; set; }
        public bool isPrimaryDocument { get; set; }
        public bool overwrite { get; set; }
        
    }

    public class OperationDocumentationViewModel : LoanDocumentViewModel
    {
        public int targetId { get; set; }
        public int operationId { get; set; }
        public int operationDocumentationId { get; set; }
        public string customerName { get; set; }
        public string operationName { get; set; }
    }
}