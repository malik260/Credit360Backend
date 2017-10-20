using System;
using System.Configuration;
using System.Data.Entity.SqlServer;
using System.Linq;
using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
using System.Data.Entity;

namespace FintrakBanking.Repositories.AppEmail
{
    public class EmailAndAlertsRepository : IEmailAndAlertsRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private EmailHelpers emailHelpers;
        private IGeneralSetupRepository genSetup;

        public EmailAndAlertsRepository(FinTrakBankingContext _context, IAuditTrailRepository _auditTrail,
            EmailHelpers _emailHelpers, IGeneralSetupRepository _general)
        {
            context = _context;
            auditTrail = _auditTrail;
            emailHelpers = _emailHelpers;
            genSetup = _general;
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
                        where DbFunctions.DiffDays(genSetup.GetApplicationDate(), a.NextCovenantDate) <= 10
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
                            loanRefNumber = e.tbl_Loan_Application.ApplicationReferenceNumber,
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
                        ToAddress = $"{item.officerEmail};{item.managerEmail}",
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
                        where a.NextCovenantDate.Value >= genSetup.GetApplicationDate()
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
                            loanRefNumber = e.tbl_Loan_Application.ApplicationReferenceNumber,
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
                        ToAddress = $"{item.officerEmail};{item.managerEmail}",
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
                        where (DbFunctions.DiffDays(DbFunctions.AddDays(f.LastValuationDate, a.ValuationCycle), genSetup.GetApplicationDate() ) <= 30)
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

        public void SendAlertsForLoanNplMonitoring()
        {
            var data = (from a in context.tbl_Loan_Application
                        join d in context.tbl_Loan_Application_Detail on a.LoanApplicationId equals d.LoanApplicationId
                        join b in context.tbl_Loan on d.LoanApplicationDetailId equals b.LoanApplicationDetailId
                        join c in context.tbl_Loan_Revolving on d.LoanApplicationDetailId equals c.LoanApplicationDetailId
                        where b.InternalPrudentialGuidelineStatusId != (int)LoanPrudentialStatusEnum.Performing
                        select new LoanViewModel
                        {
                            applicationReferenceNumber = a.ApplicationReferenceNumber,
                            loanReferenceNumber = b.LoanReferenceNumber,
                            bookingDate = b.BookingDate,
                            disburseDate = b.DisburseDate,
                            nplDate = b.NPLDate.Value,
                            outstandingInterest = b.OutstandingInterest,
                            outstandingPrincipal = b.OutstandingPrincipal,
                            loanTypeName = b.tbl_Loan_Type.LoanTypeName,
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
                    var recipient = item.relationshipManagerEmail;

                    var otherRecipient = item.relationshipOfficerEmail;

                    var messageSubject = "FIRSTBANKONLINE (REMINDER) - NON-PERFORMING LOANS";

                    var dataTable =
                        "<table><tr><th>Loan Ref #</th><th>Loan Type</th><th>Outstanding Interest</th><th>Oustanding Principal</th>" +
                        "<th>Booking Date</th><th>Disbursed Date</th></tr>" +
                        $"<tr><td>{item.loanReferenceNumber}</td><td>{item.loanTypeName}</td>" +
                        $"<td style='text-align:right;'>{item.outstandingInterest:f}</td><td style='text-align:right;'>{item.outstandingPrincipal:f}</td>" +
                        $"<td>{item.bookingDate:d}</td><td>{item.disburseDate:d}</td></tr>";

                    dataTable = dataTable + "</table>";

                    var messageContent = $"Dear {item.relationshipOfficerName}, <br /><br />" +
                                         "This is to bring your attention the following loans " +
                                         "which are underperforming. <br /><br />" +
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
                        ToAddress = $"{item.relationshipOfficerEmail};{item.relationshipManagerEmail}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };

                    SaveMessageDetails(messageModel);

                    emailHelpers.SendMail(recipient, otherRecipient, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion NPL Monitoring

        #region LPO/CFF/IDF/Self-Liquidating Loans

        public void SendAlertsOnSelfLiquidatingLoanExpiry()
        {
            var data = (from a in context.tbl_Loan
                        join b in context.tbl_Product on a.ProductId equals b.ProductId
                        join c in context.tbl_Product_Type on b.ProductTypeId equals c.ProductTypeId
                        join d in context.tbl_Loan_Application_Detail on a.LoanApplicationDetailId equals d.LoanApplicationDetailId
                        where a.tbl_Product.ProductTypeId == (int)LoanProductTypeEnum.SelfLiquidating && 
                        DbFunctions.DiffDays(a.MaturityDate, genSetup.GetApplicationDate()) <= 30
                        select new LoanViewModel
                        {
                            applicationReferenceNumber = d.tbl_Loan_Application.ApplicationReferenceNumber,
                            loanReferenceNumber = a.LoanReferenceNumber,
                            bookingDate = a.BookingDate,
                            disburseDate = a.DisburseDate,
                            maturityDate = a.MaturityDate,
                            productName = b.ProductName,
                            outstandingInterest = a.OutstandingInterest,
                            outstandingPrincipal = a.OutstandingPrincipal,
                            loanTypeName = a.tbl_Loan_Type.LoanTypeName,
                            productTypeName = b.tbl_Product_Type.ProductTypeName,
                            relationshipManagerId = a.RelationshipOfficerId,
                            relationshipManagerName = a.tbl_Staff.FirstName + " " + a.tbl_Staff.LastName,
                            relationshipManagerEmail = a.tbl_Staff.Email,
                            relationshipOfficerId = a.RelationshipOfficerId,
                            relationshipOfficerName = a.tbl_Staff1.FirstName + " " + a.tbl_Staff1.LastName,
                            relationshipOfficerEmail = a.tbl_Staff1.Email
                        }).ToList();

            try
            {
                foreach (var item in data)
                {
                    var recipient = item.relationshipManagerEmail;

                    var otherRecipient = item.relationshipOfficerEmail;

                    var messageSubject = "FIRSTBANKONLINE (REMINDER) - SELF-LIQUIDATING LOANs EXPIRED";

                    var dataTable =
                        "<table><tr><th>Loan Ref #</th><th>Product</th><th>Loan Type</th><th>Product Type</th><th>Outstanding Interest</th>" +
                        "<th>Oustanding Principal</th></tr><th>Disbursed Date</th><th>Maturity Date</th>" +
                        $"<tr><td>{item.loanReferenceNumber}</td><td>{item.productName}</td><td>{item.loanTypeName}</td><td>{item.productTypeName}</td>" +
                        $"<td style='text-align:right;'>{item.outstandingInterest:f}</td><td style='text-align:right;'>{item.outstandingPrincipal:f}</td>" +
                        $"<td>{item.maturityDate:d}</td><td>{item.disburseDate:d}</td></tr>";

                    dataTable = dataTable + "</table>";

                    var messageContent = $"Dear {item.relationshipOfficerName}, <br /><br />" +
                                         "This is to bring your attention the following self-liquidating loans " +
                                         "which are expired. <br /><br />" +
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
                        ToAddress = $"{item.relationshipOfficerEmail};{item.relationshipManagerEmail}",
                        DateTimeReceived = DateTime.Now,
                        SendOnDateTime = DateTime.Now
                    };

                    SaveMessageDetails(messageModel);

                    emailHelpers.SendMail(recipient, otherRecipient, messageSubject, messageContent, templateUrl);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion LPO/CFF/IDF/Self-Liquidating Loans

        public bool SaveMessageDetails(MessageLogViewModel model)
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
                var response = context.SaveChanges() > 0;

                if (response)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}