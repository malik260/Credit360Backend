using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Finance; 
using FinTrakBanking.ThirdPartyIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinTrakBanking.ThirdPartyIntegration.Finacle;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FintrakBanking.Common.CustomException;
using static FinTrakBanking.ThirdPartyIntegration.TwoFactorAuthIntegration.TwoFactorAuthIntegrationService;
using FintrakBanking.ViewModels.Setups.General;
using System.Configuration;

namespace FintrakBanking.Repositories.CASA
{
    public class CasaLienRepository : ICasaLienRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ITwoFactorAuthIntegrationService twoFactorAuth;
        private TransactionPosting tran;
        bool USE_TWO_FACTOR_AUTHENTICATION = false;
        bool USE_THIRD_PARTY_INTEGRATION = false;
        TBL_INTEGRATION_CONTROL globalIntegrationSetting = new TBL_INTEGRATION_CONTROL();
        //private ILoanOperationsRepository creditOperations;
        List<string> receiverEmailList = new List<string>();
        AlertsViewModel alert = new AlertsViewModel();

        public CasaLienRepository(IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail,
                                            //ILoanOperationsRepository _creditOperations, 
                                            FinTrakBankingContext _context, TransactionPosting tran, ITwoFactorAuthIntegrationService _twoFactorAuth)
        {
            this.context = _context;
            this.tran = tran;
            this.generalSetup = _genSetup;
            auditTrail = _auditTrail;
            this.twoFactorAuth = _twoFactorAuth;
            //this.creditOperations = _creditOperations;
            var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            globalIntegrationSetting = context.TBL_INTEGRATION_CONTROL.FirstOrDefault();
            USE_THIRD_PARTY_INTEGRATION = setup.USE_THIRD_PARTY_INTEGRATION;
            USE_TWO_FACTOR_AUTHENTICATION = setup.USE_TWO_FACTOR_AUTHENTICATION;
        }


        public string PlaceLien(CasaLienViewModel model, TwoFactorAutheticationViewModel twoFADetails = null)
        {
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
            model.lienReferenceNumber = referenceNumber;

            //call     
            if (USE_TWO_FACTOR_AUTHENTICATION)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                if (twoFADetails.skipAuthentication == false)
                {
                    var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                    if (authenticated.authenticated == false)
                        throw new TwoFactorAuthenticationException(authenticated.message);
                }
            }

            if (USE_THIRD_PARTY_INTEGRATION && globalIntegrationSetting.USE_THIRDPARTY_LIEN )
            {

                ResponseMessage result = null;

                Task.Run(async () => { result = await tran.APIProcessLien(model, "PLACE"); }).GetAwaiter().GetResult();

                if (result.APIResponse != null)
                {
                    if (result.APIResponse.responseCode == "0")
                    {
                        PlaceLienSub(model);
                    }
                    else
                    {
                        throw new ConditionNotMetException("Core Banking API Error - " + result.APIResponse.webRequestStatus);
                    }
                }
                else
                {
                    throw new APIErrorException("Core Banking API Error - " + result.Message.ReasonPhrase);
                }

            }

            else
            {
                PlaceLienSub(model);
            }

