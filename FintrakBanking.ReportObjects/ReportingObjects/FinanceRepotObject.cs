using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Finance.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FinTrakBanking.ThirdPartyIntegration.Finacle.CWGAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public partial class FinanceRepotObject
    {
        public List<TransactionViewModel> FinanceTransaction(DateTime endDate, DateTime startDate, int? staffId, int companyId, int? branchId, bool excludeSystem)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var staffSensitivityLevelId = context.TBL_STAFF.Find(staffId).CUSTOMERSENSITIVITYLEVELID;
                IQueryable<TransactionViewModel> data = (from a in context.TBL_FINANCE_TRANSACTION
                                                         where a.COMPANYID == companyId
                                                         && (a.POSTEDDATE <= endDate && a.POSTEDDATE >= startDate)
                                                         && a.TBL_CASA.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID == staffSensitivityLevelId
                                                         && branchId == 0
                                                         || a.COMPANYID == companyId
                                                         && (a.POSTEDDATE <= endDate && a.POSTEDDATE >= startDate)
                                                         && a.TBL_CASA.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID == staffSensitivityLevelId
                                                         && a.TBL_BRANCH.BRANCHID == branchId
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
                                                             postedBy = a.POSTEDBY == -1 ? SystemStaff.System.ToString() : a.TBL_STAFF.LASTNAME + " " + a.TBL_STAFF.FIRSTNAME,
                                                             approvedBy = a.POSTEDBY == -1 ? SystemStaff.System.ToString() : a.TBL_STAFF1.LASTNAME + " " + a.TBL_STAFF1.FIRSTNAME,
                                                             postCurrency = a.TBL_CURRENCY.CURRENCYNAME,
                                                             currencyRate = a.CURRENCYRATE,
                                                             approvedDate = a.APPROVEDDATE,
                                                             baseCurrency = a.TBL_COMPANY.TBL_CURRENCY.CURRENCYNAME

                                                         });


                if (branchId != null && branchId != 0)
                {
                    data = data.Where(c => c.branchId == branchId);
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
        public List<TransactionViewModel> Repayment(DateTime endDate, DateTime startDate, int? operationId, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                List<TransactionViewModel> data = (from a in context.TBL_FINANCE_TRANSACTION
                                                   where a.POSTEDDATE >= startDate && a.POSTEDDATE <= endDate
                                                   && (a.OPERATIONID == operationId || operationId ==0 || operationId==null)
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
                                                   }).ToList();
                return data;
            }

        }
        public List<DailyAccrualViewModel> DailyAccrual(DateTime endDate, DateTime startDate, int companyId, int? categoryId)// int? transactionTypeId
        {
            List<DailyAccrualViewModel> data;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                data = (from a in context.TBL_DAILY_ACCRUAL
                        where a.DATE >= startDate && a.DATE <= endDate
                        && (a.CATEGORYID == categoryId || categoryId == null || categoryId == 0)
                        // && (a.TRANSACTIONTYPEID == transactionTypeId || transactionTypeId==null || transactionTypeId == 0)
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
                        }).ToList();
            }
            return data;
        }
        public List<BulkTransactionViewModel> CustomeFacilityRepayment(DateTime endDate, DateTime startDate, int companyId,string valueCode)
        {
            List<BulkTransactionViewModel> data;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                data = (from a in context.TBL_CUSTOM_TRANSACTION_BULK
                        where a.POSTEDDATE >= startDate && a.POSTEDDATE <= endDate
                        && (a.FLOWTYPE == valueCode || valueCode==null || valueCode=="") 
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
                            valueDate = a.VALUEDATE
                        }).ToList();
            }
            return data;
        }
    }
}
