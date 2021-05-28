using FintrakBanking.ViewModels.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class LoanCAMSOLViewModel : GeneralEntity
    {
        
        public int loancamsolid { get; set; }
        public string customercode { get; set; }
        public int? loanid { get; set; }
        public string loanRef { get; set; }
        public decimal balance { get; set; }
        public DateTime date { get; set; }
        public short LOANSYSTEMTYPEID { get; set; }
        public string customername { get; set; }
        public decimal principal { get; set; }
        public decimal interestinsuspense { get; set; }
        public int camsoltypeid { get; set; }
        public string accountnumber { get; set; }
         public string accountname { get; set; }
        public string remark { get; set; }
        public bool cantakeloan { get; set; }
        public string loansystemtype { get; set; }
        public string camsolType { get; set; }
        public bool updateOption { get; set; }
        public string comment { get; set; }
        public short approvalStatusId { get; set; }
        public short tempLoancamsolid { get; set; }
        public string documentTitle { get; set; }
        public byte[] fileData { get; set; }
        public string fileName { get; set; }
        public string fileExtension { get; set; }
        public string message { get; set; }
        public short? BranchId { get; set; }

    }

    public class Blacklist
    {
        public string accountName { get; set; }
        public string accountNumber { get; set; }
        public decimal balance { get; set; }
        public bool canTakeLoan { get; set; }
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public DateTime date { get; set; }
        public decimal InterestInSuspense { get; set; }
        public decimal principal { get; set; }
        public string remark { get; set; }
        public string camsolType { get; set; }
        public string loanSystemType { get; set; }

    }
    public class CamsolDocumentViewModel : DocumentViewModel
    {
        public string customerCode { get; set; }
        public string documentTitle { get; set; }
        public short documentTypeId { get; set; }
        public DateTime SystemDateTime { get; set; }
        public short? branchId { get; set; }
        public string customerName { get; set; }
        public string accountNumber { get; set; }
        public decimal balance { get; set; }
        public string camsolTypeName { get; set; }

    }

    public class CamsolLoanDocumentViewModel
    {
        public int camsolId { get; set; }
        public string customerCode { get; set; }
        public string customerName { get; set; }
        public string accountNumber { get; set; }
        public decimal balance { get; set; }
        public decimal loanPrincipalAmount { get; set; }
        public string camsolTypeName { get; set; }
        public int? signatoryId { get; set; }
        public string signatoryName { get; set; }
        public string signatoryInitials { get; set; }
        public string signatoryTitle { get; set; }
    }

    public class camsolBulkFeedbackViewModel
    {
        public List<LoanCAMSOLViewModel> commitedRows { get; set; }
        public List<LoanCAMSOLViewModel> discardedRows { get; set; }
        public int successCount { get; set; }
        public int failureCount { get; set; }
        public string generalFeedBackMessage { get; set; }
    }

}
