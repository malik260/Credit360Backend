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
                        where a.COMPANYID == companyId && a.TERMLOANID == tearmLoanId //&& c.CUSTOMERSENSITIVITYLEVELID <= staffSensitivityLevelId
                        select new LoanInformation()
                        {
                            accountNumber = context.TBL_CASA.Where(c => c.CASAACCOUNTID == a.CASAACCOUNTID).Select(c=>c.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
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
                                                              && a.TERMLOANID == loanId 
                                                              && b.TBL_CHART_OF_ACCOUNT.GLCLASSID == (int)ChartOfAccountClassEnum.LoanSchedule
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

        public IEnumerable<DisburstLoanViewModel> GetDisburstLoans(DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short? branchId, int? productClassId, int staffId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
              //  var approvedCustomerSentivityLevelId = context.TBL_STAFF.Find(staffId).CUSTOMERSENSITIVITYLEVELID;
                var data = from a in context.TBL_LOAN
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           where (a.ISDISBURSED
                             && a.DISBURSEDATE >= startDate && a.DISBURSEDATE <= endDate
                         && a.COMPANYID == companyId) && (a.LOANREFERENCENUMBER == loanRefNo || a.TBL_CUSTOMER.FIRSTNAME.StartsWith(loanRefNo) || a.TBL_CUSTOMER.LASTNAME.StartsWith(loanRefNo) || a.TBL_CUSTOMER.MIDDLENAME.StartsWith(loanRefNo) || loanRefNo == null || loanRefNo == "")
                         && (a.BRANCHID == branchId || branchId == null || branchId == 0)
                        && (a.TBL_PRODUCT.PRODUCTCLASSID == productClassId || productClassId == null || productClassId == 0)
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
                               branchId = a.BRANCHID

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

        public IList<LoanDocumentWaivedViewModel> LoanDocumentWaived(DateTime startDate, DateTime endDate, int companyId, short? branchId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_CHECKLIST_DETAIL
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.TARGETID equals b.LOANAPPLICATIONDETAILID

                           where a.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Waived
                           // && b.TBL_CUSTOMER.COMPANYID == companyId
                            && DbFunctions.TruncateTime(a.TBL_CHECKLIST_DEFINITION.TBL_CHECKLIST_ITEM.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate)
                            && DbFunctions.TruncateTime(a.TBL_CHECKLIST_DEFINITION.TBL_CHECKLIST_ITEM.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                            && b.TBL_LOAN_APPLICATION.APPLICATIONSTATUSID > (short)LoanApplicationStatusEnum.ApplicationCompleted
                            && b.TBL_LOAN_APPLICATION.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved

                            && (b.TBL_CUSTOMER.BRANCHID == branchId || branchId == null)


                           select new LoanDocumentWaivedViewModel()
                           {
                               firstName = b.TBL_CUSTOMER.FIRSTNAME,
                               lastName = b.TBL_CUSTOMER.LASTNAME,
                               middleName = b.TBL_CUSTOMER.MIDDLENAME,
                               applicationRefrenceNumber = b.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                               waivedDocument = a.TBL_CHECKLIST_DEFINITION.TBL_CHECKLIST_ITEM.CHECKLISTITEMNAME,
                               facilityAmount = b.APPROVEDAMOUNT,
                               facilityExpirationDate = b.TBL_LOAN.Select(c => c.MATURITYDATE).FirstOrDefault(),
                               facilityGrantedDate = b.TBL_LOAN.Select(m => m.EFFECTIVEDATE).FirstOrDefault(),
                               companyName = b.TBL_CUSTOMER.TBL_COMPANY.NAME, //b.TBL_LOAN.Select(p => p.TBL_COMPANY.NAME).FirstOrDefault(),
                               waveredDate = a.TBL_CHECKLIST_DEFINITION.TBL_CHECKLIST_ITEM.DATETIMECREATED,
                               branchName = b.TBL_CUSTOMER.TBL_BRANCH.BRANCHNAME,
                               facilityType = b.TBL_PRODUCT.PRODUCTNAME,
                               loanApplicationId = b.LOANAPPLICATIONDETAILID,
                               proposedAmount = b.PROPOSEDAMOUNT

                           };
                return data.ToList();
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
                            && (c.BRANCHID == branchId || branchId == null)


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
                               defferalExpiryDate = a.DEFERREDDATE

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
                             && a.DEFERREDDATE >= startDate
                            && (c.BRANCHID == branchCode  || branchCode == 0 || branchCode == null)

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

                           where (a.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Waived )
                            && b.TBL_CUSTOMER.COMPANYID == companyId
                            && DbFunctions.TruncateTime(b.DATETIMECREATED) <= DbFunctions.TruncateTime(startDate)
                            && (b.TBL_CUSTOMER.BRANCHID == branchCode || branchCode == 0 || branchCode == null)


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
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANID equals b.LOANAPPLICATIONID
                           join c in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                           where c.COLLATERALCODE == collateralCode

                           select new CollateralEstimatedViewModel()
                           {
                               firstName = b.TBL_CUSTOMER.FIRSTNAME,
                               lastName = b.TBL_CUSTOMER.LASTNAME,
                               middleName = b.TBL_CUSTOMER.MIDDLENAME,
                               facilityAmount = b.APPROVEDAMOUNT,
                               companyName = b.TBL_CUSTOMER.TBL_COMPANY.NAME,
                               customerId = b.CUSTOMERID,
                               facilityName = b.TBL_PRODUCT.PRODUCTNAME,
                               collateralType = c.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                               collateralDetail = c.TBL_COLLATERAL_TYPE.DETAILS,
                               collateralCode = c.COLLATERALCODE,
                               collateralValue = c.COLLATERALVALUE,
                               hairCut = c.HAIRCUT,


                           };

                return data.ToList();
            }
        }

        public IList<FCYScheuledLoanViewModel> FCYScheuledLoan(int companyId, int loanId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var currdata = from a in context.TBL_LOAN
                               where a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
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

        public IList<CasaLienViewModel> AccountsWithLein(DateTime startDate, DateTime endDate,  string searchParamemter, int companyId)
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

                            select new CasaLienViewModel
                            {
                               sourceReferenceNumber = a.SOURCEREFERENCENUMBER,
                               productAccountNumber =a.PRODUCTACCOUNTNUMBER,
                               lienReferenceNumber = a.LIENREFERENCENUMBER,
                               branchName = context.TBL_BRANCH.Where(x=>x.BRANCHID==a.BRANCHID).Select(x=>x.BRANCHNAME).FirstOrDefault(),
                               lienAmount =a.LIENAMOUNT,
                               description = a.DESCRIPTION,
                               lienTypeName = context.TBL_CASA_LIEN_TYPE.Where(x=>x.LIENTYPEID==a.LIENTYPEID).Select(x=>x.LIENTYPENAME).FirstOrDefault(),
                               dateTimeCreated = a.DATETIMECREATED,
                               //customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,

                            }).ToList();
                return data;

                
            }

        }

        public IList<LoanViewModel> GetStakeHolderOnExperationOfFTP(short? branchId, string customerName, DateTime maturityDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = (
                            from s in context.TBL_CASA
                            join rv in context.TBL_LOAN_REVOLVING on s.CASAACCOUNTID equals rv.CASAACCOUNTID
                            join br in context.TBL_BRANCH on s.BRANCHID equals br.BRANCHID
                            join cs in context.TBL_CUSTOMER on s.CUSTOMERID equals cs.CUSTOMERID
                            where (s.AVAILABLEBALANCE < 0 && rv.MATURITYDATE > maturityDate)
                            && (br.BRANCHID == branchId || branchId == null)
                            && (cs.FIRSTNAME.StartsWith(customerName.Trim()) || cs.MIDDLENAME.StartsWith(customerName.Trim()) || cs.LASTNAME.StartsWith(customerName.Trim()) || customerName == null || rv.LOANREFERENCENUMBER.StartsWith(customerName.Trim()))

                            select new LoanViewModel
                            {
                                applicationReferenceNumber = rv.LOANREFERENCENUMBER,
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
                                // teno = (a.MATURITYDATE - a.EFFECTIVEDATE).Days,
                                effectiveDate = rv.EFFECTIVEDATE,
                                overDraft = rv.OVERDRAFTLIMIT

                            }).ToList();

                return data;
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
                              accountPayTo = context.TBL_CASA.Where(o=>o.ACCOUNTSTATUSID==c.CASAACCOUNTID).Select(o=>o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                              //accountReceiveFrom = context.TBL_CASA.Where(o => o.ACCOUNTSTATUSID == c.CASAACCOUNTID2).Select(o => o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                              naration = "",
                              remark ="",
                              current="",
                              businessGroup="",
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
                    output = output + ", " + item;

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
                                  select m.LOANID );

                var reportData = (
                                  from  l in context.TBL_LOAN 
                                  join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                  where  (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                  && l.COMPANYID == 1 && loansWithCollateral.Contains(l.TERMLOANID) && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  select new StalledPerfectionViewModel
                                  {
                                      loanId = l.TERMLOANID,
                                      customerName = c.LASTNAME + " " + c.LASTNAME,
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
            List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                 subList = (from sl in stagecontext.STG_STAFFMIS
                               select new SubHead { subHead = sl.GROUP_HUB, staffCode = sl.STAFFCODE }).ToList();
            }



            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {


                var staffRecords = (from s in context.TBL_STAFF join
                                          su in subList on s.STAFFCODE equals su.staffCode
                                    select new { s.STAFFID, s.STAFFCODE, su.subHead });

                var loansWithCollateral = (from f in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                                           join m in context.TBL_LOAN_COLLATERAL_MAPPING on f.COLLATERALCUSTOMERID equals m.COLLATERALCUSTOMERID
                                           where f.PERFECTIONSTATUSID == (int)(CollateralPerfectionStatusEnum.NotPerfected)
                                           select m.LOANID);

                var reportData = (
                                  from l in context.TBL_LOAN
                                  join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                  join sub in staffRecords on l.RELATIONSHIPOFFICERID equals sub.STAFFID
                                  where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                  && l.COMPANYID == companyid && loansWithCollateral.Contains(l.TERMLOANID) && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  select new CollateralPerfectionyettoCommenceViewModel
                                  {
                                      loanId = l.TERMLOANID,
                                      customername = c.LASTNAME + " " + c.LASTNAME,
                                      outstandingBalance = l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL,
                                       outstandingInterest = l.OUTSTANDINGINTEREST + l.PASTDUEINTEREST,
                                      startDate = startDate,
                                      endDate = endDate,
                                      facilityGrantDate = l.EFFECTIVEDATE,
                                      // loanSystemTypeId = l.LOANSYSTEMTYPEID,
                                      subHead = sub.subHead


                                  }).ToList().Select(x =>
                                  {
                                      x.collateralType = LoanCollateralType(x.loanId, LoanSystemTypeEnum.TermDisbursedFacility);
                                      return x;
                                  }).ToList();

                return reportData;
            }
        }

        public List<CommercialLoanReport> AllCommercialLoanReport(DateTime startDate, DateTime endDate, int companyid)
        {

            List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS
                           select new SubHead { subHead = sl.GROUP_HUB, staffCode = sl.STAFFCODE }).ToList();
            }



            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var staffRecords = (from s in context.TBL_STAFF
                                    join
                             su in subList on s.STAFFCODE equals su.staffCode
                                    select new { s.STAFFID, s.STAFFCODE, su.subHead });
                var reportData = (from l in context.TBL_LOAN join
                                       c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID join
                                       cu in context.TBL_CURRENCY on l.CURRENCYID equals cu.CURRENCYID join
                                       st in context.TBL_LOAN_STATUS on l.LOANSTATUSID equals st.LOANSTATUSID join
                                       cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID join
                                        cas2 in context.TBL_CASA on l.CASAACCOUNTID2 equals cas2.CASAACCOUNTID join
                                       sub in staffRecords on l.RELATIONSHIPOFFICERID equals sub.STAFFID join 
                                       prod in context.TBL_PRODUCT on l.PRODUCTID equals prod.PRODUCTID join
                                       pc in context.TBL_PRODUCT_CLASS on prod.PRODUCTCLASSID equals pc.PRODUCTCLASSID
                                  where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                  DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                  && l.COMPANYID == companyid && l.LOANSTATUSID == (short)LoanStatusEnum.Active && pc.PRODUCTCLASSID == (short)ProductClassEnum.Commercial
                                  select new CommercialLoanReport
                                  {
                                      accountPayTo = cas.PRODUCTACCOUNTNAME,
                                      accountReceiveFrom = cas2.PRODUCTACCOUNTNAME,
                                      businessGroup = sub.subHead,
                                      capturesDate = l.DATETIMECREATED,
                                      currency = cu.CURRENCYNAME,
                                      customerName = c.LASTNAME + " " + c.FIRSTNAME,
                                      dealDate = (DateTime)l.DATEAPPROVED,
                                      endDate = l.MATURITYDATE,
                                      startDate = l.EFFECTIVEDATE,
                                      interestRate = (decimal)l.INTERESTRATE,
                                      interestRateChange = 0,
                                      interestToDate = 0,
                                      interestType = "",
                                      loanReferenceNo = l.LOANREFERENCENUMBER,
                                      narration = "",
                                      principalAmount = l.PRINCIPALAMOUNT,
                                      status = st.ACCOUNTSTATUS,
                                      tenor = Convert.ToInt32 ((l.MATURITYDATE - l.EFFECTIVEDATE).Days),
                                      tenorToDate = Convert.ToInt32((l.MATURITYDATE - DateTime.Now).Days)

                                  }).ToList();

                return reportData;
            }


        }

        public List<UnearnedLoanInterestReport> UnearnedLoanInterest(DateTime startDate, DateTime endDate, int companyid)
        {

            List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS
                           select new SubHead { subHead = sl.GROUP_HUB, staffCode = sl.STAFFCODE }).ToList();
            }

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var accruedInterest = (from accr in context.TBL_DAILY_ACCRUAL join
                                       loan in context.TBL_LOAN on accr.REFERENCENUMBER equals loan.LOANREFERENCENUMBER
                                       select new { refnumber = loan.LOANREFERENCENUMBER, amount = accr.DAILYACCURALAMOUNT })
                                     .GroupBy(x => x.refnumber).Select(f => new
                                     {
                                         loanReference = f.FirstOrDefault().refnumber,
                                         accruedInterest = f.Sum(x => x.amount)
                                     });

                var staffRecords = (from s in context.TBL_STAFF join
                             su in subList on s.STAFFCODE equals su.staffCode
                                    select new { s.STAFFID, s.STAFFCODE, su.subHead });

                var reportData = (from l in context.TBL_LOAN join
                                c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID join
                                cu in context.TBL_CURRENCY on l.CURRENCYID equals cu.CURRENCYID join
                                st in context.TBL_LOAN_STATUS on l.LOANSTATUSID equals st.LOANSTATUSID join
                                cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID join
                                cas2 in context.TBL_CASA on l.CASAACCOUNTID2 equals cas2.CASAACCOUNTID join
                                sub in staffRecords on l.RELATIONSHIPOFFICERID equals sub.STAFFID join
                                 acc in accruedInterest on l.LOANREFERENCENUMBER equals acc.loanReference
                                  where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                  DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                  && l.COMPANYID == companyid && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  select new UnearnedLoanInterestReport
                                  {
                                      accountPayTo = cas.PRODUCTACCOUNTNAME,
                                      accountReceiveFrom = cas.PRODUCTACCOUNTNAME,
                                      businessGroup = sub.subHead,
                                      customerName = c.LASTNAME + " " + c.FIRSTNAME,
                                      endDate = l.MATURITYDATE,
                                      startDate = l.EFFECTIVEDATE,
                                      interestRate = (decimal)l.INTERESTRATE,
                                      interestRateChange = 0,
                                      interestToDate = 0,
                                      interestType = "",
                                      principalAmount = l.PRINCIPALAMOUNT,
                                      tenor = Convert.ToInt32((l.MATURITYDATE - l.EFFECTIVEDATE).Days),
                                      tenorToDate = Convert.ToInt32((DateTime.Now - l.MATURITYDATE).Days),
                                      accruedInterestToDate =acc.accruedInterest,
                                      tenorToMaturity = Convert.ToInt32((l.MATURITYDATE - DateTime.Now).Days),
                                      unearnedInterestAsAtDate = 0,

                                  }).ToList();

                return reportData;
            }

        }

        public List<ReceivableInterestReport> ReceivableLoanInterest(DateTime startDate, DateTime endDate, int companyid)
        {
            List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS
                           select new SubHead { subHead = sl.GROUP_HUB, staffCode = sl.STAFFCODE }).ToList();
            }

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var accruedInterest = (from accr in context.TBL_DAILY_ACCRUAL join
                                        loan in context.TBL_LOAN on accr.REFERENCENUMBER equals loan.LOANREFERENCENUMBER
                                       select new { refnumber = loan.LOANREFERENCENUMBER, amount = accr.DAILYACCURALAMOUNT })
                                       .GroupBy(x => x.refnumber).Select(f => new
                                       {
                                           loanReference = f.FirstOrDefault().refnumber,
                                           accruedInterest = f.Sum(x => x.amount)
                                       });

                var staffRecords = (from s in context.TBL_STAFF join
                                su in subList on s.STAFFCODE equals su.staffCode
                                    select new { s.STAFFID, s.STAFFCODE, su.subHead });

                var reportData = (from l in context.TBL_LOAN join
                                 c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID join
                                 cu in context.TBL_CURRENCY on l.CURRENCYID equals cu.CURRENCYID join
                                 st in context.TBL_LOAN_STATUS on l.LOANSTATUSID equals st.LOANSTATUSID join
                                 cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID join
                                 cas2 in context.TBL_CASA on l.CASAACCOUNTID2 equals cas2.CASAACCOUNTID join
                                 sub in staffRecords on l.RELATIONSHIPOFFICERID equals sub.STAFFID join
                                  acc in accruedInterest on l.LOANREFERENCENUMBER equals acc.loanReference
                                  where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                  DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                  && l.COMPANYID == companyid && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  select new ReceivableInterestReport
                                  {
                                      accountPayTo = cas.PRODUCTACCOUNTNAME,
                                      accountReceiveFrom = cas.PRODUCTACCOUNTNAME,
                                      businessGroup = sub.subHead,
                                      customerName = c.LASTNAME + " " + c.FIRSTNAME,
                                      endDate = l.MATURITYDATE,
                                      startDate = l.EFFECTIVEDATE,
                                      interestRate = (decimal)l.INTERESTRATE,
                                      interestRateChange = 0,
                                      interestToDate = 0,
                                      interestType = "",
                                      principalAmount = l.PRINCIPALAMOUNT,
                                      tenor = Convert.ToInt32((l.MATURITYDATE - l.EFFECTIVEDATE).Days),
                                      tenorToDate = Convert.ToInt32((DateTime.Now - l.MATURITYDATE).Days),
                                      accruedInterestToDate = acc.accruedInterest,
                                      tenorToMaturity = Convert.ToInt32((l.MATURITYDATE - DateTime.Now).Days)
                                  }).ToList();

                return reportData;
            }

        }

        public List<CashBacked> CashBackedReport(DateTime startDate, DateTime endDate, int companyid)
        {
          
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var cashbackedData = (from l in context.TBL_LOAN
                                      join
                                        cm in context.TBL_LOAN_COLLATERAL_MAPPING on l.TERMLOANID equals cm.LOANID
                                      join
                                        ca in context.TBL_CASA on l.CUSTOMERID equals ca.CUSTOMERID
                                                                              join
                                        cd in context.TBL_COLLATERAL_DEPOSIT on cm.COLLATERALCUSTOMERID equals cd.COLLATERALCUSTOMERID
                                                                              join
                                        cust in context.TBL_CUSTOMER on l.CUSTOMERID equals cust.CUSTOMERID
                                                                              join
                                        cus in context.TBL_COLLATERAL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                                                              join
                                        ct in context.TBL_COLLATERAL_TYPE on cus.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                                                                              join
                                        b in context.TBL_BRANCH on ca.BRANCHID equals b.BRANCHID
                                                                              join
                                        curr in context.TBL_CURRENCY on l.CURRENCYID equals curr.CURRENCYID
                                      where (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                      DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                      && l.COMPANYID == companyid && l.LOANSTATUSID == (short)LoanStatusEnum.Active
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
                                          exchangeRate = l.EXCHANGERATE
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
                                          securityValue = x.securityValue
                                      }).ToList();



                return cashbackedData;

            }


        }

        // CashBackedBondAndGuarantee

        public List<CashBackedBondAndGuarantee> CashBackedBondAndGuarantee(DateTime startDate, DateTime endDate, int companyid, int productClassId)
        {

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var reportData = (from p in context.TBL_PRODUCT
                                  join
                                    lc in context.TBL_LOAN_CONTINGENT on p.PRODUCTID equals lc.PRODUCTID
                                                                      join
                                    l in context.TBL_LOAN on lc.CUSTOMERID equals l.CUSTOMERID
                                                                      join
                                    cm in context.TBL_LOAN_COLLATERAL_MAPPING on l.TERMLOANID equals cm.LOANID
                                                                      join
                                    cc in context.TBL_COLLATERAL_CASA on cm.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                                                                      join
                                    cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                                                      join
                                    b in context.TBL_CUSTOMER on cm.COLLATERALCUSTOMERID equals b.CUSTOMERID
                                                                      join
                                    co in context.TBL_COLLATERAL_CUSTOMER on cm.COLLATERALCUSTOMERID equals co.COLLATERALCUSTOMERID
                                                                      join
                                    t in context.TBL_COLLATERAL_TYPE on co.COLLATERALTYPEID equals t.COLLATERALTYPEID
                                                                      join
                                    cur in context.TBL_CURRENCY on lc.CURRENCYID equals cur.CURRENCYID
                                  where
                                  (DbFunctions.TruncateTime(l.EFFECTIVEDATE) >= DbFunctions.TruncateTime(startDate) &&
                                      DbFunctions.TruncateTime(l.EFFECTIVEDATE) <= DbFunctions.TruncateTime(endDate))
                                      && l.COMPANYID == companyid && p.PRODUCTCLASSID == productClassId

                                  select new
                                  {
                                      accountNo = cc.ACCOUNTNUMBER,
                                      beneficiary = b.LASTNAME + " " + b.FIRSTNAME,
                                      bondAmount = lc.CONTINGENTAMOUNT,
                                      bondGuaranteeType = p.PRODUCTNAME,
                                      cashSecurityAccount = "",
                                      currencyType = cur.CURRENCYNAME,
                                      customerId = l.CUSTOMERID,
                                      customerName = cus.LASTNAME + " " + cus.FIRSTNAME,
                                      dateIssue = l.DATETIMECREATED,
                                      nairaEqualvalentOfFCY = lc.CONTINGENTAMOUNT,
                                      purpose = "",
                                      serialNo = p.PRODUCTCODE,
                                      typeofSecurity = t.COLLATERALTYPENAME,
                                      exchangerate = l.EXCHANGERATE

                                  }).ToList().Select(x => new CashBackedBondAndGuarantee
                                  {
                                      accountNo = x.accountNo,
                                      beneficiary = x.beneficiary,
                                      bondAmount = x.bondAmount,
                                      bondGuaranteeType = x.bondGuaranteeType,
                                      cashSecurityAccount = "",
                                      currencyType = x.currencyType,
                                      customerId = x.customerId,
                                      customerName = x.customerName,
                                      dateIssue = x.dateIssue,
                                      nairaEqualvalentOfFCY = x.nairaEqualvalentOfFCY * (decimal)x.exchangerate,
                                      purpose = "",
                                      serialNo = x.serialNo,
                                      typeofSecurity = x.typeofSecurity
                                  }).ToList();

                return reportData.ToList();
            }


        }


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


    }
}
    

