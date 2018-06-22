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
        public  List<TransactionViewModel> FinanceTransaction(DateTime endDate, DateTime startDate, int? staffId, int companyId, int? branchId, bool excludeSystem)
        { 
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var staffSensitivityLevelId = context.TBL_STAFF.Find(staffId).CUSTOMERSENSITIVITYLEVELID;
                IQueryable< TransactionViewModel > data  = (from a in context.TBL_FINANCE_TRANSACTION
                            where a.COMPANYID == companyId 
                            && (a.POSTEDDATE <= endDate && a.POSTEDDATE >= startDate)  
                            && a.TBL_CASA.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID == staffSensitivityLevelId
                            && branchId==0
                            || a.COMPANYID == companyId
                            && (a.POSTEDDATE <= endDate && a.POSTEDDATE >= startDate)
                            && a.TBL_CASA.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID == staffSensitivityLevelId
                            && a.TBL_BRANCH.BRANCHID == branchId
                                                            orderby a.POSTEDDATE, a.TRANSACTIONID descending
                            select new TransactionViewModel()
                            {
                                postedByStaffId = a.POSTEDBY ,
                                branchId = a.SOURCEBRANCHID ,
                                branch = a.TBL_BRANCH.BRANCHNAME,
                                batchNo = a.BATCHCODE,
                                companyName= a.TBL_COMPANY.NAME,
                                creditAmount = a.CREDITAMOUNT,
                                debitAmount = a.DEBITAMOUNT,
                                accountName = a.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME +"("+ a.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE + ")",
                                description = a.DESCRIPTION,
                                valueDate = a.VALUEDATE,
                                postedDate = a.POSTEDDATE,
                                postedTime = a.POSTEDDATETIME,
                                postedBy = a.POSTEDBY == -1 ? SystemStaff.System.ToString() : a.TBL_STAFF.LASTNAME + " " + a.TBL_STAFF.FIRSTNAME,
                                approvedBy = a.POSTEDBY == -1 ? SystemStaff.System.ToString() : a.TBL_STAFF1.LASTNAME + " " + a.TBL_STAFF1.FIRSTNAME,
                                postCurrency = a.TBL_CURRENCY.CURRENCYNAME,
                                currencyRate = a.CURRENCYRATE,
                                approvedDate = a.APPROVEDDATE,
                                baseCurrency = a.TBL_COMPANY.TBL_CURRENCY.CURRENCYNAME
                                
                            });


                if (branchId!= null && branchId != 0)
                {
                    data = data.Where(c => c.branchId ==  branchId);
                }

                if (excludeSystem)
                {
                    data = data.Where(c => c.postedByStaffId != -1);

                }
                

                if (staffId != null && staffId != 0)
                {
                    data = data.Where(c => c.postedByStaffId == staffId);
                }
               
                return data.ToList();
            }

        }

    }
}
