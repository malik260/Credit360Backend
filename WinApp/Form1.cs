using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FintrakBanking.Common;
using FintrakBanking.Entities;
using FintrakBanking.Interfaces;
using FintrakBanking.Repositories;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.Entities.Models;
using FintrakBanking.Repositories.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Repositories.Credit;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.CreditLimitValidations;
using FinTrakBanking.ThirdPartyIntegration.Finacle;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;

namespace WinApp
{
    public partial class Form1 : Form
    {
        private IAuditTrailRepository auditTrail;
        IGeneralSetupRepository genSetup;
        IFinanceTransactionRepository financeTransaction;
        ILoanScheduleRepository loanSchedule;
        FinTrakBankingContext context = new FinTrakBankingContext();
        ILoanOperationsRepository loanOperation;
        ICustomerFSRatioRepository cust;
        ILoanRepository loan;
        ICreditLimitValidationsRepository credit;
        ICustomerStagingRepository customer;
        IIntegrationWithCWGAPI cwpAIP;
        public Form1(FinTrakBankingContext _context , IGeneralSetupRepository _genSetup, IFinanceTransactionRepository _financeTransaction,
            ILoanScheduleRepository _loanSchedule, ICustomerFSRatioRepository _cust, ILoanRepository _loan,
            IAuditTrailRepository _auditTrail, ILoanOperationsRepository _loanOperation , ICreditLimitValidationsRepository _credit,
            ICustomerStagingRepository _customer, IIntegrationWithCWGAPI _cwpAIP)
        //IGeneralSetupRepository _genSetup )
        {
            InitializeComponent();
           this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            this.loanSchedule = _loanSchedule;
            this.financeTransaction = _financeTransaction;
            this.loanOperation = _loanOperation;
            this.cust = _cust;
            this.loan = _loan;
            credit = _credit;
            customer = _customer;
            cwpAIP = _cwpAIP;

        }

        //public class FinanceTransactionViewModel  : GeneralEntity
        //{
        //    public string accounts { get; set; }
        //    public string description { get; set; }
        //    public string batchCode { get; set; }
        //    public string currencyType { get; set; }
        //    public string amount { get; set; }
        //    public string webRequestStatus { get; set; }
        //    public DateTime webRequestDate { get; set; }
        //    public string responseCode { get; set; }
        //    public int casaAccountId { get; set; }
        //    public int currencyId { get; set; }
        //    public decimal creditAmount { get; set; }




        //}

        //public class FinanceTransactionDetailViewModel
        //{
        //    public int transactionId { get; set; }
        //    public int glAccountId { get; set; }
        //    public string sourceReferenceNumber { get; set; }
        //    public int? casaAccountId { get; set; }
        //    public decimal debitAmount { get; set; }
        //    public decimal creditAmount { get; set; }
        //    public short sourceBranchId { get; set; }
        //    public short destinationBranchId { get; set; }
        //}


        public class FinanceTransactionViewModelTest : GeneralEntity
        {
            //public FinanceTransactionViewModel()
            //{
            //    transactionDetails = new List<FinanceTransactionDetailViewModel>();
            //}

            public string batchCode { get; set; }
            public int operationId { get; set; }
            public string description { get; set; }
            public DateTime valueDate { get; set; }
            public DateTime transactionDate { get; set; }
            public short currencyId { get; set; }
            public double currencyRate { get; set; }
            public DateTime postedDateTime { get; set; }
            public bool isApproved { get; set; }
            public int postedBy { get; set; }
            public int approvedBy { get; set; }
            public DateTime approvedDate { get; set; }
            public DateTime approvedDateTime { get; set; }
            public short sourceApplicationId { get; set; }
            public int transactionId { get; set; }
            public int glAccountId { get; set; }
            public string sourceReferenceNumber { get; set; }
            public int? casaAccountId { get; set; }
            public decimal debitAmount { get; set; }
            public decimal creditAmount { get; set; }
            public short sourceBranchId { get; set; }
            public short destinationBranchId { get; set; }
            //public List<FinanceTransactionDetailViewModel> transactionDetails { get; set; }

        }

        //public class FinanceTransactionDetailViewModel
        //{
        //    public int transactionId { get; set; }
        //    public int glAccountId { get; set; }
        //    public string sourceReferenceNumber { get; set; }
        //    public int? casaAccountId { get; set; }
        //    public decimal debitAmount { get; set; }
        //    public decimal creditAmount { get; set; }
        //    public short sourceBranchId { get; set; }
        //    public short destinationBranchId { get; set; }
        //}

