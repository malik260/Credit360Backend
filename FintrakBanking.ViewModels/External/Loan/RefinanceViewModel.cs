using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Loan
{
    public class RefinanceViewModel
    {
        public decimal? TotalAmount { get; set; }
        public string PmbId { get; set; }
        public string PmbName{ get; set; }
        public List<RefinanceDetail> RefinanceDetails { get; set;}
        public int LoanSource { get; set; }
        public string RefinanceBatchNumber{ get; set; }
        public string SecondaryLenderId { get; set; }

    }

    public class RefinanceDetail
    {
        public string Nhfnumber { get; set; }
        public decimal? Amount { get; set; }
        public string LoanId { get; set; }
        public int? Tenor { get; set; }
        public string ProductCode { get; set; }
        public int? ApplicationStatus { get; set; }
        public int? Disbursed { get; set; }
        public int? Rate { get; set; }
        public DateTime? ApplicationDate { get; set; }
        public string CustomerName { get; set; }
        public string RefinanceNumber { get; set; }
        public string CustomerId { get; set; }

    }

    public class LoanDisbursement
    {
        public List<string> RefinanceNumber { get; set; }
    }
}
