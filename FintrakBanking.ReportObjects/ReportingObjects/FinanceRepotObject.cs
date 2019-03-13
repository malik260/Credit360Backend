using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Finance.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FinTrakBanking.ThirdPartyIntegration.Finacle.CWGAPI;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public partial class FinanceRepotObject
    {
        public List<TransactionViewModel> FinanceTransaction(DateTime startDate, DateTime endDate,  int companyId, int? branchId,int glAccountId,int? PostedByStaffId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                IQueryable<TransactionViewModel> data = (from a in context.TBL_FINANCE_TRANSACTION
                                                         where a.COMPANYID == companyId
                                                         && (DbFunctions.TruncateTime(a.POSTEDDATE) <= DbFunctions.TruncateTime(endDate) && DbFunctions.TruncateTime(a.POSTEDDATE) >= DbFunctions.TruncateTime(startDate))
                                                         && (branchId == null || branchId==0 || a.TBL_BRANCH.BRANCHID==branchId)
                                                         && (a.GLACCOUNTID == glAccountId || glAccountId==0)
                                                         && (a.POSTEDBY == PostedByStaffId || PostedByStaffId ==0)
                                                         orderby a.POSTEDDATE, a.TRANSACTIONID descending
                                                         select new TransactionViewModel()
                                                         {
                                                             postedByStaffId = a.POSTEDBY,
                                                             branchId = a.SOURCEBRANCHID,
                                                             branch = a.TBL_BRANCH.BRANCHNAME,
                                                             branchCode = a.TBL_BRANCH.BRANCHCODE,
                                                             batchNo = a.BATCHCODE,
                                                             batchNo2 = a.BATCHCODE2,
                                                             companyName = a.TBL_COMPANY.NAME,
                                                             creditAmount = a.CREDITAMOUNT,
                                                             debitAmount = a.DEBITAMOUNT,
                                                             accountName = context.TBL_CHART_OF_ACCOUNT.Where(x=>x.GLACCOUNTID==a.GLACCOUNTID).Select(x=>x.ACCOUNTNAME).FirstOrDefault(),
                                                             GLAccountCode = context.TBL_CHART_OF_ACCOUNT.Where(x=>x.GLACCOUNTID==a.GLACCOUNTID).Select(x=>x.ACCOUNTCODE).FirstOrDefault(),
                                                             description = a.DESCRIPTION,
                                                             valueDate = a.VALUEDATE,
                                                             postedDate = a.POSTEDDATE,
                                                             postedTime = a.POSTEDDATETIME,
                                                             postedBy = context.TBL_STAFF.Where(x => x.STAFFID == a.POSTEDBY).Select(x => x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault(),// ? SystemStaff.System.ToString() : a.TBL_STAFF.LASTNAME + " " + a.TBL_STAFF.FIRSTNAME,
                                                             postCurrency = a.TBL_CURRENCY.CURRENCYNAME,
                                                             currencyRate = a.CURRENCYRATE,
                                                             approvedDate = a.APPROVEDDATE,
                                                             baseCurrency = a.TBL_COMPANY.TBL_CURRENCY.CURRENCYNAME,
                                                             

                                                         });


                return data.ToList();
                }

        }
        public List<TransactionViewModel> LoanRepayment( DateTime startDate, DateTime endDate, int operationId, int companyId)
        {
           int[] operations = { (int)OperationsEnum.InterestLoanRepayment, (int)OperationsEnum.PrincipalPastDueLoanRepayment, (int)OperationsEnum.InterestPastDueLoanRepayment, (int)OperationsEnum.PrincipalLoanRepayment, (int)OperationsEnum.TermLoanBooking };
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                List<TransactionViewModel> data = (from a in context.TBL_FINANCE_TRANSACTION
                                                   join l in context.TBL_LOAN on a.SOURCEREFERENCENUMBER equals l.LOANREFERENCENUMBER
                                                   join p in context.TBL_PRODUCT on l.PRODUCTID equals p.PRODUCTID
                                                   join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                                   
                                                   where DbFunctions.TruncateTime(a.POSTEDDATE) >= DbFunctions.TruncateTime(startDate)
                                                    && DbFunctions.TruncateTime(a.POSTEDDATE) <= DbFunctions.TruncateTime(endDate)
                                                                           && a.COMPANYID == companyId
                                                                            && a.CREDITAMOUNT == 0
                                                                            && a.DEBITAMOUNT > 0
                                                                          && operations.Contains(a.OPERATIONID)
                                                   orderby a.POSTEDDATE, a.TRANSACTIONID descending

                                                   select new TransactionViewModel()
                                                   {
                                                       postedByStaffId = a.POSTEDBY,
                                                       branchId = a.SOURCEBRANCHID,
                                                       branch = a.TBL_BRANCH.BRANCHNAME,
                                                       batchNo = a.BATCHCODE,
                                                       companyName = a.TBL_COMPANY.NAME,
                                                       creditAmount = a.CREDITAMOUNT,
                                                       debitAmount = a.DEBITAMOUNT,
                                                       accountName = a.TBL_CHART_OF_ACCOUNT.ACCOUNTNAME + "(" + a.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE + ")",
                                                       description = a.DESCRIPTION,
                                                       valueDate = a.VALUEDATE,
                                                       postedDate = a.POSTEDDATE,
                                                       postedTime = a.POSTEDDATETIME,
                                                       postCurrency = a.TBL_CURRENCY.CURRENCYNAME,
                                                       currencyRate = a.CURRENCYRATE,
                                                       approvedDate = a.APPROVEDDATE,
                                                       baseCurrency = a.TBL_COMPANY.TBL_CURRENCY.CURRENCYNAME,
                                                       casaAccountNumber = context.TBL_CASA.Where(x => x.CASAACCOUNTID == a.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                       branchName = context.TBL_BRANCH.Where(x => x.BRANCHID == a.SOURCEBRANCHID).Select(x => x.BRANCHNAME).FirstOrDefault(),
                                                       productCode = p.PRODUCTCODE,
                                                       outstandingBalance = l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL,
                                                       productName = p.PRODUCTNAME,
                                                       customerCode = c.CUSTOMERCODE,
                                                       customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,
                                                       branchCode = context.TBL_BRANCH.Where(x => x.BRANCHID == l.BRANCHID).Select(x => x.BRANCHCODE).FirstOrDefault(),
                                                       sourceReferenceNumber = a.SOURCEREFERENCENUMBER,
                                                       operationName = context.TBL_OPERATIONS.Where(o=>o.OPERATIONID==a.OPERATIONID).Select(o=>o.OPERATIONNAME).FirstOrDefault(),
                                                       transactionID = a.TRANSACTIONID
                                                       
                                                      
                                                   }).ToList();

                
                return data;
            }

        }
        public List<DailyAccrualViewModel> DailyAccrual(DateTime endDate, DateTime startDate, int companyId, string searchParamemter)// int? transactionTypeId
        {
            List<DailyAccrualViewModel> data;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                data = (from a in context.TBL_DAILY_ACCRUAL
                        join p in context.TBL_PRODUCT on a.PRODUCTID equals p.PRODUCTID
                        join l in context.TBL_LOAN on a.REFERENCENUMBER equals l.LOANREFERENCENUMBER
                        join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                        where DbFunctions.TruncateTime(a.DATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(a.DATE) <= DbFunctions.TruncateTime(endDate)
                        && a.COMPANYID == companyId
                        && (a.REFERENCENUMBER.ToLower() ==searchParamemter.ToLower() 
                        || a.REFERENCENUMBER.ToLower().Contains( searchParamemter.ToLower()) 
                        || a.REFERENCENUMBER.ToLower().StartsWith(searchParamemter.ToLower()) 
                        || searchParamemter == "")//(searchParamemter.ToLower().Trim().Contains(a.REFERENCENUMBER.ToLower()) || searchParamemter =="")
                        orderby a.DAILYACCURALID descending
                        select new DailyAccrualViewModel()
                        {
                            baseReferenceNumber = a.BASEREFERENCENUMBER,
                            branchName = context.TBL_BRANCH.Where(x => x.BRANCHID == a.BRANCHID).Select(x => x.BRANCHNAME).FirstOrDefault(),
                            categoryName = context.TBL_DAILY_ACCRUAL_CATEGORY.Where(x => x.CATEGORYID == a.CATEGORYID).Select(x => x.CATEGORYNAME).FirstOrDefault(),
                            currencyName = context.TBL_CURRENCY.Where(x => x.CURRENCYID == a.CURRENCYID).Select(x => x.CURRENCYNAME).FirstOrDefault(),
                            dailyAccrualAmount = a.DAILYACCURALAMOUNT,
                            date = a.DATE,
                            exchangeRate = a.EXCHANGERATE,
                            interestRate = a.INTERESTRATE,
                            mainAmount = a.MAINAMOUNT,
                            referenceNumber = a.REFERENCENUMBER,
                            repaymentPostedStatus = a.REPAYMENTPOSTEDSTATUS,
                            transactionTypeName = context.TBL_LOAN_TRANSACTION_TYPE.Where(x => x.TRANSACTIONTYPEID == a.TRANSACTIONTYPEID).Select(x => x.TRANSACTIONTYPENAME).FirstOrDefault(),
                            productCode = p.PRODUCTCODE,
                            productName =p.PRODUCTNAME,
                            customerCode = c.CUSTOMERCODE,
                            customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,
                            branchCode = context.TBL_BRANCH.Where(x => x.BRANCHID == a.BRANCHID).Select(x => x.BRANCHCODE).FirstOrDefault(),
                        }).ToList();
            }
            return data;
        }
        public List<BulkTransactionViewModel> CustomeFacilityRepayment(DateTime startDate, DateTime endDate,  int companyId,string valueCode)
        {
            List<BulkTransactionViewModel> data;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                data = (from a in context.TBL_CUSTOM_TRANSACTION_BULK
                        join l in context.TBL_LOAN on a.SOURCEREFERENCENUMBER equals l.LOANREFERENCENUMBER
                        join p in context.TBL_PRODUCT on l.PRODUCTID equals p.PRODUCTID
                        join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                        where DbFunctions.TruncateTime( a.POSTEDDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(a.POSTEDDATE) <= DbFunctions.TruncateTime(endDate)
                        && a.COMPANYID == companyId
                        && (a.FLOWTYPE.Trim() == valueCode.Trim() || valueCode==null || valueCode=="") 
                        orderby a.BULKTRANSACTIONID descending
                        select new BulkTransactionViewModel()
                        {
                            amount = a.AMOUNT,
                            amountCollected = a.AMOUNTCOLLECTED,
                            bankId = a.BANKID,
                            batchId = a.BATCHID,
                            bulkTransactionID = a.BULKTRANSACTIONID,
                            creditAccount = a.CREDITACCOUNT,
                            currencyRate = a.CURRENCYRATE,
                            currencyRateCode = a.CURRENCYRATECODE,
                            debitAccount = a.DEBITACCOUNT,
                            description = a.DESCRIPTION,
                            destinationBranchId = a.DESTINATIONBRANCHID,
                            flowType = a.FLOWTYPE,
                            forceDebitAccount = a.FORCEDEBITACCOUNT,
                            isposted = a.ISPOSTED,
                            operationId = a.OPERATIONID,
                            postedBy = a.POSTEDBY,
                            postedDate = a.POSTEDDATE,
                            sid = a.SID,
                            sourceBranchId = a.SOURCEBRANCHID,
                            sourceReferenceNumber = a.SOURCEREFERENCENUMBER,
                            syetemDateTime = a.SYSTEMDATETIME,
                            transactionType = a.TRANSACTIONTYPE,
                            valueDate = a.VALUEDATE,
                            productCode = p.PRODUCTCODE,
                            productName = p.PRODUCTNAME,
                            customerCode = c.CUSTOMERCODE,
                            customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,
                            branchCode = context.TBL_BRANCH.Where(x => x.BRANCHID == l.BRANCHID).Select(x => x.BRANCHCODE).FirstOrDefault(),
                            branchName = context.TBL_BRANCH.Where(x => x.BRANCHID == l.BRANCHID).Select(x => x.BRANCHNAME).FirstOrDefault(),
                        }).ToList();
            }
            return data;
        }
    }
}
