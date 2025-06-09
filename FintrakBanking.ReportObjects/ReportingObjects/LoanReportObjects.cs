using FintrakBanking.APICore.Results;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ReportObjects.Enums;
using FintrakBanking.ReportObjects.ReportHelper;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Media;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.ViewModels.Reports;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace FintrakBanking.ReportObjects
{

    public class LoanReportObjects
    {
        private IGeneralSetupRepository generalSetup;
        FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext();
        FinTrakBankingDocumentsContext documentsContext = new FinTrakBankingDocumentsContext();
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
                        orderby b.PAYMENTDATE // a.BOOKINGDATE descending//&& c.CUSTOMERSENSITIVITYLEVELID <= staffSensitivityLevelId
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

        public List<JobRequestViewModel> GetAllGlobalJobRequest(DateTime startDate, DateTime endDate)
        {
            using (var context = new FinTrakBankingContext())
            {
                //var thisStaff = context.TBL_STAFF.Find(staffId);
                //var staffAdmin = context.TBL_JOB_TYPE_REASSIGNMENT.Where(x => x.STAFFID == staffId);
                //var staffHub = context.TBL_JOB_TYPE_HUB_STAFF.Where(x => x.STAFFID == staffId);
                //var middleOfficeUnit = from x in context.TBL_JOB_TYPE_UNIT
                //                       join t in context.TBL_JOB_TYPE_HUB_STAFF on x.JOBTYPEUNITID equals t.JOBTYPEUNITID
                //                       where x.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && t.STAFFID == staffId
                //                       select x;

                //bool isTeamLead = (from s in context.TBL_JOB_TYPE_HUB_STAFF where s.STAFFID == staffId select s.ISTEAMLEAD).FirstOrDefault();
                List<JobRequestViewModel> allData = new List<JobRequestViewModel>();

                //List<int> adminJobTypeIds = new List<int>();
                //if (staffAdmin.Any())
                //{
                //    foreach (var i in staffAdmin)
                //    {
                //        adminJobTypeIds.Add(i.JOBTYPEID);
                //    }

                //}
                //List<int> unitIds = new List<int>();
                //foreach (var i in staffHub)
                //{
                //    unitIds.Add(i.JOBTYPEUNITID);
                //}
                allData = (from x in context.TBL_JOB_REQUEST
                           join s in context.TBL_JOB_TYPE_SUB on x.JOB_SUB_TYPEID equals s.JOB_SUB_TYPEID
                           join t in context.TBL_JOB_TYPE on x.JOBTYPEID equals t.JOBTYPEID
                           join u in context.TBL_LOAN_APPLICATION_DETAIL on x.TARGETID equals u.LOANAPPLICATIONDETAILID
                           where x.ARRIVALDATE >= startDate && x.ARRIVALDATE <= endDate
                           //|| ((unitIds.Contains((int)x.JOBTYPEUNITID)) && !middleOfficeUnit.Any())
                           //|| adminJobTypeIds.Contains(x.JOBTYPEID)
                           //&& .COMPANYID == companyId
                           orderby x.JOBREQUESTID descending
                           select (
                          new JobRequestViewModel
                          {
                              jobRequestId = x.JOBREQUESTID,
                              requestTitle = x.JOB_TITLE,

                              facilityAmount = u.PROPOSEDAMOUNT,//context.TBL_LOAN_APPLICATION_DETAIL.Where(a=>a.LOANAPPLICATIONDETAILID==x.TARGETID).FirstOrDefault().PROPOSEDAMOUNT,
                              jobRequestCode = x.JOBREQUESTCODE,
                              targetId = x.TARGETID,
                              jobTypeId = t.JOBTYPEID,
                              jobSubTypeId = s.JOB_SUB_TYPEID,
                              jobTypeName = t.JOBTYPENAME,
                              requireCharge = s.REQUIRECHARGE ?? false,
                              chargeFeeId = s.CHARGEFEEID ?? 0,
                              jobSubTypeName = s.JOB_SUB_TYPE_NAME,

                              senderStaffId = x.SENDERSTAFFID,
                              senderRole = x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME,
                              senderRoleCode = x.TBL_STAFF.TBL_STAFF_ROLE != null ? x.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLECODE : string.Empty,


                              receiverStaffId = (int)x.RECEIVERSTAFFID,
                              reassignedTo = x.REASSIGNEDTO,
                              isReassigned = x.ISREASSIGNED,
                              isAcknowledged = x.ISACKNOWLEDGED,
                              operationsId = x.OPERATIONSID,
                              operationName = x.TBL_OPERATIONS.OPERATIONNAME,
                              requestStatusId = x.REQUESTSTATUSID,
                              requestStatusname = x.REQUESTSTATUSID == (short)JobRequestStatusEnum.approved ? "Completed" : x.TBL_JOB_REQUEST_STATUS.STATUSNAME,

                              senderComment = x.SENDERCOMMENT,
                              responseComment = x.RESPONSECOMMENT,
                              arrivalDate = x.ARRIVALDATE,
                              systemArrivalDate = x.SYSTEMARRIVALDATE,
                              reassignedDate = x.REASSIGNEDDATE,
                              systemReassignedDate = x.SYSTEMREASSIGNEDDATE,
                              responseDate = x.RESPONSEDATE,
                              systemResponseDate = x.SYSTEMRESPONSEDATE,
                              acknowledgementDate = x.ACKNOWLEDGEMENTDATE,
                              systemAcknowledgementDate = x.SYSTEMACKNOWLEDGEMENTDATE,
                              //loggedInStaffId = staffId,
                              jobTypeUnitId = x.JOBTYPEUNITID,
                              jobTypeHubId = x.JOBTYPEHUBID,
                              jobSourceId = x.JOBSOURCEID,

                              branchId = x.BRANCHID,
                              /*sourceRegionName = context.TBL_BRANCH_REGION_STAFF.Where(l => l.STAFFID == x.SENDERSTAFFID).Any()
                                                      ? context.TBL_BRANCH_REGION_STAFF.Where(l => l.STAFFID == x.SENDERSTAFFID).FirstOrDefault().TBL_BRANCH_REGION.REGION_NAME : "n/a",
                              sourceBranchCode = context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).Any()
                                                       ? context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).FirstOrDefault().BRANCHNAME : "n/a",*/

                              sourceBranchName = context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).Any()
                                                       ? context.TBL_BRANCH.Where(l => l.BRANCHID == x.BRANCHID).FirstOrDefault().BRANCHNAME : "n/a",
                              /*
                              //isTeamLead = (from s in context.TBL_JOB_TYPE_HUB_STAFF where s.STAFFID == staffId select s.ISTEAMLEAD).FirstOrDefault(),

                              //refNo = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(l => l.LOANAPPLICATIONDETAILID == x.TARGETID) != null
                              // ? context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == x.TARGETID).FirstOrDefault().TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER : "n/a",
                              */
                              fromSender = x.TBL_STAFF.FIRSTNAME == null ? "n/a" : x.TBL_STAFF.FIRSTNAME + " " + x.TBL_STAFF.LASTNAME,
                              fromBranchName = (from y in context.TBL_BRANCH.Where(i => i.BRANCHID == x.BRANCHID) select y.BRANCHNAME).FirstOrDefault(),
                              to = x.TBL_STAFF2.FIRSTNAME == null ? "n/a" : x.TBL_STAFF2.FIRSTNAME + " " + x.TBL_STAFF2.LASTNAME,
                              assignee = x.TBL_STAFF1.FIRSTNAME == null ? "Assign" : x.TBL_STAFF1.FIRSTNAME + " " + x.TBL_STAFF1.LASTNAME,

                          })).ToList();

                allData.OrderBy(y => y.dateTimeCreated);

                //allData =  setCustomerName(allData);

                // allData = setJobRequestOtherDetails(allData);

                //allData = PadModelWithLMSReference(allData);


                return allData;
            }
        }

        public IList<LoanStatementViewModel> LoanStatement(int companyId, int loanId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var principalRepayment = (from a in context.TBL_LOAN
                                          join b in context.TBL_FINANCE_TRANSACTION on a.LOANREFERENCENUMBER equals b.SOURCEREFERENCENUMBER
                                          where a.COMPANYID == companyId && a.TERMLOANID == loanId
                                          //&& b.TBL_CHART_OF_ACCOUNT.GLCLASSID == (int)ChartOfAccountClassEnum.LoanSchedule
                                          select new LoanStatementViewModel()
                                          {
                                              facilityType = a.TBL_PRODUCT.PRODUCTNAME,
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
                                        //&& b.TBL_CHART_OF_ACCOUNT.GLCLASSID == (int)ChartOfAccountClassEnum.LoanInterestReceivable
                                        select new LoanStatementViewModel()
                                        {
                                            //balance = a.OUTSTANDINGPRINCIPAL,
                                            facilityType = a.TBL_PRODUCT.PRODUCTNAME,
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
                                           //&& b.REPAYMENTPOSTEDSTATUS == true
                                           //&& b.TRANSACTIONTYPEID == (byte)LoanTransactionTypeEnum.Interest
                                           select new LoanStatementViewModel()
                                           {
                                               facilityType = a.TBL_PRODUCT.PRODUCTNAME,
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
                                            a.facilityType,
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
                                            facilityType = groupedQ.Key.facilityType,
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

        public List<CorporateLoansDeptViewModel> GetCorporateLoansReport(DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var termLoans = (from ft in context.TBL_LOAN
                                 join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                 select new
                                 {
                                     ft.LOANREFERENCENUMBER,
                                     ft.PRODUCTID,
                                     BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                     p.PRODUCTCODE,
                                     p.PRODUCTNAME,
                                     ft.CUSTOMERID,
                                     PRINCIPALAMOUNT = ft.PRINCIPALAMOUNT,
                                     customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                     appDetailId = ft.LOANAPPLICATIONDETAILID,
                                     appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                     accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                     createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                     ft.LOANSTATUSID,
                                     accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                     ft.DISBURSEDATE,
                                     ft.MATURITYDATE,
                                     ft.DATETIMECREATED,
                                     ft.APPROVERCOMMENT,
                                     ft.LOAN_BOOKING_REQUESTID
                                 });

                var revolvingLoans = (from ft in context.TBL_LOAN_REVOLVING
                                      join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                      select new
                                      {
                                          ft.LOANREFERENCENUMBER,
                                          ft.PRODUCTID,
                                          BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                          p.PRODUCTCODE,
                                          p.PRODUCTNAME,
                                          ft.CUSTOMERID,
                                          PRINCIPALAMOUNT = ft.OVERDRAFTLIMIT,
                                          customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                          appDetailId = ft.LOANAPPLICATIONDETAILID,
                                          appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                          accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                          createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                          ft.LOANSTATUSID,
                                          accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                          ft.DISBURSEDATE,
                                          ft.MATURITYDATE,
                                          ft.DATETIMECREATED,
                                          ft.APPROVERCOMMENT,
                                          ft.LOAN_BOOKING_REQUESTID
                                      });

                var contingentLoans = (from ft in context.TBL_LOAN_CONTINGENT
                                       join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                       select new
                                       {
                                           ft.LOANREFERENCENUMBER,
                                           ft.PRODUCTID,
                                           BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                           p.PRODUCTCODE,
                                           p.PRODUCTNAME,
                                           ft.CUSTOMERID,
                                           PRINCIPALAMOUNT = ft.CONTINGENTAMOUNT,
                                           customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                           appDetailId = ft.LOANAPPLICATIONDETAILID,
                                           appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                           accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                           createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                           ft.LOANSTATUSID,
                                           accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                           ft.DISBURSEDATE,
                                           ft.MATURITYDATE,
                                           ft.DATETIMECREATED,
                                           ft.APPROVERCOMMENT,
                                           ft.LOAN_BOOKING_REQUESTID

                                       });

                var allLoans = termLoans.Union(revolvingLoans).Union(contingentLoans).Distinct();
                var staffdata = context.TBL_STAFF.ToList();
                var data = (from a in context.TBL_LOAN_APPLICATION
                            join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                            select new CorporateLoansDeptViewModel
                            {
                                applicationreferencenumber = a.APPLICATIONREFERENCENUMBER,
                                solId = a.TBL_BRANCH.BRANCHCODE,
                                BranchName = a.TBL_BRANCH.BRANCHNAME,
                                DateTimeInitiated = a.SYSTEMDATETIME,
                                loanapplicationid = a.LOANAPPLICATIONID,
                                bookingrequestid = allLoans.Where(x => x.appDetailId == b.LOANAPPLICATIONDETAILID).FirstOrDefault().LOAN_BOOKING_REQUESTID,
                                status = context.TBL_LOAN_APPLICATION_STATUS.Where(x => x.APPLICATIONSTATUSID == a.APPLICATIONSTATUSID).FirstOrDefault().APPLICATIONSTATUSNAME,
                                PreviousStage = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == a.LOANAPPLICATIONID && x.APPROVALTRAILID == context.TBL_APPROVAL_TRAIL.Where(y => y.TARGETID == a.LOANAPPLICATIONID).Max(z => z.APPROVALTRAILID)).FirstOrDefault().TBL_APPROVAL_LEVEL1.LEVELNAME,
                                DisburseOfficerName = context.TBL_STAFF.Where(x => x.STAFFID == b.TBL_LOAN.FirstOrDefault().DISBURSEDBY).Select(x => new { name = x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME }).FirstOrDefault().name,
                                disburseDateTime = allLoans.Where(x => x.appDetailId == b.LOANAPPLICATIONDETAILID).FirstOrDefault().DISBURSEDATE,
                                verificationOfficer = context.TBL_STAFF.Where(w => w.STAFFID == context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == a.LOANAPPLICATIONID && x.OPERATIONID == 1 && x.APPROVALTRAILID == (context.TBL_APPROVAL_TRAIL.Where(y => y.TARGETID == a.LOANAPPLICATIONID && y.OPERATIONID == 1).Max(z => z.APPROVALTRAILID))).FirstOrDefault().REQUESTSTAFFID).Select(v => new { name = v.FIRSTNAME + " " + v.LASTNAME }).FirstOrDefault().name,
                                verificationDateTime = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == a.LOANAPPLICATIONID && x.OPERATIONID == 1 && x.APPROVALTRAILID == (context.TBL_APPROVAL_TRAIL.Where(y => y.TARGETID == a.LOANAPPLICATIONID && y.OPERATIONID == 1).Max(z => z.APPROVALTRAILID))).FirstOrDefault().SYSTEMRESPONSEDATETIME,
                                loanOfficerName = context.TBL_STAFF.Where(x => x.STAFFID == a.CREATEDBY).Select(x => new { name = x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME }).FirstOrDefault().name,
                                loanOfficerDateTime = a.SYSTEMDATETIME,
                                productType = b.TBL_PRODUCT.PRODUCTNAME,
                                RM = context.TBL_STAFF.Where(x => x.STAFFID == a.RELATIONSHIPMANAGERID).Select(x => new { name = x.FIRSTNAME + " " + x.MIDDLENAME + " " + x.LASTNAME }).FirstOrDefault().name,
                                RMDate = DateTime.Now,
                                availmentDate = a.AVAILMENTDATE,
                                crmsDate = b.CRMSDATE,
                                customerOperativeAccount = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                customerName = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == a.CUSTOMERID).Select(y => new { name = y.FIRSTNAME + " " + y.MIDDLENAME + " " + y.LASTNAME }).FirstOrDefault().name,
                                customerLoanAmount = a.APPROVEDAMOUNT == 0 ? a.APPLICATIONAMOUNT : a.APPROVEDAMOUNT,
                                loanTenure = a.APPLICATIONTENOR,
                                loanAmountDisbursed = allLoans.Where(x => x.appDetailId == b.LOANAPPLICATIONDETAILID).FirstOrDefault().PRINCIPALAMOUNT,
                                refferBackComment = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == a.LOANAPPLICATIONID && x.APPROVALSTATUSID == 5 && x.APPROVALTRAILID == (context.TBL_APPROVAL_TRAIL.Where(y => y.TARGETID == a.LOANAPPLICATIONID && y.APPROVALSTATUSID == 5).Max(z => z.APPROVALTRAILID))).FirstOrDefault().COMMENT,
                                refferBackUser = context.TBL_STAFF.Where(w => w.STAFFID == context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == a.LOANAPPLICATIONID && x.APPROVALSTATUSID == 5 && x.APPROVALTRAILID == (context.TBL_APPROVAL_TRAIL.Where(y => y.TARGETID == a.LOANAPPLICATIONID && y.APPROVALSTATUSID == 5).Max(z => z.APPROVALTRAILID))).FirstOrDefault().REQUESTSTAFFID).Select(v => new { name = v.FIRSTNAME + " " + v.LASTNAME }).FirstOrDefault().name,
                                completedComment = allLoans.Where(x => x.appDetailId == b.LOANAPPLICATIONDETAILID).FirstOrDefault().APPROVERCOMMENT
                            }).ToList();
                //data = data.Select(w => {


                //    var traildata = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == w.loanapplicationid).ToList();
                //    //var dt = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == w.loanapplicationid).Count();
                //    var approvaleleveldata = context.TBL_APPROVAL_LEVEL.ToList();
                //    if (traildata.Count > 0)
                //    {
                //        var recentTrail = traildata.Where(x => x.TARGETID == w.loanapplicationid && x.APPROVALTRAILID == traildata.Where(y => y.TARGETID == x.TARGETID).Max(z => z.APPROVALTRAILID)).Select(u => new { u.FROMAPPROVALLEVELID, u.REQUESTSTAFFID, u.TOAPPROVALLEVELID, u.RESPONSESTAFFID, u.RESPONSEDATE, u.SYSTEMARRIVALDATETIME }).FirstOrDefault();
                //        if (recentTrail.FROMAPPROVALLEVELID != null)
                //        {
                //            w.PreviousStage = approvaleleveldata.Where(k => traildata.Where(x => x.TARGETID == w.loanapplicationid && x.APPROVALTRAILID == traildata.Where(y => y.TARGETID == x.TARGETID).Max(z => z.APPROVALTRAILID)).FirstOrDefault().FROMAPPROVALLEVELID == k.APPROVALLEVELID).FirstOrDefault().LEVELNAME;
                //        }
                //        else if (recentTrail.REQUESTSTAFFID != null)
                //        {
                //            w.PreviousStage = staffdata.Where(v => recentTrail.REQUESTSTAFFID == v.STAFFID).FirstOrDefault().TBL_STAFF_ROLE.STAFFROLENAME;
                //        }
                //        if (recentTrail.TOAPPROVALLEVELID != null)
                //        {
                //            w.status = approvaleleveldata.Where(k => traildata.Where(x => x.TARGETID == w.loanapplicationid && x.APPROVALTRAILID == traildata.Where(y => y.TARGETID == x.TARGETID).Max(z => z.APPROVALTRAILID)).FirstOrDefault().TOAPPROVALLEVELID == k.APPROVALLEVELID).FirstOrDefault().LEVELNAME;
                //        }
                //        else if (recentTrail.RESPONSESTAFFID != null)
                //        {
                //            w.status = staffdata.Where(v => recentTrail.RESPONSESTAFFID == v.STAFFID).FirstOrDefault().TBL_STAFF_ROLE.STAFFROLENAME;
                //        }
                //        if (traildata.Where(q => q.TARGETID == w.bookingrequestid && q.FROMAPPROVALLEVELID == 569 && context.TBL_OPERATIONS.Where(i => i.OPERATIONTYPEID == 1).Select(s => s.OPERATIONID).Contains(q.OPERATIONID) && q.APPROVALTRAILID == traildata.Where(z => z.TARGETID == w.bookingrequestid && context.TBL_OPERATIONS.Where(i => i.OPERATIONTYPEID == 1).Select(s => s.OPERATIONID).Contains(q.OPERATIONID)).Max(aa => aa.APPROVALTRAILID)).Count() > 0)
                //        {
                //            w.verificationOfficer = staffdata.Where(a => a.STAFFID == traildata.Where(q => q.TARGETID == w.bookingrequestid && q.FROMAPPROVALLEVELID == 569 && context.TBL_OPERATIONS.Where(i => i.OPERATIONTYPEID == 1).Select(s => s.OPERATIONID).Contains(q.OPERATIONID) && q.APPROVALTRAILID == traildata.Where(z => z.TARGETID == w.bookingrequestid && context.TBL_OPERATIONS.Where(i => i.OPERATIONTYPEID == 1).Select(s => s.OPERATIONID).Contains(q.OPERATIONID)).Max(aa => aa.APPROVALTRAILID)).FirstOrDefault().REQUESTSTAFFID).Select(d => new { fullname = d.FIRSTNAME + " " + d.MIDDLENAME + " " + d.LASTNAME }).FirstOrDefault().fullname;
                //            w.verificationDateTime = traildata.Where(q => q.TARGETID == w.bookingrequestid && q.FROMAPPROVALLEVELID == 569 && context.TBL_OPERATIONS.Where(i => i.OPERATIONTYPEID == 1).Select(s => s.OPERATIONID).Contains(q.OPERATIONID) && q.APPROVALTRAILID == traildata.Where(z => z.TARGETID == w.bookingrequestid && context.TBL_OPERATIONS.Where(i => i.OPERATIONTYPEID == 1).Select(s => s.OPERATIONID).Contains(q.OPERATIONID)).Max(aa => aa.APPROVALTRAILID)).FirstOrDefault().SYSTEMARRIVALDATETIME;
                //        }

                //    }

                //    return w;
                //}).ToList();
                return data;
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
                           join bt in context.TBL_STAFF on a.DISBURSEDBY equals bt.STAFFID
                           join br in context.TBL_STAFF on a.CREATEDBY equals br.STAFFID
                           where (a.ISDISBURSED && DbFunctions.TruncateTime(a.DISBURSEDATE) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(a.DISBURSEDATE) <= DbFunctions.TruncateTime(endDate))
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
                               disbursedUser = bt.FIRSTNAME + " " + bt.MIDDLENAME + " " + bt.LASTNAME,
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

        public IEnumerable<RelatedPartyLoansViewModel> GetInsiderRelatedLoans(DateTime startDate, DateTime endDate, string loanRefNo)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var termLoans = (from ft in context.TBL_LOAN
                                 join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                 select new
                                 {
                                     ft.LOANREFERENCENUMBER,
                                     ft.PRODUCTID,
                                     BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                     p.PRODUCTCODE,
                                     p.PRODUCTNAME,
                                     ft.CUSTOMERID,
                                     PRINCIPALAMOUNT = ft.PRINCIPALAMOUNT,
                                     customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                     appDetailId = ft.LOANAPPLICATIONDETAILID,
                                     appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                     accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                     createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                     ft.LOANSTATUSID,
                                     accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                     ft.DISBURSEDATE,
                                     ft.MATURITYDATE,
                                     ft.DATETIMECREATED
                                 });

                var revolvingLoans = (from ft in context.TBL_LOAN_REVOLVING
                                      join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                      select new
                                      {
                                          ft.LOANREFERENCENUMBER,
                                          ft.PRODUCTID,
                                          BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                          p.PRODUCTCODE,
                                          p.PRODUCTNAME,
                                          ft.CUSTOMERID,
                                          PRINCIPALAMOUNT = ft.OVERDRAFTLIMIT,
                                          customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                          appDetailId = ft.LOANAPPLICATIONDETAILID,
                                          appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                          accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                          createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                          ft.LOANSTATUSID,
                                          accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                          ft.DISBURSEDATE,
                                          ft.MATURITYDATE,
                                          ft.DATETIMECREATED
                                      });

                var contingentLoans = (from ft in context.TBL_LOAN_CONTINGENT
                                       join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                       select new
                                       {
                                           ft.LOANREFERENCENUMBER,
                                           ft.PRODUCTID,
                                           BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                           p.PRODUCTCODE,
                                           p.PRODUCTNAME,
                                           ft.CUSTOMERID,
                                           PRINCIPALAMOUNT = ft.CONTINGENTAMOUNT,
                                           customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                           appDetailId = ft.LOANAPPLICATIONDETAILID,
                                           appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                           accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                           createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                           ft.LOANSTATUSID,
                                           accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                           ft.DISBURSEDATE,
                                           ft.MATURITYDATE,
                                           ft.DATETIMECREATED

                                       });

                var allLoans = termLoans.Union(revolvingLoans).Union(contingentLoans).Distinct();

                var data = (from a in allLoans
                            join b in context.TBL_CUSTOMER_RELATED_PARTY on a.CUSTOMERID equals b.CUSTOMERID
                            join c in context.TBL_COMPANY_DIRECTOR on b.COMPANYDIRECTORID equals c.COMPANYDIRECTORID
                            where a.DATETIMECREATED >= startDate && a.DATETIMECREATED <= endDate
                            select new RelatedPartyLoansViewModel
                            {
                                loanReferenceNumber = a.LOANREFERENCENUMBER,
                                solId = a.BRANCHCODE,
                                customerName = a.customerName,
                                productName = a.PRODUCTNAME,
                                accountStatus = a.accountStatus,
                                principalAmount = a.PRINCIPALAMOUNT,
                                relatedParty = c.TITLE + " " + c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                                relationshipType = b.RELATIONSHIPTYPE
                            }
                    );
                if (!String.IsNullOrWhiteSpace(loanRefNo))
                {
                    data = data.Where(x => x.loanReferenceNumber == loanRefNo);
                }

                return data.ToList();
            }
        }

        public IEnumerable<MarturedLoansViewModel> GetMaturedLoans(DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short? branchId, int? productClassId, int? staffId)
        {
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    //IEnumerable<STG_MIS_INFO> misInfo = new IEnumerable<STG_MIS_INFO>();
                    // var misInfo = (from mis in stagecontext.STG_MIS_INFO select mis);
                    var termLoans = (from ft in context.TBL_LOAN
                                     join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                     select new
                                     {
                                         ft.LOANREFERENCENUMBER,
                                         ft.PRODUCTID,
                                         BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                         p.PRODUCTCODE,
                                         p.PRODUCTNAME,
                                         ft.CUSTOMERID,
                                         PRINCIPALAMOUNT = ft.PRINCIPALAMOUNT,
                                         customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                         appDetailId = ft.LOANAPPLICATIONDETAILID,
                                         appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                         accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                         createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                         // disbursedby=  "1",//ft.DISBURSEDBY.ToString(),
                                         ft.LOANSTATUSID,
                                         ft.DISBURSEDATE,
                                         ft.MATURITYDATE
                                     });

                    var revolvingLoans = (from ft in context.TBL_LOAN_REVOLVING
                                          join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                          select new
                                          {
                                              ft.LOANREFERENCENUMBER,
                                              ft.PRODUCTID,
                                              BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                              p.PRODUCTCODE,
                                              p.PRODUCTNAME,
                                              ft.CUSTOMERID,
                                              PRINCIPALAMOUNT = ft.OVERDRAFTLIMIT,
                                              customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                              appDetailId = ft.LOANAPPLICATIONDETAILID,
                                              appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                              accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                              createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                              // disbursedby =   ft.DISBURSEDBY,
                                              ft.LOANSTATUSID,
                                              ft.DISBURSEDATE,
                                              ft.MATURITYDATE
                                          });

                    var contingentLoans = (from ft in context.TBL_LOAN_CONTINGENT
                                           join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                           select new
                                           {
                                               ft.LOANREFERENCENUMBER,
                                               ft.PRODUCTID,
                                               BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                               p.PRODUCTCODE,
                                               p.PRODUCTNAME,
                                               ft.CUSTOMERID,
                                               PRINCIPALAMOUNT = ft.CONTINGENTAMOUNT,
                                               customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                               appDetailId = ft.LOANAPPLICATIONDETAILID,
                                               appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                               accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                               createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                               // disbursedby = ft.DISBURSEDBY,
                                               ft.LOANSTATUSID,
                                               ft.DISBURSEDATE,
                                               ft.MATURITYDATE
                                           });

                    var allLoans = termLoans.Union(revolvingLoans).Union(contingentLoans).Distinct();

                    var data = (from a in allLoans
                                join c in context.TBL_LOAN_APPLICATION_DETAIL on a.appDetailId equals c.LOANAPPLICATIONDETAILID
                                join b in context.TBL_LOAN_APPLICATION on c.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                join d in context.TBL_CUSTOMER on a.CUSTOMERID equals d.CUSTOMERID
                                // join e in context.TBL_STAFF on a.disbursedby equals e.STAFFID.ToString()
                                where a.MATURITYDATE < context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE &&
                                a.MATURITYDATE >= startDate && a.MATURITYDATE <= endDate
                                select new MarturedLoansViewModel
                                {
                                    applicationReferenceNumber = b.APPLICATIONREFERENCENUMBER,
                                    solId = a.BRANCHCODE,
                                    customerName = a.customerName,
                                    staffName = a.createdby,
                                    // staffName= e.FIRSTNAME+" "+e.MIDDLENAME+" "+e.LASTNAME,
                                    approvedAmount = b.APPROVEDAMOUNT,
                                    exchangeRate = c.EXCHANGERATE,
                                    approvedTenor = c.APPROVEDTENOR,
                                    principalAmount = a.PRINCIPALAMOUNT,
                                    disbursedDate = a.DISBURSEDATE,
                                    maturityDate = a.MATURITYDATE,
                                    misCode = b.MISCODE,
                                    status = a.LOANSTATUSID == 4 ? "Terminated" : "Not Terminated"
                                }
                        ).ToList().Select(x =>
                        {
                            x.principalAmount = x.approvedAmount * (decimal)x.exchangeRate;

                            //var businessUnitName = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD8).FirstOrDefault();
                            //if (businessUnitName != null)
                            //{
                            //    x.BU = businessUnitName;
                            //}
                            //else
                            //{
                            //    x.BU = "N/A";
                            //}

                            return x;
                        }).ToList();

                    return data.ToList();
                }
            }
        }

        public IEnumerable<ApprovedLoansViewModel> GetApprovedLoans(DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short? branchId, int? productClassId, int? staffId)
        {
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var termLoans = (from ft in context.TBL_LOAN
                                     join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                     select new
                                     {
                                         ft.LOANREFERENCENUMBER,
                                         ft.PRODUCTID,
                                         BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                         p.PRODUCTCODE,
                                         p.PRODUCTNAME,
                                         ft.CUSTOMERID,
                                         PRINCIPALAMOUNT = ft.PRINCIPALAMOUNT,
                                         customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                         appDetailId = ft.LOANAPPLICATIONDETAILID,
                                         appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                         accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                         createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                         gl = ft.TBL_PRODUCT.PRINCIPALBALANCEGL,
                                         ft.TBL_CURRENCY.CURRENCYCODE,
                                         // glCode=ft.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                                         loanid = ft.TERMLOANID,
                                         ft.LOANSTATUSID,
                                         accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                         ft.ISDISBURSED,
                                         ft.DISBURSEDATE,
                                         ft.MATURITYDATE,
                                         ft.DATETIMECREATED,
                                         ft.APPROVERCOMMENT,
                                         ft.LOAN_BOOKING_REQUESTID,
                                         ft.APPROVALSTATUSID
                                     });

                    var revolvingLoans = (from ft in context.TBL_LOAN_REVOLVING
                                          join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                          select new
                                          {
                                              ft.LOANREFERENCENUMBER,
                                              ft.PRODUCTID,
                                              BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                              p.PRODUCTCODE,
                                              p.PRODUCTNAME,
                                              ft.CUSTOMERID,
                                              PRINCIPALAMOUNT = ft.OVERDRAFTLIMIT,
                                              customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                              appDetailId = ft.LOANAPPLICATIONDETAILID,
                                              appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                              accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                              createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                              gl = ft.TBL_PRODUCT.PRINCIPALBALANCEGL,
                                              ft.TBL_CURRENCY.CURRENCYCODE,
                                              //glCode = ft.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                                              loanid = ft.REVOLVINGLOANID,
                                              ft.LOANSTATUSID,
                                              accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                              ft.ISDISBURSED,
                                              ft.DISBURSEDATE,
                                              ft.MATURITYDATE,
                                              ft.DATETIMECREATED,
                                              ft.APPROVERCOMMENT,
                                              ft.LOAN_BOOKING_REQUESTID,
                                              ft.APPROVALSTATUSID
                                          });

                    var contingentLoans = (from ft in context.TBL_LOAN_CONTINGENT
                                           join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                           select new
                                           {
                                               ft.LOANREFERENCENUMBER,
                                               ft.PRODUCTID,
                                               BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                               p.PRODUCTCODE,
                                               p.PRODUCTNAME,
                                               ft.CUSTOMERID,
                                               PRINCIPALAMOUNT = ft.CONTINGENTAMOUNT,
                                               customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                               appDetailId = ft.LOANAPPLICATIONDETAILID,
                                               appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                               accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                               createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                               gl = ft.TBL_PRODUCT.PRINCIPALBALANCEGL,
                                               ft.TBL_CURRENCY.CURRENCYCODE,
                                               // glCode = ft.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                                               loanid = ft.CONTINGENTLOANID,
                                               ft.LOANSTATUSID,
                                               accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                               ft.ISDISBURSED,
                                               ft.DISBURSEDATE,
                                               ft.MATURITYDATE,
                                               ft.DATETIMECREATED,
                                               ft.APPROVERCOMMENT,
                                               ft.LOAN_BOOKING_REQUESTID,
                                               ft.APPROVALSTATUSID

                                           });

                    var allLoans = termLoans.Union(revolvingLoans).Union(contingentLoans).Distinct();
                    //IEnumerable<STG_MIS_INFO> misInfo = new IEnumerable<STG_MIS_INFO>();
                    //var misInfo = (from mis in stagecontext.STG_MIS_INFO select mis);
                    var data = (
                            from a in context.TBL_LOAN_APPLICATION
                            join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                            where b.STATUSID == 2 && a.APPROVEDDATE >= startDate && a.APPROVEDDATE <= endDate
                            select new ApprovedLoansViewModel
                            {
                                applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                solId = a.TBL_BRANCH.BRANCHCODE,
                                //utilizedAmount = allLoans.Where(x => x.appDetailId == b.LOANAPPLICATIONDETAILID && x.ISDISBURSED == true).Sum(y => y.PRINCIPALAMOUNT),
                                disbursedStatus = allLoans.Where(x => x.appDetailId == b.LOANAPPLICATIONDETAILID).Where(x1 => x1.ISDISBURSED == true).Count() > 0 ? "Disbursed" : "Not Disbursed",
                                customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                staffName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                branchName = a.TBL_BRANCH.BRANCHNAME,
                                product = b.TBL_PRODUCT.PRODUCTDESCRIPTION,
                                GLCode = context.TBL_CUSTOM_CHART_OF_ACCOUNT.Where(y => b.TBL_PRODUCT.PRINCIPALBALANCEGL == y.CUSTOMACCOUNTID).Select(z => new { gldesc = z.ACCOUNTID + " ~ " + z.ACCOUNTNAME }).FirstOrDefault().gldesc,
                                manageFee = context.TBL_LOAN_FEE.FirstOrDefault(x => x.LOANID == allLoans.FirstOrDefault(y => y.appDetailId == b.LOANAPPLICATIONDETAILID).loanid && x.CHARGEFEEID == 4).FEEAMOUNT,
                                interestRate = b.APPROVEDINTERESTRATE,
                                approvedTenor = b.APPROVEDTENOR,
                                approvedAmount = b.APPROVEDAMOUNT,
                                currency = context.TBL_CURRENCY.Where(x => x.CURRENCYID == b.CURRENCYID).FirstOrDefault().CURRENCYCODE,
                                exchangeRate = b.EXCHANGERATE,
                                dateTimeCreated = a.DATETIMECREATED,
                                approvedDate = a.APPROVEDDATE,
                                account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == b.OPERATINGCASAACCOUNTID).PRODUCTACCOUNTNUMBER,
                                misCode = a.MISCODE
                            }
                        ).ToList().Select(x =>
                        {
                            x.applicationAmount = x.approvedAmount * (decimal)x.exchangeRate;

                            //var businessUnitName = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD8).FirstOrDefault();
                            //if (businessUnitName != null)
                            //{
                            //    x.BU = businessUnitName;
                            //}
                            //else
                            //{
                            //    x.BU = "N/A";
                            //}

                            return x;
                        }).ToList();

                    return data;
                }
            }
        }
        public IEnumerable<ApprovedLoansViewModel> GetCancelledLoans(DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short? branchId, int? productClassId, int? staffId)
        {
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var termLoans = (from ft in context.TBL_LOAN
                                     join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                     select new
                                     {
                                         ft.LOANREFERENCENUMBER,
                                         ft.PRODUCTID,
                                         BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                         p.PRODUCTCODE,
                                         p.PRODUCTNAME,
                                         ft.CUSTOMERID,
                                         PRINCIPALAMOUNT = ft.PRINCIPALAMOUNT,
                                         customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                         appDetailId = ft.LOANAPPLICATIONDETAILID,
                                         appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                         accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                         createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                         gl = ft.TBL_PRODUCT.PRINCIPALBALANCEGL,
                                         ft.TBL_CURRENCY.CURRENCYCODE,
                                         // glCode=ft.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                                         loanid = ft.TERMLOANID,
                                         ft.LOANSTATUSID,
                                         accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                         ft.ISDISBURSED,
                                         ft.DISBURSEDATE,
                                         ft.MATURITYDATE,
                                         ft.DATETIMECREATED,
                                         ft.APPROVERCOMMENT,
                                         ft.LOAN_BOOKING_REQUESTID,
                                         ft.APPROVALSTATUSID
                                     });

                    var revolvingLoans = (from ft in context.TBL_LOAN_REVOLVING
                                          join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                          select new
                                          {
                                              ft.LOANREFERENCENUMBER,
                                              ft.PRODUCTID,
                                              BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                              p.PRODUCTCODE,
                                              p.PRODUCTNAME,
                                              ft.CUSTOMERID,
                                              PRINCIPALAMOUNT = ft.OVERDRAFTLIMIT,
                                              customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                              appDetailId = ft.LOANAPPLICATIONDETAILID,
                                              appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                              accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                              createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                              gl = ft.TBL_PRODUCT.PRINCIPALBALANCEGL,
                                              ft.TBL_CURRENCY.CURRENCYCODE,
                                              //glCode = ft.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                                              loanid = ft.REVOLVINGLOANID,
                                              ft.LOANSTATUSID,
                                              accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                              ft.ISDISBURSED,
                                              ft.DISBURSEDATE,
                                              ft.MATURITYDATE,
                                              ft.DATETIMECREATED,
                                              ft.APPROVERCOMMENT,
                                              ft.LOAN_BOOKING_REQUESTID,
                                              ft.APPROVALSTATUSID
                                          });

                    var contingentLoans = (from ft in context.TBL_LOAN_CONTINGENT
                                           join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                           select new
                                           {
                                               ft.LOANREFERENCENUMBER,
                                               ft.PRODUCTID,
                                               BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                               p.PRODUCTCODE,
                                               p.PRODUCTNAME,
                                               ft.CUSTOMERID,
                                               PRINCIPALAMOUNT = ft.CONTINGENTAMOUNT,
                                               customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                               appDetailId = ft.LOANAPPLICATIONDETAILID,
                                               appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                               accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                               createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                               gl = ft.TBL_PRODUCT.PRINCIPALBALANCEGL,
                                               ft.TBL_CURRENCY.CURRENCYCODE,
                                               // glCode = ft.TBL_PRODUCT.TBL_CHART_OF_ACCOUNT.ACCOUNTCODE,
                                               loanid = ft.CONTINGENTLOANID,
                                               ft.LOANSTATUSID,
                                               accountStatus = ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                               ft.ISDISBURSED,
                                               ft.DISBURSEDATE,
                                               ft.MATURITYDATE,
                                               ft.DATETIMECREATED,
                                               ft.APPROVERCOMMENT,
                                               ft.LOAN_BOOKING_REQUESTID,
                                               ft.APPROVALSTATUSID

                                           });

                    var allLoans = termLoans.Union(revolvingLoans).Union(contingentLoans).Distinct();
                    //IEnumerable<STG_MIS_INFO> misInfo = new IEnumerable<STG_MIS_INFO>();
                    //var misInfo = (from mis in stagecontext.STG_MIS_INFO select mis);
                    var data = (
                            from a in context.TBL_LOAN_APPLICATION
                            join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                            where a.APPLICATIONSTATUSID == 22 && context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == a.LOANAPPLICATIONID).Max(x => x.SYSTEMARRIVALDATETIME) >= startDate && context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == a.LOANAPPLICATIONID).Max(x => x.SYSTEMARRIVALDATETIME) <= endDate
                            select new ApprovedLoansViewModel
                            {
                                applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                solId = a.TBL_BRANCH.BRANCHCODE,
                                utilizedAmount = allLoans.Where(x => x.appDetailId == b.LOANAPPLICATIONDETAILID && x.ISDISBURSED == true).Sum(y => y.PRINCIPALAMOUNT),
                                disbursedStatus = allLoans.Where(x => x.appDetailId == b.LOANAPPLICATIONDETAILID).Where(x1 => x1.ISDISBURSED == true).Count() > 0 ? "Disbursed" : "Not Disbursed",
                                customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                                staffName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                branchName = a.TBL_BRANCH.BRANCHNAME,
                                product = b.TBL_PRODUCT.PRODUCTDESCRIPTION,
                                GLCode = context.TBL_CUSTOM_CHART_OF_ACCOUNT.Where(y => b.TBL_PRODUCT.PRINCIPALBALANCEGL == y.CUSTOMACCOUNTID).Select(z => new { gldesc = z.ACCOUNTID + " ~ " + z.ACCOUNTNAME }).FirstOrDefault().gldesc,
                                manageFee = context.TBL_LOAN_FEE.FirstOrDefault(x => x.LOANID == allLoans.FirstOrDefault(y => y.appDetailId == b.LOANAPPLICATIONDETAILID).loanid && x.CHARGEFEEID == 4).FEEAMOUNT,
                                interestRate = b.APPROVEDINTERESTRATE,
                                approvedTenor = b.APPROVEDTENOR,
                                approvedAmount = b.APPROVEDAMOUNT,
                                currency = context.TBL_CURRENCY.Where(x => x.CURRENCYID == b.CURRENCYID).FirstOrDefault().CURRENCYCODE,
                                exchangeRate = b.EXCHANGERATE,
                                dateTimeCreated = a.DATETIMECREATED,
                                approvedDate = context.TBL_APPROVAL_TRAIL.Where(x => x.TARGETID == a.LOANAPPLICATIONID).Max(x => x.SYSTEMARRIVALDATETIME),
                                account = context.TBL_CASA.FirstOrDefault(x => x.CASAACCOUNTID == b.OPERATINGCASAACCOUNTID).PRODUCTACCOUNTNUMBER,
                                misCode = a.MISCODE
                            }
                        ).ToList().Select(x =>
                        {
                            x.applicationAmount = x.approvedAmount * (decimal)x.exchangeRate;

                            //var businessUnitName = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD8).FirstOrDefault();
                            //if (businessUnitName != null)
                            //{
                            //    x.BU = businessUnitName;
                            //}
                            //else
                            //{
                            //    x.BU = "N/A";
                            //}

                            return x;
                        }).ToList();

                    return data;
                }
            }
        }

        public IEnumerable<InitiatedLoansViewModel> GetInitiatedLoans(DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short? branchId, int? productClassId, int? staffId)
        {
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    //IEnumerable<STG_MIS_INFO> misInfo = new IEnumerable<STG_MIS_INFO>();
                    //var misInfo = (from mis in stagecontext.STG_MIS_INFO select mis);
                    var data = (
                        from a in context.TBL_LOAN_APPLICATION
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        where a.DATETIMECREATED >= startDate && a.DATETIMECREATED <= endDate
                        select new InitiatedLoansViewModel
                        {
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            solId = a.TBL_BRANCH.BRANCHCODE,
                            customerName = a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME,
                            staffName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                            approvedTenor = b.APPROVEDTENOR,
                            proposedAmount = b.PROPOSEDAMOUNT,
                            exchangeRate = b.EXCHANGERATE,
                            product = b.TBL_PRODUCT.PRODUCTNAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            misCode = a.MISCODE
                        }
                    ).ToList().Select(x =>
                    {
                        x.applicationAmount = x.proposedAmount * (decimal)x.exchangeRate;
                        //var businessUnitName = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD8).FirstOrDefault();
                        //if (businessUnitName != null)
                        //{
                        //    x.BU = businessUnitName;
                        //}
                        //else
                        //{
                        //    x.BU = "N/A";
                        //}

                        return x;
                    }).ToList();

                    return data;
                }
            }
        }
        public IEnumerable<CustomerViewModel> GetListOfCustomers(DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short? branchId, int? productClassId, int? staffId)
        {
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    //IEnumerable<STG_MIS_INFO> misInfo = new IEnumerable<STG_MIS_INFO>();
                    //var misInfo = (from mis in stagecontext.STG_MIS_INFO select mis);
                    var data = (
                        from a in context.TBL_CUSTOMER


                        select new CustomerViewModel
                        {
                            customerCode = a.CUSTOMERCODE,
                            firstName = a.FIRSTNAME,
                            lastName = a.LASTNAME,
                            solId = a.TBL_BRANCH.BRANCHCODE,
                            staffname = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                            dateTimeCreated = a.DATETIMECREATED,
                            misCode = a.MISCODE
                        }
                    ).ToList().Select(x =>
                    {

                        //var businessUnitName = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD8).FirstOrDefault();
                        //if (businessUnitName != null)
                        //{
                        //    x.BU = businessUnitName;
                        //}
                        //else
                        //{
                        //    x.BU = "N/A";
                        //}

                        return x;
                    }).ToList(); ;

                    return data;
                }
            }
        }

        public IEnumerable<TerminatedLoansViewModel> GetTerminatedLoans(DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short? branchId, int? productClassId, int? staffId)
        {
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    //IEnumerable<STG_MIS_INFO> misInfo = new IEnumerable<STG_MIS_INFO>();
                    // var misInfo = (from mis in stagecontext.STG_MIS_INFO select mis);
                    var termLoans = (from ft in context.TBL_LOAN
                                     join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                     select new
                                     {
                                         ft.LOANREFERENCENUMBER,
                                         ft.PRODUCTID,
                                         BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                         p.PRODUCTCODE,
                                         p.PRODUCTNAME,
                                         ft.CUSTOMERID,
                                         PRINCIPALAMOUNT = ft.PRINCIPALAMOUNT,
                                         customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                         appDetailId = ft.LOANAPPLICATIONDETAILID,
                                         appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                         accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                         createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                         ft.LOANSTATUSID,
                                         ft.DISBURSEDATE,
                                         ft.MATURITYDATE
                                     });

                    var revolvingLoans = (from ft in context.TBL_LOAN_REVOLVING
                                          join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                          select new
                                          {
                                              ft.LOANREFERENCENUMBER,
                                              ft.PRODUCTID,
                                              BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                              p.PRODUCTCODE,
                                              p.PRODUCTNAME,
                                              ft.CUSTOMERID,
                                              PRINCIPALAMOUNT = ft.OVERDRAFTLIMIT,
                                              customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                              appDetailId = ft.LOANAPPLICATIONDETAILID,
                                              appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                              accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                              createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                              ft.LOANSTATUSID,
                                              ft.DISBURSEDATE,
                                              ft.MATURITYDATE
                                          });

                    var contingentLoans = (from ft in context.TBL_LOAN_CONTINGENT
                                           join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                           select new
                                           {
                                               ft.LOANREFERENCENUMBER,
                                               ft.PRODUCTID,
                                               BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                               p.PRODUCTCODE,
                                               p.PRODUCTNAME,
                                               ft.CUSTOMERID,
                                               PRINCIPALAMOUNT = ft.CONTINGENTAMOUNT,
                                               customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                               appDetailId = ft.LOANAPPLICATIONDETAILID,
                                               appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                               accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                               createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                               ft.LOANSTATUSID,
                                               ft.DISBURSEDATE,
                                               ft.MATURITYDATE

                                           });

                    var allLoans = termLoans.Union(revolvingLoans).Union(contingentLoans).Distinct();

                    var data = (from a in allLoans
                                join c in context.TBL_LOAN_APPLICATION_DETAIL on a.appDetailId equals c.LOANAPPLICATIONDETAILID
                                join b in context.TBL_LOAN_APPLICATION on c.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                join d in context.TBL_CUSTOMER on a.CUSTOMERID equals d.CUSTOMERID

                                where a.LOANSTATUSID == 4 && a.DISBURSEDATE >= startDate && a.DISBURSEDATE <= endDate
                                select new TerminatedLoansViewModel
                                {
                                    applicationReferenceNumber = b.APPLICATIONREFERENCENUMBER,
                                    solId = a.BRANCHCODE,
                                    customerName = a.customerName,
                                    staffName = a.createdby,
                                    approvedTenor = c.APPROVEDTENOR,
                                    principalAmount = a.PRINCIPALAMOUNT,
                                    disbursedDate = a.DISBURSEDATE,
                                    maturityDate = a.MATURITYDATE,
                                    misCode = b.MISCODE
                                }
                        ).ToList().Select(x =>
                        {


                            //var businessUnitName = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD8).FirstOrDefault();
                            //if (businessUnitName != null)
                            //{
                            //    x.BU = businessUnitName;
                            //}
                            //else
                            //{
                            //    x.BU = "N/A";
                            //}

                            return x;
                        }).ToList();

                    return data.ToList();
                }
            }
        }

        public IEnumerable<DisburstLoanViewModel> GetDisbursedLoans(DateTime startDate, DateTime endDate, int companyId, string loanRefNo, short? branchId, int? productClassId, int? staffId)
        {
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    //IEnumerable<STG_MIS_INFO> misInfo = new IEnumerable<STG_MIS_INFO>();
                    //var misInfo = (from mis in stagecontext.STG_MIS_INFO select mis);
                    var termLoans = (from ft in context.TBL_LOAN
                                     join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                     select new
                                     {
                                         ft.LOANREFERENCENUMBER,
                                         ft.PRODUCTID,
                                         BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                         p.PRODUCTCODE,
                                         p.PRODUCTNAME,
                                         ft.CUSTOMERID,
                                         PRINCIPALAMOUNT = ft.PRINCIPALAMOUNT,
                                         customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                         appDetailId = ft.LOANAPPLICATIONDETAILID,
                                         appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                         accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                         createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                         exchnageRate = ft.EXCHANGERATE,
                                         disbusedby = "",
                                         isDisbursed = ft.ISDISBURSED,
                                         ft.LOANSTATUSID,
                                         ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                         ft.DISBURSEDATE,
                                         ft.MATURITYDATE
                                     });

                    var revolvingLoans = (from ft in context.TBL_LOAN_REVOLVING
                                          join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                          select new
                                          {
                                              ft.LOANREFERENCENUMBER,
                                              ft.PRODUCTID,
                                              BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                              p.PRODUCTCODE,
                                              p.PRODUCTNAME,
                                              ft.CUSTOMERID,
                                              PRINCIPALAMOUNT = ft.OVERDRAFTLIMIT,
                                              customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                              appDetailId = ft.LOANAPPLICATIONDETAILID,
                                              appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                              accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                              createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                              exchnageRate = ft.EXCHANGERATE,
                                              disbusedby = ft.DISBURSEDBY,
                                              isDisbursed = ft.ISDISBURSED,
                                              ft.LOANSTATUSID,
                                              ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,

                                              ft.DISBURSEDATE,
                                              ft.MATURITYDATE
                                          });

                    var contingentLoans = (from ft in context.TBL_LOAN_CONTINGENT
                                           join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                           select new
                                           {
                                               ft.LOANREFERENCENUMBER,
                                               ft.PRODUCTID,
                                               BRANCHCODE = ft.TBL_BRANCH.BRANCHCODE,
                                               p.PRODUCTCODE,
                                               p.PRODUCTNAME,
                                               ft.CUSTOMERID,
                                               PRINCIPALAMOUNT = ft.CONTINGENTAMOUNT,
                                               customerName = ft.TBL_CUSTOMER.FIRSTNAME + " " + ft.TBL_CUSTOMER.LASTNAME + " " + ft.TBL_CUSTOMER.MIDDLENAME,
                                               appDetailId = ft.LOANAPPLICATIONDETAILID,
                                               appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                               accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                               createdby = ft.TBL_STAFF.FIRSTNAME + " " + ft.TBL_STAFF.MIDDLENAME + " " + ft.TBL_STAFF.LASTNAME,
                                               exchnageRate = ft.EXCHANGERATE,
                                               disbusedby = ft.DISBURSEDBY,
                                               isDisbursed = ft.ISDISBURSED,

                                               ft.LOANSTATUSID,
                                               ft.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                               ft.DISBURSEDATE,
                                               ft.MATURITYDATE
                                           });

                    var allLoans = termLoans.Union(revolvingLoans).Union(contingentLoans).Distinct();

                    var data = (from a in allLoans
                                join c in context.TBL_LOAN_APPLICATION_DETAIL on a.appDetailId equals c.LOANAPPLICATIONDETAILID
                                join b in context.TBL_LOAN_APPLICATION on c.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                                join d in context.TBL_CUSTOMER on a.CUSTOMERID equals d.CUSTOMERID

                                where a.isDisbursed && a.DISBURSEDATE >= startDate && a.DISBURSEDATE <= endDate
                                select new DisburstLoanViewModel
                                {
                                    applicationReferenceNumber = b.APPLICATIONREFERENCENUMBER,
                                    solId = a.BRANCHCODE,
                                    branchName = context.TBL_BRANCH.FirstOrDefault(x => x.BRANCHCODE == a.BRANCHCODE).BRANCHNAME,
                                    customerName = a.customerName,
                                    loanrefnum = a.LOANREFERENCENUMBER,
                                    status = a.ACCOUNTSTATUS,
                                    exchangeRate = a.exchnageRate,
                                    tenor = c.APPROVEDTENOR + " " + context.TBL_TENOR_MODE.Where(x => x.TENORMODEID == c.TENORFREQUENCYTYPEID).FirstOrDefault().TENORMODENAME,
                                    staffName = a.createdby,
                                    disbursedBy = a.disbusedby,
                                    pricipalAmount = a.PRINCIPALAMOUNT,
                                    disburseDate = a.DISBURSEDATE,
                                    maturitydate = a.MATURITYDATE,
                                    misCode = b.MISCODE,
                                    productName = a.PRODUCTNAME //context.TBL_PRODUCT.FirstOrDefault(x=>a.PRODUCTID==c.APPROVEDPRODUCTID).PRODUCTNAME
                                }
                        ).ToList().Select(x =>
                        {
                            //x.amountDisbursed = x.pricipalAmount * (decimal)x.exchangeRate;
                            //if (string.IsNullOrEmpty(x.disbursedBy) | string.IsNullOrWhiteSpace(x.disbursedBy))
                            //{
                            //    try
                            //    {
                            //        x.disbursedBy = context.TBL_LOAN.Where(o => o.LOANREFERENCENUMBER == o.LOANREFERENCENUMBER).FirstOrDefault().DISBURSEDBY.ToString();
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        x.disbursedBy = "";
                            //    }

                            //var businessUnitName = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD8).FirstOrDefault();
                            //if (businessUnitName != null)
                            //{
                            //    x.BU = businessUnitName;
                            //}
                            //else
                            //{
                            //    x.BU = "N/A";
                            //}

                            //}
                            //x.staffName = GetStaffFullName(int.Parse(x.disbursedBy));
                            return x;
                        }).ToList();

                    return data.ToList();
                }
            }
        }

        public string GetStaffFullName(int? staffid)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var data = from a in context.TBL_STAFF
                           where a.STAFFID == staffid
                           select new fullname
                           {
                               name = a.FIRSTNAME + " " + a.MIDDLENAME + " " + a.LASTNAME
                           };


                return data.FirstOrDefault().name;//staffFullName context.TBL_STAFF.Where(y =>new  { fullname=y.FIRSTNAME + " " + y.MIDDLENAME + " " + y.LASTNAME}).fullname;

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

        //public IList<LoanAnniverseryViewModel> LoanAnniversery(DateTime startDate, DateTime endDate, int companyId)
        //{
        //    using (FinTrakBankingContext context = new FinTrakBankingContext())
        //    {
        //        var data = from a in context.TBL_LOAN
        //                   join b in context.TBL_LOAN_SCHEDULE_PERIODIC on a.TERMLOANID equals b.LOANID
        //                   join c in context.TBL_CUSTOMER_PHONECONTACT on a.CUSTOMERID equals c.CUSTOMERID
        //                   where a.COMPANYID == companyId && a.LOANSTATUSID == (int)LoanStatusEnum.Active
        //                   && DbFunctions.TruncateTime(b.PAYMENTDATE) >= DbFunctions.TruncateTime(startDate)
        //                    && DbFunctions.TruncateTime(b.PAYMENTDATE) <= DbFunctions.TruncateTime(endDate)
        //                   orderby b.PAYMENTDATE descending
        //                   //&& DbFunctions.TruncateTime(startDate) >= DbFunctions.TruncateTime(b.PAYMENTDATE)
        //                   //&& DbFunctions.TruncateTime(b.PAYMENTDATE) <= DbFunctions.TruncateTime(endDate)
        //                   select new LoanAnniverseryViewModel()
        //                   {
        //                       customerId = a.CUSTOMERID,
        //                       maturityDate = a.MATURITYDATE,
        //                       grantedAmount = a.PRINCIPALAMOUNT,
        //                       outstandingIntrestAmt = a.OUTSTANDINGINTEREST,
        //                       outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
        //                       loanRefrenceNumber = a.LOANREFERENCENUMBER,
        //                       applicationRefrenceNumber = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
        //                       accountNumber = a.TBL_CASA.PRODUCTACCOUNTNUMBER,
        //                       productName = a.TBL_PRODUCT.PRODUCTNAME,
        //                       productId = a.PRODUCTID,
        //                       companyName = a.TBL_COMPANY.NAME,
        //                       logoPath = a.TBL_COMPANY.LOGOPATH,
        //                       firstName = a.TBL_CUSTOMER.FIRSTNAME,
        //                       lastName = a.TBL_CUSTOMER.LASTNAME,

        //                       middleName = a.TBL_CUSTOMER.MIDDLENAME,
        //                       totalperiodicPaymentAmt = b.PERIODPAYMENTAMOUNT,
        //                       periodicInterestAmt = b.PERIODINTERESTAMOUNT,
        //                       periodicPrincipalAmt = b.PERIODPRINCIPALAMOUNT,
        //                       paymentdate = b.PAYMENTDATE,
        //                       intrestrate = b.INTERESTRATE,
        //                       emailAddress = a.TBL_CUSTOMER.EMAILADDRESS,
        //                       phoneNumber = c.PHONENUMBER,
        //                       totalOutstanding = a.PASTDUEPRINCIPAL + a.PASTDUEINTEREST + a.INTERESTONPASTDUEINTEREST + a.INTERESTONPASTDUEPRINCIPAL + a.OUTSTANDINGINTEREST + a.OUTSTANDINGPRINCIPAL,


        //                   };

        //        return data.ToList();
        //    }

        //}

        public IList<LoanAnniverseryViewModel> LoanAnniversery(DateTime startDate, DateTime endDate, int companyId)
        {
            List<FINTRAK_TRAN_PROC_DETAILS> procedureDetails = new List<FINTRAK_TRAN_PROC_DETAILS>();
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                procedureDetails = stagecontext.FINTRAK_TRAN_PROC_DETAILS.ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var data = (from a in context.TBL_LOAN
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
                                    phoneNumber = c.PHONENUMBER,
                                    totalOutstanding = a.PASTDUEPRINCIPAL + a.PASTDUEINTEREST + a.INTERESTONPASTDUEINTEREST + a.INTERESTONPASTDUEPRINCIPAL + a.OUTSTANDINGINTEREST + a.OUTSTANDINGPRINCIPAL,


                                }).ToList().Select(x =>
                                {
                                    var recoveredValue = procedureDetails.Where(f => f.LOAN_ACCT == x.loanRefrenceNumber && f.RCRE_DATE == x.paymentdate && ((f.NARRATION.Equals("Principal Repayment") || f.NARRATION.Equals("Interest Repayment")))).Select(f => f.AMT_COLLECTED).Sum();
                                    Decimal finalrecoveredAmount = 0.0M;

                                    finalrecoveredAmount = recoveredValue;


                                    x.recoveredAmount = finalrecoveredAmount;





                                    return x;
                                });

                    return data.ToList();
                }
            }

        }


        public IList<LoanDocumentWaivedViewModel> LoanDocumentDeferred(DateTime startDate, DateTime endDate, int companyId, short? branchId)
        {

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var deferredConditionsTerm = (from a in context.TBL_LOAN_CONDITION_PRECEDENT
                                              join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                              join c in context.TBL_LOAN_CONDITION_DEFERRAL on a.LOANCONDITIONID equals c.LOANCONDITIONID
                                              join d in context.TBL_CHECKLIST_STATUS on a.CHECKLISTSTATUSID equals d.CHECKLISTSTATUSID
                                              join e in context.TBL_LOAN on b.CUSTOMERID equals e.CUSTOMERID
                                              join cc in context.TBL_CUSTOMER on b.CUSTOMERID equals cc.CUSTOMERID
                                              join p in context.TBL_PRODUCT on b.PROPOSEDPRODUCTID equals p.PRODUCTID
                                              where a.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Deferred
                                               //&& cc.COMPANYID == companyId
                                               && DbFunctions.TruncateTime(a.DEFEREDDATE) >= DbFunctions.TruncateTime(startDate)
                                               && DbFunctions.TruncateTime(a.DEFEREDDATE) <= DbFunctions.TruncateTime(endDate)
                                               // && b.TBL_LOAN_APPLICATION.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                               && c.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                               && (cc.BRANCHID == branchId || branchId == null || branchId == 0)
                                              orderby c.DATETIMECREATED descending
                                              select new LoanDocumentWaivedViewModel()
                                              {
                                                  applicationRefrenceNumber = b.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                  defferalDocument = a.CONDITION,
                                                  facilityAmount = b.APPROVEDAMOUNT,
                                                  facilityExpirationDate = e.MATURITYDATE,
                                                  facilityGrantedDate = e.EFFECTIVEDATE,
                                                  facilityType = b.TBL_PRODUCT.PRODUCTNAME,
                                                  customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,
                                                  initialDefferalDate = c.DATETIMECREATED,
                                                  nameOfRM = context.TBL_STAFF.Where(o => o.STAFFID == e.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                                  customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + " " + b.TBL_CUSTOMER.LASTNAME,
                                                  currentExposure = e.OUTSTANDINGPRINCIPAL + e.PASTDUEPRINCIPAL,
                                                  currentDefferalDate = c.DEFERREDDATE,
                                                  deferralDurration = (int)DbFunctions.DiffDays((DateTime?)c.DEFERREDDATE, (DateTime?)c.DATETIMECREATED).Value,
                                                  cummulativeDays = c.DEFERREDDATE.Value.Day + c.DATETIMECREATED.Day,
                                                  deferralExpiryDate = (b.EXPIRYDATE.Value == null ? default(DateTime) : b.EXPIRYDATE.Value),
                                                  nameOfBM = context.TBL_STAFF.Where(o => o.SUPERVISOR_STAFFID.Value == b.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                                  createdBy = a.CREATEDBY,
                                                  businessUnitId = cc.BUSINESSUNTID,
                                                  reasonForDeferral = c.DEFERRALREASON,
                                                  finalApprovalDate = c.DEFEREDDATEONFINALAPPROVAL,
                                                  perfectionRelated = (from x in context.TBL_LOAN_APPLICATION_COLLATERL join t in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals t.COLLATERALCUSTOMERID join y in context.TBL_COLLATERAL_TYPE on t.COLLATERALTYPEID equals y.COLLATERALTYPEID where x.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID select y.COLLATERALTYPEID).FirstOrDefault() == (int)CollateralTypeEnum.Property ? "Yes" : "No",
                                                  currency = (from x in context.TBL_LOAN_APPLICATION_COLLATERL join t in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals t.COLLATERALCUSTOMERID join y in context.TBL_CURRENCY on t.CURRENCYID equals y.CURRENCYID where x.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID select y.CURRENCYCODE).FirstOrDefault(),
                                                  accountOfficer = context.TBL_STAFF.Where(s => s.STAFFID == a.CREATEDBY).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault(),

                                              }).ToList();

                var deferredConditionsRevolving = (from a in context.TBL_LOAN_CONDITION_PRECEDENT
                                                   join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                                   join c in context.TBL_LOAN_CONDITION_DEFERRAL on a.LOANCONDITIONID equals c.LOANCONDITIONID
                                                   join d in context.TBL_CHECKLIST_STATUS on a.CHECKLISTSTATUSID equals d.CHECKLISTSTATUSID
                                                   join e in context.TBL_LOAN_REVOLVING on b.CUSTOMERID equals e.CUSTOMERID
                                                   join cc in context.TBL_CUSTOMER on b.CUSTOMERID equals cc.CUSTOMERID
                                                   join p in context.TBL_PRODUCT on b.PROPOSEDPRODUCTID equals p.PRODUCTID
                                                   where a.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Deferred
                                                    //&& cc.COMPANYID == companyId
                                                    && DbFunctions.TruncateTime(a.DEFEREDDATE) >= DbFunctions.TruncateTime(startDate)
                                                    && DbFunctions.TruncateTime(a.DEFEREDDATE) <= DbFunctions.TruncateTime(endDate)
                                                    //&& b.TBL_LOAN_APPLICATION.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                                    && c.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                                    && (cc.BRANCHID == branchId || branchId == null || branchId == 0)
                                                   orderby c.DATETIMECREATED descending
                                                   select new LoanDocumentWaivedViewModel()
                                                   {
                                                       applicationRefrenceNumber = b.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                       defferalDocument = a.CONDITION,
                                                       facilityAmount = b.APPROVEDAMOUNT,
                                                       facilityExpirationDate = e.MATURITYDATE,
                                                       facilityGrantedDate = e.EFFECTIVEDATE,
                                                       facilityType = b.TBL_PRODUCT.PRODUCTNAME,
                                                       customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,
                                                       initialDefferalDate = c.DATETIMECREATED,
                                                       nameOfRM = context.TBL_STAFF.Where(o => o.STAFFID == e.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                                       customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + " " + b.TBL_CUSTOMER.LASTNAME,
                                                       currentExposure = e.OVERDRAFTLIMIT + e.PASTDUEPRINCIPAL,
                                                       currentDefferalDate = c.DEFERREDDATE,
                                                       deferralDurration = (int)DbFunctions.DiffDays((DateTime?)c.DEFERREDDATE, (DateTime?)c.DATETIMECREATED).Value,
                                                       cummulativeDays = c.DEFERREDDATE.Value.Day + c.DATETIMECREATED.Day,
                                                       deferralExpiryDate = (b.EXPIRYDATE.Value == null ? default(DateTime) : b.EXPIRYDATE.Value),
                                                       nameOfBM = context.TBL_STAFF.Where(o => o.SUPERVISOR_STAFFID.Value == b.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                                       createdBy = a.CREATEDBY,
                                                       businessUnitId = cc.BUSINESSUNTID,
                                                       reasonForDeferral = c.DEFERRALREASON,
                                                       finalApprovalDate = c.DEFEREDDATEONFINALAPPROVAL,
                                                       perfectionRelated = (from x in context.TBL_LOAN_APPLICATION_COLLATERL join t in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals t.COLLATERALCUSTOMERID join y in context.TBL_COLLATERAL_TYPE on t.COLLATERALTYPEID equals y.COLLATERALTYPEID where x.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID select y.COLLATERALTYPEID).FirstOrDefault() == (int)CollateralTypeEnum.Property ? "Yes" : "No",
                                                       currency = (from x in context.TBL_LOAN_APPLICATION_COLLATERL join t in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals t.COLLATERALCUSTOMERID join y in context.TBL_CURRENCY on t.CURRENCYID equals y.CURRENCYID where x.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID select y.CURRENCYCODE).FirstOrDefault(),
                                                       accountOfficer = context.TBL_STAFF.Where(s => s.STAFFID == a.CREATEDBY).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault(),

                                                   }).ToList();

                var deferredConditionsContingent = (from a in context.TBL_LOAN_CONDITION_PRECEDENT
                                                    join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                                    join c in context.TBL_LOAN_CONDITION_DEFERRAL on a.LOANCONDITIONID equals c.LOANCONDITIONID
                                                    join d in context.TBL_CHECKLIST_STATUS on a.CHECKLISTSTATUSID equals d.CHECKLISTSTATUSID
                                                    join e in context.TBL_LOAN_CONTINGENT on b.CUSTOMERID equals e.CUSTOMERID
                                                    join cc in context.TBL_CUSTOMER on b.CUSTOMERID equals cc.CUSTOMERID
                                                    join p in context.TBL_PRODUCT on b.PROPOSEDPRODUCTID equals p.PRODUCTID
                                                    where a.CHECKLISTSTATUSID == (short)CheckListStatusEnum.Deferred
                                                     //&& cc.COMPANYID == companyId
                                                     && DbFunctions.TruncateTime(a.DEFEREDDATE) >= DbFunctions.TruncateTime(startDate)
                                                     && DbFunctions.TruncateTime(a.DEFEREDDATE) <= DbFunctions.TruncateTime(endDate)
                                                     //&& b.TBL_LOAN_APPLICATION.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                                     && c.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                                     && (cc.BRANCHID == branchId || branchId == null || branchId == 0)
                                                    orderby c.DATETIMECREATED descending
                                                    select new LoanDocumentWaivedViewModel()
                                                    {
                                                        applicationRefrenceNumber = b.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                        defferalDocument = a.CONDITION,
                                                        facilityAmount = b.APPROVEDAMOUNT,
                                                        facilityExpirationDate = e.MATURITYDATE,
                                                        facilityGrantedDate = e.EFFECTIVEDATE,
                                                        facilityType = b.TBL_PRODUCT.PRODUCTNAME,
                                                        customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,
                                                        initialDefferalDate = c.DATETIMECREATED,
                                                        nameOfRM = context.TBL_STAFF.Where(o => o.STAFFID == e.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                                        customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + " " + b.TBL_CUSTOMER.LASTNAME,
                                                        currentExposure = e.CONTINGENTAMOUNT,
                                                        currentDefferalDate = c.DEFERREDDATE,
                                                        deferralDurration = (int)DbFunctions.DiffDays((DateTime?)c.DEFERREDDATE, (DateTime?)c.DATETIMECREATED).Value,
                                                        cummulativeDays = c.DEFERREDDATE.Value.Day + c.DATETIMECREATED.Day,
                                                        deferralExpiryDate = (b.EXPIRYDATE.Value == null ? default(DateTime) : b.EXPIRYDATE.Value),
                                                        nameOfBM = context.TBL_STAFF.Where(o => o.SUPERVISOR_STAFFID.Value == b.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                                        createdBy = a.CREATEDBY,
                                                        businessUnitId = cc.BUSINESSUNTID,
                                                        reasonForDeferral = c.DEFERRALREASON,
                                                        finalApprovalDate = c.DEFEREDDATEONFINALAPPROVAL,
                                                        perfectionRelated = (from x in context.TBL_LOAN_APPLICATION_COLLATERL join t in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals t.COLLATERALCUSTOMERID join y in context.TBL_COLLATERAL_TYPE on t.COLLATERALTYPEID equals y.COLLATERALTYPEID where x.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID select y.COLLATERALTYPEID).FirstOrDefault() == (int)CollateralTypeEnum.Property ? "Yes" : "No",
                                                        currency = (from x in context.TBL_LOAN_APPLICATION_COLLATERL join t in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals t.COLLATERALCUSTOMERID join y in context.TBL_CURRENCY on t.CURRENCYID equals y.CURRENCYID where x.LOANAPPLICATIONDETAILID == b.LOANAPPLICATIONDETAILID select y.CURRENCYCODE).FirstOrDefault(),
                                                        accountOfficer = context.TBL_STAFF.Where(s => s.STAFFID == a.CREATEDBY).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault(),
                                                        dateCreated = c.DATETIMECREATED,
                                                    }).ToList();

                var data = deferredConditionsTerm.Union(deferredConditionsRevolving).Union(deferredConditionsContingent);
                var deferredConditions = data.ToList();

                foreach (var k in deferredConditions)
                {
                    var ao = context.TBL_STAFF.Find(k.createdBy);
                    if (ao != null && ao.SUPERVISOR_STAFFID != null)
                    {
                        k.relationshipManager = context.TBL_STAFF.Where(o => o.STAFFID == ao.SUPERVISOR_STAFFID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault();
                        var rm = context.TBL_STAFF.Where(o => o.STAFFID == ao.SUPERVISOR_STAFFID).FirstOrDefault();

                        if (rm != null && rm.SUPERVISOR_STAFFID != null)
                        {
                            var zh = context.TBL_STAFF.Where(o => o.STAFFID == rm.SUPERVISOR_STAFFID).FirstOrDefault();
                            if (zh != null && zh.SUPERVISOR_STAFFID != null)
                            {
                                var gh = context.TBL_STAFF.Where(o => o.STAFFID == zh.SUPERVISOR_STAFFID).FirstOrDefault();
                                k.groupHead = gh.FIRSTNAME + " " + gh.LASTNAME + " " + gh.MIDDLENAME;
                            }
                        }
                    }

                    k.sbu = context.TBL_PROFILE_BUSINESS_UNIT.Where(f => f.BUSINESSUNITID == k.businessUnitId).Select(f => f.BUSINESSUNITNAME + " " + f.BUSINESSUNITSHORTCODE).FirstOrDefault();
                }

                return deferredConditions.OrderBy(u => u.dateCreated).ToList();
            }
        }

        public IList<LoanDocumentWaivedViewModel> LoanDocumentWaived(DateTime startDate, DateTime endDate, int companyId, short? branchId, string searchParameter)
        {
            //List<SubHead> subList = new List<SubHead>();
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                //subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION }).ToList();
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

                                            //&& b.TBL_LOAN_APPLICATION.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                            //&& c.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                                            //&& (b.TBL_CUSTOMER.BRANCHID == branchId || branchId == null || branchId == 0)
                                            //&& (b.APPROVEDAMOUNT.ToString().Contains(searchParameter.Trim()) || b.TBL_PRODUCT.PRODUCTNAME.Contains(searchParameter.Trim()) || b.TBL_CUSTOMER.CUSTOMERCODE.Contains(searchParameter.Trim())
                                            //  || b.TBL_CUSTOMER.FIRSTNAME.StartsWith(searchParameter.Trim()) || b.TBL_CUSTOMER.MIDDLENAME.StartsWith(searchParameter.Trim()) || b.TBL_CUSTOMER.LASTNAME.StartsWith(searchParameter.Trim())
                                            //  || b.TBL_CUSTOMER.FIRSTNAME.Contains(searchParameter.Trim()) || b.TBL_CUSTOMER.MIDDLENAME.Contains(searchParameter.Trim()) || b.TBL_CUSTOMER.LASTNAME.Contains(searchParameter.Trim())
                                            //  || a.CONDITION.Contains(searchParameter.Trim())
                                            //  || p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME.Contains(searchParameter.Trim())
                                            //  || searchParameter == "" || searchParameter == null)

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




                                            }).ToList().Select(x =>
                                            {
                                                //var buDescription = waivedConditions.Where(f => f.staffCode == x.staffCode).Select(f => f.region).FirstOrDefault();
                                                //if (buDescription != null)
                                                //{
                                                //    x.buDescription = buDescription;
                                                //}
                                                //else if (buDescription == null)
                                                //{
                                                x.buDescription = "";
                                                //}
                                                return x;
                                            });
                    return waivedConditions.Distinct().OrderBy(u => u.waveredDate).ToList();
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
                               businessUnitId = c.BUSINESSUNTID,
                               createdBy = a.CREATEDBY,
                               name = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                               facilityProduct = e.TBL_PRODUCT.PRODUCTNAME,
                               customerCode = c.CUSTOMERCODE,
                               initialDefferalDate = a.DEFERREDDATE.Value,
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
                               reasonForDeferral = a.DEFERRALREASON,
                               finalApprovalDate = a.DEFEREDDATEONFINALAPPROVAL,
                               perfectionRelated = (from x in context.TBL_LOAN_APPLICATION_COLLATERL join t in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals t.COLLATERALCUSTOMERID join y in context.TBL_COLLATERAL_TYPE on t.COLLATERALTYPEID equals y.COLLATERALTYPEID where x.LOANAPPLICATIONDETAILID == e.LOANAPPLICATIONDETAILID select y.COLLATERALTYPEID).FirstOrDefault() == (int)CollateralTypeEnum.Property ? "Yes" : "No",
                               currency = (from x in context.TBL_LOAN_APPLICATION_COLLATERL join t in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals t.COLLATERALCUSTOMERID join y in context.TBL_CURRENCY on t.CURRENCYID equals y.CURRENCYID where x.LOANAPPLICATIONDETAILID == e.LOANAPPLICATIONDETAILID select y.CURRENCYCODE).FirstOrDefault(),
                               accountOfficer = context.TBL_STAFF.Where(s => s.STAFFID == a.CREATEDBY).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault(),
                           };

                foreach (var k in data)
                {
                    var ao = context.TBL_STAFF.Find(k.createdBy);
                    if (ao != null && ao.SUPERVISOR_STAFFID != null)
                    {
                        k.relationshipManager = context.TBL_STAFF.Where(o => o.STAFFID == ao.SUPERVISOR_STAFFID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault();
                        var rm = context.TBL_STAFF.Where(o => o.STAFFID == ao.SUPERVISOR_STAFFID).FirstOrDefault();

                        if (rm != null && rm.SUPERVISOR_STAFFID != null)
                        {
                            var zh = context.TBL_STAFF.Where(o => o.STAFFID == rm.SUPERVISOR_STAFFID).FirstOrDefault();
                            if (zh != null && zh.SUPERVISOR_STAFFID != null)
                            {
                                var gh = context.TBL_STAFF.Where(o => o.STAFFID == zh.SUPERVISOR_STAFFID).FirstOrDefault();
                                k.groupHead = gh.FIRSTNAME + " " + gh.LASTNAME + " " + gh.MIDDLENAME;
                            }
                        }
                    }

                    k.businessUnit = context.TBL_PROFILE_BUSINESS_UNIT.Where(f => f.BUSINESSUNITID == k.businessUnitId).Select(f => f.BUSINESSUNITNAME + " " + f.BUSINESSUNITSHORTCODE).FirstOrDefault();
                }

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
                var data = from d in context.TBL_COLLATERAL_CUSTOMER //context.TBL_LOAN_COLLATERAL_MAPPING
                           join c in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on c.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                           //join d in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals d.COLLATERALCUSTOMERID
                           where d.COLLATERALCODE == collateralCode
                           orderby d.DATETIMECREATED descending
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
                               loanRefrenceNumber = "test", //l.LOANREFERENCENUMBER,
                           };

                //var data1 = from a in context.TBL_LOAN_COLLATERAL_MAPPING
                //           join l in context.TBL_LOAN_CONTINGENT on a.LOANID equals l.CONTINGENTLOANID
                //           join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                //           join c in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                //           join d in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals d.COLLATERALCUSTOMERID
                //           where d.COLLATERALCODE == collateralCode
                //           orderby a.DATETIMECREATED descending
                //           select new CollateralEstimatedViewModel()
                //           {
                //               firstName = b.TBL_CUSTOMER.FIRSTNAME,
                //               lastName = b.TBL_CUSTOMER.LASTNAME,
                //               middleName = b.TBL_CUSTOMER.MIDDLENAME,
                //               facilityAmount = b.APPROVEDAMOUNT,
                //               companyName = b.TBL_CUSTOMER.TBL_COMPANY.NAME,
                //               customerId = b.CUSTOMERID,
                //               facilityName = b.TBL_PRODUCT.PRODUCTNAME,
                //               collateralType = d.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                //               collateralDetail = d.TBL_COLLATERAL_TYPE.DETAILS,
                //               collateralCode = d.COLLATERALCODE,
                //               collateralValue = d.COLLATERALVALUE,
                //               hairCut = d.HAIRCUT,
                //               loanRefrenceNumber = l.LOANREFERENCENUMBER,
                //           };

                //var data2 = from a in context.TBL_LOAN_COLLATERAL_MAPPING
                //            join l in context.TBL_LOAN_REVOLVING on a.LOANID equals l.REVOLVINGLOANID
                //            join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                //            join c in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                //            join d in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals d.COLLATERALCUSTOMERID
                //            where d.COLLATERALCODE == collateralCode
                //            orderby a.DATETIMECREATED descending
                //            select new CollateralEstimatedViewModel()
                //            {
                //                firstName = b.TBL_CUSTOMER.FIRSTNAME,
                //                lastName = b.TBL_CUSTOMER.LASTNAME,
                //                middleName = b.TBL_CUSTOMER.MIDDLENAME,
                //                facilityAmount = b.APPROVEDAMOUNT,
                //                companyName = b.TBL_CUSTOMER.TBL_COMPANY.NAME,
                //                customerId = b.CUSTOMERID,
                //                facilityName = b.TBL_PRODUCT.PRODUCTNAME,
                //                collateralType = d.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                //                collateralDetail = d.TBL_COLLATERAL_TYPE.DETAILS,
                //                collateralCode = d.COLLATERALCODE,
                //                collateralValue = d.COLLATERALVALUE,
                //                hairCut = d.HAIRCUT,
                //                loanRefrenceNumber = l.LOANREFERENCENUMBER,
                //            };

                //return data.Union(data1).Union(data2).ToList();
                return data.ToList();
            }
        }

        public IList<FCYScheuledLoanViewModel> FCYScheuledLoan(int companyId, int loanId)
        {

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var currentDate = context.TBL_FINANCECURRENTDATE.Where(x => x.COMPANYID == companyId).Select(d => d.CURRENTDATE).FirstOrDefault();
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
                                   loanGlBalance = a.PRINCIPALAMOUNT,
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
                                   tenorToDate = (int)DbFunctions.DiffDays(a.EFFECTIVEDATE, currentDate),
                                   tenorToMaturity = (int)DbFunctions.DiffDays(currentDate, a.MATURITYDATE),
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

                var allLien = (from a in context.TBL_CASA_LIEN

                               join l in context.TBL_LOAN on a.SOURCEREFERENCENUMBER equals l.LOANREFERENCENUMBER
                               join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
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
                                   customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,

                               }).ToList();


                var placeLien = allLien.Where(u => u.lienAmount >= 0);
                var releaseLien = allLien.Where(x => x.lienAmount < 0);
                var getLienValue = (from b in placeLien
                                    join c in releaseLien on b.lienId equals c.lienId
                                    orderby b.lienReferenceNumber, b.sourceReferenceNumber

                                    select new CasaLienViewModel
                                    {
                                        dateLienRemoved = c.dateTimeCreated,


                                    }).ToList();













                return allLien;


            }

        }

        public IList<LoanViewModel> GetStakeHolderOnExperationOfFTP(short? branchId, string customerName, DateTime maturityDate, string searchParameter)
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
                                && (rv.INTERESTRATE.ToString().Contains(searchParameter.Trim()) || rv.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME.Contains(searchParameter.Trim()) || cs.CUSTOMERCODE.Contains(searchParameter.Trim())
                                               || s.BRANCHID.ToString().Contains(searchParameter.Trim())
                                               || br.BRANCHNAME.Contains(searchParameter.Trim())
                                               || rv.TBL_PRODUCT.PRODUCTNAME.Contains(searchParameter.Trim())
                                               || rv.TBL_CURRENCY.CURRENCYNAME.Contains(searchParameter.Trim())
                                               || s.AVAILABLEBALANCE.ToString().Contains(searchParameter.Trim())
                                               || s.PRODUCTACCOUNTNUMBER.Contains(searchParameter.Trim())
                                               || cs.CUSTOMERID.ToString().Contains(searchParameter.Trim())
                                               || s.TENOR.Value.ToString().Contains(searchParameter.Trim())
                                               || rv.OVERDRAFTLIMIT.ToString().Contains(searchParameter.Trim())
                                               || p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME.Contains(searchParameter.Trim())
                                               || s.INTERESTRATE.Value.ToString().Contains(searchParameter.Trim())
                                               || rv.EXCHANGERATE.ToString().Contains(searchParameter.Trim())
                                               || cs.CUSTOMERCODE.Contains(searchParameter.Trim())
                                               || context.TBL_STAFF.Where(o => o.STAFFID == s.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault().Contains(searchParameter.Trim())
                                               || searchParameter == "" || searchParameter == null)
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
                                    teno = s.TENOR.Value,
                                    effectiveDate = rv.EFFECTIVEDATE,
                                    facilityLimit = rv.OVERDRAFTLIMIT,
                                    businessUnit = p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,

                                    valueDate = rv.EFFECTIVEDATE,
                                    tenorToMaturity = (int)DbFunctions.DiffDays(rv.EFFECTIVEDATE, DateTime.Now),
                                    //facilityLimit = s.PRINCIPALAMOUNT,
                                    facilityRate = s.INTERESTRATE.Value,
                                    exchangeRate = rv.EXCHANGERATE,
                                    //tenorDays = a.MATURITYDATE.Subtract(a.EFFECTIVEDATE)
                                    tenorToDate = (int)DbFunctions.DiffDays(rv.MATURITYDATE, DateTime.Now),
                                    customerCode = cs.CUSTOMERCODE,
                                    nameOfRM = context.TBL_STAFF.Where(o => o.STAFFID == s.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),

                                }).ToList().Select(x =>
                                {
                                    if (x.teno >= 0 && x.teno <= 30)
                                    {
                                        x.zeroToThirtyDays = x.teno;
                                    }

                                    else if (x.teno >= 31 && x.teno <= 90)
                                    {
                                        x.ThirtyOneToNinety = x.teno;
                                    }
                                    else if (x.teno >= 31 && x.teno <= 90)
                                    {
                                        x.ninetyOneToOneEightyDays = x.teno;
                                    }

                                    else if (x.teno >= 181 && x.teno <= 366)
                                    {
                                        x.OneEightyDaysToThreeSixtyDays = x.teno;
                                    }
                                    else if (x.teno >= 365 && x.teno <= 1095)
                                    {
                                        x.overOneToThreeYears = x.teno;
                                    }

                                    else if (x.teno >= 1095)
                                    {
                                        x.overThreeYears = x.teno;
                                    }
                                    return x;
                                });


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

                //var loansWithCollateral = (from f in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                //                           //join m in context.TBL_LOAN_COLLATERAL_MAPPING on f.COLLATERALCUSTOMERID equals m.COLLATERALCUSTOMERID
                //                           where f.PERFECTIONSTATUSID == (int)(CollateralPerfectionStatusEnum.Stalled)
                //                           //select m.LOANID
                //                           );

                var reportData = (
                                  from l in context.TBL_LOAN
                                  join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                  join ll in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ll.LOANAPPLICATIONDETAILID
                                  join cm in context.TBL_LOAN_APPLICATION_COLLATERL on ll.LOANAPPLICATIONID equals cm.LOANAPPLICATIONID
                                  join cc in context.TBL_COLLATERAL_CUSTOMER on cm.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                                  where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                  && l.COMPANYID == 1
                                  //&& loansWithCollateral.Contains(l.TERMLOANID)
                                  && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                  orderby l.DATETIMECREATED descending
                                  select new StalledPerfectionViewModel
                                  {
                                      loanId = l.TERMLOANID,
                                      customerName = c.FIRSTNAME + " " + " " + c.MIDDLENAME + " " + " " + c.LASTNAME,
                                      outstandingBalance = l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL,
                                      startDate = startDate,
                                      endDate = endDate,
                                      loanRefno = l.LOANREFERENCENUMBER,
                                      collateralCode = cc.COLLATERALCODE,
                                      collateralSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(x => x.COLLATERALSUBTYPEID == cc.COLLATERALSUBTYPEID).Select(x => x.COLLATERALSUBTYPENAME).FirstOrDefault(),
                                      perfectionDate = cm.DATETIMECREATED,
                                      outstandingInterest = l.OUTSTANDINGINTEREST + l.PASTDUEINTEREST
                                  }).ToList().Select(x =>
                                  {
                                      x.collateralType = LoanCollateralType(x.loanId, LoanSystemTypeEnum.TermDisbursedFacility);
                                      x.reasonsforStalledPerfection = LoanCollateralPerfectionReasons(x.loanId, LoanSystemTypeEnum.TermDisbursedFacility);
                                      return x;
                                  }).ToList();

                return reportData;
            }




        }

        public List<CollateralPerfectionyettoCommenceViewModel> CollateralPerfectionYetToCommence(DateTime startDate, DateTime endDate, int companyid)
        {

            //List<SubHead> stagMis = new List<SubHead>();
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                //stagMis = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB }).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    //var loansWithCollateral = (from f in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                    //                           join lc in context.TBL_LOAN_COLLATERAL_MAPPING on f.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                    //                           where f.PERFECTIONSTATUSID == (int)(CollateralPerfectionStatusEnum.NotPerfected)
                    //                           select lc.LOANID);

                    var reportData = (
                                      from l in context.TBL_LOAN
                                      join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                      join sta in context.TBL_STAFF on l.RELATIONSHIPOFFICERID equals sta.STAFFID
                                      join ll in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ll.LOANAPPLICATIONDETAILID
                                      join cm in context.TBL_LOAN_APPLICATION_COLLATERL on ll.LOANAPPLICATIONID equals cm.LOANAPPLICATIONID
                                      join cc in context.TBL_COLLATERAL_CUSTOMER on cm.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                                      where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                      && l.COMPANYID == companyid
                                      //&& loansWithCollateral.Contains(l.TERMLOANID) 
                                      && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                      orderby l.DATETIMECREATED descending
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
                                          collateralCode = cc.COLLATERALCODE,
                                          collateralSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(x => x.COLLATERALSUBTYPEID == cc.COLLATERALSUBTYPEID).Select(x => x.COLLATERALSUBTYPENAME).FirstOrDefault(),
                                          captureDate = cm.DATETIMECREATED,
                                          total = (l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL) + (l.OUTSTANDINGINTEREST + l.PASTDUEINTEREST)

                                      }).ToList().Select(x =>
                                      {
                                          // x.subHead = stagMis.Where(f => f.staffCode == x.staffCode).FirstOrDefault().subHead;
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

            //List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                //subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB }).ToList();

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
                                      orderby l.EFFECTIVEDATE descending, l.MATURITYDATE descending
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
                                          //businessGroup = subList.Where(f => f.staffCode == x.staffcode).FirstOrDefault().subHead

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
            List<SubHead> subList = new List<SubHead>();


            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION, teamUnit = sl.TEAM_UNIT, businessDevelopmentManger = sl.DIRECTORATE, deptName = sl.DEPT_NAME }).ToList();
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
                                          select new CashBacked
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
                                          }).ToList().Select(x =>
                                          {

                                              x.loanBalanceForeignCurrency = x.loanBalanceForeignCurrency * (decimal)x.exchangeRate;
                                              var checkForGroupHead = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.subHead).FirstOrDefault();

                                              if (checkForGroupHead == null)
                                              {
                                                  x.groupDescription = "";
                                              }
                                              else if (checkForGroupHead != null)
                                              {
                                                  x.groupDescription = checkForGroupHead;
                                              }
                                              var checkForTeamDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.teamUnit).FirstOrDefault();
                                              if (checkForTeamDescription == null)
                                              {
                                                  x.teamDescription = "";
                                              }
                                              else if (checkForTeamDescription != null)
                                              {
                                                  x.teamDescription = checkForTeamDescription;
                                              }
                                              var checkForDeskDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.deptName).FirstOrDefault();
                                              if (checkForDeskDescription == null)
                                              {
                                                  x.deskDescription = "";
                                              }
                                              else if (checkForDeskDescription != null)
                                              {
                                                  x.deskDescription = checkForDeskDescription;
                                              }


                                              var checkForBuDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.region).FirstOrDefault();
                                              if (checkForBuDescription == null)
                                              {
                                                  x.buDescription = "";
                                              }
                                              else if (checkForBuDescription != null)
                                              {
                                                  x.buDescription = checkForBuDescription;
                                              }

                                              return x;

                                          }).ToList();



                    return cashbackedData;

                }

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
                                  //join cm in context.TBL_LOAN_COLLATERAL_MAPPING on l.TERMLOANID equals cm.LOANID
                                  //join ccust in context.TBL_COLLATERAL_CUSTOMER on cm.COLLATERALCUSTOMERID equals ccust.COLLATERALCUSTOMERID
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
                                       //businessDevelopmentManager = "",





                                   }).ToList();

                return ageAnalysis;

            }
        }

        public List<CreditScheduleViewModel> CreditScheduleReport(DateTime startDate, DateTime endDate, int companyid)
        {
            var applicationDate = DateTime.Now;
            //List<SubHead> subList = new List<SubHead>();         
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                //subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION, teamUnit = sl.TEAM_UNIT, businessDevelopmentManger = sl.DIRECTORATE, deptName = sl.DEPT_NAME }).ToList();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var creditSchedule = (

                                          from a in context.TBL_LOAN_COLLATERAL_MAPPING
                                          join l in context.TBL_LOAN on a.LOANID equals l.TERMLOANID
                                          join ccu in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals ccu.COLLATERALCUSTOMERID
                                          join st in context.TBL_STAFF on l.CREATEDBY equals st.STAFFID
                                          join cu in context.TBL_CUSTOMER on ccu.CUSTOMERID equals cu.CUSTOMERID

                                          join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                          join cas in context.TBL_COLLATERAL_CASA on a.COLLATERALCUSTOMERID equals cas.COLLATERALCUSTOMERID
                                          join lpd in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals lpd.LOANAPPLICATIONDETAILID

                                          join c in context.TBL_SUB_SECTOR on l.SUBSECTORID equals c.SUBSECTORID
                                          join s in context.TBL_SECTOR on c.SECTORID equals s.SECTORID
                                          join cgm in context.TBL_CUSTOMER_GROUP_MAPPING on ccu.CUSTOMERID equals cgm.CUSTOMERID
                                          join cg in context.TBL_CUSTOMER_GROUP on cgm.CUSTOMERGROUPID equals cg.CUSTOMERGROUPID

                                          join f in context.TBL_FREQUENCY_TYPE on l.INTERESTFREQUENCYTYPEID equals f.FREQUENCYTYPEID
                                          join fs in context.TBL_FREQUENCY_TYPE on l.PRINCIPALFREQUENCYTYPEID equals fs.FREQUENCYTYPEID
                                          join pg in context.TBL_LOAN_PRUDENTIALGUIDELINE on l.USER_PRUDENTIAL_GUIDE_STATUSID equals pg.PRUDENTIALGUIDELINESTATUSID
                                          join p in context.TBL_PRODUCT on l.PRODUCTID equals p.PRODUCTID

                                          let glInfo = (from gl in context.TBL_CHART_OF_ACCOUNT
                                                        join cst in context.TBL_CUSTOM_CHART_OF_ACCOUNT on gl.ACCOUNTCODE equals cst.PLACEHOLDERID
                                                        join pr in context.TBL_PRODUCT on gl.GLACCOUNTID equals pr.PRINCIPALBALANCEGL
                                                        where pr.PRODUCTID == p.PRODUCTID && cst.CURRENCYCODE == cur.CURRENCYCODE
                                                        select cst.ACCOUNTID).FirstOrDefault()

                                          where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                              DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                           && cu.COMPANYID == companyid
                                          orderby a.DATETIMECREATED descending


                                          select new CreditScheduleViewModel()
                                          {
                                              accountNumber = cas.ACCOUNTNUMBER,
                                              bvn = cu.CUSTOMERBVN,
                                              customerName = cu.FIRSTNAME + " " + " " + cu.MIDDLENAME + " " + " " + cu.LASTNAME,
                                              tin = cu.TAXNUMBER,
                                              facilityType = context.TBL_PRODUCT_TYPE.Where(o => o.PRODUCTTYPEID == lpd.TBL_PRODUCT.PRODUCTTYPEID).Select(o => o.PRODUCTTYPENAME).FirstOrDefault(),
                                              glSubHeadCode = glInfo,
                                              sector = s.NAME,
                                              subSector = c.NAME,
                                              customerId = cu.CUSTOMERID,
                                              groupOrganization = cg.GROUPNAME,
                                              dateGranted = l.EFFECTIVEDATE,
                                              lastCreditDate = DateTime.Now,
                                              expiryDate = l.MATURITYDATE,
                                              sanctionLimit = l.PRINCIPALAMOUNT,
                                              previousLimit = context.TBL_LOAN_SCHEDULE_PERIODIC.Where(x => x.PAYMENTDATE < applicationDate).OrderByDescending(x => x.PAYMENTDATE).Skip(1).Select(x => x.PERIODPRINCIPALAMOUNT).FirstOrDefault(),
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
                                              bankClassification = pg.STATUSNAME,
                                              detailsOfSecuritiesOthers = ccu.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                              collateralValue = ccu.COLLATERALVALUE,
                                              collateralStatus = ccu.APPROVALSTATUS,
                                              staffCode = st.STAFFCODE,


                                          }).ToList().Select(x =>
                                               {

                                                   //var checkForBuDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.region).FirstOrDefault();
                                                   //if (checkForBuDescription == null)
                                                   //{
                                                   //    x.businessType = "";
                                                   //}
                                                   //else if (checkForBuDescription != null)
                                                   //{
                                                   //    x.businessType = checkForBuDescription;
                                                   //}


                                                   return x;
                                               }).ToList();
                    return creditSchedule;
                }
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

                                         let glInfo = (from gl in context.TBL_CHART_OF_ACCOUNT
                                                       join cst in context.TBL_CUSTOM_CHART_OF_ACCOUNT on gl.ACCOUNTCODE equals cst.PLACEHOLDERID
                                                       join pr in context.TBL_PRODUCT on gl.GLACCOUNTID equals pr.PRINCIPALBALANCEGL
                                                       where pr.PRODUCTID == p.PRODUCTID && cst.CURRENCYCODE == cur.CURRENCYCODE
                                                       select cst.ACCOUNTID).FirstOrDefault()


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
                                             glSubHeadCode = glInfo,
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
                                             //lchgUserId = "",
                                             //lchgTime = DateTime.Now,
                                             //rcreUserid = "",
                                             //rcreTime = DateTime.Now,
                                             //entererId = 0,
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

                                             //limitLevel = "",
                                             limitAccountInterestRate = 0,
                                             //limitInterestRate = 0,
                                             //cotCode = "",
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

        public List<ImpairedWatchListViewModel> ImpairedWatchListReport(DateTime startDate, DateTime endDate, int companyId, short? branchId)
        {

            var getRunningLoan = new RunningLoan();

            var data = getRunningLoan.GetRunningLoan(startDate, endDate, companyId, branchId);



            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var impairedWatchList = (from l in context.TBL_LOAN
                                         join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                         join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                         join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                         join st in context.TBL_STAFF on l.CREATEDBY equals st.STAFFID
                                         join rm in context.TBL_STAFF on l.RELATIONSHIPMANAGERID equals rm.STAFFID
                                         join p in context.TBL_PRODUCT on l.PRODUCTID equals p.PRODUCTID
                                         join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                                         join cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID



                                         where l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                                  && l.COMPANYID == companyId
                                         orderby l.DATETIMECREATED descending
                                         select new ImpairedWatchListViewModel
                                         {

                                             clrBalance = cas.AVAILABLEBALANCE,
                                             interestOverDue = l.PASTDUEINTEREST,
                                             principalOverDue = l.PASTDUEPRINCIPAL,
                                             totalExposure = l.PASTDUEINTEREST + cas.AVAILABLEBALANCE,
                                             interestRate = l.INTERESTRATE,

                                             loanId = l.TERMLOANID,
                                             loanSystemTypeId = l.LOANSYSTEMTYPEID,

                                         }).ToList().Select(x =>
                                         {

                                             foreach (var d in data)
                                             {

                                                 x.rmCode = d.rmCode;

                                                 x.branchName = d.branchName;
                                                 x.branchCode = d.branchCode;
                                                 x.loanRefNo = d.loanRefNo;
                                                 x.customerName = d.customerName;
                                                 x.currencyType = d.currencyType;
                                                 x.schemeType = d.schemeType;
                                                 x.sanctionLimit = d.sanctionLimit;
                                                 x.customerId = d.customerId;

                                                 x.schemeCode = d.schemeCode;


                                                 x.glSubHeadCode = d.glSubHeadCode;


                                                 x.limitExpiryDate = d.limitExpiryDate;
                                                 x.pastDueDate = d.pastDueDate;

                                                 x.staffCode = d.staffCode;
                                                 x.buDescription = d.buDescription;
                                                 x.teamCode = d.teamCode;
                                                 x.deskCode = d.deskCode;
                                                 x.groupCode = d.groupCode;
                                                 x.buCode = d.buCode;
                                                 x.groupDescription = d.groupDescription;
                                                 x.teamDescription = d.teamDescription;
                                                 x.deskDescription = d.deskDescription;




                                             }





                                             return x;
                                         }).ToList();

                return impairedWatchList;
            }
        }




        public List<ExpiredViewModel> ExpiredReport(int companyid, DateTime startDate, DateTime endDate)
        {
            List<SubHead> subList = new List<SubHead>();

            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION, teamUnit = sl.TEAM_UNIT, businessDevelopmentManger = sl.DIRECTORATE, deptName = sl.DEPT_NAME }).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var expiredList = (from l in context.TBL_LOAN
                                       join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                       join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                       join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                       join cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                       join rm in context.TBL_STAFF on l.RELATIONSHIPMANAGERID equals rm.STAFFID
                                       join st in context.TBL_STAFF on l.CREATEDBY equals st.STAFFID
                                       join p in context.TBL_PRODUCT on l.PRODUCTID equals p.PRODUCTID
                                       join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                                       join pg in context.TBL_LOAN_PRUDENTIALGUIDELINE on l.USER_PRUDENTIAL_GUIDE_STATUSID equals pg.PRUDENTIALGUIDELINESTATUSID
                                       join pgt in context.TBL_LOAN_PRUDENT_GUIDE_TYPE on pg.PRUDENTIALGUIDELINETYPEID equals pgt.PRUDENTIALGUIDELINETYPEID

                                       let glInfo = (from gl in context.TBL_CHART_OF_ACCOUNT
                                                     join cst in context.TBL_CUSTOM_CHART_OF_ACCOUNT on gl.ACCOUNTCODE equals cst.PLACEHOLDERID
                                                     join pr in context.TBL_PRODUCT on gl.GLACCOUNTID equals pr.PRINCIPALBALANCEGL
                                                     where pr.PRODUCTID == p.PRODUCTID && cst.CURRENCYCODE == cur.CURRENCYCODE
                                                     select cst.ACCOUNTID).FirstOrDefault()


                                       where
                                           DbFunctions.TruncateTime(l.MATURITYDATE) >= DbFunctions.TruncateTime(startDate)
                                           && DbFunctions.TruncateTime(l.MATURITYDATE) <= DbFunctions.TruncateTime(endDate)
                                           && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                           && l.COMPANYID == companyid
                                       orderby l.MATURITYDATE descending



                                       select new ExpiredViewModel
                                       {


                                           branchCode = b.BRANCHCODE,
                                           branchName = b.BRANCHNAME,
                                           customerId = l.CUSTOMERID,
                                           currencyType = cur.CURRENCYNAME,
                                           customerName = cus.FIRSTNAME + " " + " " + cus.MIDDLENAME + " " + " " + cus.LASTNAME,
                                           rmName = rm.FIRSTNAME + " " + " " + rm.MIDDLENAME + " " + " " + rm.LASTNAME,
                                           rmCode = rm.STAFFCODE,
                                           buCode = "",
                                           account = l.LOANREFERENCENUMBER,
                                           accountName = cus.FIRSTNAME + " " + " " + cus.MIDDLENAME + " " + " " + cus.LASTNAME,
                                           schemeType = pt.PRODUCTTYPENAME,
                                           sanctionLimit = l.PRINCIPALAMOUNT,
                                           schemeCode = p.PRODUCTCODE,
                                           userClassification = pgt.PRUDENTIALGUIDELINETYPENAME,
                                           transactionDateBalance = l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL,
                                           subClassification = pg.STATUSNAME,
                                           pastDueDate = (l.PASTDUEDATE.Value == null ? default(DateTime) : l.PASTDUEDATE.Value),
                                           classificationDate = DateTime.Now,
                                           expiryDate = l.MATURITYDATE,
                                           staffCode = st.STAFFCODE,
                                           teamCode = "",
                                           deskCode = "",
                                           groupCode = "",
                                           glSubHeadCode = glInfo,

                                           //expiryDate = l.MATURITYDATE,


                                       }).ToList().Select(x =>
                                       {
                                           var checkForGroupHead = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.subHead).FirstOrDefault();

                                           if (checkForGroupHead == null)
                                           {
                                               x.groupDescription = "";
                                           }
                                           else if (checkForGroupHead != null)
                                           {
                                               x.groupDescription = checkForGroupHead;
                                           }
                                           var checkForTeamDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.teamUnit).FirstOrDefault();
                                           if (checkForTeamDescription == null)
                                           {
                                               x.teamDescription = "";
                                           }
                                           else if (checkForTeamDescription != null)
                                           {
                                               x.teamDescription = checkForTeamDescription;
                                           }
                                           var checkForDeskDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.deptName).FirstOrDefault();
                                           if (checkForDeskDescription == null)
                                           {
                                               x.deskDescription = "";
                                           }
                                           else if (checkForDeskDescription != null)
                                           {
                                               x.deskDescription = checkForDeskDescription;
                                           }

                                           var checkForBuDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.region).FirstOrDefault();
                                           if (checkForBuDescription == null)
                                           {
                                               x.buDescription = "";
                                           }
                                           else if (checkForBuDescription != null)
                                           {
                                               x.buDescription = checkForBuDescription;
                                           }



                                           return x;
                                       }).ToList();

                    return expiredList;
                }
            }

        }


        public List<ExcessViewModel> ExcessReport(DateTime startDate, DateTime endDate, int companyId, short? branchId)
        {
            List<SubHead> subList = new List<SubHead>();


            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION, teamUnit = sl.TEAM_UNIT, businessDevelopmentManger = sl.DIRECTORATE, deptName = sl.DEPT_NAME }).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var excessList = (from l in context.TBL_LOAN_REVOLVING
                                      join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                      join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                      join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                      join cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID
                                      //join ld in context.TBL_LOAN on l.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                      join rm in context.TBL_STAFF on l.RELATIONSHIPMANAGERID equals rm.STAFFID
                                      join st in context.TBL_STAFF on l.CREATEDBY equals st.STAFFID
                                      join p in context.TBL_PRODUCT on l.PRODUCTID equals p.PRODUCTID
                                      join pt in context.TBL_PRODUCT_TYPE on p.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                                      join pg in context.TBL_LOAN_PRUDENTIALGUIDELINE on l.USER_PRUDENTIAL_GUIDE_STATUSID equals pg.PRUDENTIALGUIDELINESTATUSID
                                      join pgt in context.TBL_LOAN_PRUDENT_GUIDE_TYPE on pg.PRUDENTIALGUIDELINETYPEID equals pgt.PRUDENTIALGUIDELINETYPEID
                                      where cas.AVAILABLEBALANCE < 0
                                      && (Math.Abs(cas.AVAILABLEBALANCE) > l.OVERDRAFTLIMIT)
                                      &&
                                      l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                      && DbFunctions.TruncateTime(l.MATURITYDATE) >= DbFunctions.TruncateTime(startDate)
                                               && DbFunctions.TruncateTime(l.MATURITYDATE) <= DbFunctions.TruncateTime(endDate)
                                               && (b.BRANCHID == branchId || branchId == null || branchId == 0)
                                                && l.COMPANYID == companyId
                                      orderby l.DATETIMECREATED descending
                                      select new ExcessViewModel
                                      {
                                          branchCode = b.BRANCHCODE,
                                          branchName = b.BRANCHNAME,
                                          customerId = l.CUSTOMERID,
                                          currencyType = cur.CURRENCYNAME,
                                          customerName = cus.FIRSTNAME + " " + " " + cus.MIDDLENAME + " " + " " + cus.LASTNAME,
                                          rmCode = rm.STAFFCODE,
                                          rmName = rm.FIRSTNAME + " " + " " + rm.MIDDLENAME + " " + " " + rm.LASTNAME,
                                          excess = cas.AVAILABLEBALANCE - l.OVERDRAFTLIMIT,
                                          endDate = DateTime.Now,
                                          staffCode = st.STAFFCODE,
                                          buCode = "",

                                          account = l.LOANREFERENCENUMBER,
                                          accountName = cus.FIRSTNAME + " " + " " + cus.MIDDLENAME + " " + " " + cus.LASTNAME,
                                          schemeType = pt.PRODUCTTYPENAME,
                                          sanctionLimit = l.OVERDRAFTLIMIT,
                                          schemeCode = p.PRODUCTCODE,
                                          userClassification = pgt.PRUDENTIALGUIDELINETYPENAME,
                                          transactionDateBalance = cas.AVAILABLEBALANCE,
                                          subClassification = pg.STATUSNAME,
                                          //pastDueDate = (ld.PASTDUEDATE.Value == null ? default(DateTime) : ld.PASTDUEDATE.Value),
                                          classificationDate = DateTime.Now,
                                          expiryDate = l.MATURITYDATE,


                                      }).ToList().Select(x =>
                                      {

                                          var checkForGroupHead = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.subHead).FirstOrDefault();

                                          if (checkForGroupHead == null)
                                          {
                                              x.groupDescription = "";
                                          }
                                          else if (checkForGroupHead != null)
                                          {
                                              x.groupDescription = checkForGroupHead;
                                          }
                                          var checkForTeamDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.teamUnit).FirstOrDefault();
                                          if (checkForTeamDescription == null)
                                          {
                                              x.teamDescription = "";
                                          }
                                          else if (checkForTeamDescription != null)
                                          {
                                              x.teamDescription = checkForTeamDescription;
                                          }
                                          var checkForDeskDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.deptName).FirstOrDefault();
                                          if (checkForDeskDescription == null)
                                          {
                                              x.deskDescription = "";
                                          }
                                          else if (checkForDeskDescription != null)
                                          {
                                              x.deskDescription = checkForDeskDescription;
                                          }


                                          var checkForBuDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.region).FirstOrDefault();
                                          if (checkForBuDescription == null)
                                          {
                                              x.buDescription = "";
                                          }
                                          else if (checkForBuDescription != null)
                                          {
                                              x.buDescription = checkForBuDescription;
                                          }




                                          return x;
                                      }).ToList();

                    return excessList;
                }
            }
        }
        public List<InsuranceViewModel> InsuranceReport(int companyId)
        {

            var getRunningLoan = new RunningLoan();

            //var data = getRunningLoan.GetRunningLoan(companyId);

            var data = getRunningLoan.GetRunningInsuranceLoan(companyId);



            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var insuranceList = (from ccu in context.TBL_COLLATERAL_CUSTOMER
                                     join cu in context.TBL_CUSTOMER on ccu.CUSTOMERID equals cu.CUSTOMERID
                                     join br in context.TBL_BRANCH on cu.BRANCHID equals br.BRANCHID
                                     join ccp in context.TBL_COLLATERAL_ITEM_POLICY on ccu.COLLATERALCUSTOMERID equals ccp.COLLATERALCUSTOMERID
                                     //join ip in context.TBL_COLLATERAL_IMMOVE_PROPERTY on ccu.COLLATERALCUSTOMERID equals ip.COLLATERALCUSTOMERID
                                     //join pe in context.TBL_COLLATERAL_PERFECTN_STAT on ip.PERFECTIONSTATUSID equals pe.PERFECTIONSTATUSID
                                     join ct in context.TBL_COLLATERAL_TYPE on ccu.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                                     //join lc in context.TBL_LOAN_COLLATERAL_MAPPING on ccu.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                                     //join l in context.TBL_LOAN on lc.LOANID equals l.TERMLOANID
                                     //join st in context.TBL_STAFF on l.CREATEDBY equals st.STAFFID

                                     where ccu.COMPANYID == companyId
                                     //where l.LOANSTATUSID == (short)LoanStatusEnum.Active

                                     //&& ccu.COMPANYID == companyId
                                     orderby ccp.STARTDATE descending

                                     select new InsuranceViewModel
                                     {

                                         branchId = br.BRANCHID,
                                         customerCode = ccu.COLLATERALCODE,
                                         collateralType = ct.COLLATERALTYPENAME + ":" + ccu.COLLATERALCODE,
                                         //  perfectionStatus = pe.PERFECTIONSTATUSNAME,
                                         // insuranceType = ccp.INSURANCETYPEID,
                                         insurancePolicyNumber = ccp.POLICYREFERENCENUMBER,
                                         //  insuranceCompanyName = ccp.INSURANCECOMPANYNAME,
                                         insuredValue = ccp.SUMINSURED,
                                         //account = l.LOANREFERENCENUMBER,
                                         accountName = cu.FIRSTNAME + " " + " " + cu.MIDDLENAME + " " + " " + cu.LASTNAME,

                                         startDate = ccp.STARTDATE,

                                         // maturityDate = l.MATURITYDATE,

                                         //days = (int)DbFunctions.DiffDays((DateTime?)l.MATURITYDATE, (DateTime?)l.EFFECTIVEDATE),
                                         //workFlowID = "",
                                         //  status = l.TBL_LOAN_STATUS.ACCOUNTSTATUS,
                                         //staffCode = st.STAFFCODE,
                                         //rmName = st.FIRSTNAME + " " + " " + st.MIDDLENAME + " " + " " + st.LASTNAME,
                                         customerId = ccu.COLLATERALCUSTOMERID,
                                         //sanctionLimit = l.PRINCIPALAMOUNT,


                                     }).ToList().Select(x =>
                                     {

                                         foreach (var d in data)
                                         {




                                             var checkForRemarks = context.TBL_COLLATERAL_POLICY.Where(u => u.COLLATERALCUSTOMERID == x.customerId).Select(u => u.REMARK).FirstOrDefault();

                                             if (checkForRemarks == null)
                                             {
                                                 x.remarks = "";
                                             }
                                             else if (checkForRemarks != null)
                                             {
                                                 x.remarks = checkForRemarks;
                                             }


                                             var checkForPremiumPaid = context.TBL_COLLATERAL_POLICY.Where(u => u.COLLATERALCUSTOMERID == x.customerId).Select(u => u.PREMIUMAMOUNT).FirstOrDefault();

                                             if (checkForPremiumPaid == 0)
                                             {
                                                 x.premiumPaid = 0;
                                             }
                                             else if (checkForPremiumPaid != 0)
                                             {
                                                 x.premiumPaid = checkForPremiumPaid;
                                             }

                                         }

                                         return x;
                                     }).ToList();




                return insuranceList;




            }
        }

        public List<DisbursalCreditTurnoverViewModel> DisburseCreditTurnover(DateTime startDate, DateTime endDate, int companyid)
        {
            DateTime dateTime = DateTime.Now;
            //List<DisbursalCreditTurnoverViewModel> disburseCreditList;
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var disburseCreditList = (from l in context.TBL_LOAN
                                          join cu in context.TBL_CUSTOMER on l.CUSTOMERID equals cu.CUSTOMERID
                                          join br in context.TBL_BRANCH on l.BRANCHID equals br.BRANCHID
                                          join cas in context.TBL_CASA on l.CASAACCOUNTID equals cas.CASAACCOUNTID
                                          join lpd in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals lpd.LOANAPPLICATIONDETAILID
                                          join cusmap in context.TBL_CUSTOMER_GROUP_MAPPING on cu.CUSTOMERID equals cusmap.CUSTOMERID
                                          join custgr in context.TBL_CUSTOMER_GROUP on cusmap.CUSTOMERGROUPID equals custgr.CUSTOMERGROUPID
                                          join ld in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ld.LOANAPPLICATIONDETAILID
                                          join lpg in context.TBL_LOAN_PRUDENTIALGUIDELINE on l.USER_PRUDENTIAL_GUIDE_STATUSID equals lpg.PRUDENTIALGUIDELINESTATUSID


                                          where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                  DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                               && l.COMPANYID == companyid
                                          orderby l.DATETIMECREATED descending


                                          select new DisbursalCreditTurnoverViewModel()
                                          {
                                              bdo = "",
                                              customerName = cu.FIRSTNAME + " " + " " + cu.MIDDLENAME + " " + " " + cu.LASTNAME,
                                              branches = br.BRANCHNAME,
                                              groupName = custgr.GROUPNAME,
                                              operativeAcct = cas.PRODUCTACCOUNTNUMBER,
                                              dateDisbursed = l.DISBURSEDATE,
                                              expiryDate = l.MATURITYDATE,
                                              daysPastDue = (int)DbFunctions.DiffDays((l.PASTDUEDATE.Value == null ? default(DateTime) : l.PASTDUEDATE.Value), dateTime),
                                              status = lpg.STATUSNAME,
                                              currentBalance = l.PASTDUEPRINCIPAL + l.PRINCIPALAMOUNT,
                                              excessAboveLimit = "",
                                              totalExposure = l.PASTDUEINTEREST + cas.AVAILABLEBALANCE,
                                              crTurnover = "",
                                              custId = l.CUSTOMERID,
                                              sanctionLimit = lpd.APPROVEDAMOUNT,
                                              schemeCode = l.PRODUCTID.ToString()

                                          }).ToList();

                //return disburseCreditList.ToList();
                return disburseCreditList;
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
        private IEnumerable<LoanViewModel> GetLoanFacility(int companyId, int? staffId, string searchInfo, DateTime startDate, DateTime endDate)
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
                            loanStatus = context.TBL_LOAN_STATUS.Where(o => o.LOANSTATUSID == ln.LOANSTATUSID).Select(o => o.ACCOUNTSTATUS).FirstOrDefault(),
                        }).ToList();


            return data;
        }

        public IList<AnalystReportViewModel> GetAnalystReport(DateTime startDate, DateTime endDate, short? branchId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var condition = context.TBL_LOAN_CONDITION_PRECEDENT.ToList();

                var data = from a in context.TBL_LOAN_APPLICATION
                           join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                           join c in context.TBL_CUSTOMER on b.CUSTOMERID equals c.CUSTOMERID
                           join d in context.TBL_LOAN_CONDITION_PRECEDENT.Select(p => new { p.CREATEDBY, p.LOANAPPLICATIONDETAILID }).Distinct() on b.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                           join e in context.TBL_BRANCH on a.BRANCHID equals e.BRANCHID
                           select new AnalystReportViewModel()
                           {
                               customer_name = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                               applicationreferencenumber = a.APPLICATIONREFERENCENUMBER,
                               proposedamount = b.PROPOSEDAMOUNT,
                               proposedinterestrate = b.PROPOSEDINTERESTRATE,
                               loanpurpose = b.LOANPURPOSE,
                               analystname = context.TBL_STAFF.Where(z => z.STAFFID == d.CREATEDBY).Select(c => new { FULLNAME = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME }).FirstOrDefault().FULLNAME, //+ context.TBL_STAFF.Where(z => z.STAFFID == d.CREATEDBY).FirstOrDefault().MIDDLENAME + " " + context.TBL_STAFF.Where(z => z.STAFFID == d.CREATEDBY).FirstOrDefault().MIDDLENAME,
                               branch = e.BRANCHCODE + " ~ " + e.BRANCHNAME
                           };
                return data.ToList();
            }
        }

        public List<RuniningLoanViewModel> RunningLoanReport(DateTime startDate, DateTime endDate, int companyId, short? branchId)
        {
            //List<SubHead> subList = new List<SubHead>();
            //subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION, teamUnit = sl.TEAM_UNIT, businessDevelopmentManger = sl.DIRECTORATE, deptName = sl.DEPT_NAME }).ToList();
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                var getRunningLoan = new RunningLoan();
                var data = getRunningLoan.GetRunningLoan(startDate, endDate, companyId, branchId);

                var runningLoanList = (from l in context.TBL_LOAN
                                       join al in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals al.LOANAPPLICATIONDETAILID
                                       join pg in context.TBL_LOAN_PRUDENTIALGUIDELINE on l.USER_PRUDENTIAL_GUIDE_STATUSID equals pg.PRUDENTIALGUIDELINESTATUSID
                                       join pgt in context.TBL_LOAN_PRUDENT_GUIDE_TYPE on pg.PRUDENTIALGUIDELINETYPEID equals pgt.PRUDENTIALGUIDELINETYPEID
                                       join d in context.TBL_COLLATERAL_CUSTOMER on l.CUSTOMERID equals d.CUSTOMERID
                                       join su in context.TBL_SUB_SECTOR on l.SUBSECTORID equals su.SUBSECTORID
                                       join s in context.TBL_SECTOR on su.SECTORID equals s.SECTORID
                                       where l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                           && l.COMPANYID == companyId
                                       orderby l.MATURITYDATE descending
                                       select new RuniningLoanViewModel
                                       {
                                           applicationReferenceNumber = al.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                           product = context.TBL_PRODUCT.Where(x => x.PRODUCTID == al.APPROVEDPRODUCTID).Select(x => x.PRODUCTNAME).FirstOrDefault(),
                                           arrivalDate = al.DATETIMECREATED,
                                           divisionName = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == l.CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
                                           //currentLevel = "",
                                           //state = "",
                                           //lastTreatedDate 
                                           //currentlyWith
                                           //lastReviewComment
                                           receivableAmount = 0,
                                           sanctionLimitDate = l.BOOKINGDATE,
                                           securityDetails = d.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                           sector = s.NAME,
                                           insiderFlag = (context.TBL_CUSTOMER_RELATED_PARTY.Where(x => x.CUSTOMERID == l.CUSTOMERID && x.DELETED == false).Count() > 1 ? "No" : "Yes"),
                                           //bookingDate = l.BOOKINGDATE,
                                           otherIncome = 0,
                                           interestInSupense = 0,
                                           endDate = DateTime.Now,
                                           maturityDate = l.MATURITYDATE,
                                           effective = l.EFFECTIVEDATE,
                                           interestRate = l.INTERESTRATE,
                                           facilityGrantedAmount = l.PRINCIPALAMOUNT,
                                           customerId = l.CUSTOMERID,
                                           subSectorCode = su.CODE,
                                           otherCharges = 0,
                                           finalBalance = 0,
                                           lastCreditDate = DateTime.Now,
                                           lastCreditAmount = 0,
                                           fxRate = 0,
                                           subStandard = (pg.PRUDENTIALGUIDELINESTATUSID == 2 ? l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL : 0),
                                           doubtfull = (pg.PRUDENTIALGUIDELINESTATUSID == 3 ? l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL : 0),
                                           lost = (pg.PRUDENTIALGUIDELINESTATUSID == 5 ? l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL : 0),
                                           applicationDate = DateTime.Now,
                                           loanId = l.TERMLOANID,
                                           loanSytemTypeId = l.LOANSYSTEMTYPEID
                                       }).ToList().Select(x =>
                                       {
                                           foreach (var d in data)
                                           {

                                               x.rmCode = d.rmCode;
                                               x.rmName = d.rmName;
                                               x.branchName = d.branchName;
                                               x.branchCode = d.branchCode;
                                               x.loanRefNo = d.loanRefNo;
                                               x.customerName = d.customerName;
                                               x.currencyType = d.currencyType;
                                               x.transactionDateBalance = d.transactionDateBalance;
                                               x.schemeType = d.schemeType;
                                               x.schemeDescription = d.schemeDescription;
                                               x.sanctionLimit = d.sanctionLimit;
                                               x.expiryDate = d.expiryDate;
                                               x.customerId = d.customerId;
                                               x.schemeCode = d.schemeCode;
                                               x.subUserClassification = d.subUserClassification;
                                               x.userClassification = d.userClassification;
                                               x.glSubHeadCode = d.glSubHeadCode;
                                               x.classificationDate = d.classificationDate;
                                               x.limitExpiryDate = d.limitExpiryDate;
                                               x.pastDueDate = d.pastDueDate;
                                               x.staffCode = d.staffCode;
                                               x.buDescription = d.buDescription;
                                               x.teamCode = d.teamCode;
                                               x.deskCode = d.deskCode;
                                               x.groupCode = d.groupCode;
                                               x.buCode = d.buCode;
                                               x.pastDueDays = d.pastDueDays;
                                               x.groupDescription = d.groupDescription;
                                               x.teamDescription = d.teamDescription;
                                               x.deskDescription = d.deskDescription;
                                               x.businessDevelopmentManger = d.businessDevelopmentManger;
                                           }

                                           return x;
                                       }).ToList();

                //data.Where(o => o.loanId == x.loanId && x.loanSytemTypeId == x.loanSytemTypeId

                return runningLoanList;
            }

        }

        public List<Form3800BReportViewModel> Form3800BApprovedFacility(int companyId, DateTime startDate, DateTime endDate)
        {
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                var misList = (from sl in stagecontext.STG_STAFFMIS select new { sl.DIRECTORATE, sl.USERNAME, sl.GROUP_HUB, sl.DEPT_NAME }).ToList();
                IQueryable<Form3800BReportViewModel> termLoan;
                IQueryable<Form3800BReportViewModel> lmsTermLoan;
                IQueryable<Form3800BReportViewModel> lmsOD;
                IQueryable<Form3800BReportViewModel> lmsContingent;
                IQueryable<Form3800BReportViewModel> data;
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    termLoan = (from x in context.TBL_LOAN_APPLICATION_DETAIL
                                join a in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID

                                let staffCode = context.TBL_STAFF.Where(s => s.STAFFID == a.RELATIONSHIPMANAGERID).Select(s => s.STAFFCODE).FirstOrDefault()
                                //   let mis = misList.Where(o => o.USERNAME == staffCode).Select(o=>o.DEPT_NAME).FirstOrDefault()
                                where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                              DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                                              && a.COMPANYID == companyId
                                              && x.STATUSID == (int)ApprovalStatusEnum.Approved
                                select new Form3800BReportViewModel
                                {
                                    date = a.AVAILMENTDATE,
                                    operativeAccount = context.TBL_CASA.Where(o => o.CASAACCOUNTID == x.OPERATINGCASAACCOUNTID).Select(o => o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),  // x cassaaccountid
                                    businessUnit = "",
                                    businessGroup = "", //group
                                    branch = a.TBL_BRANCH.BRANCHNAME,
                                    customer = a.TBL_CUSTOMER.FIRSTNAME + "" + a.TBL_CUSTOMER.LASTNAME + "" + a.TBL_CUSTOMER.MIDDLENAME,
                                    cap = "CAP",
                                    status = "NEW APPROVAL",
                                    purpose = x.LOANPURPOSE,
                                    newApproval = x.APPROVEDAMOUNT,
                                    currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == x.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                                    staffCode = staffCode,
                                    systemDate = x.DATETIMECREATED,
                                })

                    ;
                    lmsTermLoan = (from x in context.TBL_LMSR_APPLICATION_DETAIL
                                   join a in context.TBL_LMSR_APPLICATION on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                   join l in context.TBL_LOAN on x.LOANID equals l.TERMLOANID
                                   let loanDetail = context.TBL_LOAN_APPLICATION_DETAIL.Join(
                                       context.TBL_LOAN, a => a.LOANAPPLICATIONDETAILID, b => b.LOANAPPLICATIONDETAILID, (a, b) => new { a, b }).Join(
                                       context.TBL_LMSR_APPLICATION_DETAIL, x => x.b.TERMLOANID, y => y.LOANID, (x, y) => new { x.b.RELATIONSHIPMANAGERID, x.a.LOANPURPOSE, x.a.CURRENCYID }).FirstOrDefault()
                                   let staffCode = context.TBL_STAFF.Where(s => s.STAFFID == loanDetail.RELATIONSHIPMANAGERID).Select(s => s.STAFFCODE).FirstOrDefault()
                                   where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                  DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                                                  && a.COMPANYID == companyId
                                                  && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                   select new Form3800BReportViewModel
                                   {
                                       date = a.AVAILMENTDATE,
                                       operativeAccount = context.TBL_CASA.Where(o => o.CASAACCOUNTID == l.CASAACCOUNTID).Select(o => o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),  // x cassaaccountid
                                       businessUnit = "",
                                       businessGroup = "", //group
                                       branch = a.TBL_BRANCH.BRANCHNAME,
                                       customer = a.TBL_CUSTOMER.FIRSTNAME + "" + a.TBL_CUSTOMER.LASTNAME + "" + a.TBL_CUSTOMER.MIDDLENAME,
                                       cap = "CAP",
                                       status = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(), //operationId
                                       purpose = loanDetail.LOANPURPOSE,
                                       newApproval = x.APPROVEDAMOUNT,
                                       currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == loanDetail.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                                       staffCode = staffCode,
                                       systemDate = x.DATETIMECREATED,

                                   });
                    lmsOD = (from x in context.TBL_LMSR_APPLICATION_DETAIL
                             join a in context.TBL_LMSR_APPLICATION on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                             join l in context.TBL_LOAN_REVOLVING on x.LOANID equals l.REVOLVINGLOANID
                             let loanDetail = context.TBL_LOAN_APPLICATION_DETAIL.Join(
                                 context.TBL_LOAN, a => a.LOANAPPLICATIONDETAILID, b => b.LOANAPPLICATIONDETAILID, (a, b) => new { a, b }).Join(
                                 context.TBL_LMSR_APPLICATION_DETAIL, x => x.b.TERMLOANID, y => y.LOANID, (x, y) => new { x.b.RELATIONSHIPMANAGERID, x.a.LOANPURPOSE, x.a.CURRENCYID }).FirstOrDefault()
                             let staffCode = context.TBL_STAFF.Where(s => s.STAFFID == loanDetail.RELATIONSHIPMANAGERID).Select(s => s.STAFFCODE).FirstOrDefault()
                             where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                            DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                                            && a.COMPANYID == companyId
                                            && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                             select new Form3800BReportViewModel
                             {
                                 date = a.AVAILMENTDATE,
                                 operativeAccount = context.TBL_CASA.Where(o => o.CASAACCOUNTID == l.CASAACCOUNTID).Select(o => o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),  // x cassaaccountid
                                 businessUnit = "",
                                 businessGroup = "", //group
                                 branch = a.TBL_BRANCH.BRANCHNAME,
                                 customer = a.TBL_CUSTOMER.FIRSTNAME + "" + a.TBL_CUSTOMER.LASTNAME + "" + a.TBL_CUSTOMER.MIDDLENAME,
                                 cap = "CAP",
                                 status = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(), //operationId
                                 purpose = loanDetail.LOANPURPOSE,
                                 newApproval = x.APPROVEDAMOUNT,
                                 currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == loanDetail.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                                 staffCode = staffCode,
                                 systemDate = x.DATETIMECREATED,

                             });
                    lmsContingent = (from x in context.TBL_LMSR_APPLICATION_DETAIL
                                     join a in context.TBL_LMSR_APPLICATION on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                                     join l in context.TBL_LOAN_CONTINGENT on x.LOANID equals l.CONTINGENTLOANID
                                     let loanDetail = context.TBL_LOAN_APPLICATION_DETAIL.Join(
                                         context.TBL_LOAN, a => a.LOANAPPLICATIONDETAILID, b => b.LOANAPPLICATIONDETAILID, (a, b) => new { a, b }).Join(
                                         context.TBL_LMSR_APPLICATION_DETAIL, x => x.b.TERMLOANID, y => y.LOANID, (x, y) => new { x.b.RELATIONSHIPMANAGERID, x.a.LOANPURPOSE, x.a.CURRENCYID }).FirstOrDefault()
                                     let staffCode = context.TBL_STAFF.Where(s => s.STAFFID == loanDetail.RELATIONSHIPMANAGERID).Select(s => s.STAFFCODE).FirstOrDefault()
                                     where DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                    DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                                                    && a.COMPANYID == companyId
                                                    && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                     select new Form3800BReportViewModel
                                     {
                                         date = a.AVAILMENTDATE,
                                         operativeAccount = context.TBL_CASA.Where(o => o.CASAACCOUNTID == l.CASAACCOUNTID).Select(o => o.PRODUCTACCOUNTNUMBER).FirstOrDefault(),  // x cassaaccountid
                                         businessUnit = "",
                                         businessGroup = "", //group
                                         branch = a.TBL_BRANCH.BRANCHNAME,
                                         customer = a.TBL_CUSTOMER.FIRSTNAME + "" + a.TBL_CUSTOMER.LASTNAME + "" + a.TBL_CUSTOMER.MIDDLENAME,
                                         cap = "CAP",
                                         status = context.TBL_OPERATIONS.Where(o => o.OPERATIONID == x.OPERATIONID).Select(o => o.OPERATIONNAME).FirstOrDefault(), //operationId
                                         purpose = loanDetail.LOANPURPOSE,
                                         newApproval = x.APPROVEDAMOUNT,
                                         currency = context.TBL_CURRENCY.Where(o => o.CURRENCYID == loanDetail.CURRENCYID).Select(o => o.CURRENCYNAME).FirstOrDefault(),
                                         staffCode = staffCode,
                                         systemDate = x.DATETIMECREATED,

                                     });
                    data = termLoan.Union(lmsTermLoan).Union(lmsOD).Union(lmsContingent).OrderByDescending(o => o.systemDate);
                    return data.ToList().Select(x =>
                    {
                        x.businessUnit = misList.Where(o => o.USERNAME == x.staffCode).Select(o => o.DIRECTORATE).FirstOrDefault();
                        x.businessGroup = misList.Where(o => o.USERNAME == x.staffCode).Select(o => o.GROUP_HUB).FirstOrDefault();
                        return x;
                    }).ToList();
                }
            }

        }

        public List<UnutilizedFacilityViewModel> UnutilizedFacilityReport(int companyId)
        {
            List<SubHead> subList = new List<SubHead>();


            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                subList = (from sl in stagecontext.STG_STAFFMIS select new SubHead { staffCode = sl.USERNAME, subHead = sl.GROUP_HUB, firstName = sl.FIRSTNAME, middleName = sl.MIDDLENAME, lastName = sl.LASTNAME, region = sl.REGION, teamUnit = sl.TEAM_UNIT, businessDevelopmentManger = sl.DIRECTORATE, deptName = sl.DEPT_NAME }).ToList();
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var runningLoans = ( //l in context.TBL_LOAN
                                        from ld in context.TBL_LOAN_APPLICATION_DETAIL
                                        join la in context.TBL_LOAN_APPLICATION on ld.LOANAPPLICATIONID equals la.LOANAPPLICATIONID
                                        join l in context.TBL_LOAN on ld.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID
                                        join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                        join cur in context.TBL_CURRENCY on l.CURRENCYID equals cur.CURRENCYID
                                        join cus in context.TBL_CUSTOMER on l.CUSTOMERID equals cus.CUSTOMERID

                                        //join st in context.TBL_STAFF on l.CREATEDBY equals st.STAFFID

                                        where l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                              && l.COMPANYID == companyId
                                        // group l.LOANAPPLICATIONDETAILID desc


                                        select new UnutilizedFacilityViewModel
                                        {
                                            branch = b.BRANCHNAME,
                                            customerName = cus.FIRSTNAME + " " + cus.MIDDLENAME + " " + cus.LASTNAME,
                                            loanApplicationDetailId = ld.LOANAPPLICATIONDETAILID,
                                            purpose = ld.LOANPURPOSE,
                                            reviwedDate = DateTime.Now,
                                            cap = "CAP",
                                            status = l.TBL_LOAN_STATUS.ACCOUNTSTATUS,

                                        }).ToList().Select(x =>
                                        {

                                            var checkForBuDescription = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.region).FirstOrDefault();
                                            if (checkForBuDescription == null)
                                            {
                                                x.businessUnits = "";
                                            }
                                            else if (checkForBuDescription != null)
                                            {
                                                x.businessUnits = checkForBuDescription;
                                            }


                                            var checkForGroupHead = subList.Where(u => u.staffCode == x.staffCode).Select(u => u.subHead).FirstOrDefault();

                                            if (checkForGroupHead == null)
                                            {
                                                x.group = "";
                                            }
                                            else if (checkForGroupHead != null)
                                            {
                                                x.group = checkForGroupHead;
                                            }
                                            return x;
                                        }).ToList();

                    foreach (var item in runningLoans)
                    {

                        var loans = context.TBL_LOAN.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                        var overdrafts = context.TBL_LOAN_REVOLVING.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                        var contingents = context.TBL_LOAN_CONTINGENT.Where(tl => tl.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId);
                        switch (item.productTypeId)
                        {
                            case (short)LoanProductTypeEnum.TermLoan:
                                decimal customerAvailableAmount = 0;
                                foreach (var loan in loans)
                                {
                                    if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount = customerAvailableAmount + loan.PRINCIPALAMOUNT;
                                }
                                item.undisbursedAmount = item.approvedAmount - customerAvailableAmount;
                                break;
                            case (short)LoanProductTypeEnum.CommercialLoan:
                                decimal customerAvailableAmount2 = 0;
                                foreach (var loan in loans)
                                {
                                    if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount2 = customerAvailableAmount2 + loan.PRINCIPALAMOUNT;
                                }
                                item.undisbursedAmount = item.approvedAmount - customerAvailableAmount2;
                                break;
                            case (short)LoanProductTypeEnum.SelfLiquidating:
                                decimal customerAvailableAmount3 = 0;
                                foreach (var loan in loans)
                                {
                                    if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount3 = customerAvailableAmount3 + loan.PRINCIPALAMOUNT;
                                }
                                item.undisbursedAmount = item.approvedAmount - customerAvailableAmount3;
                                break;
                            case (short)LoanProductTypeEnum.RevolvingLoan:
                                decimal overdraftBal = 0;
                                foreach (var overdraft in overdrafts)
                                {
                                    if (overdraft.OVERDRAFTLIMIT > 0) overdraftBal = overdraftBal + overdraft.OVERDRAFTLIMIT;
                                }
                                item.undisbursedAmount = item.approvedAmount - overdraftBal;
                                break;
                            case (short)LoanProductTypeEnum.ContingentLiability:
                                decimal contingentBal = 0;
                                foreach (var contingent in contingents)
                                {
                                    if (contingent.CONTINGENTAMOUNT > 0) contingentBal = contingentBal + contingent.CONTINGENTAMOUNT;
                                }
                                item.undisbursedAmount = item.approvedAmount - contingentBal;
                                break;
                            case (short)LoanProductTypeEnum.ForeignXRevolving:
                                decimal customerAvailableAmount4 = 0;
                                foreach (var loan in loans)
                                {
                                    if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount4 = customerAvailableAmount4 + loan.PRINCIPALAMOUNT;
                                }
                                item.undisbursedAmount = item.approvedAmount - customerAvailableAmount4;
                                break;
                            case (short)LoanProductTypeEnum.SyndicatedTermLoan:
                                decimal customerAvailableAmount5 = 0;
                                foreach (var loan in loans)
                                {
                                    if (loan.PRINCIPALAMOUNT > 0) customerAvailableAmount = customerAvailableAmount5 + loan.PRINCIPALAMOUNT;
                                }
                                item.undisbursedAmount = item.approvedAmount - customerAvailableAmount5;
                                break;
                        }


                        var disbursedLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == item.loanApplicationDetailId && x.ISDISBURSED == true);
                        if (disbursedLoan.Any())
                        {
                            item.amountDisbursed = disbursedLoan.Sum(c => c.PRINCIPALAMOUNT);
                        }

                        item.utilizedAmount = item.approvedAmount - item.undisbursedAmount;
                    }

                    return runningLoans;

                }

            }
        }


        public IEnumerable<OriginalDocumentApprovalViewModel> GetOriginalDocumentSubmissionReport(DateTime startDate, DateTime endDate, string referenceNumber)
        {
            var data = new List<OriginalDocumentApprovalViewModel>();

            FinTrakBankingContext context = new FinTrakBankingContext();

            data = (from x in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                    join l in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                    join a in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                    join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                    where x.DELETED == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                    && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                            DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)

                    select new OriginalDocumentApprovalViewModel
                    {
                        originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                        loanApplicationId = x.LOANAPPLICATIONID,
                        description = x.DESCRIPTION,
                        approvalStatusId = (short)x.APPROVALSTATUSID,
                        applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                        referenceNumber = x.REFERENCENUMBER,
                        dateTimeCreated = x.DATETIMECREATED,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        customerId = c.CUSTOMERID,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        applicationDate = l.APPLICATIONDATE,
                        applicationAmount = l.APPLICATIONAMOUNT,
                        interestRate = l.INTERESTRATE,
                        approvalDate = x.APPROVALDATE,
                        productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        createdByName = context.TBL_STAFF.Where(o => o.STAFFID == x.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),

                    })
               .ToList();
            return data;
        }
        public IEnumerable<OriginalDocumentApprovalViewModel> SubmissionOfOriginalDocuments(DateTime startDate, DateTime endDate, string referenceNumber)
        {
            var data = new List<OriginalDocumentApprovalViewModel>();
            FinTrakBankingContext context = new FinTrakBankingContext();

            data = (from x in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                    join ll in context.TBL_LOAN_APPLICATION_COLLATERL on x.COLLATERALCUSTOMERID equals ll.COLLATERALCUSTOMERID
                    join a in context.TBL_LOAN_APPLICATION_DETAIL on ll.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                    join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                    join l in context.TBL_LOAN on a.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID
                    join p in context.TBL_PRODUCT on a.APPROVEDPRODUCTID equals p.PRODUCTID
                    join sta in context.TBL_STAFF on x.CREATEDBY equals sta.STAFFID

                    where
                    l.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && x.DELETED == false
                    && DbFunctions.TruncateTime(x.DATETIMECREATED).Value >= DbFunctions.TruncateTime(startDate).Value &&
                                            DbFunctions.TruncateTime(x.DATETIMECREATED).Value <= DbFunctions.TruncateTime(endDate).Value
                    select new OriginalDocumentApprovalViewModel
                    {
                        customerID = c.CUSTOMERID,
                        fintrakReferenceId = x.REFERENCENUMBER,
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        facilityType = p.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                        facilityAmount = l.PRINCIPALAMOUNT,
                        bookingDate = l.BOOKINGDATE,
                        sla = "N/A",
                        daysOverdue = (int)DbFunctions.DiffDays(DateTime.Now, l.MATURITYDATE),
                        accountOfficer = sta.FIRSTNAME + " " + sta.LASTNAME,
                        createdBy = sta.STAFFID,
                        team = sta.MISCODE,
                        division = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == l.CUSTOMERID select p.BUSINESSUNITNAME).FirstOrDefault(),
                    }).ToList();

            foreach (var i in data)
            {
                var rm = context.TBL_STAFF.Where(xx => xx.STAFFID == i.createdBy).Select(xx => xx.SUPERVISOR_STAFFID).FirstOrDefault();
                if (rm != null)
                {
                    var zh = context.TBL_STAFF.Where(xx => xx.STAFFID == rm).Select(xx => xx.SUPERVISOR_STAFFID).FirstOrDefault();
                    if (zh != null)
                    {
                        i.groupHead = context.TBL_STAFF.Where(xx => xx.STAFFID == zh).Select(xx => xx.FIRSTNAME + " " + xx.MIDDLENAME + " " + xx.LASTNAME).FirstOrDefault();
                    }
                }
            }

            return data;
        }

        //public IEnumerable<CustomerCompanyInfomationViewModels> CorporateCustomerCreation(DateTime startDate, DateTime endDate)
        //{
        //    var data = new List<CustomerCompanyInfomationViewModels>();
        //    FinTrakBankingContext context = new FinTrakBankingContext();

        //    data = (from ln in context.TBL_LOAN
        //            join r in context.TBL_LOAN_BOOKING_REQUEST on ln.LOAN_BOOKING_REQUESTID equals r.LOAN_BOOKING_REQUESTID
        //            join d in context.TBL_LOAN_APPLICATION_DETAIL on r.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
        //            join l in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
        //            join c in context.TBL_CUSTOMER on r.CUSTOMERID equals c.CUSTOMERID
        //            join cd in context.TBL_CUSTOMER_COMPANY_DIRECTOR on c.CUSTOMERID equals cd.CUSTOMERID
        //            join ci in context.TBL_CUSTOMER_COMPANYINFOMATION on cd.CUSTOMERID equals ci.CUSTOMERID
        //            join p in context.TBL_PRODUCT on ln.PRODUCTID equals p.PRODUCTID
        //            join pc in context.TBL_PRODUCT_CLASS on p.PRODUCTCLASSID equals pc.PRODUCTCLASSID
        //            join es in context.TBL_ESG_CHECKLIST_SUMMARY on d.LOANAPPLICATIONDETAILID equals es.LOANAPPLICATIONDETAILID into esr
        //            from es in esr.DefaultIfEmpty()
        //            join sc in context.TBL_ESG_CHECKLIST_SCORES on es.RATINGID equals sc.SCORE into scr
        //            from sc in scr.DefaultIfEmpty()

        //            where DbFunctions.TruncateTime(ln.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
        //            DbFunctions.TruncateTime(ln.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
        //            && c.CUSTOMERTYPEID == (int)CustomerTypeEnum.Corporate
        //            && (es.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || es == null)
        //            && (sc.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || sc == null)
        //            select new CustomerCompanyInfomationViewModels
        //            {
        //                companyName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
        //                fullName = cd.FIRSTNAME + " " + cd.MIDDLENAME + " " + cd.SURNAME,
        //                birthDate = cd.DATEOFBIRTH,
        //                gender =  cd.GENDER,
        //                address = context.TBL_CUSTOMER_ADDRESS.Where(x=> x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ADDRESS).FirstOrDefault(),
        //                state = (from a in context.TBL_CUSTOMER_ADDRESS join b in context.TBL_STATE on a.STATEID equals b.STATEID where a.CUSTOMERID == c.CUSTOMERID select b.STATENAME).FirstOrDefault(),
        //                phoneNo = context.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault(ph => ph.CUSTOMERID == c.CUSTOMERID).PHONENUMBER,
        //                email = c.EMAILADDRESS,
        //                approvedAmount = d.APPROVEDAMOUNT,
        //                amountGranted = r.AMOUNT_REQUESTED,
        //                effectiveDate = ln.EFFECTIVEDATE,
        //                tenor = d.APPROVEDTENOR,
        //                tenorType = "days",
        //                rate = d.APPROVEDINTERESTRATE,
        //                baseYear = ln.DISBURSEDATE.HasValue ? ln.DISBURSEDATE.Value.Year : 0,
        //                bvn = cd.CUSTOMERBVN,
        //                scheduleType = context.TBL_LOAN_SCHEDULE_TYPE.Where(x => x.SCHEDULETYPEID == ln.SCHEDULETYPEID).Select(x => x.SCHEDULETYPENAME).FirstOrDefault(),
        //                interestRepayStartDate = ln.FIRSTINTERESTPAYMENTDATE,
        //                principalRepayStartDate = ln.FIRSTPRINCIPALPAYMENTDATE,
        //                interestRepayFreq = context.TBL_FREQUENCY_TYPE.Where(x => x.FREQUENCYTYPEID == ln.INTERESTFREQUENCYTYPEID).Select(x => x.MODE).FirstOrDefault(),
        //                principalRepayFreq = context.TBL_FREQUENCY_TYPE.Where(x => x.FREQUENCYTYPEID == ln.PRINCIPALFREQUENCYTYPEID).Select(x => x.MODE).FirstOrDefault(),
        //                sector = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
        //                natureOfBusiness = ln.TBL_SUB_SECTOR.NAME,
        //                firstTimeAccessToCredit = ci.ISFIRSTTIMECREDIT ? "Yes" : "No",
        //                startUp = ci.ISSTARTUP ? "Yes" : "No",
        //                msmeAnnualTurnover = ci.ANNUALTURNOVER,
        //                noOfEmployees = ci.NUMBEROFEMPLOYEES ?? 0,
        //                noOfFemaleEmployees = ci.NOOFFEMALEEMPLOYEES,
        //                moratorium = d.MORATORIUM,
        //                esRating = sc.GRADE,
        //                wpower = pc.PRODUCTCLASSID == 31 ? "Yes" : "No",
        //                facilityType = p.PRODUCTNAME,
        //                refNo = l.APPLICATIONREFERENCENUMBER,
        //                customerCode = c.CUSTOMERCODE,
        //                totalAsset = ci.TOTALASSETS

        //            })
        //       .ToList();

        //var revolving = (from ln in context.TBL_LOAN_REVOLVING
        //            join r in context.TBL_LOAN_BOOKING_REQUEST on ln.LOAN_BOOKING_REQUESTID equals r.LOAN_BOOKING_REQUESTID
        //            join d in context.TBL_LOAN_APPLICATION_DETAIL on r.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
        //            join l in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
        //            join c in context.TBL_CUSTOMER on r.CUSTOMERID equals c.CUSTOMERID
        //            join cd in context.TBL_CUSTOMER_COMPANY_DIRECTOR on c.CUSTOMERID equals cd.CUSTOMERID
        //            join ci in context.TBL_CUSTOMER_COMPANYINFOMATION on cd.CUSTOMERID equals ci.CUSTOMERID
        //            join p in context.TBL_PRODUCT on ln.PRODUCTID equals p.PRODUCTID
        //            join pc in context.TBL_PRODUCT_CLASS on p.PRODUCTCLASSID equals pc.PRODUCTCLASSID
        //            join es in context.TBL_ESG_CHECKLIST_SUMMARY on d.LOANAPPLICATIONDETAILID equals es.LOANAPPLICATIONDETAILID into esr
        //            from es in esr.DefaultIfEmpty()
        //            join sc in context.TBL_ESG_CHECKLIST_SCORES on es.RATINGID equals sc.SCORE into scr
        //            from sc in scr.DefaultIfEmpty()

        //            where DbFunctions.TruncateTime(ln.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
        //            DbFunctions.TruncateTime(ln.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
        //            && c.CUSTOMERTYPEID == (int)CustomerTypeEnum.Corporate
        //            && (es.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || es == null)
        //            && (sc.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || sc == null)
        //                 select new CustomerCompanyInfomationViewModels
        //            {
        //                companyName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
        //                fullName = cd.FIRSTNAME + " " + cd.MIDDLENAME + " " + cd.SURNAME,
        //                birthDate = cd.DATEOFBIRTH,
        //                gender = cd.GENDER,
        //                address = context.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ADDRESS).FirstOrDefault(),
        //                state = (from a in context.TBL_CUSTOMER_ADDRESS join b in context.TBL_STATE on a.STATEID equals b.STATEID where a.CUSTOMERID == c.CUSTOMERID select b.STATENAME).FirstOrDefault(),
        //                phoneNo = context.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault(ph => ph.CUSTOMERID == c.CUSTOMERID).PHONENUMBER,
        //                email = c.EMAILADDRESS,
        //                approvedAmount = d.APPROVEDAMOUNT,
        //                amountGranted = r.AMOUNT_REQUESTED,
        //                effectiveDate = ln.EFFECTIVEDATE,
        //                tenor = d.APPROVEDTENOR,
        //                tenorType = "days",
        //                rate = d.APPROVEDINTERESTRATE,
        //                baseYear = ln.DISBURSEDATE.HasValue ? ln.DISBURSEDATE.Value.Year : 0,
        //                bvn = cd.CUSTOMERBVN,
        //                scheduleType = "N/A",
        //                interestRepayStartDate = ln.MATURITYDATE,
        //                principalRepayStartDate = ln.MATURITYDATE,
        //                interestRepayFreq = context.TBL_REPAYMENT_TERM.FirstOrDefault(x => x.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).REPAYMENTTERMDETAIL,
        //                principalRepayFreq = context.TBL_REPAYMENT_TERM.FirstOrDefault(x => x.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).REPAYMENTTERMDETAIL,
        //                sector = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
        //                natureOfBusiness = ln.TBL_SUB_SECTOR.NAME,
        //                firstTimeAccessToCredit = ci.ISFIRSTTIMECREDIT ? "Yes" : "No",
        //                startUp = ci.ISSTARTUP ? "Yes" : "No",
        //                msmeAnnualTurnover = ci.ANNUALTURNOVER,
        //                noOfEmployees = ci.NUMBEROFEMPLOYEES ?? 0,
        //                noOfFemaleEmployees = ci.NOOFFEMALEEMPLOYEES,
        //                moratorium = d.MORATORIUM,
        //                esRating = sc.GRADE,
        //                wpower = pc.PRODUCTCLASSID == 31 ? "Yes" : "No",
        //                facilityType = p.PRODUCTNAME,
        //                refNo = l.APPLICATIONREFERENCENUMBER,
        //                customerCode = c.CUSTOMERCODE
        //                totalAsset = ci.TOTALASSETS
        //                 });

        //    var contingent = (from ln in context.TBL_LOAN_CONTINGENT
        //                      join r in context.TBL_LOAN_BOOKING_REQUEST on ln.LOAN_BOOKING_REQUESTID equals r.LOAN_BOOKING_REQUESTID
        //                      join d in context.TBL_LOAN_APPLICATION_DETAIL on r.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
        //                      join l in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
        //                      join c in context.TBL_CUSTOMER on r.CUSTOMERID equals c.CUSTOMERID
        //                      join cd in context.TBL_CUSTOMER_COMPANY_DIRECTOR on c.CUSTOMERID equals cd.CUSTOMERID
        //                      join ci in context.TBL_CUSTOMER_COMPANYINFOMATION on cd.CUSTOMERID equals ci.CUSTOMERID
        //                      join p in context.TBL_PRODUCT on ln.PRODUCTID equals p.PRODUCTID
        //                      join pc in context.TBL_PRODUCT_CLASS on p.PRODUCTCLASSID equals pc.PRODUCTCLASSID
        //                      join es in context.TBL_ESG_CHECKLIST_SUMMARY on d.LOANAPPLICATIONDETAILID equals es.LOANAPPLICATIONDETAILID into esr
        //                      from es in esr.DefaultIfEmpty()
        //                      join sc in context.TBL_ESG_CHECKLIST_SCORES on es.RATINGID equals sc.SCORE into scr
        //                      from sc in scr.DefaultIfEmpty()

        //                      where DbFunctions.TruncateTime(ln.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
        //                      DbFunctions.TruncateTime(ln.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
        //                      && c.CUSTOMERTYPEID == (int)CustomerTypeEnum.Corporate
        //                      && (es.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || es == null)
        //                      && (sc.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || sc == null)
        //                      select new CustomerCompanyInfomationViewModels
        //                      {
        //                          companyName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
        //                          fullName = cd.FIRSTNAME + " " + cd.MIDDLENAME + " " + cd.SURNAME,
        //                          birthDate = cd.DATEOFBIRTH,
        //                          gender = cd.GENDER,
        //                          address = context.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ADDRESS).FirstOrDefault(),
        //                          state = (from a in context.TBL_CUSTOMER_ADDRESS join b in context.TBL_STATE on a.STATEID equals b.STATEID where a.CUSTOMERID == c.CUSTOMERID select b.STATENAME).FirstOrDefault(),
        //                          phoneNo = context.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault(ph => ph.CUSTOMERID == c.CUSTOMERID).PHONENUMBER,
        //                          email = c.EMAILADDRESS,
        //                          approvedAmount = d.APPROVEDAMOUNT,
        //                          amountGranted = r.AMOUNT_REQUESTED,
        //                          effectiveDate = ln.EFFECTIVEDATE,
        //                          tenor = d.APPROVEDTENOR,
        //                          tenorType = "days",
        //                          rate = d.APPROVEDINTERESTRATE,
        //                          baseYear = ln.DISBURSEDATE.HasValue ? ln.DISBURSEDATE.Value.Year : 0,
        //                          bvn = cd.CUSTOMERBVN,
        //                          scheduleType = "N/A",
        //                          interestRepayStartDate = ln.MATURITYDATE,
        //                          principalRepayStartDate = ln.MATURITYDATE,
        //                          interestRepayFreq = context.TBL_REPAYMENT_TERM.FirstOrDefault(x => x.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).REPAYMENTTERMDETAIL,
        //                          principalRepayFreq = context.TBL_REPAYMENT_TERM.FirstOrDefault(x => x.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).REPAYMENTTERMDETAIL,
        //                          sector = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
        //                          natureOfBusiness = ln.TBL_SUB_SECTOR.NAME,
        //                          firstTimeAccessToCredit = ci.ISFIRSTTIMECREDIT ? "Yes" : "No",
        //                          startUp = ci.ISSTARTUP ? "Yes" : "No",
        //                          msmeAnnualTurnover = ci.ANNUALTURNOVER,
        //                          noOfEmployees = ci.NUMBEROFEMPLOYEES ?? 0,
        //                          noOfFemaleEmployees = ci.NOOFFEMALEEMPLOYEES,
        //                          moratorium = d.MORATORIUM,
        //                          esRating = sc.GRADE,
        //                          wpower = pc.PRODUCTCLASSID == 31 ? "Yes" : "No",
        //                          facilityType = p.PRODUCTNAME,
        //                          refNo = l.APPLICATIONREFERENCENUMBER,
        //                          customerCode = c.CUSTOMERCODE,
        //                           totalAsset = ci.TOTALASSETS
        //                      });

        //    var result = data.Union(revolving).Union(contingent).ToList();
        //    return result;
        //}


        public IEnumerable<CustomerCompanyInfomationViewModels> CorporateCustomerCreation(DateTime startDate, DateTime endDate)
        {
            var data = new List<CustomerCompanyInfomationViewModels>();
            FinTrakBankingContext context = new FinTrakBankingContext();

            data = (from ln in context.TBL_LOAN
                    join r in context.TBL_LOAN_BOOKING_REQUEST on ln.LOAN_BOOKING_REQUESTID equals r.LOAN_BOOKING_REQUESTID
                    join d in context.TBL_LOAN_APPLICATION_DETAIL on r.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                    join l in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                    join c in context.TBL_CUSTOMER on r.CUSTOMERID equals c.CUSTOMERID
                    //join cd in context.TBL_CUSTOMER_COMPANY_DIRECTOR on c.CUSTOMERID equals cd.CUSTOMERID
                    //join ci in context.TBL_CUSTOMER_COMPANYINFOMATION on cd.CUSTOMERID equals ci.CUSTOMERID
                    join p in context.TBL_PRODUCT on ln.PRODUCTID equals p.PRODUCTID
                    join pc in context.TBL_PRODUCT_CLASS on p.PRODUCTCLASSID equals pc.PRODUCTCLASSID
                    /*join es in context.TBL_ESG_CHECKLIST_SUMMARY on d.LOANAPPLICATIONDETAILID equals es.LOANAPPLICATIONDETAILID into esr
                    from es in esr.DefaultIfEmpty()
                    join sc in context.TBL_ESG_CHECKLIST_SCORES on es.RATINGID equals sc.SCORE into scr
                    from sc in scr.DefaultIfEmpty()*/

                    where DbFunctions.TruncateTime(ln.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                    DbFunctions.TruncateTime(ln.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                    && c.CUSTOMERTYPEID == (int)CustomerTypeEnum.Corporate
                    //&& (es.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || es == null)
                    //&& (sc.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || sc == null)

                    select new CustomerCompanyInfomationViewModels
                    {
                        customerId = c.CUSTOMERID,
                        companyName = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.COMPANYNAME).FirstOrDefault(),

                        fullName = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.FIRSTNAME + " " + cd.MIDDLENAME + " " + cd.SURNAME).FirstOrDefault(),
                        birthDate = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.DATEOFBIRTH).FirstOrDefault(),
                        gender = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.GENDER).FirstOrDefault(),
                        field1 = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.ISTHEPROMOTER).FirstOrDefault() == true ? "Yes" : "No",
                        bvn = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.CUSTOMERBVN).FirstOrDefault(),

                        address = context.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ADDRESS).FirstOrDefault(),
                        state = (from a in context.TBL_CUSTOMER_ADDRESS join b in context.TBL_STATE on a.STATEID equals b.STATEID where a.CUSTOMERID == c.CUSTOMERID select b.STATENAME).FirstOrDefault(),
                        phoneNo = context.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault(ph => ph.CUSTOMERID == c.CUSTOMERID).PHONENUMBER,
                        email = c.EMAILADDRESS,
                        approvedAmount = d.APPROVEDAMOUNT,
                        amountGranted = r.AMOUNT_REQUESTED,
                        effectiveDate = ln.EFFECTIVEDATE,
                        tenor = d.APPROVEDTENOR,
                        tenorType = "days",
                        rate = d.APPROVEDINTERESTRATE,
                        baseYear = ln.DISBURSEDATE.HasValue ? ln.DISBURSEDATE.Value.Year : 0,
                        scheduleType = context.TBL_LOAN_SCHEDULE_TYPE.Where(x => x.SCHEDULETYPEID == ln.SCHEDULETYPEID).Select(x => x.SCHEDULETYPENAME).FirstOrDefault(),
                        interestRepayStartDate = ln.FIRSTINTERESTPAYMENTDATE,
                        principalRepayStartDate = ln.FIRSTPRINCIPALPAYMENTDATE,
                        interestRepayFreq = context.TBL_FREQUENCY_TYPE.Where(x => x.FREQUENCYTYPEID == ln.INTERESTFREQUENCYTYPEID).Select(x => x.MODE).FirstOrDefault(),
                        principalRepayFreq = context.TBL_FREQUENCY_TYPE.Where(x => x.FREQUENCYTYPEID == ln.PRINCIPALFREQUENCYTYPEID).Select(x => x.MODE).FirstOrDefault(),
                        sector = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                        natureOfBusiness = ln.TBL_SUB_SECTOR.NAME,
                        moratorium = d.MORATORIUM,
                        esRating = (from es in context.TBL_ESG_CHECKLIST_SUMMARY
                                    join sc in context.TBL_ESG_CHECKLIST_SCORES on es.RATINGID equals sc.SCORE
                                    where es.LOANAPPLICATIONDETAILID == d.LOANAPPLICATIONDETAILID
                                    && (es.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || es == null
                                    && (sc.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || sc == null))
                                    select sc.GRADE).FirstOrDefault(),   //sc.GRADE,
                        wpower = pc.PRODUCTCLASSID == 31 ? "Yes" : "No",
                        facilityType = p.PRODUCTNAME,
                        refNo = l.APPLICATIONREFERENCENUMBER,
                        customerCode = c.CUSTOMERCODE,

                        firstTimeAccessToCredit = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ISFIRSTTIMECREDIT).FirstOrDefault() ? "Yes" : "No",
                        startUp = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ISSTARTUP).FirstOrDefault() ? "Yes" : "No",
                        msmeAnnualTurnover = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ANNUALTURNOVER).FirstOrDefault(),
                        noOfEmployees = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.NUMBEROFEMPLOYEES).FirstOrDefault() ?? 0,
                        noOfFemaleEmployees = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.NOOFFEMALEEMPLOYEES).FirstOrDefault(),
                        totalAsset = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.TOTALASSETS).FirstOrDefault() ?? 0,

                    }).ToList();

            var revolving = (from ln in context.TBL_LOAN_REVOLVING
                             join r in context.TBL_LOAN_BOOKING_REQUEST on ln.LOAN_BOOKING_REQUESTID equals r.LOAN_BOOKING_REQUESTID
                             join d in context.TBL_LOAN_APPLICATION_DETAIL on r.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                             join l in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                             join c in context.TBL_CUSTOMER on r.CUSTOMERID equals c.CUSTOMERID
                             //join cd in context.TBL_CUSTOMER_COMPANY_DIRECTOR on c.CUSTOMERID equals cd.CUSTOMERID
                             //join ci in context.TBL_CUSTOMER_COMPANYINFOMATION on cd.CUSTOMERID equals ci.CUSTOMERID
                             join p in context.TBL_PRODUCT on ln.PRODUCTID equals p.PRODUCTID
                             join pc in context.TBL_PRODUCT_CLASS on p.PRODUCTCLASSID equals pc.PRODUCTCLASSID
                             /*join es in context.TBL_ESG_CHECKLIST_SUMMARY on d.LOANAPPLICATIONDETAILID equals es.LOANAPPLICATIONDETAILID into esr
                             from es in esr.DefaultIfEmpty()
                             join sc in context.TBL_ESG_CHECKLIST_SCORES on es.RATINGID equals sc.SCORE into scr
                             from sc in scr.DefaultIfEmpty()*/

                             where DbFunctions.TruncateTime(ln.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                             DbFunctions.TruncateTime(ln.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                             && c.CUSTOMERTYPEID == (int)CustomerTypeEnum.Corporate
                             //&& (es.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || es == null)
                             //&& (sc.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || sc == null)
                             //&& cd.ISTHEPROMOTER == true
                             select new CustomerCompanyInfomationViewModels
                             {
                                 customerId = c.CUSTOMERID,
                                 companyName = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.COMPANYNAME).FirstOrDefault(),
                                 fullName = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.FIRSTNAME + " " + cd.MIDDLENAME + " " + cd.SURNAME).FirstOrDefault(),
                                 birthDate = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.DATEOFBIRTH).FirstOrDefault(),
                                 gender = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.GENDER).FirstOrDefault(),
                                 field1 = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.ISTHEPROMOTER).FirstOrDefault() == true ? "Yes" : "No",
                                 bvn = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.CUSTOMERBVN).FirstOrDefault(),

                                 address = context.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ADDRESS).FirstOrDefault(),
                                 state = (from a in context.TBL_CUSTOMER_ADDRESS join b in context.TBL_STATE on a.STATEID equals b.STATEID where a.CUSTOMERID == c.CUSTOMERID select b.STATENAME).FirstOrDefault(),
                                 phoneNo = context.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault(ph => ph.CUSTOMERID == c.CUSTOMERID).PHONENUMBER,
                                 email = c.EMAILADDRESS,
                                 approvedAmount = d.APPROVEDAMOUNT,
                                 amountGranted = r.AMOUNT_REQUESTED,
                                 effectiveDate = ln.EFFECTIVEDATE,
                                 tenor = d.APPROVEDTENOR,
                                 tenorType = "days",
                                 rate = d.APPROVEDINTERESTRATE,
                                 baseYear = ln.DISBURSEDATE.HasValue ? ln.DISBURSEDATE.Value.Year : 0,
                                 scheduleType = "N/A",
                                 interestRepayStartDate = ln.MATURITYDATE,
                                 principalRepayStartDate = ln.MATURITYDATE,
                                 interestRepayFreq = context.TBL_REPAYMENT_TERM.FirstOrDefault(x => x.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).REPAYMENTTERMDETAIL,
                                 principalRepayFreq = context.TBL_REPAYMENT_TERM.FirstOrDefault(x => x.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).REPAYMENTTERMDETAIL,
                                 sector = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                 natureOfBusiness = ln.TBL_SUB_SECTOR.NAME,
                                 moratorium = d.MORATORIUM,
                                 esRating = (from es in context.TBL_ESG_CHECKLIST_SUMMARY
                                             join sc in context.TBL_ESG_CHECKLIST_SCORES on es.RATINGID equals sc.SCORE
                                             where es.LOANAPPLICATIONDETAILID == d.LOANAPPLICATIONDETAILID
                                                && (es.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || es == null
                                                && (sc.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || sc == null))
                                             select sc.GRADE).FirstOrDefault(),   //sc.GRADE,
                                 wpower = pc.PRODUCTCLASSID == 31 ? "Yes" : "No",
                                 facilityType = p.PRODUCTNAME,
                                 refNo = l.APPLICATIONREFERENCENUMBER,
                                 customerCode = c.CUSTOMERCODE,
                                 firstTimeAccessToCredit = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ISFIRSTTIMECREDIT).FirstOrDefault() ? "Yes" : "No",
                                 startUp = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ISSTARTUP).FirstOrDefault() ? "Yes" : "No",
                                 msmeAnnualTurnover = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ANNUALTURNOVER).FirstOrDefault(),
                                 noOfEmployees = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.NUMBEROFEMPLOYEES).FirstOrDefault() ?? 0,
                                 noOfFemaleEmployees = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.NOOFFEMALEEMPLOYEES).FirstOrDefault(),
                                 totalAsset = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.TOTALASSETS).FirstOrDefault() ?? 0,

                             }).ToList();

            var contingent = (from ln in context.TBL_LOAN_CONTINGENT
                              join r in context.TBL_LOAN_BOOKING_REQUEST on ln.LOAN_BOOKING_REQUESTID equals r.LOAN_BOOKING_REQUESTID
                              join d in context.TBL_LOAN_APPLICATION_DETAIL on r.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                              join l in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                              join c in context.TBL_CUSTOMER on r.CUSTOMERID equals c.CUSTOMERID
                              //join cd in context.TBL_CUSTOMER_COMPANY_DIRECTOR on c.CUSTOMERID equals cd.CUSTOMERID
                              //join ci in context.TBL_CUSTOMER_COMPANYINFOMATION on cd.CUSTOMERID equals ci.CUSTOMERID
                              join p in context.TBL_PRODUCT on ln.PRODUCTID equals p.PRODUCTID
                              join pc in context.TBL_PRODUCT_CLASS on p.PRODUCTCLASSID equals pc.PRODUCTCLASSID
                              /*join es in context.TBL_ESG_CHECKLIST_SUMMARY on d.LOANAPPLICATIONDETAILID equals es.LOANAPPLICATIONDETAILID into esr
                              from es in esr.DefaultIfEmpty()
                              join sc in context.TBL_ESG_CHECKLIST_SCORES on es.RATINGID equals sc.SCORE into scr
                              from sc in scr.DefaultIfEmpty()*/

                              where DbFunctions.TruncateTime(ln.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                              DbFunctions.TruncateTime(ln.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)
                              && c.CUSTOMERTYPEID == (int)CustomerTypeEnum.Corporate
                              //&& (es.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || es == null)
                              //&& (sc.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || sc == null)
                              //&& cd.ISTHEPROMOTER == true
                              select new CustomerCompanyInfomationViewModels
                              {
                                  customerId = c.CUSTOMERID,
                                  companyName = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.COMPANYNAME).FirstOrDefault(),
                                  fullName = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.FIRSTNAME + " " + cd.MIDDLENAME + " " + cd.SURNAME).FirstOrDefault(),
                                  birthDate = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.DATEOFBIRTH).FirstOrDefault(),
                                  gender = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.GENDER).FirstOrDefault(),
                                  field1 = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.ISTHEPROMOTER).FirstOrDefault() == true ? "Yes" : "No",
                                  bvn = context.TBL_CUSTOMER_COMPANY_DIRECTOR.Where(cd => cd.CUSTOMERID == c.CUSTOMERID).Select(cd => cd.CUSTOMERBVN).FirstOrDefault(),

                                  address = context.TBL_CUSTOMER_ADDRESS.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ADDRESS).FirstOrDefault(),
                                  state = (from a in context.TBL_CUSTOMER_ADDRESS join b in context.TBL_STATE on a.STATEID equals b.STATEID where a.CUSTOMERID == c.CUSTOMERID select b.STATENAME).FirstOrDefault(),
                                  phoneNo = context.TBL_CUSTOMER_PHONECONTACT.FirstOrDefault(ph => ph.CUSTOMERID == c.CUSTOMERID).PHONENUMBER,
                                  email = c.EMAILADDRESS,
                                  approvedAmount = d.APPROVEDAMOUNT,
                                  amountGranted = r.AMOUNT_REQUESTED,
                                  effectiveDate = ln.EFFECTIVEDATE,
                                  tenor = d.APPROVEDTENOR,
                                  tenorType = "days",
                                  rate = d.APPROVEDINTERESTRATE,
                                  baseYear = ln.DISBURSEDATE.HasValue ? ln.DISBURSEDATE.Value.Year : 0,
                                  scheduleType = "N/A",
                                  interestRepayStartDate = ln.MATURITYDATE,
                                  principalRepayStartDate = ln.MATURITYDATE,
                                  interestRepayFreq = context.TBL_REPAYMENT_TERM.FirstOrDefault(x => x.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).REPAYMENTTERMDETAIL,
                                  principalRepayFreq = context.TBL_REPAYMENT_TERM.FirstOrDefault(x => x.REPAYMENTSCHEDULEID == d.REPAYMENTSCHEDULEID).REPAYMENTTERMDETAIL,
                                  sector = ln.TBL_SUB_SECTOR.TBL_SECTOR.NAME,
                                  natureOfBusiness = ln.TBL_SUB_SECTOR.NAME,
                                  moratorium = d.MORATORIUM,
                                  esRating = (from es in context.TBL_ESG_CHECKLIST_SUMMARY
                                              join sc in context.TBL_ESG_CHECKLIST_SCORES on es.RATINGID equals sc.SCORE
                                              where es.LOANAPPLICATIONDETAILID == d.LOANAPPLICATIONDETAILID
                                                && (es.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || es == null
                                                && (sc.CHECKLIST_TYPEID == (int)CheckListTypeEnum.ESGMChecklist || sc == null))
                                              select sc.GRADE).FirstOrDefault(),   //sc.GRADE,
                                  wpower = pc.PRODUCTCLASSID == 31 ? "Yes" : "No",
                                  facilityType = p.PRODUCTNAME,
                                  refNo = l.APPLICATIONREFERENCENUMBER,
                                  customerCode = c.CUSTOMERCODE,
                                  firstTimeAccessToCredit = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ISFIRSTTIMECREDIT).FirstOrDefault() ? "Yes" : "No",
                                  startUp = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ISSTARTUP).FirstOrDefault() ? "Yes" : "No",
                                  msmeAnnualTurnover = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.ANNUALTURNOVER).FirstOrDefault(),
                                  noOfEmployees = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.NUMBEROFEMPLOYEES).FirstOrDefault() ?? 0,
                                  noOfFemaleEmployees = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.NOOFFEMALEEMPLOYEES).FirstOrDefault(),
                                  totalAsset = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == c.CUSTOMERID).Select(x => x.TOTALASSETS).FirstOrDefault() ?? 0,
                              }).ToList();
            var result = data.Union(revolving).Union(contingent).Distinct().ToList();

            return result;
        }

        public List<TrialBalanceViewModel> TrialBalanceSummary(int glAccountId, int currencyCode, int companyId, int staffId)
        {

            List<TrialBalanceViewModel> trialBal;

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var termLoans = (from ft in context.TBL_LOAN
                                 join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                 select new
                                 {
                                     ft.LOANREFERENCENUMBER,
                                     ft.PRODUCTID,
                                     p.PRODUCTCODE,
                                     p.PRODUCTNAME,
                                     firstName = ft.TBL_CUSTOMER.FIRSTNAME,
                                     lastName = ft.TBL_CUSTOMER.LASTNAME,
                                     middleName = ft.TBL_CUSTOMER.MIDDLENAME,
                                     appDetailId = ft.LOANAPPLICATIONDETAILID,
                                     appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                     accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER
                                 });

                var revolvingLoans = (from ft in context.TBL_LOAN_REVOLVING
                                      join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                      select new
                                      {
                                          ft.LOANREFERENCENUMBER,
                                          ft.PRODUCTID,
                                          p.PRODUCTCODE,
                                          p.PRODUCTNAME,
                                          firstName = ft.TBL_CUSTOMER.FIRSTNAME,
                                          lastName = ft.TBL_CUSTOMER.LASTNAME,
                                          middleName = ft.TBL_CUSTOMER.MIDDLENAME,
                                          appDetailId = ft.LOANAPPLICATIONDETAILID,
                                          appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                          accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER
                                      });

                var contingentLoans = (from ft in context.TBL_LOAN_CONTINGENT
                                       join p in context.TBL_PRODUCT on ft.PRODUCTID equals p.PRODUCTID
                                       select new
                                       {
                                           ft.LOANREFERENCENUMBER,
                                           ft.PRODUCTID,
                                           p.PRODUCTCODE,
                                           p.PRODUCTNAME,
                                           firstName = ft.TBL_CUSTOMER.FIRSTNAME,
                                           lastName = ft.TBL_CUSTOMER.LASTNAME,
                                           middleName = ft.TBL_CUSTOMER.MIDDLENAME,
                                           appDetailId = ft.LOANAPPLICATIONDETAILID,
                                           appRef = ft.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                           accountnumber = ft.TBL_CASA.PRODUCTACCOUNTNUMBER
                                       });

                var allLoans = termLoans.Union(revolvingLoans).Union(contingentLoans).Distinct().ToList();

                var all = allLoans;

                trialBal = (from ft in context.TBL_FINANCE_TRANSACTION
                            join ca in context.TBL_CHART_OF_ACCOUNT on ft.GLACCOUNTID equals ca.GLACCOUNTID
                            join cu in context.TBL_CURRENCY on ft.CURRENCYID equals cu.CURRENCYID

                            where ft.GLACCOUNTID == glAccountId && cu.CURRENCYID == currencyCode && ft.COMPANYID == companyId
                            //   group ft by new { ft.GLACCOUNTID, ca.ACCOUNTCODE, ca.ACCOUNTNAME, cu.CURRENCYNAME } into groupedQ

                            select new TrialBalanceViewModel()
                            {

                                glAccountId = ft.GLACCOUNTID,
                                accountCode = ca.ACCOUNTCODE,
                                accountName = ca.ACCOUNTNAME,
                                currency = cu.CURRENCYNAME,
                                creditAmount = ft.CREDITAMOUNT,
                                debitAmount = ft.DEBITAMOUNT,
                                sourceReferenceNumber = ft.SOURCEREFERENCENUMBER,
                                batchCode = ft.BATCHCODE,
                                transactionDate = ft.APPROVEDDATE,
                                narration = ft.DESCRIPTION,

                                //  totalDebit = groupedQ.Sum(i=>i.DEBITAMOUNT)

                            }).ToList().Select(x =>
                            {

                                var loanreaferencenumber = 1;
                                bool hasdetailid = false;
                                string accountcode = "";
                                if (allLoans.Where(w => w.LOANREFERENCENUMBER == x.sourceReferenceNumber).Count() == 0 && x.sourceReferenceNumber.Contains('-'))
                                {
                                    string[] localRef = x.sourceReferenceNumber.Split('-');
                                    int localloandetailid = 0;
                                    int.TryParse(localRef[1], out localloandetailid);
                                    if (allLoans.Where(e => e.appDetailId == localloandetailid).Count() > 0)
                                        x.sourceReferenceNumber = allLoans.Where(e => e.appDetailId == localloandetailid).FirstOrDefault().LOANREFERENCENUMBER;
                                    accountcode = allLoans.Where(e => e.appDetailId == localloandetailid).Count() > 0 ? allLoans.Where(e => e.appDetailId == localloandetailid).FirstOrDefault().accountnumber : null;

                                    hasdetailid = true;
                                }


                                var getProductname = all.Where(m => m.LOANREFERENCENUMBER == x.sourceReferenceNumber).Select(m => m.PRODUCTNAME).FirstOrDefault();

                                // var getProductname = allLoans.Where(m => m.LOANREFERENCENUMBER == x.sourceReferenceNumber).Select(m => m.PRODUCTNAME).FirstOrDefault();

                                x.productName = getProductname == null ? " " : getProductname.ToString();

                                var getProductCode = allLoans.Where(z => z.LOANREFERENCENUMBER == x.sourceReferenceNumber).Select(z => z.PRODUCTCODE).FirstOrDefault();

                                x.productCode = getProductCode == null ? " " : getProductCode.ToString();
                                var firstName = allLoans.Where(z => z.LOANREFERENCENUMBER == x.sourceReferenceNumber).Select(m => m.firstName).FirstOrDefault();
                                firstName = firstName == null ? " " : firstName;
                                var middleName = allLoans.Where(z => z.LOANREFERENCENUMBER == x.sourceReferenceNumber).Select(m => m.middleName).FirstOrDefault();
                                middleName = middleName == null ? " " : middleName;
                                var lastName = allLoans.Where(z => z.LOANREFERENCENUMBER == x.sourceReferenceNumber).Select(m => m.lastName).FirstOrDefault();
                                lastName = lastName == null ? " " : lastName;
                                x.customerName = firstName + " " + middleName + " " + lastName;
                                if (hasdetailid)
                                {

                                    try
                                    {
                                        x.sourceReferenceNumber = accountcode;
                                    }
                                    catch (Exception ex)
                                    {
                                        string message = ex.Message;
                                    }

                                }
                                return x;
                            }).OrderBy(z => z.transactionDate).ToList();

            }

            return trialBal;
        }

        public List<CollateralPerfectionViewModel> CollateralPerfection(DateTime startDate, DateTime endDate, int status, int companyid)
        {
            // List<STG_MIS_INFO> misInfo = new List<STG_MIS_INFO>();
            using (FinTrakBankingStagingContext stagecontext = new FinTrakBankingStagingContext())
            {
                // misInfo = (from mis in stagecontext.STG_MIS_INFO select mis).ToList();

                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    IQueryable<int> loansWithCollateral = null;

                    if (status != -1)
                    {
                        loansWithCollateral = (from f in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                                               join lc in context.TBL_LOAN_APPLICATION_COLLATERL on f.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                                               join ll in context.TBL_LOAN_APPLICATION_DETAIL on lc.LOANAPPLICATIONID equals ll.LOANAPPLICATIONID
                                               where f.PERFECTIONSTATUSID == status
                                               select ll.LOANAPPLICATIONDETAILID);
                    }
                    else if (status == -1)
                    {
                        loansWithCollateral = (from f in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                                               join lc in context.TBL_LOAN_APPLICATION_COLLATERL on f.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                                               join ll in context.TBL_LOAN_APPLICATION_DETAIL on lc.LOANAPPLICATIONID equals ll.LOANAPPLICATIONID
                                               //where f.PERFECTIONSTATUSID == status
                                               select ll.LOANAPPLICATIONDETAILID);
                    }





                    var reportData = (
                                     from l in context.TBL_LOAN
                                     join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                     join sta in context.TBL_STAFF on l.RELATIONSHIPOFFICERID equals sta.STAFFID
                                     join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                     join ll in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ll.LOANAPPLICATIONDETAILID
                                     join cm in context.TBL_LOAN_APPLICATION_COLLATERL on ll.LOANAPPLICATIONID equals cm.LOANAPPLICATIONID
                                     join cim in context.TBL_COLLATERAL_IMMOVE_PROPERTY on cm.COLLATERALCUSTOMERID equals cim.COLLATERALCUSTOMERID
                                     join cc in context.TBL_COLLATERAL_CUSTOMER on cm.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID

                                     where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) && DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                     && l.COMPANYID == companyid && loansWithCollateral.Contains(l.LOANAPPLICATIONDETAILID) && l.LOANSTATUSID == (short)LoanStatusEnum.Active
                                     orderby l.DATETIMECREATED descending
                                     select new CollateralPerfectionViewModel
                                     {
                                         loanId = l.TERMLOANID,
                                         customername = c.FIRSTNAME + " " + c.MIDDLENAME,
                                         outstandingBalance = l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL,
                                         outstandingInterest = l.OUTSTANDINGINTEREST + l.PASTDUEINTEREST,
                                         startDate = startDate,
                                         endDate = endDate,
                                         facilityGrantDate = l.EFFECTIVEDATE,
                                         staffCode = sta.STAFFCODE,
                                         total = (l.OUTSTANDINGPRINCIPAL + l.PASTDUEPRINCIPAL) + (l.OUTSTANDINGINTEREST + l.PASTDUEINTEREST),
                                         misCode = l.MISCODE,
                                         loanReferenceNumber = l.LOANREFERENCENUMBER,
                                         solId = b.BRANCHID,
                                         branchName = b.BRANCHNAME,
                                         expiryDate = l.MATURITYDATE,
                                         sanctionLimit = l.PRINCIPALAMOUNT,
                                         remarks = cim.REMARK,
                                         collateralSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(x => x.COLLATERALSUBTYPEID == cc.COLLATERALSUBTYPEID).Select(x => x.COLLATERALSUBTYPENAME).FirstOrDefault(),
                                         tenor = (int)DbFunctions.DiffDays(l.EFFECTIVEDATE, l.MATURITYDATE),
                                         collateralCode = cc.COLLATERALCODE,
                                         captureDate = cm.DATETIMECREATED


                                     }).Distinct().ToList().Select(x =>
                                     {


                                         //  var getCollateralCustomerID = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANID == x.loanId).Select(z => z.COLLATERALCUSTOMERID).FirstOrDefault();

                                         //    x.remarks = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(z => z.COLLATERALCUSTOMERID == getCollateralCustomerID).Select(m => m.REMARK).FirstOrDefault();
                                         //  x.subHead = misInfo.Where(f => f.FIELD1 == x.).FirstOrDefault().subHead;
                                         x.collateralType = LoanCollateralType(x.loanId, LoanSystemTypeEnum.TermDisbursedFacility);

                                         //var businessUnitName = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD8).FirstOrDefault();

                                         //if (businessUnitName != null)
                                         //{
                                         //    x.businessUnit = businessUnitName;
                                         //}
                                         //else if (businessUnitName != null)
                                         //{
                                         //    x.businessUnit = "";
                                         //}

                                         //var buCode = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD4).FirstOrDefault();

                                         //if (buCode != null)
                                         //{
                                         //    x.buCode = buCode;
                                         //}
                                         //else if (buCode != null)
                                         //{
                                         //    x.buCode = "N/A";
                                         //}

                                         //var groupName = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD7).FirstOrDefault();
                                         //if (groupName != null)
                                         //{
                                         //    x.buDescription = groupName;
                                         //}
                                         //else if (groupName == null)
                                         //{
                                         //    x.buDescription = "N/A";
                                         //}

                                         //var groupCode = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD3).FirstOrDefault();
                                         //if (groupCode != null)
                                         //{
                                         //    x.groupCode = groupCode;
                                         //}
                                         //else if (groupCode == null)
                                         //{
                                         //    x.groupCode = "N/A";
                                         //}

                                         //var teamName = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD6).FirstOrDefault();
                                         //if (teamName != null)
                                         //{
                                         //    x.teamDescription = teamName;

                                         //}
                                         //else if (string.IsNullOrEmpty(teamName))
                                         //{
                                         //    x.teamDescription = "N/A";
                                         //}

                                         //var teamCode = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD2).FirstOrDefault();
                                         //if (teamCode != null)
                                         //{
                                         //    x.TeamCode = teamCode;

                                         //}
                                         //else if (string.IsNullOrEmpty(teamCode))
                                         //{
                                         //    x.TeamCode = "N/A";
                                         //}

                                         //var deskName = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD5).FirstOrDefault();

                                         //if (deskName != null)
                                         //{
                                         //    x.deskDescription = deskName;

                                         //}
                                         //else if (string.IsNullOrEmpty(deskName))
                                         //{
                                         //    x.deskDescription = "N/A";
                                         //}


                                         //var deskCode = misInfo.Where(z => z.FIELD1 == x.misCode).Select(z => z.FIELD1).FirstOrDefault();

                                         //if (deskCode != null)
                                         //{
                                         //    x.deskCode = deskCode;

                                         //}
                                         //else if (string.IsNullOrEmpty(deskCode))
                                         //{
                                         //    x.deskCode = "N/A";
                                         //}

                                         return x;
                                     }).ToList();

                    return reportData;
                }

            }
        }

        public List<DisbursedFacilityCollateralViewModel> DisbursedFacilityCollateralReport(int classification, int productId)
        {

            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {


                if (classification == (int)CollateralTypeEnum.Gaurantee)
                {
                    var reportData = (
                                                         from l in context.TBL_LOAN
                                                         join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                                         join sta in context.TBL_STAFF on l.RELATIONSHIPOFFICERID equals sta.STAFFID
                                                         join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                                         join ll in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ll.LOANAPPLICATIONDETAILID
                                                         join cm in context.TBL_LOAN_APPLICATION_COLLATERL on ll.LOANAPPLICATIONID equals cm.LOANAPPLICATIONID
                                                         join cc in context.TBL_COLLATERAL_CUSTOMER on cm.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                                                         join cg in context.TBL_COLLATERAL_GAURANTEE on cc.COLLATERALCUSTOMERID equals cg.COLLATERALCUSTOMERID
                                                         join ct in context.TBL_COLLATERAL_TYPE on cc.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                                                         where
                                                         cc.COLLATERALTYPEID == classification
                                                         && l.PRODUCTID == productId
                                                         select new DisbursedFacilityCollateralViewModel
                                                         {
                                                             customerID = c.CUSTOMERCODE,
                                                             customerName = c.FIRSTNAME + " " + c.MIDDLENAME,
                                                             settlementAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == l.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                                             refNo = l.LOANREFERENCENUMBER,
                                                             productName = context.TBL_PRODUCT.Where(x => x.PRODUCTID == l.PRODUCTID).Select(x => x.PRODUCTNAME).FirstOrDefault(),
                                                             adjFacilityType = (from a in context.TBL_PRODUCT join b in context.TBL_PRODUCT_TYPE on a.PRODUCTTYPEID equals b.PRODUCTTYPEID where a.PRODUCTID == l.PRODUCTID select b.PRODUCTTYPENAME).FirstOrDefault(),
                                                             currencyType = (from a in context.TBL_LOAN join b in context.TBL_CURRENCY on a.CURRENCYID equals b.CURRENCYID where a.PRODUCTID == l.PRODUCTID select b.CURRENCYNAME).FirstOrDefault(),
                                                             loanAmountLCY = l.PRINCIPALAMOUNT,
                                                             totalExposureLCY = l.PASTDUEPRINCIPAL + l.PASTDUEINTEREST + l.INTERESTONPASTDUEINTEREST + l.INTERESTONPASTDUEPRINCIPAL + l.OUTSTANDINGINTEREST + l.OUTSTANDINGPRINCIPAL,
                                                             bookingDate = l.BOOKINGDATE,
                                                             valueDate = l.DISBURSEDATE,
                                                             maturityDate = l.MATURITYDATE,
                                                             rate = l.INTERESTRATE,
                                                             tenor = (int)DbFunctions.DiffDays(l.MATURITYDATE, l.EFFECTIVEDATE),
                                                             collateralType = ct.COLLATERALTYPENAME,
                                                             collateralDetails = ct.DETAILS,
                                                             guarantorName = cg.FIRSTNAME + " " + " " + cg.MIDDLENAME + " " + cg.LASTNAME,
                                                             guarantorPhoneNo = cg.PHONENUMBER1,
                                                             guarantorEmailAddress = cg.EMAILADDRESS,
                                                             guarantorBVN = cg.BVN,
                                                             guarantorHomeAddress = cg.GUARANTORADDRESS,
                                                             guarantorOfficeAddress = cg.GUARANTORADDRESS,
                                                             accountOfficerName = sta.FIRSTNAME + " " + sta.LASTNAME,
                                                             misCode = sta.MISCODE
                                                         }).Distinct().ToList().Select(x =>
                                                         {
                                                             x.collateralType = LoanCollateralType(x.loanId, LoanSystemTypeEnum.TermDisbursedFacility);
                                                             return x;
                                                         }).ToList();
                    foreach (var data in reportData)
                    {
                        data.teamName = stagecontext.STG_TEAM.Where(x => x.ACCOUNTOFFICERCODE == data.misCode).Select(x => x.TEAMNAME).FirstOrDefault();
                        data.regionName = stagecontext.STG_TEAM.Where(x => x.ACCOUNTOFFICERCODE == data.misCode).Select(x => x.REGIONNAME).FirstOrDefault();
                        data.groupName = stagecontext.STG_TEAM.Where(x => x.ACCOUNTOFFICERCODE == data.misCode).Select(x => x.UNITNAME).FirstOrDefault();
                        data.divisionName = stagecontext.STG_TEAM.Where(x => x.ACCOUNTOFFICERCODE == data.misCode).Select(x => x.DIVISIONNAME).FirstOrDefault();
                    }

                    return reportData;
                }
                else
                {

                    var reportData = (
                                    from l in context.TBL_LOAN
                                    join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                    join sta in context.TBL_STAFF on l.RELATIONSHIPOFFICERID equals sta.STAFFID
                                    join b in context.TBL_BRANCH on l.BRANCHID equals b.BRANCHID
                                    join ll in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals ll.LOANAPPLICATIONDETAILID
                                    join cm in context.TBL_LOAN_APPLICATION_COLLATERL on ll.LOANAPPLICATIONID equals cm.LOANAPPLICATIONID
                                    join cc in context.TBL_COLLATERAL_CUSTOMER on cm.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                                    join ct in context.TBL_COLLATERAL_TYPE on cc.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                                    where
                                    cc.COLLATERALTYPEID == classification
                                    && l.PRODUCTID == productId

                                    select new DisbursedFacilityCollateralViewModel
                                    {
                                        customerID = c.CUSTOMERCODE,
                                        customerName = c.FIRSTNAME + " " + c.MIDDLENAME,
                                        settlementAccount = context.TBL_CASA.Where(x => x.CASAACCOUNTID == l.CASAACCOUNTID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                                        refNo = l.LOANREFERENCENUMBER,
                                        productName = context.TBL_PRODUCT.Where(x => x.PRODUCTID == l.PRODUCTID).Select(x => x.PRODUCTNAME).FirstOrDefault(),
                                        adjFacilityType = (from a in context.TBL_PRODUCT join b in context.TBL_PRODUCT_TYPE on a.PRODUCTTYPEID equals b.PRODUCTTYPEID where a.PRODUCTID == l.PRODUCTID select b.PRODUCTTYPENAME).FirstOrDefault(),
                                        currencyType = (from a in context.TBL_LOAN join b in context.TBL_CURRENCY on a.CURRENCYID equals b.CURRENCYID where a.PRODUCTID == l.PRODUCTID select b.CURRENCYNAME).FirstOrDefault(),
                                        loanAmountLCY = l.PRINCIPALAMOUNT,
                                        totalExposureLCY = l.PASTDUEPRINCIPAL + l.PASTDUEINTEREST + l.INTERESTONPASTDUEINTEREST + l.INTERESTONPASTDUEPRINCIPAL + l.OUTSTANDINGINTEREST + l.OUTSTANDINGPRINCIPAL,
                                        bookingDate = l.BOOKINGDATE,
                                        valueDate = l.DISBURSEDATE,
                                        maturityDate = l.MATURITYDATE,
                                        rate = l.INTERESTRATE,
                                        tenor = (int)DbFunctions.DiffDays(l.MATURITYDATE, l.EFFECTIVEDATE),
                                        collateralType = ct.COLLATERALTYPENAME,
                                        collateralDetails = ct.DETAILS,
                                        guarantorName = "",
                                        guarantorPhoneNo = "",
                                        guarantorEmailAddress = "",
                                        guarantorBVN = "",
                                        guarantorHomeAddress = "",
                                        guarantorOfficeAddress = "",
                                        accountOfficerName = sta.FIRSTNAME + " " + sta.LASTNAME,
                                        misCode = sta.MISCODE
                                    }).Distinct().ToList().Select(x =>
                                    {
                                        x.collateralType = LoanCollateralType(x.loanId, LoanSystemTypeEnum.TermDisbursedFacility);
                                        return x;
                                    }).ToList();
                    foreach (var data in reportData)
                    {
                        data.teamName = stagecontext.STG_TEAM.Where(x => x.ACCOUNTOFFICERCODE == data.misCode).Select(x => x.TEAMNAME).FirstOrDefault();
                        data.regionName = stagecontext.STG_TEAM.Where(x => x.ACCOUNTOFFICERCODE == data.misCode).Select(x => x.REGIONNAME).FirstOrDefault();
                        data.groupName = stagecontext.STG_TEAM.Where(x => x.ACCOUNTOFFICERCODE == data.misCode).Select(x => x.UNITNAME).FirstOrDefault();
                        data.divisionName = stagecontext.STG_TEAM.Where(x => x.ACCOUNTOFFICERCODE == data.misCode).Select(x => x.DIVISIONNAME).FirstOrDefault();
                    }


                    return reportData;

                }
            }

        }


        public List<CollateralRegisterReportViewModel> CollateralRegister(DateTime startDate, DateTime endDate, int companyid, short? branchId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var data = (from l in context.TBL_COLLATERAL_CUSTOMER
                            join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                            join sta in context.TBL_STAFF on l.CREATEDBY equals sta.STAFFID
                            join b in context.TBL_BRANCH on c.BRANCHID equals b.BRANCHID
                            join ct in context.TBL_COLLATERAL_TYPE on l.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                            join cs in context.TBL_COLLATERAL_TYPE_SUB on l.COLLATERALSUBTYPEID equals cs.COLLATERALSUBTYPEID
                            where (DbFunctions.TruncateTime(l.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                            DbFunctions.TruncateTime(l.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))

                            orderby l.DATETIMECREATED descending
                            select new CollateralRegisterReportViewModel
                            {
                                businessUnitId = c.BUSINESSUNTID,
                                customerCode = c.CUSTOMERCODE,
                                collateralCustomerID = l.COLLATERALCUSTOMERID,
                                collateralTypeId = ct.COLLATERALTYPEID,
                                guarantorName = context.TBL_COLLATERAL_GAURANTEE.Where(g => g.COLLATERALCUSTOMERID == l.COLLATERALCUSTOMERID).Select(g => g.FIRSTNAME + " " + g.MIDDLENAME + " " + g.LASTNAME).FirstOrDefault(),
                                collateralSummary = l.COLLATERALSUMMARY,
                                collateralForm = ct.COLLATERALCLASSIFICATIONID == 1 ? "Tangible Related" : ct.COLLATERALCLASSIFICATIONID == 2 ? "Comfort Related" : "",
                                customerID = c.CUSTOMERCODE,
                                customername = c.FIRSTNAME + " " + c.LASTNAME,
                                collateralType = cs.COLLATERALSUBTYPENAME.ToLower() == "others" ? l.COLLATERALSUMMARY : cs.COLLATERALSUBTYPENAME,
                                rmCode = sta.STAFFCODE,
                                rmName = sta.FIRSTNAME + " " + " " + sta.MIDDLENAME + " " + " " + sta.LASTNAME,
                                misCode = sta.MISCODE,
                                collateralCode = l.COLLATERALCODE,
                                collateralDescription = l.COLLATERALSUMMARY,
                                collateralValue = l.COLLATERALVALUE,
                                collCustomerId = c.CUSTOMERID
                            }).ToList();
                if (data.Count() > 0)
                {
                    foreach (var k in data)
                    {
                        var insurancePolicy = context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(x => x.COLLATERALCUSTOMERID == k.collateralCustomerID)?.FirstOrDefault();
                        if (insurancePolicy != null && insurancePolicy.INSURANCEPOLICYTYPEID > 0)
                        {
                            k.insurancePolicyType = context.TBL_INSURANCE_POLICY_TYPE.Where(xx => xx.POLICYTYPEID == insurancePolicy.INSURANCEPOLICYTYPEID).Select(xx => xx.DESCRIPTION)?.FirstOrDefault();
                        }
                        else { k.insurancePolicyType = insurancePolicy?.OTHERINSURANCEPOLICYTYPE; }

                        if (insurancePolicy != null && insurancePolicy.INSURANCECOMPANYID > 0)
                        {
                            k.insuranceCompany = context.TBL_INSURANCE_COMPANY.Where(xx => xx.INSURANCECOMPANYID == insurancePolicy.INSURANCECOMPANYID)?.Select(xx => xx.COMPANYNAME).FirstOrDefault();
                        }
                        else { k.insuranceCompany = insurancePolicy?.OTHERINSURANCECOMPANY; }

                        k.dateOfInsurance = insurancePolicy?.INSURANCESTARTDATE;
                        k.expiryDate = insurancePolicy?.INSURANCEENDDATE;

                        var expo = (from a in context.TBL_LOAN_APPLICATION_COLLATERL join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID join c in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID where a.COLLATERALCUSTOMERID == k.collateralCustomerID select c).ToList();
                        if (expo.Count() > 0)
                        {
                            k.exposure = expo.Sum(x => x.TOTALEXPOSUREAMOUNT);
                        }
                        if (k.businessUnitId != null)
                        {
                            k.businessUnit = context.TBL_PROFILE_BUSINESS_UNIT.Where(f => f.BUSINESSUNITID == k.businessUnitId).Select(f => f.BUSINESSUNITNAME + " " + f.BUSINESSUNITSHORTCODE)?.FirstOrDefault();
                            k.groupDescription = context.TBL_PROFILE_BUSINESS_UNIT.Where(f => f.BUSINESSUNITID == k.businessUnitId).Select(f => f.BUSINESSUNITNAME + " " + f.BUSINESSUNITSHORTCODE)?.FirstOrDefault();
                        }

                        k.glSubheadCode = (from gl in context.TBL_CHART_OF_ACCOUNT
                                           join cst in context.TBL_CUSTOM_CHART_OF_ACCOUNT on gl.ACCOUNTCODE equals cst.PLACEHOLDERID
                                           join pr in context.TBL_PRODUCT on gl.GLACCOUNTID equals pr.PRINCIPALBALANCEGL
                                           select cst.ACCOUNTID)?.FirstOrDefault();

                        var visitation = context.TBL_COLLATERAL_VISITATION.Where(z => z.COLLATERALCUSTOMERID == k.collateralCustomerID).Select(o => o.VISITATIONDATE)?.FirstOrDefault();

                        if (visitation != null)
                        {
                            k.dateOfCollateralInspection = visitation;
                        }

                        if (k.collateralTypeId == (int)CollateralTypeEnum.Property)
                        {
                            var cim = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(x => x.COLLATERALCUSTOMERID == k.collateralCustomerID).FirstOrDefault();
                            if (cim != null)
                            {
                                k.nameOfValuer = context.TBL_ACCREDITEDCONSULTANT.Where(x => x.ACCREDITEDCONSULTANTID == cim.VALUERID).Select(o => o.FIRMNAME)?.FirstOrDefault();
                                k.perfectionStatus = cim?.TBL_COLLATERAL_PERFECTN_STAT?.PERFECTIONSTATUSNAME;
                                k.collateralValueOmv = cim.OPENMARKETVALUE ?? (decimal)0;
                                k.collateralValueEfsv = cim.FORCEDSALEVALUE ?? (decimal)0;

                                k.securityValue = cim.SECURITYVALUE;
                                k.collateralLocation = cim.PROPERTYADDRESS;
                                k.dateOfValuation = cim.LASTVALUATIONDATE;
                                k.valuerId = cim.VALUERID;
                                k.collateralCustomerID = cim.COLLATERALCUSTOMERID;
                                k.stc = cim.STAMPTOCOVER;
                            }
                        }

                        if (k.collateralTypeId == (int)CollateralTypeEnum.PlantAndMachinery)
                        {
                            var cim = context.TBL_COLLATERAL_PLANT_AND_EQUIP.Where(x => x.COLLATERALCUSTOMERID == k.collateralCustomerID).FirstOrDefault();
                            if (cim != null)
                            {

                                k.collateralValueOmv = k.collateralValue ?? (decimal)0;
                                k.collateralValueEfsv = cim?.REPLACEMENTVALUE ?? (decimal)0;
                            }
                        }

                        var proposedCol = context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.COLLATERALCUSTOMERID == k.collateralCustomerID).FirstOrDefault();
                        if (proposedCol != null)
                        {
                            decimal totalValue = 0;
                            decimal totalValue2 = 0;
                            decimal totalValue3 = 0;
                            var loanDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == proposedCol.LOANAPPLICATIONDETAILID).FirstOrDefault();
                            var termLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == proposedCol.LOANAPPLICATIONDETAILID).ToList();
                            var contingentLoan = context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == proposedCol.LOANAPPLICATIONDETAILID).ToList();
                            var revolvingLoan = context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == proposedCol.LOANAPPLICATIONDETAILID).ToList();
                            if (termLoan.Count() > 0)
                            {
                                totalValue = termLoan.Sum(x => x.PRINCIPALAMOUNT);
                            }
                            if (contingentLoan.Count() > 0)
                            {
                                totalValue2 = contingentLoan.Sum(x => x.CONTINGENTAMOUNT);
                            }
                            if (revolvingLoan.Count() > 0)
                            {
                                totalValue2 = revolvingLoan.Sum(x => x.OVERDRAFTLIMIT);
                            }
                            k.grossBalance = totalValue + totalValue2 + totalValue3;
                            k.approvedAmount = loanDetail?.APPROVEDAMOUNT ?? (decimal)0;
                            k.dateOfExpiration = loanDetail.EXPIRYDATE;

                            decimal? collateralVal = 0;
                            decimal? totalCollateralVal = 0;
                            if (loanDetail != null)
                            {
                                var proposedCols = context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.LOANAPPLICATIONDETAILID == loanDetail.LOANAPPLICATIONDETAILID).Select(x => x.COLLATERALCUSTOMERID).ToList();
                                if (proposedCols.Count() > 0)
                                {
                                    var totalCollaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => proposedCols.Contains(x.COLLATERALCUSTOMERID)).ToList();
                                    if (totalCollaterals.Count() > 0)
                                    {
                                        foreach (var v in totalCollaterals)
                                        {
                                            if (k.collateralTypeId == (int)CollateralTypeEnum.Property)
                                            {
                                                var cim = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(x => x.COLLATERALCUSTOMERID == v.COLLATERALCUSTOMERID).FirstOrDefault();
                                                if (cim != null)
                                                {
                                                    collateralVal = cim.FORCEDSALEVALUE ?? (decimal)0;
                                                }
                                                else
                                                {
                                                    collateralVal = v?.COLLATERALVALUE ?? (decimal)0;
                                                }
                                            }
                                            totalCollateralVal = totalCollateralVal + collateralVal;
                                        }
                                    }
                                }
                            }
                            k.collateralCoverage = (totalCollateralVal > 0 && k.grossBalance > 0) ? (totalCollateralVal / k.grossBalance) : 0;
                        }
                        k.accountNumber = context.TBL_CASA.Where(x => x.CUSTOMERID == k.collCustomerId).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault();

                    }
                }
                return data;

            }
        }


        public List<CollateralAdequacyViewModel> CollateralAdequacy(DateTime startDate, DateTime endDate, int companyid, short? branchId)
        {
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {

                var collateralAdequacyReportData = (from l in context.TBL_LOAN_APPLICATION_DETAIL
                                                    join cm in context.TBL_LOAN_APPLICATION_COLLATERL on l.LOANAPPLICATIONID equals cm.LOANAPPLICATIONID
                                                    join ccu in context.TBL_COLLATERAL_CUSTOMER on cm.COLLATERALCUSTOMERID equals ccu.COLLATERALCUSTOMERID
                                                    join ct in context.TBL_COLLATERAL_TYPE on ccu.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                                                    join c in context.TBL_CUSTOMER on cm.CUSTOMERID equals c.CUSTOMERID
                                                    join cs in context.TBL_COLLATERAL_TYPE_SUB on ccu.COLLATERALSUBTYPEID equals cs.COLLATERALSUBTYPEID
                                                    where (DbFunctions.TruncateTime(ccu.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                                    DbFunctions.TruncateTime(ccu.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate))
                                                    && ccu.COMPANYID == companyid
                                                    && (c.BRANCHID == branchId || branchId == null || branchId == 0)
                                                    orderby ccu.DATETIMECREATED descending

                                                    select new CollateralAdequacyViewModel
                                                    {
                                                        collateralVal = ccu.COLLATERALVALUE,
                                                        currencyId = ccu.CURRENCYID,
                                                        collateralTypeId = ct.COLLATERALTYPEID,
                                                        collateralCustomerID = ccu.COLLATERALCUSTOMERID,
                                                        cCustomerId = c.CUSTOMERID,
                                                        productId = l.PROPOSEDPRODUCTID,
                                                        businessUnitId = c.BUSINESSUNTID,
                                                        createdBy = l.CREATEDBY,
                                                        accountOfficer = context.TBL_STAFF.Where(o => o.STAFFID == ccu.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                                                        currency = context.TBL_CURRENCY.Where(x => x.CURRENCYID == ccu.CURRENCYID).Select(x => x.CURRENCYCODE).FirstOrDefault(),
                                                        customerId = c.CUSTOMERCODE,
                                                        customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                                                        collateralType = cs.COLLATERALSUBTYPENAME.ToLower() == "others" ? ccu.COLLATERALSUMMARY : cs.COLLATERALSUBTYPENAME,
                                                    }).ToList();


                foreach (var k in collateralAdequacyReportData)
                {
                    if (k.collateralTypeId == (int)CollateralTypeEnum.Property)
                    {
                        var cim = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(x => x.COLLATERALCUSTOMERID == k.collateralCustomerID).FirstOrDefault();
                        if (cim != null)
                        {
                            k.collateralValue = cim.FORCEDSALEVALUE ?? (decimal)0;

                        }
                        else
                        {
                            k.collateralValue = k.collateralVal;
                        }
                    }
                    var ao = context.TBL_STAFF.Find(k.createdBy);
                    if (ao != null && ao.SUPERVISOR_STAFFID != null)
                    {
                        k.relationshipManager = context.TBL_STAFF.Where(o => o.STAFFID == ao.SUPERVISOR_STAFFID).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME)?.FirstOrDefault();
                        var rm = context.TBL_STAFF.Where(o => o.STAFFID == ao.SUPERVISOR_STAFFID)?.FirstOrDefault();

                        if (rm != null && rm.SUPERVISOR_STAFFID != null)
                        {
                            var zh = context.TBL_STAFF.Where(o => o.STAFFID == rm.SUPERVISOR_STAFFID)?.FirstOrDefault();

                            if (zh != null && zh.SUPERVISOR_STAFFID != null)
                            {
                                var gh = context.TBL_STAFF.Where(o => o.STAFFID == zh.SUPERVISOR_STAFFID)?.FirstOrDefault();
                                k.groupHead = gh?.FIRSTNAME + " " + gh?.LASTNAME + " " + gh?.MIDDLENAME;
                            }
                        }
                    }
                    if (k.businessUnitId > 0)
                    {
                        k.sbu = context.TBL_PROFILE_BUSINESS_UNIT.Where(f => f.BUSINESSUNITID == k.businessUnitId).Select(f => f.BUSINESSUNITNAME + " " + f.BUSINESSUNITSHORTCODE)?.FirstOrDefault();
                    }

                    //============================================
                    var proposedCol = context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.COLLATERALCUSTOMERID == k.collateralCustomerID).FirstOrDefault();
                    decimal allExposures = 0;

                    if (proposedCol != null)
                    {
                        decimal totalValue = 0;
                        decimal totalValue2 = 0;
                        decimal totalValue3 = 0;
                        //var loanDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == proposedCol.LOANAPPLICATIONDETAILID).FirstOrDefault();
                        var termLoan = context.TBL_LOAN.Where(x => x.LOANAPPLICATIONDETAILID == proposedCol.LOANAPPLICATIONDETAILID).ToList();
                        var contingentLoan = context.TBL_LOAN_CONTINGENT.Where(x => x.LOANAPPLICATIONDETAILID == proposedCol.LOANAPPLICATIONDETAILID).ToList();
                        var revolvingLoan = context.TBL_LOAN_REVOLVING.Where(x => x.LOANAPPLICATIONDETAILID == proposedCol.LOANAPPLICATIONDETAILID).ToList();
                        if (termLoan.Count() > 0)
                        {
                            totalValue = termLoan.Sum(x => x.PRINCIPALAMOUNT);
                        }
                        if (contingentLoan.Count() > 0)
                        {
                            totalValue2 = contingentLoan.Sum(x => x.CONTINGENTAMOUNT);
                        }
                        if (revolvingLoan.Count() > 0)
                        {
                            totalValue2 = revolvingLoan.Sum(x => x.OVERDRAFTLIMIT);
                        }
                        allExposures = totalValue + totalValue2 + totalValue3;
                    }
                    //==================================
                    //var allExposures = (from a in context.TBL_LOAN_APPLICATION_COLLATERL
                    //                        join b in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals b.COLLATERALCUSTOMERID
                    //                        join o in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals o.LOANAPPLICATIONDETAILID
                    //                        join pp in context.TBL_PRODUCT on o.PROPOSEDPRODUCTID equals pp.PRODUCTID
                    //                        join pt in context.TBL_PRODUCT_TYPE on pp.PRODUCTTYPEID equals pt.PRODUCTTYPEID
                    //                        where a.CUSTOMERID == k.cCustomerId && a.COLLATERALCUSTOMERID == k.collateralCustomerID && pt.PRODUCTTYPEID != (int)LoanProductTypeEnum.ContingentLiability
                    //                        select a).ToList();

                    if (k.currencyId > 0)
                    {
                        k.collateralCurrency = context.TBL_CURRENCY.Where(x => x.CURRENCYID == k.currencyId).Select(x => x.CURRENCYCODE)?.FirstOrDefault();
                    }
                    if (allExposures > 0)
                    {
                        k.totalDirectExposure = allExposures;
                    }
                    var tangibleCollaterals = (from z in context.TBL_COLLATERAL_CUSTOMER
                                               join to in context.TBL_COLLATERAL_TYPE on z.COLLATERALTYPEID equals to.COLLATERALTYPEID
                                               join pp in context.TBL_COLLATERAL_IMMOVE_PROPERTY on z.COLLATERALCUSTOMERID equals pp.COLLATERALCUSTOMERID
                                               where z.CUSTOMERID == k.cCustomerId && z.COLLATERALCUSTOMERID == k.collateralCustomerID && to.COLLATERALTYPEID == k.collateralTypeId && to.COLLATERALCLASSIFICATIONID == (int)CollateralClassificationEnum.Tangible
                                               select pp).ToList();
                    if (tangibleCollaterals.Count() > 0)
                    {
                        k.totalTangibleCollateral = tangibleCollaterals.Sum(x => x.FORCEDSALEVALUE);
                    }
                    var intangibleCollaterals = (from z in context.TBL_COLLATERAL_CUSTOMER
                                                 join to in context.TBL_COLLATERAL_TYPE on z.COLLATERALTYPEID equals to.COLLATERALTYPEID
                                                 where z.CUSTOMERID == k.cCustomerId && z.COLLATERALCUSTOMERID == k.collateralCustomerID && to.COLLATERALTYPEID == k.collateralTypeId && to.COLLATERALCLASSIFICATIONID == (int)CollateralClassificationEnum.Intangible
                                                 select z).ToList();
                    if (intangibleCollaterals.Count() > 0)
                    {
                        k.totalIntangibleCollateral = intangibleCollaterals.Sum(x => x.COLLATERALVALUE);
                    }

                    if (k.totalTangibleCollateral > 0 && allExposures > 0)
                    {
                        k.percentageTangibleCoverage = (k.totalTangibleCollateral / allExposures);
                    }
                    if (k.totalIntangibleCollateral > 0 && allExposures > 0)
                    {
                        k.percentageIntangibleCoverage = (k.totalIntangibleCollateral / allExposures);
                    }
                    k.totalPercentageCoverage = k.percentageTangibleCoverage + k.percentageIntangibleCoverage;
                }
                return collateralAdequacyReportData;
            }

        }


        public IEnumerable<OriginalDocumentReleaseViewModel> SecurityReleaseReport(DateTime startDate, DateTime endDate)
        {
            var data = new List<OriginalDocumentReleaseViewModel>();

            FinTrakBankingContext context = new FinTrakBankingContext();

            data = (from x in context.TBL_ORIGINAL_DOCUMENT_RELEASE
                    join l in context.TBL_LOAN_APPLICATION on x.COLLATERALCUSTOMERID equals l.LOANAPPLICATIONID
                    join a in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                    join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                    where x.DELETED == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                    && DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                            DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)

                    select new OriginalDocumentReleaseViewModel
                    {
                        originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                        loanApplicationId = l.LOANAPPLICATIONID,
                        description = a.LOANPURPOSE,
                        approvalStatusId = (short)x.APPROVALSTATUSID,
                        applicationReferenceNumber = l.APPLICATIONREFERENCENUMBER,
                        referenceNumber = l.COLLATERALDETAIL,
                        dateTimeCreated = x.DATETIMECREATED,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        customerId = c.CUSTOMERID,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        applicationDate = l.APPLICATIONDATE,
                        applicationAmount = l.APPLICATIONAMOUNT,
                        interestRate = l.INTERESTRATE,
                        approvalDate = x.APPROVALDATE,
                        productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        createdByName = context.TBL_STAFF.Where(o => o.STAFFID == x.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),

                    })
               .ToList();
            return data;
        }
        public IEnumerable<OriginalDocumentReleaseViewModel> InsuranceSpoolReport(DateTime startDate, DateTime endDate, int documentTypeId)
        {
            var data = new List<OriginalDocumentReleaseViewModel>();

            FinTrakBankingContext context = new FinTrakBankingContext();

            data = (from us in documentsContext.TBL_DOCUMENT_USAGE
                    join up in documentsContext.TBL_DOCUMENT_UPLOAD on us.DOCUMENTUPLOADID equals up.DOCUMENTUPLOADID
                    join t in documentsContext.TBL_DOCUMENT_TYPE on up.DOCUMENTTYPEID equals t.DOCUMENTTYPEID
                    where us.DELETED == false && t.DOCUMENTTYPEID == (int)DocumentTypeEnum.InsurancePolicy
                    && DbFunctions.TruncateTime(us.DATETIMECREATED) >= DbFunctions.TruncateTime(startDate) &&
                                          DbFunctions.TruncateTime(us.DATETIMECREATED) <= DbFunctions.TruncateTime(endDate)

                    select new OriginalDocumentReleaseViewModel
                    {
                        applicationReferenceNumber = us.TARGETREFERENCENUMBER,
                        documentTypeName = t.DOCUMENTTYPENAME,
                        fileName = up.FILENAME,
                    })
               .ToList();
            foreach (var d in data)
            {
                var loanApplication = context.TBL_LOAN_APPLICATION.Where(x => x.APPLICATIONREFERENCENUMBER == d.applicationReferenceNumber).FirstOrDefault();
                var loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == loanApplication.LOANAPPLICATIONID).FirstOrDefault();
                var customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == loanApplicationDetail.CUSTOMERID).FirstOrDefault();
                var currency = context.TBL_CURRENCY.Where(x => x.CURRENCYID == loanApplicationDetail.CURRENCYID).FirstOrDefault();
                var bookingRequest = context.TBL_LOAN_BOOKING_REQUEST.Where(x => x.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID).FirstOrDefault();
                if (bookingRequest != null)
                {
                    d.customerName = customer.LASTNAME + " " + customer.FIRSTNAME + " " + customer.MIDDLENAME;
                    d.facilityAmount = bookingRequest.AMOUNT_REQUESTED;
                    d.currency = currency.CURRENCYCODE;
                    d.drawdownInitiationDate = bookingRequest.DATETIMECREATED;
                }
                else
                {
                    d.customerName = customer.LASTNAME + " " + customer.FIRSTNAME + " " + customer.MIDDLENAME;
                    d.facilityAmount = (decimal)0.00;
                    d.currency = currency.CURRENCYCODE;
                    d.drawdownInitiationDate = loanApplicationDetail.DATETIMECREATED;
                }

            }

            return data;
        }

        public IEnumerable<OutStandingDocumentViewModel> GetOutstandingDocumentDeferralList()
        {
            var data = new List<OutStandingDocumentViewModel>();

            FinTrakBankingContext context = new FinTrakBankingContext();

            data = (from a in context.TBL_LOAN_CONDITION_DEFERRAL
                    join b in context.TBL_LOAN_CONDITION_PRECEDENT on a.LOANCONDITIONID equals b.LOANCONDITIONID
                    join c in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                    where a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                    && (DbFunctions.TruncateTime(a.DEFEREDDATEONFINALAPPROVAL) <= DbFunctions.TruncateTime(DateTime.Now)
                    || DbFunctions.TruncateTime(a.DEFERREDDATE) <= DbFunctions.TruncateTime(DateTime.Now))
                    && b.ISDOCUMENT == true

                    select new OutStandingDocumentViewModel
                    {
                        reference = context.TBL_LOAN_APPLICATION.Where(s => s.LOANAPPLICATIONID == c.LOANAPPLICATIONID).Select(s => s.APPLICATIONREFERENCENUMBER + "-" + a.CHECKLISTDEFERRALID).FirstOrDefault(),
                        condition = b.CONDITION,
                        facilityAmount = c.APPROVEDAMOUNT,
                        facilityType = context.TBL_PRODUCT.Where(p => p.PRODUCTID == c.APPROVEDPRODUCTID).Select(p => p.PRODUCTNAME).FirstOrDefault(),
                        createdBy = a.CREATEDBY,
                        customerId = c.CUSTOMERID,
                        deferredDate = a.DEFERREDDATE,
                        dateOnFinalApproval = a.DEFEREDDATEONFINALAPPROVAL,
                        accountOfficerName = context.TBL_STAFF.Where(s => s.STAFFID == a.CREATEDBY).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault(),
                        customerCode = context.TBL_CUSTOMER.Where(p => p.CUSTOMERID == c.CUSTOMERID).Select(p => p.CUSTOMERCODE).FirstOrDefault(),
                        customerName = context.TBL_CUSTOMER.Where(s => s.CUSTOMERID == c.CUSTOMERID).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault(),
                    }).ToList();

            foreach (var d in data)
            {
                var ao = context.TBL_STAFF.Where(x => x.STAFFID == d.createdBy).FirstOrDefault();
                var cus = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == d.customerId).FirstOrDefault();

                if (ao != null)
                {
                    var rm = context.TBL_STAFF.Where(x => x.STAFFID == ao.SUPERVISOR_STAFFID).FirstOrDefault();
                    d.relationshipManager = rm?.FIRSTNAME + " " + rm?.MIDDLENAME + " " + rm?.LASTNAME;
                    if (rm != null)
                    {
                        var zh = context.TBL_STAFF.Where(x => x.STAFFID == rm.SUPERVISOR_STAFFID).FirstOrDefault();
                        if (zh != null)
                        {
                            var gh = context.TBL_STAFF.Where(x => x.STAFFID == zh.SUPERVISOR_STAFFID).FirstOrDefault();
                            d.groupHead = gh?.FIRSTNAME + " " + gh?.MIDDLENAME + " " + gh?.LASTNAME;
                        }
                    }
                }

                d.sbu = context.TBL_PROFILE_BUSINESS_UNIT.Where(x => x.BUSINESSUNITID == cus.BUSINESSUNTID).Select(x => x.BUSINESSUNITNAME + "-" + x.BUSINESSUNITSHORTCODE).FirstOrDefault();
                if (d.dateOnFinalApproval != null)
                {
                    d.numberOfDays = (int)(DateTime.Now - (DateTime)d.dateOnFinalApproval).TotalDays;
                }
                else
                {
                    d.numberOfDays = (int)(DateTime.Now - (DateTime)d.deferredDate).TotalDays;
                }

            }

            return data;
        }

        public List<AvailmentUtilizationTicketViewModel> AvailmentUtilizationTicket(int LoanApplicationDetId)
        {
            try
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var Loan = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(x => x.LOANAPPLICATIONDETAILID == LoanApplicationDetId);
                    var CustomerId = Loan.CUSTOMERID;

                    var data = (from l in context.TBL_LOAN
                                join b in context.TBL_LOAN_BOOKING_REQUEST on l.LOAN_BOOKING_REQUESTID equals b.LOAN_BOOKING_REQUESTID
                                join d in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                join c in context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                                where c.CUSTOMERID == CustomerId && l.ISDISBURSED == true

                                orderby l.DISBURSEDATE

                                select new
                                {
                                    // fetch from dataset
                                    customerId = c.CUSTOMERID,
                                    customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                    location = context.TBL_CUSTOMER_ADDRESS.FirstOrDefault(c => c.CUSTOMERID == CustomerId).ADDRESS,
                                    sector = context.TBL_SECTOR.FirstOrDefault(x => x.SECTORID == c.SUBSECTORID).NAME,
                                    transactionDate = l.DISBURSEDATE,
                                    approvedAmount = d.APPROVEDAMOUNT,
                                    classification = "Perfroming",
                                    facilityType = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == l.PRODUCTID).PRODUCTNAME,
                                    disbursedAmount = d.APPROVEDAMOUNT,
                                    Purpose = d.LOANPURPOSE,
                                    InterestRate = d.APPROVEDINTERESTRATE,
                                    Tenor = d.APPROVEDTENOR,
                                    EffectiveDate = d.EFFECTIVEDATE,
                                    //totalDisbursedAmount = d.APPROVEDAMOUNT,

                                }).ToList();

                    var Product = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == Loan.APPROVEDPRODUCTID).PRODUCTNAME;
                    var Disbursed = data.Select(x => x.disbursedAmount).ToList().Sum();
                    var Purpose = Loan.LOANPURPOSE;
                    var Interest = Loan.APPROVEDINTERESTRATE;
                    var Tenor = Loan.APPROVEDTENOR;
                    var EffectiveDate = Loan.EFFECTIVEDATE;
                    var OtherFees = 0.0;
                    var AmountTobeDisbursed = Loan.APPROVEDAMOUNT;
                    var TotalApproved = Disbursed + AmountTobeDisbursed;
                    var AmountinFull = NumberToWordsConverter.ConvertAmountToWords(AmountTobeDisbursed);
                   
                    var results = new List<AvailmentUtilizationTicketViewModel>();
                    foreach (var item in data)
                    {
                        var result = new AvailmentUtilizationTicketViewModel
                        {                    
                            customerId = LoanApplicationDetId,
                            customerName = item.customerName,
                            location = item.location,
                            sector = item.sector,
                            transactionDate = Convert.ToDateTime(item.transactionDate).ToString("dd-MM-yyyy"),
                            approvedAmount = string.Format("{0:0,0.0}", item.approvedAmount),
                            classification = item.classification,
                            facilityType = item.facilityType,
                            disbursedAmount = string.Format("{0:0,0.0}", item.disbursedAmount),
                            totalDisbursedAmount = string.Format("{0:0,0.0}", item.disbursedAmount),
                            purpose = Purpose,
                            interestRate = Interest,
                            amountInFull = AmountinFull,
                            tenor = Tenor,
                            effectiveDate = Convert.ToDateTime(EffectiveDate).ToString("dd-MMMM-yyyy"),
                            otherFees = OtherFees,
                            repaymentCycle = "Monthly",
                            amountToBeDisbursed = string.Format("{0:0,0.0}", AmountTobeDisbursed),
                            approvedTotal = string.Format("{0:0,0.0}", TotalApproved),
                            totalDisbursedAmountTotal = string.Format("{0:0,0.0}", Disbursed),
                            conditionPrecedent = Product
                            //conditionPrecedent = Condition

                        };
                        results.Add(result);
                    }

                    return results;

                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<ConditionViewModel> GetConditionPrecedents()
        {
            try
            {
                using (FinTrakBankingContext context = new FinTrakBankingContext())
                {
                    var data =  context.TBL_NMRC_CONDITIONPRECEDENTS.OrderBy(x=> x.Id).ToList();
                    var ResultList = new List<ConditionViewModel>();
                    foreach (var item in data)
                    {
                        var result = new ConditionViewModel();
                        result.Conditions = item.ConditionPrecedent;
                        result.SN = item.Id;
                        ResultList.Add(result);

                    }
                    return ResultList;
                }
            }
            catch (Exception ex)
            {
                throw;
            }


        }
    }
}

