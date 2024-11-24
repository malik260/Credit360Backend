using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Document
{
    public class LoanUploadedDocumentForReturn
    {
        //public int documentId { get; set; }
        public string loanReferenceNumber { get; set; }
        //public int customerId { get; set; }
        public string documentTitle { get; set; }
        //public short documentTypeId { get; set; }
        public byte[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public DateTime systemDateTime { get; set; }
        //public string physicalFileNumber { get; set; }
        //public string physicalLocation { get; set; }
    }
}
