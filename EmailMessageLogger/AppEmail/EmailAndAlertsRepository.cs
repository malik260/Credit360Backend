using EmailMessageLogger;
using EmailMessageLogger.Enum;
//using FintrakBanking.Common;
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
    public class EmailAndAlertsRepository
    {
        private FinTrakBankingContext context = new FinTrakBankingContext();
        private IAuditTrailRepository auditTrail;
        private EmailHelpers emailHelpers;
        private IGeneralSetupRepository genSetup;
        private DateTime applDate;
        private IStaffRepository staffRepo;
        public string response = string.Empty;

        private readonly string supportEmail = ConfigurationManager.AppSettings["SupportEmailAddr"];

        public EmailAndAlertsRepository(
                FinTrakBankingContext _context,
                IAuditTrailRepository _auditTrail,
                EmailHelpers _emailHelpers,
                IGeneralSetupRepository _general,
                IStaffRepository _staffRepo
            )
        {
            context = _context;
            auditTrail = _auditTrail;
            emailHelpers = _emailHelpers;
            genSetup = _general;
            staffRepo = _staffRepo;
        }

        #region Covenant Monitoring

        public bool SendAlertsForCovenantsApproachingDueDate(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP alertsetupForCovenantsApproachingDueDate = (from x in alertSetups
                                                                                   where x.MONITORING_ITEMID == 8
                                                                                   select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanCovenantDetailViewModel> loanDetails = (from a in context.TBL_LOAN_COVENANT_DETAIL
                                                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                                             join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                                                             join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                                             join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals (short?)f.FREQUENCYTYPEID
                                                             join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                                                             where DbFunctions.DiffDays(a.NEXTCOVENANTDATE, currentDate) <= alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD1
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
                                                                 notificationDuration = (int)DbFunctions.DiffDays(a.NEXTCOVENANTDATE, currentDate)
                                                             }).ToList();
            if (loanDetails != null)
            {
                SendAlertsForCovenantsApproachingDueDateToRM(loanDetails, alertsetupForCovenantsApproachingDueDate.MESSAGE_TITLE);
                if (alertsetupForCovenantsApproachingDueDate.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelOne = (from x in loanDetails
                                                                            where x.notificationDuration <= alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD1
                                                                            select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertsForCovenantsApproachingDueDateToMonitoringTeam(escalationLevelOne, alertsetupForCovenantsApproachingDueDate);
                    }
                }
                if (alertsetupForCovenantsApproachingDueDate.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelTwo = (from x in loanDetails
                                                                            where x.notificationDuration <= alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD2
                                                                            select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertsForCovenantsApproachingDueDateToMonitoringTeam(escalationLevelTwo, alertsetupForCovenantsApproachingDueDate);
                    }
                }
                if (alertsetupForCovenantsApproachingDueDate.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelThree = (from x in loanDetails
                                                                              where x.notificationDuration <= alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD3
                                                                              select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        SendAlertsForCovenantsApproachingDueDateToMonitoringTeam(escalationLevelThree, alertsetupForCovenantsApproachingDueDate);
                    }
                }
                return true;
            }
            return false;
        }

        public void SendAlertsForCovenantsApproachingDueDateToRM(List<LoanCovenantDetailViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList = (from x in staffList
                             where dataList.Contains(x.STAFFID)
                             select x).ToList();
                foreach (TBL_STAFF item2 in staffList)
                {
                    string recipient = item2.EMAIL.Trim();
                    string dataTable = "<table><tr><th>Application Ref</th><th>Covenant Detail</th><th>Covenant Type</th><th>Amount</th><th>Covenant Date</th><th>Due Date</th></tr>";
                    List<LoanCovenantDetailViewModel> mailList = (from x in loanDetails
                                                                  where x.relationshipManagerId == item2.STAFFID
                                                                  select x).ToList();
                    foreach (LoanCovenantDetailViewModel item3 in mailList)
                    {
                        dataTable = dataTable + $"<tr><td>{item3.loanRefNumber}</td><td>{item3.covenantDetail}</td><td>{item3.covenantTypeName}</td>" + $"<td>{item3.covenantAmount}</td><td>{item3.covenantDate:d}</td><td>{item3.dueDate:d}</td></tr>";
                        dataTable += "</table>";
                    }
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following loan covenants which are approaching their due date for revaluation. <br /><br />" + $"{dataTable}";
                    string additionalRecipient = loanDetails.FirstOrDefault((LoanCovenantDetailViewModel x) => x.relationshipOfficerId == item2.STAFFID).officerEmail;
                    string templateUrl = "EmailTemplates\\Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = title,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{additionalRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Covenants Approaching Due Date was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Covenants Approaching Due Date looging has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SendAlertsForCovenantsApproachingDueDateToMonitoringTeam(List<LoanCovenantDetailViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable = "<table><tr><th>Application Ref</th><th>Covenant Detail</th><th>Covenant Type</th><th>Amount</th><th>Covenant Date</th><th>Due Date</th></tr>";
                foreach (LoanCovenantDetailViewModel loanDetail in loanDetails)
                {
                    dataTable = dataTable + $"<tr><td>{loanDetail.loanRefNumber}</td><td>{loanDetail.covenantDetail}</td><td>{loanDetail.covenantTypeName}</td>" + $"<td>{loanDetail.covenantAmount}</td><td>{loanDetail.covenantDate:d}</td><td>{loanDetail.dueDate:d}</td></tr>";
                    dataTable += "</table>";
                }
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following loan covenants which are approaching their due date for revaluation. <br /><br />" + $"{dataTable}";
                string templateUrl = "EmailTemplates\\Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Covenants Approaching Due Date was logged successfully, ");
                }
                else
                {
                    response += (response = " Covenants Approaching Due Date looging has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }






        public bool SendAlertsForCovenantsOverDue(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP alertsetupForCovenantsOverDue = (from x in alertSetups
                                                                        where x.MONITORING_ITEMID == 2
                                                                        select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanCovenantDetailViewModel> loanDetails = (from a in context.TBL_LOAN_COVENANT_DETAIL
                                                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                                             join c in context.TBL_STAFF on b.RELATIONSHIPMANAGERID equals c.STAFFID
                                                             join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                                                             join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                                             join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals (short?)f.FREQUENCYTYPEID
                                                             join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                                                             where DbFunctions.DiffDays((DateTime?)a.NEXTCOVENANTDATE.Value, (DateTime?)currentDate) >= (int?)alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
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
                                                                 notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.NEXTCOVENANTDATE.Value, (DateTime?)currentDate)
                                                             }).ToList();
            if (loanDetails != null)
            {
                SendAlertsForCovenantsOverDueRM(loanDetails, alertsetupForCovenantsOverDue.MESSAGE_TITLE);
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelOne = (from x in loanDetails
                                                                            where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
                                                                            select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertsForCovenantsOverDueMonitoringTeam(escalationLevelOne, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelTwo = (from x in loanDetails
                                                                            where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD2
                                                                            select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertsForCovenantsOverDueMonitoringTeam(escalationLevelTwo, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelThree = (from x in loanDetails
                                                                              where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD3
                                                                              select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        SendAlertsForCovenantsOverDueMonitoringTeam(escalationLevelThree, alertsetupForCovenantsOverDue);
                    }
                }
                return true;
            }
            return false;
        }

        public void SendAlertsForCovenantsOverDueRM(List<LoanCovenantDetailViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList = (from x in staffList
                             where dataList.Contains(x.STAFFID)
                             select x).ToList();
                foreach (TBL_STAFF item2 in staffList)
                {
                    string recipient = item2.EMAIL.Trim();
                    List<LoanCovenantDetailViewModel> mailList = (from x in loanDetails
                                                                  where x.relationshipManagerId == item2.STAFFID
                                                                  select x).ToList();
                    string dataTable2 = "<table><tr><th>Application Ref</th><th>Covenant Detail</th><th>Covenant Type</th><th>Amount</th><th>Covenant Date</th><th>Due Date</th></tr>";
                    foreach (LoanCovenantDetailViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanRefNumber}</td><td>{item3.covenantDetail}</td><td>{item3.covenantTypeName}</td>" + $"<td>{item3.covenantAmount}</td><td>{item3.covenantDate:d}</td><td>{item3.dueDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following loan covenants which are overdue for revaluation. <br /><br />" + $"{dataTable2}";
                    string additionalRecipient = loanDetails.FirstOrDefault((LoanCovenantDetailViewModel x) => x.relationshipOfficerId == item2.STAFFID).officerEmail;
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{additionalRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Covenants Due Date data was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Covenants Due Date data log has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SendAlertsForCovenantsOverDueMonitoringTeam(List<LoanCovenantDetailViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable = "<table><tr><th>Application Ref</th><th>Covenant Detail</th><th>Covenant Type</th><th>Amount</th><th>Covenant Date</th><th>Due Date</th></tr>";
                foreach (LoanCovenantDetailViewModel loanDetail in loanDetails)
                {
                    dataTable = dataTable + $"<tr><td>{loanDetail.loanRefNumber}</td><td>{loanDetail.covenantDetail}</td><td>{loanDetail.covenantTypeName}</td>" + $"<td>{loanDetail.covenantAmount}</td><td>{loanDetail.covenantDate:d}</td><td>{loanDetail.dueDate:d}</td></tr>";
                }
                dataTable += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following loan covenants which are overdue for revaluation. <br /><br />" + $"{dataTable}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Covenants Due Date data was logged successfully, ");
                }
                else
                {
                    response += (response = " Covenants Due Date data log has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }





        public bool SendAlertsForCollateralPropertyRevaluation(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP alertsetupForCovenantsOverDue = (from x in alertSetups
                                                                        where x.MONITORING_ITEMID == 3
                                                                        select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<CollateralViewModel> loanDetails = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     where DbFunctions.DiffDays((DateTime?)f.LASTVALUATIONDATE, (DateTime?)currentDate) <= (int?)alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
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
                                                         relationshipManagerEmail = c.EMAIL,
                                                         notificationDuration = (int)DbFunctions.DiffDays((DateTime?)f.LASTVALUATIONDATE, (DateTime?)currentDate)
                                                     }).ToList();
            if (loanDetails != null)
            {
                SendAlertsForCollateralPropertyRevaluationRM(loanDetails, alertsetupForCovenantsOverDue.MESSAGE_TITLE);
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelOne = (from x in loanDetails
                                                                    where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
                                                                    select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertsForCollateralPropertyRevaluationMonitoringTeam(escalationLevelOne, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelTwo = (from x in loanDetails
                                                                    where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD2
                                                                    select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertsForCollateralPropertyRevaluationMonitoringTeam(escalationLevelTwo, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelThree = (from x in loanDetails
                                                                      where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD3
                                                                      select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        SendAlertsForCollateralPropertyRevaluationMonitoringTeam(escalationLevelThree, alertsetupForCovenantsOverDue);
                    }
                }
                return true;
            }
            return false;
        }

        public void SendAlertsForCollateralPropertyRevaluationRM(List<CollateralViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList = (from x in staffList
                             where dataList.Contains(x.STAFFID)
                             select x).ToList();
                foreach (TBL_STAFF item2 in staffList)
                {
                    string recipient = item2.EMAIL.Trim();
                    List<CollateralViewModel> mailList = (from x in loanDetails
                                                          where x.relationshipManagerId == item2.STAFFID
                                                          select x).ToList();
                    string dataTable2 = "<table><tr><th>Collateral Code</th><th>Collateral Type</th><th>Collateral Sub Type</th><th>Property</th><th>Last Valuation Date</th></tr>";
                    foreach (CollateralViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.collateralCode}</td><td>{item3.collateralType}</td><td>{item3.collateralSubType}</td>" + $"<td>{item3.propertyName}</td><td>{item3.lastValuationDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following collaterals which are due for revaluation. <br /><br />" + $"{dataTable2}";
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = title,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = recipient,
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Collateral  Property Revaluation was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Collateral  Property Revaluation logged has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SendAlertsForCollateralPropertyRevaluationMonitoringTeam(List<CollateralViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable = "<table><tr><th>Collateral Code</th><th>Collateral Type</th><th>Collateral Sub Type</th><th>Property</th><th>Last Valuation Date</th></tr>";
                foreach (CollateralViewModel loanDetail in loanDetails)
                {
                    dataTable = dataTable + $"<tr><td>{loanDetail.collateralCode}</td><td>{loanDetail.collateralType}</td><td>{loanDetail.collateralSubType}</td>" + $"<td>{loanDetail.propertyName}</td><td>{loanDetail.lastValuationDate:d}</td></tr>";
                }
                dataTable += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following collaterals which are due for revaluation. <br /><br />" + $"{dataTable}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = recipient,
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Collateral  Property Revaluation was logged successfully, ");
                }
                else
                {
                    response += (response = " Collateral  Property Revaluation logged has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }





        public bool SendAlertsForLoanNplMonitoring(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP alertsetupForCovenantsOverDue = (from x in alertSetups
                                                                        where x.MONITORING_ITEMID == 3
                                                                        select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_APPLICATION
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                                               join b in context.TBL_LOAN on d.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                                               join c in context.TBL_LOAN_REVOLVING on d.LOANAPPLICATIONDETAILID equals c.LOANAPPLICATIONDETAILID
                                               //   where (object?)b.INT_PRUDENT_GUIDELINE_STATUSID != (object?)(int?)1
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                                   loanReferenceNumber = b.LOANREFERENCENUMBER,
                                                   bookingDate = b.BOOKINGDATE,
                                                   disburseDate = b.DISBURSEDATE,
                                                   nplDate = (DateTime?)b.NPLDATE.Value,
                                                   outstandingInterest = b.OUTSTANDINGINTEREST,
                                                   outstandingPrincipal = b.OUTSTANDINGPRINCIPAL,
                                                   loanTypeName = b.TBL_LOAN_TYPE.LOANTYPENAME,
                                                   relationshipManagerId = b.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = b.TBL_STAFF1.FIRSTNAME + " " + b.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = b.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = b.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = b.TBL_STAFF.FIRSTNAME + " " + b.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = b.TBL_STAFF.EMAIL
                                               }).ToList();
            if (loanDetails != null)
            {
                SendAlertsForLoanNplMonitoringRM(loanDetails, alertsetupForCovenantsOverDue.MESSAGE_TITLE);
                SendAlertsForLoanNplMonitoringMonitoringTeam(loanDetails, alertsetupForCovenantsOverDue);
                return true;
            }
            return false;
        }

        public void SendAlertsForLoanNplMonitoringRM(List<LoanViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList = (from x in staffList
                             where dataList.Contains(x.STAFFID)
                             select x).ToList();
                foreach (TBL_STAFF item2 in staffList)
                {
                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Loan Type</th><th>Outstanding Interest</th><th>Oustanding Principal</th><th>Booking Date</th><th>Disbursed Date</th></tr>";
                    string recipient = item2.EMAIL;
                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.STAFFID
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.loanTypeName}</td>" + $"<td style='text-align:right;'>{item3.outstandingInterest:f}</td><td style='text-align:right;'>{item3.outstandingPrincipal:f}</td>" + $"<td>{item3.bookingDate:d}</td><td>{item3.disburseDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following loans which are underperforming. <br /><br />" + $"{dataTable2}";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Loan Npl Monitoring was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Loan Npl Monitoring logging has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SendAlertsForLoanNplMonitoringMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable = "<table><tr><th>Loan Ref #</th><th>Loan Type</th><th>Outstanding Interest</th><th>Oustanding Principal</th><th>Booking Date</th><th>Disbursed Date</th></tr>";
                foreach (LoanViewModel loanDetail in loanDetails)
                {
                    dataTable = dataTable + $"<tr><td>{loanDetail.loanReferenceNumber}</td><td>{loanDetail.loanTypeName}</td>" + $"<td style='text-align:right;'>{loanDetail.outstandingInterest:f}</td><td style='text-align:right;'>{loanDetail.outstandingPrincipal:f}</td>" + $"<td>{loanDetail.bookingDate:d}</td><td>{loanDetail.disburseDate:d}</td></tr>";
                }
                dataTable += "</table>";
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following loans which are underperforming. <br /><br />" + $"{dataTable}";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Loan Npl Monitoring was logged successfully, ");
                }
                else
                {
                    response += (response = " Loan Npl Monitoring logging has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }





        public bool SendAlertsOnSelfLiquidatingLoanExpiry(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP alertOnSelfLiquidatingLoanExpiry = (from x in alertSetups
                                                                           where x.MONITORING_ITEMID == 5
                                                                           select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN
                                               join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                                               join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                               where a.TBL_PRODUCT.PRODUCTTYPEID == 2 && DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)applDate) <= (int?)alertOnSelfLiquidatingLoanExpiry.NOTIFICATION_PERIOD1
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
                                                   loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                                                   productTypeName = b.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL
                                               }).ToList();
            if (loanDetails != null)
            {
                SendAlertsOnSelfLiquidatingLoanExpiryRM(loanDetails, alertOnSelfLiquidatingLoanExpiry.MESSAGE_TITLE);
                if (alertOnSelfLiquidatingLoanExpiry.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelOne = (from x in loanDetails
                                                              where x.notificationDuration <= alertOnSelfLiquidatingLoanExpiry.NOTIFICATION_PERIOD1
                                                              select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertsOnSelfLiquidatingLoanExpiryMonitoringTeam(escalationLevelOne, alertOnSelfLiquidatingLoanExpiry);
                    }
                }
                if (alertOnSelfLiquidatingLoanExpiry.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelTwo = (from x in loanDetails
                                                              where x.notificationDuration <= alertOnSelfLiquidatingLoanExpiry.NOTIFICATION_PERIOD2
                                                              select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertsOnSelfLiquidatingLoanExpiryMonitoringTeam(escalationLevelTwo, alertOnSelfLiquidatingLoanExpiry);
                    }
                }
                if (alertOnSelfLiquidatingLoanExpiry.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelThree = (from x in loanDetails
                                                                where x.notificationDuration <= alertOnSelfLiquidatingLoanExpiry.NOTIFICATION_PERIOD3
                                                                select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        return false;
                    }
                    SendAlertsOnSelfLiquidatingLoanExpiryMonitoringTeam(escalationLevelThree, alertOnSelfLiquidatingLoanExpiry);
                }
                return true;
            }
            return false;
        }

        public void SendAlertsOnSelfLiquidatingLoanExpiryRM(List<LoanViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList2 = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList2 = (from x in staffList2
                              where dataList.Contains(x.STAFFID)
                              select x).ToList();
                foreach (TBL_STAFF item2 in staffList2)
                {
                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Product</th><th>Loan Type</th><th>Product Type</th><th>Outstanding Interest</th><th>Oustanding Principal</th><th>Disbursed Date</th><th>Maturity Date</th></tr>";
                    string recipient = item2.EMAIL;
                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.STAFFID
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.productName}</td><td>{item3.loanTypeName}</td><td>{item3.productTypeName}</td>" + $"<td style='text-align:right;'>{item3.outstandingInterest:f}</td><td style='text-align:right;'>{item3.outstandingPrincipal:f}</td>" + $"<td>{item3.maturityDate:d}</td><td>{item3.disburseDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = "Dear Team, <br /><br />This is to bring your attention the following self-liquidating loans which are approaching expiry. <br /><br />" + $"{dataTable2}";
                    string otherRecipient = loanDetails.FirstOrDefault((LoanViewModel x) => x.relationshipOfficerId == item2.STAFFID).relationshipOfficerEmail;
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{otherRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Self Liquidating Loan Expiry was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Self Liquidating Loan Expiry logging has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendAlertsOnSelfLiquidatingLoanExpiryMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Product</th><th>Loan Type</th><th>Product Type</th><th>Outstanding Interest</th><th>Oustanding Principal</th><th>Disbursed Date</th><th>Maturity Date</th></tr>";
                foreach (LoanViewModel loanDetail in loanDetails)
                {
                    dataTable2 = dataTable2 + $"<tr><td>{loanDetail.loanReferenceNumber}</td><td>{loanDetail.productName}</td><td>{loanDetail.loanTypeName}</td><td>{loanDetail.productTypeName}</td>" + $"<td style='text-align:right;'>{loanDetail.outstandingInterest:f}</td><td style='text-align:right;'>{loanDetail.outstandingPrincipal:f}</td>" + $"<td>{loanDetail.maturityDate:d}</td><td>{loanDetail.disburseDate:d}</td></tr>";
                }
                dataTable2 += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following self-liquidating loans which are approaching expiry. <br /><br />" + $"{dataTable2}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Self Liquidating Loan Expiry was logged successfully, ");
                }
                else
                {
                    response += (response = " Self Liquidating Loan Expiry logging has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }






        public bool SendAlertsOnOverDraftLoansAlmostDue(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP alertOnOverDraftLoansAlmostDue = (from x in alertSetups
                                                                         where x.MONITORING_ITEMID == 3
                                                                         select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_REVOLVING
                                               join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                                               join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                               where a.TBL_PRODUCT.PRODUCTTYPEID == 6 && DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)applDate) <= (int?)alertOnOverDraftLoansAlmostDue.NOTIFICATION_PERIOD1
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = d.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   productName = b.PRODUCTNAME,
                                                   loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                                                   productTypeName = b.TBL_PRODUCT_TYPE.PRODUCTTYPENAME,
                                                   overdraftLimit = a.OVERDRAFTLIMIT,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL
                                               }).ToList();
            if (loanDetails != null)
            {
                SendAlertsOnOverDraftLoansAlmostDueRM(loanDetails, alertOnOverDraftLoansAlmostDue.MESSAGE_TITLE);
                if (alertOnOverDraftLoansAlmostDue.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelOne = (from x in loanDetails
                                                              where x.notificationDuration <= alertOnOverDraftLoansAlmostDue.NOTIFICATION_PERIOD1
                                                              select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertsOnOverDraftLoansAlmostDueMonitoringTeam(escalationLevelOne, alertOnOverDraftLoansAlmostDue);
                    }
                }
                if (alertOnOverDraftLoansAlmostDue.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelTwo = (from x in loanDetails
                                                              where x.notificationDuration <= alertOnOverDraftLoansAlmostDue.NOTIFICATION_PERIOD2
                                                              select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertsOnOverDraftLoansAlmostDueMonitoringTeam(escalationLevelTwo, alertOnOverDraftLoansAlmostDue);
                    }
                }
                if (alertOnOverDraftLoansAlmostDue.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelThree = (from x in loanDetails
                                                                where x.notificationDuration <= alertOnOverDraftLoansAlmostDue.NOTIFICATION_PERIOD3
                                                                select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        SendAlertsOnOverDraftLoansAlmostDueMonitoringTeam(escalationLevelThree, alertOnOverDraftLoansAlmostDue);
                    }
                }
                return true;
            }
            return false;
        }

        public void SendAlertsOnOverDraftLoansAlmostDueRM(List<LoanViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList2 = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList2 = (from x in staffList2
                              where dataList.Contains(x.STAFFID)
                              select x).ToList();
                foreach (TBL_STAFF item2 in staffList2)
                {
                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Product</th><th>Loan Type</th><th>Product Type</th><th>Overdraft Limit</th><th>Disbursed Date</th><th>Maturity Date</th></tr>";
                    string recipient = item2.EMAIL;
                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.STAFFID
                                                    select x).ToList();
                    foreach (LoanViewModel loanDetail in loanDetails)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{loanDetail.loanReferenceNumber}</td><td>{loanDetail.productName}</td><td>{loanDetail.loanTypeName}</td><td>{loanDetail.productTypeName}</td>" + $"<td style='text-align:right;'>{loanDetail.overdraftLimit:f}</td>" + $"<td>{loanDetail.maturityDate:d}</td><td>{loanDetail.disburseDate:d}</td><td>{loanDetail.maturityDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = "Dear Team, <br /><br />This is to bring your attention the following overdraft loans which are approaching expiry. <br /><br />" + $"{dataTable2}";
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Over Draft Loans Almost Due was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Over Draft Loans Almost Due has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendAlertsOnOverDraftLoansAlmostDueMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Product</th><th>Loan Type</th><th>Product Type</th><th>Overdraft Limit</th><th>Disbursed Date</th><th>Maturity Date</th></tr>";
                foreach (LoanViewModel loanDetail in loanDetails)
                {
                    dataTable2 = dataTable2 + $"<tr><td>{loanDetail.loanReferenceNumber}</td><td>{loanDetail.productName}</td><td>{loanDetail.loanTypeName}</td><td>{loanDetail.productTypeName}</td>" + $"<td style='text-align:right;'>{loanDetail.overdraftLimit:f}</td>" + $"<td>{loanDetail.maturityDate:d}</td><td>{loanDetail.disburseDate:d}</td><td>{loanDetail.maturityDate:d}</td></tr>";
                }
                dataTable2 += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following overdraft loans which are approaching expiry. <br /><br />" + $"{dataTable2}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Over Draft Loans Almost Due was logged successfully, ");
                }
                else
                {
                    response += (response = " Over Draft Loans Almost Due has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        public bool SendAlertsOnLoanCASAwithPND(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP CASAwithPND = (from x in alertSetups
                                                      where x.MONITORING_ITEMID == 9
                                                      select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where s.HASLIEN == true && (s.POSTNOSTATUSID == 2 || s.POSTNOSTATUSID == 4)
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
            if (loanDetails != null)
            {
                SendAlertsOnLoanCASAwithPNDrm(loanDetails, CASAwithPND.MESSAGE_TITLE);
                if (CASAwithPND.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelOne = (from x in loanDetails
                                                              where x.notificationDuration <= CASAwithPND.NOTIFICATION_PERIOD1
                                                              select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertsOnLoanCASAwithPNDmonitoringTeam(escalationLevelOne, CASAwithPND);
                    }
                }
                if (CASAwithPND.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelTwo = (from x in loanDetails
                                                              where x.notificationDuration <= CASAwithPND.NOTIFICATION_PERIOD2
                                                              select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertsOnLoanCASAwithPNDmonitoringTeam(escalationLevelTwo, CASAwithPND);
                    }
                }
                if (CASAwithPND.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelThree = (from x in loanDetails
                                                                where x.notificationDuration <= CASAwithPND.NOTIFICATION_PERIOD3
                                                                select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        SendAlertsOnLoanCASAwithPNDmonitoringTeam(escalationLevelThree, CASAwithPND);
                    }
                }
                return true;
            }
            return false;
        }

        public void SendAlertsOnLoanCASAwithPNDrm(List<LoanViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList2 = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList2 = (from x in staffList2
                              where dataList.Contains(x.STAFFID)
                              select x).ToList();
                foreach (TBL_STAFF item2 in staffList2)
                {
                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Principal Amount</th><th>Outstanding Principal</th><th>Tenor</th><th>Interest Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";
                    string recipient = item2.EMAIL;
                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.STAFFID
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.customerName}</td><td>{item3.principalAmount}</td><td>{item3.outstandingPrincipal}</td>" + $"<td style='text-align:right;'>{item3.tenor:f}</td>" + $"<td style='text-align:right;'>{item3.interestRate:f}</td>" + $"<td>{item3.bookingDate:d}</td><td>{item3.effectiveDate:d}</td><td>{item3.maturityDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following loans are on PND and lein placed on them. <br /><br />" + $"{dataTable2}";
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = title,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Loan CASA with PND was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Loan CASA with PND logging has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendAlertsOnLoanCASAwithPNDmonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Principal Amount</th><th>Outstanding Principal</th><th>Tenor</th><th>Interest Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";
                foreach (LoanViewModel loanDetail in loanDetails)
                {
                    dataTable2 = dataTable2 + $"<tr><td>{loanDetail.loanReferenceNumber}</td><td>{loanDetail.customerName}</td><td>{loanDetail.principalAmount}</td><td>{loanDetail.outstandingPrincipal}</td>" + $"<td style='text-align:right;'>{loanDetail.tenor:f}</td>" + $"<td style='text-align:right;'>{loanDetail.interestRate:f}</td>" + $"<td>{loanDetail.bookingDate:d}</td><td>{loanDetail.effectiveDate:d}</td><td>{loanDetail.maturityDate:d}</td></tr>";
                }
                dataTable2 += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following loans are on PND and lein placed on them. <br /><br />" + $"{dataTable2}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Loan CASA with PND was logged successfully, ");
                }
                else
                {
                    response += (response = " Loan CASA with PND logging has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        public bool SendAlertsOnInActiveBondAndGuarantee(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP InActiveBondAndGuarantee = (from x in alertSetups
                                                                   where x.MONITORING_ITEMID == 11
                                                                   select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)DateTime.Now) < (int?)InActiveBondAndGuarantee.NOTIFICATION_PERIOD1 && a.LOANSTATUSID == 1
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   principalAmount = a.CONTINGENTAMOUNT,
                                                   exchangeRate = a.EXCHANGERATE,
                                                   loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                   branchId = a.BRANCHID,
                                                   branchName = br.BRANCHNAME,
                                                   customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                                   notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)DateTime.Now)
                                               }).ToList();
            if (loanDetails != null)
            {
                SendAlertsOnInActiveBondAndGuaranteeRM(loanDetails, InActiveBondAndGuarantee.MESSAGE_TITLE);
                if (InActiveBondAndGuarantee.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelOne = (from x in loanDetails
                                                              where x.notificationDuration <= InActiveBondAndGuarantee.NOTIFICATION_PERIOD1
                                                              select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertsOnInActiveBondAndGuaranteeMonitoringTeam(escalationLevelOne, InActiveBondAndGuarantee);
                    }
                }
                if (InActiveBondAndGuarantee.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelTwo = (from x in loanDetails
                                                              where x.notificationDuration <= InActiveBondAndGuarantee.NOTIFICATION_PERIOD2
                                                              select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertsOnInActiveBondAndGuaranteeMonitoringTeam(escalationLevelTwo, InActiveBondAndGuarantee);
                    }
                }
                if (InActiveBondAndGuarantee.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelThree = (from x in loanDetails
                                                                where x.notificationDuration <= InActiveBondAndGuarantee.NOTIFICATION_PERIOD3
                                                                select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        SendAlertsOnInActiveBondAndGuaranteeMonitoringTeam(escalationLevelThree, InActiveBondAndGuarantee);
                    }
                }
                return true;
            }
            return false;
        }

        public void SendAlertsOnInActiveBondAndGuaranteeRM(List<LoanViewModel> loanDetails, string title)
        {
            try
            {
                List<StaffInfoViewModel> staffList2 = staffRepo.GetAllStaff().ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList2 = (from x in staffList2
                              where dataList.Contains(x.StaffId)
                              select x).ToList();
                foreach (StaffInfoViewModel item2 in staffList2)
                {
                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";
                    string recipient = item2.Email;
                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.StaffId
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.customerName}</td><td>{item3.principalAmount}</td><td>{item3.exchangeRate}</td>" + $"<td style='text-align:right;'>{item3.tenor:f}</td>" + $"<td style='text-align:right;'>{item3.interestRate:f}</td>" + $"<td>{item3.bookingDate:d}</td><td>{item3.effectiveDate:d}</td><td>{item3.maturityDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FirstName + " " + item2.LastName) + "This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                    string otherRecipient = loanDetails.FirstOrDefault((LoanViewModel x) => x.relationshipOfficerId == item2.StaffId).relationshipOfficerEmail;
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{otherRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Bond and guarantee about to expire was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Bond and guarantee about to expire logging has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendAlertsOnInActiveBondAndGuaranteeMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";
                foreach (LoanViewModel loanDetail in loanDetails)
                {
                    dataTable2 = dataTable2 + $"<tr><td>{loanDetail.loanReferenceNumber}</td><td>{loanDetail.customerName}</td><td>{loanDetail.principalAmount}</td><td>{loanDetail.exchangeRate}</td>" + $"<td style='text-align:right;'>{loanDetail.tenor:f}</td>" + $"<td style='text-align:right;'>{loanDetail.interestRate:f}</td>" + $"<td>{loanDetail.bookingDate:d}</td><td>{loanDetail.effectiveDate:d}</td><td>{loanDetail.maturityDate:d}</td></tr>";
                }
                dataTable2 += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Bond and guarantee about to expire was logged successfully, ");
                }
                else
                {
                    response += (response = " Bond and guarantee about to expire logging has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }






        public bool SendAlertsOnExpiredActiveBondAndGuarantee(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP ExpiredActiveBondAndGuarantee = (from x in alertSetups
                                                                        where x.MONITORING_ITEMID == 12
                                                                        select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where a.ISTENORED == false && DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)DateTime.Now) < (int?)0 && a.LOANSTATUSID == 1 && a.RELATED_LOAN_REFERENCE_NUMBER != string.Empty
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   principalAmount = a.CONTINGENTAMOUNT,
                                                   exchangeRate = a.EXCHANGERATE,
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
            if (loanDetails != null)
            {
                SendAlertsOnInActiveBondAndGuaranteeRM(loanDetails, ExpiredActiveBondAndGuarantee.MESSAGE_TITLE);
                SendAlertsOnExpiredActiveBondAndGuaranteeMonitoringTeam(loanDetails, ExpiredActiveBondAndGuarantee);
                return true;
            }
            return false;
        }

        public void ExpiredActiveBondAndGuaranteeRM(List<LoanViewModel> loanDetails, string title)
        {
            try
            {
                List<StaffInfoViewModel> staffList2 = staffRepo.GetAllStaff().ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList2 = (from x in staffList2
                              where dataList.Contains(x.StaffId)
                              select x).ToList();
                foreach (StaffInfoViewModel item2 in staffList2)
                {
                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";
                    string recipient = item2.Email;
                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.StaffId
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.customerName}</td><td>{item3.principalAmount}</td><td>{item3.exchangeRate}</td>" + $"<td style='text-align:right;'>{item3.tenor:f}</td>" + $"<td style='text-align:right;'>{item3.interestRate:f}</td>" + $"<td>{item3.bookingDate:d}</td><td>{item3.effectiveDate:d}</td><td>{item3.maturityDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FirstName + " " + item2.LastName) + "This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                    string otherRecipient = loanDetails.FirstOrDefault((LoanViewModel x) => x.relationshipOfficerId == item2.StaffId).relationshipOfficerEmail;
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{otherRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Bond and guarantee about to expire was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Bond and guarantee about to expire logging has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendAlertsOnExpiredActiveBondAndGuaranteeMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";
                foreach (LoanViewModel loanDetail in loanDetails)
                {
                    dataTable2 = dataTable2 + $"<tr><td>{loanDetail.loanReferenceNumber}</td><td>{loanDetail.customerName}</td><td>{loanDetail.principalAmount}</td><td>{loanDetail.exchangeRate}</td>" + $"<td style='text-align:right;'>{loanDetail.tenor:f}</td>" + $"<td style='text-align:right;'>{loanDetail.interestRate:f}</td>" + $"<td>{loanDetail.bookingDate:d}</td><td>{loanDetail.effectiveDate:d}</td><td>{loanDetail.maturityDate:d}</td></tr>";
                }
                dataTable2 += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Bond and guarantee about to expire was logged successfully, ");
                }
                else
                {
                    response += (response = " Bond and guarantee about to expire logging has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        public bool SendAlertOnAccountWithExeption(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP AccountWithExeption = (from x in alertSetups
                                                              where x.MONITORING_ITEMID == 13
                                                              select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails3 = (from a in context.TBL_LOAN_CONTINGENT
                                                join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                                join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                                join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                                where DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate) <= (int?)AccountWithExeption.NOTIFICATION_PERIOD1 && s.AVAILABLEBALANCE < 0m
                                                select new LoanViewModel
                                                {
                                                    applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                    loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                    bookingDate = a.BOOKINGDATE,
                                                    disburseDate = a.DISBURSEDATE,
                                                    maturityDate = a.MATURITYDATE,
                                                    principalAmount = a.CONTINGENTAMOUNT,
                                                    exchangeRate = a.EXCHANGERATE,
                                                    loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                                                    relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                    relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                    relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                    relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                    relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                    relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                    branchId = a.BRANCHID,
                                                    branchName = br.BRANCHNAME,
                                                    customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                                    notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate)
                                                }).ToList();
            List<LoanViewModel> loanDetails4 = (from a in context.TBL_LOAN
                                                join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                                join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                                join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                                where DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate) <= (int?)AccountWithExeption.NOTIFICATION_PERIOD1 && s.AVAILABLEBALANCE < 0m
                                                select new LoanViewModel
                                                {
                                                    applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                    loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                    bookingDate = a.BOOKINGDATE,
                                                    disburseDate = a.DISBURSEDATE,
                                                    maturityDate = a.MATURITYDATE,
                                                    principalAmount = (decimal)s.OVERDRAFTAMOUNT,
                                                    exchangeRate = a.EXCHANGERATE,
                                                    loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                                                    relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                    relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                    relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                    relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                    relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                    relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                    branchId = a.BRANCHID,
                                                    branchName = br.BRANCHNAME,
                                                    customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                                    notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate)
                                                }).ToList();
            List<LoanViewModel> loanDetails2 = (from a in context.TBL_LOAN_REVOLVING
                                                join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                                join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                                join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                                where DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate) <= (int?)AccountWithExeption.NOTIFICATION_PERIOD1 && s.AVAILABLEBALANCE < 0m
                                                select new LoanViewModel
                                                {
                                                    applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                    loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                    bookingDate = a.BOOKINGDATE,
                                                    disburseDate = a.DISBURSEDATE,
                                                    maturityDate = a.MATURITYDATE,
                                                    principalAmount = (decimal)s.OVERDRAFTAMOUNT,
                                                    exchangeRate = a.EXCHANGERATE,
                                                    loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                                                    relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                    relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                    relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                    relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                    relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                    relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                    branchId = a.BRANCHID,
                                                    branchName = br.BRANCHNAME,
                                                    customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                                    notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate)
                                                }).ToList();
            IEnumerable<LoanViewModel> loanDetails = loanDetails3.ToList().Union(loanDetails4.ToList()).Union(loanDetails2.ToList());
            if (loanDetails != null)
            {
                SendAlertsOnAccountWithExeptionRM(loanDetails.ToList(), AccountWithExeption.MESSAGE_TITLE);
                if (AccountWithExeption.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelOne = (from x in loanDetails
                                                              where x.notificationDuration <= AccountWithExeption.NOTIFICATION_PERIOD1
                                                              select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertsOnAccountWithExeptionMonitoringTeam(escalationLevelOne, AccountWithExeption);
                    }
                }
                if (AccountWithExeption.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelTwo = (from x in loanDetails
                                                              where x.notificationDuration <= AccountWithExeption.NOTIFICATION_PERIOD2
                                                              select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertsOnAccountWithExeptionMonitoringTeam(escalationLevelTwo, AccountWithExeption);
                    }
                }
                if (AccountWithExeption.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelThree = (from x in loanDetails
                                                                where x.notificationDuration <= AccountWithExeption.NOTIFICATION_PERIOD3
                                                                select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        SendAlertsOnAccountWithExeptionMonitoringTeam(escalationLevelThree, AccountWithExeption);
                    }
                }
                return true;
            }
            return false;
        }

        public void SendAlertsOnAccountWithExeptionRM(List<LoanViewModel> loanDetails, string title)
        {
            try
            {
                List<StaffInfoViewModel> staffList2 = staffRepo.GetAllStaff().ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList2 = (from x in staffList2
                              where dataList.Contains(x.StaffId)
                              select x).ToList();
                foreach (StaffInfoViewModel item2 in staffList2)
                {
                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";
                    string recipient = item2.Email;
                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.StaffId
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.customerName}</td><td>{item3.principalAmount}</td><td>{item3.exchangeRate}</td>" + $"<td style='text-align:right;'>{item3.tenor:f}</td>" + $"<td style='text-align:right;'>{item3.interestRate:f}</td>" + $"<td>{item3.bookingDate:d}</td><td>{item3.effectiveDate:d}</td><td>{item3.maturityDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FirstName + " " + item2.LastName) + "This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                    string otherRecipient = loanDetails.FirstOrDefault((LoanViewModel x) => x.relationshipOfficerId == item2.StaffId).relationshipOfficerEmail;
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{otherRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Bond and guarantee about to expire was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Bond and guarantee about to expire logging has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendAlertsOnAccountWithExeptionMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";
                foreach (LoanViewModel loanDetail in loanDetails)
                {
                    dataTable2 = dataTable2 + $"<tr><td>{loanDetail.loanReferenceNumber}</td><td>{loanDetail.customerName}</td><td>{loanDetail.principalAmount}</td><td>{loanDetail.exchangeRate}</td>" + $"<td style='text-align:right;'>{loanDetail.tenor:f}</td>" + $"<td style='text-align:right;'>{loanDetail.interestRate:f}</td>" + $"<td>{loanDetail.bookingDate:d}</td><td>{loanDetail.effectiveDate:d}</td><td>{loanDetail.maturityDate:d}</td></tr>";
                }
                dataTable2 += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Bond and guarantee about to expire was logged successfully, ");
                }
                else
                {
                    response += (response = " Bond and guarantee about to expire logging has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }






        public bool SendAlertOnPastDueObligationAccounts(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP PastDueObligationAccounts = (from x in alertSetups
                                                                    where x.MONITORING_ITEMID == 14
                                                                    select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate) <= (int?)PastDueObligationAccounts.NOTIFICATION_PERIOD1 && s.AVAILABLEBALANCE < 0m
                                               select new LoanViewModel
                                               {
                                                   applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                   loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                   bookingDate = a.BOOKINGDATE,
                                                   disburseDate = a.DISBURSEDATE,
                                                   maturityDate = a.MATURITYDATE,
                                                   principalAmount = a.CONTINGENTAMOUNT,
                                                   exchangeRate = a.EXCHANGERATE,
                                                   loanTypeName = a.TBL_LOAN_TYPE.LOANTYPENAME,
                                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = a.TBL_STAFF1.FIRSTNAME + " " + a.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = a.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = a.TBL_STAFF.EMAIL,
                                                   branchId = a.BRANCHID,
                                                   branchName = br.BRANCHNAME,
                                                   customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                                   notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate)
                                               }).ToList();
            if (loanDetails != null)
            {
                SendAlertsOnPastDueObligationAccountsRM(loanDetails.ToList(), PastDueObligationAccounts.MESSAGE_TITLE);
                if (PastDueObligationAccounts.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelOne = (from x in loanDetails
                                                              where x.notificationDuration <= PastDueObligationAccounts.NOTIFICATION_PERIOD1
                                                              select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertsOnPastDueObligationAccountsMonitoringTeam(escalationLevelOne, PastDueObligationAccounts);
                    }
                }
                if (PastDueObligationAccounts.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelTwo = (from x in loanDetails
                                                              where x.notificationDuration <= PastDueObligationAccounts.NOTIFICATION_PERIOD2
                                                              select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertsOnPastDueObligationAccountsMonitoringTeam(escalationLevelTwo, PastDueObligationAccounts);
                    }
                }
                if (PastDueObligationAccounts.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelThree = (from x in loanDetails
                                                                where x.notificationDuration <= PastDueObligationAccounts.NOTIFICATION_PERIOD3
                                                                select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        SendAlertsOnPastDueObligationAccountsMonitoringTeam(escalationLevelThree, PastDueObligationAccounts);
                    }
                }
                return true;
            }
            return false;
        }

        public void SendAlertsOnPastDueObligationAccountsRM(List<LoanViewModel> loanDetails, string title)
        {
            try
            {
                List<StaffInfoViewModel> staffList2 = staffRepo.GetAllStaff().ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList2 = (from x in staffList2
                              where dataList.Contains(x.StaffId)
                              select x).ToList();
                foreach (StaffInfoViewModel item2 in staffList2)
                {
                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";
                    string recipient = item2.Email;
                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.StaffId
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.customerName}</td><td>{item3.principalAmount}</td><td>{item3.exchangeRate}</td>" + $"<td style='text-align:right;'>{item3.tenor:f}</td>" + $"<td style='text-align:right;'>{item3.interestRate:f}</td>" + $"<td>{item3.bookingDate:d}</td><td>{item3.effectiveDate:d}</td><td>{item3.maturityDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FirstName + " " + item2.LastName) + "This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                    string otherRecipient = loanDetails.FirstOrDefault((LoanViewModel x) => x.relationshipOfficerId == item2.StaffId).relationshipOfficerEmail;
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = $"{recipient};{otherRecipient}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Bond and guarantee about to expire was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Bond and guarantee about to expire logging has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendAlertsOnPastDueObligationAccountsMonitoringTeam(List<LoanViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";
                foreach (LoanViewModel loanDetail in loanDetails)
                {
                    dataTable2 = dataTable2 + $"<tr><td>{loanDetail.loanReferenceNumber}</td><td>{loanDetail.customerName}</td><td>{loanDetail.principalAmount}</td><td>{loanDetail.exchangeRate}</td>" + $"<td style='text-align:right;'>{loanDetail.tenor:f}</td>" + $"<td style='text-align:right;'>{loanDetail.interestRate:f}</td>" + $"<td>{loanDetail.bookingDate:d}</td><td>{loanDetail.effectiveDate:d}</td><td>{loanDetail.maturityDate:d}</td></tr>";
                }
                dataTable2 += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Bond and guarantee about to expire was logged successfully, ");
                }
                else
                {
                    response += (response = " Bond and guarantee about to expire logging has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }






        public bool SendAlertOnInsuranceApprochingExpiration(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP InsuranceApprochingExpiration = (from x in alertSetups
                                                                        where x.MONITORING_ITEMID == 15
                                                                        select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<CollateralViewModel> loanDetails = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     join p in context.TBL_COLLATERAL_ITEM_POLICY on a.COLLATERALCUSTOMERID equals p.COLLATERALCUSTOMERID
                                                     where DbFunctions.DiffDays((DateTime?)p.ENDDATE, (DateTime?)currentDate) <= (int?)InsuranceApprochingExpiration.NOTIFICATION_PERIOD1
                                                     select new CollateralViewModel
                                                     {
                                                         collateralType = d.COLLATERALTYPENAME,
                                                         collateralCode = a.COLLATERALCODE,
                                                         collateralSubType = e.COLLATERALSUBTYPENAME,
                                                         customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                                         propertyName = f.PROPERTYNAME,
                                                         lastValuationDate = f.LASTVALUATIONDATE,
                                                         collateralValue = (decimal?)a.COLLATERALVALUE,
                                                         valuationCycle = a.VALUATIONCYCLE,
                                                         insuranceCompany = p.INSURANCECOMPANYNAME,
                                                         startDate = (DateTime?)p.STARTDATE,
                                                         endDate = (DateTime?)p.ENDDATE,
                                                         notificationDuration = (int)DbFunctions.DiffDays((DateTime?)p.ENDDATE, (DateTime?)currentDate)
                                                     }).ToList();
            if (loanDetails != null)
            {
                SendAlertOnInsuranceApprochingExpirationRM(loanDetails, InsuranceApprochingExpiration.MESSAGE_TITLE);
                if (InsuranceApprochingExpiration.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelOne = (from x in loanDetails
                                                                    where x.notificationDuration <= InsuranceApprochingExpiration.NOTIFICATION_PERIOD1
                                                                    select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertOnInsuranceApprochingExpirationMonitoringTeam(escalationLevelOne, InsuranceApprochingExpiration);
                    }
                }
                if (InsuranceApprochingExpiration.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelTwo = (from x in loanDetails
                                                                    where x.notificationDuration <= InsuranceApprochingExpiration.NOTIFICATION_PERIOD2
                                                                    select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertOnInsuranceApprochingExpirationMonitoringTeam(escalationLevelTwo, InsuranceApprochingExpiration);
                    }
                }
                if (InsuranceApprochingExpiration.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelThree = (from x in loanDetails
                                                                      where x.notificationDuration <= InsuranceApprochingExpiration.NOTIFICATION_PERIOD3
                                                                      select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        SendAlertOnInsuranceApprochingExpirationMonitoringTeam(escalationLevelThree, InsuranceApprochingExpiration);
                    }
                }
                return true;
            }
            return false;
        }

        public void SendAlertOnInsuranceApprochingExpirationRM(List<CollateralViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList2 = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList2 = (from x in staffList2
                              where dataList.Contains(x.STAFFID)
                              select x).ToList();
                foreach (TBL_STAFF item2 in staffList2)
                {
                    string recipient = item2.EMAIL;
                    List<CollateralViewModel> mailList = (from x in loanDetails
                                                          where x.relationshipManagerId == item2.STAFFID
                                                          select x).ToList();
                    string dataTable2 = "<table><tr><th>Collateral Code</th><th>Collateral Type</th><th>Collateral Sub Type</th><th>Customer Name</th><th>Property</th><th>Last Valuation Date</th><th>Collateral Value</th><th>Valuation Cycle</th><th>Insurance Company</th><th>Start Date</th><th>End Date</th></tr>";
                    foreach (CollateralViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.collateralCode}</td><td>{item3.collateralType}</td><td>{item3.collateralSubType}</td><td>{item3.customerName}</td>" + $"<td>{item3.propertyName}</td><td>{item3.lastValuationDate:d}</td>><td>{item3.collateralValue:d}</td>" + $"><td>{item3.valuationCycle:d}</td>><td>{item3.insuranceCompany:d}</td>><td>{item3.startDate:d}</td>" + $"><td>{item3.endDate:d}</td>></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following Insurance has expired. <br /><br />" + $"{dataTable2}";
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = recipient,
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Expired Insurance was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Expired Insurance logging has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SendAlertOnInsuranceApprochingExpirationMonitoringTeam(List<CollateralViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable2 = "<table><tr><th>Collateral Code</th><th>Collateral Type</th><th>Collateral Sub Type</th><th>Customer Name</th><th>Property</th><th>Last Valuation Date</th><th>Collateral Value</th><th>Valuation Cycle</th><th>Insurance Company</th><th>Start Date</th><th>End Date</th></tr>";
                foreach (CollateralViewModel loanDetail in loanDetails)
                {
                    dataTable2 = dataTable2 + $"<tr><td>{loanDetail.collateralCode}</td><td>{loanDetail.collateralType}</td><td>{loanDetail.collateralSubType}</td><td>{loanDetail.customerName}</td>" + $"<td>{loanDetail.propertyName}</td><td>{loanDetail.lastValuationDate:d}</td>><td>{loanDetail.collateralValue:d}</td>" + $"><td>{loanDetail.valuationCycle:d}</td>><td>{loanDetail.insuranceCompany:d}</td>><td>{loanDetail.startDate:d}</td>" + $"><td>{loanDetail.endDate:d}</td>></tr>";
                }
                dataTable2 += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following Insurance has expired. <br /><br />" + $"{dataTable2}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = recipient,
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Expired Insurance was logged successfully, ");
                }
                else
                {
                    response += (response = " Expired Insurance logging has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }






        public bool SendAlertForExpiredInsurance(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP ExpiredInsurance = (from x in alertSetups
                                                           where x.MONITORING_ITEMID == 16
                                                           select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<CollateralViewModel> loanDetails = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     join p in context.TBL_COLLATERAL_ITEM_POLICY on a.COLLATERALCUSTOMERID equals p.COLLATERALCUSTOMERID
                                                     where DbFunctions.DiffDays((DateTime?)p.ENDDATE, (DateTime?)DateTime.Now) <= (int?)ExpiredInsurance.NOTIFICATION_PERIOD1 && p.HASEXPIRED == true
                                                     select new CollateralViewModel
                                                     {
                                                         collateralType = d.COLLATERALTYPENAME,
                                                         collateralCode = a.COLLATERALCODE,
                                                         collateralSubType = e.COLLATERALSUBTYPENAME,
                                                         customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                                         propertyName = f.PROPERTYNAME,
                                                         lastValuationDate = f.LASTVALUATIONDATE,
                                                         collateralValue = (decimal?)a.COLLATERALVALUE,
                                                         valuationCycle = a.VALUATIONCYCLE,
                                                         insuranceCompany = p.INSURANCECOMPANYNAME,
                                                         startDate = (DateTime?)p.STARTDATE,
                                                         endDate = (DateTime?)p.ENDDATE,
                                                         notificationDuration = (int)DbFunctions.DiffDays((DateTime?)p.ENDDATE, (DateTime?)DateTime.Now)
                                                     }).ToList();
            if (loanDetails != null)
            {
                SendAlertOnExpiredInsuranceRM(loanDetails, ExpiredInsurance.MESSAGE_TITLE);
                if (ExpiredInsurance.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelOne = (from x in loanDetails
                                                                    where x.notificationDuration <= ExpiredInsurance.NOTIFICATION_PERIOD1
                                                                    select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertOnExpiredInsuranceMonitoringTeam(escalationLevelOne, ExpiredInsurance);
                    }
                }
                if (ExpiredInsurance.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelTwo = (from x in loanDetails
                                                                    where x.notificationDuration <= ExpiredInsurance.NOTIFICATION_PERIOD2
                                                                    select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertOnExpiredInsuranceMonitoringTeam(escalationLevelTwo, ExpiredInsurance);
                    }
                }
                if (ExpiredInsurance.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelThree = (from x in loanDetails
                                                                      where x.notificationDuration <= ExpiredInsurance.NOTIFICATION_PERIOD3
                                                                      select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        SendAlertOnExpiredInsuranceMonitoringTeam(escalationLevelThree, ExpiredInsurance);
                    }
                }
                return true;
            }
            return false;
        }

        public void SendAlertOnExpiredInsuranceRM(List<CollateralViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList2 = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList2 = (from x in staffList2
                              where dataList.Contains(x.STAFFID)
                              select x).ToList();
                foreach (TBL_STAFF item2 in staffList2)
                {
                    string recipient = item2.EMAIL;
                    List<CollateralViewModel> mailList = (from x in loanDetails
                                                          where x.relationshipManagerId == item2.STAFFID
                                                          select x).ToList();
                    string dataTable2 = "<table><tr><th>Collateral Code</th><th>Collateral Type</th><th>Collateral Sub Type</th><th>Customer Name</th><th>Property</th><th>Last Valuation Date</th><th>Collateral Value</th><th>Valuation Cycle</th><th>Insurance Company</th><th>Start Date</th><th>End Date</th></tr>";
                    foreach (CollateralViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.collateralCode}</td><td>{item3.collateralType}</td><td>{item3.collateralSubType}</td><td>{item3.customerName}</td>" + $"<td>{item3.propertyName}</td><td>{item3.lastValuationDate:d}</td>><td>{item3.collateralValue:d}</td>" + $"><td>{item3.valuationCycle:d}</td>><td>{item3.insuranceCompany:d}</td>><td>{item3.startDate:d}</td>" + $"><td>{item3.endDate:d}</td>></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following Insurance has expired. <br /><br />" + $"{dataTable2}";
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = recipient,
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Expired Insurance was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Expired Insurance logging has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SendAlertOnExpiredInsuranceMonitoringTeam(List<CollateralViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable2 = "<table><tr><th>Collateral Code</th><th>Collateral Type</th><th>Collateral Sub Type</th><th>Customer Name</th><th>Property</th><th>Last Valuation Date</th><th>Collateral Value</th><th>Valuation Cycle</th><th>Insurance Company</th><th>Start Date</th><th>End Date</th></tr>";
                foreach (CollateralViewModel loanDetail in loanDetails)
                {
                    dataTable2 = dataTable2 + $"<tr><td>{loanDetail.collateralCode}</td><td>{loanDetail.collateralType}</td><td>{loanDetail.collateralSubType}</td><td>{loanDetail.customerName}</td>" + $"<td>{loanDetail.propertyName}</td><td>{loanDetail.lastValuationDate:d}</td>><td>{loanDetail.collateralValue:d}</td>" + $"><td>{loanDetail.valuationCycle:d}</td>><td>{loanDetail.insuranceCompany:d}</td>><td>{loanDetail.startDate:d}</td>" + $"><td>{loanDetail.endDate:d}</td>></tr>";
                }
                dataTable2 += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following Insurance has expired. <br /><br />" + $"{dataTable2}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = recipient,
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Expired Insurance was logged successfully, ");
                }
                else
                {
                    response += (response = " Expired Insurance logging has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }






        public bool SendAlertOnTurnoverCovenant(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP TurnoverCovenant = (from x in alertSetups
                                                           where x.MONITORING_ITEMID == 17
                                                           select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanCovenantDetailViewModel> loanDetails = (from a in context.TBL_LOAN_COVENANT_DETAIL
                                                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                                             join c in context.TBL_STAFF on b.RELATIONSHIPMANAGERID equals c.STAFFID
                                                             join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                                                             join ca in context.TBL_CASA on b.CASAACCOUNTID equals ca.CASAACCOUNTID
                                                             join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                                             join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals (short?)f.FREQUENCYTYPEID
                                                             join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                                                             where DbFunctions.DiffDays((DateTime?)a.NEXTCOVENANTDATE.Value, (DateTime?)currentDate) <= (int?)TurnoverCovenant.NOTIFICATION_PERIOD1 && (decimal?)ca.AVAILABLEBALANCE < a.COVENANTAMOUNT
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
                                                                 notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.NEXTCOVENANTDATE.Value, (DateTime?)currentDate)
                                                             }).ToList();
            if (loanDetails != null)
            {
                SendAlertOnTurnoverCovenantRM(loanDetails, TurnoverCovenant.MESSAGE_TITLE);
                if (TurnoverCovenant.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelOne = (from x in loanDetails
                                                                            where x.notificationDuration <= TurnoverCovenant.NOTIFICATION_PERIOD1
                                                                            select x).ToList();
                    if (escalationLevelOne != null)
                    {
                        SendAlertOnTurnoverCovenantMonitoringTeam(escalationLevelOne, TurnoverCovenant);
                    }
                }
                if (TurnoverCovenant.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelTwo = (from x in loanDetails
                                                                            where x.notificationDuration <= TurnoverCovenant.NOTIFICATION_PERIOD2
                                                                            select x).ToList();
                    if (escalationLevelTwo != null)
                    {
                        SendAlertOnTurnoverCovenantMonitoringTeam(escalationLevelTwo, TurnoverCovenant);
                    }
                }
                if (TurnoverCovenant.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelThree = (from x in loanDetails
                                                                              where x.notificationDuration <= TurnoverCovenant.NOTIFICATION_PERIOD3
                                                                              select x).ToList();
                    if (escalationLevelThree != null)
                    {
                        SendAlertOnTurnoverCovenantMonitoringTeam(escalationLevelThree, TurnoverCovenant);
                    }
                }
                return true;
            }
            return false;
        }

        public void SendAlertOnTurnoverCovenantRM(List<LoanCovenantDetailViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList2 = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                staffList2 = (from x in staffList2
                              where dataList.Contains(x.STAFFID)
                              select x).ToList();
                foreach (TBL_STAFF item2 in staffList2)
                {
                    string recipient = item2.EMAIL;
                    List<LoanCovenantDetailViewModel> mailList = (from x in loanDetails
                                                                  where x.relationshipManagerId == item2.STAFFID
                                                                  select x).ToList();
                    string dataTable2 = "<table><tr><th>Covenant Type</th><th>Covenant Amount</th><th>Covenant Date</th><th>Covenant Detail</th><th>Effective Date</th><th>Due Date</th></tr>";
                    foreach (LoanCovenantDetailViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.covenantTypeName}</td><td>{item3.covenantAmount}</td><td>{item3.covenantDate}</td><td>{item3.covenantDetail}</td>" + $"<td>{item3.effectiveDate:d}</td>><td>{item3.dueDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following Insurance has expired. <br /><br />" + $"{dataTable2}";
                    string templateUrl = "~/EmailTemplates/Monitoring.html";
                    string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                    MessageLogViewModel messageModel = new MessageLogViewModel
                    {
                        MessageSubject = messageSubject,
                        MessageBody = mailBody,
                        MessageStatusId = 1,
                        MessageTypeId = 1,
                        FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                        ToAddress = recipient,
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };
                    if (SaveMessageDetails(messageModel) != 0)
                    {
                        response += (response = " Expired Insurance was logged successfully, ");
                    }
                    else
                    {
                        response += (response = " Expired Insurance logging has failed, ");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SendAlertOnTurnoverCovenantMonitoringTeam(List<LoanCovenantDetailViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable2 = "<table><tr><th>Covenant Type</th><th>Covenant Amount</th><th>Covenant Date</th><th>Covenant Detail</th><th>Effective Date</th><th>Due Date</th></tr>";
                foreach (LoanCovenantDetailViewModel loanDetail in loanDetails)
                {
                    dataTable2 = dataTable2 + $"<tr><td>{loanDetail.covenantTypeName}</td><td>{loanDetail.covenantAmount}</td><td>{loanDetail.covenantDate}</td><td>{loanDetail.covenantDetail}</td>" + $"<td>{loanDetail.effectiveDate:d}</td>><td>{loanDetail.dueDate:d}</td></tr>";
                }
                dataTable2 += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following Insurance has expired. <br /><br />" + $"{dataTable2}";
                string templateUrl = "~/EmailTemplates/Monitoring.html";
                string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = recipient,
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now
                };
                if (SaveMessageDetails(messageModel) != 0)
                {
                    response += (response = " Expired Insurance was logged successfully, ");
                }
                else
                {
                    response += (response = " Expired Insurance logging has failed, ");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }




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
                throw new Exception(ex.Message);
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
        //        throw new Exception(ex.Message);
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