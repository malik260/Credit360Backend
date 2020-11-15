using ExcelDataReader;
using FintrakBanking.Common;
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
        private FinTrakBankingContext _finContext;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public KYCDocumentUploadRepository(FinTrakBankingDocumentsContext _context, IGeneralSetupRepository general, IAuditTrailRepository audit, FinTrakBankingContext finContext)
        {
            this.context = _context;
            this.general = general;
            this.audit = audit;
            this._finContext = finContext;
        }

        #region KYC Document Upload
        public bool KYCDocumentUpload(CustomerDocumentUploadViewModel model, byte[] file)
        {
            var auditDetail = string.Empty;
            short auditType = 0;
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
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
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
            var kycDocuments = from x in context.TBL_MEDIA_KYC_DOCUMENTS where x.CUSTOMERID == customerId select new CustomerDocumentUploadViewModel
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
            };

            return kycDocuments;

            //return this.context.TBL_MEDIA_KYC_DOCUMENTS.Where(x => x.CUSTOMERID == customerId).Select(x => new CustomerDocumentUploadViewModel
            //{
            //    documentId = x.DOCUMENTID,
            //    customerId = x.CUSTOMERID,
            //    customerCode = x.CUSTOMERCODE,
            //    documentTitle = x.DOCUMENTTITLE,
            //    documentTypeId = (short)x.DOCUMENTTYPEID,
            //    fileData = x.FILEDATA,
            //    fileName = x.FILENAME,
            //    fileExtension = x.FILEEXTENSION,
            //    systemDateTime = x.SYSTEMDATETIME,
            //    physicalFileNumber = x.PHYSICALFILENUMBER,
            //    physicalLocation = x.PHYSICALLOCATION,
            //});
        }
        #endregion

        #region CheckList Document Upload
        public int CheckListDocumentUpload(CheckListDocumentUploadViewModel model, byte[] file)
        {
            var existing = context.TBL_MEDIA_CHECKLIST_DOCUMENTS
                         .Where(x => x.FILENAME == model.fileName
                             && x.FILEEXTENSION == model.fileExtension
                             && x.LOANAPPLICATIONID == model.loanApplicationId
                             && x.CHECKLISTDEFINITIONID == model.checkListDefinitionId
                             );

            if (existing.Count() > 0 && model.overwrite == false) return 3;

            if (existing.Count() > 0 && model.overwrite == true)
            {
                context.TBL_MEDIA_CHECKLIST_DOCUMENTS.RemoveRange(existing);
            }

            //if (isDocumentUploaded == true)
            //{
            //    var existingDocument = (context.TBL_MEDIA_CHECKLIST_DOCUMENTS.Where(x => x.LOANAPPLICATIONID == model.loanApplicationId).Select(x => x)).FirstOrDefault();

            //    existingDocument.FILEDATA = file;
            //    existingDocument.CHECKLISTDEFINITIONID = model.checkListDefinitionId;
            //    existingDocument.CHECKLISTSTATUSID = model.checkListStatusId;
            //    existingDocument.LOANAPPLICATIONID = model.loanApplicationId;
            //    existingDocument.LOANDETAILSID = model.loanDetailsId;
            //    existingDocument.FILENAME = model.fileName;
            //    existingDocument.FILEEXTENSION = model.fileExtension;
            //    existingDocument.SYSTEMDATETIME = DateTime.Now;
            //    existingDocument.PHYSICALFILENUMBER = model.physicalFileNumber;
            //    existingDocument.PHYSICALLOCATION = model.physicalLocation;
            //    existingDocument.CREATEDBY = (int)model.createdBy;
            //    existingDocument.DATECREATED = DateTime.Now;

            //    // Audit Section ---------------------------
            //    var audit = new TBL_AUDIT
            //    {
            //        AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
            //        STAFFID = model.createdBy,
            //        BRANCHID = (short)model.userBranchId,
            //        DETAIL = $"uploaded Checklist Document for item with ID: '{ model.checkListDefinitionId }' has been replaced ",
            //        IPADDRESS = model.userIPAddress,
            //        URL = model.applicationUrl,
            //        APPLICATIONDATE = general.GetApplicationDate(),
            //        SYSTEMDATETIME = DateTime.Now
            //    };
            //    this.audit.AddAuditTrail(audit);
            //    // End of Audit Section ---------------------
            //}
            //else
            //{

                context.TBL_MEDIA_CHECKLIST_DOCUMENTS.Add(new TBL_MEDIA_CHECKLIST_DOCUMENTS
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
                });

                // Audit Section ---------------------------
                this.audit.AddAuditTrail(new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Checklist Document for item with ID: '{ model.checkListDefinitionId }' ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                });
                // End of Audit Section ---------------------
            //}

            return context.SaveChanges() == 0 ? 1 : 2;

        }
        public CheckListDocumentUploadViewModel CheckListDocumentUploadViewModel(int definitionId, int statusId, int detailId, bool isProductBased, int? customerId=null,int? checkListItemId = null, int? checkListTypeId=null, DateTime ? checklistDate = null)
        {
            if (checkListTypeId == (int)CheckListTypeEnum.RegulatoryChecklist)
            {
                var creditBureauId = _finContext.TBL_CREDIT_BUREAU.Where(o => o.CHECKLISTITEMID == checkListItemId).Select(o=>o.CREDITBUREAUID).FirstOrDefault();
                if (creditBureauId!=null)
                {
                    var customerCreditBureauId = (from q in _finContext.TBL_CUSTOMER_CREDIT_BUREAU 
                                                  where q.CREDITBUREAUID == creditBureauId && q.CUSTOMERID == customerId && q.DATETIMECREATED <= checklistDate.Value
                                                  orderby q.CUSTOMERCREDITBUREAUID descending
                                                  select q.CUSTOMERCREDITBUREAUID).FirstOrDefault();

                     var checklistDoc = (from ck in context.TBL_CUSTOMER_CREDIT_BUREAU
                                        where ck.CUSTOMERCREDITBUREAUID == customerCreditBureauId                                        
                                        select new CheckListDocumentUploadViewModel()
                                        {
                                            fileData = ck.FILEDATA,
                                            fileName = ck.FILENAME,
                                            fileExtension = ck.FILEEXTENSION
                                        }).FirstOrDefault();
                    return checklistDoc;
                }
            }

            if (isProductBased)
            {
                var checklistDoc = (from ck in context.TBL_MEDIA_CHECKLIST_DOCUMENTS
                                    where ck.CHECKLISTDEFINITIONID == definitionId
                                    // && ck.CHECKLISTSTATUSID == statusId
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
                var checklistDoc = new CheckListDocumentUploadViewModel();

                if (checkListTypeId == (int)CheckListTypeEnum.PreLendingCallGrid)
                {
                    checklistDoc = (from ck in context.TBL_MEDIA_CHECKLIST_DOCUMENTS
                                    where ck.CHECKLISTDEFINITIONID == definitionId
                                     // && ck.CHECKLISTSTATUSID == statusId
                                     && ck.LOANAPPLICATIONID == detailId
                                    select new CheckListDocumentUploadViewModel()
                                    {
                                        fileData = ck.FILEDATA,
                                        fileName = ck.FILENAME,
                                        fileExtension = ck.FILEEXTENSION
                                    }).FirstOrDefault();
                }
                else
                {
                    checklistDoc = (from ck in context.TBL_MEDIA_CHECKLIST_DOCUMENTS
                                    where ck.CHECKLISTDEFINITIONID == definitionId
                                    // && ck.CHECKLISTSTATUSID == statusId
                                     && ck.LOANDETAILSID == detailId
                                    select new CheckListDocumentUploadViewModel()
                                    {
                                        fileData = ck.FILEDATA,
                                        fileName = ck.FILENAME,
                                        fileExtension = ck.FILEEXTENSION
                                    }).FirstOrDefault();
                }
                return checklistDoc;
            }
        }
        public bool RemoveCheckListDocument(int definitionId, int statusId, int detailId, bool isProductBased)
        {
            if (isProductBased)
            {
                var checklistDoc = (from ck in context.TBL_MEDIA_CHECKLIST_DOCUMENTS
                                    where ck.CHECKLISTDEFINITIONID == definitionId
                                     && ck.CHECKLISTSTATUSID == statusId
                                     && ck.LOANDETAILSID == detailId
                                    select ck).FirstOrDefault();
                if (checklistDoc != null)
                {
                    this.context.TBL_MEDIA_CHECKLIST_DOCUMENTS.Remove(checklistDoc);
                    return context.SaveChanges() != 0;
                }
            }
            else
            {
                var checklistDoc = (from ck in context.TBL_MEDIA_CHECKLIST_DOCUMENTS
                                    where ck.CHECKLISTDEFINITIONID == definitionId
                                     && ck.CHECKLISTSTATUSID == statusId
                                     && ck.LOANAPPLICATIONID == detailId
                                    select ck).FirstOrDefault();
                if (checklistDoc != null)
                {
                    this.context.TBL_MEDIA_CHECKLIST_DOCUMENTS.Remove(checklistDoc);
                    return context.SaveChanges() != 0;
                }
            }
            return false;
        }
        public bool RemoveConditionPrecedentDocument(int conditionId, int loanApplicationId)
        {
                var checklistDoc = (from ck in context.TBL_LOAN_CONDITION_DOCUMENTS
                                    where ck.CONDITIONID == conditionId
                                     && ck.LOANAPPLICATIONID == loanApplicationId
                                    select ck).FirstOrDefault();
                if (checklistDoc != null)
                {
                    this.context.TBL_LOAN_CONDITION_DOCUMENTS.Remove(checklistDoc);
                    return context.SaveChanges() != 0;
                }         
            return false;
        }
        
        #endregion

        #region Conditions Precedent Document Upload
        public ConditionsPrecedentUploadViewModel GetLoanConditionDocumentByConditionId(int conditionId,int loanApplicationId)
        {
            var checklistDoc = (from ck in context.TBL_LOAN_CONDITION_DOCUMENTS
                                where ck.CONDITIONID == conditionId
                                && ck.LOANAPPLICATIONID == loanApplicationId && ck.DELETED == false
                                select new ConditionsPrecedentUploadViewModel()
                                {
                                    fileData = ck.FILEDATA,
                                    fileName = ck.FILENAME,
                                    fileExtension = ck.FILEEXTENSION
                                }).FirstOrDefault();
            return checklistDoc;
        }
        public ConditionsPrecedentUploadViewModel GetLoanConditionDocumentBydocumentId(int documentId)
        {
            var checklistDoc = (from ck in context.TBL_LOAN_CONDITION_DOCUMENTS
                                where ck.DOCUMENTID == documentId && ck.DELETED == false
                                select new ConditionsPrecedentUploadViewModel()
                                {
                                    fileData = ck.FILEDATA,
                                    fileName = ck.FILENAME,
                                    fileExtension = ck.FILEEXTENSION
                                }).FirstOrDefault();
            return checklistDoc;
        }

        public bool DeleteConditionDocumentBydocumentId(int documentId, int staffId)
        {
            var checklistDoc = context.TBL_LOAN_CONDITION_DOCUMENTS.Find(documentId);
            if(checklistDoc != null)
            {
                checklistDoc.DELETED = true;
                checklistDoc.DATETIMEDELETED = DateTime.Now;
                checklistDoc.DELETEDBY = staffId;
            }
             return context.SaveChanges() != 0;
                                
        }
        public IEnumerable<ConditionsPrecedentUploadViewModel> GetLoanConditionDocumentByContionId(int conditionId)
        {
            return this.context.TBL_LOAN_CONDITION_DOCUMENTS.Where(x => x.CONDITIONID == conditionId && x.DELETED == false).Select(x => new ConditionsPrecedentUploadViewModel
            {
                documentId = x.DOCUMENTID,
                conditionId = x.CONDITIONID,
                loanApplicationId = x.LOANAPPLICATIONID,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                systemDateTime = x.SYSTEMDATETIME,
                physicalFileNumber = x.PHYSICALFILENUMBER,
                physicalLocation = x.PHYSICALLOCATION,
                createdBy = x.CREATEDBY,
            });
        }

        public IEnumerable<ConditionsPrecedentUploadViewModel> GetDeletedLoanConditionDocumentByContionId(int conditionId)
        {
            
                var data = (from x in context.TBL_LOAN_CONDITION_DOCUMENTS
                            where
                            x.CONDITIONID == conditionId
                            && x.DELETED == true
                            select new ConditionsPrecedentUploadViewModel
                            {
                                documentId = x.DOCUMENTID,
                                conditionId = x.CONDITIONID,
                                loanApplicationId = x.LOANAPPLICATIONID,
                                fileName = x.FILENAME,
                                fileExtension = x.FILEEXTENSION,
                                systemDateTime = x.SYSTEMDATETIME,
                                physicalFileNumber = x.PHYSICALFILENUMBER,
                                physicalLocation = x.PHYSICALLOCATION,
                                createdBy = x.CREATEDBY,
                                dateTimeDeleted = x.DATETIMEDELETED,
                                deletedBy = x.DELETEDBY
                            }).ToList();

                foreach(var p in data)
                {
                p.deletedByName = _finContext.TBL_STAFF.Where(s => s.STAFFID == p.deletedBy).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault();
                                
                }

                return data;
            
        }
        public bool ConditionsPrecedentDocumentUpload(ConditionsPrecedentUploadViewModel model, byte[] file)
        {
            try
            {
                var data = new TBL_LOAN_CONDITION_DOCUMENTS()
                {
                    FILEDATA = file,
                    CONDITIONID = model.conditionId,
                    LOANAPPLICATIONID = model.loanApplicationId,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    PHYSICALFILENUMBER = model.physicalFileNumber,
                    PHYSICALLOCATION = model.physicalLocation,
                    CREATEDBY = (int)model.createdBy,
                    DATECREATED = DateTime.Now
                };

                context.TBL_LOAN_CONDITION_DOCUMENTS.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Condition Precedent Document for condition with ID: '{ model.conditionId }' ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
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
        #endregion
    }
}
