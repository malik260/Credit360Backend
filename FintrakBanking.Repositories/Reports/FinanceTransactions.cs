using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Finance.ViewModels;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.ViewModels.Reports;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Reports
{
   public  class FinanceTransactionsReport: IFinanceTransactionsReport
    {
        private FinTrakBankingContext context;

        public FinanceTransactionsReport(FinTrakBankingContext _context) {
            this.context = _context;

        }



      
        public IEnumerable<dynamic> PostTransactionsByStaffByDate(DateRange dateItem, int companyId)  
        {
            try
            {
                var data = from a in context.TBL_FINANCE_TRANSACTION
                           where (a.POSTEDDATE >= dateItem.startDate  && a.POSTEDDATE <=  dateItem.endDate)
                           && a.COMPANYID == companyId
                           select new
                           {
                               staffId = a.TBL_STAFF.STAFFID,
                               staffName = a.TBL_STAFF.LASTNAME + " " + a.TBL_STAFF.FIRSTNAME,
                      

                           };
                return data.ToList().Distinct();
            }
            catch (Exception ex )
            {

                throw;
            }
           

        }
        public IEnumerable<dynamic> PostTransactionsByBranchByDate(DateRange dateItem, int companyId)
        {
            try
            {
                var data = from a in context.TBL_FINANCE_TRANSACTION
                           where (a.POSTEDDATE >= dateItem.startDate && a.POSTEDDATE <= dateItem.endDate)
                           && a.COMPANYID == companyId
                           select new
                           {
                               
                               branchId = a.TBL_BRANCH.BRANCHID,
                               branchName = a.TBL_BRANCH.BRANCHNAME

                           };
                return data.ToList().Distinct();
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public List<DailyAccrualViewModel> GetAllLoanTransactionType()
        {
            try
            {
                var data = from a in context.TBL_LOAN_TRANSACTION_TYPE
                           
                           select new DailyAccrualViewModel
                           {
                               transactionTypeId = a.TRANSACTIONTYPEID,
                               transactionTypeName = a.TRANSACTIONTYPENAME
                           };
                return data.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public List<DailyAccrualViewModel> GetAllDailyAccrualCategories()
        {
            try
            {
                var data = from a in context.TBL_DAILY_ACCRUAL_CATEGORY

                           select new DailyAccrualViewModel
                           {
                               categoryId = a.CATEGORYID,
                               categoryName = a.CATEGORYNAME
                           };
                return data.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public List<LoanOperationTypeViewModel> Operations()
        {
            List<LoanOperationTypeViewModel> operationList = new List<LoanOperationTypeViewModel>();

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var operations = (from x in context.TBL_OPERATIONS
                                  select x).ToList();
               foreach(var x in operations)
                {
                    if (x.OPERATIONID== (int)OperationsEnum.InterestLoanRepayment || x.OPERATIONID==(int)OperationsEnum.InterestPastDueLoanRepayment || x.OPERATIONID==(int)OperationsEnum.PrincipalLoanRepayment || x.OPERATIONID== (int)OperationsEnum.PrincipalPastDueLoanRepayment)
                    {
                        operationList.Add(new LoanOperationTypeViewModel {
                            operationTypeId = x.OPERATIONID,
                            operationTypeName = x.OPERATIONNAME
                        });
                    }
                }
                
                return operationList;
            }
        }
    }
}