            return referenceNumber;
        }

        private void PlaceLienSub(CasaLienViewModel model)
        {
            var validate = context.TBL_CASA_LIEN.Where(x => x.SOURCEREFERENCENUMBER == model.sourceReferenceNumber).FirstOrDefault();
            if (validate == null)
            {
                var data = new TBL_CASA_LIEN
                {
                    PRODUCTACCOUNTNUMBER = model.productAccountNumber,
                    LIENREFERENCENUMBER = model.lienReferenceNumber,
                    SOURCEREFERENCENUMBER = model.sourceReferenceNumber,
                    BRANCHID = model.branchId,
                    COMPANYID = model.companyId,
                    LIENAMOUNT = model.lienAmount,
                    DESCRIPTION = model.description,
                    LIENTYPEID = model.lienTypeId,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    LIENSTATUS = (int)LienStatusEnum.Active,
                    ISLIENREMOVED = false
                };

                context.TBL_CASA_LIEN.Add(data);

                // Audit Section ---------------------------            

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LienPlaced,
                    STAFFID = model.createdBy,
                    BRANCHID = model.branchId,
                    DETAIL = $"Applied lien with reference number: {model.lienReferenceNumber}",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = generalSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()


                };
                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -------------------------------

                if (context.SaveChanges() > 0)
                {
                    if (model.createdBy != 1)
                    {
                        var customer = "";
                        var termLoan = context.TBL_LOAN.Where(t => t.LOANREFERENCENUMBER == model.sourceReferenceNumber).FirstOrDefault();
                        if(termLoan != null)
                        {
                            customer = context.TBL_CUSTOMER.Where(c=>c.CUSTOMERID == termLoan.CUSTOMERID).Select(c=>c.FIRSTNAME + " " +c.MIDDLENAME +" " + c.LASTNAME).FirstOrDefault();
                        }
                        var revolvingLoan = context.TBL_LOAN_REVOLVING.Where(t => t.LOANREFERENCENUMBER == model.sourceReferenceNumber).FirstOrDefault();
                        if (revolvingLoan != null)
                        {
                            customer = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == revolvingLoan.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME).FirstOrDefault();
                        }
                        var contingentLoan = context.TBL_LOAN_CONTINGENT.Where(t => t.LOANREFERENCENUMBER == model.sourceReferenceNumber).FirstOrDefault();
                        if (contingentLoan != null)
                        {
                            customer = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == contingentLoan.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME).FirstOrDefault();
                        }
                        var staff = context.TBL_STAFF.Find(model.createdBy);
                        var alertDetail = context.TBL_ALERT_TITLE.Where(x => x.BINDINGMETHOD == "LienAccountNotification").FirstOrDefault();
                        var emailList = GetBusinessUsersEmailsToGroupHead(staff.MISCODE);
                        alert.receiverEmailList.Add(emailList);
                        var alertTemplate = alertDetail.TEMPLATE;
                        var accountOfficer = staff.FIRSTNAME + " " + staff.LASTNAME + " " + staff.MIDDLENAME;
                        alertTemplate = alertTemplate.Replace("@{{accountOfficer}}", accountOfficer);
                        alertTemplate = alertTemplate.Replace("@{{customer}}", customer);
                        alertTemplate = alertTemplate.Replace("@{{accountNumber}}", model.productAccountNumber);
                        alertTemplate = alertTemplate.Replace("@{{sourceReferenceNumber}}", model.sourceReferenceNumber);
                        LogEmailAlert(alertTemplate, alertDetail.TITLE, alert.receiverEmailList, "11023", 11023, "LienAccountNotification");
                    }
                }
            }
        }

        public bool ReleaseLien(CasaLienViewModel model, TwoFactorAutheticationViewModel twoFADetails = null, bool require2FA = true)
        {
            var existingLien = context.TBL_CASA_LIEN.Where(x => x.LIENREFERENCENUMBER == model.lienReferenceNumber).FirstOrDefault();

            if (existingLien == null)
                throw new SecureException($"Cannot release lien because lien with reference number {model.lienReferenceNumber} does not exist");


            var lienSum = context.TBL_CASA_LIEN.Where(x => x.LIENREFERENCENUMBER == model.lienReferenceNumber).Sum(y => y.LIENAMOUNT);

            if (lienSum <= 0)
                throw new SecureException($"Cannot release lien because lien with reference number {model.lienReferenceNumber} has already been released");

            model.lienAmount = existingLien.LIENAMOUNT;
            model.sourceReferenceNumber = existingLien.SOURCEREFERENCENUMBER;
            model.productAccountNumber = existingLien.PRODUCTACCOUNTNUMBER;
            //model.description = data.DESCRIPTION;

            //call
            if (USE_TWO_FACTOR_AUTHENTICATION  && require2FA == true)
            {
                if (twoFADetails == null)
                    throw new TwoFactorAuthenticationException("Authentication token not specified. Specify the second factor authentication token");

                var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                if (authenticated.authenticated == false)
                    throw new TwoFactorAuthenticationException(authenticated.message);
            }

            if (USE_THIRD_PARTY_INTEGRATION)
            {

                ResponseMessage result = null;

                Task.Run(async () => { result = await tran.APIProcessLien(model, "LIFTLIEN"); }).GetAwaiter().GetResult();

                if (result.APIResponse != null)
                    if (result.APIResponse.responseCode == "0")
                    {
                        ReleaseLienSub(model, existingLien);
                    }
                    else
                    {
                        throw new ConditionNotMetException(result.APIResponse.webRequestStatus);
                    }

            }
            else
            {
                ReleaseLienSub(model, existingLien);
            }

            return true;
        }

        private void ReleaseLienSub(CasaLienViewModel model, TBL_CASA_LIEN existingLien)
        {
            var data = new TBL_CASA_LIEN
            {
                PRODUCTACCOUNTNUMBER = existingLien.PRODUCTACCOUNTNUMBER,
                LIENREFERENCENUMBER = existingLien.LIENREFERENCENUMBER,
                SOURCEREFERENCENUMBER = existingLien.SOURCEREFERENCENUMBER,
                BRANCHID = model.branchId,
                COMPANYID = model.companyId,
                LIENAMOUNT = Math.Abs(existingLien.LIENAMOUNT) * -1,
                DESCRIPTION = "Lien Release -- " + model.description,
                LIENTYPEID = existingLien.LIENTYPEID,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now

            };

            existingLien.ISLIENREMOVED = true;
            context.TBL_CASA_LIEN.Add(data);

            // Audit Section ---------------------------            

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.LienReleased,
            //    STAFFID = model.createdBy,
            //    BRANCHID = model.branchId,
            //    DETAIL = $"Released lien with reference number: {existingLien.LIENREFERENCENUMBER}",
            //    IPADDRESS = model.userIPAddress,
            //    URL = model.applicationUrl,
            //    APPLICATIONDATE = generalSetup.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now
            //};
            //this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            context.SaveChanges() ;
            
        }

        private string GetBusinessUsersEmailsToGroupHead(string accountOfficerMIsCode)
        {
            string emailList = "";

            var accountOfficer = context.TBL_STAFF.Where(x => x.MISCODE.ToLower() == accountOfficerMIsCode.ToLower()).FirstOrDefault();
            if (accountOfficer != null)
            {
                emailList = accountOfficer.EMAIL;
                if (accountOfficer.SUPERVISOR_STAFFID != null)
                {
                    var relationshipManager = context.TBL_STAFF.Where(x => x.STAFFID == accountOfficer.SUPERVISOR_STAFFID).FirstOrDefault();
                    if (relationshipManager != null)
                    {
                        emailList = emailList + ";" + relationshipManager.EMAIL;
                        if (relationshipManager.SUPERVISOR_STAFFID != null)
                        {
                            var zonalHead = context.TBL_STAFF.Where(x => x.STAFFID == relationshipManager.SUPERVISOR_STAFFID).FirstOrDefault();
                            if (zonalHead != null)
                            {
                                emailList = emailList + ";" + zonalHead.EMAIL;

                                var groupHead = context.TBL_STAFF.Where(x => x.STAFFID == zonalHead.SUPERVISOR_STAFFID).FirstOrDefault();

                                if (groupHead != null)
                                {
                                    emailList = emailList + ";" + groupHead.EMAIL;
                                }
                            }
                        }
                    }
                }

            }

            return emailList;
        }
        public void LogEmailAlert(string messageBody, string alertSubject, List<string> recipients, string referenceCode, int targetId, string operationMehtod)
        {
            try
            {
                string recipient = string.Join("", recipients.ToArray());
                string messageSubject = alertSubject + " ALERT";
                string messageContent = messageBody;
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = messageContent,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now,
                    ReferenceCode = referenceCode,
                    targetId = targetId,
                    operationMethod = operationMehtod,
                };
                SaveMessageDetails(messageModel);
            }
            catch (Exception ex)
            {
                new SecureException(ex.ToString());
            }
        }

        private void SaveMessageDetails(MessageLogViewModel model)
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
                SENDONDATETIME = model.SendOnDateTime,
                ATTACHMENTCODE = model.ReferenceCode,
                ATTACHMENTTYPEID = (short)AttachementTypeEnum.JobRequest,
                TARGETID = (int)model.targetId,
                OPERATIONMETHOD = model.operationMethod
            };

            context.TBL_MESSAGE_LOG.Add(message);
            context.SaveChanges();

        }
    }
}
