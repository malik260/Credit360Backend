using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Repositories.WorkFlow;
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
        private IGeneralSetupRepository _general;
        private FinTrakBankingDocumentsContext _docContext;
        public OriginalDocumentReleaseRepository(
                                                    FinTrakBankingContext context, 
                                                   IWorkflow workflow, 
                                                   IGeneralSetupRepository general,
                                                   FinTrakBankingDocumentsContext docContext
                                                 )
        {
            _context = context;
            _workflow = workflow;
            _general = general;
        }

        public bool AddOriginalDocumentRelease(IEnumerable<OriginalDocumentReleaseViewModel> model)
        {

            bool Update = false;

            foreach (var mod in model)
            {
                

                var resultCheck = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(x => x.DOCUMENTUPLOADID == mod.documentUploadId
                                                                                && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                                                                         .Any();
                if (resultCheck)
                {
                    Update = UpdateOriginalDocumentRelease(mod);
                    if (Update == false) return false;
                    else continue;
                }

                var result = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(x => x.DOCUMENTUPLOADID == mod.documentUploadId
                                                                            && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                                                                   .Any(); //test this guy, add a rejected entry in the database



                if (result == true) return false;
             
                    var entity = new TBL_ORIGINAL_DOCUMENT_RELEASE
                    {
                        ORIGINALDOCUMENTRELEASEID = mod.originalDocumentReleaseId,
                        ORIGINALDOCUMENTAPPROVALID = mod.originalDocumentApprovalId,
                        DOCUMENTUPLOADID = mod.documentUploadId,
                        DOCSUBMISSIONOPERATIONID = (int)OperationsEnum.OriginalDocumentApproval,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                        COMPANYID = mod.companyId,
                        CREATEDBY = mod.createdBy,
                        DATETIMECREATED = DateTime.Now
                    };
                    _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Add(entity);
             
            }
            try
            {
                return _context.SaveChanges() > 0 || Update == true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdateOriginalDocumentRelease(OriginalDocumentReleaseViewModel model)
        {

            var entity = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.FirstOrDefault(ct => ct.DOCUMENTUPLOADID == model.documentUploadId);

            if (entity != null)
            {
                entity.LASTUPDATEDBY = model.createdBy;
                entity.DATETIMEUPDATED = DateTime.Now;
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
            var ids = _general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.SecurityRelease).ToList();

            var record = from dr in _context.TBL_ORIGINAL_DOCUMENT_RELEASE
                         join oda in _context.TBL_ORIGINAL_DOCUMENT_APPROVAL on dr.ORIGINALDOCUMENTAPPROVALID equals oda.ORIGINALDOCUMENTAPPROVALID
                         //join l in _context.TBL_LOAN_APPLICATION on oda.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join cc in _context.TBL_COLLATERAL_CUSTOMER on oda.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                         join atrail in _context.TBL_APPROVAL_TRAIL on dr.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                         join c in _context.TBL_CUSTOMER on cc.CUSTOMERID equals c.CUSTOMERID
                         where dr.DELETED == false && (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                         && atrail.RESPONSESTAFFID == null
                         && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                         && atrail.OPERATIONID == (int)OperationsEnum.SecurityRelease
                         select new OriginalDocumentReleaseViewModel
                         {
                             approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                             customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,
                             //applicationReferenceNumber = l.APPLICATIONREFERENCENUMBER,
                             documentReferenceNumber = oda.REFERENCENUMBER,
                             docDateTimeCreated = dr.DATETIMECREATED,
                             createdByName = _context.TBL_STAFF.Where(o => o.STAFFID == dr.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                             documentDescription = oda.DESCRIPTION,
                             originalDocumentApprovalId = oda.ORIGINALDOCUMENTAPPROVALID,
                            originalDocumentReleaseId = dr.ORIGINALDOCUMENTRELEASEID,
                            docSubmissionOperationId = dr.DOCSUBMISSIONOPERATIONID,
                             approvalDate = dr.APPROVALDATE,
                             collateralCode = cc.COLLATERALCODE,
                             collateralCustomerId = cc.COLLATERALCUSTOMERID,
                             operationId = (int)OperationsEnum.SecurityRelease
                         };

            var result = record.GroupBy(r => r.originalDocumentApprovalId)
                               .Select( r =>r.FirstOrDefault()).ToList();

            return result;
            //return record.ToList();
            
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
                    approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == t.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                    companyId = t.COMPANYID

                }));

        }

        

        public bool saveChanges()
        {
            return _context.SaveChanges() > 0;
        }

        public bool GoForApproval(IEnumerable<OriginalDocumentReleaseViewModel> entity)
        {
            var record = entity.GroupBy(x => x.originalDocumentApprovalId).Select(x => x.First());

            foreach (var x in record)
            {
                var data = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(t => t.ORIGINALDOCUMENTAPPROVALID == x.originalDocumentApprovalId && t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending).FirstOrDefault();

                if (data != null)
                {
                    var release = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(o => o.ORIGINALDOCUMENTAPPROVALID == data.ORIGINALDOCUMENTAPPROVALID).ToList();
                    //data.CREATEDBY = x.createdBy;

                    foreach (var d in release)
                    {
                        d.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
                        //d.CREATEDBY = x.createdBy;
                    }

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

        public WorkflowResponse SubmitApproval(OriginalDocumentReleaseViewModel model)
        {
            bool responce = false;

            using (var transaction = _context.Database.BeginTransaction())
            {
               _workflow.StaffId = model.createdBy;
               _workflow.CompanyId = model.companyId;
               _workflow.StatusId = (int)ApprovalStatusEnum.Processing;
               _workflow.TargetId = model.originalDocumentApprovalId;
               _workflow.Comment = model.comment;
               _workflow.OperationId = (int)OperationsEnum.SecurityRelease;
               _workflow.DeferredExecution = true;
               _workflow.LogActivity();
                try
                {
                    if (_workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var documents = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(o => o.ORIGINALDOCUMENTAPPROVALID == model.originalDocumentApprovalId).ToList();
                        if (documents != null)
                        {
                            foreach (var x in documents)
                            {
                                x.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                                x.APPROVALDATE = _general.GetApplicationDate();
                            }
                        }

                    }

                    responce = _context.SaveChanges() > 0;
                    transaction.Commit();

                    return _workflow.Response;
                }
                catch (Exception ex)
                {

                    transaction.Rollback();


                    throw ex;
                }
                //return false;
            }
        }
    }
}
