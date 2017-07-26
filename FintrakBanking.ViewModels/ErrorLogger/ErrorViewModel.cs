using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.ErrorLogger
{
    public class ErrorViewModel
    {

        public int errorLogId { get; set; }
        public string username { get; set; }
        public string errorType { get; set; }
        public string errorSource { get; set; }
        public string errorMessage { get; set; }
        public string apiEndpoint { get; set; }
        public string errorPath { get; set; }
        public int? statusCode { get; set; }
        public DateTime timeUtc { get; set; }
        public string allXml { get; set; }
    }
}
