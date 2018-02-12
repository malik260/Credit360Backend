
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
        public int SourceId { get; set; }

        //Other references
        public int customerCreditBureauId { get; set; }
    }

}