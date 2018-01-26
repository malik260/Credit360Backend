using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
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
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
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

        public IEnumerable<LoanInformation> GetLoanSchedule(int companyId, int tearmLoanId)
        {
            IEnumerable<LoanInformation> loan;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var company = context.TBL_COMPANY.Where(c => c.COMPANYID == companyId).FirstOrDefault();
                loan = (from a in context.TBL_LOAN
                        join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.TERMLOANID equals b.LOANID
                        where a.COMPANYID == companyId && a.TERMLOANID == tearmLoanId
                        select new LoanInformation()
                        {
                            accountNumber = context.TBL_CASA.FirstOrDefault(c => c.CASAACCOUNTID == a.CASAACCOUNTID).PRODUCTACCOUNTNUMBER,
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

        public IEnumerable<DisburstLoanViewModel> GetDisburstLoans(DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short? branchId, int? productClassId)
        {


            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                           where (a.ISDISBURSED
                            // && a.DISBURSEDATE >= startDate && a.DISBURSEDATE <= endDate
                            && DbFunctions.TruncateTime(a.DISBURSEDATE) >= DbFunctions.TruncateTime(startDate)
                           && DbFunctions.TruncateTime(a.DISBURSEDATE) <= DbFunctions.TruncateTime(endDate)
                         && a.COMPANYID == companyId) && (a.LOANREFERENCENUMBER == loanRefNo || a.TBL_CUSTOMER.FIRSTNAME.StartsWith(loanRefNo) || a.TBL_CUSTOMER.LASTNAME.StartsWith(loanRefNo) || a.TBL_CUSTOMER.MIDDLENAME.StartsWith(loanRefNo) || loanRefNo == null)
                         && (a.BRANCHID == branchId || branchId == null)
                         && (a.TBL_PRODUCT.PRODUCTCLASSID == productClassId || productClassId == null || productClassId == 0)

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

        public IList<LoanAnniverseryViewModel> LoanAnniversery(DateTime startDate, DateTime endDate, int companyId)
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

        public IList<LoanDocumentWaivedViewModel> LoanDocumentWaived(DateTime startDate, DateTime endDate, int companyId, short? branchId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_CHECKLIST_DETAIL
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.TARGETID equals b.LOANAPPLICATIONDETAILID

                           where a.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Deferred
                            && b.TBL_CUSTOMER.COMPANYID == companyId
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
                           join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.CONDITIONID equals b.CONDITIONID
                           join d in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                           join c in context.TBL_CUSTOMER on d.CUSTOMERID equals c.CUSTOMERID
                           join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONID equals e.LOANAPPLICATIONID

                           where
                           // c.COMPANYID == companyId
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
                               applicationRefrenceNumber = b.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
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

        public IList<LoanDocumentWaivedViewModel> LoanDeferralMCCCur(DateTime startDate, int companyId, string branchCode)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN_CONDITION_DEFERRAL
                           join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.CONDITIONID equals b.CONDITIONID
                           join d in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                           join c in context.TBL_CUSTOMER on d.CUSTOMERID equals c.CUSTOMERID
                           join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONID equals e.LOANAPPLICATIONID

                           where
                            d.APPLICATIONSTATUSID > (short)LoanApplicationStatusEnum.ApplicationCompleted
                            && d.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved
                             && a.DEFERREDDATE >= startDate
                            && (c.BRANCHID == context.TBL_BRANCH.Where(x => x.BRANCHCODE == branchCode).Select(x => x.BRANCHID).FirstOrDefault() || branchCode == null)

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
        public IList<LoanDocumentWaivedViewModel> LoanDeferralMCCExp(DateTime startDate, int companyId, string branchCode)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN_CONDITION_DEFERRAL
                           join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.CONDITIONID equals b.CONDITIONID
                           join d in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                           join c in context.TBL_CUSTOMER on d.CUSTOMERID equals c.CUSTOMERID
                           join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONID equals e.LOANAPPLICATIONID

                           where
                            d.APPLICATIONSTATUSID > (short)LoanApplicationStatusEnum.ApplicationCompleted
                            && d.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved
                             && a.DEFERREDDATE <= startDate
                            && (c.BRANCHID == context.TBL_BRANCH.Where(x => x.BRANCHCODE == branchCode).Select(x => x.BRANCHID).FirstOrDefault() || branchCode == null)
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
        public IList<LoanDocumentWaivedViewModel> LoanDocumentWaivedForMCC(DateTime startDate, int companyId, string branchCode)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_LOAN_CONDITION_PRECEDENT
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID

                           where (a.CHECKLISTSTATUSID1 == (short)CheckListStatusEnum.Waived || a.CHECKLISTSTATUSID2 == (short)CheckListStatusEnum.Waived)
                            && b.TBL_CUSTOMER.COMPANYID == companyId
                            && DbFunctions.TruncateTime(b.DATETIMECREATED) <= DbFunctions.TruncateTime(startDate)
                            && (b.TBL_CUSTOMER.BRANCHID == context.TBL_BRANCH.Where(x => x.BRANCHCODE == branchCode).Select(x => x.BRANCHID).FirstOrDefault() || branchCode == null)


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
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
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
                               where a.COMPANYID == companyId && a.LOANSTATUSID == 1
                               && a.TERMLOANID == loanId
                               //&& a.CURRENCYID != 1
                               select new FCYScheuledLoanViewModel()
                               {
                                   loanRefrenceNumber = a.LOANREFERENCENUMBER,
                                   accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
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
                            loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
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

        public IList<LoanViewModel> GetLoanWithLein(short branchId, string customerName)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = (from a in context.TBL_LOAN
                            join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                            join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                            join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                            where (s.HASLIEN == true || (s.POSTNOSTATUSID == (short)CASAPostNoStatusEnum.PostNoDebit || s.POSTNOSTATUSID == (short)CASAPostNoStatusEnum.PostNoDebitandCredit)
                            && (a.LOANREFERENCENUMBER.StartsWith(customerName.Trim()) || cs.FIRSTNAME.StartsWith(customerName.Trim()) || cs.MIDDLENAME.StartsWith(customerName.Trim()) || cs.LASTNAME.StartsWith(customerName.Trim()) || customerName == "undefined")
                            && (a.BRANCHID == branchId || branchId == 0)
                            )


                            select new LoanViewModel
                            {
                                applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                loanReferenceNumber = a.LOANREFERENCENUMBER,
                                bookingDate = a.BOOKINGDATE,
                                disburseDate = a.DISBURSEDATE,
                                maturityDate = a.MATURITYDATE,
                                principalAmount = a.PRINCIPALAMOUNT,
                                interestRate = a.INTERESTRATE,
                                outstandingInterest = a.OUTSTANDINGINTEREST,
                                outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                                loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                                relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                branchId = a.BRANCHID,
                                branchName = br.BRANCHNAME,
                                customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME
                            }).ToList();
                return data;

                //if (branchId == 0 && customerName == "undefined")
                //{
                //    return data;

                //}
                //else if (branchId == 0 && customerName != "undefined")
                //{
                //    return data.Where(x => x.loanReferenceNumber.StartsWith(customerName) || x.customerName.Contains(customerName) && x.branchId == 0).ToList();
                //}
                //else
                //{
                //    return data.Where(x => x.loanReferenceNumber.StartsWith(customerName) || x.customerName.Contains(customerName) && x.branchId == branchId).ToList();
                //}
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
                                loanTypeName = rv.TBL_LOAN_TYPE.LOANTYPENAME,
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
                                loanType = context.TBL_LOAN_TYPE.Where(o => o.LOANTYPEID == ln.LOANTYPEID).Select(o => o.LOANTYPENAME).FirstOrDefault(),
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
                              accountReceiveFrom = context.TBL_CASA.Where(o => o.ACCOUNTSTATUSID == c.CASAACCOUNTID2).Select(o => o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
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

        public IEnumerable<DisburstLoanViewModel> GetLoansInterestReceivable(DateTime startDate, DateTime endDate, int companyId, string searchParamemter, int? productClassId)
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
    }
}
    

