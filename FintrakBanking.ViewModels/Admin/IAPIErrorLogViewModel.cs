using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Admin
{
   public class ErroLogViewModel
    {
        public string username { get; set; }
        public string errorType { get; set; }
        public string errorSource { get; set; }
        public string errorMessage { get; set; }
        public string apendEndPoint { get; set; }
        public string errorPath { get; set; }
        public int? statusCode { get; set; }
        public string allXml { get; set; }
        public DateTime utc { get; set; }
    }
    public class APILogViewModel
    {
        public string apiUrl { get; set; }
        public string logType { get; set; }
        public DateTime requestDateTime { get; set; }
        public DateTime responseDateTime { get; set; }
        public string requestMessage { get; set; }
        public string responseMessage { get; set; }
        public string referenceNumber { get; set; }
        public int companyId { get; set; }
    }
}
