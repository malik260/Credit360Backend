using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FintrakBanking.Common;
using FintrakBanking.ViewModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;

namespace WinApp
{
    public partial class Form1 : Form
    {
         
        FinTrakBankingContext context = new FinTrakBankingContext();
        private IIntegrationWithFinacle _integration;


        public Form1(FinTrakBankingContext _context, IIntegrationWithFinacle integration )
        {
            InitializeComponent();
           this.context = _context;
            this._integration = integration;
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
            short sectorId = 264;


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
            //credit.ValidateAmountBySector(sectorId);// GetSectorLoanAmountLimit

            //TransactionPosting transaction = new TransactionPosting(context);




            //bool data = false;

            //Task.Run(async () => { data = await transaction.APITransactionPosting(tran); }).GetAwaiter().GetResult();
            //financeTransaction.UpdateCustomTransactions(tran[0].batchCode);
            _integration.GetCustomerAccountBalance("10003981910000");
            MessageBox.Show("Successful", "Fintrak");
        }

        private void button3_Click(object sender, EventArgs e)
        {
           // TransactionPosting tp = new TransactionPosting(context);
           // ResponseMessageViewModel integrationResult = null;
           // var model = new OverDraftNormalViewModel
           // {
           //    accountNumber= "2030562192",
           //     sanctionReferenceNumber= "1234422",
           //     documentDate ="03-04-2018",
           //     sanctionLevel= "003",
           //     sanctionAuthorizer="999",
           //     reviewedDate = "03-05-2018",
           //     sanctionLimit= "50000",
           //     applicationDate= "03-04-2018",
           //     expiryDate="20-06-2099",
           //     sanctionDate ="03-04-2018"
           // };

           //integrationResult =  cwpAIP.OverDraftNormal(model);

        }
 

        private void button1_Click(object sender, EventArgs e)
        {
            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = 1,
            //    STAFFID = 1
            //};

            var trail = new TBL_APPROVAL_TRAIL
            {
                APPROVALTRAILID = -1,
                FROMAPPROVALLEVELID = 31,
                TOAPPROVALLEVELID = 32,
                TARGETID = 104,
                COMPANYID = 1,
                REQUESTSTAFFID = 5221,
                OPERATIONID = 6,
                COMMENT = "test from console",
                ARRIVALDATE = DateTime.Now.Date,
                APPROVALSTATEID = 2,
                APPROVALSTATUSID = 2,
                SYSTEMARRIVALDATETIME = DateTime.Now.Date,
                VOTE = 1,
                TOSTAFFID = 1
            };

            //var loan = new TBL_LOAN
            //{
            //    PRODUCTID = -10,
            //    LOANREFERENCENUMBER = "N/A"

            //};
            FinTrakBankingContext con = new FinTrakBankingContext();

            //con.Configuration.LazyLoadingEnabled = false;

            con.Database.Log = Console.Write;


            //con.TBL_LOAN.Add(loan);

            //con.TBL_AUDIT.Add(audit);

            //var data = con.TBL_APPROVAL_TRAIL.Where(x =>
            //              x.COMPANYID == 1
            //              && x.OPERATIONID == 1
            //              && x.TARGETID == 1
            //              && x.RESPONSESTAFFID == null
            //               && (x.APPROVALSTATEID != 3 && x.RESPONSEDATE == null)
            //              );

            //var info = data.ToList();

            con.TBL_APPROVAL_TRAIL.Add(trail);
            con.SaveChanges() ;
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            List<FinanceTransactionViewModel> tran = new List<FinanceTransactionViewModel>();

            FinanceTransactionViewModel tran1 = new FinanceTransactionViewModel();

            {

                tran1.operationId = 7;
                tran1.sourceReferenceNumber = "C10000";
                tran1.description = "yes";
                tran1.batchCode = "22222";
                tran1.currencyId = 1;
                tran1.casaAccountId = 202236746;
                tran1.debitAmount = 200000;
                tran1.valueDate = DateTime.Now;



            }
            tran.Add(tran1);
            FinanceTransactionViewModel tran2 = new FinanceTransactionViewModel();
            {
                tran2.operationId = 3;
                tran2.sourceReferenceNumber = "D10000";
                tran2.description = "no";
                tran2.batchCode = "22222";
                tran2.currencyId = 1;
                tran2.casaAccountId = 2004169347;
                tran2.creditAmount = 200000;
                tran2.valueDate = DateTime.Now;

            }
            tran.Add(tran2);

            var result = _integration.PostTransactions(tran);
          

            // var result = _integration.GetCustomerByAccountsNumber(textBox2.Text);
            //  "003";
            // "999";

            //DateTime experyDate  =  DateTime.Now.Date.AddMonths(12);

            //DateTime dat = DateTime.Now.Date;
            //var datstr = dat ;
            //var result = _integration.OverDraftNormal(new OverDraftNormalViewModel()
            //{
            //    sanctionReferenceNumber = "12311358",
            //    accountNumber = textBox2.Text, // "1000451805",
            //    applicationDate = datstr.ToShortDateString(),
            //    documentDate = datstr.ToShortDateString(),
            //    expiryDate = experyDate.ToShortDateString(),
            //    reviewedDate = datstr.ToShortDateString(),
            //    sanctionAuthorizer = "999",
            //    sanctionLevel = "003",
            //    sanctionDate = datstr.ToShortDateString(),
            //    sanctionLimit = 100000.ToString()

            //});

            // cwpAIP.ValidateTDAccountNumber("1014010029564");
        }
    }
}
