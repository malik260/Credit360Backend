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
                var data = from a in context.tbl_Finance_Transaction
                           where (a.PostedDate >= dateItem.startDate  && a.PostedDate <=  dateItem.endDate)
                           && a.CompanyId == companyId
                           select new
                           {
                               staffId = a.tbl_Staff.StaffId,
                               staffName = a.tbl_Staff.LastName + " " + a.tbl_Staff.FirstName,
                      

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
                var data = from a in context.tbl_Finance_Transaction
                           where (a.PostedDate >= dateItem.startDate && a.PostedDate <= dateItem.endDate)
                           && a.CompanyId == companyId
                           select new
                           {
                               
                               branchId = a.tbl_Branch.BranchId,
                               branchName = a.tbl_Branch.BranchName

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
