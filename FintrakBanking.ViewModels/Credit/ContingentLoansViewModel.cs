using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Common.Enum;
using FintrakBanking.Common;
using FintrakBanking.ViewModels.WorkFlow;

namespace FintrakBanking.ViewModels.Credit
{
    public class ContingentLoansViewModel
    {
        public int contingentLoanId { get; set; }

        public int customerId { get; set; }

        public string customerName { get { return $"{lastName} { firstName} {middleName}"; } }

        public string productName { get; set; }

        public string casaAccountNumber { get; set; }

        public string loanReferenceNumber { get; set; }

        public string loanApplicationReferenceNumber { get; set; }

        public string currencyCode { get; set; }

        public short currencyId { get; set; }

        public double exchangeRate { get; set; }

        public DateTime effectiveDate { get; set; }

        public DateTime maturityDate { get; set; }

        public DateTime bookingDate { get; set; }

        public String contingentAmount { get { return CommonHelpers.FormatNumberTwoPlaces(facilityAmount); } }

        public string loanStatus { get; set; }

        public string principalName { get; set; }

        public decimal usedAmount { get; set; }

        public decimal facilityAmount { get; set; }

        public double percentageUsed { get { return (float)(((float)usedAmount / (float)facilityAmount) * 100); } }

        public decimal amountRemaining { get { return (facilityAmount - usedAmount); } }

        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public short productId { get; set; }
        public int contingentLoanUsageId { get; set; }
        public decimal? amountRequested { get; set; }
        public short loanSystemTypeId { get; set; }
        public string remark { get; set; }
        public int loanReviewApplicationId { get; set; }
        public string operationName { get; set; }
        public DateTime timeIn { get; set; }

        public string timeLapse
        {
            get
            {
                if (timeIn == null) return "n/a";
                int count = (int)Math.Round((DateTime.Now - (DateTime)timeIn).TotalDays);
                string units = count == 1 ? " day" : " days";
                if ((DateTime.Now - (DateTime)timeIn).TotalHours < 24) return timeIn.ToString();
                return count.ToString() + units;
            }
        }

        public string loanApplicationNumber { get; set; }
    }


    public class ContingentLoanUsageViewModel : GeneralEntity
    {

        public int contingentLoanId { get; set; }

        public decimal amountRequested { get; set; }

        public string loanReferenceNumber { get; set; }

        public string productName { get; set; }

        public short productId { get; set; }

        public string remark { get; set; }
        public string fileExtension { get; set; }
        public string fileName { get; set; }
        public string documentTitle { get; set; }
        public int loanReviewApplicationId { get; set; }
    }

    public class ApproveAPSRequestViewModel : ApprovalViewModel
    {
        public int contingenliabilityUsageId { get; set; }       
        public new int productId { get; set; }
        public short userBranchId { get; set; }
        public string loanReferenceNumber { get; set; }
        public decimal amountRequested { get; set; }
    }
}
