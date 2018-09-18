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

namespace FintrakBanking.Repositories.CASA
{
    public class CasaLienRepository : ICasaLienRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private ITwoFactorAuthIntegrationService twoFactorAuth;
        bool USE_TWO_FACTOR_AUTHENTICATION = false;
        bool USE_THIRD_PARTY_INTEGRATION = false;
        //private ILoanOperationsRepository creditOperations;
        private TransactionPosting tran;
        public CasaLienRepository(IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail,
                                            //ILoanOperationsRepository _creditOperations, 
                                            FinTrakBankingContext _context, TransactionPosting tran)
        {
            this.context = _context;
            this.tran = tran;
            this.generalSetup = _genSetup;
            auditTrail = _auditTrail;
            //this.creditOperations = _creditOperations;
            var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
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

                var authenticated = twoFactorAuth.Authenticate(twoFADetails.username, twoFADetails.passcode);

                if (authenticated.authenticated == false)
                    throw new TwoFactorAuthenticationException(authenticated.message);
            }

            if (USE_THIRD_PARTY_INTEGRATION)
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
                DATETIMECREATED = DateTime.Now

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
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------

            context.SaveChanges();

        }

        public bool ReleaseLien(CasaLienViewModel model, TwoFactorAutheticationViewModel twoFADetails = null)
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
            if (USE_TWO_FACTOR_AUTHENTICATION)
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

            context.SaveChanges();
        }
    }
}
