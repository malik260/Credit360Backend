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

namespace WinApp
{
    public partial class Form1 : Form
    {
        private IAuditTrailRepository auditTrail;
        IGeneralSetupRepository genSetup;
        IFinanceTransactionRepository financeTransaction;
        ILoanScheduleRepository loanSchedule;
        FinTrakBankingContext context;
        ILoanOperationsRepository loanOperation;
        public Form1(FinTrakBankingContext _context , IGeneralSetupRepository _genSetup, IFinanceTransactionRepository _financeTransaction,
            ILoanScheduleRepository _loanSchedule, 
            IAuditTrailRepository _auditTrail, ILoanOperationsRepository _loanOperation )
        //IGeneralSetupRepository _genSetup )
        {
            InitializeComponent();
            this.context = _context;
            this.auditTrail = _auditTrail;
            this.genSetup = _genSetup;
            this.loanSchedule = _loanSchedule;
            this.financeTransaction = _financeTransaction;
            this.loanOperation = _loanOperation;

        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            //LoanOperationsRepository  data = new LoanOperationsRepository (context,genSetup, financeTransaction, auditTrail, loanSchedule);
            //PublicHolidayViewModel model  = new PublicHolidayViewModel();

            //tbl_Public_Holiday model = new tbl_Public_Holiday();

            //decimal vCountry =  NunCountry.Value;
            DateTime vDate = dtpDate.Value;
            int vStaff = 1;
            int vLoan  = 21;
            decimal vAmount  = NunCountry.Value;
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
            //loanOperation.GetDailyTeamLoansInterestAccrual(vDate);
            //loanOperation.BuildLoanRepaymentPostingForceDebit(vDate);
            //loanOperation.BuildLoanRepaymentPostingPastDue(vDate);
            //loanOperation.GetDailyPastDueInterestAccrual(vDate);
            loanOperation.InterestSuspension(vLoan, vDate, vStaff);

            MessageBox.Show("Successful", "Fintrak");


        }

    }
}
