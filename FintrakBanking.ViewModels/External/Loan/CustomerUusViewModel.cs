using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Loan
{
    public class CustomerUusViewModel
    {
        public string NhfNumber { get; set; }
        public string Item { get; set; }
        public string Description { get; set; }
        public string PmbId { get; set; }
        public DateTime DeferDate { get; set; }
        public Options Option { get; set; }
        public string FileName { get; set; }        
        public string FileType { get; set; }       
        public string FileContentBase64 { get; set; } 
    }

    public enum Options
    {
        Yes = 1,
        No,
        Waived,
        Defer
    }
}
