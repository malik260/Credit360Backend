using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("tbl_NmrcRefinancingLoans")]

    public partial class TblNmrcRefinancingLoan
    {
        public long Id { get; set; }
        public long? LenderId { get; set; }
        public string Nhfnumber { get; set; }
        public string RefinanceNumber { get; set; }
        public decimal? Amount { get; set; }
        public decimal? TotalAmount { get; set; }
        public string LoanId { get; set; }
        public int? Status { get; set; }
        public int? Approved { get; set; }
        public DateTime? ApplicationDate { get; set; }
        public int? Tenor { get; set; }
        public int? Rate { get; set; }
        public long? PmbId { get; set; }
        public string ProductCode { get; set; }
        public int? ApplicationStatus { get; set; }
        public int? Reviewed { get; set; }
        public int? Checklisted { get; set; }
        public int? Disbursed { get; set; }
        public string CustomerName { get; set; }
        public int? ReviewalStatus { get; set; }
        public int? ApprovalStatus { get; set; }
    }
}