        private void btnSave_Click(object sender, EventArgs e)
        {

            //LoanOperationsRepository  data = new LoanOperationsRepository (context,genSetup, financeTransaction, auditTrail, loanSchedule);
            //PublicHolidayViewModel model  = new PublicHolidayViewModel();

            //tbl_Public_Holiday model = new tbl_Public_Holiday();

            //decimal vCountry =  NunCountry.Value;
            // DateTime vDate = dtpDate.Value;
            // int vStaff = 1;
            //// int vLoan  = 215;
            // decimal vAmount  = NunCountry.Value;
            // short vReview  = 19;

            short priceIndex = 1;
            double newRate = 20;
            int customeId = 1;

            //List<FinanceTransactionViewModel> tran = new List<FinanceTransactionViewModel>();

            //FinanceTransactionViewModel tran1 = new FinanceTransactionViewModel();
           
            //{

            //        tran1.operationId = 7;
            //        tran1.sourceReferenceNumber = "C10000";
            //        tran1.description = "yes";
            //        tran1.batchCode = "22222";
            //        tran1.currencyId = 1;
            //        tran1.casaAccountId = 202236746;
            //        tran1.debitAmount = 200000;




            //}
            //    tran.Add(tran1);
            //FinanceTransactionViewModel tran2 = new FinanceTransactionViewModel();
            //{
            //        tran2.operationId = 3;
            //        tran2.sourceReferenceNumber = "D10000";
            //        tran2.description = "no";
            //        tran2.batchCode = "22222";
            //        tran2.currencyId = 1;
            //        tran2.casaAccountId = 2004169347;
            //    tran2.creditAmount = 200000;


            //}
            //    tran.Add(tran2);


            //string vDesc = txtDesc.Text;

            //model.CountryId = 1;//(int)vCountry;
            //model.Date = vDate;
            //model.Description = vDesc;

            //this.context.tbl_Public_Holiday.Add(model);
            //var response = context.SaveChanges();
            //data.AddPublicHoliday(model);
            //loanOperation.LoanCancellation(vLoan, vDate, vStaff);
            //loanOperation.OverdraftTopUp(vLoan, vAmount);

            //loanOperation.NPLByBranchSuspension();
            //DateTime date = DateTime.Now.Date;
            //date.ToString("yyyy-MM-dd") = vDate.ToString("yyyy-MM-dd");
            //loanOperation.ProcessDailyTeamLoansInterestAccrual(vDate);
            //loanOperation.BuildLoanRepaymentPostingForceDebit(vDate);
            //loanOperation.BuildLoanRepaymentPostingPastDue(vDate);
            //loanOperation.GetDailyPastDueInterestAccrual(vDate);
            //loanOperation.InterestSuspension(vLoan, vDate, vStaff);
            //loanOperation.ArchiveLoan(vLoan, vStaff);
            //loanOperation.BulkArchiveLoan(vStaff);
            //loanOperation.ArchivePeriodicSchedule(vLoan);
            //loanOperation.ArchiveDailySchedule(vLoan);
            //loanOperation.UpdatePeriodicSchedule(vLoan, vDate);
            //loanOperation.LoanRephasementProcess(vReview,vLoan, vStaff);
            //loanOperation.ProcessDailyAuthorisedOverdraftInterestAccrual(vDate);
            //loanOperation.ProcessDailyUnauthorisedOverdraftInterestAccrual(vDate);
            //loanOperation.ProcessLoanRepaymentPostingPastDue(vDate);
            //loanOperation.ProcessLoanRepaymentPostingForceDebit(vDate);
            //loanOperation.ProcessAuthorisedOverdraftRepaymentPostingForceDebit(vDate);
            //loanOperation.BulkRateReview(priceIndex,newRate,vDate,vStaff,vReview);
            //loanOperation.CalLoanClassification(vDate);

            //cust.GetCustomerFSRatioValues(customeId);
            //loan.GetCustomerTotalOutstandingBalance(customeId);
            //credit.ValidateAmountByCustomer(customeId);
            //customer.GetIntegratedCustomerInformation("1000021211");
            //CustomerDetails ccc = new CustomerDetails(context);
            //ccc.GetCustomerByAccountNumber("1000021211").GetAwaiter().GetResult();
            // Task.Run(async () => { await SubscribeMembersusingAPI(list); }).GetAwaiter().GetResult();
            //Task.Run(async() => { await ccc.RunAsync();}).GetAwaiter().GetResult();
            // Task.Run(async () => { await ccc.GetAllCustomers(); }).GetAwaiter().GetResult();
            //Task.Run(async () => { await ccc.GetCustomerByAccountsNumber("1000021211"); }).GetAwaiter().GetResult();
            //ccc.RunAsync().GetAwaiter().GetResult();
            //ccc.GetAllCustomers().GetAwaiter().GetResult();
            //loan.AddLoanTestFees();
            // TransactionPosting transaction = new TransactionPosting(context);




            //bool data = false;

            //Task.Run(async () => { data = await transaction.APITransactionPosting(tran); }).GetAwaiter().GetResult();
            //financeTransaction.UpdateCustomTransactions(tran[0].batchCode);
            //MessageBox.Show("Successful", "Fintrak");
            credit.ValidateAmountBySector(264);
        }

        //private void button3_Click(object sender, EventArgs e)
        //{
        //    TransactionPosting tp = new TransactionPosting(context);
        //    ResponseMessageViewModel integrationResult = null;
        //    var model = new OverDraftExtendViewModel
        //    {
        //       accountNumber= "2030562192",
        //        sanctionReferenceNumber= "1234422",
        //        //documentDate ="03-04-2018",
        //        //sanctionLevel= "003",
        //        //sanctionAuthorizer="999",
        //        //reviewedDate = "03-05-2018",
        //        sanctionLimit= "50000",
        //        //applicationDate= "03-04-2018",
        //        expiryDate="20-06-2099",
        //        //sanctionDate ="03-04-2018"
        //    };

        //    bool result =  false;

        //    result = cwpAIP.GetExposePersonStatus("1000451874");

        // //  integrationResult =  cwpAIP.OverDraftExtend(model);

        //}
    }
}
