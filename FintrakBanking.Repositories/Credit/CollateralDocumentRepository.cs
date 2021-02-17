using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Credit
{
    public class CollateralDocumentRepository : ICollateralDocumentRepository
    {
        private FinTrakBankingDocumentsContext context;
        private FinTrakBankingContext bankingContext;

        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private ICustomerCollateralRepository coll;
        // private ICollateralDocumentRepository document;

        public CollateralDocumentRepository(CustomerCollateralRepository coll, FinTrakBankingDocumentsContext context, FinTrakBankingContext bankingContext, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.bankingContext = bankingContext;
            this.general = general;
            this.audit = audit;
            this.coll = coll;
        }

        public bool AddCollateralDocument(CollateralDocumentViewModel model, byte[] file)
        {
            var data = new Entities.DocumentModels.TBL_MEDIA_COLLATERAL_DOCUMENTS
            {
                FILEDATA = file,
                DOCUMENTCODE = model.documentTitle,
                FILENAME = model.fileName,
                FILEEXTENSION = model.fileExtension,
                COLLATERALCUSTOMERID = model.collateralId,
                SYSTEMDATETIME = DateTime.Now,
                CREATEDBY = (int)model.createdBy,
                ISPRIMARYDOCUMENT = model.isPrimaryDocument,
                DOCUMENTTYPEID = model.documentTypeId,
                
            };

            context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralDocumentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Collateral Document '{ model.documentTitle }' ",
                IPADDRESS =CommonHelpers.GetLocalIpAddress(),
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
        public bool AddTempCollateralDocument(CollateralDocumentViewModel model, byte[] file)
        {
            var data = new Entities.DocumentModels.TBL_TEMP_MEDIA_COLLATERAL_DOCS
            {
                FILEDATA = file,
                DOCUMENTCODE = model.documentTitle,
                FILENAME = model.fileName,
                FILEEXTENSION = model.fileExtension,
                TEMPCOLLATERALCUSTOMERID = model.collateralId,
                SYSTEMDATETIME = DateTime.Now,
                CREATEDBY = (int)model.createdBy,
                ISPRIMARYDOCUMENT = model.isPrimaryDocument,
               // COLLATERALCODE = model.collateralCode,
            };

            context.TBL_TEMP_MEDIA_COLLATERAL_DOCS.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralDocumentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Collateral Document '{ model.documentTitle }' ",
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

       
        public bool UpdateCollateralDocument(CollateralDocumentViewModel model, int documentId)
        {
            var data = this.context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Find(documentId);
            if (data == null)
            {
                return false;
            }

            data.DOCUMENTCODE= model.documentTitle;
            data.FILENAME = model.fileName;
            data.FILEEXTENSION = model.fileExtension;
            data.SYSTEMDATETIME = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralDocumentUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Collateral Document '{ model.documentTitle }' ",
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

        public IEnumerable<CollateralDocumentViewModel> GetAllCollateralDocument()
        {
            return this.context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Select(x => new CollateralDocumentViewModel
            {
                collateralId = x.COLLATERALCUSTOMERID,
                documentId = x.DOCUMENTID,
                documentTitle = x.DOCUMENTCODE,
                fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                targetId  = x.TARGETID
            });
        }
        public IEnumerable<CollateralDocumentViewModel> GetTempAllCollateralDocument(int collateralId)
        {
            string docName = "";
            var docMedia = this.context.TBL_TEMP_MEDIA_COLLATERAL_DOCS.Where(x => x.TEMPCOLLATERALCUSTOMERID == collateralId).Select(x => new CollateralDocumentViewModel
            {
                collateralId = x.TEMPCOLLATERALCUSTOMERID,
                documentId = x.DOCUMENTID,
                documentTitle = x.DOCUMENTCODE,
                fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                targetId = x.TARGETID,
                documentTypeId = x.DOCUMENTTYPEID,
                //documentType = x.DOCUMENTTYPEID != null ? docName : "N/A",
            }).ToList();

            foreach(var item in docMedia.ToList())
            {
                docName = bankingContext.TBL_COLLATERAL_DOCUMENT_TYPE.Where(p => p.DOCUMENTTYPEID == item.documentTypeId).Select(m => m.DOCUMENTTYPENAME).FirstOrDefault();
                if (!string.IsNullOrEmpty(docName))
                {
                    item.documentType = docName;
                }
            }

            var test = docMedia.ToList();


            return docMedia;
        }

        public CollateralDocumentViewModel GetCollateralDocument(int documentId)
        {
            var data = this.context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Find(documentId);

            if (data == null)
            {
                return null;
            }

            return new CollateralDocumentViewModel
            {
                documentId = data.DOCUMENTID,
                documentTitle = data.DOCUMENTCODE,
                fileData = data.FILEDATA,
                fileName = data.FILENAME,
                fileExtension = data.FILEEXTENSION,
            };
        }
        public IEnumerable<CollateralDocumentViewModel> GetCustomerCollateralReleaseDocument(int collateralId)
        {
            List<int> record = new List<int>();
            var data2 = (from a in bankingContext.TBL_COLLATERAL_RELEASE
                         join b in bankingContext.TBL_COLLATERAL_RELEASE_DOC on a.COLLATERALRELEASEID equals b.COLLATERALRELEASEID
                         where a.COLLATERALCUSTOMERID == collateralId
                         select new { b.DOCUMENTID }
                         ).ToList();
            foreach (var rec in data2)
            {
                record.Add(rec.DOCUMENTID);
            }
            var data = context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Where(x => x.COLLATERALCUSTOMERID == collateralId && record.Contains(x.DOCUMENTID)).Select(x => new CollateralDocumentViewModel
            {
                collateralId = x.COLLATERALCUSTOMERID,
                documentId = x.DOCUMENTID,
                documentTitle = x.DOCUMENTCODE,
                fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                targetId = x.TARGETID
            });
            return data.ToList();
        }

        public IEnumerable<CollateralDocumentViewModel> GetCustomerCollateralDocument(int collateralId)
        {
           var inVaultNAme=  bankingContext.TBL_COLLATERAL_RELEASE_STATUS.Find((int)CollateralReleaseStatus.InVault).COLLATERALRELEASESTATUSNAME;
            var releasetoBM = bankingContext.TBL_COLLATERAL_RELEASE_STATUS.Find((int)CollateralReleaseStatus.ReleasedToBM).COLLATERALRELEASESTATUSNAME;
            var ReleasedToCustomerNAme = bankingContext.TBL_COLLATERAL_RELEASE_STATUS.Find((int)CollateralReleaseStatus.ReleasedToCustomer).COLLATERALRELEASESTATUSNAME;
            var ReleasedToLegalNAme = bankingContext.TBL_COLLATERAL_RELEASE_STATUS.Find((int)CollateralReleaseStatus.ReleasedToLegal).COLLATERALRELEASESTATUSNAME;


            var data = (from x in context.TBL_DOCUMENT_USAGE 
                        join b in context.TBL_DOCUMENT_UPLOAD on x.DOCUMENTUPLOADID equals b.DOCUMENTUPLOADID
                        where x.TARGETID == collateralId
                        select new CollateralDocumentViewModel
            {
                collateralId = x.TARGETID,
                documentId = b.DOCUMENTUPLOADID,
                documentTitle = x.DOCUMENTTITLE,
                fileData = b.FILEDATA,
                fileName = b.FILENAME,
                fileExtension = b.FILEEXTENSION,
                targetId = x.TARGETID,
                isPrimaryDocumentValue = x.ISPRIMARYDOCUMENT == true ? "Yes":"No",
                //collateralReleaseStatusId =x.COLLATERALRELEASESTATUSID,
              //collateralReleaseStatusName = x.COLLATERALRELEASESTATUSID == null ? inVaultNAme : x.COLLATERALRELEASESTATUSID == (int)CollateralReleaseStatus.InVault ? inVaultNAme : x.COLLATERALRELEASESTATUSID == (int)CollateralReleaseStatus.ReleasedToBM ? releasetoBM : x.COLLATERALRELEASESTATUSID == (int)CollateralReleaseStatus.ReleasedToCustomer ? ReleasedToCustomerNAme: x.COLLATERALRELEASESTATUSID == (int)CollateralReleaseStatus.ReleasedToLegal ? ReleasedToLegalNAme : inVaultNAme,
          }).ToList();
            return data;
        }

        public IEnumerable<CollateralDocumentViewModel> GetTempCustomerCollateralDocument(int tempCollateralId)
        {
          var data = context.TBL_TEMP_MEDIA_COLLATERAL_DOCS.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).Select(x => new CollateralDocumentViewModel
            {
                collateralId = x.TEMPCOLLATERALCUSTOMERID,
                documentId = x.DOCUMENTID,
                documentTitle = x.DOCUMENTCODE,
                fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                targetId = x.TARGETID
            });
            return data.ToList();
        }

        public IEnumerable<CollateralDocumentViewModel> GetCollateralGuaranteeDocument(int targetId)
        {
            return this.GetAllCollateralDocument().Where(x => x.targetId == targetId).ToList();
        }



        public CollateralVisitationDocumentViewModel GetCollateralVisitationDocument(int collateralVisitationId)
        {
            var data = (from x in this.context.TBL_DOC_COLLATERAL_VISITATION
                        where x.COLLATERALVISITATIONID == collateralVisitationId
                        && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                        select new CollateralVisitationDocumentViewModel
                        {
                            documentId = x.DOCUMENTID,
                            collateralCustomerId = x.COLLATERALVISITATIONID,
                            fileData = x.FILEDATA,
                            fileName = x.FILENAME,
                            fileExtension = x.FILEEXTENSION,
                            CollateralVisitationID = x.COLLATERALVISITATIONID
                        });

            return data.FirstOrDefault();
        }
        public CollateralVisitationDocumentViewModel GetTempCollateralVisitationDocument(int collateralVisitationId)
        {
            var data = (from x in this.context.TBL_DOC_COLLATERAL_VISITATION
                        where x.COLLATERALVISITATIONID == collateralVisitationId
                        && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                        select new CollateralVisitationDocumentViewModel
                        {
                            documentId = x.DOCUMENTID,
                            collateralCustomerId = x.COLLATERALVISITATIONID,
                            fileData = x.FILEDATA,
                            fileName = x.FILENAME,
                            fileExtension = x.FILEEXTENSION,
                            CollateralVisitationID = x.COLLATERALVISITATIONID
                        });

            return data.FirstOrDefault();
        }


        public bool AddCollateralVisitation(CollateralDocumentViewModel model, byte[] file)
        {
          var visitationId =  coll.AddPropertyVistation(model);
            if (visitationId > 0)
            {


                var data = new Entities.DocumentModels.TBL_DOC_COLLATERAL_VISITATION
                {
                    FILEDATA = file,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    COLLATERALCUSTOMERID = Convert.ToInt32(model.collateralCustomerId),
                    SYSTEMDATETIME = DateTime.Now,
                    CREATEDBY = (int)model.createdBy,
                    COLLATERALVISITATIONID = visitationId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved,
                };

                context.TBL_DOC_COLLATERAL_VISITATION.Add(data);
            }
           // Audit Section ---------------------------
           var audit = new TBL_AUDIT
           {
               AUDITTYPEID = (short)AuditTypeEnum.CollateralDocumentAdded,
               STAFFID = model.createdBy,
               BRANCHID = (short)model.userBranchId,
               DETAIL = $"Added Collateral Visitation File '{ model.documentTitle }' ",
               IPADDRESS = CommonHelpers.GetLocalIpAddress(),
               URL =model.applicationUrl,
               APPLICATIONDATE = general.GetApplicationDate(),
               SYSTEMDATETIME = DateTime.Now,
               DEVICENAME = CommonHelpers.GetDeviceName(),
               OSNAME = CommonHelpers.FriendlyName()
           };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool AddTempCollateralVisitation(CollateralDocumentViewModel model, byte[] file)
        {
          var visitationId =  coll.AddPropertyVistation(model);
            if (visitationId > 0)
            {


                var data = new Entities.DocumentModels.TBL_DOC_COLLATERAL_VISITATION
                {
                    FILEDATA = file,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    COLLATERALCUSTOMERID = Convert.ToInt32(model.collateralCustomerId),
                    SYSTEMDATETIME = DateTime.Now,
                    CREATEDBY = (int)model.createdBy,
                    COLLATERALVISITATIONID = visitationId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                };

                context.TBL_DOC_COLLATERAL_VISITATION.Add(data);
            }
           // Audit Section ---------------------------
           var audit = new TBL_AUDIT
           {
               AUDITTYPEID = (short)AuditTypeEnum.CollateralDocumentAdded,
               STAFFID = model.createdBy,
               BRANCHID = (short)model.userBranchId,
               DETAIL = $"Added Collateral Visitation File '{ model.documentTitle }' ",
               IPADDRESS = CommonHelpers.GetLocalIpAddress(),
               URL =model.applicationUrl,
               APPLICATIONDATE = general.GetApplicationDate(),
               SYSTEMDATETIME = DateTime.Now,
               DEVICENAME = CommonHelpers.GetDeviceName(),
               OSNAME = CommonHelpers.FriendlyName()
           };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

       
    }
}
