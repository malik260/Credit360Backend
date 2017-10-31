using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Finance.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public partial class FinanceRepotObject
    {
        public  List<TransactionViewModel> FinanceTransaction(DateTime? endDate, DateTime? startDate, int? staffId, int companyId, int? branchId, bool excludeSystem)
        {
           
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                IQueryable< TransactionViewModel > data  = (from a in context.tbl_Finance_Transaction
                            where a.CompanyId == companyId && (a.PostedDate <= endDate && a.PostedDate >= startDate) || (a.PostedDate == endDate) ||(  endDate == null || startDate == null  )

                            orderby a.PostedDate, a.TransactionId descending
                            select new TransactionViewModel()
                            {
                                postedByStaffId = a.PostedBy ,
                                branchId = a.SourceBranchId ,
                                branch = a.tbl_Branch.BranchName,
                                batchNo = a.BatchCode,
                                companyName= a.tbl_Company.Name,
                                creditAmount = a.CreditAmount,
                                debitAmount = a.DebitAmount,
                                accountName = a.tbl_Chart_Of_Account.AccountName +"("+ a.tbl_Chart_Of_Account.AccountCode + ")",
                                description = a.Description,
                                valueDate = a.ValueDate,
                                postedDate = a.PostedDate,
                                postedTime = a.PostedDateTime,
                                postedBy = a.PostedBy == -1 ?   SystemStaff.System.ToString()  : a.tbl_Staff.LastName + " " + a.tbl_Staff.FirstName,
                                approvedBy = a.PostedBy == -1 ? SystemStaff.System.ToString() : a.tbl_Staff1.LastName + " " + a.tbl_Staff1.FirstName,
                                postCurrency = a.tbl_Currency.CurrencyName,
                                currencyRate = a.CurrencyRate,
                                approvedDate = a.ApprovedDate,
                                baseCurrency = a.tbl_Company.tbl_Currency.CurrencyName
                                
                            });


                if (branchId!= null)
                {
                    data = data.Where(c => c.branchId ==  branchId);
                }

                if (excludeSystem)
                {
                    data = data.Where(c => c.postedByStaffId != -1);

                }
                

                if (staffId != null)
                {
                    data = data.Where(c => c.postedByStaffId == staffId);
                }
               
                return data.ToList();
            }

        }

    }
}
