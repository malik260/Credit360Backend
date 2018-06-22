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
        public int loansystemtypeid { get; set; }
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
}
