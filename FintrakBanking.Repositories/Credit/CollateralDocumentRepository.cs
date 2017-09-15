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

        public CollateralDocumentRepository(FinTrakBankingDocumentsContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
        }

        public bool AddCollateralDocument(CollateralDocumentViewModel model, byte[] file)
        {
            var data = new tbl_Media_Collateral_Documents
            {
                FileData = file,
                DocumentCode = model.documentTitle,
                FileName = model.fileName,
                FileExtension = model.fileExtension,
                CollateralCustomerId = model.collateralId,
                SystemDateTime = DateTime.Now,
                CreatedBy = (int)model.createdBy,
            };

            context.tbl_Media_Collateral_Documents.Add(data);

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CollateralDocumentAdded,
                StaffId = model.createdBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Added Collateral Document '{ model.documentTitle }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateCollateralDocument(CollateralDocumentViewModel model, int documentId)
        {
            var data = this.context.tbl_Media_Collateral_Documents.Find(documentId);
            if (data == null)
            {
                return false;
            }

            data.DocumentCode= model.documentTitle;
            data.FileName = model.fileName;
            data.FileExtension = model.fileExtension;
            data.SystemDateTime = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.CollateralDocumentUpdated,
                StaffId = model.lastUpdatedBy,
                BranchId = (short)model.userBranchId,
                Detail = $"Updated Collateral Document '{ model.documentTitle }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = general.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<CollateralDocumentViewModel> GetAllCollateralDocument()
        {
            return this.context.tbl_Media_Collateral_Documents.Select(x => new CollateralDocumentViewModel
            {
                collateralId = x.CollateralCustomerId,
                documentId = x.DocumentId,
                documentTitle = x.DocumentCode,
                fileData = x.FileData,
                fileName = x.FileName,
                fileExtension = x.FileExtension,
            });
        }

        public CollateralDocumentViewModel GetCollateralDocument(int documentId)
        {
            var data = this.context.tbl_Media_Collateral_Documents.Find(documentId);

            if (data == null)
            {
                return null;
            }

            return new CollateralDocumentViewModel
            {
                documentId = data.DocumentId,
                documentTitle = data.DocumentCode,
                fileData = data.FileData,
                fileName = data.FileName,
                fileExtension = data.FileExtension,
            };
        }

        public IEnumerable<CollateralDocumentViewModel> GetCustomerCollateralDocument(int collateralId)
        {
            return this.GetAllCollateralDocument().Where(x => x.collateralId == collateralId).ToList();
        }

    }
}
