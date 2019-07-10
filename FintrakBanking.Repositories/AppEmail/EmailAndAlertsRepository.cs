using FintrakBanking.Common.CustomException;

using FintrakBanking.Common;
using FintrakBanking.Common.AlertMonitoring;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;

namespace FintrakBanking.Repositories.AppEmail
{
    public class EmailAndAlertsRepository : IEmailAndAlertsRepository
    {
        private FinTrakBankingContext context = new FinTrakBankingContext();
        private IAuditTrailRepository auditTrail;
        // private EmailHelpers emailHelpers;
        private IGeneralSetupRepository genSetup;
        private DateTime applDate;
        private IStaffRepository staffRepo;
        public string response = string.Empty;

        private readonly string supportEmail = ConfigurationManager.AppSettings["SupportEmailAddr"];

        public EmailAndAlertsRepository(
                FinTrakBankingContext _context,
                IAuditTrailRepository _auditTrail,
                //   EmailHelpers _emailHelpers,
                IGeneralSetupRepository _general,
                IStaffRepository _staffRepo
            )
        {
            context = _context;
            auditTrail = _auditTrail;
            // emailHelpers = _emailHelpers;
            genSetup = _general;
            staffRepo = _staffRepo;
        }

        #region Covenant Monitoring

        public void SendAlertsForCovenantsApproachingDueDateToRM(List<LoanCovenantDetailViewModel> loanDetails, string title, string carbonCopy)
        {
            try
            {
                //   var staffList = staffRepo.GetAllStaff().ToList();
                var staffList = context.TBL_STAFF.ToList();

                var dataList = loanDetails.Select(g => g.relationshipManagerId).ToList();

                staffList = staffList.Where(x => dataList.Contains(x.STAFFID)).ToList();

                string dataTable;

                foreach (var mailItem in staffList)
                {
                    string recipient = mailItem.EMAIL;

                    dataTable =
                        "<table><tr><th>Application Ref</th><th>Covenant Detail</th><th>Covenant Type</th>" +
                        "<th>Amount</th><th>Covenant Date</th><th>Due Date</th></tr>";

                    var mailList = loanDetails.Where(x => x.relationshipManagerId == mailItem.STAFFID).ToList();

                    foreach (var item in mailList)
                    {
                        dataTable = dataTable +
                             $"<tr><td>{item.loanRefNumber}</td><td>{item.covenantDetail}</td><td>{item.covenantTypeName}</td>" +
                             $"<td>{item.covenantAmount}</td><td>{item.covenantDate:d}</td><td>{item.dueDate:d}</td></tr>";

                        dataTable = dataTable + "</table>";
                    }

                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title; //" (REMINDER) - LOAN COVENANTS APPROACHING DUE DATE";

                    string messageContent = $"Dear {mailItem.FIRSTNAME + " " + mailItem.LASTNAME}, <br /><br />" +
                                     "This is to bring your attention the following loan covenants " +
                                     "which are approaching their due date for revaluation. <br /><br />" +
                                     $"{dataTable}";

                    string additionalRecipient = loanDetails.FirstOrDefault(x => x.relationshipOfficerId == mailItem.STAFFID).officerEmail;

                    var templateUrl = "~/EmailTemplates/Monitoring.html";

                    var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

                    var messageModel = new MessageLogViewModel()
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = (short)MessageStatusEnum.Pending,
                        MessageTypeId = (short)MessageTypeEnum.Email,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{additionalRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };

                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += response = " Covenants Approaching Due Date was logged successfully, ";
                    }
                    else
                        response += response = " Covenants Approaching Due Date looging has failed, ";


                    //  emailHelpers.SendMail(recipient, additionalRecipient, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        public void SendAlertsForCovenantsApproachingDueDateToMonitoringTeam(List<LoanCovenantDetailViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {

        }
        public void SendAlertsForCovenantsApproachingDueDate(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            var alertsetupForCovenantsApproachingDueDate = alertSetups.Where(x => x.MONITORING_ITEMID == (int)AlertMessageEnum.CovenantsInsuranceApproachingDueDate).FirstOrDefault();

            // var currentDate = context.TBL_FINANCECURRENTDATE.Select(x=>x.CURRENTDATE).FirstOrDefault();
            var currentDate = DateTime.Now;

            var loanDetails = (from a in context.TBL_LOAN_COVENANT_DETAIL
                               join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                               //join c in context.TBL_STAFF on b.RELATIONSHIPMANAGERID equals c.STAFFID
                               join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                               join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                               join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals f.FREQUENCYTYPEID
                               join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                               where (int)DbFunctions.DiffDays(currentDate, a.NEXTCOVENANTDATE) <= alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD1
                               select new LoanCovenantDetailViewModel
                               {
                                   companyId = a.COMPANYID,
                                   covenantAmount = a.COVENANTAMOUNT,
                                   covenantDate = a.COVENANTDATE,
                                   dueDate = a.NEXTCOVENANTDATE,
                                   covenantDetail = a.COVENANTDETAIL,
                                   covenantTypeId = a.COVENANTTYPEID,
                                   covenantTypeName = g.COVENANTTYPENAME,
                                   frequencyTypeId = a.FREQUENCYTYPEID,
                                   frequencyTypeName = f.MODE,
                                   loanId = a.LOANID,
                                   loanRefNumber = e.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                   relationshipManager = d.FIRSTNAME + " " + d.LASTNAME,
                                   relationshipManagerId = d.STAFFID,
                                   managerEmail = d.EMAIL,
                                   relationshipOfficerId = d.STAFFID,
                                   relationshipOfficer = d.FIRSTNAME + " " + d.LASTNAME,
                                   officerEmail = d.EMAIL,
                                   notificationDuration = (int)DbFunctions.DiffDays(a.NEXTCOVENANTDATE, currentDate)    //(int)((TimeSpan)(currentDate.Date - a.NEXTCOVENANTDATE)).TotalDays
                               }).ToList();

            SendAlertsForCovenantsApproachingDueDateToRM(loanDetails, alertsetupForCovenantsApproachingDueDate.MESSAGE_TITLE, "");

            var escalationLevelOne = loanDetails.Where(x => x.notificationDuration <= alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD1).ToList();
            SendAlertsForCovenantsApproachingDueDateToMonitoringTeam(escalationLevelOne, alertsetupForCovenantsApproachingDueDate);

            var escalationLevelTwo = loanDetails.Where(x => x.notificationDuration <= alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD2).ToList();
            SendAlertsForCovenantsApproachingDueDateToMonitoringTeam(escalationLevelTwo, alertsetupForCovenantsApproachingDueDate);

            var escalationLevelThree = loanDetails.Where(x => x.notificationDuration <= alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD3).ToList();
            SendAlertsForCovenantsApproachingDueDateToMonitoringTeam(escalationLevelThree, alertsetupForCovenantsApproachingDueDate);

        }

        public void SendAlertsForCovenantsOverDue(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

            var data = (from a in context.TBL_LOAN_COVENANT_DETAIL
                        join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                        join c in context.TBL_STAFF on b.RELATIONSHIPMANAGERID equals c.STAFFID
                        join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                        join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                        join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals f.FREQUENCYTYPEID
                        join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                        where a.NEXTCOVENANTDATE.Value >= applDate
                        select new LoanCovenantDetailViewModel
                        {
                            companyId = a.COMPANYID,
                            covenantAmount = a.COVENANTAMOUNT,
                            covenantDate = a.COVENANTDATE,
                            dueDate = a.NEXTCOVENANTDATE,
                            covenantDetail = a.COVENANTDETAIL,
                            covenantTypeId = a.COVENANTTYPEID,
                            covenantTypeName = g.COVENANTTYPENAME,
                            frequencyTypeId = a.FREQUENCYTYPEID,
                            frequencyTypeName = f.MODE,
                            loanId = a.LOANID,
                            loanRefNumber = e.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            relationshipManager = c.FIRSTNAME + " " + c.LASTNAME,
                            managerEmail = c.EMAIL,
                            relationshipOfficer = d.FIRSTNAME + " " + d.LASTNAME,
                            officerEmail = d.EMAIL,
                        }).ToList();

            try
            {
                var staffList = staffRepo.GetAllStaff().ToList();

                var dataList = data.Select(g => g.relationshipManagerId).ToList();

                staffList = staffList.Where(x => dataList.Contains(x.StaffId)).ToList();

                string dataTable;

                foreach (var mailItem in staffList)
                {
                    string recipient = mailItem.Email;

                    var mailList = data.Where(x => x.relationshipManagerId == mailItem.StaffId).ToList();

                    dataTable =
                            "<table><tr><th>Application Ref</th><th>Covenant Detail</th><th>Covenant Type</th>" +
                            "<th>Amount</th><th>Covenant Date</th><th>Due Date</th></tr>";

                    foreach (var item in mailList)
                    {
                        dataTable = dataTable +
                            $"<tr><td>{item.loanRefNumber}</td><td>{item.covenantDetail}</td><td>{item.covenantTypeName}</td>" +
                            $"<td>{item.covenantAmount}</td><td>{item.covenantDate:d}</td><td>{item.dueDate:d}</td></tr>";
                    }

                    dataTable = dataTable + "</table>";

                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;  //+ " (REMINDER) - LOAN COVENANTS OVERDUE";

                    var messageContent = $"Dear {mailItem.FirstName + " " + mailItem.LastName}, <br /><br />" + messageBody +
                                         //"This is to bring your attention the following loan covenants " +
                                         //"which are overdue for revaluation. <br /><br />" +
                                         $"{dataTable}";

                    var additionalRecipient = data.FirstOrDefault(x => x.relationshipOfficerId == mailItem.StaffId).officerEmail;

                    var templateUrl = "~/EmailTemplates/Monitoring.html";

                    var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

                    var messageModel = new MessageLogViewModel()
                    {
                        //MessageId = model.MessageId,
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = (short)MessageStatusEnum.Pending,
                        MessageTypeId = (short)MessageTypeEnum.Email,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{additionalRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };

                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += response = " Covenants Due Date data was logged successfully, ";
                    }
                    else
                        response += response = " Covenants Due Date data log has failed, ";


                    //   emailHelpers.SendMail(recipient, additionalRecipient, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        #endregion Covenant Monitoring

        #region Collateral Monitoring

        public void SendAlertsForCollateralPropertyRevaluation(string title, string messageBody)
        {
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

            var data = (from a in context.TBL_COLLATERAL_CUSTOMER
                        join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                        join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                        join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                        join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                        join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f
                            .COLLATERALCUSTOMERID
                        where (DbFunctions.DiffDays(DbFunctions.AddDays(f.LASTVALUATIONDATE, a.VALUATIONCYCLE), applDate) <= 30)
                        select new CollateralViewModel
                        {
                            collateralTypeId = a.COLLATERALTYPEID,
                            collateralType = d.COLLATERALTYPENAME,
                            collateralCode = a.COLLATERALCODE,
                            collateralSubType = e.COLLATERALSUBTYPENAME,
                            customerName = b.FIRSTNAME + " " + b.LASTNAME,
                            propertyName = f.PROPERTYNAME,
                            lastValuationDate = f.LASTVALUATIONDATE,
                            relationshipManagerId = a.CREATEDBY,
                            relationshipManager = c.FIRSTNAME + " " + c.LASTNAME,
                            relationshipManagerEmail = c.EMAIL
                        }).ToList();

            try
            {
                var staffList = staffRepo.GetAllStaff().ToList();

                var dataList = data.Select(g => g.relationshipManagerId).ToList();

                staffList = staffList.Where(x => dataList.Contains(x.StaffId)).ToList();

                string dataTable;

                foreach (var mailItem in staffList)
                {
                    string recipient = mailItem.Email;

                    var mailList = data.Where(x => x.relationshipManagerId == mailItem.StaffId).ToList();

                    dataTable =
                            "<table><tr><th>Collateral Code</th><th>Collateral Type</th>" +
                            "<th>Collateral Sub Type</th><th>Property</th><th>Last Valuation Date</th></tr>";

                    foreach (var item in mailList)
                    {
                        dataTable = dataTable +
                           $"<tr><td>{item.collateralCode}</td><td>{item.collateralType}</td><td>{item.collateralSubType}</td>" +
                            $"<td>{item.propertyName}</td><td>{item.lastValuationDate:d}</td></tr>";
                    }

                    dataTable = dataTable + "</table>";

                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title; //+ " (REMINDER) - COLLATERAL DUE FOR RE-EVALUATION";

                    var messageContent = $"Dear {mailItem.FirstName + " " + mailItem.LastName}, <br /><br />" + messageBody +
                                             //"This is to bring your attention the following collaterals " +
                                             //"which are due for revaluation. <br /><br />" +
                                             $"{dataTable}";

                    var templateUrl = "~/EmailTemplates/Monitoring.html";

                    var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

                    var messageModel = new MessageLogViewModel()
                    {
                        //MessageId = model.MessageId,
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = (short)MessageStatusEnum.Pending,
                        MessageTypeId = (short)MessageTypeEnum.Email,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = recipient,
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };

                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += response = " Collateral  Property Revaluation was logged successfully, ";
                    }
                    else
                        response += response = " Collateral  Property Revaluation logged has failed, ";


                    //emailHelpers.SendMail(recipient, null, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        #endregion Collateral Monitoring

        #region NPL Monitoring

        public void SendAlertsForLoanNplMonitoring(string title, string messageBody)
        {
            var data = (from a in context.TBL_LOAN_APPLICATION
                        join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                        join b in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                        join c in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                        where b.INT_PRUDENT_GUIDELINE_STATUSID != (int)LoanPrudentialStatusEnum.Performing
                        select new LoanViewModel
                        {
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            loanReferenceNumber = b.LOANREFERENCENUMBER,
                            bookingDate = b.BOOKINGDATE,
                            disburseDate = b.DISBURSEDATE,
                            nplDate = b.NPLDATE.Value,
                            outstandingInterest = b.OUTSTANDINGINTEREST,
                            outstandingPrincipal = b.OUTSTANDINGPRINCIPAL,
                            loanTypeName = b.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            relationshipManagerId = b.RELATIONSHIPMANAGERID,
                            relationshipManagerName = b.TBL_STAFF1.FIRSTNAME + " " + b.TBL_STAFF1.LASTNAME,
                            relationshipManagerEmail = b.TBL_STAFF1.EMAIL,
                            relationshipOfficerId = b.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = b.TBL_STAFF.FIRSTNAME + " " + b.TBL_STAFF.LASTNAME,
                            relationshipOfficerEmail = b.TBL_STAFF.EMAIL
                        }).ToList();

            try
            {
                var staffList = staffRepo.GetAllStaff().ToList();

                var dataList = data.Select(g => g.relationshipManagerId).ToList();

                staffList = staffList.Where(x => dataList.Contains(x.StaffId)).ToList();

                string dataTable;

                foreach (var mailItem in staffList)
                {
                    dataTable =
                        "<table><tr><th>Loan Ref #</th><th>Loan Type</th><th>Outstanding Interest</th><th>Oustanding Principal</th>" +
                        "<th>Booking Date</th><th>Disbursed Date</th></tr>";

                    string recipient = mailItem.Email;

                    var mailList = data.Where(x => x.relationshipManagerId == mailItem.StaffId).ToList();

                    foreach (var item in mailList)
                    {
                        dataTable = dataTable +
                            $"<tr><td>{item.loanReferenceNumber}</td><td>{item.loanTypeName}</td>" +
                            $"<td style='text-align:right;'>{item.outstandingInterest:f}</td><td style='text-align:right;'>{item.outstandingPrincipal:f}</td>" +
                            $"<td>{item.bookingDate:d}</td><td>{item.disburseDate:d}</td></tr>";
                    }

                    dataTable = dataTable + "</table>";

                    var messageContent = $"Dear {mailItem.FirstName + " " + mailItem.LastName}, <br /><br />" + messageBody +
                                             //"This is to bring your attention the following loans " +
                                             //"which are underperforming. <br /><br />" +
                                             $"{dataTable}";

                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title; //+ " (REMINDER) - NON-PERFORMING LOANS";

                    var otherRecipient = data.FirstOrDefault(x => x.relationshipOfficerId == mailItem.StaffId).relationshipOfficerEmail;

                    var templateUrl = "~/EmailTemplates/Monitoring.html";

                    var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

                    var messageModel = new MessageLogViewModel()
                    {
                        //MessageId = model.MessageId,
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = (short)MessageStatusEnum.Pending,
                        MessageTypeId = (short)MessageTypeEnum.Email,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{otherRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };

                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += response = " Loan Npl Monitoring was logged successfully, ";
                    }
                    else
                        response += response = " Loan Npl Monitoring logging has failed, ";


                    //emailHelpers.SendMail(recipient, otherRecipient, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        #endregion NPL Monitoring

        #region LPO/CFF/IDF/Self-Liquidating Loans

        public void SendAlertsOnSelfLiquidatingLoanExpiry(string title, string messageBody)
        {
            var applDate = genSetup.GetApplicationDate();

            var data = (from a in context.TBL_LOAN
                        join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                        join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                        join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                        where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.SelfLiquidating &&
                        DbFunctions.DiffDays(a.MATURITYDATE, applDate) <= 30
                        select new LoanViewModel
                        {
                            applicationReferenceNumber = d.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            loanReferenceNumber = a.LOANREFERENCENUMBER,
                            bookingDate = a.BOOKINGDATE,
                            disburseDate = a.DISBURSEDATE,
                            maturityDate = a.MATURITYDATE,
                            productName = b.PRODUCTNAME,
                            outstandingInterest = a.OUTSTANDINGINTEREST,
                            outstandingPrincipal = a.OUTSTANDINGPRINCIPAL,
                            loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productTypeName = b.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                            relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                            relationshipOfficerEmail = a.TBL_STAFF.EMAIL
                        }).ToList();

            try
            {
                var staffList = staffRepo.GetAllStaff().ToList();

                var dataList = data.Select(g => g.relationshipManagerId).ToList();

                staffList = staffList.Where(x => dataList.Contains(x.StaffId)).ToList();

                string dataTable;

                foreach (var mailItem in staffList)
                {
                    dataTable =
                            "<table><tr><th>Loan Ref #</th><th>Product</th><th>Loan Type</th><th>Product Type</th><th>Outstanding Interest</th>" +
                            "<th>Oustanding Principal</th><th>Disbursed Date</th><th>Maturity Date</th></tr>";

                    string recipient = mailItem.Email;

                    var mailList = data.Where(x => x.relationshipManagerId == mailItem.StaffId).ToList();

                    foreach (var item in mailList)
                    {
                        dataTable = dataTable +
                            $"<tr><td>{item.loanReferenceNumber}</td><td>{item.productName}</td><td>{item.loanTypeName}</td><td>{item.productTypeName}</td>" +
                            $"<td style='text-align:right;'>{item.outstandingInterest:f}</td><td style='text-align:right;'>{item.outstandingPrincipal:f}</td>" +
                            $"<td>{item.maturityDate:d}</td><td>{item.disburseDate:d}</td></tr>";
                    }

                    dataTable = dataTable + "</table>";

                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title; //+ " (REMINDER) - EXPIRED SELF-LIQUIDATING LOANS";

                    string messageContent = $"Dear {mailItem.FirstName + " " + mailItem.LastName}, <br /><br />" + messageBody +
                                             //"This is to bring your attention the following self-liquidating loans " +
                                             //"which are approaching expiry. <br /><br />" +
                                             $"{dataTable}";

                    var otherRecipient = data.FirstOrDefault(x => x.relationshipOfficerId == mailItem.StaffId).relationshipOfficerEmail;

                    var templateUrl = "~/EmailTemplates/Monitoring.html";

                    var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

                    var messageModel = new MessageLogViewModel()
                    {
                        //MessageId = model.MessageId,
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = (short)MessageStatusEnum.Pending,
                        MessageTypeId = (short)MessageTypeEnum.Email,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{otherRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };

                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += response = " Self Liquidating Loan Expiry was logged successfully, ";
                    }
                    else
                        response += response = " Self Liquidating Loan Expiry logging has failed, ";
                    //emailHelpers.SendMail(recipient, otherRecipient, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion LPO/CFF/IDF/Self-Liquidating Loans

        #region Overdraft Monitoring

        public void SendAlertsOnOverDraftLoansAlmostDue(string title, string messageBody)
        {
            var applDate = genSetup.GetApplicationDate();

            var data = (from a in context.TBL_LOAN_REVOLVING
                        join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                        join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                        join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                        where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.RevolvingLoan &&
                        DbFunctions.DiffDays(a.MATURITYDATE, applDate) <= 90
                        select new LoanViewModel
                        {
                            applicationReferenceNumber = d.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                            loanReferenceNumber = a.LOANREFERENCENUMBER,
                            bookingDate = a.BOOKINGDATE,
                            disburseDate = a.DISBURSEDATE,
                            maturityDate = a.MATURITYDATE,
                            productName = b.PRODUCTNAME,
                            loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productTypeName = b.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                            overdraftLimit = a.OVERDRAFTLIMIT,
                            relationshipManagerId = a.RELATIONSHIPMANAGERID,
                            relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                            relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                            relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                            relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                            relationshipOfficerEmail = a.TBL_STAFF.EMAIL
                        }).ToList();

            try
            {
                var staffList = staffRepo.GetAllStaff().ToList();

                var dataList = data.Select(g => g.relationshipManagerId).ToList();

                staffList = staffList.Where(x => dataList.Contains(x.StaffId)).ToList();

                string dataTable;

                foreach (var mailItem in staffList)
                {
                    dataTable =
                            "<table><tr><th>Loan Ref #</th><th>Product</th><th>Loan Type</th><th>Product Type</th>" +
                            "<th>Overdraft Limit</th><th>Disbursed Date</th><th>Maturity Date</th></tr>";

                    string recipient = mailItem.Email;

                    var mailList = data.Where(x => x.relationshipManagerId == mailItem.StaffId).ToList();

                    foreach (var item in mailList)
                    {
                        dataTable = dataTable +
                            $"<tr><td>{item.loanReferenceNumber}</td><td>{item.productName}</td><td>{item.loanTypeName}</td><td>{item.productTypeName}</td>" +
                            $"<td style='text-align:right;'>{item.overdraftLimit:f}</td>" +
                            $"<td>{item.maturityDate:d}</td><td>{item.disburseDate:d}</td><td>{item.maturityDate:d}</td></tr>";
                    }

                    dataTable = dataTable + "</table>";

                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title; //+ " (REMINDER) - EXPIRED OVERDRAFT LOANS";

                    string messageContent = $"Dear {mailItem.FirstName + " " + mailItem.LastName}, <br /><br />" + messageBody +
                                             //"This is to bring your attention the following overdraft loans " +
                                             //"which are approaching expiry. <br /><br />" +
                                             $"{dataTable}";

                    var otherRecipient = data.FirstOrDefault(x => x.relationshipOfficerId == mailItem.StaffId).relationshipOfficerEmail;

                    var templateUrl = "~/EmailTemplates/Monitoring.html";

                    var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

                    var messageModel = new MessageLogViewModel()
                    {
                        //MessageId = model.MessageId,
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = (short)MessageStatusEnum.Pending,
                        MessageTypeId = (short)MessageTypeEnum.Email,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{otherRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };

                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += response = " Over Draft Loans Almost Due was logged successfully, ";
                    }
                    else
                        response += response = " Over Draft Loans Almost Due has failed, ";

                    //emailHelpers.SendMail(recipient, otherRecipient, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion Overdraft Monitoring

        #region Collateral Monitoring

        public void SendAlertsForExpiredInsurance(string title, string messageBody)
        {
            var applDate = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE;

            var data = (from a in context.TBL_COLLATERAL_CUSTOMER
                        join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                        join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                        join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                        join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                        join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                        join p in context.TBL_COLLATERAL_ITEM_POLICY on a.COLLATERALCUSTOMERID equals p.COLLATERALCUSTOMERID
                        where (DbFunctions.DiffDays(p.ENDDATE, DateTime.Now) <= 0)
                        select new CollateralViewModel
                        {
                            collateralType = d.COLLATERALTYPENAME,
                            collateralCode = a.COLLATERALCODE,
                            collateralSubType = e.COLLATERALSUBTYPENAME,
                            customerName = b.FIRSTNAME + " " + b.LASTNAME,
                            propertyName = f.PROPERTYNAME,
                            lastValuationDate = f.LASTVALUATIONDATE,
                            collateralValue = a.COLLATERALVALUE,
                            valuationCycle = a.VALUATIONCYCLE,
                          //  insuranceCompany = p.INSURANCECOMPANYNAME,
                            startDate = p.STARTDATE,
                            endDate = p.ENDDATE


                        }).ToList();

            try
            {
                var staffList = staffRepo.GetAllStaff().ToList();

                var dataList = data.Select(g => g.relationshipManagerId).ToList();

                staffList = staffList.Where(x => dataList.Contains(x.StaffId)).ToList();

                string dataTable;

                foreach (var mailItem in staffList)
                {
                    string recipient = mailItem.Email;

                    var mailList = data.Where(x => x.relationshipManagerId == mailItem.StaffId).ToList();

                    dataTable =
                            "<table><tr><th>Collateral Code</th><th>Collateral Type</th>" +
                            "<th>Collateral Sub Type</th><th>Customer Name</th><th>Property</th><th>Last Valuation Date</th><th>Collateral Value</th>" +
                            "<th>Valuation Cycle</th><th>Insurance Company</th><th>Start Date</th><th>End Date</th></tr>";

                    foreach (var item in mailList)
                    {
                        dataTable = dataTable +
                           $"<tr><td>{item.collateralCode}</td><td>{item.collateralType}</td><td>{item.collateralSubType}</td><td>{item.customerName}</td>" +
                            $"<td>{item.propertyName}</td><td>{item.lastValuationDate:d}</td>><td>{item.collateralValue:d}</td>" +
                            $"><td>{item.valuationCycle:d}</td>><td>{item.insuranceCompany:d}</td>><td>{item.startDate:d}</td>" +
                            $"><td>{item.endDate:d}</td>></tr>";
                    }

                    dataTable = dataTable + "</table>";

                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;  //+ " (REMINDER) - EXPIRED INSURANCE";

                    var messageContent = $"Dear {mailItem.FirstName + " " + mailItem.LastName}, <br /><br />" + messageBody +
                                             //"This is to bring your attention the following Insurance " +
                                             //"has expired. <br /><br />" +
                                             $"{dataTable}";

                    var templateUrl = "~/EmailTemplates/Monitoring.html";

                    var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

                    var messageModel = new MessageLogViewModel()
                    {
                        //MessageId = model.MessageId,
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = (short)MessageStatusEnum.Pending,
                        MessageTypeId = (short)MessageTypeEnum.Email,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = recipient,
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };

                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += response = " Expired Insurance was logged successfully, ";
                    }
                    else
                        response += response = " Expired Insurance logging has failed, ";

                    //emailHelpers.SendMail(recipient, null, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        #endregion Collateral Monitoring

        #region LOAN CASA WITH PND
        //#######################        LOAN CASA WITH PND         ############################ 
        public void SendAlertsOnLoanCASAwithPND(string title, string messageBody)
        {
            var applDate = genSetup.GetApplicationDate();


            var data = (from a in context.TBL_LOAN
                        join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                        join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                        join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                        where s.HASLIEN == true && (s.POSTNOSTATUSID == (short)CASAPostNoStatusEnum.PostNoDebit || s.POSTNOSTATUSID == (short)CASAPostNoStatusEnum.PostNoDebitandCredit)

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
                            loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
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

            try
            {
                var staffList = staffRepo.GetAllStaff().ToList();

                var dataList = data.Select(g => g.relationshipManagerId).ToList();

                staffList = staffList.Where(x => dataList.Contains(x.StaffId)).ToList();

                string dataTable;

                foreach (var mailItem in staffList)
                {
                    dataTable =
                             "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Principal Amount</th><th>Outstanding Principal</th>" +
                        "<th>Tenor</th><th>Interest Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";

                    string recipient = mailItem.Email;

                    var mailList = data.Where(x => x.relationshipManagerId == mailItem.StaffId).ToList();

                    foreach (var item in mailList)
                    {
                        dataTable = dataTable +
                            $"<tr><td>{item.loanReferenceNumber}</td><td>{item.customerName}</td><td>{item.principalAmount}</td><td>{item.outstandingPrincipal}</td>" +
                            $"<td style='text-align:right;'>{item.tenor:f}</td>" + $"<td style='text-align:right;'>{item.interestRate:f}</td>" +
                            $"<td>{item.bookingDate:d}</td><td>{item.effectiveDate:d}</td><td>{item.maturityDate:d}</td></tr>";
                    }

                    dataTable = dataTable + "</table>";

                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;// + " (REMINDER) - LOAN CASA WITH PND AND LEIN";

                    string messageContent = $"Dear {mailItem.FirstName + " " + mailItem.LastName}, <br /><br />" + messageBody +
                                             //"This is to bring your attention the following loans " +
                                             //"are on PND and lein placed on them. <br /><br />" +
                                             $"{dataTable}";

                    var otherRecipient = data.FirstOrDefault(x => x.relationshipOfficerId == mailItem.StaffId).relationshipOfficerEmail;

                    var templateUrl = "~/EmailTemplates/Monitoring.html";

                    var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

                    var messageModel = new MessageLogViewModel()
                    {
                        //MessageId = model.MessageId,
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = (short)MessageStatusEnum.Pending,
                        MessageTypeId = (short)MessageTypeEnum.Email,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{otherRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += response = " Loan CASA with PND was logged successfully, ";
                    }
                    else
                        response += response = " Loan CASA with PND logging has failed, ";


                    //emailHelpers.SendMail(recipient, otherRecipient, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion LOAN CASA WITH PND

        #region INACTIVE BOND AND GUARANTEE
        //#######################        INACTIVE BOND AND GUARANTEE         ############################ 
        public void SendAlertsOnLoanForInActiveBondAndGuarantee(string title, string messageBody)
        {
            var applDate = genSetup.GetApplicationDate();

            var data = (from a in context.TBL_LOAN_CONTINGENT
                        join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                        join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                        join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                        where s.HASLIEN == true && (s.POSTNOSTATUSID == (short)CASAPostNoStatusEnum.PostNoDebit || s.POSTNOSTATUSID == (short)CASAPostNoStatusEnum.PostNoDebitandCredit)

                        select new LoanViewModel
                        {
                            applicationReferenceNumber = a.LOANREFERENCENUMBER,
                            loanReferenceNumber = a.LOANREFERENCENUMBER,
                            bookingDate = a.BOOKINGDATE,
                            disburseDate = a.DISBURSEDATE,
                            maturityDate = a.MATURITYDATE,
                            principalAmount = a.CONTINGENTAMOUNT,
                            exchangeRate = a.EXCHANGERATE,
                            loanTypeName = a.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
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

            try
            {
                var staffList = staffRepo.GetAllStaff().ToList();

                var dataList = data.Select(g => g.relationshipManagerId).ToList();

                staffList = staffList.Where(x => dataList.Contains(x.StaffId)).ToList();

                string dataTable;

                foreach (var mailItem in staffList)
                {
                    dataTable =
                             "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th>" +
                        "<th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";

                    string recipient = mailItem.Email;

                    var mailList = data.Where(x => x.relationshipManagerId == mailItem.StaffId).ToList();

                    foreach (var item in mailList)
                    {
                        dataTable = dataTable +
                            $"<tr><td>{item.loanReferenceNumber}</td><td>{item.customerName}</td><td>{item.principalAmount}</td><td>{item.exchangeRate}</td>" +
                            $"<td style='text-align:right;'>{item.tenor:f}</td>" + $"<td style='text-align:right;'>{item.interestRate:f}</td>" +
                            $"<td>{item.bookingDate:d}</td><td>{item.effectiveDate:d}</td><td>{item.maturityDate:d}</td></tr>";
                    }

                    dataTable = dataTable + "</table>";

                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;  ///+ " (REMINDER) - INACTIVE BOND AND GUARANTEE";

                    string messageContent = $"Dear {mailItem.FirstName + " " + mailItem.LastName}, <br /><br />" + messageBody +
                                             //"This is to bring your attention the following Bond and guarantee " +
                                             //" are about to expire. <br /><br />" +
                                             $"{dataTable}";

                    var otherRecipient = data.FirstOrDefault(x => x.relationshipOfficerId == mailItem.StaffId).relationshipOfficerEmail;

                    var templateUrl = "~/EmailTemplates/Monitoring.html";

                    var mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);

                    var messageModel = new MessageLogViewModel()
                    {
                        //MessageId = model.MessageId,
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = (short)MessageStatusEnum.Pending,
                        MessageTypeId = (short)MessageTypeEnum.Email,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{otherRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };

                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += response = " Bond and guarantee about to expire was logged successfully, ";
                    }
                    else
                        response += response = " Bond and guarantee about to expire logging has failed, ";

                    //emailHelpers.SendMail(recipient, otherRecipient, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion INACTIVE BOND AND GUARANTEE

        #region Helper Methods

        public int SaveMessageDetails(MessageLogViewModel model)
        {
            var message = new TBL_MESSAGE_LOG()
            {
                //MessageId = model.MessageId,
                MESSAGESUBJECT = model.MessageSubject,
                MESSAGEBODY = model.MessageBody,
                MESSAGESTATUSID = model.MessageStatusId,
                MESSAGETYPEID = model.MessageTypeId,
                FROMADDRESS = model.FromAddress,
                TOADDRESS = model.ToAddress,
                DATETIMERECEIVED = model.DateTimeReceived,
                SENDONDATETIME = model.SendOnDateTime
            };

            context.TBL_MESSAGE_LOG.Add(message);

            try
            {
                return context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        //public bool CreateEmailMessageAndSend(MessageLogViewModel model)
        //{
        //    bool sentEmail;

        //    var templateUrl = "~/EmailTemplates/Monitoring.html";

        //    var message = new TBL_MESSAGE_LOG()
        //    {
        //        //MessageId = model.MessageId,
        //        MESSAGESUBJECT = model.MessageSubject,
        //        MESSAGEBODY = model.MessageBody,
        //        MESSAGESTATUSID = model.MessageStatusId,
        //        MESSAGETYPEID = model.MessageTypeId,
        //        FROMADDRESS = ConfigurationManager.AppSettings["SupportEmailAddr"],
        //        TOADDRESS = model.ToAddress,
        //        DATETIMERECEIVED = model.DateTimeReceived,
        //        SENDONDATETIME = model.SendOnDateTime
        //    };

        //    try
        //    {
        //        context.TBL_MESSAGE_LOG.Add(message);

        //        context.SaveChanges();

        //       // sentEmail = EmailHelpers.SendMail(model.ToAddress, null, model.MessageSubject, model.MessageBody, templateUrl);

        //        //if (sentEmail)
        //        //{
        //        //    message.MESSAGESTATUSID = (short)MessageStatusEnum.Sent;

        //        //    context.SaveChanges();

        //        //    return true;
        //        //}
        //        //else
        //        //{
        //        //    message.MESSAGESTATUSID = (short)MessageStatusEnum.Attempted;

        //        //    context.SaveChanges();

        //        //    return false;
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new SecureException(ex.Message);
        //    }
        //}

        public IEnumerable<MessageLogViewModel> GetMailingList()
        {
            var mailList = (from data in context.TBL_MESSAGE_LOG
                            where data.MESSAGESTATUSID == (short)MessageStatusEnum.Pending
                            select new MessageLogViewModel()
                            {
                                MessageId = data.MESSAGEID,
                                MessageSubject = data.MESSAGESUBJECT,
                                MessageBody = data.MESSAGEBODY,
                                MessageStatusId = data.MESSAGESTATUSID,
                                MessageTypeId = data.MESSAGETYPEID,
                                FromAddress = data.FROMADDRESS,
                                ToAddress = data.TOADDRESS,
                                DateTimeReceived = data.DATETIMERECEIVED,
                                SendOnDateTime = data.SENDONDATETIME
                            }).ToList();

            return mailList;
        }

        public IEnumerable<MessageLogViewModel> GetEmailMailingList()
        {
            var mailList = GetMailingList().Where(m => m.MessageTypeId == (short)MessageTypeEnum.Email).ToList();

            return mailList;
        }

        public IEnumerable<MessageLogViewModel> GetSmsMailingList()
        {
            var mailList = GetMailingList().Where(m => m.MessageTypeId == (short)MessageTypeEnum.SMS).ToList();

            return mailList;
        }

        public bool UpdateMailDeliveryStatus(int messageId, short statusId)
        {
            var mailMessage = context.TBL_MESSAGE_LOG.Find(messageId);

            if (mailMessage != null)
            {
                mailMessage.MESSAGESTATUSID = (short)statusId;

                var output = context.SaveChanges() > 0;

                if (output)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public bool CreateEmailMessageAndSend(MessageLogViewModel model)
        {
            throw new NotImplementedException();
        }

        #endregion Helper Methods


    }
}