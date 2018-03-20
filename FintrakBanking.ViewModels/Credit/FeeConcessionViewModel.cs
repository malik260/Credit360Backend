using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class FeeConcessionViewModel : GeneralEntity
    {
        public int concessionId { get; set; }
        public int concessionTypeId { get; set; }
        public string concessionTypeName { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int? loanChargeFeeId { get; set; }
        public string loanChargeFeeName { get; set; }
        public string concessionReason { get; set; }
        public double concession { get; set; }
        public int approvalStatusId { get; set; }
        public string loanRefNo { get; set; }
        public string approvalStatus { get; set; }
        public decimal? defaultValue { get; set; }
    }
    public class FeeConcessionTypeViewModel
    {
        public int concessionTypeId { get; set; }
        public string concessionTypeName { get; set; }
    }
    public class LoanFeeChargesViewModel
    {
        public int loanChargeFeeId { get; set; }
        public int chargesId { get; set; }
        public string chargesTypeName { get; set; }
        public decimal defaultValue { get; set; }
    }
}
