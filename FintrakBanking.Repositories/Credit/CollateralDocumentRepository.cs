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

namespace FintrakBanking.Repositories.Credit
{
    public class CollateralDocumentRepository : ICollateralDocumentRepository
    {
        private FinTrakBankingDocumentsContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private ICustomerCollateralRepository coll;
         private ICollateralDocumentRepository document;

        public CollateralDocumentRepository(CustomerCollateralRepository coll, FinTrakBankingDocumentsContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
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
                ISPRIMARYDOCUMENT = model.isPrimaryDocument
            };

            context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralDocumentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Collateral Document '{ model.documentTitle }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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
            });
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

        public IEnumerable<CollateralDocumentViewModel> GetCustomerCollateralDocument(int documentId)
        {
            return this.GetAllCollateralDocument().Where(x => x.collateralId == documentId).ToList();
        }




        public CollateralVisitationDocumentViewModel GetCollateralVisitationDocument(int collateralVisitationId)
        {
            var data = (from x in this.context.TBL_DOC_COLLATERAL_VISITATION
                        where x.COLLATERALVISITATIONID == collateralVisitationId
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
                    // COLLATERALCODE ="aaasss",
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
               IPADDRESS = model.userIPAddress,
               URL = model.applicationUrl,
               APPLICATIONDATE = general.GetApplicationDate(),
               SYSTEMDATETIME = DateTime.Now
           };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

       
    }
}
