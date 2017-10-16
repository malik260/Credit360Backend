using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class EmailAndAlertsRepository : IEmailAndAlertsRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private EmailHelpers emailHelpers;

        public EmailAndAlertsRepository(FinTrakBankingContext _context, IAuditTrailRepository _auditTrail,
            EmailHelpers _emailHelpers)
        {
            context = _context;
            auditTrail = _auditTrail;
            emailHelpers = _emailHelpers;
        }

        #region Covenant Monitoring

        public void SendAlertsForCovenantsApproachingDueDate()
        {
            var data = (from a in context.tbl_Loan_Covenant_Detail
                        join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                        join c in context.tbl_Staff on b.RelationshipManagerId equals c.StaffId
                        join d in context.tbl_Staff on b.RelationshipOfficerId equals d.StaffId
                        join e in context.tbl_Loan_Application_Detail on b.LoanApplicationDetailId equals e.LoanApplicationDetailId
                        join f in context.tbl_Frequency_Type on a.FrequencyTypeId equals f.FrequencyTypeId
                        join g in context.tbl_Loan_Covenant_Type on a.CovenantTypeId equals g.CovenantTypeId
                        where SqlFunctions.DateDiff("DAY", a.CovenantDate, a.NextCovenantDate) == 4
                        select new LoanCovenantDetailViewModel
                        {
                            companyId = a.CompanyId,
                            covenantAmount = a.CovenantAmount,
                            covenantDate = a.CovenantDate,
                            dueDate = a.NextCovenantDate,
                            covenantDetail = a.CovenantDetail,
                            covenantTypeId = a.CovenantTypeId,
                            covenantTypeName = g.CovenantTypeName,
                            frequencyTypeId = a.FrequencyTypeId,
                            frequencyTypeName = f.Mode,
                            loanId = a.LoanId,
                            //loanRefNumber = e.ApplicationReferenceNumber,
                            relationshipManager = c.FirstName + " " + c.LastName,
                            managerEmail = c.Email,
                            relationshipOfficer = d.FirstName + " " + d.LastName,
                            officerEmail = d.Email,
                        }).ToList();

            try
            {
                foreach (var item in data)
                {
                    var recipient = item.officerEmail;

                    var additionalRecipient = item.managerEmail;

                    var messageSubject = "FIRSTBANKONLINE (REMINDER) - LOAN COVENANTS APPROACHING DUE DATE";

                    var dataTable =
                        "<table><tr><th>Application Ref</th><th>Covenant Detail</th><th>Covenant Type</th>" +
                        "<th>Amount</th><th>Covenant Date</th><th>Due Date</th></tr>" +
                        $"<tr><td>{item.loanRefNumber}</td><td>{item.covenantDetail}</td><td>{item.covenantTypeName}</td>" +
                        $"<td>{item.covenantAmount}</td><td>{item.covenantDate:d}</td><td>{item.dueDate:d}</td></tr>";

                    dataTable = dataTable + "</table>";

                    var messageContent = $"Dear {item.relationshipOfficer}, <br /><br />" +
                                         "This is to bring your attention the following loan covenants " +
                                         "which are approaching their due date for revaluation. <br /><br />" +
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
                        ToAddress = item.officerEmail,
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = item.dueDate.Value
                    };

                    SaveMessageDetails(messageModel);

                    emailHelpers.SendMail(recipient, additionalRecipient, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void SendAlertsForCovenantsOverDue()
        {
            var data = (from a in context.tbl_Loan_Covenant_Detail
                        join b in context.tbl_Loan on a.LoanId equals b.TermLoanId
                        join c in context.tbl_Staff on b.RelationshipManagerId equals c.StaffId
                        join d in context.tbl_Staff on b.RelationshipOfficerId equals d.StaffId
                        join e in context.tbl_Loan_Application_Detail on b.LoanApplicationDetailId equals e.LoanApplicationDetailId
                        join f in context.tbl_Frequency_Type on a.FrequencyTypeId equals f.FrequencyTypeId
                        join g in context.tbl_Loan_Covenant_Type on a.CovenantTypeId equals g.CovenantTypeId
                        where a.NextCovenantDate.Value >= DateTime.Now
                        select new LoanCovenantDetailViewModel
                        {
                            companyId = a.CompanyId,
                            covenantAmount = a.CovenantAmount,
                            covenantDate = a.CovenantDate,
                            dueDate = a.NextCovenantDate,
                            covenantDetail = a.CovenantDetail,
                            covenantTypeId = a.CovenantTypeId,
                            covenantTypeName = g.CovenantTypeName,
                            frequencyTypeId = a.FrequencyTypeId,
                            frequencyTypeName = f.Mode,
                            loanId = a.LoanId,
                            //loanRefNumber = e.ApplicationReferenceNumber,
                            relationshipManager = c.FirstName + " " + c.LastName,
                            managerEmail = c.Email,
                            relationshipOfficer = d.FirstName + " " + d.LastName,
                            officerEmail = d.Email,
                        }).ToList();

            try
            {
                foreach (var item in data)
                {
                    var recipient = item.officerEmail;

                    var additionalRecipient = item.managerEmail;

                    var messageSubject = "FIRSTBANKONLINE (REMINDER) - LOAN COVENANTS OVERDUE";

                    var dataTable =
                        "<table><tr><th>Application Ref</th><th>Covenant Detail</th><th>Covenant Type</th>" +
                        "<th>Amount</th><th>Covenant Date</th><th>Due Date</th></tr>" +
                        $"<tr><td>{item.loanRefNumber}</td><td>{item.covenantDetail}</td><td>{item.covenantTypeName}</td>" +
                        $"<td>{item.covenantAmount}</td><td>{item.covenantDate:d}</td><td>{item.dueDate:d}</td></tr>";

                    dataTable = dataTable + "</table>";

                    var messageContent = $"Dear {item.relationshipOfficer}, <br /><br />" +
                                         "This is to bring your attention the following loan covenants " +
                                         "which are overdue for revaluation. <br /><br />" +
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
                        ToAddress = item.officerEmail,
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = (DateTime)item.dueDate
                    };

                    SaveMessageDetails(messageModel);

                    emailHelpers.SendMail(recipient, additionalRecipient, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion Covenant Monitoring

        #region Collateral Monitoring

        public void SendAlertsForCollateralPropertyRevaluation()
        {
            var data = (from a in context.tbl_Collateral_Customer
                        join b in context.tbl_Customer on a.CustomerId equals b.CustomerId
                        join c in context.tbl_Staff on a.CreatedBy equals c.StaffId
                        join d in context.tbl_Collateral_Type on a.CollateralTypeId equals d.CollateralTypeId
                        join e in context.tbl_Collateral_Type_Sub on a.CollateralSubTypeId equals e.CollateralSubTypeId
                        join f in context.tbl_Collateral_Immovable_Property on a.CollateralCustomerId equals f
                            .CollateralCustomerId
                        where a.IsLocationBased
                        select new CollateralViewModel
                        {
                            collateralTypeId = a.CollateralTypeId,
                            collateralType = d.CollateralTypeName,
                            collateralCode = a.CollateralCode,
                            collateralSubType = e.CollateralSubTypeName,
                            customerName = b.FirstName + " " + b.LastName,
                            propertyName = f.PropertyName,
                            lastValuationDate = f.LastValuationDate,
                            relationshipManagerId = a.CreatedBy,
                            relationshipManager = c.FirstName + " " + c.LastName,
                            relationshipManagerEmail = c.Email
                        }).ToList();

            try
            {
                foreach (var item in data)
                {
                    var recipient = item.relationshipManagerEmail;

                    var messageSubject = "FIRSTBANKONLINE (REMINDER) - COLLATERAL DUE FOR RE-EVALUATION";

                    var dataTable =
                        "<table><tr><th>Collateral Code</th><th>Collateral Type</th>" +
                        "<th>Collateral Sub Type</th><th>Property</th><th>Last Valuation Date</th></tr>" +
                        $"<tr><td>{item.collateralCode}</td><td>{item.collateralType}</td><td>{item.collateralSubType}</td>" +
                        $"<td>{item.propertyName}</td><td>{item.lastValuationDate}</td></tr>";

                    dataTable = dataTable + "</table>";

                    var messageContent = $"Dear {item.relationshipManager}, <br /><br />" +
                                         "This is to bring your attention the following collaterals " +
                                         "which are due for revaluation. <br /><br />" +
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
                        ToAddress = item.relationshipManagerEmail,
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = (DateTime)item.lastValuationDate
                    };

                    SaveMessageDetails(messageModel);

                    emailHelpers.SendMail(recipient, null, messageSubject, messageContent, templateUrl);

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion Collateral Monitoring

        #region NPL Monitoring

        public void SendAlertsForNplMonitoring()
        {
            var data = (from a in context.tbl_Loan_Application
                        join d in context.tbl_Loan_Application_Detail on a.LoanApplicationId equals d.LoanApplicationId
                        join b in context.tbl_Loan on d.LoanApplicationDetailId equals b.LoanApplicationDetailId
                        join c in context.tbl_Loan_Revolving on d.LoanApplicationDetailId equals c.LoanApplicationDetailId
                        where b.NPLDate != null
                        select new LoanViewModel
                        {
                            applicationReferenceNumber = a.ApplicationReferenceNumber,
                            loanReferenceNumber = b.LoanReferenceNumber,
                            bookingDate = b.BookingDate,
                            disburseDate = b.DisburseDate,
                            nplDate = b.NPLDate.Value,
                            outstandingInterest = b.OutstandingInterest,
                            outstandingPrincipal = b.OutstandingPrincipal,
                            loanTypeName = context.tbl_Loan_Type.FirstOrDefault(x => x.LoanTypeId == b.LoanTypeId).LoanTypeName,
                            relationshipManagerId = b.RelationshipOfficerId,
                            relationshipManagerName = b.tbl_Staff.FirstName + " " + b.tbl_Staff.LastName,
                            relationshipManagerEmail = b.tbl_Staff.Email,
                            relationshipOfficerId = b.RelationshipOfficerId,
                            relationshipOfficerName = b.tbl_Staff1.FirstName + " " + b.tbl_Staff1.LastName,
                            relationshipOfficerEmail = b.tbl_Staff1.Email
                        }).ToList();

            try
            {
                foreach (var item in data)
                {
                    Console.WriteLine(item);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion NPL Monitoring

        public void SaveMessageDetails(MessageLogViewModel model)
        {
            var message = new tbl_Message_Log()
            {
                //MessageId = model.MessageId,
                MessageSubject = model.MessageSubject,
                MessageBody = model.MessageBody,
                MessageStatusId = model.MessageStatusId,
                MessageTypeId = model.MessageTypeId,
                FromAddress = model.FromAddress,
                ToAddress = model.ToAddress,
                DateTimeReceived = model.DateTimeReceived,
                SendOnDateTime = model.SendOnDateTime
            };

            context.tbl_Message_Log.Add(message);

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}