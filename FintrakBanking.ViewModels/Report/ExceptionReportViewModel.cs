using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Report
{
   public class ExceptionReportViewModel
    {
        public string branchCode { get; set; }
        public string branchName { get; set; }
        public string SBU { get; set; }
        public string GRP { get; set; }
        public string team { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public string customerCode { get; set; }
        public string currencyCode { get; set; }
        public string sanctionLimit { get; set; }
        public string utilizedAmount { get; set; }
        public string facilityGrantedDate { get; set; }
        public string limitExpirationDate { get; set; }
        public string LCMaturity { get; set; }
        public string LCEstablishmentDate { get; set; }
        public string LCReferenceNumber { get; set; }
        public string formMNUmber { get; set; }
        public string exceptionAmount { get; set; }
        public string unauthorized { get; set; }
        public string overDueObiligation { get; set; }
        public string daysPastDue { get; set; }
    }
}
