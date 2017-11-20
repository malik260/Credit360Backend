using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace FintrakBanking.ReportObjects
{

    public  class LoanReportObjects 
    {
      

        private IQueryable<LoanInformation> Loans(int companyId , DateTime startDate, DateTime endDate)
        {
            IQueryable<LoanInformation> loan;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                loan = (from a in context.TBL_LOAN
                        join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.TERMLOANID equals b.LOANID
                        where a.COMPANYID == companyId && a.ISDISBURSED == true &&( a.DISBURSEDATE >= startDate && a.DISBURSEDATE <= endDate)
                        select new LoanInformation()
                        { customerId = a.CUSTOMERID,
                        
                            tearmLoanId = a.TERMLOANID ,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            branchCode = a.TBL_BRANCH.BRANCHCODE,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            firstName = a.TBL_CUSTOMER.FIRSTNAME,
                            lastName = a.TBL_CUSTOMER.LASTNAME,
                            middleName = a.TBL_CUSTOMER.MIDDLENAME,
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                            productName = a.TBL_PRODUCT.PRODUCTNAME,
                            loanRefrenceNumber = a.LOANREFERENCENUMBER,
                            frequancy = context.TBL_LOAN_SCHEDULE_PERIODIC .Where(c=> c.LOANID == a.TERMLOANID ).Count()-1,
                            frequencyType = a.TBL_FREQUENCY_TYPE.MODE,
                            companyName = a.TBL_COMPANY.NAME,
                            companylogo = a.TBL_COMPANY.LOGOPATH,
                            effectiveDate = a.EFFECTIVEDATE,
                            interestRate = a.INTERESTRATE,
                            maturityDate = a.MATURITYDATE,
                            principalAmount = a.PRINCIPALAMOUNT,          
                            closePrincipalAmount = b.ENDPRINCIPALAMOUNT,
                            startingBalance = b.STARTPRINCIPALAMOUNT ,
                            paymentDate = b.PAYMENTDATE,
                            periodInterestAmount = b.PERIODINTERESTAMOUNT,
                            principalRepaymentAmount = b.PERIODPAYMENTAMOUNT,
                            outstandingPrincipal =a.OUTSTANDINGPRINCIPAL,
                            outstandingInterest   = a.OUTSTANDINGINTEREST
                        });
                return loan;
            }

            
        }

        public IEnumerable<LoanInformation> GetLoanSchedule( int companyId, int tearmLoanId)
        {
            IEnumerable<LoanInformation> loan;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var company = context.TBL_COMPANY.Where(c => c.COMPANYID == companyId).FirstOrDefault();
                loan = (from a in context.TBL_LOAN
                        join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.TERMLOANID equals b.LOANID
                        where a.COMPANYID == companyId  && a.TERMLOANID == tearmLoanId
                        select new LoanInformation()
                        {
                             accountNumber = context.TBL_CASA .FirstOrDefault(c=> c.CASAACCOUNTID == a.CASAACCOUNTID ).PRODUCTACCOUNTNUMBER ,
                            customerId = a.CUSTOMERID,
                            tearmLoanId = a.TERMLOANID,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            branchCode = a.TBL_BRANCH.BRANCHCODE,
                            customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                            firstName = a.TBL_CUSTOMER.FIRSTNAME,
                            lastName = a.TBL_CUSTOMER.LASTNAME,
                            middleName = a.TBL_CUSTOMER.MIDDLENAME,
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
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
                            outstandingInterest = a.OUTSTANDINGINTEREST
                        });
                return loan.ToList();
            }
             
        }

        public static IList<LoanStatementViewModel> LoanStatement(int companyId, int loanId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                IQueryable<LoanStatementViewModel> Loandata = from a in context.TBL_LOAN
                                                              join b in context.TBL_FINANCE_TRANSACTION on a.LOANREFERENCENUMBER equals b.SOURCEREFERENCENUMBER
                                                              where a.COMPANYID == companyId && a.LOANSTATUSID == 1
                                                              && a.TERMLOANID == loanId && b.CASAACCOUNTID == a.CASAACCOUNTID
                                                              select new LoanStatementViewModel()
                                                              {
                                                                  balance = a.OUTSTANDINGPRINCIPAL,
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
                                                              };

                return Loandata.ToList();
            }

        }

        public  IEnumerable<DisburstLoanViewModel> GetDisburstLoans(DateTime startDate, DateTime endDate, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           where a.ISDISBURSED
                            && DbFunctions.TruncateTime(startDate) >= DbFunctions.TruncateTime(a.DISBURSEDATE)
                           && DbFunctions.TruncateTime(a.DISBURSEDATE) <= DbFunctions.TruncateTime(endDate)
                         && a.COMPANYID == companyId

                           select new DisburstLoanViewModel
                           {
                               bookingRef = a.LOANREFERENCENUMBER ,
                               outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                               approvedInterestRate = a.INTERESTRATE,
                               outstandingInterest = a.OUTSTANDINGINTEREST,
                               amountDisbursed = a.PRINCIPALAMOUNT,
                               accountNumber= a.TBL_CASA.PRODUCTACCOUNTNUMBER,
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
                               status = a.TBL_LOAN_STATUS.ACCOUNTSTATUS
                           };


                return data.ToList();
            }
        }

        public static List<AllLoanViewModel> LoanReport(int ProductClassId, DateTime startDate, DateTime endDdate, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = (from a in context.TBL_LOAN
                            where a.TBL_PRODUCT.PRODUCTCLASSID == ProductClassId && a.ISDISBURSED
                            && DbFunctions.TruncateTime(startDate) >= DbFunctions.TruncateTime(a.DISBURSEDATE)
                            && DbFunctions.TruncateTime(a. DISBURSEDATE) <= DbFunctions.TruncateTime(endDdate)
                            && a.COMPANYID == companyId
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
                                businessGroup = context.TBL_DEPARTMENT.FirstOrDefault(d => d.DEPARTMENTID == a.TBL_STAFF.DEPARTMENTID).DEPARTMENTNAME

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
                                businessGroup = context.TBL_DEPARTMENT.FirstOrDefault(d => d.DEPARTMENTID == a.TBL_STAFF.DEPARTMENTID).DEPARTMENTNAME

                            }).ToList();
                return data;
            }
        }

        public  IList<LoanAnniverseryViewModel> LoanAnniversery(DateTime startDate, DateTime endDate, int companyId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN
                                join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.TERMLOANID equals b.LOANID
                                join c in context.TBL_CUSTOMER_PHONECONTACT on a.CUSTOMERID equals c.CUSTOMERID
                                where a.COMPANYID == companyId && a.LOANSTATUSID == 1
                                && DbFunctions.TruncateTime(b.PAYMENTDATE) >= DbFunctions.TruncateTime(startDate)
                                 && DbFunctions.TruncateTime(b.PAYMENTDATE) <= DbFunctions.TruncateTime(endDate)
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
    }


}
