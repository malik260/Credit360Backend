using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Entities.Models
{
    [Table("tbl_NmrcRefinancingTranches")]

    public class TblNmrcRefinancingTranches
    {
        public long Id { get; set; }
        public long? LenderId { get; set; }
        public decimal? TotalAmount { get; set; }
        public int? Status { get; set; }
        public DateTime? ApplicationDate { get; set; }
        public int? Tenor { get; set; }
        public int? Rate { get; set; }
        public long? PmbId { get; set; }
        public string ProductCode { get; set; }      
        public int? Disbursed { get; set; }
        public int? IsTranched { get; set; }
        public string TranchNumber { get; set; }
        public string BookingNumber { get; set; }
        public int? IsScheduled { get; set; }
        public int? IsBooked { get; set; }
        public decimal TotalApprovalAmount { get; set; }
        public string PmbName { get; set; }

    }
}

