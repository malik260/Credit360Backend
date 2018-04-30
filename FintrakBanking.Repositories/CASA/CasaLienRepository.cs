using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Finance;
using FinTrakBanking.ThirdPartyIntegration.Finacle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.CASA
{
    public class CasaLienRepository : ICasaLienRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        //private ILoanOperationsRepository creditOperations;

        public CasaLienRepository(IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail,
                                            //ILoanOperationsRepository _creditOperations, 
                                            FinTrakBankingContext _context)
        {
            this.context = _context;
            this.generalSetup = _genSetup;
            auditTrail = _auditTrail;
            //this.creditOperations = _creditOperations;
        }


        public string PlaceLien(CasaLienViewModel model)
        {
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
            model.lienReferenceNumber = referenceNumber;

            //call     

            var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (setup.USE_THIRD_PARTY_INTEGRATION)
            {
                TransactionPosting tran = new TransactionPosting(context);
                bool dataModel = false;

                Task.Run(async () => { dataModel = await tran.APIProcessLien(model, "PLACE"); }).GetAwaiter().GetResult();

                if (dataModel == true)
                {
                    PlaceLienSub(model);
                }
                else
                {
                    //display message
                    throw new Exception($" Lien Placement Failed.");
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

        public bool ReleaseLien(CasaLienViewModel model)
        {
            var existingLien = context.TBL_CASA_LIEN.Where(x => x.LIENREFERENCENUMBER == model.lienReferenceNumber).FirstOrDefault();

            if (existingLien == null)
                throw new Exception($"Cannot release lien because lien with reference number {model.lienReferenceNumber} does not exist");


            var lienSum = context.TBL_CASA_LIEN.Where(x => x.LIENREFERENCENUMBER == model.lienReferenceNumber).Sum(y => y.LIENAMOUNT);

            if (lienSum <= 0)
                throw new Exception($"Cannot release lien because lien with reference number {model.lienReferenceNumber} has already been released");

            model.lienAmount = existingLien.LIENAMOUNT;
            model.sourceReferenceNumber = existingLien.SOURCEREFERENCENUMBER;
            model.productAccountNumber = existingLien.PRODUCTACCOUNTNUMBER;
            //model.description = data.DESCRIPTION;

            //call

            var setup = context.TBL_SETUP_GLOBAL.FirstOrDefault();
            if (setup.USE_THIRD_PARTY_INTEGRATION)
            {
                TransactionPosting tran = new TransactionPosting(context);
                bool dataModel = false;

                Task.Run(async () => { dataModel = await tran.APIProcessLien(model, "LIFTLIEN"); }).GetAwaiter().GetResult();

                if (dataModel == true)
                {
                    ReleaseLienSub(model, existingLien);
                }
                else
                {
                    //display message
                    throw new Exception($" Lien Lift Failed.");
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
