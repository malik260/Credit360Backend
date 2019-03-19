using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class Form3800BReportViewModel
    {
        public DateTime? date { get; set; }
        public string operativeAccount { get; set; }
        public string businessUnit { get; set; }
        public string businessGroup { get; set; }
        public string branch { get; set; }
        public string customer { get; set; }
        public string cap { get; set; }
        public string status { get; set; }
        public string purpose { get; set; }
        public decimal newApproval { get; set; }
        public string currency { get; set; }
        public string staffCode { get; set; }
        public DateTime systemDate { get; set; }
    }

}
