using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace FintrakBanking.ReportObjects
{

    public class LoanReportObjects
    {
        private IQueryable<LoanInformation> Loans(int companyId, DateTime startDate, DateTime endDate)
        {
            IQueryable<LoanInformation> loan;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                loan = (from a in context.TBL_LOAN
                        join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.TERMLOANID equals b.LOANID
                        where a.COMPANYID == companyId && a.ISDISBURSED == true && (a.DISBURSEDATE >= startDate && a.DISBURSEDATE <= endDate)
                        orderby a.DISBURSEDATE descending
                        select new LoanInformation()
                        {
                            customerId = a.CUSTOMERID,

                            tearmLoanId = a.TERMLOANID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            branchCode = a.TBL_BRANCH.BRANCHCODE,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            firstName = a.TBL_CUSTOMER.FIRSTNAME,
                            lastName = a.TBL_CUSTOMER.LASTNAME,
                            middleName = a.TBL_CUSTOMER.MIDDLENAME,
                            loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            loanRefrenceNumber = a.LOANREFERENCENUMBER,
                            frequancy = context.TBL_LOAN_SCHEDULE_PERIODIC.Where(c => c.LOANID == a.TERMLOANID).Count() - 1,
                            frequencyType = a.TBL_FREQUENCY_TYPE.MODE,
                            companyName = a.TBL_COMPANY.NAME,
                            companylogo = a.TBL_COMPANY.LOGOPATH,
                            effectiveDate = a.EFFECTIVEDATE,
                            interestRate = a.INTERESTRATE,
                            maturityDate = a.MATURITYDATE,
                            principalAmount = a.PRINCIPALAMOUNT,
                            closePrincipalAmount = b.ENDPRINCIPALAMOUNT,
                            startingBalance = b.STARTPRINCIPALAMOUNT,
                            paymentDate = b.PAYMENTDATE,
                            periodInterestAmount = b.PERIODINTERESTAMOUNT,
                            principalRepaymentAmount = b.PERIODPAYMENTAMOUNT,
                            outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                            outstandingInterest = a.OUTSTANDINGINTEREST
                        });
                return loan;
            }
        }

        public IEnumerable<LoanInformation> GetLoanSchedule(int companyId, int tearmLoanId, int staffId)
        {
            IEnumerable<LoanInformation> loan;

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                //var staffSensitivityLevelId = context.TBL_STAFF.Find(staffId).CUSTOMERSENSITIVITYLEVELID;
                var company = context.TBL_COMPANY.Where(c => c.COMPANYID == companyId).FirstOrDefault();
                loan = (from a in context.TBL_LOAN
                        join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                        join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.TERMLOANID equals b.LOANID
                        where a.COMPANYID == companyId && a.TERMLOANID == tearmLoanId
                        orderby a.BOOKINGDATE descending//&& c.CUSTOMERSENSITIVITYLEVELID <= staffSensitivityLevelId
                        select new LoanInformation()
                        {
                            accountNumber = context.TBL_CASA.Where(c => c.CASAACCOUNTID == a.CASAACCOUNTID).Select(c => c.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                            customerId = a.CUSTOMERID,
                            tearmLoanId = a.TERMLOANID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            branchCode = a.TBL_BRANCH.BRANCHCODE,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            firstName = a.TBL_CUSTOMER.FIRSTNAME,
                            lastName = a.TBL_CUSTOMER.LASTNAME,
                            middleName = a.TBL_CUSTOMER.MIDDLENAME,
                            loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            loanRefrenceNumber = a.LOANREFERENCENUMBER,
                            frequancy = context.TBL_LOAN_SCHEDULE_PERIODIC.Where(c => c.LOANID == a.TERMLOANID).Count() - 1,
                            frequencyType = a.TBL_FREQUENCY_TYPE.MODE,
                            companyName = company.NAME,
                            companylogo = company.LOGOPATH,
                            effectiveDate = a.EFFECTIVEDATE,
                            interestRate = a.INTERESTRATE,
                            maturityDate = a.MATURITYDATE,
                            principalAmount = a.PRINCIPALAMOUNT,
                            closePrincipalAmount = b.ENDPRINCIPALAMOUNT,
                            startingBalance = b.STARTPRINCIPALAMOUNT,
                            paymentDate = b.PAYMENTDATE,
                            periodInterestAmount = b.PERIODINTERESTAMOUNT,
                            principalRepaymentAmount = b.PERIODPAYMENTAMOUNT,
                            outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                            outstandingInterest = a.OUTSTANDINGINTEREST,
                            bookingDate = a.BOOKINGDATE
                        });

                return loan.ToList();
            }

        }

        public static IList<LoanStatementViewModel> LoanStatement(int companyId, int loanId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var principalRepayment = (from a in context.TBL_LOAN
                                          join b in context.TBL_FINANCE_TRANSACTION on a.LOANREFERENCENUMBER equals b.SOURCEREFERENCENUMBER
                                          where a.COMPANYID == companyId && a.TERMLOANID == loanId
                                          && b.TBL_CHART_OF_ACCOUNT.GLCLASSID == (int)ChartOfAccountClassEnum.LoanSchedule
                                          select new LoanStatementViewModel()
                                          {
                                              //balance = a.OUTSTANDINGPRINCIPAL,
                                              companyName = a.TBL_COMPANY.NAME,
                                              logoPath = a.TBL_COMPANY.LOGOPATH,
                                              firstName = a.TBL_CUSTOMER.FIRSTNAME,
                                              lastName = a.TBL_CUSTOMER.LASTNAME,
                                              middleName = a.TBL_CUSTOMER.MIDDLENAME,
                                              accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                              productName = a.TBL_PRODUCT.PRODUCTNAME,
                                              loanRefrenceNumber = a.LOANREFERENCENUMBER,
                                              applicationRefrenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                              grantedAmount = a.PRINCIPALAMOUNT,
                                              loanCurrency = a.TBL_CURRENCY.CURRENCYCODE,
                                              productId = a.PRODUCTID,
                                              postDate = b.POSTEDDATE,
                                              valueDate = b.VALUEDATE,
                                              creditAmount = b.CREDITAMOUNT,
                                              debitAmount = b.DEBITAMOUNT,
                                              discription = b.DESCRIPTION,
                                              transactionCurrency = b.TBL_CURRENCY.CURRENCYCODE,
                                          }).ToList();

                var interstRepayment = (from a in context.TBL_LOAN
                                        join b in context.TBL_FINANCE_TRANSACTION on a.LOANREFERENCENUMBER equals b.SOURCEREFERENCENUMBER
                                        where a.COMPANYID == companyId && a.TERMLOANID == loanId
                                        && b.TBL_CHART_OF_ACCOUNT.GLCLASSID == (int)ChartOfAccountClassEnum.LoanInterestReceivable
                                        select new LoanStatementViewModel()
                                        {
                                            //balance = a.OUTSTANDINGPRINCIPAL,
                                            companyName = a.TBL_COMPANY.NAME,
                                            logoPath = a.TBL_COMPANY.LOGOPATH,
                                            firstName = a.TBL_CUSTOMER.FIRSTNAME,
                                            lastName = a.TBL_CUSTOMER.LASTNAME,
                                            middleName = a.TBL_CUSTOMER.MIDDLENAME,
                                            accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                                            loanRefrenceNumber = a.LOANREFERENCENUMBER,
                                            applicationRefrenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                            grantedAmount = a.PRINCIPALAMOUNT,
                                            loanCurrency = a.TBL_CURRENCY.CURRENCYCODE,
                                            productId = a.PRODUCTID,
                                            postDate = b.POSTEDDATE,
                                            valueDate = b.VALUEDATE,
                                            creditAmount = b.CREDITAMOUNT,
                                            debitAmount = b.DEBITAMOUNT,
                                            discription = b.DESCRIPTION,
                                            transactionCurrency = b.TBL_CURRENCY.CURRENCYCODE,
                                        }).ToList();

                //var loan = context.TBL_LOAN.Find(loanId);

                List<short> interestItems = new List<short>() { (short)DailyAccrualCategory.TermLoan, (short)DailyAccrualCategory.PastDueInterest, (short)DailyAccrualCategory.PastDuePrincipal };


                var interestAccuralsSub = (from a in context.TBL_LOAN
                                           join b in context.TBL_DAILY_ACCRUAL on a.LOANREFERENCENUMBER equals b.REFERENCENUMBER
                                           join c in context.TBL_DAILY_ACCRUAL_CATEGORY on b.CATEGORYID equals c.CATEGORYID
                                           where a.COMPANYID == companyId && a.TERMLOANID == loanId && b.COMPANYID == companyId
                                            && interestItems.Contains(b.CATEGORYID)
                                            && b.REPAYMENTPOSTEDSTATUS == true
                                            && b.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                           select new LoanStatementViewModel()
                                           {
                                               //balance = a.OUTSTANDINGPRINCIPAL,
                                               companyName = a.TBL_COMPANY.NAME,
                                               logoPath = a.TBL_COMPANY.LOGOPATH,
                                               firstName = a.TBL_CUSTOMER.FIRSTNAME,
                                               lastName = a.TBL_CUSTOMER.LASTNAME,
                                               middleName = a.TBL_CUSTOMER.MIDDLENAME,
                                               accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                               productName = a.TBL_PRODUCT.PRODUCTNAME,
                                               loanRefrenceNumber = a.LOANREFERENCENUMBER,
                                               applicationRefrenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                               grantedAmount = a.PRINCIPALAMOUNT,
                                               loanCurrency = a.TBL_CURRENCY.CURRENCYCODE,
                                               productId = a.PRODUCTID,
                                               postDate = b.DEMANDDATE,
                                               valueDate = b.DEMANDDATE,
                                               creditAmount = 0,
                                               debitAmount = b.DAILYACCURALAMOUNT,
                                               discription = c.CATEGORYNAME,
                                               transactionCurrency = b.TBL_CURRENCY.CURRENCYCODE,
                                           }).ToList();

                var interestAccurals = (from a in interestAccuralsSub
                                        group a by new
                                        {
                                            a.companyName,
                                            a.logoPath,
                                            a.firstName,
                                            a.lastName,
                                            a.middleName,
                                            a.accountNumber,
                                            a.productName,
                                            a.loanRefrenceNumber,
                                            a.applicationRefrenceNumber,
                                            a.grantedAmount,
                                            a.loanCurrency,
                                            a.productId,
                                            a.postDate,
                                            a.valueDate,
                                            a.discription,
                                            a.transactionCurrency
                                        } into groupedQ
                                        select new LoanStatementViewModel()
                                        {
                                            companyName = groupedQ.Key.companyName,
                                            logoPath = groupedQ.Key.logoPath,
                                            firstName = groupedQ.Key.firstName,
                                            lastName = groupedQ.Key.lastName,
                                            middleName = groupedQ.Key.middleName,
                                            accountNumber = groupedQ.Key.accountNumber,
                                            productName = groupedQ.Key.productName,
                                            loanRefrenceNumber = groupedQ.Key.loanRefrenceNumber,
                                            applicationRefrenceNumber = groupedQ.Key.applicationRefrenceNumber,
                                            grantedAmount = groupedQ.Key.grantedAmount,
                                            loanCurrency = groupedQ.Key.loanCurrency,
                                            productId = groupedQ.Key.productId,
                                            postDate = groupedQ.Key.postDate,
                                            valueDate = groupedQ.Key.valueDate,
                                            creditAmount = groupedQ.Sum(i => i.creditAmount),
                                            debitAmount = groupedQ.Sum(i => i.debitAmount),
                                            discription = groupedQ.Key.discription,
                                            transactionCurrency = groupedQ.Key.transactionCurrency
                                        }



                                        ).ToList();



                var list = (principalRepayment.Union(interstRepayment).Union(interestAccurals)).OrderBy(x => x.valueDate).ToList();

                decimal rbalance = 0;
                list = list.Select(i =>
                {
                    rbalance += i.debitAmount - i.creditAmount;
                    i.balance = rbalance;
                    return i;
                }).ToList();



                return list;
            }

        }


        public IEnumerable<DisburstLoanViewModel> GetDisburstLoans(DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short? branchId, int? productClassId, int staffId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                //  var approvedCustomerSentivityLevelId = context.TBL_STAFF.Find(staffId).CUSTOMERSENSITIVITYLEVELID;
                StringBuilder sb = new StringBuilder();
                var data = from a in context.TBL_LOAN
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           join p in context.TBL_PRODUCT on a.PRODUCTID equals p.PRODUCTID
                           join pc in context.TBL_PRODUCT_CLASS on p.PRODUCTCLASSID equals pc.PRODUCTCLASSID
                           join br in context.TBL_STAFF on a.CREATEDBY equals br.STAFFID
                           where (a.ISDISBURSED
                             && a.DISBURSEDATE >= startDate && a.DISBURSEDATE <= endDate)
                         && a.COMPANYID == companyId
                           orderby a.DISBURSEDATE descending




                           //  && a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID <= approvedCustomerSentivityLevelId

                           select new DisburstLoanViewModel
                           {
                               bookingRef = a.LOANREFERENCENUMBER,
                               outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                               approvedInterestRate = a.INTERESTRATE,
                               outstandingInterest = a.OUTSTANDINGINTEREST,
                               amountDisbursed = a.PRINCIPALAMOUNT,
                               accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                               applicationReferenceNumber = b.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                               productName = p.PRODUCTNAME,
                               approvedAmount = b.APPROVEDAMOUNT,
                               baseCurrency = b.TBL_LOAN_APPLICATION.TBL_COMPANY.TBL_CURRENCY.CURRENCYCODE,
                               companyName = a.TBL_COMPANY.NAME,
                               logoPath = a.TBL_COMPANY.LOGOPATH,
                               customerName = a.TBL_CUSTOMER.LASTNAME + " " + a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME,
                               disburseDate = a.DISBURSEDATE,
                               effectiveDate = a.EFFECTIVEDATE,
                               exchangeRate = a.EXCHANGERATE,
                               exchangeValue = (a.EXCHANGERATE * (double)a.PRINCIPALAMOUNT),
                               facilityCurrency = a.TBL_CURRENCY.CURRENCYCODE,
                               maturitydate = a.MATURITYDATE,
                               productId = a.PRODUCTID,
                               status = a.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                               branchId = a.BRANCHID,
                               productClassName = pc.PRODUCTCLASSNAME,
                               productClassID = p.PRODUCTCLASSID,
                               firstName = a.TBL_CUSTOMER.FIRSTNAME,
                               lastName = a.TBL_CUSTOMER.LASTNAME,
                               middleName = a.TBL_CUSTOMER.MIDDLENAME,
                               staffName = br.FIRSTNAME + " " + " " + br.MIDDLENAME + " " + " " + br.LASTNAME


                           };
                if (productClassId != 0)
                {
                    return data.Where(u => u.productClassID == productClassId.Value || productClassId == null || productClassId == 0).ToList();
                }
                else if (branchId != 0)
                {
                    return data.Where(u => u.branchId == branchId.Value || branchId == null || branchId.Value == 0).ToList();
                }
                else if (loanRefNo != null)
                {
                    return data.Where(u => u.bookingRef == loanRefNo
                         || u.firstName.ToLower().StartsWith(loanRefNo.ToLower())
                         || u.lastName.ToLower().StartsWith(loanRefNo.ToLower())
                         || u.middleName.ToLower().StartsWith(loanRefNo.ToLower())
                         || u.firstName.ToLower().EndsWith(loanRefNo.ToLower())
                         || u.lastName.ToLower().EndsWith(loanRefNo.ToLower())
                         || u.middleName.ToLower().EndsWith(loanRefNo.ToLower())
                         || u.firstName.ToLower().Contains(loanRefNo.ToLower())
                         || u.lastName.ToLower().Contains(loanRefNo.ToLower())
                         || u.middleName.ToLower().Contains(loanRefNo.ToLower())
                         || loanRefNo.ToLower().Contains(u.firstName.ToLower())
                         || loanRefNo.ToLower().Contains(u.lastName.ToLower())
                         || loanRefNo.ToLower().Contains(u.middleName.ToLower())
                         || loanRefNo == null || loanRefNo == "").ToList();
                }

                else
                {
                    return data.ToList();
                }

            }
        }

        public IEnumerable<DisburstLoanViewModel> RunningFacilities(DateTime startDate, DateTime endDate, int companyId, int staffId, string crmSCode)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                //  var approvedCustomerSentivityLevelId = context.TBL_STAFF.Find(staffId).CUSTOMERSENSITIVITYLEVELID;
                IQueryable<DisburstLoanViewModel> data = from a in context.TBL_LOAN
                                                         join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                                         where (a.ISDISBURSED && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                                                           && DbFunctions.TruncateTime(a.DISBURSEDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(a.DISBURSEDATE) <= DbFunctions.TruncateTime(endDate)
                                                       && a.COMPANYID == companyId)
                                                         orderby a.DISBURSEDATE descending
                                                         //&& (a.BRANCHID == branchId || branchId == null || branchId == 0)

                                                         //  && a.TBL_CUSTOMER.CUSTOMERSENSITIVITYLEVELID <= approvedCustomerSentivityLevelId

                                                         select new DisburstLoanViewModel
                                                         {
                                                             cRMSCode = b.CRMSCODE,
                                                             bookingRef = a.LOANREFERENCENUMBER,
                                                             outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                                             approvedInterestRate = a.INTERESTRATE,
                                                             outstandingInterest = a.OUTSTANDINGINTEREST,
                                                             amountDisbursed = a.PRINCIPALAMOUNT,
                                                             accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                                             applicationReferenceNumber = b.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                             productName = a.TBL_PRODUCT.PRODUCTNAME,
                                                             approvedAmount = b.APPROVEDAMOUNT,
                                                             baseCurrency = b.TBL_LOAN_APPLICATION.TBL_COMPANY.TBL_CURRENCY.CURRENCYCODE,
                                                             companyName = a.TBL_COMPANY.NAME,
                                                             logoPath = a.TBL_COMPANY.LOGOPATH,
                                                             customerName = a.TBL_CUSTOMER.LASTNAME + " " + a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME,
                                                             disburseDate = a.DISBURSEDATE,
                                                             effectiveDate = a.EFFECTIVEDATE,
                                                             exchangeRate = a.EXCHANGERATE,
                                                             exchangeValue = (a.EXCHANGERATE * (double)a.PRINCIPALAMOUNT),
                                                             facilityCurrency = a.TBL_CURRENCY.CURRENCYCODE,
                                                             maturitydate = a.MATURITYDATE,
                                                             status = a.TBL_LOAN_STATUS.ACCOUNTSTATUS,

                                                         };
                // var output = data.ToList();

                if (crmSCode == "Yes")
                {

                    return data.Where(u => u.cRMSCode != null).ToList();

                }
                else if (crmSCode == "No")
                {
                    return data.Where(x => x.cRMSCode == null).ToList();
                }
                else
                {
                    return data.ToList();
                }
                //var output = data.ToList();

                ///return output;


            }
        }

        public static List<AllLoanViewModel> LoanReport(int ProductClassId, DateTime startDate, DateTime endDdate, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = (from a in context.TBL_LOAN
                            where a.TBL_PRODUCT.PRODUCTCLASSID == ProductClassId && a.ISDISBURSED
                            && DbFunctions.TruncateTime(startDate) >= DbFunctions.TruncateTime(a.DISBURSEDATE)
                            && DbFunctions.TruncateTime(a.DISBURSEDATE) <= DbFunctions.TruncateTime(endDdate)
                            && a.COMPANYID == companyId
                            orderby a.DISBURSEDATE descending
                            select new AllLoanViewModel()
                            {
                                requestState = a.TBL_BRANCH.TBL_STATE.STATENAME,
                                bookingDate = a.BOOKINGDATE,
                                effectiveDate = a.EFFECTIVEDATE,
                                maturityDate = a.MATURITYDATE.Date,
                                disburseDate = a.DISBURSEDATE,
                                bookingNumber = a.LOANREFERENCENUMBER,
                                loanStatus = a.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                customerName = a.TBL_CUSTOMER.LASTNAME + " " + a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME,
                                principalAmount = a.PRINCIPALAMOUNT,
                                rate = a.TBL_PRODUCT.PRODUCTPRICEINDEXSPREAD,
                                rateCharged = a.INTERESTRATE,
                                payAccountTo = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                interestToDate = a.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(c => c.DATE == DateTime.Now.Date).ACCRUEDINTEREST,
                                currency = a.TBL_CURRENCY.CURRENCYCODE,
                                businessGroup = context.TBL_DEPARTMENT.FirstOrDefault(d => d.DEPARTMENTID == a.TBL_STAFF.TBL_DEPARTMENT_UNIT.DEPARTMENTID).DEPARTMENTNAME

                            }).ToList();
                return data;
            }
        }

        public static List<AllLoanViewModel> EarnedAndReceivableLoans(int ProductClassId, DateTime startDate, DateTime endDdate, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = (from a in context.TBL_LOAN
                            where a.TBL_PRODUCT.PRODUCTCLASSID == ProductClassId && a.ISDISBURSED
                            && DbFunctions.TruncateTime(startDate) >= DbFunctions.TruncateTime(a.DISBURSEDATE)
                            && DbFunctions.TruncateTime(a.DISBURSEDATE) <= DbFunctions.TruncateTime(endDdate)
                            && a.COMPANYID == companyId
                            orderby a.DISBURSEDATE descending
                            select new AllLoanViewModel()
                            {
                                requestState = a.TBL_BRANCH.TBL_STATE.STATENAME,
                                bookingDate = a.BOOKINGDATE,
                                effectiveDate = a.EFFECTIVEDATE,
                                maturityDate = a.MATURITYDATE.Date,
                                disburseDate = a.DISBURSEDATE,
                                bookingNumber = a.LOANREFERENCENUMBER,
                                loanStatus = a.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                customerName = a.TBL_CUSTOMER.LASTNAME + " " + a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME,
                                principalAmount = a.PRINCIPALAMOUNT,
                                rate = a.TBL_PRODUCT.PRODUCTPRICEINDEXSPREAD,
                                rateCharged = a.INTERESTRATE,
                                payAccountTo = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                interestToDate = a.TBL_LOAN_SCHEDULE_DAILY.FirstOrDefault(c => c.DATE == DateTime.Now.Date).ACCRUEDINTEREST,
                                currency = a.TBL_CURRENCY.CURRENCYCODE,
                                businessGroup = context.TBL_DEPARTMENT.FirstOrDefault(d => d.DEPARTMENTID == a.TBL_STAFF.TBL_DEPARTMENT_UNIT.DEPARTMENTID).DEPARTMENTNAME

                            }).ToList();
                return data;
            }
        }

        public IList<LoanAnniverseryViewModel> LoanAnniversery(DateTime startDate, DateTime endDate, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN
                           join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.TERMLOANID equals b.LOANID
                           join c in context.TBL_CUSTOMER_PHONECONTACT on a.CUSTOMERID equals c.CUSTOMERID
                           where a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
                           && DbFunctions.TruncateTime(b.PAYMENTDATE) >= DbFunctions.TruncateTime(startDate)
                            && DbFunctions.TruncateTime(b.PAYMENTDATE) <= DbFunctions.TruncateTime(endDate)
                           orderby b.PAYMENTDATE descending
                           //&& DbFunctions.TruncateTime(startDate) >= DbFunctions.TruncateTime(b.PAYMENTDATE)
                           //&& DbFunctions.TruncateTime(b.PAYMENTDATE) <= DbFunctions.TruncateTime(endDate)
                           select new LoanAnniverseryViewModel()
                           {
                               customerId = a.CUSTOMERID,
                               maturityDate = a.MATURITYDATE,
                               grantedAmount = a.PRINCIPALAMOUNT,
                               outstandingIntrestAmt = a.OUTSTANDINGINTEREST,
                               outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                               loanRefrenceNumber = a.LOANREFERENCENUMBER,
                               applicationRefrenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                               accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                               productName = a.TBL_PRODUCT.PRODUCTNAME,
                               productId = a.PRODUCTID,
                               companyName = a.TBL_COMPANY.NAME,
                               logoPath = a.TBL_COMPANY.LOGOPATH,
                               firstName = a.TBL_CUSTOMER.FIRSTNAME,
                               lastName = a.TBL_CUSTOMER.LASTNAME,

                               middleName = a.TBL_CUSTOMER.MIDDLENAME,
                               totalperiodicPaymentAmt = b.PERIODPAYMENTAMOUNT,
                               periodicInterestAmt = b.PERIODINTERESTAMOUNT,
                               periodicPrincipalAmt = b.PERIODPRINCIPALAMOUNT,
                               paymentdate = b.PAYMENTDATE,
                               intrestrate = b.INTERESTRATE,
                               emailAddress = a.TBL_CUSTOMER.EMAILADDRESS,
                               phoneNumber = c.PHONENUMBER

                           };

                return data.ToList();
            }

        }

        public IList<LoanDocumentWaivedViewModel> LoanDocumentDeferred(DateTime startDate, DateTime endDate, int companyId, short? branchId,string searchParameter)
        {
            List<SbHead> subList = new List<SbHead>();
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SbHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, teamUnit = sl.TEAM_UNIT }).ToList();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {


                    var deferredConditions = (from a in context.TBL_LOAN_CONDITION_PRECEDENT
                                              join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                              join c in context.TBL_LOAN_CONDITION_DEFERRAL on a.LOANCONDITIONID equals c.LOANCONDITIONID
                                              join d in context.TBL_CHECKLIST_STATUS on a.CHECKLISTSTATUSID equals d.CHECKLISTSTATUSID
                                              join e in context.TBL_LOAN on b.CUSTOMERID equals e.CUSTOMERID
                                              //join rm in context.TBL_STAFF on e.CREATEDBY equals rm.STAFFID
                                              join cas in context.TBL_CASA on e.CASAACCOUNTID equals cas.CASAACCOUNTID
                                              join p in context.TBL_PRODUCT on e.PRODUCTID equals p.PRODUCTID
                                              where a.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Deferred
                                               && b.TBL_CUSTOMER.COMPANYID == companyId
                                               && DbFunctions.TruncateTime(b.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                               && DbFunctions.TruncateTime(b.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                                               //&& b.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID > (short)LoanApplicationStatusEnum.ApplicationCompleted
                                               && b.TBL_LOAN_APPLICATION.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                               && c.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                               && (b.TBL_CUSTOMER.BRANCHID == branchId || branchId == null || branchId == 0)
                                               && (context.TBL_STAFF.Where(o => o.STAFFID == e.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault().StartsWith(searchParameter.Trim()) 
                                               || context.TBL_STAFF.Where(o => o.STAFFID == e.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault().Contains(searchParameter.Trim()) 
                                               || b.APPROVEDAMOUNT.ToString().Contains(searchParameter.Trim())
                                               || b.TBL_PRODUCT.PRODUCTNAME.Contains(searchParameter.Trim()) || b.TBL_CUSTOMER.CUSTOMERCODE.Contains(searchParameter.Trim()) 
                                               || b.TBL_CUSTOMER.FIRSTNAME.StartsWith(searchParameter.Trim()) || b.TBL_CUSTOMER.MIDDLENAME.StartsWith(searchParameter.Trim()) || b.TBL_CUSTOMER.LASTNAME.StartsWith(searchParameter.Trim())
                                               || b.TBL_CUSTOMER.FIRSTNAME.Contains(searchParameter.Trim()) || b.TBL_CUSTOMER.MIDDLENAME.Contains(searchParameter.Trim()) || b.TBL_CUSTOMER.LASTNAME.Contains(searchParameter.Trim())
                                               || a.CONDITION.Contains(searchParameter.Trim()) 
                                           
                                               || context.TBL_STAFF.Where(o => o.STAFFID == o.SUPERVISOR_STAFFID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault().Contains(searchParameter.Trim())
                                               || context.TBL_STAFF.Where(o => o.STAFFID == o.SUPERVISOR_STAFFID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault().StartsWith(searchParameter.Trim())
                                               || p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME.Contains(searchParameter.Trim()) || searchParameter == "" || searchParameter == null)
                                              orderby e.EFFECTIVEDATE descending
                                              select new LoanDocumentWaivedViewModel()
                                              {

                                                  applicationRefrenceNumber = b.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                  waivedDocument = a.CONDITION,
                                                  facilityAmount = b.APPROVEDAMOUNT,
                                                  facilityExpirationDate = e.MATURITYDATE,
                                                  facilityGrantedDate = e.EFFECTIVEDATE,
                                                  //companyName = b.TBL_CUSTOMER.TBL_COMPANY.NAME, //b.TBL_LOAN.Select(p => p.TBL_COMPANY.NAME).FirstOrDefault(),
                                                                                                 //waveredDate = c.DEFERREDDATE,
                                                  //branchName = b.TBL_CUSTOMER.TBL_BRANCH.BRANCHNAME,
                                                  facilityType = b.TBL_PRODUCT.PRODUCTNAME,
                                                  //loanApplicationId = b.LOANAPPLICATIONDETAILID,
                                                  //proposedAmount = b.PROPOSEDAMOUNT,
                                                  //checkListStatusName = d.CHECKLISTSTATUSNAME,
                                                  customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,
                                                  initialDefferalDate = c.DATETIMECREATED,
                                                  nameOfRM = context.TBL_STAFF.Where(o => o.STAFFID == e.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                                  //staffCode = ,
                                                  //customerAcct = cas.PRODUCTACCOUNTNUMBER,
                                                  customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + " " + b.TBL_CUSTOMER.LASTNAME,
                                                  currentExposure = e.OUTSTANDINGPRINCIPAL + e.PASTDUEPRINCIPAL,
                                                  currentDefferalDate = c.DEFERREDDATE,
                                                  deferralDurration = (int)DbFunctions.DiffDays((DateTime?)c.DEFERREDDATE, (DateTime?)c.DATETIMECREATED),
                                                 
                                                  cummulativeDays = c.DEFERREDDATE.Day + c.DATETIMECREATED.Day,
                                                  deferralExpiryDate = (b.EXPIRYDATE.Value == null ? default(DateTime) : b.EXPIRYDATE.Value),
                                                  
                                                  nameOfBM = context.TBL_STAFF.Where(o => o.SUPERVISOR_STAFFID.Value == e.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                                  businessUnit = p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME





                                              }).ToList();

                    return deferredConditions.OrderBy(u => u.waveredDate).ToList();

                }
            }
        }

        public IList<LoanDocumentWaivedViewModel> LoanDocumentWaived(DateTime startDate, DateTime endDate, int companyId, short? branchId,string searchParameter)
        {
            List<SbHead> subList = new List<SbHead>();
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SbHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, teamUnit = sl.TEAM_UNIT }).ToList();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {


                    var waivedConditions = (from a in context.TBL_LOAN_CONDITION_PRECEDENT
                                            join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                            join c in context.TBL_LOAN_CONDITION_DEFERRAL on a.LOANCONDITIONID equals c.LOANCONDITIONID
                                            join d in context.TBL_CHECKLIST_STATUS on a.CHECKLISTSTATUSID equals d.CHECKLISTSTATUSID
                                            join e in context.TBL_LOAN on b.CUSTOMERID equals e.CUSTOMERID
                                            join rm in context.TBL_STAFF on e.CREATEDBY equals rm.STAFFID
                                            join cas in context.TBL_CASA on e.CASAACCOUNTID equals cas.CASAACCOUNTID
                                            join p in context.TBL_PRODUCT on e.PRODUCTID equals p.PRODUCTID
                                            where a.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Waived
                                             && b.TBL_CUSTOMER.COMPANYID == companyId
                                             && DbFunctions.TruncateTime(b.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                                             && DbFunctions.TruncateTime(b.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                                             //&& b.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID > (short)LoanApplicationStatusEnum.ApplicationCompleted
                                             && b.TBL_LOAN_APPLICATION.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                             && c.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                             && (b.TBL_CUSTOMER.BRANCHID == branchId || branchId == null || branchId == 0)
                                             && (b.APPROVEDAMOUNT.ToString().Contains(searchParameter.Trim()) || b.TBL_PRODUCT.PRODUCTNAME.Contains(searchParameter.Trim()) || b.TBL_CUSTOMER.CUSTOMERCODE.Contains(searchParameter.Trim())
                                               || b.TBL_CUSTOMER.FIRSTNAME.StartsWith(searchParameter.Trim()) || b.TBL_CUSTOMER.MIDDLENAME.StartsWith(searchParameter.Trim()) || b.TBL_CUSTOMER.LASTNAME.StartsWith(searchParameter.Trim())
                                               || b.TBL_CUSTOMER.FIRSTNAME.Contains(searchParameter.Trim()) || b.TBL_CUSTOMER.MIDDLENAME.Contains(searchParameter.Trim()) || b.TBL_CUSTOMER.LASTNAME.Contains(searchParameter.Trim())
                                               || a.CONDITION.Contains(searchParameter.Trim())
                                               || p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME.Contains(searchParameter.Trim())
                                               || searchParameter == "" || searchParameter == null)
                                               
                                            orderby e.EFFECTIVEDATE descending
                                            select new LoanDocumentWaivedViewModel()
                                            {

                                                waivedDocument = a.CONDITION,
                                                facilityAmount = b.APPROVEDAMOUNT,
                                                facilityExpirationDate = e.MATURITYDATE,
                                                facilityGrantedDate = e.EFFECTIVEDATE,
                                                facilityType = b.TBL_PRODUCT.PRODUCTNAME,
                                                customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + " " + b.TBL_CUSTOMER.LASTNAME,
                                                businessUnit = p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                                                customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,






                                            }).ToList();
                                            

                    return waivedConditions.OrderBy(u => u.waveredDate).ToList();

                }
            }
        }

        public IList<LoanDocumentWaivedViewModel> LoanDeferrals(DateTime startDate, DateTime endDate, int companyId, short? branchId)
        {

            
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var data = from a in context.TBL_LOAN_CONDITION_DEFERRAL
                               join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANCONDITIONID equals b.LOANCONDITIONID
                               join d in context.TBL_LOAN_APPLICATION on b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                               join c in context.TBL_CUSTOMER on d.CUSTOMERID equals c.CUSTOMERID
                               join e in context.TBL_LOAN_APPLICATION_DETAIL on b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals e.LOANAPPLICATIONID

                               where
                                  DbFunctions.TruncateTime(a.DEFERREDDATE) >= DbFunctions.TruncateTime(startDate)
                                && DbFunctions.TruncateTime(a.DEFERREDDATE) <= DbFunctions.TruncateTime(endDate)
                               && d.APPLICATIONSTATUSID > (short)LoanApplicationStatusEnum.ApplicationCompleted
                                && d.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved
                                && (c.BRANCHID == branchId || branchId == null || branchId == 0)
                               orderby a.DEFERREDDATE descending


                               select new LoanDocumentWaivedViewModel()
                               {
                                   name = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                                   facilityProduct = e.TBL_PRODUCT.PRODUCTNAME,
                                   customerCode = c.CUSTOMERCODE,
                                   initialDefferalDate = a.DEFERREDDATE,
                                   applicationRefrenceNumber = b.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                   defferalDocument = b.CONDITION,
                                   facilityAmount = d.APPROVEDAMOUNT,
                                   facilityExpirationDate = e.TBL_LOAN.Select(c => c.MATURITYDATE).FirstOrDefault(),
                                   facilityGrantedDate = e.TBL_LOAN.Select(m => m.EFFECTIVEDATE).FirstOrDefault(),
                                   companyName = c.TBL_COMPANY.NAME,
                                   branchName = c.TBL_BRANCH.BRANCHNAME,
                                   facilityType = e.TBL_PRODUCT.PRODUCTNAME,
                                   loanApplicationId = b.LOANAPPLICATIONDETAILID,
                                   proposedAmount = d.APPROVEDAMOUNT,
                                   dateCreated = a.DATETIMECREATED,
                                   defferalExpiryDate = a.DEFERREDDATE,
                                   nameOfRM = context.TBL_STAFF.Where(o => o.STAFFID == d.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),


                               };
                    
                    return data.ToList();
                }
            }
        
        public IList<LoanDocumentWaivedViewModel> LoanDeferralMCCCur(DateTime startDate, int companyId, int? branchCode)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN_CONDITION_DEFERRAL
                           join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANCONDITIONID equals b.LOANCONDITIONID
                           join d in context.TBL_LOAN_APPLICATION on b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                           join c in context.TBL_CUSTOMER on d.CUSTOMERID equals c.CUSTOMERID
                           join e in context.TBL_LOAN_APPLICATION_DETAIL on b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals e.LOANAPPLICATIONID

                           where
                            d.APPLICATIONSTATUSID > (short)LoanApplicationStatusEnum.ApplicationCompleted
                            && d.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved
                             && DbFunctions.TruncateTime(a.DEFERREDDATE) >= startDate
                            && (c.BRANCHID == branchCode || branchCode == 0 || branchCode == null)
                           orderby a.DEFERREDDATE descending

                           select new LoanDocumentWaivedViewModel()
                           {
                               name = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                               defferalDocument = b.CONDITION,
                               facilityAmount = d.APPROVEDAMOUNT,
                               facilityType = e.TBL_PRODUCT.PRODUCTNAME,
                               dateCreated = a.DATETIMECREATED,
                               defferalExpiryDate = a.DEFERREDDATE,


                           };
                return data.ToList();
            }
        }
        public IList<LoanDocumentWaivedViewModel> LoanDeferralMCCExp(DateTime startDate, int companyId, int? branchCode)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN_CONDITION_DEFERRAL
                           join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANCONDITIONID equals b.LOANCONDITIONID
                           join d in context.TBL_LOAN_APPLICATION on b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                           join c in context.TBL_CUSTOMER on d.CUSTOMERID equals c.CUSTOMERID
                           join e in context.TBL_LOAN_APPLICATION_DETAIL on b.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID equals e.LOANAPPLICATIONID

                           where
                            d.APPLICATIONSTATUSID > (short)LoanApplicationStatusEnum.ApplicationCompleted
                            && d.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved
                             && a.DEFERREDDATE <= startDate
                            && (c.BRANCHID == branchCode || branchCode == 0 || branchCode == null)
                           orderby a.DEFERREDDATE descending
                           select new LoanDocumentWaivedViewModel()
                           {
                               name = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                               defferalDocument = b.CONDITION,
                               facilityAmount = d.APPROVEDAMOUNT,
                               facilityType = e.TBL_PRODUCT.PRODUCTNAME,
                               dateCreated = a.DATETIMECREATED,
                               defferalExpiryDate = a.DEFERREDDATE,


                           };
                return data.ToList();
            }
        }
        public IList<LoanDocumentWaivedViewModel> LoanDocumentWaivedForMCC(DateTime startDate, int companyId, int? branchCode)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN_CONDITION_PRECEDENT
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID

                           where (a.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Waived)
                            && b.TBL_CUSTOMER.COMPANYID == companyId
                            && DbFunctions.TruncateTime(b.DATETIMECREATED) <= DbFunctions.TruncateTime(startDate)
                            && (b.TBL_CUSTOMER.BRANCHID == branchCode || branchCode == 0 || branchCode == null)
                           orderby b.DATETIMECREATED descending


                           select new LoanDocumentWaivedViewModel()
                           {
                               name = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                               waivedDocument = a.CONDITION,
                               facilityAmount = b.APPROVEDAMOUNT,
                               waveredDate = a.DATETIMECREATED,
                               facilityType = b.TBL_PRODUCT.PRODUCTNAME,

                           };
                return data.ToList();
            }
        }
        public static IList<CollateralEstimatedViewModel> CollateralEstimated(int companyId, string collateralCode) //(string collateralCode, string acctNumber, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN_COLLATERAL_MAPPING
                           join l in context.TBL_LOAN on a.LOANID equals l.TERMLOANID
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           join c in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                           join d in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals d.COLLATERALCUSTOMERID
                           where d.COLLATERALCODE == collateralCode
                           orderby a.DATETIMECREATED descending
                           select new CollateralEstimatedViewModel()
                           {
                               firstName = b.TBL_CUSTOMER.FIRSTNAME,
                               lastName = b.TBL_CUSTOMER.LASTNAME,
                               middleName = b.TBL_CUSTOMER.MIDDLENAME,
                               facilityAmount = b.APPROVEDAMOUNT,
                               companyName = b.TBL_CUSTOMER.TBL_COMPANY.NAME,
                               customerId = b.CUSTOMERID,
                               facilityName = b.TBL_PRODUCT.PRODUCTNAME,
                               collateralType = d.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                               collateralDetail = d.TBL_COLLATERAL_TYPE.DETAILS,
                               collateralCode = d.COLLATERALCODE,
                               collateralValue = d.COLLATERALVALUE,
                               hairCut = d.HAIRCUT,
                               loanRefrenceNumber = l.LOANREFERENCENUMBER,


                           };

                return data.ToList();
            }
        }

        public IList<FCYScheuledLoanViewModel> FCYScheuledLoan(int companyId, int loanId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var currdata = from a in context.TBL_LOAN
                               where a.COMPANYID == companyId
                               && a.TERMLOANID == loanId

                               //&& a.CURRENCYID != 1
                               select new FCYScheuledLoanViewModel()
                               {
                                   loanRefrenceNumber = a.LOANREFERENCENUMBER,
                                   accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                   firstName = a.TBL_CUSTOMER.FIRSTNAME,
                                   lastName = a.TBL_CUSTOMER.LASTNAME,
                                   middleName = a.TBL_CUSTOMER.MIDDLENAME,
                                   loanCurrency = a.TBL_CURRENCY.CURRENCYCODE,
                                   //scheduleTypeId = a.SCHEDULETYPEID,
                                   //scheduleTypeName = a.TBL_LOAN_SCHEDULE_type.SCHEDULETYPENAME,
                                   interestRate = a.INTERESTRATE,
                                   valueDate = a.EFFECTIVEDATE,
                                   maturityDate = a.MATURITYDATE,
                                   facilityLimit = a.PRINCIPALAMOUNT,
                                   facilityRate = a.INTERESTRATE,
                                   exchangeRate = a.EXCHANGERATE,
                                   //tenorDays = a.MATURITYDATE.Subtract(a.EFFECTIVEDATE)
                                   tenorDays = (a.MATURITYDATE.Day - a.EFFECTIVEDATE.Day),
                                   //tenorToDate =(DateTime.Now - a.EFFECTIVEDATE.Day),
                                   logoPath = a.TBL_COMPANY.LOGOPATH,
                                   companyName = a.TBL_COMPANY.NAME,
                                   applicationRefrenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                   loanFigure = a.PRINCIPALAMOUNT,

                                   //tenorToDate = DbFunctions.DiffDays(DateTime.Now,(a.EFFECTIVEDATE.Day))

                                   // tenorToDate = DbFunctions.DiffDays(DateTime.Now, a.EFFECTIVEDATE.Day)



                               };
                return currdata.ToList();
            }
        }

        public List<dynamic> LoanUtilization()
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = (from n in context.TBL_LOAN_APPLICATION_DETAIL
                            select new
                            {
                                n.APPROVEDAMOUNT,
                                disbursedAmount = (decimal?)(from a in context.TBL_LOAN where a.LOANAPPLICATIONDETAILID == n.LOANAPPLICATIONDETAILID select a.PRINCIPALAMOUNT).Sum() ?? 0,

                            }).ToList();
            }

            return null;
        }

        private IQueryable<LoanInformation> GeneralLoansReport(int companyId)
        {
            IQueryable<LoanInformation> loan;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                loan = (from a in context.TBL_LOAN
                        where a.COMPANYID == companyId && a.ISDISBURSED == true
                        orderby a.DISBURSEDATE descending
                        select new LoanInformation()
                        {
                            customerId = a.CUSTOMERID,
                            tearmLoanId = a.TERMLOANID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            branchCode = a.TBL_BRANCH.BRANCHCODE,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            firstName = a.TBL_CUSTOMER.FIRSTNAME,
                            lastName = a.TBL_CUSTOMER.LASTNAME,
                            middleName = a.TBL_CUSTOMER.MIDDLENAME,
                            loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            loanRefrenceNumber = a.LOANREFERENCENUMBER,
                            frequancy = context.TBL_LOAN_SCHEDULE_PERIODIC.Where(c => c.LOANID == a.TERMLOANID).Count() - 1,
                            frequencyType = a.TBL_FREQUENCY_TYPE.MODE,
                            companyName = a.TBL_COMPANY.NAME,
                            companylogo = a.TBL_COMPANY.LOGOPATH,
                            effectiveDate = a.EFFECTIVEDATE,
                            interestRate = a.INTERESTRATE,
                            maturityDate = a.MATURITYDATE,
                            principalAmount = a.PRINCIPALAMOUNT,
                            outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                            outstandingInterest = a.OUTSTANDINGINTEREST,
                            stateName = a.TBL_BRANCH.TBL_STATE.STATENAME,
                            stateId = a.TBL_BRANCH.STATEID,
                            sectorName = a.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                            sectorId = a.TBL_SUB_SECTOR.SECTORID,
                            subSectorName = a.TBL_SUB_SECTOR.NAME,
                            subSectorId = a.SUBSECTORID,
                            employer = a.TBL_CUSTOMER.TBL_CUSTOMER_EMPLOYMENTHISTORY.Where(x => x.CUSTOMERID == a.CUSTOMERID & x.ACTIVE == true).Select(x => new { x.EMPLOYERNAME, x.EMPLOYERADDRESS, x.OFFICEPHONE }).FirstOrDefault(),
                            groupId = context.TBL_CUSTOMER_GROUP_MAPPING.Where(x => x.CUSTOMERID == x.CUSTOMERID).Select(x => x.CUSTOMERGROUPID).FirstOrDefault(),
                            groupName = a.TBL_CUSTOMER.TBL_CUSTOMER_GROUP_MAPPING.Where(x => x.CUSTOMERID == x.CUSTOMERID).Select(x => x.TBL_CUSTOMER_GROUP.GROUPNAME).FirstOrDefault()
                        });
                return loan;
            }
        }

        public IList<CasaLienViewModel> AccountsWithLein(DateTime startDate, DateTime endDate, string searchParamemter, int companyId)
        {

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var data = (from a in context.TBL_CASA_LIEN
                            
                                //join l in context.TBL_LOAN on a.SOURCEREFERENCENUMBER equals l.
                                // join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                            where a.COMPANYID == companyId
                            && (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                            && DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                            && (a.PRODUCTACCOUNTNUMBER == searchParamemter || a.LIENREFERENCENUMBER == searchParamemter || searchParamemter == null)
                            orderby a.DATETIMECREATED descending

                            select new CasaLienViewModel
                            {
                                sourceReferenceNumber = a.SOURCEREFERENCENUMBER,
                                productAccountNumber = a.PRODUCTACCOUNTNUMBER,
                                lienReferenceNumber = a.LIENREFERENCENUMBER,
                                branchName = context.TBL_BRANCH.Where(x => x.BRANCHID == a.BRANCHID).Select(x => x.BRANCHNAME).FirstOrDefault(),
                                lienAmount = a.LIENAMOUNT,
                                description = a.DESCRIPTION,
                                lienTypeName = context.TBL_CASA_LIEN_TYPE.Where(x => x.LIENTYPEID == a.LIENTYPEID).Select(x => x.LIENTYPENAME).FirstOrDefault(),
                                dateTimeCreated = a.DATETIMECREATED,
                                //dateTimeUpdated = a.
                                //customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,

                            }).ToList();
                return data;


            }

        }

        public IList<LoanViewModel> GetStakeHolderOnExperationOfFTP(short? branchId, string customerName, DateTime maturityDate)
        {
            List<SbHead> subList = new List<SbHead>();
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SbHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, teamUnit = sl.TEAM_UNIT }).ToList();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var data = (
                                from s in context.TBL_CASA
                                join rv in context.TBL_LOAN_REVOLVING on s.CASAACCOUNTID equals rv.CASAACCOUNTID
                                join p in context.TBL_PRODUCT on rv.PRODUCTID equals p.PRODUCTID
                                join br in context.TBL_BRANCH on s.BRANCHID equals br.BRANCHID
                                join cs in context.TBL_CUSTOMER on s.CUSTOMERID equals cs.CUSTOMERID
                                join rm in context.TBL_STAFF on s.CREATEDBY equals rm.STAFFID
                                //join cas in context.TBL_CASA on s.CASAACCOUNTID equals cas.CASAACCOUNTID
                                where s.AVAILABLEBALANCE < 0
                                && DbFunctions.TruncateTime(rv.MATURITYDATE) > DbFunctions.TruncateTime(maturityDate)

                                 && (br.BRANCHID == branchId || branchId == null || branchId == 0)
                                && (cs.FIRSTNAME.StartsWith(customerName.Trim()) || cs.MIDDLENAME.StartsWith(customerName.Trim()) || cs.LASTNAME.StartsWith(customerName.Trim()) || customerName == null || customerName == "" || rv.LOANREFERENCENUMBER.StartsWith(customerName.Trim()) || (customerName.Trim()).Contains(cs.FIRSTNAME) || (customerName.Trim()).Contains(cs.MIDDLENAME) || (customerName.Trim().Contains(cs.LASTNAME)))
                                orderby rv.MATURITYDATE descending, rv.EFFECTIVEDATE descending

                                // 1
                                // 2
                                // 3
                                //
                                select new LoanViewModel
                                {
                                    //applicationReferenceNumber = rv.LOANREFERENCENUMBER,
                                    loanReferenceNumber = rv.LOANREFERENCENUMBER,
                                    bookingDate = rv.BOOKINGDATE,
                                    disburseDate = rv.DISBURSEDATE,
                                    maturityDate = rv.MATURITYDATE,
                                    interestRate = rv.INTERESTRATE,
                                    loanTypeName = rv.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                    branchId = s.BRANCHID,
                                    branchName = br.BRANCHNAME,
                                    customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                    productName = rv.TBL_PRODUCT.PRODUCTNAME,
                                    currency = rv.TBL_CURRENCY.CURRENCYNAME,
                                    customerAvailableAmount = s.AVAILABLEBALANCE,
                                    //nameOfRM = rm.FIRSTNAME + " " + " " + rm.MIDDLENAME + " " + " " + rm.LASTNAME,
                                    staffCode = rm.STAFFCODE,
                                    customerAcct = s.PRODUCTACCOUNTNUMBER,
                                    productAccountName = s.PRODUCTACCOUNTNAME,
                                    customerId = cs.CUSTOMERID,
                                    // teno = (a.MATURITYDATE - a.EFFECTIVEDATE).Days,
                                    effectiveDate = rv.EFFECTIVEDATE,
                                    facilityLimit = rv.OVERDRAFTLIMIT,
                                    businessUnit = p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                                    
                                    valueDate = rv.EFFECTIVEDATE,
                                    
                                    //facilityLimit = s.PRINCIPALAMOUNT,
                                    facilityRate = s.INTERESTRATE.Value,
                                    exchangeRate = rv.EXCHANGERATE,
                                    //tenorDays = a.MATURITYDATE.Subtract(a.EFFECTIVEDATE)
                                    tenorToDate = (int)DbFunctions.DiffDays(rv.MATURITYDATE, DateTime.Now),
                                    customerCode = cs.CUSTOMERCODE,
                                    nameOfRM = context.TBL_STAFF.Where(o => o.STAFFID == s.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),

                                }).ToList();


                    return data.ToList();
                }

            }
        }

        public IList<FacilityReport> FacilityApprovedNotUtilised(DateTime startDate, DateTime endDate, string customerName)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = (from ln in context.TBL_LOAN
                            join coy in context.TBL_COMPANY on ln.COMPANYID equals coy.COMPANYID
                            join req in context.TBL_LOAN_BOOKING_REQUEST on ln.LOANAPPLICATIONDETAILID equals req.LOANAPPLICATIONDETAILID
                            join br in context.TBL_BRANCH on ln.BRANCHID equals br.BRANCHID
                            join atrail in context.TBL_APPROVAL_TRAIL on ln.TERMLOANID equals atrail.TARGETID
                            join c in context.TBL_CUSTOMER on ln.CUSTOMERID equals c.CUSTOMERID
                            where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                                  && atrail.OPERATIONID == (int)OperationsEnum.TermLoanBooking
                                  //  && atrail.TOAPPROVALLEVELID == staffApprovalLevelId
                                  && atrail.RESPONSESTAFFID == null
                            orderby ln.TERMLOANID descending

                            select new FacilityReport
                            {
                                customerNames = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                                loanType = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                facilityType = context.TBL_PRODUCT.Where(o => o.PRODUCTID == ln.PRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                                refNo = ln.LOANREFERENCENUMBER,
                                approvedAmount = ln.PRINCIPALAMOUNT,
                                unitlizedAmount = 0,
                                accountBalance = 0,
                                tenor = 0,
                                interest = ln.INTERESTRATE,
                                dateApproved = ln.DATETIMECREATED,
                                branchName = br.BRANCHNAME

                            }).ToList();

                return data;
            }

        }
        public IEnumerable<DisburstLoanViewModel> GetRuningLoansByLoanType(DateTime startDate, DateTime endDate, int companyId, string searchParamemter, int? productClassId)
        {


            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           join c in context.TBL_LOAN_REVOLVING on a.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                           where (
                           a.ISDISBURSED
                           && DbFunctions.TruncateTime(a.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate)
                           && DbFunctions.TruncateTime(a.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate)
                           && a.COMPANYID == companyId
                           )
                           && (a.BRANCHID == context.TBL_BRANCH.Where(o => o.BRANCHNAME == searchParamemter).Select(o => o.BRANCHID).FirstOrDefault()
                           || a.LOANREFERENCENUMBER == searchParamemter || a.TBL_CUSTOMER.FIRSTNAME.StartsWith(searchParamemter)
                           || a.TBL_CUSTOMER.LASTNAME.StartsWith(searchParamemter)
                           || a.TBL_CUSTOMER.MIDDLENAME.StartsWith(searchParamemter) || searchParamemter == null)
                           && (a.TBL_PRODUCT.PRODUCTCLASSID == productClassId || productClassId == null)
                           orderby a.EFFECTIVEDATE descending

                           select new DisburstLoanViewModel
                           {
                               dealDate = a.BOOKINGDATE,
                               disburseDate = a.DISBURSEDATE,
                               effectiveDate = a.EFFECTIVEDATE,
                               loanTenor = (c.MATURITYDATE - c.EFFECTIVEDATE).Days,
                               tenorToDate = (DateTime.Now - c.EFFECTIVEDATE).Days,
                               applicationReferenceNumber = c.LOANREFERENCENUMBER,
                               status = "",
                               interestType = "",
                               interestRateChange = 0,
                               interestToDate = 0,
                               accountPayTo = context.TBL_CASA.Where(o => o.ACCOUNTSTATUSID == c.CASAACCOUNTID).Select(o => o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                               //accountReceiveFrom = context.TBL_CASA.Where(o => o.ACCOUNTSTATUSID == c.CASAACCOUNTID2).Select(o => o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                               naration = "",
                               remark = "",
                               current = "",
                               businessGroup = "",
                               customerName = a.TBL_CUSTOMER.LASTNAME + " " + a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME,
                               pricipalAmount = a.PRINCIPALAMOUNT,
                               rate = c.INTERESTRATE,

                               //productClassName = a.TBL_PRODUCT.PRODUCTNAME,
                               //bookingRef = c.LOANREFERENCENUMBER,
                               //outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                               //approvedInterestRate = c.INTERESTRATE,
                               //outstandingInterest = a.OUTSTANDINGINTEREST,
                               //amountDisbursed = a.PRINCIPALAMOUNT,
                               //accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                               //productName = a.TBL_PRODUCT.PRODUCTNAME,
                               //approvedAmount = b.APPROVEDAMOUNT,
                               //baseCurrency = b.TBL_LOAN_APPLICATION.TBL_COMPANY.TBL_CURRENCY.CURRENCYCODE,
                               //companyName = a.TBL_COMPANY.NAME,
                               //logoPath = a.TBL_COMPANY.LOGOPATH,
                               //customerName = a.TBL_CUSTOMER.LASTNAME + " " + a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME,
                               //disburseDate = a.DISBURSEDATE,
                               //effectiveDate = a.EFFECTIVEDATE,
                               //exchangeRate = a.EXCHANGERATE,
                               //exchangeValue = (a.EXCHANGERATE * (double)a.PRINCIPALAMOUNT),
                               //facilityCurrency = a.TBL_CURRENCY.CURRENCYCODE,
                               //maturitydate = a.MATURITYDATE,
                               //productId = a.PRODUCTID,
                               //status = a.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                               //branchId = a.BRANCHID,
                               //branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == a.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),

                           };
                return data.ToList();


            }
        }

        public IEnumerable<DisburstLoanViewModel> GetLoansInterestReceivable(DateTime startDate, DateTime endDate, int companyId, string searchParamemter, int productClassId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           where (
                           a.ISDISBURSED
                           && DbFunctions.TruncateTime(a.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate)
                           && DbFunctions.TruncateTime(a.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate)
                           && a.COMPANYID == companyId
                           )
                           && (a.BRANCHID == context.TBL_BRANCH.Where(o => o.BRANCHNAME == searchParamemter).Select(o => o.BRANCHID).FirstOrDefault()
                           || a.LOANREFERENCENUMBER == searchParamemter || a.TBL_CUSTOMER.FIRSTNAME.StartsWith(searchParamemter)
                           || a.TBL_CUSTOMER.LASTNAME.StartsWith(searchParamemter)
                           || a.TBL_CUSTOMER.MIDDLENAME.StartsWith(searchParamemter) || searchParamemter == null)
                           && (a.TBL_PRODUCT.PRODUCTCLASSID == productClassId || productClassId == null)
                           orderby a.EFFECTIVEDATE descending

                           select new DisburstLoanViewModel
                           {
                               productClassName = a.TBL_PRODUCT.PRODUCTNAME,
                               bookingRef = a.LOANREFERENCENUMBER,
                               outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                               approvedInterestRate = a.INTERESTRATE,
                               outstandingInterest = a.OUTSTANDINGINTEREST,
                               amountDisbursed = a.PRINCIPALAMOUNT,
                               accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                               applicationReferenceNumber = b.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                               productName = a.TBL_PRODUCT.PRODUCTNAME,
                               approvedAmount = b.APPROVEDAMOUNT,
                               baseCurrency = b.TBL_LOAN_APPLICATION.TBL_COMPANY.TBL_CURRENCY.CURRENCYCODE,
                               companyName = a.TBL_COMPANY.NAME,
                               logoPath = a.TBL_COMPANY.LOGOPATH,
                               customerName = a.TBL_CUSTOMER.LASTNAME + " " + a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME,
                               disburseDate = a.DISBURSEDATE,
                               effectiveDate = a.EFFECTIVEDATE,
                               exchangeRate = a.EXCHANGERATE,
                               exchangeValue = (a.EXCHANGERATE * (double)a.PRINCIPALAMOUNT),
                               facilityCurrency = a.TBL_CURRENCY.CURRENCYCODE,
                               maturitydate = a.MATURITYDATE,
                               productId = a.PRODUCTID,
                               status = a.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                               branchId = a.BRANCHID,
                               branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == a.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                               interest = a.OUTSTANDINGINTEREST

                           };
                return data.ToList();
            }
        }

        public List<CollateralViewModel> CollateralPropertyApproachingRevaluation(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var loanDetails = from a in context.TBL_COLLATERAL_CUSTOMER
                                  join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                  join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                  join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                  join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                  join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                  where DbFunctions.TruncateTime(f.LASTVALUATIONDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(f.LASTVALUATIONDATE) <= DbFunctions.TruncateTime(endDate)
                                  orderby f.LASTVALUATIONDATE descending
                                  select new CollateralViewModel
                                  {
                                      collateralTypeId = a.COLLATERALTYPEID,
                                      collateralType = d.COLLATERALTYPENAME,
                                      collateralCode = a.COLLATERALCODE,
                                      collateralSubType = e.COLLATERALSUBTYPENAME,
                                      customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                      propertyName = f.PROPERTYNAME,
                                      lastValuationDate = f.LASTVALUATIONDATE,
                                      valuationCycle = e.VISITATIONCYCLE,
                                      valuationDate = f.LASTVALUATIONDATE.AddDays((double)e.VISITATIONCYCLE),
                                      relationshipManagerId = a.CREATEDBY,
                                      relationshipManager = c.FIRSTNAME + " " + c.LASTNAME,
                                      relationshipManagerEmail = c.EMAIL,
                                  };
                return loanDetails.ToList();
            }

        }

        private string LoanCollateralPerfectionReasons(int loanId, LoanSystemTypeEnum loanSystemTypeId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var perfectionReasons = (from f in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                                         join m in context.TBL_LOAN_COLLATERAL_MAPPING on f.COLLATERALCUSTOMERID equals m.COLLATERALCUSTOMERID
                                         where m.LOANID == loanId && m.LOANSYSTEMTYPEID == (short)loanSystemTypeId
                                         select f.PERFECTIONSTATUSREASON).ToList();

                string output = "";

                foreach (var item in perfectionReasons)
                    output = output + ", " + item;

                return output;
            }
        }

        private string LoanCollateralType(int loanId, LoanSystemTypeEnum loanSystemTypeId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var loancollateralType = (from c in context.TBL_COLLATERAL_CUSTOMER
                                          join st in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals st.COLLATERALTYPEID
                                          join l in context.TBL_LOAN on c.CUSTOMERID equals l.CUSTOMERID
                                          where l.TERMLOANID == loanId && l.LOANSYSTEMTYPEID == (short)loanSystemTypeId
                                          select st.COLLATERALTYPENAME).ToList();

                string output = "";

                foreach (var item in loancollateralType)
                    output = output + "  " + item;

                return output;
            }
        }


        public List<StalledPerfectionViewModel> StalledPerfectionForCollateral(DateTime startDate, DateTime endDate, int companyid)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var loansWithCollateral = (from f in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                                           join m in context.TBL_LOAN_COLLATERAL_MAPPING on f.COLLATERALCUSTOMERID equals m.COLLATERALCUSTOMERID
                                           where f.PERFECTIONSTATUSID == (int)(CollateralPerfectionStatusEnum.Stalled)
                                           select m.LOANID);

                var reportData = (
                                  from l in context.TBL_LOAN
                                  join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                  where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                  && l.COMPANYID == 1 && loansWithCollateral.Contains(l.TERMLOANID) && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  orderby l.EFFECTIVEDATE descending
                                  select new StalledPerfectionViewModel
                                  {
                                      loanId = l.TERMLOANID,
                                      customerName = c.FIRSTNAME + " " + " " + c.MIDDLENAME + " " + " " + c.LASTNAME,
                                      outstandingBalance = l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL,
                                      startDate = startDate,
                                      endDate = endDate,
                                      loanRefno = l.LOANREFERENCENUMBER,
                                      outstandingInterest = l.OUTSTANDINGINTEREST + l.PASTDUEINTEREST
                                  }).ToList().Select(x =>
                                  {
                                      x.reasonsforStalledPerfection = LoanCollateralPerfectionReasons(x.loanId, LoanSystemTypeEnum.TermDisbursedFacility);
                                      return x;
                                  }).ToList();

                return reportData;
            }




        }

        public List<CollateralPerfectionyettoCommenceViewModel> CollateralPerfectionYetToCommence(DateTime startDate, DateTime endDate, int companyid)
        {

            List<SubHead> stagMis = new List<SubHead>();
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                stagMis = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB }).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var loansWithCollateral = (from f in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                                               join lc in context.TBL_LOAN_COLLATERAL_MAPPING on f.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                                               where f.PERFECTIONSTATUSID == (int)(CollateralPerfectionStatusEnum.NotPerfected)
                                               select lc.LOANID);

                    var reportData = (
                                      from l in context.TBL_LOAN
                                      join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                      join sta in context.TBL_STAFF on l.RELATIONSHIPOFFICERID equals sta.STAFFID
                                      where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                      && l.COMPANYID == companyid && loansWithCollateral.Contains(l.TERMLOANID) && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                      orderby l.EFFECTIVEDATE descending
                                      select new CollateralPerfectionyettoCommenceViewModel
                                      {
                                          loanId = l.TERMLOANID,
                                          customername = c.FIRSTNAME + " " + c.MIDDLENAME,
                                          outstandingBalance = l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL,
                                          outstandingInterest = l.OUTSTANDINGINTEREST + l.PASTDUEINTEREST,
                                          startDate = startDate,
                                          endDate = endDate,
                                          facilityGrantDate = l.EFFECTIVEDATE,
                                          staffCode = sta.STAFFCODE,
                                          total = (l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL) + (l.OUTSTANDINGINTEREST + l.PASTDUEINTEREST)

                                      }).ToList().Select(x =>
                                      {
                                          x.subHead = stagMis.Where(f => f.staffCode == x.staffCode).FirstOrDefault().subHead;
                                          x.collateralType = LoanCollateralType(x.loanId, LoanSystemTypeEnum.TermDisbursedFacility);
                                          return x;
                                      }).ToList();

                    return reportData;
                }


            }




        }

        public List<CommercialLoanReport> AllCommercialLoanReport(DateTime startDate, DateTime endDate, int companyid)
        {

            List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB }).ToList();


                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var reportData = (from l in context.TBL_LOAN
                                      join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                      join cu in context.TBL_CURRENCY on l.CURRENCYID equals cu.CURRENCYID
                                      join st in context.TBL_LOAN_STATUS on l.LOANSTATUSID equals st.LOANSTATUSID
                                      join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                      join cas2 in context.TBL_CASA on l.CASAACCOUNTID2 equals cas2.CASAACCOUNTID
                                      join sta in context.TBL_STAFF on l.RELATIONSHIPOFFICERID equals sta.STAFFID
                                      join prod in context.TBL_PRODUCT on l.PRODUCTID equals prod.PRODUCTID
                                      //join pc in context.TBL_PRODUCT_CLASS on prod.PRODUCTCLASSID equals pc.PRODUCTCLASSID
                                      where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                      DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                      && l.COMPANYID == companyid && l.LOANSTATUSID == (short)LoanStatusEnum.Active && prod.PRODUCTTYPEID == (short)LoanProductTypeEnum.CommercialLoan
                                      orderby l.EFFECTIVEDATE descending
                                      select new
                                      {
                                          accountPayTo = cas.PRODUCTACCOUNTNAME,
                                          accountReceiveFrom = cas2.PRODUCTACCOUNTNAME,
                                          capturesDate = l.DATETIMECREATED,
                                          currency = cu.CURRENCYNAME,
                                          customerName = c.LASTNAME + " " + c.FIRSTNAME,
                                          dealDate = (l.DATEAPPROVED.Value == null ? default(DateTime) : l.DATEAPPROVED.Value),
                                          endDate = l.MATURITYDATE,
                                          startDate = l.EFFECTIVEDATE,
                                          interestRate = l.INTERESTRATE,
                                          interestRateChange = 0,
                                          interestToDate = 0,
                                          interestType = "",
                                          loanReferenceNo = l.LOANREFERENCENUMBER,
                                          narration = "",
                                          principalAmount = l.PRINCIPALAMOUNT,
                                          status = st.ACCOUNTSTATUS,
                                          tenor = (int)DbFunctions.DiffDays(l.MATURITYDATE, l.EFFECTIVEDATE),
                                          tenorToDate = (int)DbFunctions.DiffDays(l.MATURITYDATE, DateTime.Now),
                                          staffcode = sta.STAFFCODE

                                      }).ToList().Select(x => new CommercialLoanReport
                                      {

                                          accountPayTo = x.accountPayTo,
                                          accountReceiveFrom = x.accountReceiveFrom,
                                          capturesDate = x.capturesDate,
                                          currency = x.currency,
                                          customerName = x.customerName,
                                          dealDate = (DateTime)x.dealDate,
                                          endDate = x.endDate,
                                          startDate = x.startDate,
                                          interestRate = x.interestRate,
                                          interestRateChange = x.interestRateChange,
                                          interestToDate = x.interestToDate,
                                          // interestType = x.interestType,
                                          loanReferenceNo = x.loanReferenceNo,
                                          // narration = x.narration,
                                          principalAmount = x.principalAmount,
                                          status = x.status,
                                          tenor = x.tenor,
                                          tenorToDate = x.tenorToDate,
                                          staffcode = x.staffcode,

                                      }).ToList().Select(y =>
                                      {
                                          if (y.interestType == null)
                                          {
                                              y.interestType = "";
                                          }
                                          if (y.narration == null)
                                          {
                                              y.narration = "";
                                          }
                                          var checkBusinessGroup = subList.Where(f => f.staffCode == y.staffcode).Select(f => f.subHead).FirstOrDefault();
                                          if (checkBusinessGroup != null)
                                          {
                                              y.businessGroup = checkBusinessGroup;
                                          }
                                          else
                                          {
                                              y.businessGroup = "";
                                          }

                                          return y;
                                      }




                    ).ToList();

                    return reportData;
                }

            }

        }

        public List<UnearnedLoanInterestReport> UnearnedLoanInterest(DateTime startDate, DateTime endDate, int companyid)
        {

            List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB }).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {

                    var accruedInterest = (from accr in context.TBL_DAILY_ACCRUAL
                                           join loan in context.TBL_LOAN on accr.REFERENCENUMBER equals loan.LOANREFERENCENUMBER
                                           where loan.COMPANYID == companyid
                                           select new { refnumber = loan.LOANREFERENCENUMBER, amount = accr.DAILYACCURALAMOUNT })
                                         .GroupBy(x => x.refnumber).Select(f => new
                                         {
                                             loanReference = f.FirstOrDefault().refnumber,
                                             accruedInterest = f.Sum(x => x.amount)
                                         });


                    var reportData = (from l in context.TBL_LOAN
                                      join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                      join cu in context.TBL_CURRENCY on l.CURRENCYID equals cu.CURRENCYID
                                      join st in context.TBL_LOAN_STATUS on l.LOANSTATUSID equals st.LOANSTATUSID
                                      join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                      join cas2 in context.TBL_CASA on l.CASAACCOUNTID2 equals cas2.CASAACCOUNTID
                                      join sub in context.TBL_STAFF on l.RELATIONSHIPOFFICERID equals sub.STAFFID
                                      join acc in accruedInterest on l.LOANREFERENCENUMBER equals acc.loanReference
                                      join la in context.TBL_LOAN_ARCHIVE on l.TERMLOANID equals la.LOANID
                                      where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                      DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                       && l.COMPANYID == companyid && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                      orderby l.EFFECTIVEDATE descending ,l.MATURITYDATE descending
                                      select new
                                      {
                                          accountPayTo = cas.PRODUCTACCOUNTNAME,
                                          accountReceiveFrom = cas.PRODUCTACCOUNTNAME,
                                          customerName = c.LASTNAME + " " + c.FIRSTNAME,
                                          endDate = l.MATURITYDATE,
                                          startDate = l.EFFECTIVEDATE,
                                          interestRate = l.INTERESTRATE,
                                          //interestRateChange = context.TBL_LOAN.Where(x=>x.TERMLOANID == la.LOANID  && x.LOANSTATUSID == (short)LoanStatusEnum.Active).Select(x=>x.INTERESTRATE - la.INTERESTRATE),
                                          interestToDate = 0,
                                          interestType = "",
                                          principalAmount = l.PRINCIPALAMOUNT,
                                          tenor = (int)DbFunctions.DiffDays(l.EFFECTIVEDATE, l.MATURITYDATE),
                                          tenorToDate = (int)DbFunctions.DiffDays(l.MATURITYDATE, DateTime.Now),
                                          accruedInterestToDate = acc.accruedInterest,
                                          tenorToMaturity = 0,
                                          unearnedInterestAsAtDate = 0,
                                          staffcode = sub.STAFFCODE,
                                          businessGroup = " ",
                                          laInterestRate = la.INTERESTRATE,
                                          laLoanId = la.LOANID,


                                      }).ToList().Select(x => new UnearnedLoanInterestReport
                                      {
                                          accountPayTo = x.accountPayTo,
                                          accountReceiveFrom = x.accountReceiveFrom,
                                          customerName = x.customerName,
                                          endDate = x.endDate,
                                          startDate = x.startDate,
                                          interestRate = x.interestRate,
                                          //interestRateChange = x.interestRateChange,
                                          interestToDate = x.interestToDate,
                                          interestType = x.interestType,
                                          principalAmount = x.principalAmount,
                                          tenor = x.tenor,
                                          tenorToDate = x.tenorToDate,
                                          accruedInterestToDate = x.accruedInterestToDate,
                                          tenorToMaturity = x.tenorToMaturity,
                                          unearnedInterestAsAtDate = x.unearnedInterestAsAtDate,
                                          staffcode = x.staffcode,
                                          businessGroup = subList.Where(f => f.staffCode == x.staffcode).FirstOrDefault().subHead

                                      }).ToList().Select(x =>
                                     {

                                         x.interestRateChange = context.TBL_LOAN.Where(u => u.TERMLOANID == x.laLoanId && u.LOANSTATUSID == (short)LoanStatusEnum.Active).Select(u => u.INTERESTRATE - x.laInterestRate).FirstOrDefault();
                                         return x;
                                     }).ToList();

                    return reportData;
                }

            }



        }

        public List<ReceivableInterestReport> ReceivableLoanInterest(DateTime startDate, DateTime endDate, int companyid)
        {
            List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB }).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var accruedInterest = (from accr in context.TBL_DAILY_ACCRUAL
                                           join loan in context.TBL_LOAN on accr.REFERENCENUMBER equals loan.LOANREFERENCENUMBER
                                           select new { refnumber = loan.LOANREFERENCENUMBER, amount = accr.DAILYACCURALAMOUNT })
                                           .GroupBy(x => x.refnumber).Select(f => new
                                           {
                                               loanReference = f.FirstOrDefault().refnumber,
                                               accruedInterest = f.Sum(x => x.amount)
                                           });


                    var reportData = (from l in context.TBL_LOAN
                                      join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                      join cu in context.TBL_CURRENCY on l.CURRENCYID equals cu.CURRENCYID
                                      join st in context.TBL_LOAN_STATUS on l.LOANSTATUSID equals st.LOANSTATUSID
                                      join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                      join cas2 in context.TBL_CASA on l.CASAACCOUNTID2 equals cas2.CASAACCOUNTID
                                      join sub in context.TBL_STAFF on l.RELATIONSHIPOFFICERID equals sub.STAFFID
                                      join acc in accruedInterest on l.LOANREFERENCENUMBER equals acc.loanReference
                                      where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                      DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                      && l.COMPANYID == companyid && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                      orderby l.EFFECTIVEDATE descending
                                      select new
                                      {
                                          accountPayTo = cas.PRODUCTACCOUNTNAME,
                                          accountReceiveFrom = cas.PRODUCTACCOUNTNAME,
                                          customerName = c.LASTNAME + " " + c.FIRSTNAME,
                                          endDate = l.MATURITYDATE,
                                          startDate = l.EFFECTIVEDATE,
                                          interestRate = l.INTERESTRATE,
                                          interestRateChange = 0,
                                          interestToDate = 0,
                                          interestType = "",
                                          principalAmount = l.PRINCIPALAMOUNT,
                                          tenor = (int)DbFunctions.DiffDays(l.MATURITYDATE, l.EFFECTIVEDATE),
                                          tenorToDate = (int)DbFunctions.DiffDays(l.MATURITYDATE, DateTime.Now),
                                          accruedInterestToDate = acc.accruedInterest,
                                          tenorToMaturity = 0,
                                          staffcode = sub.STAFFCODE
                                      }).ToList().Select(x => new ReceivableInterestReport
                                      {
                                          accountPayTo = x.accountPayTo,
                                          accountReceiveFrom = x.accountReceiveFrom,
                                          customerName = x.customerName,
                                          endDate = x.endDate,
                                          startDate = x.startDate,
                                          interestRate = x.interestRate,
                                          interestRateChange = x.interestRateChange,
                                          interestToDate = x.interestToDate,
                                          interestType = "",
                                          principalAmount = x.principalAmount,
                                          tenor = x.tenor,
                                          tenorToDate = x.tenorToDate,
                                          accruedInterestToDate = x.accruedInterestToDate,
                                          tenorToMaturity = 0,
                                          staffcode = x.staffcode,
                                          businessGroup = subList.Where(f => f.staffCode == x.staffcode).FirstOrDefault().subHead
                                      }).ToList();

                    return reportData;
                }



            }


        }

        public List<CashBacked> CashBackedReport(DateTime startDate, DateTime endDate, int companyid)
        {

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var cashbackedData = (from l in context.TBL_LOAN
                                      join cm in context.TBL_LOAN_COLLATERAL_MAPPING on l.TERMLOANID equals cm.LOANID
                                      join ca in context.TBL_CASA on l.CASAACCOUNTID equals ca.CASAACCOUNTID
                                      join cd in context.TBL_COLLATERAL_DEPOSIT on cm.COLLATERALCUSTOMERID equals cd.COLLATERALCUSTOMERID
                                      join cust in context.TBL_CUSTOMER on l.CUSTOMERID equals cust.CUSTOMERID
                                      join cus in context.TBL_COLLATERAL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                      join ct in context.TBL_COLLATERAL_TYPE on cus.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                                      join b in context.TBL_BRANCH on ca.BRANCHID equals b.BRANCHID
                                      join curr in context.TBL_CURRENCY on l.CURRENCYID equals curr.CURRENCYID
                                      where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                      DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                      && l.COMPANYID == companyid && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                      && l.LOANSYSTEMTYPEID == cm.LOANSYSTEMTYPEID
                                      orderby l.EFFECTIVEDATE descending
                                      select new
                                      {
                                          accountName = cust.LASTNAME + " " + cust.FIRSTNAME,
                                          accountNo = ca.PRODUCTACCOUNTNAME,
                                          branch = b.BRANCHNAME,
                                          currencyType = curr.CURRENCYNAME,
                                          depositAccountNo = cd.ACCOUNTNUMBER,
                                          loanBalance = l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL,
                                          loanBalanceForeignCurrency = (l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL),
                                          loanLimit = 0,
                                          loanLimitForeignCurrency = 0,
                                          securityInTheNameOf = cus.TBL_CUSTOMER.LASTNAME + " " + cus.TBL_CUSTOMER.FIRSTNAME,
                                          securityType = ct.COLLATERALTYPENAME,
                                          securityValue = cd.SECURITYVALUE,
                                          exchangeRate = l.EXCHANGERATE,
                                          loanReferenceNumber = l.LOANREFERENCENUMBER
                                      }).ToList().Select(x => new CashBacked
                                      {
                                          accountName = x.accountName,
                                          accountNo = x.accountNo,
                                          branch = x.branch,
                                          currencyType = x.currencyType,
                                          depositAccountNo = x.depositAccountNo,
                                          loanBalance = x.loanBalance,
                                          loanBalanceForeignCurrency = x.loanBalanceForeignCurrency * (decimal)x.exchangeRate,
                                          loanLimit = 0,
                                          loanLimitForeignCurrency = 0,
                                          securityInTheNameOf = x.securityInTheNameOf,
                                          securityType = x.securityType,
                                          securityValue = x.securityValue,
                                          loanReferenceNumber = x.loanReferenceNumber,
                                      }).ToList();



                return cashbackedData;

            }


        }

        public List<CashBackedBondAndGuarantee> CashBackedBondAndGuarantee(DateTime startDate, DateTime endDate, int companyid)
        {

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var reportData = (from p in context.TBL_PRODUCT
                                  join lc in context.TBL_LOAN_CONTINGENT on p.PRODUCTID equals lc.PRODUCTID
                                  join l in context.TBL_LOAN on lc.CUSTOMERID equals l.CUSTOMERID
                                  join cm in context.TBL_LOAN_COLLATERAL_MAPPING on l.TERMLOANID equals cm.LOANID
                                  join cc in context.TBL_COLLATERAL_CASA on cm.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                                  join cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                  //join b in context.TBL_CUSTOMER on cm.COLLATERALCUSTOMERID equals b.CUSTOMERID
                                  join co in context.TBL_COLLATERAL_CUSTOMER on cus.CUSTOMERID equals co.CUSTOMERID
                                  join t in context.TBL_COLLATERAL_TYPE on co.COLLATERALTYPEID equals t.COLLATERALTYPEID
                                  join cur in context.TBL_CURRENCY on lc.CURRENCYID equals cur.CURRENCYID
                                  where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                  DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                 && l.COMPANYID == companyid
                                  orderby l.EFFECTIVEDATE descending

                                  select new
                                  {
                                      accountNo = cc.ACCOUNTNUMBER,
                                      beneficiary = cus.LASTNAME + " " + cus.FIRSTNAME,
                                      bondAmount = lc.CONTINGENTAMOUNT,
                                      bondGuaranteeType = p.PRODUCTNAME,
                                      cashSecurityAccount = cc.ACCOUNTNUMBER,
                                      currencyType = cur.CURRENCYNAME,
                                      customerId = l.CUSTOMERID,
                                      customerName = cus.LASTNAME + " " + cus.FIRSTNAME,
                                      dateIssue = l.DATETIMECREATED,
                                      nairaEqualvalentOfFCY = lc.CONTINGENTAMOUNT,
                                      purpose = cc.REMARK,
                                      serialNo = p.PRODUCTCODE,
                                      typeofSecurity = t.COLLATERALTYPENAME,
                                      exchangerate = l.EXCHANGERATE,
                                      securityValue = cc.SECURITYVALUE
                                      

                                  }).ToList().Select(x => new CashBackedBondAndGuarantee
                                  {
                                      accountNo = x.accountNo,
                                      beneficiary = x.beneficiary,
                                      bondAmount = x.bondAmount,
                                      bondGuaranteeType = x.bondGuaranteeType,
                                      cashSecurityAccount = x.cashSecurityAccount,
                                      currencyType = x.currencyType,
                                      customerId = x.customerId,
                                      customerName = x.customerName,
                                      dateIssue = x.dateIssue,
                                      nairaEqualvalentOfFCY = x.nairaEqualvalentOfFCY * (decimal)x.exchangerate,
                                      purpose = x.purpose,
                                      serialNo = x.serialNo,
                                      typeofSecurity = x.typeofSecurity,
                                      securityValue = x.securityValue
                                  }).ToList();

                return reportData.ToList();
            }


        }

        public List<WeeklyRecoveryReportFINCON> WeeklyRecoveryReportFINCON(DateTime startDate, DateTime endDate, int companyid)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var reportData = (from l in context.TBL_LOAN
                                  join lp in context.TBL_LOAN_PRUDENTIALGUIDELINE on l.INT_PRUDENT_GUIDELINE_STATUSID equals lp.PRUDENTIALGUIDELINETYPEID
                                  join cu in context.TBL_CUSTOMER on l.CUSTOMERID equals cu.CUSTOMERID
                                  join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                  join cam in context.TBL_LOAN_CAMSOL on l.TERMLOANID equals cam.LOANID
                                  join br in context.TBL_BRANCH on l.BRANCHID equals br.BRANCHID
                                  join a in context.TBL_COLLATERAL_CUSTOMER on cu.CUSTOMERID equals a.CUSTOMERID
                                  join c in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals c.COLLATERALTYPEID
                                  where lp.STATUSNAME != null && (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                  DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                  && l.COMPANYID == companyid
                                  orderby l.EFFECTIVEDATE descending
                                  select new WeeklyRecoveryReportFINCON
                                  {
                                      loanReferenceNumber = l.LOANREFERENCENUMBER,
                                      principal = l.PRINCIPALAMOUNT,
                                      interest = l.OUTSTANDINGINTEREST,
                                      status = lp.STATUSNAME,
                                      effectiveDate = l.EFFECTIVEDATE,
                                      maturityDate = l.MATURITYDATE,
                                      customerName = cu.FIRSTNAME + " " + " " + cu.MIDDLENAME + " " + " " + cu.LASTNAME,
                                      account = cas.PRODUCTACCOUNTNUMBER,
                                      camsolAccount = cam.ACCOUNTNUMBER,
                                      branchName = br.BRANCHNAME,
                                      businessDevelopmentManager = "",
                                      classification = "",
                                      source = "",
                                      collateral = c.COLLATERALTYPENAME,
                                      suspense = cam.INTERESTINSUSPENSE,
                                      outStandingAmount = cam.PRINCIPAL

                                  }).ToList();


                                  

                return reportData;
            }
        }

        public List<CashCollaterizedCredits> CashCollaterizedCredits(DateTime startDate, DateTime endDate, int companyid)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var reportData = (from l in context.TBL_LOAN
                                  join la in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals la.LOANAPPLICATIONDETAILID
                                  join a in context.TBL_LOAN_APPLICATION on la.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                  join ca in context.TBL_CASA on l.CASAACCOUNTID equals ca.CASAACCOUNTID
                                  join cm in context.TBL_LOAN_COLLATERAL_MAPPING on l.TERMLOANID equals cm.LOANID
                                  join ccust in context.TBL_COLLATERAL_CUSTOMER on cm.COLLATERALCUSTOMERID equals ccust.COLLATERALCUSTOMERID
                                  join li in context.TBL_CASA_LIEN on ca.PRODUCTACCOUNTNUMBER equals li.PRODUCTACCOUNTNUMBER
                                  where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                   DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                  && l.COMPANYID == companyid && l.ISDISBURSED == true
                                  orderby l.EFFECTIVEDATE descending
                                  select new CashCollaterizedCredits
                                  {
                                      availablebalance = a.APPROVEDAMOUNT,
                                      cashBalance = ca.AVAILABLEBALANCE,
                                      lien = " ",
                                      //lienamount = li.LIENAMOUNT,
                                      overdraftAccount = l.LOANREFERENCENUMBER,
                                      loanOverdraftAccountBalance = l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL,
                                      hasLien = ca.HASLIEN,

                                      productaccountnumber = li.PRODUCTACCOUNTNUMBER
                                  }).ToList().Select(x =>

                                  {

                                      if (x.hasLien == false)
                                      {
                                          x.isLien = "No";
                                      }
                                      else
                                      {
                                          x.isLien = "Yes";
                                      }

                                      return x;
                                  }).ToList();

                return reportData;
            }
        }

        //Report on Cash Collaterized Credits

        public static List<DropdownParam> GetProductClass()
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var allProductClass = (from f in context.TBL_PRODUCT_CLASS
                                       select new DropdownParam
                                       {
                                           valueId = f.PRODUCTCLASSID,
                                           valueName = f.PRODUCTCLASSNAME
                                       }).ToList();

                return allProductClass.ToList();
            }


        }

        public List<ExceptionReportViewModel> ExceptionReportForTradeTransactions(DateTime startDate, DateTime endDate, int companyid)
        {

            List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB }).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {

                    var accruedInterest = (from accr in context.TBL_DAILY_ACCRUAL
                                           join loan in context.TBL_LOAN on accr.REFERENCENUMBER equals loan.LOANREFERENCENUMBER
                                           select new { refnumber = loan.LOANREFERENCENUMBER, amount = accr.DAILYACCURALAMOUNT })
                                         .GroupBy(x => x.refnumber).Select(f => new
                                         {
                                             loanReference = f.FirstOrDefault().refnumber,
                                             accruedInterest = f.Sum(x => x.amount)
                                         });


                    var reportData = (from l in context.TBL_LOAN
                                      join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                      join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                      join cu in context.TBL_CURRENCY on l.CURRENCYID equals cu.CURRENCYID
                                      join st in context.TBL_LOAN_STATUS on l.LOANSTATUSID equals st.LOANSTATUSID
                                      join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                      join cas2 in context.TBL_CASA on l.CASAACCOUNTID2 equals cas2.CASAACCOUNTID
                                      join sub in context.TBL_STAFF on l.RELATIONSHIPOFFICERID equals sub.STAFFID
                                      join acc in accruedInterest on l.LOANREFERENCENUMBER equals acc.loanReference
                                      where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                      DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                      && l.COMPANYID == companyid && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                      orderby l.EFFECTIVEDATE descending
                                      select new ExceptionReportViewModel
                                      {
                                          branchCode = b.BRANCHCODE,
                                          branchName = b.BRANCHNAME,
                                          SBU = stagecontext.STG_STAFFMIS.Where(s => s.STAFFCODE == context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPMANAGERID).Select(o => o.STAFFCODE).FirstOrDefault()).Select(s => s.GROUP_HUB).FirstOrDefault(),
                                          GRP = "",
                                          team = "",
                                          accountNumber = cas.PRODUCTACCOUNTNUMBER,
                                          accountName = cas.PRODUCTACCOUNTNAME,
                                          currencyCode = cu.CURRENCYCODE,

                                      }).ToList();

                    return reportData;
                }

            }

            // businessGroup = subList.Where(f => f.staffCode == x.staffcode).FirstOrDefault().subHead
        }


        public List<LoanViewModel> ContigentLiabilityInformation(int companyId, short loanStatusId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                       //join tt in context.TBL_OPERATIONS on a.OPERATIONID equals tt.OPERATIONID
                                   join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                   join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                   join lp in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                   //join at in context.TBL_LOAN_APPLICATION_TYPE on lp.LOANAPPLICATIONTYPEID equals at.LOANAPPLICATIONTYPEID
                                   join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                   join pr in context.TBL_PRODUCT on a.PRODUCTID equals pr.PRODUCTID
                                   join st in context.TBL_STAFF on a.RELATIONSHIPOFFICERID equals st.STAFFID
                                   //join stm in context.TBL_STAFF on a.RELATIONSHIPMANAGERID equals stm.STAFFID
                                   where a.LOANSTATUSID == loanStatusId //&& (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                                        //DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                   orderby a.DATETIMECREATED descending
                                   select new LoanViewModel
                                   {
                                       //loanId = a.CONTINGENTLOANID,
                                       customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                       branchName = br.BRANCHNAME,
                                       loanReferenceNumber = a.LOANREFERENCENUMBER,
                                       applicationReferenceNumber = lp.APPLICATIONREFERENCENUMBER ?? "N/A",
                                       productName = pr.PRODUCTNAME,
                                       productTypeName = pr.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                       relationshipOfficerName = st.FIRSTNAME + " " + st.MIDDLENAME + " " + st.LASTNAME,
                                       //relationshipManagerName = stm.FIRSTNAME + " " + stm.MIDDLENAME + " " + stm.LASTNAME,
                                       effectiveDate = a.EFFECTIVEDATE,
                                       maturityDate = a.MATURITYDATE,
                                       bookingDate = a.BOOKINGDATE,
                                       dateTimeCreated = a.DATETIMECREATED,
                                       //isDisbursedState = a.ISDISBURSED ? "True" : "False",
                                       //disburserComment = a.DISBURSERCOMMENT,
                                       //operationName = tt.OPERATIONNAME,
                                       casaAccountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                       productAccountName = a.TBL_PRODUCT.PRODUCTNAME,
                                       //loanTypeName = at.LOANAPPLICATIONTYPENAME,
                                       currency = a.TBL_CURRENCY.CURRENCYNAME,
                                       // ApprovalStatus = context.TBL_APPROVAL_STATUS.Where(x => x.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(x => x.APPROVALSTATUSNAME).FirstOrDefault(),
                                       loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == a.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS).FirstOrDefault(),
                                       //productPriceIndex = ld.PRODUCTPRICEINDEXID != null ? "+ " + context.TBL_PRODUCT_PRICE_INDEX.Where(x => x.PRODUCTPRICEINDEXID == ld.PRODUCTPRICEINDEXID).Select(x => x.PRICEINDEXNAME).FirstOrDefault() : "",

                                   }).ToList();
                return loanDetails;
            }
        }


        public List<MiddleOfficeViewModel> MiddleOfficeReport(DateTime startDate, DateTime endDate, int companyid)
        {
            List<SbHead> subList = new List<SbHead>();
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SbHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, teamUnit = sl.TEAM_UNIT }).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {

                    var middleOffice = (from a in context.TBL_LOAN_APPLICATION_DETL_INV


                                        join ld in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                        join cu in context.TBL_CUSTOMER on ld.CUSTOMERID equals cu.CUSTOMERID
                                        join pr in context.TBL_LOAN_PRINCIPAL on a.PRINCIPALID equals pr.PRINCIPALID
                                        join cb in context.TBL_STAFF on a.CREATEDBY equals cb.STAFFID
                                        join jb in context.TBL_JOB_REQUEST on ld.LOANAPPLICATIONDETAILID equals jb.TARGETID
                                        join cas in context.TBL_CASA on cu.CUSTOMERID equals cas.CUSTOMERID
                                        //join br in context.TBL_BRANCH on jb.BRANCHID equals br.BRANCHID
                                        join curr in context.TBL_CURRENCY on a.INVOICE_CURRENCYID equals curr.CURRENCYID

                                        where (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                               DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                            && cb.COMPANYID == companyid
                                        orderby a.DATETIMECREATED descending

                                        select new MiddleOfficeViewModel
                                        {
                                            statusId = jb.REQUESTSTATUSID,
                                            status = jb.TBL_JOB_REQUEST_STATUS.STATUSNAME,
                                            statusFeedbackId = jb.JOB_STATUS_FEEDBACKID,
                                            branchName = context.TBL_BRANCH.Where(u => u.BRANCHID == jb.BRANCHID).Select(x => x.BRANCHNAME).FirstOrDefault(),
                                            customerName = cu.FIRSTNAME + " " + " " + cu.MAIDENNAME + " " + " " + cu.LASTNAME,
                                            modVerificationOfficerName = cb.FIRSTNAME + " " + " " + cb.MIDDLENAME + " " + " " + cb.LASTNAME,
                                            modVerificationOfficerStaffNo = cb.STAFFCODE,
                                            principalName = pr.NAME,
                                            invoiceDate = a.INVOICE_DATE,
                                            invoiceNumber = a.INVOICENO,
                                            customerAccount = cas.PRODUCTACCOUNTNUMBER,
                                            dateTimeCreated = a.DATETIMECREATED,
                                            dateTimeUpdated = a.DATETIMEUPDATED,
                                            jobRequestCode = jb.JOBREQUESTCODE,
                                            currencyType = curr.CURRENCYNAME,
                                            staffCode = cb.STAFFCODE,
                                            loanType = context.TBL_LOAN_APPLICATION_TYPE.Where(o => o.LOANAPPLICATIONTYPEID == ld.TBL_LOAN_APPLICATION.LOANAPPLICATIONTYPEID).Select(o => o.LOANAPPLICATIONTYPENAME).FirstOrDefault(),

                                            createdByName = cb.FIRSTNAME + " " + " " + cb.MIDDLENAME + " " + " " + cb.LASTNAME,
                                        }).ToList().Select(x =>
                                        {
                                            var checkForBusinessGroup = subList.Where(f => f.staffCode == x.staffCode).Select(f => f.subHead).FirstOrDefault();

                                            if (checkForBusinessGroup == null)
                                            {
                                                x.businessGroup = "";
                                            }
                                            else if (checkForBusinessGroup != null)
                                            {
                                                x.businessGroup = checkForBusinessGroup;
                                            }

                                            var checkForBusinessUnit = subList.Where(f => f.staffCode == x.staffCode).Select(f => f.teamUnit).FirstOrDefault();
                                            if (checkForBusinessUnit == null)
                                            {
                                                x.businessUnit = "";
                                            }
                                            else if (checkForBusinessUnit != null)
                                            {
                                                x.businessUnit = checkForBusinessUnit;
                                            }
                                            return x;
                                        }).ToList();

                    foreach (var item in middleOffice)
                    {

                        if (item.statusId == (short)JobRequestStatusEnum.disapproved)
                        {
                            var feedback = context.TBL_JOB_REQUEST_STATUS_FEEDBAK.Where(x => x.JOB_STATUS_FEEDBACKID == item.statusFeedbackId);
                            if (feedback != null)
                                item.middleOfficeComment = feedback.FirstOrDefault().JOB_STATUS_FEEDBACK_NAME;
                            else item.middleOfficeComment = "None";
                        }
                        else if (item.statusId == (short)JobRequestStatusEnum.approved)
                            item.middleOfficeComment = "Approved";
                        else if (item.statusId == (short)JobRequestStatusEnum.cancel) item.middleOfficeComment = "Cancelled";
                        else item.middleOfficeComment = "N/A";


                    }

                    return middleOffice;
                }
            }
        }

        public List<CollateralValuationViewModel> CollateralValuationReport(DateTime startDate, DateTime endDate, int companyid)
        {
            List<SbHead> stagMis = new List<SbHead>();
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                stagMis = (from sl in stagecontext.STG_STAFFMIS select new SbHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB }).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {

                    var collateralValuation = (from a in context.TBL_LOAN_COLLATERAL_MAPPING
                                               join l in context.TBL_LOAN on a.LOANID equals l.TERMLOANID
                                               join ccu in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals ccu.COLLATERALCUSTOMERID
                                               join e in context.TBL_COLLATERAL_TYPE_SUB on ccu.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                               join cu in context.TBL_CUSTOMER on ccu.CUSTOMERID equals cu.CUSTOMERID
                                               join br in context.TBL_BRANCH on cu.BRANCHID equals br.BRANCHID
                                               join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                               join cas in context.TBL_COLLATERAL_CASA on a.COLLATERALCUSTOMERID equals cas.COLLATERALCUSTOMERID
                                               join lpd in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals lpd.LOANAPPLICATIONDETAILID
                                               join lp in context.TBL_LOAN_APPLICATION on lpd.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                               join ip in context.TBL_COLLATERAL_IMMOVE_PROPERTY on ccu.COLLATERALCUSTOMERID equals ip.COLLATERALCUSTOMERID
                                               join p in context.TBL_COLLATERAL_ITEM_POLICY on ccu.COLLATERALCUSTOMERID equals p.COLLATERALCUSTOMERID
                                               join v in context.TBL_COLLATERAL_VISITATION on ccu.COLLATERALCUSTOMERID equals v.COLLATERALCUSTOMERID
                                               join c in context.TBL_CITY on ip.CITYID equals c.CITYID
                                               join cgm in context.TBL_CUSTOMER_GROUP_MAPPING on ccu.CUSTOMERID equals cgm.CUSTOMERID
                                               join cg in context.TBL_CUSTOMER_GROUP on cgm.CUSTOMERGROUPID equals cg.CUSTOMERGROUPID
                                               join pe in context.TBL_COLLATERAL_PERFECTN_STAT on ip.PERFECTIONSTATUSID equals pe.PERFECTIONSTATUSID
                                               join cb in context.TBL_STAFF on v.CREATEDBY equals cb.CREATEDBY

                                               where (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                      DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                   && cu.COMPANYID == companyid && a.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility
                                               orderby a.DATETIMECREATED descending

                                               select new CollateralValuationViewModel
                                               {
                                                   solId = br.BRANCHID,
                                                   branchName = br.BRANCHNAME,
                                                   groupDescription = cg.GROUPDESCRIPTION,
                                                   customerName = cu.FIRSTNAME + " " + " " + cu.MIDDLENAME + " " + " " + cu.LASTNAME,
                                                   accountNumber = cas.ACCOUNTNUMBER,
                                                   bvn = cu.CUSTOMERBVN,
                                                   facilityType = context.TBL_PRODUCT_TYPE.Where(o => o.PRODUCTTYPEID == lpd.TBL_PRODUCT.PRODUCTTYPEID).Select(o => o.PRODUCTTYPENAME).FirstOrDefault(),
                                                   expiryDate = (DateTime)lpd.EXPIRYDATE,
                                                   balance = cas.AVAILABLEBALANCE,
                                                   currency = cur.CURRENCYNAME,
                                                   collateralDetail = lp.COLLATERALDETAIL,
                                                   perfectionStatus = pe.PERFECTIONSTATUSNAME,
                                                   location = ip.PROPERTYADDRESS,
                                                   valuationDate = ip.LASTVALUATIONDATE,
                                                   dateOfInsurance = p.STARTDATE,
                                                   dateOfInspection = v.VISITATIONDATE,
                                                   stateOfCollateral = c.CITYNAME,
                                                   inspectingStaffNo = cb.STAFFCODE,
                                                   dateTimeCreated = v.DATETIMECREATED,
                                                   dateGranted = l.EFFECTIVEDATE,
                                                   collateralValue = ccu.COLLATERALVALUE,
                                                   relationshipManagerId = l.RELATIONSHIPMANAGERID,
                                                   //tenor = (l.EFFECTIVEDATE.Date - l.MATURITYDATE.Date).Days,








                                               }).ToList().Select(x =>
                                               {
                                                   var checkForGroupHead = stagMis.Where(f => f.staffCode == x.inspectingStaffNo).Select(f => f.subHead).FirstOrDefault();
                                                   if (checkForGroupHead == null)
                                                   {
                                                       x.groupHead = "";
                                                   }
                                                   else if (checkForGroupHead != null)
                                                   {
                                                       x.groupHead = checkForGroupHead;
                                                   }

                                                   return x;
                                               }).ToList();



                    return collateralValuation;
                }
            }
        }

        public List<AgeAnalysisViewModel> AgeAnalysisReport(DateTime startDate, DateTime endDate, int companyid)
        {
            DateTime dateTime = DateTime.Now;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var ageAnalysis = (from l in context.TBL_LOAN
                                   join cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                   join br in context.TBL_BRANCH on l.BRANCHID equals br.BRANCHID
                                   join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                   join custmap in context.TBL_CUSTOMER_GROUP_MAPPING on cus.CUSTOMERID equals custmap.CUSTOMERID
                                   join custgr in context.TBL_CUSTOMER_GROUP on custmap.CUSTOMERGROUPID equals custgr.CUSTOMERGROUPID
                                   join ld in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                   join lpd in context.TBL_LOAN_PRUDENTIALGUIDELINE on l.USER_PRUDENTIAL_GUIDE_STATUSID equals lpd.PRUDENTIALGUIDELINESTATUSID

                                   where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                     DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                  && l.COMPANYID == companyid
                                   orderby l.DATETIMECREATED descending



                                   select new AgeAnalysisViewModel()
                                   {
                                       customerName = cus.FIRSTNAME + " " + " " + cus.MIDDLENAME + " " + " " + cus.LASTNAME,
                                       groupName = custgr.GROUPNAME,
                                       branchName = br.BRANCHNAME,
                                       schemmeCode = br.BRANCHCODE,
                                       operativeAccount = cas.PRODUCTACCOUNTNUMBER,
                                       status = lpd.STATUSNAME,
                                       totalExposure = l.PASTDUEPRINCIPAL + l.PASTDUEINTEREST + l.INTERESTONPASTDUEINTEREST + l.INTERESTONPASTDUEPRINCIPAL + l.OUTSTANDINGINTEREST + l.OUTSTANDINGPRINCIPAL,
                                       disbursedDate = (l.DISBURSEDATE.Value == null ? default(DateTime) : l.DISBURSEDATE.Value),
                                       expireDate = l.MATURITYDATE,
                                       pastDueDays = (int)DbFunctions.DiffDays((l.PASTDUEDATE.Value == null ? default(DateTime) : l.PASTDUEDATE.Value), dateTime),
                                       sanctionLimit = 0,
                                       businessDevelopmentManager = "",





                                   }).ToList();

                return ageAnalysis;

            }
        }

        public List<CreditScheduleViewModel> CreditScheduleReport(DateTime startDate, DateTime endDate, int companyid)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var creditSchedule = (from a in context.TBL_LOAN_COLLATERAL_MAPPING
                                      join l in context.TBL_LOAN on a.LOANID equals l.TERMLOANID
                                      join ccu in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals ccu.COLLATERALCUSTOMERID
                                      ///join e in context.TBL_COLLATERAL_TYPE_SUB on ccu.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                      join cu in context.TBL_CUSTOMER on ccu.CUSTOMERID equals cu.CUSTOMERID
                                      //join br in context.TBL_BRANCH on cu.BRANCHID equals br.BRANCHID
                                      join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                      join cas in context.TBL_COLLATERAL_CASA on a.COLLATERALCUSTOMERID equals cas.COLLATERALCUSTOMERID
                                      join lpd in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals lpd.LOANAPPLICATIONDETAILID
                                      //join lp in context.TBL_LOAN_APPLICATION on lpd.LOANAPPLICATIONID equals lp.LOANAPPLICATIONID
                                      // join ip in context.TBL_COLLATERAL_IMMOVE_PROPERTY on ccu.COLLATERALCUSTOMERID equals ip.COLLATERALCUSTOMERID
                                      //join p in context.TBL_COLLATERAL_ITEM_POLICY on ccu.COLLATERALCUSTOMERID equals p.COLLATERALCUSTOMERID
                                      //join v in context.TBL_COLLATERAL_VISITATION on ccu.COLLATERALCUSTOMERID equals v.COLLATERALCUSTOMERID
                                      join c in context.TBL_SUB_SECTOR on l.SUBSECTORID equals c.SUBSECTORID
                                      join s in context.TBL_SECTOR on c.SECTORID equals s.SECTORID
                                      join cgm in context.TBL_CUSTOMER_GROUP_MAPPING on ccu.CUSTOMERID equals cgm.CUSTOMERID
                                      join cg in context.TBL_CUSTOMER_GROUP on cgm.CUSTOMERGROUPID equals cg.CUSTOMERGROUPID
                                      //join pe in context.TBL_COLLATERAL_PERFECTN_STAT on ip.PERFECTIONSTATUSID equals pe.PERFECTIONSTATUSID
                                      join f in context.TBL_FREQUENCY_TYPE on l.INTERESTFREQUENCYTYPEID equals f.FREQUENCYTYPEID
                                      join fs in context.TBL_FREQUENCY_TYPE on l.PRINCIPALFREQUENCYTYPEID equals fs.FREQUENCYTYPEID

                                      where (DbFunctions.TruncateTime(a.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                     DbFunctions.TruncateTime(a.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                  && cu.COMPANYID == companyid
                                      orderby a.DATETIMECREATED descending


                                      select new CreditScheduleViewModel()
                                      {
                                          accountNumber = cas.ACCOUNTNUMBER,
                                          bvn = cu.CUSTOMERBVN,
                                          customerName = cu.FIRSTNAME + " " + " " + cu.MIDDLENAME + " " + " " + cu.LASTNAME,
                                          tin = "",
                                          facilityType = context.TBL_PRODUCT_TYPE.Where(o => o.PRODUCTTYPEID == lpd.TBL_PRODUCT.PRODUCTTYPEID).Select(o => o.PRODUCTTYPENAME).FirstOrDefault(),
                                          glSubHeadCode = "",
                                          businessType = "",
                                          sector = s.NAME,
                                          subSector = c.NAME,
                                          customerId = cu.CUSTOMERID,
                                          groupOrganization = cg.GROUPNAME,
                                          dateGranted = l.EFFECTIVEDATE,
                                          lastCreditDate = DateTime.Now,
                                          expiryDate = l.MATURITYDATE,
                                          sanctionLimit = " ",
                                          previousLimit = " ",
                                          repaymentFrequencyForInterest = f.MODE,
                                          repaymentFrequencyForPrincipal = fs.MODE,
                                          cumInterestDueNotYetPaid = l.PASTDUEINTEREST,
                                          cumRepaymentAmountDue = l.OUTSTANDINGPRINCIPAL,
                                          cumRepaymentAmountPaid = l.PRINCIPALAMOUNT - l.OUTSTANDINGPRINCIPAL,
                                          cumPrincipalDueNotYetPaid = l.PASTDUEPRINCIPAL,
                                          interestRate = l.INTERESTRATE,
                                          tenor = (int)DbFunctions.DiffDays(l.MATURITYDATE, l.EFFECTIVEDATE),
                                          balance = cas.AVAILABLEBALANCE,
                                          curr = cur.CURRENCYNAME,
                                          bankClassification = "",
                                          detailsOfSecuritiesOthers = "",
                                          collateralValue = ccu.COLLATERALVALUE,
                                          collateralStatus = ccu.APPROVALSTATUS,








                                      }).ToList();

                return creditSchedule;
            }

        }


        public List<SanctionLimitReportViewModel> SanctionLimitReport(DateTime startDate, DateTime endDate, int companyid)
        {
            List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION }).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var sanctionLimit = (from l in context.TBL_LOAN
                                         join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                         join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                         join cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                         join st in context.TBL_STAFF on l.CREATEDBY equals st.STAFFID
                                         join p in context.TBL_PRODUCT on l.PRODUCTID equals p.PRODUCTID
                                         join f in context.TBL_FREQUENCY_TYPE on l.INTERESTFREQUENCYTYPEID equals f.FREQUENCYTYPEID
                                         join fs in context.TBL_FREQUENCY_TYPE on l.PRINCIPALFREQUENCYTYPEID equals fs.FREQUENCYTYPEID
                                         join ap in context.TBL_APPROVAL_LEVEL_STAFF on st.STAFFID equals ap.STAFFID
                                         join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                         join ld in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                         join lpd in context.TBL_LOAN_PRUDENTIALGUIDELINE on l.USER_PRUDENTIAL_GUIDE_STATUSID equals lpd.PRUDENTIALGUIDELINESTATUSID
                                         join rm in context.TBL_STAFF on l.RELATIONSHIPMANAGERID equals rm.STAFFID

                                         where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                     DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                  && l.COMPANYID == companyid
                                         orderby l.DATETIMECREATED descending
                                         select new SanctionLimitReportViewModel()
                                         {
                                             initSol = "",
                                             initSoldDesc = "",
                                             branchCode = b.BRANCHCODE,
                                             branchName = b.BRANCHNAME,
                                             currency = cur.CURRENCYNAME,
                                             loanOdAcct = l.LOANREFERENCENUMBER,
                                             accountNumber = cas.PRODUCTACCOUNTNUMBER,
                                             acctopNdate = DateTime.Now,
                                             customerId = cus.CUSTOMERID,
                                             glSubHeadCode = "",
                                             productName = p.PRODUCTNAME,
                                             accountName = cas.PRODUCTACCOUNTNAME,
                                             sanctionLimit = "",
                                             limitSanctionDate = DateTime.Now,
                                             applicableDate = DateTime.Now,
                                             status = lpd.STATUSNAME,
                                             limitExpiryDate = l.MATURITYDATE,
                                             tenor = (int)DbFunctions.DiffDays(l.MATURITYDATE, l.EFFECTIVEDATE),
                                             interestStartDate = DateTime.Now,
                                             interestRepaymentFrequency = f.MODE,
                                             principalStartDate = DateTime.Now,
                                             lchgUserId = "",
                                             lchgTime = DateTime.Now,
                                             rcreUserid = "",
                                             rcreTime = DateTime.Now,
                                             entererId = 0,
                                             entererName = "",
                                             entererCode = "",
                                             entererLevel = "",
                                             entererAppName = "",
                                             staffId = st.STAFFID,
                                             staffName = st.FIRSTNAME + " " + " " + st.MIDDLENAME + " " + " " + st.LASTNAME,
                                             staffCode = st.STAFFCODE,
                                             staffLevel = "",
                                             authAppName = "",
                                             clrBalanceAmount = 0,
                                             relationshipManagerCode = rm.STAFFCODE,

                                             limitLevel = "",
                                             limitAccountInterestRate = 0,
                                             limitInterestRate = 0,
                                             cotCode = "",
                                             relationshipManagerId = l.RELATIONSHIPMANAGERID,
                                             branchId = l.BRANCHID,











                                         }).ToList().Select(x =>
                                         {
                                             x.sbuCode = subList.Where(s => s.staffCode == context.TBL_STAFF.Where(o => o.STAFFID == x.relationshipManagerId).Select(o => o.STAFFCODE).FirstOrDefault()).Select(s => s.staffCode).FirstOrDefault();
                                             x.sbuName = subList.Where(s => s.staffCode == context.TBL_STAFF.Where(o => o.STAFFID == x.relationshipManagerId).Select(o => o.STAFFCODE).FirstOrDefault()).Select(s => s.firstName + " " + " " + s.middleName + " " + " " + s.lastName).FirstOrDefault();
                                             x.sbuBranch = subList.Where(s => s.staffCode == context.TBL_STAFF.Where(o => o.STAFFID == x.relationshipManagerId).Select(o => o.STAFFCODE).FirstOrDefault()).Select(s => s.region).FirstOrDefault();
                                             x.relationshipManagerSbu = subList.Where(s => s.staffCode == context.TBL_STAFF.Where(o => o.STAFFID == x.relationshipManagerId).Select(o => o.STAFFCODE).FirstOrDefault()).Select(s => s.subHead).FirstOrDefault();



                                             return x;
                                         }).ToList();

                    return sanctionLimit;
                }
            }
        }

        public List<ImpairedWatchListViewModel> ImpairedWatchListReport(DateTime startDate, DateTime endDate, int companyid)
        {
            List<SubHead> subList = new List<SubHead>();
            List<LoanMart> loanMart = new List<LoanMart>();
            List<SubHeadCode> subHeadCode = new List<SubHeadCode>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION }).ToList();
                loanMart = (from l in stagecontext.STG_LOAN_MART select new LoanMart { branchCode = l.BRANCH, groupCode = l.GROUP_CODE, teamCode = l.TEAM_CODE, buCode = l.BU_CODE, buDescription = l.BU_DESCRIPTION, deskCode = l.DESK_CODE, rmCode = l.RM_CODE, schemeCode = l.SCHEME_CODE, deskDescription = l.DESK_DESCRIPTION, accountName = l.ACCOUNT_NAME, groupDescription = l.GROUP_DESCRIPTION, pastDueDate = l.DAYS_PAST_DUE, sanctionLimit = l.SANCTIONED_LIMIT, schemeType = l.SCHM_TYPE, account = l.ACCOUNT, customerId = l.CUST_ID }).ToList();
                subHeadCode = (from h in stagecontext.STG_GL_SUBHEAD_TBL select new SubHeadCode { glSubHeadCode = h.GL_SUB_HEAD_CODE, schemeCodes = h.SCHM_CODE }).ToList();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var impairedWatchList = (from l in context.TBL_LOAN
                                             join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                             join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                             join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                             where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                         DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                      && l.COMPANYID == companyid
                                             orderby l.DATETIMECREATED descending
                                             select new ImpairedWatchListViewModel
                                             {
                                                 branchCode = b.BRANCHCODE,
                                                 branchName = b.BRANCHNAME,
                                                 customerId = l.CUSTOMERID.ToString(),
                                                 currencyType = cur.CURRENCYNAME,
                                                 clrBalance = cas.AVAILABLEBALANCE,
                                                 interestOverDue = l.PASTDUEINTEREST,
                                                 principalOverDue = l.PASTDUEPRINCIPAL,
                                                 totalExposure = l.PASTDUEPRINCIPAL + l.PASTDUEINTEREST + l.INTERESTONPASTDUEINTEREST + l.INTERESTONPASTDUEPRINCIPAL + l.OUTSTANDINGINTEREST + l.OUTSTANDINGPRINCIPAL,
                                                 interestRate = l.INTERESTRATE,
                                             }).ToList().Select(x =>
                                             {
                                                 var checkForBusinessDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.buDescription).FirstOrDefault();
                                                 if (checkForBusinessDescription == null)
                                                 {
                                                     x.buDescription = "";
                                                 }
                                                 else if (checkForBusinessDescription != null)
                                                 {
                                                     x.buDescription = checkForBusinessDescription;
                                                 }

                                                 var checkForGroupDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.groupDescription).FirstOrDefault();
                                                 if (checkForGroupDescription == null)
                                                 {
                                                     x.groupDescription = "";
                                                 }
                                                 else if (checkForGroupDescription != null)
                                                 {
                                                     x.groupDescription = checkForGroupDescription;
                                                 }
                                                 var checkForTeamDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.teamDescription).FirstOrDefault();
                                                 if (checkForTeamDescription == null)
                                                 {
                                                     x.teamDescription = "";
                                                 }
                                                 else if (checkForTeamDescription != null)
                                                 {
                                                     x.teamDescription = checkForTeamDescription;
                                                 }
                                                 var checkForDeskDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.deskDescription).FirstOrDefault();
                                                 if (checkForDeskDescription == null)
                                                 {
                                                     x.deskDescription = "";
                                                 }
                                                 else if (checkForDeskDescription != null)
                                                 {
                                                     x.deskDescription = checkForDeskDescription;
                                                 }
                                                 var checkForAccountName = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.accountName).FirstOrDefault();
                                                 if (checkForAccountName == null)
                                                 {
                                                     x.accountName = "";
                                                 }
                                                 else if (checkForAccountName != null)
                                                 {
                                                     x.accountName = checkForAccountName;
                                                 }
                                                 var checkForAccount = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.account).FirstOrDefault();
                                                 if (checkForAccount == null)
                                                 {
                                                     x.account = "";
                                                 }
                                                 else if (checkForAccount != null)
                                                 {
                                                     x.account = checkForAccount;
                                                 }
                                                 var checkForSanctionLimit = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.sanctionLimit).FirstOrDefault();
                                                 if (checkForSanctionLimit == null)
                                                 {
                                                     x.sanctionLimit = 0;
                                                 }
                                                 else if (checkForSanctionLimit != null)
                                                 {
                                                     x.sanctionLimit = checkForSanctionLimit;
                                                 }
                                                 var checkForLimitExpiryDate = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.limitExpiryDate).FirstOrDefault();
                                                 if (checkForLimitExpiryDate == null)
                                                 {
                                                     x.limitExpiryDate = DateTime.Now; ;
                                                 }
                                                 else if (checkForLimitExpiryDate != null)
                                                 {
                                                     x.limitExpiryDate = checkForLimitExpiryDate;
                                                 }

                                                 var checkForSchmType = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.schemeType).FirstOrDefault();
                                                 if (checkForSchmType == null)
                                                 {
                                                     x.schemeType = "";
                                                 }
                                                 else if (checkForSchmType != null)
                                                 {
                                                     x.schemeType = checkForSchmType;
                                                 }

                                                 var checkForDueDate = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.pastDueDate).FirstOrDefault();
                                                 if (checkForDueDate == null)
                                                 {
                                                     x.pastDueDate = 0;
                                                 }
                                                 else if (checkForDueDate != null)
                                                 {
                                                     x.pastDueDate = checkForDueDate;
                                                 }
                                                 var checkForSchmCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.schemeCode).FirstOrDefault();
                                                 if (checkForSchmCode == null)
                                                 {
                                                     x.schemeCode = "";
                                                 }
                                                 else if (checkForSchmCode != null)
                                                 {
                                                     x.schemeCode = checkForSchmCode;
                                                 }


                                                 var checkForSubHeadCode = subHeadCode.Where(f => f.schemeCodes == x.schemeCode).Select(f => f.glSubHeadCode).FirstOrDefault();
                                                 if (checkForSubHeadCode == null)
                                                 {
                                                     x.glSubHeadCode = "";
                                                 }
                                                 else if (checkForSubHeadCode != null)
                                                 {
                                                     x.glSubHeadCode = checkForSubHeadCode;
                                                 }
                                                 var checkForBuCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.buCode).FirstOrDefault();
                                                 if (checkForBuCode == null)
                                                 {
                                                     x.buCode = "";
                                                 }
                                                 else if (checkForBuCode != null)
                                                 {
                                                     x.buCode = checkForBuCode;
                                                 }
                                                 var checkForGroupCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.groupCode).FirstOrDefault();
                                                 if (checkForGroupCode == null)
                                                 {
                                                     x.groupCode = "";
                                                 }
                                                 else if (checkForGroupCode != null)
                                                 {
                                                     x.groupCode = checkForGroupCode;
                                                 }
                                                 var checkForTeamCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.teamCode).FirstOrDefault();
                                                 if (checkForTeamCode == null)
                                                 {
                                                     x.teamCode = "";
                                                 }
                                                 else if (checkForTeamCode != null)
                                                 {
                                                     x.teamCode = checkForTeamCode;
                                                 }

                                                 var checkForDeskCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.deskCode).FirstOrDefault();
                                                 if (checkForDeskCode == null)
                                                 {
                                                     x.deskCode = "";
                                                 }
                                                 else if (checkForDeskCode != null)
                                                 {
                                                     x.deskCode = checkForDeskCode;
                                                 }

                                                 var checkForRmCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.rmCode).FirstOrDefault();
                                                 if (checkForRmCode == null)
                                                 {
                                                     x.rmCode = "";
                                                 }
                                                 else if (checkForRmCode != null)
                                                 {
                                                     x.rmCode = checkForRmCode;
                                                 }
                                                 return x;
                                             }).ToList();

                    return impairedWatchList;
                }
            }

        }


        public List<ExpiredViewModel> ExpiredReport(DateTime startDate, DateTime endDate, int companyid)
        {
            List<SubHead> subList = new List<SubHead>();
            List<LoanMart> loanMart = new List<LoanMart>();
            List<SubHeadCode> subHeadCode = new List<SubHeadCode>();
            List<LoanMartWatchList> loanMartWatch = new List<LoanMartWatchList>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION }).ToList();
                loanMart = (from l in stagecontext.STG_LOAN_MART select new LoanMart { branchCode = l.BRANCH, groupCode = l.GROUP_CODE, teamCode = l.TEAM_CODE, buCode = l.BU_CODE, buDescription = l.BU_DESCRIPTION, deskCode = l.DESK_CODE, rmCode = l.RM_CODE, schemeCode = l.SCHEME_CODE, deskDescription = l.DESK_DESCRIPTION, accountName = l.ACCOUNT_NAME, groupDescription = l.GROUP_DESCRIPTION, pastDueDate = l.DAYS_PAST_DUE, sanctionLimit = l.SANCTIONED_LIMIT, schemeType = l.SCHM_TYPE, account = l.ACCOUNT, customerId = l.CUST_ID, classificationDate = l.CLASSIFICATION_DATE, transactionDateBalance = l.TRAN_DATE_BAL, userClassification = l.USER_CLASSIFICATION, expiryDate = l.EXPIRY_DATE }).ToList();
                loanMartWatch = (from lm in stagecontext.STG_LOAN_WATCHLIST select new LoanMartWatchList { subClassification = lm.SUBCLASSIFICATION }).ToList();
                subHeadCode = (from h in stagecontext.STG_GL_SUBHEAD_TBL select new SubHeadCode { glSubHeadCode = h.GL_SUB_HEAD_CODE, schemeCodes = h.SCHM_CODE }).ToList();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var expiredList = (from l in context.TBL_LOAN
                                       join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                       join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                       join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                       join cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                       join rm in context.TBL_STAFF on l.CREATEDBY equals rm.STAFFID
                                       where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                   DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                && l.COMPANYID == companyid
                                       orderby l.DATETIMECREATED descending
                                       select new ExpiredViewModel
                                       {
                                           branchCode = b.BRANCHCODE,
                                           branchName = b.BRANCHNAME,
                                           customerId = l.CUSTOMERID.ToString(),
                                           currencyType = cur.CURRENCYNAME,
                                           customerName = cus.FIRSTNAME + " " + " " + cus.MIDDLENAME + " " + " " + cus.LASTNAME,
                                           rmName = rm.FIRSTNAME + " " + " " + rm.MIDDLENAME + " " + " " + rm.LASTNAME,
                                           //expiryDate = l.MATURITYDATE,


                                       }).ToList().Select(x =>
                                       {
                                           var checkForBusinessDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.buDescription).FirstOrDefault();
                                           if (checkForBusinessDescription == null)
                                           {
                                               x.buDescription = "";
                                           }
                                           else if (checkForBusinessDescription != null)
                                           {
                                               x.buDescription = checkForBusinessDescription;
                                           }

                                           var checkForGroupDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.groupDescription).FirstOrDefault();
                                           if (checkForGroupDescription == null)
                                           {
                                               x.groupDescription = "";
                                           }
                                           else if (checkForGroupDescription != null)
                                           {
                                               x.groupDescription = checkForGroupDescription;
                                           }
                                           var checkForTeamDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.teamDescription).FirstOrDefault();
                                           if (checkForTeamDescription == null)
                                           {
                                               x.teamDescription = "";
                                           }
                                           else if (checkForTeamDescription != null)
                                           {
                                               x.teamDescription = checkForTeamDescription;
                                           }
                                           var checkForDeskDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.deskDescription).FirstOrDefault();
                                           if (checkForDeskDescription == null)
                                           {
                                               x.deskDescription = "";
                                           }
                                           else if (checkForDeskDescription != null)
                                           {
                                               x.deskDescription = checkForDeskDescription;
                                           }
                                           var checkForAccountName = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.accountName).FirstOrDefault();
                                           if (checkForAccountName == null)
                                           {
                                               x.accountName = "";
                                           }
                                           else if (checkForAccountName != null)
                                           {
                                               x.accountName = checkForAccountName;
                                           }
                                           var checkForAccount = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.account).FirstOrDefault();
                                           if (checkForAccount == null)
                                           {
                                               x.account = "";
                                           }
                                           else if (checkForAccount != null)
                                           {
                                               x.account = checkForAccount;
                                           }
                                           var checkForSanctionLimit = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.sanctionLimit).FirstOrDefault();
                                           if (checkForSanctionLimit == null)
                                           {
                                               x.sanctionLimit = 0;
                                           }
                                           else if (checkForSanctionLimit != null)
                                           {
                                               x.sanctionLimit = checkForSanctionLimit;
                                           }


                                           var checkForSchmType = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.schemeType).FirstOrDefault();
                                           if (checkForSchmType == null)
                                           {
                                               x.schemeType = "";
                                           }
                                           else if (checkForSchmType != null)
                                           {
                                               x.schemeType = checkForSchmType;
                                           }

                                           var checkForDueDate = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.pastDueDate).FirstOrDefault();
                                           if (checkForDueDate == null)
                                           {
                                               x.pastDueDate = 0;
                                           }
                                           else if (checkForDueDate != null)
                                           {
                                               x.pastDueDate = checkForDueDate;
                                           }
                                           var checkForSchmCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.schemeCode).FirstOrDefault();
                                           if (checkForSchmCode == null)
                                           {
                                               x.schemeCode = "";
                                           }
                                           else if (checkForSchmCode != null)
                                           {
                                               x.schemeCode = checkForSchmCode;
                                           }


                                           var checkForSubHeadCode = subHeadCode.Where(f => f.schemeCodes == x.schemeCode).Select(f => f.glSubHeadCode).FirstOrDefault();
                                           if (checkForSubHeadCode == null)
                                           {
                                               x.glSubHeadCode = "";
                                           }
                                           else if (checkForSubHeadCode != null)
                                           {
                                               x.glSubHeadCode = checkForSubHeadCode;
                                           }
                                           var checkForBuCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.buCode).FirstOrDefault();
                                           if (checkForBuCode == null)
                                           {
                                               x.buCode = "";
                                           }
                                           else if (checkForBuCode != null)
                                           {
                                               x.buCode = checkForBuCode;
                                           }
                                           var checkForGroupCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.groupCode).FirstOrDefault();
                                           if (checkForGroupCode == null)
                                           {
                                               x.groupCode = "";
                                           }
                                           else if (checkForGroupCode != null)
                                           {
                                               x.groupCode = checkForGroupCode;
                                           }
                                           var checkForTeamCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.teamCode).FirstOrDefault();
                                           if (checkForTeamCode == null)
                                           {
                                               x.teamCode = "";
                                           }
                                           else if (checkForTeamCode != null)
                                           {
                                               x.teamCode = checkForTeamCode;
                                           }

                                           var checkForDeskCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.deskCode).FirstOrDefault();
                                           if (checkForDeskCode == null)
                                           {
                                               x.deskCode = "";
                                           }
                                           else if (checkForDeskCode != null)
                                           {
                                               x.deskCode = checkForDeskCode;
                                           }

                                           var checkForRmCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.rmCode).FirstOrDefault();
                                           if (checkForRmCode == null)
                                           {
                                               x.rmCode = "";
                                           }
                                           else if (checkForRmCode != null)
                                           {
                                               x.rmCode = checkForRmCode;
                                           }

                                           var checkForTransactionDateBalance = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.transactionDateBalance).FirstOrDefault();
                                           if (checkForTransactionDateBalance == null)
                                           {
                                               x.transactionDateBalance = 0;
                                           }
                                           else if (checkForTransactionDateBalance != null)
                                           {
                                               x.transactionDateBalance = checkForTransactionDateBalance;
                                           }


                                           var checkForUserClassification = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.userClassification).FirstOrDefault();
                                           if (checkForUserClassification == null)
                                           {
                                               x.userClassification = "";
                                           }
                                           else if (checkForUserClassification != null)
                                           {
                                               x.userClassification = checkForUserClassification;
                                           }

                                           var checkForSubClassification = loanMartWatch.Where(f => f.branchCode == x.branchCode).Select(f => f.subClassification).FirstOrDefault();
                                           if (checkForSubClassification == null)
                                           {
                                               x.subClassification = "";
                                           }
                                           else if (checkForSubClassification != null)
                                           {
                                               x.subClassification = checkForSubClassification;
                                           }

                                           var checkForClassificationDate = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.classificationDate).FirstOrDefault();
                                           if (checkForClassificationDate == null)
                                           {
                                               x.classificationDate = DateTime.Now;
                                           }
                                           else if (checkForClassificationDate != null)
                                           {
                                               x.classificationDate = checkForClassificationDate;
                                           }

                                           var checkForExpiryDate = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.expiryDate).FirstOrDefault();
                                           if (checkForExpiryDate == null)
                                           {
                                               x.expiryDate = DateTime.Now;
                                           }
                                           else if (checkForExpiryDate != null)
                                           {
                                               x.expiryDate = checkForExpiryDate;
                                           }


                                           return x;
                                       }).ToList();

                    return expiredList;
                }
            }

        }


        public List<ExcessViewModel> ExcessReport(DateTime startDate, DateTime endDate, int companyid)
        {
            List<SubHead> subList = new List<SubHead>();
            List<LoanMart> loanMart = new List<LoanMart>();
            List<SubHeadCode> subHeadCode = new List<SubHeadCode>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION }).ToList();
                loanMart = (from l in stagecontext.STG_LOAN_MART select new LoanMart { branchCode = l.BRANCH, groupCode = l.GROUP_CODE, teamCode = l.TEAM_CODE, buCode = l.BU_CODE, buDescription = l.BU_DESCRIPTION, deskCode = l.DESK_CODE, rmCode = l.RM_CODE, schemeCode = l.SCHEME_CODE, deskDescription = l.DESK_DESCRIPTION, accountName = l.ACCOUNT_NAME, groupDescription = l.GROUP_DESCRIPTION, pastDueDate = l.DAYS_PAST_DUE, sanctionLimit = l.SANCTIONED_LIMIT, schemeType = l.SCHM_TYPE, account = l.ACCOUNT, customerId = l.CUST_ID, classificationDate = l.CLASSIFICATION_DATE, transactionDateBalance = l.TRAN_DATE_BAL, subClassification = l.SUB_USER_CLASSIFICATION, userClassification = l.USER_CLASSIFICATION, expiryDate = l.EXPIRY_DATE }).ToList();
                subHeadCode = (from h in stagecontext.STG_GL_SUBHEAD_TBL select new SubHeadCode { glSubHeadCode = h.GL_SUB_HEAD_CODE, schemeCodes = h.SCHM_CODE }).ToList();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var excessList = (from l in context.TBL_LOAN
                                      join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                      join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                      join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                      join cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                      join rm in context.TBL_STAFF on l.CREATEDBY equals rm.STAFFID
                                      where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                   DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                && l.COMPANYID == companyid
                                      orderby l.DATETIMECREATED descending
                                      select new ExcessViewModel
                                      {
                                          branchCode = b.BRANCHCODE,
                                          branchName = b.BRANCHNAME,
                                          customerId = l.CUSTOMERID.ToString(),
                                          currencyType = cur.CURRENCYNAME,
                                          customerName = cus.FIRSTNAME + " " + " " + cus.MIDDLENAME + " " + " " + cus.LASTNAME,
                                          rmName = rm.FIRSTNAME + " " + " " + rm.MIDDLENAME + " " + " " + rm.LASTNAME,
                                          excess = "",
                                          endDate = l.EFFECTIVEDATE,
                                          //expiryDate = l.MATURITYDATE,


                                      }).ToList().Select(x =>
                                      {
                                          var checkForBusinessDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.buDescription).FirstOrDefault();
                                          if (checkForBusinessDescription == null)
                                          {
                                              x.buDescription = "";
                                          }
                                          else if (checkForBusinessDescription != null)
                                          {
                                              x.buDescription = checkForBusinessDescription;
                                          }

                                          var checkForGroupDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.groupDescription).FirstOrDefault();
                                          if (checkForGroupDescription == null)
                                          {
                                              x.groupDescription = "";
                                          }
                                          else if (checkForGroupDescription != null)
                                          {
                                              x.groupDescription = checkForGroupDescription;
                                          }
                                          var checkForTeamDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.teamDescription).FirstOrDefault();
                                          if (checkForTeamDescription == null)
                                          {
                                              x.teamDescription = "";
                                          }
                                          else if (checkForTeamDescription != null)
                                          {
                                              x.teamDescription = checkForTeamDescription;
                                          }
                                          var checkForDeskDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.deskDescription).FirstOrDefault();
                                          if (checkForDeskDescription == null)
                                          {
                                              x.deskDescription = "";
                                          }
                                          else if (checkForDeskDescription != null)
                                          {
                                              x.deskDescription = checkForDeskDescription;
                                          }
                                          var checkForAccountName = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.accountName).FirstOrDefault();
                                          if (checkForAccountName == null)
                                          {
                                              x.accountName = "";
                                          }
                                          else if (checkForAccountName != null)
                                          {
                                              x.accountName = checkForAccountName;
                                          }
                                          var checkForAccount = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.account).FirstOrDefault();
                                          if (checkForAccount == null)
                                          {
                                              x.account = "";
                                          }
                                          else if (checkForAccount != null)
                                          {
                                              x.account = checkForAccount;
                                          }
                                          var checkForSanctionLimit = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.sanctionLimit).FirstOrDefault();
                                          if (checkForSanctionLimit == null)
                                          {
                                              x.sanctionLimit = 0;
                                          }
                                          else if (checkForSanctionLimit != null)
                                          {
                                              x.sanctionLimit = checkForSanctionLimit;
                                          }


                                          var checkForSchmType = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.schemeType).FirstOrDefault();
                                          if (checkForSchmType == null)
                                          {
                                              x.schemeType = "";
                                          }
                                          else if (checkForSchmType != null)
                                          {
                                              x.schemeType = checkForSchmType;
                                          }

                                          var checkForDueDate = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.pastDueDate).FirstOrDefault();
                                          if (checkForDueDate == null)
                                          {
                                              x.pastDueDate = 0;
                                          }
                                          else if (checkForDueDate != null)
                                          {
                                              x.pastDueDate = checkForDueDate;
                                          }
                                          var checkForSchmCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.schemeCode).FirstOrDefault();
                                          if (checkForSchmCode == null)
                                          {
                                              x.schemeCode = "";
                                          }
                                          else if (checkForSchmCode != null)
                                          {
                                              x.schemeCode = checkForSchmCode;
                                          }


                                

                                          var checkForRmCode = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.rmCode).FirstOrDefault();
                                          if (checkForRmCode == null)
                                          {
                                              x.rmCode = "";
                                          }
                                          else if (checkForRmCode != null)
                                          {
                                              x.rmCode = checkForRmCode;
                                          }

                                          var checkForTransactionDateBalance = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.transactionDateBalance).FirstOrDefault();
                                          if (checkForTransactionDateBalance == null)
                                          {
                                              x.transactionDateBalance = 0;
                                          }
                                          else if (checkForTransactionDateBalance != null)
                                          {
                                              x.transactionDateBalance = checkForTransactionDateBalance;
                                          }


                                          var checkForUserClassification = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.userClassification).FirstOrDefault();
                                          if (checkForUserClassification == null)
                                          {
                                              x.userClassification = "";
                                          }
                                          else if (checkForUserClassification != null)
                                          {
                                              x.userClassification = checkForUserClassification;
                                          }

                                          var checkForSubClassification = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.subClassification).FirstOrDefault();
                                          if (checkForSubClassification == null)
                                          {
                                              x.subClassification = "";
                                          }
                                          else if (checkForSubClassification != null)
                                          {
                                              x.subClassification = checkForSubClassification;
                                          }

                                          var checkForClassificationDate = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.classificationDate).FirstOrDefault();
                                          if (checkForClassificationDate == null)
                                          {
                                              x.classificationDate = DateTime.Now;
                                          }
                                          else if (checkForClassificationDate != null)
                                          {
                                              x.classificationDate = checkForClassificationDate;
                                          }

                                          var checkForExpiryDate = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.expiryDate).FirstOrDefault();
                                          if (checkForExpiryDate == null)
                                          {
                                              x.classificationDate = DateTime.Now;
                                          }
                                          else if (checkForExpiryDate != null)
                                          {
                                              x.expiryDate = checkForExpiryDate;
                                          }


                                          return x;
                                      }).ToList();

                    return excessList;
                }
            }
        }
            public List<InsuranceViewModel> InsuranceReport(DateTime startDate, DateTime endDate, int companyid)
            {
                List<SubHead> subList = new List<SubHead>();
                List<LoanMart> loanMart = new List<LoanMart>();
                List<SubHeadCode> subHeadCode = new List<SubHeadCode>();

                using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
                {
                    subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION ,businessDevelopmentManger = sl.DIRECTORATE}).ToList();
                    loanMart = (from l in stagecontext.STG_LOAN_MART select new LoanMart { branchCode = l.BRANCH, groupCode = l.GROUP_CODE, teamCode = l.TEAM_CODE, buCode = l.BU_CODE, buDescription = l.BU_DESCRIPTION, deskCode = l.DESK_CODE, rmCode = l.RM_CODE, schemeCode = l.SCHEME_CODE, deskDescription = l.DESK_DESCRIPTION, accountName = l.ACCOUNT_NAME, groupDescription = l.GROUP_DESCRIPTION, pastDueDate = l.DAYS_PAST_DUE, sanctionLimit = l.SANCTIONED_LIMIT, schemeType = l.SCHM_TYPE, account = l.ACCOUNT, customerId = l.CUST_ID, classificationDate = l.CLASSIFICATION_DATE, transactionDateBalance = l.TRAN_DATE_BAL, subClassification = l.SUB_USER_CLASSIFICATION, userClassification = l.USER_CLASSIFICATION, expiryDate = l.EXPIRY_DATE}).ToList();
                    subHeadCode = (from h in stagecontext.STG_GL_SUBHEAD_TBL select new SubHeadCode { glSubHeadCode = h.GL_SUB_HEAD_CODE, schemeCodes = h.SCHM_CODE }).ToList();
                    using (FinTrakBankingContext context = new FinTrakBankingContext())
                    {

                        var insuranceList = (from ccu in context.TBL_COLLATERAL_CUSTOMER
                                             join cu in context.TBL_CUSTOMER on ccu.CUSTOMERID equals cu.CUSTOMERID
                                             join br in context.TBL_BRANCH on cu.BRANCHID equals br.BRANCHID
                                             join ccp in context.TBL_COLLATERAL_ITEM_POLICY on ccu.COLLATERALCUSTOMERID equals ccp.COLLATERALCUSTOMERID
                                             //join cp in context.TBL_COLLATERAL_POLICY on ccu.COLLATERALCUSTOMERID equals cp.COLLATERALCUSTOMERID
                                             join ip in context.TBL_COLLATERAL_IMMOVE_PROPERTY on ccu.COLLATERALCUSTOMERID equals ip.COLLATERALCUSTOMERID
                                             join pe in context.TBL_COLLATERAL_PERFECTN_STAT on ip.PERFECTIONSTATUSID equals pe.PERFECTIONSTATUSID
                                             join ct in context.TBL_COLLATERAL_TYPE on ccu.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                                             join lc in context.TBL_LOAN_COLLATERAL_MAPPING on ccu.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                                             join l in context.TBL_LOAN on lc.LOANID equals l.TERMLOANID
                                             join st in context.TBL_STAFF on l.CREATEDBY equals st.STAFFID
                                             where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                  DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                               && ccu.COMPANYID == companyid
                                             orderby ccp.STARTDATE descending

                                             select new InsuranceViewModel
                                             {

                                                 branchId = br.BRANCHID,
                                                 branchName = br.BRANCHNAME,
                                                 branchCode = br.BRANCHCODE,
                                                 collateralType = ct.COLLATERALTYPENAME,
                                                 perfectionStatus = pe.PERFECTIONSTATUSNAME,
                                                 insuranceType = ccp.INSURANCETYPE,
                                                 insurancePolicyNumber = ccp.POLICYREFERENCENUMBER,
                                                 insuranceCompanyName = ccp.INSURANCECOMPANYNAME,
                                                 insuredValue = ccp.SUMINSURED,
                                                
                                                 startDate = ccp.STARTDATE,
                                                
                                                 maturityDate = l.MATURITYDATE, 
                                                 
                                                 days = (int)DbFunctions.DiffDays((DateTime?)l.MATURITYDATE , (DateTime?)l.EFFECTIVEDATE),
                                                 workFlowID = "",
                                                 status = l.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                                 staffCode = st.STAFFCODE,
                                                 rmName = st.FIRSTNAME + " "+" "+st.MIDDLENAME + " "+" "+st.LASTNAME,
                                                 customerId = ccu.COLLATERALCUSTOMERID,
                                                 
                                                 
                                             }).ToList().Select(x =>
                                             {
                                                 

                                                 var checkForGroupDescription = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.groupDescription).FirstOrDefault();
                                                 if (checkForGroupDescription == null)
                                                 {
                                                     x.groupDescription = "";
                                                 }
                                                 else if (checkForGroupDescription != null)
                                                 {
                                                     x.groupDescription = checkForGroupDescription;
                                                 }
                                                
                                                 var checkForAccountName = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.accountName).FirstOrDefault();
                                                 if (checkForAccountName == null)
                                                 {
                                                     x.accountName = "";
                                                 }
                                                 else if (checkForAccountName != null)
                                                 {
                                                     x.accountName = checkForAccountName;
                                                 }
                                                 var checkForAccount = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.account).FirstOrDefault();
                                                 if (checkForAccount == null)
                                                 {
                                                     x.account = "";
                                                 }
                                                 else if (checkForAccount != null)
                                                 {
                                                     x.account = checkForAccount;
                                                 }
                                                 var checkForSanctionLimit = loanMart.Where(f => f.branchCode == x.branchCode).Select(f => f.sanctionLimit).FirstOrDefault();
                                                 if (checkForSanctionLimit == null)
                                                 {
                                                     x.sanctionLimit = 0;
                                                 }
                                                 else if (checkForSanctionLimit != null)
                                                 {
                                                     x.sanctionLimit = checkForSanctionLimit;
                                                 }


                                                 var checkForGroupHead = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.subHead).FirstOrDefault();

                                                 if (checkForGroupHead == null)
                                                 {
                                                     x.subHead = "";
                                                 }
                                                 else if (checkForGroupHead != null)
                                                 {
                                                     x.subHead = checkForGroupHead;
                                                 }

                                                 var checkForBusinessDevelopment = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.businessDevelopmentManger).FirstOrDefault();

                                                 if (checkForBusinessDevelopment == null)
                                                 {
                                                     x.businessDevelopmentManger = "";
                                                 }
                                                 else if (checkForBusinessDevelopment != null)
                                                 {
                                                     x.businessDevelopmentManger = checkForBusinessDevelopment;
                                                 }

                                                 var checkForRemarks = context.TBL_COLLATERAL_POLICY.Where(u => u.COLLATERALCUSTOMERID == x.customerId).Select(u => u.REMARK).FirstOrDefault();

                                                 if (checkForRemarks == null)
                                                 {
                                                     x.remarks = "";
                                                 }
                                                 else if (checkForRemarks != null)
                                                 {
                                                     x.remarks = checkForRemarks;
                                                 }
                                                 return x;

                                                 var checkForPremiumPaid = context.TBL_COLLATERAL_POLICY.Where(u => u.COLLATERALCUSTOMERID == x.customerId).Select(u => u.PREMIUMAMOUNT).FirstOrDefault();

                                                 if (checkForPremiumPaid == null)
                                                 {
                                                     x.premiumPaid = 0;
                                                 }
                                                 else if (checkForPremiumPaid != null)
                                                 {
                                                     x.premiumPaid = checkForPremiumPaid;
                                                 }
                                                 return x;

                                             }).ToList();

                   


                    return insuranceList;



                    }
                }
            }

        public IEnumerable<LoanViewModel> GetLoanBookingReport(int companyId, string searchInfo, DateTime startDate, DateTime endDate)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();
           int? staffId = context.TBL_STAFF.Where(o => o.STAFFCODE == searchInfo).Select(o => o.STAFFID).FirstOrDefault();

            var termLoan = GetLoanFacility(companyId, staffId, searchInfo, startDate, endDate);
            var Contingent = GetLoanFacility(companyId, staffId, searchInfo, startDate, endDate);
            var revolving = GetLoanFacility(companyId, staffId, searchInfo, startDate, endDate);

            return termLoan.Union(Contingent).Union(revolving);
        }

        private IEnumerable<LoanViewModel> GetLoanFacility(int companyId,int? staffId,string searchInfo, DateTime startDate, DateTime endDate)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var data = (from ln in context.TBL_LOAN
                        join req in context.TBL_LOAN_BOOKING_REQUEST on ln.LOANAPPLICATIONDETAILID equals req.LOANAPPLICATIONDETAILID
                        where //ln.LOANSTATUSID != (short)LoanStatusEnum.Cancelled && ln.LOANSTATUSID != (short)LoanStatusEnum.Terminated
                         (DbFunctions.TruncateTime(ln.BOOKINGDATE) >= DbFunctions.TruncateTime(startDate) &&
                                                  DbFunctions.TruncateTime(ln.BOOKINGDATE) <= DbFunctions.TruncateTime(endDate))
                                                  && ln.COMPANYID == companyId
                                                   && (ln.CREATEDBY == staffId || staffId == 0 || staffId == null)
                                                   && (ln.LOANREFERENCENUMBER == searchInfo || searchInfo == null || searchInfo == "")


                        select new LoanViewModel()
                        {
                            casaAccountNumber = ln.TBL_CASA.PRODUCTACCOUNTNUMBER,
                            casaAccountDetails = ln.TBL_CASA.PRODUCTACCOUNTNUMBER + " (" + ln.TBL_CASA.PRODUCTACCOUNTNAME + ") ",
                            currencyCode = ln.TBL_CURRENCY.CURRENCYCODE,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            applicationReferenceNumber = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            interestRate = ln.INTERESTRATE,
                            effectiveDate = ln.EFFECTIVEDATE,
                            bookedAmount = ln.PRINCIPALAMOUNT,
                            maturityDate = ln.MATURITYDATE,
                            bookingDate = ln.BOOKINGDATE,
                            principalAmount = ln.PRINCIPALAMOUNT,
                            disbursableAmount = ln.PRINCIPALAMOUNT,
                            approvedAmount = ln.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF1.FIRSTNAME + " " + ln.TBL_STAFF1.MIDDLENAME + " " + ln.TBL_STAFF1.LASTNAME,
                            productName = ln.TBL_PRODUCT.PRODUCTNAME,
                            productTypeName = ln.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                            loanStatusName = ln.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                            creatorName = ln.TBL_STAFF.LASTNAME + " " + ln.TBL_STAFF.FIRSTNAME + " (" + ln.TBL_STAFF.STAFFCODE + ")",
                            dateTimeCreated = ln.BOOKINGDATE,
                            loanSystemTypeName = ln.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                            loanStatus = context.TBL_LOAN_STATUS.Where(o=>o.LOANSTATUSID== ln.LOANSTATUSID).Select(o=>o.ACCOUNTSTATUS).FirstOrDefault(),

                        }).ToList();




            return data;

        }

        private IEnumerable<LoanViewModel> GetRevolvingFacility(int companyId,int? staffId, string searchInfo, DateTime startDate, DateTime endDate)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var data = (from ln in context.TBL_LOAN_REVOLVING
                        join req in context.TBL_LOAN_BOOKING_REQUEST on ln.LOANAPPLICATIONDETAILID equals req.LOANAPPLICATIONDETAILID
                        where //ln.LOANSTATUSID != (short)LoanStatusEnum.Cancelled && ln.LOANSTATUSID != (short)LoanStatusEnum.Terminated
                         (DbFunctions.TruncateTime(ln.BOOKINGDATE) >= DbFunctions.TruncateTime(startDate) &&
                                                  DbFunctions.TruncateTime(ln.BOOKINGDATE) <= DbFunctions.TruncateTime(endDate))
                                                  && ln.COMPANYID == companyId
                                                   && (ln.CREATEDBY == staffId || staffId == 0 || staffId == null)
                                                   && (ln.LOANREFERENCENUMBER == searchInfo || searchInfo == null || searchInfo == "")

                        select new LoanViewModel()
                        {
                            casaAccountNumber = ln.TBL_CASA.PRODUCTACCOUNTNUMBER,
                            casaAccountDetails = ln.TBL_CASA.PRODUCTACCOUNTNUMBER + " (" + ln.TBL_CASA.PRODUCTACCOUNTNAME + ") ",
                            currencyCode = ln.TBL_CURRENCY.CURRENCYCODE,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            applicationReferenceNumber = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            interestRate = ln.INTERESTRATE,
                            effectiveDate = ln.EFFECTIVEDATE,
                            bookedAmount = 0,//ln.PRINCIPALAMOUNT,
                            maturityDate = ln.MATURITYDATE,
                            bookingDate = ln.BOOKINGDATE,
                            principalAmount = 0,//ln.PRINCIPALAMOUNT,
                            disbursableAmount = 0,//ln.PRINCIPALAMOUNT,
                            approvedAmount = ln.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF1.FIRSTNAME + " " + ln.TBL_STAFF1.MIDDLENAME + " " + ln.TBL_STAFF1.LASTNAME,
                            productName = ln.TBL_PRODUCT.PRODUCTNAME,
                            productTypeName = ln.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                            loanStatusName = ln.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                            creatorName = ln.TBL_STAFF.LASTNAME + " " + ln.TBL_STAFF.FIRSTNAME + " (" + ln.TBL_STAFF.STAFFCODE + ")",
                            dateTimeCreated = ln.DATETIMECREATED,
                            loanSystemTypeName = ln.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                            loanStatus = context.TBL_LOAN_STATUS.Where(o => o.LOANSTATUSID == ln.LOANSTATUSID).Select(o => o.ACCOUNTSTATUS).FirstOrDefault(),
                        }).ToList();




            return data;


        }

        private IEnumerable<LoanViewModel> GetContingentFacility(int companyId,int? staffId, string searchInfo, DateTime startDate, DateTime endDate)
        {
            FinTrakBankingContext context = new FinTrakBankingContext();

            var data = (from ln in context.TBL_LOAN_CONTINGENT
                        join req in context.TBL_LOAN_BOOKING_REQUEST on ln.LOANAPPLICATIONDETAILID equals req.LOANAPPLICATIONDETAILID
                        where //ln.LOANSTATUSID != (short)LoanStatusEnum.Cancelled && ln.LOANSTATUSID != (short)LoanStatusEnum.Terminated
                        (DbFunctions.TruncateTime(ln.BOOKINGDATE) >= DbFunctions.TruncateTime(startDate) &&
                                                  DbFunctions.TruncateTime(ln.BOOKINGDATE) <= DbFunctions.TruncateTime(endDate))
                                                  && ln.COMPANYID == companyId
                                                   && (ln.CREATEDBY == staffId || staffId == 0 || staffId == null)
                                                   && (ln.LOANREFERENCENUMBER == searchInfo || searchInfo == null || searchInfo == "")

                        orderby ln.CONTINGENTLOANID descending

                        select new LoanViewModel()
                        {
                            casaAccountNumber = ln.TBL_CASA.PRODUCTACCOUNTNUMBER,
                            casaAccountDetails = ln.TBL_CASA.PRODUCTACCOUNTNUMBER + " (" + ln.TBL_CASA.PRODUCTACCOUNTNAME + ") ",
                            currencyCode = ln.TBL_CURRENCY.CURRENCYCODE,
                            loanReferenceNumber = ln.LOANREFERENCENUMBER,
                            applicationReferenceNumber = ln.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            interestRate = 0,//ln.INTERESTRATE,
                            effectiveDate = ln.EFFECTIVEDATE,
                            bookedAmount = 0,//ln.PRINCIPALAMOUNT,
                            maturityDate = ln.MATURITYDATE,
                            bookingDate = ln.BOOKINGDATE,
                            principalAmount = 0,//ln.PRINCIPALAMOUNT,
                            disbursableAmount = 0,//ln.PRINCIPALAMOUNT,
                            approvedAmount = ln.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                            customerName = ln.TBL_CUSTOMER.LASTNAME + " " + ln.TBL_CUSTOMER.FIRSTNAME + " " + ln.TBL_CUSTOMER.MIDDLENAME,
                            branchName = ln.TBL_BRANCH.BRANCHNAME,
                            relationshipOfficerName = ln.TBL_STAFF.FIRSTNAME + " " + ln.TBL_STAFF.MIDDLENAME + " " + ln.TBL_STAFF.LASTNAME,
                            relationshipManagerName = ln.TBL_STAFF1.FIRSTNAME + " " + ln.TBL_STAFF1.MIDDLENAME + " " + ln.TBL_STAFF1.LASTNAME,
                            productName = ln.TBL_PRODUCT.PRODUCTNAME,
                            productTypeName = ln.TBL_PRODUCT.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                            loanStatusName = ln.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                            creatorName = ln.TBL_STAFF.LASTNAME + " " + ln.TBL_STAFF.FIRSTNAME + " (" + ln.TBL_STAFF.STAFFCODE + ")",
                            dateTimeCreated = ln.DATETIMECREATED,
                            loanSystemTypeName = ln.TBL_LOAN_SYSTEM_TYPE.LOANSYSTEMTYPENAME,
                            loanStatus = context.TBL_LOAN_STATUS.Where(o => o.LOANSTATUSID == ln.LOANSTATUSID).Select(o => o.ACCOUNTSTATUS).FirstOrDefault(),

                        }).ToList();



            return data;
        }

        public List<Form3800BReportViewModel> Form3800BApprovedFacility(int companyId, DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
               var misList = (from sl in stagecontext.STG_STAFFMIS select new {sl.DIRECTORATE, sl.USERNAME, sl.GROUP_HUB, sl.DEPT_NAME}).ToList();
              var  data = new List<Form3800BReportViewModel>();
              var  lmsData = new List<Form3800BReportViewModel>();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {

                     data = (from x in context.TBL_LOAN_APPLICATION_DETAIL
                                join a in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                

                                let staffCode = context.TBL_STAFF.Where(s => s.STAFFID == a.RELATIONSHIPMANAGERID).Select(s => s.STAFFCODE).FirstOrDefault()
                             //   let mis = misList.Where(o => o.USERNAME == staffCode).Select(o=>o.DEPT_NAME).FirstOrDefault()

                                where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                  DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                                                  && a.COMPANYID == companyId
                                                  && x.STATUSID == (int)ApprovalStatusEnum.Approved
                             select new Form3800BReportViewModel
                                {
                                  date =  a.AVAILMENTDATE,
                                  operativeAccount = "", // x cassaaccountid
                                  businessUnit ="",
                                    businessGroup = "", //group
                                  branch = a.TBL_BRANCH.BRANCHNAME,
                                  customer = a.TBL_CUSTOMER.FIRSTNAME + "" + a.TBL_CUSTOMER.LASTNAME + "" + a.TBL_CUSTOMER.MIDDLENAME,
                                  cap = "CAP",
                                  status = "NEW APPROVAL",
                                  purpose = x.LOANPURPOSE,
                                  newApproval =x.APPROVEDAMOUNT,
                                  currency = context.TBL_CURRENCY.Where(o=>o.CURRENCYID==x.CURRENCYID).Select(o=>o.CURRENCYNAME).FirstOrDefault(),
                                staffCode = staffCode,
                                }).ToList().Select(x =>
                                {
                                    x.businessUnit = misList.Where(o => o.USERNAME == x.staffCode).Select(o => o.DIRECTORATE).FirstOrDefault();
                                    x.businessGroup = misList.Where(o => o.USERNAME == x.staffCode).Select(o => o.GROUP_HUB).FirstOrDefault();
                                    return x;
                                }).ToList(); ;

                    lmsData = (from x in context.TBL_LMSR_APPLICATION_DETAIL
                                join a in context.TBL_LMSR_APPLICATION on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                join l in context.TBL_LOAN on x.LOANID equals l.TERMLOANID

                                let loanDetail = context.TBL_LOAN_APPLICATION_DETAIL.Join(
                                    context.TBL_LOAN,a=>a.LOANAPPLICATIONDETAILID,b=>b.LOANAPPLICATIONDETAILID,(a,b)=>new { a,b}).Join(
                                    context.TBL_LMSR_APPLICATION_DETAIL,x=>x.b.TERMLOANID,y=>y.LOANID,(x,y) => new { x.b.RELATIONSHIPMANAGERID,x.a.LOANPURPOSE,x.a.CURRENCYID}).FirstOrDefault()

                               let staffCode = context.TBL_STAFF.Where(s => s.STAFFID == loanDetail.RELATIONSHIPMANAGERID).Select(s => s.STAFFCODE).FirstOrDefault()

                               where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                              DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                                              && a.COMPANYID == companyId
                                              && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved

                               select new Form3800BReportViewModel
                            {
                                date = a.AVAILMENTDATE,
                                operativeAccount = "", // x cassaaccountid
                                businessUnit ="",
                                businessGroup = "", //group
                                branch = a.TBL_BRANCH.BRANCHNAME,
                                customer = a.TBL_CUSTOMER.FIRSTNAME + "" + a.TBL_CUSTOMER.LASTNAME + "" + a.TBL_CUSTOMER.MIDDLENAME,
                                cap = "CAP",
                                status = context.TBL_OPERATIONS.Where(o=>o.OPERATIONID==x.OPERATIONID).Select(o=>o.OPERATIONNAME).FirstOrDefault(), //operationId
                                purpose = loanDetail.LOANPURPOSE,
                                newApproval = x.APPROVEDAMOUNT,
                                currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == loanDetail.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                                staffCode = staffCode

                            }).ToList().Select(x =>
                            {
                                x.businessUnit = misList.Where(o => o.USERNAME == x.staffCode).Select(o => o.DIRECTORATE).FirstOrDefault();
                                x.businessGroup = misList.Where(o => o.USERNAME == x.staffCode).Select(o => o.GROUP_HUB).FirstOrDefault();
                                x.status.ToUpper();
                                return x;
                            }).ToList(); 

                }
                return data.Union(lmsData).ToList();
            }


        }
    }


}
