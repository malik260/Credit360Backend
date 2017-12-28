using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Customer
{
    public class KYCDocumentUploadRepository : IKYCDocumentUploadRepository
    {
        private FinTrakBankingDocumentsContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public KYCDocumentUploadRepository(FinTrakBankingDocumentsContext _context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = _context;
            this.general = general;
            this.audit = audit;
        }
        public bool KYCDocumentUpload(CustomerDocumentUploadViewModel model, byte[] file)
        {
            try
            {
                var data = new Entities.DocumentModels.TBL_MEDIA_KYC_DOCUMENTS
                {
                    FILEDATA = file,
                    CUSTOMERID = model.customerId,
                    CUSTOMERCODE = model.customerCode,
                    DOCUMENTTITLE = model.documentTitle,
                    DOCUMENTTYPEID = model.documentTypeId,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    PHYSICALFILENUMBER = model.physicalFileNumber,
                    PHYSICALLOCATION = model.physicalLocation,
                    CREATEDBY = (int)model.createdBy,
                    DATECREATED = DateTime.Now
                };

                context.TBL_MEDIA_KYC_DOCUMENTS.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added KYC Document '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public IEnumerable<CustomerDocumentUploadViewModel> GetKYCDocumentUploadByCustomerId(int customerId)
        {
            return this.context.TBL_MEDIA_KYC_DOCUMENTS.Where(x => x.CUSTOMERID == customerId).Select(x => new CustomerDocumentUploadViewModel
            {
                documentId = x.DOCUMENTID,
                customerId = x.CUSTOMERID,
                customerCode = x.CUSTOMERCODE,
                documentTitle = x.DOCUMENTTITLE,
                documentTypeId = (short)x.DOCUMENTTYPEID,
                fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                systemDateTime = x.SYSTEMDATETIME,
                physicalFileNumber = x.PHYSICALFILENUMBER,
                physicalLocation = x.PHYSICALLOCATION,
            });
        }

        public bool CheckListDocumentUpload(CheckListDocumentUploadViewModel model, byte[] file)
        {
            try
            {
                var data = new Entities.DocumentModels.TBL_MEDIA_CHECKLIST_DOCUMENTS
                {
                    FILEDATA = file,
                    CHECKLISTDEFINITIONID = model.checkListDefinitionId,
                    CHECKLISTSTATUSID = model.checkListStatusId,
                    LOANAPPLICATIONID = model.loanApplicationId,
                    LOANDETAILSID = model.loanDetailsId,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    PHYSICALFILENUMBER = model.physicalFileNumber,
                    PHYSICALLOCATION = model.physicalLocation,
                    CREATEDBY = (int)model.createdBy,
                    DATECREATED = DateTime.Now
                };

                context.TBL_MEDIA_CHECKLIST_DOCUMENTS.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Checklist Document for item with ID: '{ model.checkListDefinitionId }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public CheckListDocumentUploadViewModel CheckListDocumentUploadViewModel(int definitionId, int statusId, int detailId, bool isProductBased)
        {
           
            if (isProductBased)
            {
                var checklistDoc = (from ck in context.TBL_MEDIA_CHECKLIST_DOCUMENTS
                                    where ck.CHECKLISTDEFINITIONID == definitionId
                                     && ck.CHECKLISTSTATUSID == statusId
                                     && ck.LOANDETAILSID == detailId
                                    select new CheckListDocumentUploadViewModel()
                                    {
                                        fileData = ck.FILEDATA,
                                        fileName = ck.FILENAME,
                                        fileExtension = ck.FILEEXTENSION
                                    }).FirstOrDefault();
                return checklistDoc;
            }
            else
            {
                var checklistDoc = (from ck in context.TBL_MEDIA_CHECKLIST_DOCUMENTS
                                    where ck.CHECKLISTDEFINITIONID == definitionId
                                     && ck.CHECKLISTSTATUSID == statusId
                                     && ck.LOANAPPLICATIONID == detailId
                                    select new CheckListDocumentUploadViewModel()
                                    {
                                        fileData = ck.FILEDATA,
                                        fileName = ck.FILENAME,
                                        fileExtension = ck.FILEEXTENSION
                                    }).FirstOrDefault();
                return checklistDoc;
            }
           
        }
    }
}
