using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.CASA
{
    public class CasaLienRepository: ICasaLienRepository
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

            var data = new TBL_CASA_LIEN
            {
                PRODUCTACCOUNTNUMBER = model.productAccountNumber,
                LIENREFERENCENUMBER = referenceNumber,
                SOURCEREFERENCENUMBER = model.sourceReferenceNumber,
                BRANCHID = model.userBranchId,
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
                DETAIL = $"Applied lien with reference number: {referenceNumber}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();

            return referenceNumber;
        }

        public bool ReleaseLien(CasaLienViewModel model)
        {
            var existingLien = context.TBL_CASA_LIEN.Where(x => x.LIENREFERENCENUMBER == model.lienReferenceNumber).FirstOrDefault();

            if (existingLien == null)
                throw new Exception($"Cannot release lien because lien with reference number {model.lienReferenceNumber} does not exist");

            var data = new TBL_CASA_LIEN
            {
                PRODUCTACCOUNTNUMBER = existingLien.PRODUCTACCOUNTNUMBER,
                LIENREFERENCENUMBER = existingLien.LIENREFERENCENUMBER,
                SOURCEREFERENCENUMBER = existingLien.SOURCEREFERENCENUMBER,
                BRANCHID = model.userBranchId,
                COMPANYID = model.companyId,
                LIENAMOUNT = Math.Abs(existingLien.LIENAMOUNT) * -1,
                DESCRIPTION = "Lien Release -- " + model.description,
                LIENTYPEID = existingLien.LIENTYPEID,                
                CREATEDBY = model.createdBy,
                DATETIMECREATED = DateTime.Now

            };

            context.TBL_CASA_LIEN.Add(data);

            // Audit Section ---------------------------            

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LienReleased,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Released lien with reference number: {existingLien.LIENREFERENCENUMBER}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = generalSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            //end of Audit section -------------------------------
            context.SaveChanges();

            return true;
        }
    }
}
