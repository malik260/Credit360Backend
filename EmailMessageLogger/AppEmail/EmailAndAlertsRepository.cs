using EmailMessageLogger;
using FintrakBanking.Common.AlertMonitoring;
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


        public bool SendAlertsForCovenantsApproachingDueDate(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP alertsetupForCovenantsApproachingDueDate = (from x in alertSetups
                                                                                   where x.MONITORING_ITEMID == (int)AlertMessageEnum.LoanCovenantApproachingDueDates
                                                                                   select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanCovenantDetailViewModel> loanDetails = (from a in context.TBL_LOAN_COVENANT_DETAIL
                                                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                                             join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                                                             join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                                             join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals (short?)f.FREQUENCYTYPEID
                                                             join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                                                             where DbFunctions.DiffDays(a.NEXTCOVENANTDATE, currentDate) < alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD1
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
            if (loanDetails.Count != 0)
            {
                SendAlertsForCovenantsApproachingDueDateToRM(loanDetails, alertsetupForCovenantsApproachingDueDate.MESSAGE_TITLE);
                if (alertsetupForCovenantsApproachingDueDate.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelOne = (from x in loanDetails
                                                                            where x.notificationDuration <= alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD1
                                                                            select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertsForCovenantsApproachingDueDateToMonitoringTeam(escalationLevelOne, alertsetupForCovenantsApproachingDueDate);
                    }
                }
                if (alertsetupForCovenantsApproachingDueDate.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelTwo = (from x in loanDetails
                                                                            where x.notificationDuration <= alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD2
                                                                            select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertsForCovenantsApproachingDueDateToMonitoringTeam(escalationLevelTwo, alertsetupForCovenantsApproachingDueDate);
                    }
                }
                if (alertsetupForCovenantsApproachingDueDate.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelThree = (from x in loanDetails
                                                                              where x.notificationDuration <= alertsetupForCovenantsApproachingDueDate.NOTIFICATION_PERIOD3
                                                                              select x).ToList();
                    if (escalationLevelThree.Count != 0)
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
               
               var RMdetail = (from x in staffList
                             where dataList.Contains(x.STAFFID)
                             select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o=>o.STAFFCODE==item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;

                    string dataTable = "<table><tr><th>Application Ref</th><th>Covenant Detail</th><th>Covenant Type</th><th>Amount</th><th>Covenant Date</th><th>Due Date</th></tr>";
                    List<LoanCovenantDetailViewModel> mailList = (from x in loanDetails
                                                                  where x.relationshipManagerId == item2.STAFFID
                                                                  select x).ToList();
                    foreach (LoanCovenantDetailViewModel item3 in mailList)
                    {
                        dataTable = dataTable + $"<tr><td>{item3.loanRefNumber}</td><td>{item3.covenantDetail}</td><td>{item3.covenantTypeName}</td>" + $"<td>{item3.covenantAmount}</td><td>{item3.covenantDate:d}</td><td>{item3.dueDate:d}</td></tr>";
                        dataTable += "</table>";
                    }
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following loan covenants which are approaching their due date. <br /><br />" + $"{dataTable}";
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
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following loan covenants which are approaching their due date. <br /><br />" + $"{dataTable}";
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
                                                                        where x.MONITORING_ITEMID == (int)AlertMessageEnum.LoanCoveantOverdue
                                                                        select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanCovenantDetailViewModel> loanDetails = (from a in context.TBL_LOAN_COVENANT_DETAIL
                                                             join b in context.TBL_LOAN on a.LOANID equals b.TERMLOANID
                                                             join c in context.TBL_STAFF on b.RELATIONSHIPMANAGERID equals c.STAFFID
                                                             join d in context.TBL_STAFF on b.RELATIONSHIPOFFICERID equals d.STAFFID
                                                             join e in context.TBL_LOAN_APPLICATION_DETAIL on b.LOANAPPLICATIONDETAILID equals e.LOANAPPLICATIONDETAILID
                                                             join f in context.TBL_FREQUENCY_TYPE on a.FREQUENCYTYPEID equals (short?)f.FREQUENCYTYPEID
                                                             join g in context.TBL_LOAN_COVENANT_TYPE on a.COVENANTTYPEID equals g.COVENANTTYPEID
                                                             where DbFunctions.DiffDays((DateTime?)a.NEXTCOVENANTDATE.Value, (DateTime?)currentDate) < (int?)alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
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
            if (loanDetails.Count != 0)
            {
                SendAlertsForCovenantsOverDueRM(loanDetails, alertsetupForCovenantsOverDue.MESSAGE_TITLE);
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelOne = (from x in loanDetails
                                                                            where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
                                                                            select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertsForCovenantsOverDueMonitoringTeam(loanDetails, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelTwo = (from x in loanDetails
                                                                            where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD2
                                                                            select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertsForCovenantsOverDueMonitoringTeam(loanDetails, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelThree = (from x in loanDetails
                                                                              where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD3
                                                                              select x).ToList();
                    if (escalationLevelThree.Count != 0)
                    {
                        SendAlertsForCovenantsOverDueMonitoringTeam(loanDetails, alertsetupForCovenantsOverDue);
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

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;
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
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following loan covenants which have pass their due date. <br /><br />" + $"{dataTable2}";
                    string additionalRecipient = loanDetails.FirstOrDefault((LoanCovenantDetailViewModel x) => x.relationshipOfficerId == item2.STAFFID).officerEmail;
                    string templateUrl = "EmailTemplates\\Monitoring.html";
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
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following loan covenants which have pass their due date. <br /><br />" + $"{dataTable}";
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





        public bool SendAlertsForCollateralPropertyApproachingRevaluation(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP alertsetupForCovenantsOverDue = (from x in alertSetups
                                                                        where x.MONITORING_ITEMID == (int)AlertMessageEnum.CollateralApproachingRevaluation
                                                                        select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<CollateralViewModel> loanDetails = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     where DbFunctions.DiffDays((DateTime?)f.LASTVALUATIONDATE, (DateTime?)currentDate) < (int?)alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
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
            if (loanDetails.Count != 0)
            {
                SendAlertsForCollateralPropertyApproachingRevaluationRM(loanDetails, alertsetupForCovenantsOverDue.MESSAGE_TITLE);
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelOne = (from x in loanDetails
                                                                    where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
                                                                    select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertsForCollateralPropertyApproachingRevaluationMonitoringTeam(escalationLevelOne, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelTwo = (from x in loanDetails
                                                                    where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD2
                                                                    select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertsForCollateralPropertyApproachingRevaluationMonitoringTeam(escalationLevelTwo, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelThree = (from x in loanDetails
                                                                      where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD3
                                                                      select x).ToList();
                    if (escalationLevelThree.Count != 0)
                    {
                        SendAlertsForCollateralPropertyApproachingRevaluationMonitoringTeam(escalationLevelThree, alertsetupForCovenantsOverDue);
                    }
                }
                return true;
            }
            return false;
        }
        public void SendAlertsForCollateralPropertyApproachingRevaluationRM(List<CollateralViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;

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
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
        public void SendAlertsForCollateralPropertyApproachingRevaluationMonitoringTeam(List<CollateralViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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





        public bool SendAlertsForCollateralPropertyDueForRevaluation(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP alertsetupForCovenantsOverDue = (from x in alertSetups
                                                                        where x.MONITORING_ITEMID == (int)AlertMessageEnum.CollateralDueForRevaluation
                                                                        select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<CollateralViewModel> loanDetails = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     where DbFunctions.DiffDays((DateTime?)f.LASTVALUATIONDATE, (DateTime?)currentDate) < (int?)alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
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
            if (loanDetails.Count != 0)
            {
                SendAlertsForCollateralPropertyDueForRevaluationRM(loanDetails, alertsetupForCovenantsOverDue.MESSAGE_TITLE);
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelOne = (from x in loanDetails
                                                                    where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
                                                                    select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertsForCollateralPropertyDueForRevaluationMonitoringTeam(escalationLevelOne, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelTwo = (from x in loanDetails
                                                                    where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD2
                                                                    select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertsForCollateralPropertyDueForRevaluationMonitoringTeam(escalationLevelTwo, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelThree = (from x in loanDetails
                                                                      where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD3
                                                                      select x).ToList();
                    if (escalationLevelThree.Count != 0)
                    {
                        SendAlertsForCollateralPropertyDueForRevaluationMonitoringTeam(escalationLevelThree, alertsetupForCovenantsOverDue);
                    }
                }
                return true;
            }
            return false;
        }
        public void SendAlertsForCollateralPropertyDueForRevaluationRM(List<CollateralViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;

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
                    string templateUrl = "EmailTemplates\\Monitoring.html";
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
        public void SendAlertsForCollateralPropertyDueForRevaluationMonitoringTeam(List<CollateralViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
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
                string templateUrl = "EmailTemplates\\Monitoring.html";
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
                                                   loanTypeName = b.TBL_LOAN_APPLICATION_DETAIL.TBL_LOAN_APPLICATION.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                                   relationshipManagerId = b.RELATIONSHIPMANAGERID,
                                                   relationshipManagerName = b.TBL_STAFF1.FIRSTNAME + " " + b.TBL_STAFF1.LASTNAME,
                                                   relationshipManagerEmail = b.TBL_STAFF1.EMAIL,
                                                   relationshipOfficerId = b.RELATIONSHIPOFFICERID,
                                                   relationshipOfficerName = b.TBL_STAFF.FIRSTNAME + " " + b.TBL_STAFF.LASTNAME,
                                                   relationshipOfficerEmail = b.TBL_STAFF.EMAIL
                                               }).ToList();
            if (loanDetails.Count != 0)
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

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;

                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Loan Type</th><th>Outstanding Interest</th><th>Oustanding Principal</th><th>Booking Date</th><th>Disbursed Date</th></tr>";

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
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                                                                           where x.MONITORING_ITEMID == (int)AlertMessageEnum.SelfLiquidatingLoanExpiry
                                                                           select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN
                                               join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                                               join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                               where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.SelfLiquidating && DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)applDate) < (int?)alertOnSelfLiquidatingLoanExpiry.NOTIFICATION_PERIOD1
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
            if (loanDetails.Count != 0)
            {
                SendAlertsOnSelfLiquidatingLoanExpiryRM(loanDetails, alertOnSelfLiquidatingLoanExpiry.MESSAGE_TITLE);
                if (alertOnSelfLiquidatingLoanExpiry.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelOne = (from x in loanDetails
                                                              where x.notificationDuration <= alertOnSelfLiquidatingLoanExpiry.NOTIFICATION_PERIOD1
                                                              select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertsOnSelfLiquidatingLoanExpiryMonitoringTeam(escalationLevelOne, alertOnSelfLiquidatingLoanExpiry);
                    }
                }
                if (alertOnSelfLiquidatingLoanExpiry.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelTwo = (from x in loanDetails
                                                              where x.notificationDuration <= alertOnSelfLiquidatingLoanExpiry.NOTIFICATION_PERIOD2
                                                              select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertsOnSelfLiquidatingLoanExpiryMonitoringTeam(escalationLevelTwo, alertOnSelfLiquidatingLoanExpiry);
                    }
                }
                if (alertOnSelfLiquidatingLoanExpiry.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelThree = (from x in loanDetails
                                                                where x.notificationDuration <= alertOnSelfLiquidatingLoanExpiry.NOTIFICATION_PERIOD3
                                                                select x).ToList();
                    if (escalationLevelThree.Count != 0)
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
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;

                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Product</th><th>Loan Type</th><th>Product Type</th><th>Outstanding Interest</th><th>Oustanding Principal</th><th>Disbursed Date</th><th>Maturity Date</th></tr>";

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
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                                                                         where x.MONITORING_ITEMID == (int)AlertMessageEnum.OverdraftLoansAlmostDue
                                                                         select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_REVOLVING
                                               join b in context.TBL_PRODUCT on a.PRODUCTID equals b.PRODUCTID
                                               join c in context.TBL_PRODUCT_TYPE on b.PRODUCTTYPEID equals c.PRODUCTTYPEID
                                               join d in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                               where a.TBL_PRODUCT.PRODUCTTYPEID == (int)LoanProductTypeEnum.RevolvingLoan && DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)applDate) < (int?)alertOnOverDraftLoansAlmostDue.NOTIFICATION_PERIOD1
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
            if (loanDetails.Count != 0)
            {
                SendAlertsOnOverDraftLoansAlmostDueRM(loanDetails, alertOnOverDraftLoansAlmostDue.MESSAGE_TITLE);
                if (alertOnOverDraftLoansAlmostDue.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelOne = (from x in loanDetails
                                                              where x.notificationDuration <= alertOnOverDraftLoansAlmostDue.NOTIFICATION_PERIOD1
                                                              select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertsOnOverDraftLoansAlmostDueMonitoringTeam(escalationLevelOne, alertOnOverDraftLoansAlmostDue);
                    }
                }
                if (alertOnOverDraftLoansAlmostDue.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelTwo = (from x in loanDetails
                                                              where x.notificationDuration <= alertOnOverDraftLoansAlmostDue.NOTIFICATION_PERIOD2
                                                              select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertsOnOverDraftLoansAlmostDueMonitoringTeam(escalationLevelTwo, alertOnOverDraftLoansAlmostDue);
                    }
                }
                if (alertOnOverDraftLoansAlmostDue.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelThree = (from x in loanDetails
                                                                where x.notificationDuration <= alertOnOverDraftLoansAlmostDue.NOTIFICATION_PERIOD3
                                                                select x).ToList();
                    if (escalationLevelThree.Count != 0)
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
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;
                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Product</th><th>Loan Type</th><th>Product Type</th><th>Overdraft Limit</th><th>Disbursed Date</th><th>Maturity Date</th></tr>";

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
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                                                      where x.MONITORING_ITEMID == 2
                                                      select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where s.HASLIEN == true && (s.POSTNOSTATUSID == (int)LoanStatusEnum.Suspended || s.POSTNOSTATUSID == (int)LoanStatusEnum.Terminated)
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
            if (loanDetails.Count != 0)
            {
                SendAlertsOnLoanCASAwithPNDrm(loanDetails, CASAwithPND.MESSAGE_TITLE);
                if (CASAwithPND.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelOne = (from x in loanDetails
                                                              where x.notificationDuration <= CASAwithPND.NOTIFICATION_PERIOD1
                                                              select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertsOnLoanCASAwithPNDmonitoringTeam(escalationLevelOne, CASAwithPND);
                    }
                }
                if (CASAwithPND.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelTwo = (from x in loanDetails
                                                              where x.notificationDuration <= CASAwithPND.NOTIFICATION_PERIOD2
                                                              select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertsOnLoanCASAwithPNDmonitoringTeam(escalationLevelTwo, CASAwithPND);
                    }
                }
                if (CASAwithPND.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelThree = (from x in loanDetails
                                                                where x.notificationDuration <= CASAwithPND.NOTIFICATION_PERIOD3
                                                                select x).ToList();
                    if (escalationLevelThree.Count != 0)
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
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;
                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Principal Amount</th><th>Outstanding Principal</th><th>Tenor</th><th>Interest Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";

                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.STAFFID
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.customerName}</td><td>{item3.principalAmount}</td><td>{item3.outstandingPrincipal}</td>" + $"<td style='text-align:right;'>{item3.tenor:f}</td>" + $"<td style='text-align:right;'>{item3.interestRate:f}</td>" + $"<td>{item3.bookingDate:d}</td><td>{item3.effectiveDate:d}</td><td>{item3.maturityDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following loans are on PND and lein placed on them. <br /><br />" + $"{dataTable2}";
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                                                                   where x.MONITORING_ITEMID == (int)AlertMessageEnum.InactiveBondAndGuarantee
                                                                   select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)DateTime.Now) < (int?)InActiveBondAndGuarantee.NOTIFICATION_PERIOD1 && a.LOANSTATUSID == (int)LoanStatusEnum.Inactive
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
                                                   customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                                   notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)DateTime.Now)
                                               }).ToList();
            if (loanDetails.Count != 0)
            {
                SendAlertsOnInActiveBondAndGuaranteeRM(loanDetails, InActiveBondAndGuarantee.MESSAGE_TITLE);
                if (InActiveBondAndGuarantee.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelOne = (from x in loanDetails
                                                              where x.notificationDuration <= InActiveBondAndGuarantee.NOTIFICATION_PERIOD1
                                                              select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertsOnInActiveBondAndGuaranteeMonitoringTeam(escalationLevelOne, InActiveBondAndGuarantee);
                    }
                }
                if (InActiveBondAndGuarantee.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelTwo = (from x in loanDetails
                                                              where x.notificationDuration <= InActiveBondAndGuarantee.NOTIFICATION_PERIOD2
                                                              select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertsOnInActiveBondAndGuaranteeMonitoringTeam(escalationLevelTwo, InActiveBondAndGuarantee);
                    }
                }
                if (InActiveBondAndGuarantee.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelThree = (from x in loanDetails
                                                                where x.notificationDuration <= InActiveBondAndGuarantee.NOTIFICATION_PERIOD3
                                                                select x).ToList();
                    if (escalationLevelThree.Count != 0)
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
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;

                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";

                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.STAFFID
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.customerName}</td><td>{item3.principalAmount}</td><td>{item3.exchangeRate}</td>" + $"<td style='text-align:right;'>{item3.tenor:f}</td>" + $"<td style='text-align:right;'>{item3.interestRate:f}</td>" + $"<td>{item3.bookingDate:d}</td><td>{item3.effectiveDate:d}</td><td>{item3.maturityDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                    string otherRecipient = loanDetails.FirstOrDefault((LoanViewModel x) => x.relationshipOfficerId == item2.STAFFID).relationshipOfficerEmail;
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                                                                        where x.MONITORING_ITEMID == (int)AlertMessageEnum.ExpiredActiveBondAndGuarantee
                                                                        select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where a.ISTENORED == false && DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)DateTime.Now) < (int?)0 && a.LOANSTATUSID == (int)LoanStatusEnum.Active && a.RELATED_LOAN_REFERENCE_NUMBER != string.Empty
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
            if (loanDetails.Count != 0)
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
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;

                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";
                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.STAFFID
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.customerName}</td><td>{item3.principalAmount}</td><td>{item3.exchangeRate}</td>" + $"<td style='text-align:right;'>{item3.tenor:f}</td>" + $"<td style='text-align:right;'>{item3.interestRate:f}</td>" + $"<td>{item3.bookingDate:d}</td><td>{item3.effectiveDate:d}</td><td>{item3.maturityDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                    string otherRecipient = loanDetails.FirstOrDefault((LoanViewModel x) => x.relationshipOfficerId == item2.STAFFID).relationshipOfficerEmail;
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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





        public bool SendAlertOnAccountWithExeption_Overdrawn(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP AccountWithExeption = (from x in alertSetups
                                                              where x.MONITORING_ITEMID == (int)AlertMessageEnum.OverdrawnAccount
                                                              select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                                join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                                join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                                join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                                where DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate) < (int?)AccountWithExeption.NOTIFICATION_PERIOD1 && s.AVAILABLEBALANCE < 0m
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
                                                    customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                                    notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate)
                                                }).ToList();
            
            if (loanDetails.Count !=0)
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
        public bool SendAlertOnAccountWithExeption_Watchist(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP AccountWithExeption = (from x in alertSetups
                                                              where x.MONITORING_ITEMID == (int)AlertMessageEnum.WatchListedAccount
                                                              select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
           
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN
                                                join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                                join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                                join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                                where a.INT_PRUDENT_GUIDELINE_STATUSID == (int)LoanPrudentialStatusEnum.WatchList
                                               select new LoanViewModel
                                                {
                                                    applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                    loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                    bookingDate = a.BOOKINGDATE,
                                                    disburseDate = a.DISBURSEDATE,
                                                    maturityDate = a.MATURITYDATE,
                                                    principalAmount = (decimal)s.OVERDRAFTAMOUNT,
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
                                                    customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                                    notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate)
                                                }).ToList();
            
            if (loanDetails.Count != 0)
            {
                SendAlertsOnAccountWithExeptionRM(loanDetails, AccountWithExeption.MESSAGE_TITLE);
                SendAlertsOnAccountWithExeptionMonitoringTeam(loanDetails, AccountWithExeption);
                
                return true;
            }
            return false;
        }
        public bool SendAlertOnAccountWithExeption_Unauthorized(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP AccountWithExeption = (from x in alertSetups
                                                              where x.MONITORING_ITEMID == (int)AlertMessageEnum.AuathorizedAccount
                                                              select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
          
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_REVOLVING
                                                join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                                join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                                join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                                where DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate) <= (int?)AccountWithExeption.NOTIFICATION_PERIOD1 && s.AVAILABLEBALANCE < 0
                                                select new LoanViewModel
                                                {
                                                    applicationReferenceNumber = a.LOANREFERENCENUMBER,
                                                    loanReferenceNumber = a.LOANREFERENCENUMBER,
                                                    bookingDate = a.BOOKINGDATE,
                                                    disburseDate = a.DISBURSEDATE,
                                                    maturityDate = a.MATURITYDATE,
                                                    principalAmount = (decimal)s.OVERDRAFTAMOUNT,
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
                                                    customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                                    notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate)
                                                }).ToList();
            if (loanDetails.Count != 0)
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
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;

                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";

                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.STAFFID
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.customerName}</td><td>{item3.principalAmount}</td><td>{item3.exchangeRate}</td>" + $"<td style='text-align:right;'>{item3.tenor:f}</td>" + $"<td style='text-align:right;'>{item3.interestRate:f}</td>" + $"<td>{item3.bookingDate:d}</td><td>{item3.effectiveDate:d}</td><td>{item3.maturityDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                    string otherRecipient = loanDetails.FirstOrDefault((LoanViewModel x) => x.relationshipOfficerId == item2.STAFFID).relationshipOfficerEmail;
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                                                                    where x.MONITORING_ITEMID == (int)AlertMessageEnum.PastDueObligations
                                                                    select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<LoanViewModel> loanDetails = (from a in context.TBL_LOAN_CONTINGENT
                                               join s in context.TBL_CASA on a.CASAACCOUNTID equals s.CASAACCOUNTID
                                               join br in context.TBL_BRANCH on a.BRANCHID equals br.BRANCHID
                                               join cs in context.TBL_CUSTOMER on a.CUSTOMERID equals cs.CUSTOMERID
                                               where DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate) < (int?)PastDueObligationAccounts.NOTIFICATION_PERIOD1 && s.AVAILABLEBALANCE < 0m
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
                                                   customerName = cs.FIRSTNAME + " " + cs.MAIDENNAME + " " + cs.LASTNAME,
                                                   notificationDuration = (int)DbFunctions.DiffDays((DateTime?)a.MATURITYDATE, (DateTime?)currentDate)
                                               }).ToList();
            if (loanDetails.Count != 0)
            {
                SendAlertsOnPastDueObligationAccountsRM(loanDetails.ToList(), PastDueObligationAccounts.MESSAGE_TITLE);
                if (PastDueObligationAccounts.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelOne = (from x in loanDetails
                                                              where x.notificationDuration <= PastDueObligationAccounts.NOTIFICATION_PERIOD1
                                                              select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertsOnPastDueObligationAccountsMonitoringTeam(escalationLevelOne, PastDueObligationAccounts);
                    }
                }
                if (PastDueObligationAccounts.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelTwo = (from x in loanDetails
                                                              where x.notificationDuration <= PastDueObligationAccounts.NOTIFICATION_PERIOD2
                                                              select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertsOnPastDueObligationAccountsMonitoringTeam(escalationLevelTwo, PastDueObligationAccounts);
                    }
                }
                if (PastDueObligationAccounts.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanViewModel> escalationLevelThree = (from x in loanDetails
                                                                where x.notificationDuration <= PastDueObligationAccounts.NOTIFICATION_PERIOD3
                                                                select x).ToList();
                    if (escalationLevelThree.Count != 0)
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
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();
                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;

                    string dataTable2 = "<table><tr><th>Loan Ref #</th><th>Customer Name</th><th>Contingent Amount</th><th>exchange Rate</th><th>Booking Date</th><th>Effective Date</th><th>Maturity Date</th></tr>";

                    List<LoanViewModel> mailList = (from x in loanDetails
                                                    where x.relationshipManagerId == item2.STAFFID
                                                    select x).ToList();
                    foreach (LoanViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.loanReferenceNumber}</td><td>{item3.customerName}</td><td>{item3.principalAmount}</td><td>{item3.exchangeRate}</td>" + $"<td style='text-align:right;'>{item3.tenor:f}</td>" + $"<td style='text-align:right;'>{item3.interestRate:f}</td>" + $"<td>{item3.bookingDate:d}</td><td>{item3.effectiveDate:d}</td><td>{item3.maturityDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageSubject = ConfigurationManager.AppSettings["messageSubject"] + " " + title;
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following Bond and guarantee  are about to expire. <br /><br />" + $"{dataTable2}";
                    string otherRecipient = loanDetails.FirstOrDefault((LoanViewModel x) => x.relationshipOfficerId == item2.STAFFID).relationshipOfficerEmail;
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                                                                        where x.MONITORING_ITEMID == (int)AlertMessageEnum.CovenantsInsuranceApproachingDueDate
                                                                        select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<CollateralViewModel> loanDetails = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     join p in context.TBL_COLLATERAL_ITEM_POLICY on a.COLLATERALCUSTOMERID equals p.COLLATERALCUSTOMERID
                                                     where DbFunctions.DiffDays((DateTime?)p.ENDDATE, (DateTime?)currentDate) < (int?)InsuranceApprochingExpiration.NOTIFICATION_PERIOD1
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
            if (loanDetails.Count != 0)
            {
                SendAlertOnInsuranceApprochingExpirationRM(loanDetails, InsuranceApprochingExpiration.MESSAGE_TITLE);
                if (InsuranceApprochingExpiration.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelOne = (from x in loanDetails
                                                                    where x.notificationDuration <= InsuranceApprochingExpiration.NOTIFICATION_PERIOD1
                                                                    select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertOnInsuranceApprochingExpirationMonitoringTeam(escalationLevelOne, InsuranceApprochingExpiration);
                    }
                }
                if (InsuranceApprochingExpiration.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelTwo = (from x in loanDetails
                                                                    where x.notificationDuration <= InsuranceApprochingExpiration.NOTIFICATION_PERIOD2
                                                                    select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertOnInsuranceApprochingExpirationMonitoringTeam(escalationLevelTwo, InsuranceApprochingExpiration);
                    }
                }
                if (InsuranceApprochingExpiration.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelThree = (from x in loanDetails
                                                                      where x.notificationDuration <= InsuranceApprochingExpiration.NOTIFICATION_PERIOD3
                                                                      select x).ToList();
                    if (escalationLevelThree.Count != 0)
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
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;

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
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                                                           where x.MONITORING_ITEMID == (int)AlertMessageEnum.ExpiredInsurance
                                                           select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<CollateralViewModel> loanDetails = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                     join p in context.TBL_COLLATERAL_ITEM_POLICY on a.COLLATERALCUSTOMERID equals p.COLLATERALCUSTOMERID
                                                     where DbFunctions.DiffDays((DateTime?)p.ENDDATE, (DateTime?)currentDate) < (int?)ExpiredInsurance.NOTIFICATION_PERIOD1 && p.HASEXPIRED == true
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
            if (loanDetails.Count != 0)
            {
                SendAlertOnExpiredInsuranceRM(loanDetails, ExpiredInsurance.MESSAGE_TITLE);
                if (ExpiredInsurance.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelOne = (from x in loanDetails
                                                                    where x.notificationDuration <= ExpiredInsurance.NOTIFICATION_PERIOD1
                                                                    select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertOnExpiredInsuranceMonitoringTeam(escalationLevelOne, ExpiredInsurance);
                    }
                }
                if (ExpiredInsurance.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelTwo = (from x in loanDetails
                                                                    where x.notificationDuration <= ExpiredInsurance.NOTIFICATION_PERIOD2
                                                                    select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertOnExpiredInsuranceMonitoringTeam(escalationLevelTwo, ExpiredInsurance);
                    }
                }
                if (ExpiredInsurance.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelThree = (from x in loanDetails
                                                                      where x.notificationDuration <= ExpiredInsurance.NOTIFICATION_PERIOD3
                                                                      select x).ToList();
                    if (escalationLevelThree.Count != 0)
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
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;
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
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                                                           where x.MONITORING_ITEMID == (int)AlertMessageEnum.TurnoverCovenantNotMet
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
                                                             where DbFunctions.DiffDays((DateTime?)a.NEXTCOVENANTDATE, (DateTime?)currentDate) < (int?)TurnoverCovenant.NOTIFICATION_PERIOD1 && (decimal?)ca.AVAILABLEBALANCE < a.COVENANTAMOUNT
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
            if (loanDetails.Count != 0)
            {
                SendAlertOnTurnoverCovenantRM(loanDetails, TurnoverCovenant.MESSAGE_TITLE);
                if (TurnoverCovenant.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelOne = (from x in loanDetails
                                                                            where x.notificationDuration <= TurnoverCovenant.NOTIFICATION_PERIOD1
                                                                            select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertOnTurnoverCovenantMonitoringTeam(escalationLevelOne, TurnoverCovenant);
                    }
                }
                if (TurnoverCovenant.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelTwo = (from x in loanDetails
                                                                            where x.notificationDuration <= TurnoverCovenant.NOTIFICATION_PERIOD2
                                                                            select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertOnTurnoverCovenantMonitoringTeam(escalationLevelTwo, TurnoverCovenant);
                    }
                }
                if (TurnoverCovenant.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<LoanCovenantDetailViewModel> escalationLevelThree = (from x in loanDetails
                                                                              where x.notificationDuration <= TurnoverCovenant.NOTIFICATION_PERIOD3
                                                                              select x).ToList();
                    if (escalationLevelThree.Count != 0)
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
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;
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
                    string templateUrl =  "EmailTemplates\\Monitoring.html";
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
                string templateUrl =  "EmailTemplates\\Monitoring.html";
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




        public bool SendAlertsForCollateralPropertyDueForVisitation(string title, string messageBody, List<TBL_MONITORING_ALERT_SETUP> alertSetups)
        {
            TBL_MONITORING_ALERT_SETUP alertsetupForCovenantsOverDue = (from x in alertSetups
                                                                        where x.MONITORING_ITEMID == (int)AlertMessageEnum.CollateralApproachingRevaluation
                                                                        select x).FirstOrDefault();
            DateTime currentDate = DateTime.Now;
            List<CollateralViewModel> loanDetails = (from a in context.TBL_COLLATERAL_CUSTOMER
                                                     join b in context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                                     join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                                                     join d in context.TBL_COLLATERAL_TYPE on a.COLLATERALTYPEID equals d.COLLATERALTYPEID
                                                     join e in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals e.COLLATERALSUBTYPEID
                                                     join f in context.TBL_COLLATERAL_IMMOVE_PROPERTY on a.COLLATERALCUSTOMERID equals f.COLLATERALCUSTOMERID
                                                      join v in context.TBL_COLLATERAL_VISITATION on a.COLLATERALCUSTOMERID equals v.COLLATERALCUSTOMERID
                                                     where DbFunctions.DiffDays((DateTime?)v.VISITATIONDATE, (DateTime?)currentDate) < (int?)alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
                                                     && d.REQUIREVISITATION == true
                                                     select new CollateralViewModel
                                                     {
                                                         collateralTypeId = a.COLLATERALTYPEID,
                                                         collateralType = d.COLLATERALTYPENAME,
                                                         collateralCode = a.COLLATERALCODE,
                                                         collateralSubType = e.COLLATERALSUBTYPENAME,
                                                         customerName = b.FIRSTNAME + " " + b.LASTNAME,
                                                         propertyName = f.PROPERTYNAME,
                                                         lastVisitationDate = v.VISITATIONDATE,
                                                         nextVisitationDate = v.VISITATIONDATE,
                                                         visitationCycle = (int)e.VISITATIONCYCLE,
                                                         relationshipManagerId = a.CREATEDBY,
                                                         relationshipManager = c.FIRSTNAME + " " + c.LASTNAME,
                                                         relationshipManagerEmail = c.EMAIL,
                                                         notificationDuration = (int)DbFunctions.DiffDays((DateTime?)f.LASTVALUATIONDATE, (DateTime?)currentDate)
                                                     }).ToList();
            if (loanDetails.Count != 0)
            {
                SendAlertsForCollateralPropertyDueForVisitationRM(loanDetails, alertsetupForCovenantsOverDue.MESSAGE_TITLE);
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS1.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelOne = (from x in loanDetails
                                                                    where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD1
                                                                    select x).ToList();
                    if (escalationLevelOne.Count != 0)
                    {
                        SendAlertsForCollateralPropertyDueForVisitationMonitoringTeam(escalationLevelOne, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS2.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelTwo = (from x in loanDetails
                                                                    where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD2
                                                                    select x).ToList();
                    if (escalationLevelTwo.Count != 0)
                    {
                        SendAlertsForCollateralPropertyDueForVisitationMonitoringTeam(escalationLevelTwo, alertsetupForCovenantsOverDue);
                    }
                }
                if (alertsetupForCovenantsOverDue.RECIPIENTEMAILS3.Trim() != string.Empty)
                {
                    List<CollateralViewModel> escalationLevelThree = (from x in loanDetails
                                                                      where x.notificationDuration <= alertsetupForCovenantsOverDue.NOTIFICATION_PERIOD3
                                                                      select x).ToList();
                    if (escalationLevelThree.Count != 0)
                    {
                        SendAlertsForCollateralPropertyDueForVisitationMonitoringTeam(escalationLevelThree, alertsetupForCovenantsOverDue);
                    }
                }
                return true;
            }
            return false;
        }
        public void SendAlertsForCollateralPropertyDueForVisitationRM(List<CollateralViewModel> loanDetails, string title)
        {
            try
            {
                List<TBL_STAFF> staffList = context.TBL_STAFF.ToList();
                List<int> dataList = (from g in loanDetails
                                      select g.relationshipManagerId).ToList();

                var RMdetail = (from x in staffList
                                where dataList.Contains(x.STAFFID)
                                select x).ToList();

                foreach (TBL_STAFF item2 in RMdetail)
                {
                    var bankManagerID = staffList.Where(o => o.STAFFCODE == item2.STAFFCODE).FirstOrDefault().SUPERVISOR_STAFFID;
                    var bankManagerEmail = staffList.Where(o => o.STATEID == item2.STATEID).FirstOrDefault().EMAIL;

                    string recipient = item2.EMAIL.Trim() + ";" + bankManagerEmail;

                    List<CollateralViewModel> mailList = (from x in loanDetails
                                                          where x.relationshipManagerId == item2.STAFFID
                                                          select x).ToList();
                    string dataTable2 = "<table><tr><th>Collateral Code</th><th>Collateral Type</th><th>Collateral Sub Type</th><th>Property</th><th>Last Visitation Date</th><th>Visitation Cycle</th><th>Next Visitation Date</th></tr>";
                    foreach (CollateralViewModel item3 in mailList)
                    {
                        dataTable2 = dataTable2 + $"<tr><td>{item3.collateralCode}</td><td>{item3.collateralType}</td><td>{item3.collateralSubType}</td>" + $"<td>{item3.propertyName}</td><td>{item3.lastVisitationDate:d}</td><td>{item3.visitationCycle}</td><td>{item3.nextVisitationDate:d}</td></tr>";
                    }
                    dataTable2 += "</table>";
                    string messageContent = string.Format("Dear {0}, <br /><br />", item2.FIRSTNAME + " " + item2.LASTNAME) + "This is to bring your attention the following collaterals which are due for visitation. <br /><br />" + $"{dataTable2}";
                    string templateUrl = "EmailTemplates\\Monitoring.html";
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
        public void SendAlertsForCollateralPropertyDueForVisitationMonitoringTeam(List<CollateralViewModel> loanDetails, TBL_MONITORING_ALERT_SETUP alertSetups)
        {
            try
            {
                string recipient = alertSetups.RECIPIENTEMAILS2.Trim();
                string dataTable = "<table><tr><th>Collateral Code</th><th>Collateral Type</th><th>Collateral Sub Type</th><th>Property</th><th>Last Visitation Date</th><th>Visitation Cycle</th><th>Next Visitation Date</th></tr>";
                foreach (CollateralViewModel item3 in loanDetails)
                {
                    dataTable = dataTable + $"<tr><td>{item3.collateralCode}</td><td>{item3.collateralType}</td><td>{item3.collateralSubType}</td>" + $"<td>{item3.propertyName}</td><td>{item3.lastVisitationDate:d}</td><td>{item3.visitationCycle}</td><td>{item3.nextVisitationDate:d}</td></tr>";
                }
                dataTable += "</table>";
                string messageSubject = alertSetups.MESSAGE_TITLE;
                string messageContent = "Dear Team, <br /><br />This is to bring your attention the following collaterals which are due for visitation. <br /><br />" + $"{dataTable}";
                string templateUrl = "EmailTemplates\\Monitoring.html";
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


    }
}