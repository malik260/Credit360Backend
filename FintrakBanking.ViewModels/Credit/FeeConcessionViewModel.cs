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
        public double defaultValue { get; set; }
    }
    public class FeeConcessionTypeViewModel
    {
        public int concessionTypeId { get; set; }
        public string concessionTypeName { get; set; }
    }
    public class LoanFeeChargesViewModel: GeneralEntity
    {
        public int loanChargeFeeId { get; set; }
        public int chargesId { get; set; }
        public string chargesTypeName { get; set; }
        public decimal defaultValue { get; set; }
        public int? loanOperationReviewId { get; set; }
        public IEnumerable<feeDetails> feeDetails { get; set; }
        public string feeSourceModule { get; set; }
    }
    public class feeDetails
    {
        public int? loanChargeFeeId { get; set; }

        public int chargeFeeId { get; set; }
        public string chargeFeeName { get; set; }

        //public int isPosted { get; set; }
        //public int approvalStatusId { get; set; }
        //public decimal feeRateValue { get; set; }
        //public decimal feeDependantAmount { get; set; }
        public decimal feeAmount { get; set; }
        public int? casaAccount { get; set; }
        public string casaAccountName { get; set; }
        public decimal feeRate { get; set; }

        //public decimal earnedFeeAmount { get; set; }
        //public decimal taxAmount { get; set; }
        //public decimal earnedTaxAmount { get; set; }
        //public bool isIntegralFee { get; set; }
        //public bool isRecurring { get; set; }
        //public decimal recurringPaymentDay { get; set; }
        public string description { get; set; }

        public int operationTypeId { get; set; } // remove after refactor
        public string reviewDetails { get; set; }
        //public string operationType { get; set; } // not relevant
        public int loanId { get; set; }
        public int customerId { get; set; }
        public int detailId { get; set; }
        public short loanSystemTypeId { get; set; }
        public int operationId { get; set; }
        //public string loanSystemType { get; set; }
        public string loanSystemTypeName { get; set; }
        public string operationName { get; set; }
        public short productId { get; set; }

        public string obligorName { get; set; }
        public int proposedTenor { get; set; }
        public double proposedRate { get; set; }
        public decimal proposedAmount { get; set; }
        public int approvedTenor { get; set; }
        public double approvedRate { get; set; }
        public decimal approvedAmount { get; set; }
        public decimal? customerProposedAmount { get; set; }

        public string proposedTenorString
        {
            get
            {
                var units = proposedTenor == 1 ? " day" : " days";
                if (proposedTenor < 15) return proposedTenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(proposedTenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }
        public string approvedTenorString
        {
            get
            {
                var units = approvedTenor == 1 ? " day" : " days";
                if (approvedTenor < 15) return approvedTenor.ToString() + units;
                var months = Math.Ceiling((Math.Floor(approvedTenor / 15.00)) / 2);
                units = months == 1 ? " month" : " months";
                return months.ToString() + " " + units;
            }
        }

        public string loanReferenceNumber { get; set; }
    }

}
