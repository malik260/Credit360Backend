using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class OriginalDocumentReleaseRepository : IOriginalDocumentReleaseRepository
    {
        private FinTrakBankingContext _context;
        private IWorkflow _workflow;
        public OriginalDocumentReleaseRepository(FinTrakBankingContext context, IWorkflow workflow)
        {
            _context = context;
            _workflow = workflow;
        }

        public bool AddOriginalDocumentRelease(IEnumerable<OriginalDocumentReleaseViewModel> model)
        {
            foreach (var o in model)
            {
                var result = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(x => x.DOCUMENTUPLOADID == o.documentUploadId).Any();

                if (result == true) continue;
                var entity = new TBL_ORIGINAL_DOCUMENT_RELEASE
                {
                    ORIGINALDOCUMENTRELEASEID = o.originalDocumentReleaseId,
                    ORIGINALDOCUMENTAPPROVALID = o.originalDocumentApprovalId,
                    DOCUMENTUPLOADID = o.documentUploadId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    COMPANYID = o.companyId,
                    CREATEDBY = o.createdBy,
                    DATETIMECREATED = DateTime.Now
                };
                _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Add(entity);


            }
            try
            {
                return _context.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<OriginalDocumentReleaseViewModel> GetLeaseDocumentForApproval(int staffId)
        {
            var record = from dr in _context.TBL_ORIGINAL_DOCUMENT_RELEASE
                         join oda in _context.TBL_ORIGINAL_DOCUMENT_APPROVAL on dr.ORIGINALDOCUMENTAPPROVALID equals oda.ORIGINALDOCUMENTAPPROVALID
                         join l in _context.TBL_LOAN_APPLICATION on oda.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join c in _context.TBL_CUSTOMER on l.CUSTOMERID equals c.CUSTOMERID
                         select new OriginalDocumentReleaseViewModel
                         {
                             customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,
                             applicationReferenceNumber = l.APPLICATIONREFERENCENUMBER,
                             documentReferenceNumber = oda.REFERENCENUMBER,
                             docDateTimeCreated = dr.DATETIMECREATED,
                             createdByName = _context.TBL_STAFF.Where(o => o.STAFFID == staffId).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                             documentDescription = oda.DESCRIPTION,
                             originalDocumentApprovalId = dr.APPROVALSTATUSID,
                             operationId = (int)OperationsEnum.OriginalDocumentApproval
                         };
            return record.ToList();
        }

        //public OriginalDocumentReleaseViewModel GetOriginalDocmentReleaseById(int id)
        //{
        //    var entity = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(t => t.DOCUMENTUPLOADID == id)
        //        .FirstOrDefault();
        //    return new OriginalDocumentReleaseViewModel
        //    {
        //        originalDocumentReleaseId = entity.ORIGINALDOCUMENTRELEASEID,
        //        originalDocumentApprovalId = entity.ORIGINALDOCUMENTAPPROVALID,
        //        documentUploadId = entity.DOCUMENTUPLOADID,
        //        approvalStatusId = entity.APPROVALSTATUSID,
        //        companyId = entity.COMPANYID
        //    };
        //}

        public IEnumerable<OriginalDocumentReleaseViewModel> GetOriginalAllDocmentRelease(int id)
        {
            return (_context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(t => t.ORIGINALDOCUMENTAPPROVALID == id)
                .Select(t => new OriginalDocumentReleaseViewModel
                {
                    originalDocumentReleaseId = t.ORIGINALDOCUMENTRELEASEID,
                    originalDocumentApprovalId = t.ORIGINALDOCUMENTAPPROVALID,
                    documentUploadId = t.DOCUMENTUPLOADID,
                    approvalStatusId = t.APPROVALSTATUSID,
                    companyId = t.COMPANYID
                }));

        }

        

        public bool saveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public bool GoForApproval(IEnumerable<OriginalDocumentReleaseViewModel> entity)
        {
            foreach (var x in entity)
            {
                var data = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(t => t.DOCUMENTUPLOADID == x.documentUploadId && t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending).Select(t => t).FirstOrDefault();

                if (data != null)
                {
                    data.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

                    _workflow.StaffId = x.createdBy;
                    _workflow.CompanyId = x.companyId;
                    _workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    _workflow.TargetId = x.originalDocumentApprovalId;
                    _workflow.Comment = "Request for security release approval";
                    _workflow.OperationId = (int)OperationsEnum.SecurityRelease;
                    _workflow.DeferredExecution = true;
                    _workflow.ExternalInitialization = true;
                    _workflow.LogActivity();
                }
            }
            return _context.SaveChanges() != 0;


        }
    }
}
