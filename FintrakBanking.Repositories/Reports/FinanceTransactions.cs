using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Reports;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Reports;
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


    }
}
