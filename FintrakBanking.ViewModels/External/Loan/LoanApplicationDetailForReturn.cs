using FintrakBanking.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.External.Loan
{
    public class LoanApplicationDetailForReturn
    {
        public decimal amount { get; set; }
        public short productId { get; set; }
        public string productName { get; set; }
        public int approvedTenor { get; set; }
        public string customerName { get; set; }
        public string nhfAccount { get; set; }
        //public int? tenorModeId { get; set; }
        //public int? tenorFrequencyTypeId
        //{
        //    get
        //    {
        //        return tenorModeId == null ? (int?)TenorMode.Daily : tenorModeId; // default to days
        //    }
        //}
    }
}
