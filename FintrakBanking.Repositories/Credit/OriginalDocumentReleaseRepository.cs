using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Repositories.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.General;
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
        private IStaffRoleRepository _staffRepo;
        private readonly IAuthenticationRepository _authRepo;

        public OriginalDocumentReleaseRepository(
                                                    FinTrakBankingContext context, 
                                                   IWorkflow workflow, 
                                                   IGeneralSetupRepository general,
                                                   FinTrakBankingDocumentsContext docContext,
                                                   IStaffRoleRepository staffRepo,
                                                   IAuthenticationRepository authRepo
                                                 )
        {
            _context = context;
            _workflow = workflow;
            _general = general;
            _docContext = docContext;
            _staffRepo = staffRepo;
            _authRepo = authRepo;
        }

        public bool AddOriginalDocumentRelease(IEnumerable<OriginalDocumentReleaseViewModel> model)
        {

            bool Update = false;

            foreach (var mod in model)
            {

                //check if part of the documents for a collateral is not being released...
                var docCheck = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(x => x.ORIGINALDOCUMENTAPPROVALID == mod.originalDocumentApprovalId
                                                                                && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                                                                     .Any();

                if(docCheck) throw new SecureException("Collateral Document(s) currently undergoing Approval");

                //check if the document was added to TBL_ORIGINAL_DOCUMENT_RELEASE but not sent for approval
                var resultCheck = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(x => x.DOCUMENTUPLOADID == mod.documentUploadId
                                                                                && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                                                                         .Any();

                if (resultCheck)
                {
                    Update = UpdateOriginalDocumentRelease(mod);
                    if (Update == false) return false;
                    else continue;
                }
                
                //check if the document was referred
                var resultReferred =    (from odr in _context.TBL_ORIGINAL_DOCUMENT_RELEASE
                                        join atrail in _context.TBL_APPROVAL_TRAIL on odr.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                                        where atrail.OPERATIONID == (int)OperationsEnum.SecurityRelease
                                            && atrail.TARGETID == odr.ORIGINALDOCUMENTAPPROVALID
                                            && atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred
                                            && odr.DOCUMENTUPLOADID == mod.documentUploadId
                                        select odr).FirstOrDefault();
                if (resultReferred != null)
                {
                    Update = UpdateOriginalDocumentRelease(mod);
                    if (Update) continue; 
                    else return false;
                }

                //check if the document is not currently undergoing approval
                var result = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(x => x.DOCUMENTUPLOADID == mod.documentUploadId
                                                                            && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                                                                   .Any();

                    if (result == true) throw new SecureException("One of the Selected Documents is currently Undergoing Approval");

                var entity = new TBL_ORIGINAL_DOCUMENT_RELEASE
                {
                    COLLATERALCUSTOMERID = _context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Where(oda => oda.ORIGINALDOCUMENTAPPROVALID == mod.originalDocumentApprovalId).Select(oda => oda.COLLATERALCUSTOMERID).FirstOrDefault(),
                    ORIGINALDOCUMENTRELEASEID = mod.originalDocumentReleaseId,
                    ORIGINALDOCUMENTAPPROVALID = mod.originalDocumentApprovalId,
                    DOCUMENTUPLOADID = mod.documentUploadId,
                    DOCSUBMISSIONOPERATIONID = (int)OperationsEnum.OriginalDocumentApproval,
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                    COMPANYID = mod.companyId,
                    CREATEDBY = mod.createdBy,
                    DATETIMECREATED = DateTime.Now,
                };

                var inUseCollateral = _context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.COLLATERALCUSTOMERID == entity.COLLATERALCUSTOMERID && x.DELETED == false).ToList();

                if (inUseCollateral.Any()) throw new SecureException("Cannot release document for a Collateral with existing Exposure(s)");

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

        public bool AddOriginalDocumentGuaranteeRelease(IEnumerable<OriginalDocumentReleaseViewModel> model)
        {

            bool Update = false;

            foreach (var mod in model)
            {

                //check if part of the documents for a collateral is not being released...
                var docCheck = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(x => x.ORIGINALDOCUMENTAPPROVALID == mod.originalDocumentApprovalId
                                                                                && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                                                                     .Any();

                if (docCheck) throw new SecureException("Collateral Documents is currently undergoing Approval");

                //check if the document was added to TBL_ORIGINAL_DOCUMENT_RELEASE but not sent for approval
                var resultCheck = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(x => x.DOCUMENTUPLOADID == mod.documentUploadId
                                                                                && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending)
                                                                         .Any();

                if (resultCheck)
                {
                    Update = UpdateOriginalDocumentRelease(mod);
                    if (Update == false) return false;
                    else continue;
                }

                //check if the document was referred
                var resultReferred = (from odr in _context.TBL_ORIGINAL_DOCUMENT_RELEASE
                                      join atrail in _context.TBL_APPROVAL_TRAIL on odr.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                                      where atrail.OPERATIONID == (int)OperationsEnum.GuaranteeReleaseApproval
                                          && atrail.TARGETID == odr.ORIGINALDOCUMENTAPPROVALID
                                          && atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred
                                          && odr.DOCUMENTUPLOADID == mod.documentUploadId
                                      select odr).FirstOrDefault();
                if (resultReferred != null)
                {
                    Update = UpdateOriginalDocumentRelease(mod);
                    if (Update) continue;
                    else return false;
                }

                //check if the document is not currently undergoing approval
                var result = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(x => x.DOCUMENTUPLOADID == mod.documentUploadId
                                                                            && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                                                                   .Any();

                if (result == true) throw new SecureException("One of the Selected Documents is currently Undergoing Approval");

                var entity = new TBL_ORIGINAL_DOCUMENT_RELEASE
                {
                    COLLATERALCUSTOMERID = _context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Where(oda => oda.ORIGINALDOCUMENTAPPROVALID == mod.originalDocumentApprovalId).Select(oda => oda.COLLATERALCUSTOMERID).FirstOrDefault(),
                    ORIGINALDOCUMENTRELEASEID = mod.originalDocumentReleaseId,
                    ORIGINALDOCUMENTAPPROVALID = mod.originalDocumentApprovalId,
                    DOCUMENTUPLOADID = mod.documentUploadId,
                    DOCSUBMISSIONOPERATIONID = (int)OperationsEnum.OriginalDocumentApproval,
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                    COMPANYID = mod.companyId,
                    CREATEDBY = mod.createdBy,
                    DATETIMECREATED = DateTime.Now,
                };

                var inUseCollateral = _context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.COLLATERALCUSTOMERID == entity.COLLATERALCUSTOMERID && x.DELETED == false).ToList();

                if (inUseCollateral.Any()) throw new SecureException("Cannot release document for a Collateral with existing Exposure(s)");

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
            ids.AddRange(_general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.GuaranteeReleaseApproval).ToList());
            var staffs = _general.GetStaffRlieved(staffId);

            var record = from dr in _context.TBL_ORIGINAL_DOCUMENT_RELEASE
                         join oda in _context.TBL_ORIGINAL_DOCUMENT_APPROVAL on dr.ORIGINALDOCUMENTAPPROVALID equals oda.ORIGINALDOCUMENTAPPROVALID
                         //join l in _context.TBL_LOAN_APPLICATION on oda.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join cc in _context.TBL_COLLATERAL_CUSTOMER on oda.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                         join atrail in _context.TBL_APPROVAL_TRAIL on dr.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                         join c in _context.TBL_CUSTOMER on cc.CUSTOMERID equals c.CUSTOMERID
                         where dr.DELETED == false && (atrail.APPROVALSTATUSID != (short)ApprovalStatusEnum.Approved || atrail.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved)
                         && atrail.RESPONSESTAFFID == null
                         && (atrail.LOOPEDSTAFFID == null || atrail.LOOPEDSTAFFID == staffId)
                         && ((ids.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.LOOPEDSTAFFID == null) || (!ids.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.LOOPEDSTAFFID == staffId))
                         && (atrail.TOSTAFFID == null || staffs.Contains((int)atrail.TOSTAFFID))
                         && (atrail.OPERATIONID == (int)OperationsEnum.SecurityRelease || atrail.OPERATIONID == (int)OperationsEnum.GuaranteeReleaseApproval)
                         select new OriginalDocumentReleaseViewModel
                         {
                             approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault().ToUpper(),
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
                             dateRecieved = atrail.ARRIVALDATE,
                             collateralId = cc.COLLATERALCUSTOMERID,
                             customerId = c.CUSTOMERID,
                             collateralCode = cc.COLLATERALCODE,
                             collateralCustomerId = cc.COLLATERALCUSTOMERID,
                             operationId = atrail.OPERATIONID,
                             perfectionStatusId = dr.PERFECTIONSTATUSID,
                             litigationStatusId = dr.LITIGATIONSTATUSID,
                             isOnAmconList = dr.ISONAMCONLIST,
                             numberOfTimesApprove = dr.NUMBEROFTIMESAPPROVE,
                             isAmconList = dr.ISONAMCONLIST !=null ? ((dr.ISONAMCONLIST == true) ? "Yes": "No") : "N/A",
                             perfectionStatus = dr.PERFECTIONSTATUSID !=null ? _context.TBL_COLLATERAL_PERFECTN_STAT.Where(p=>p.PERFECTIONSTATUSID == dr.PERFECTIONSTATUSID).Select(p=>p.PERFECTIONSTATUSNAME).FirstOrDefault() : "N/A",
                             litigationStatus = dr.LITIGATIONSTATUSID != null ? ((dr.LITIGATIONSTATUSID == 1) ? "Ongoing Court Case" : "No Ongoing Court Case") : "N/A",
                         };

            var result = record.GroupBy(r => r.originalDocumentApprovalId)
                               .Select( r =>r.FirstOrDefault()).ToList();
            return result;            
        }


        public IEnumerable<OriginalDocumentReleaseViewModel> GetSecurityReleaseSearch(string  searchString)
        {
            searchString = searchString.Trim().ToLower();

            var record = from dr in _context.TBL_ORIGINAL_DOCUMENT_RELEASE
                         join oda in _context.TBL_ORIGINAL_DOCUMENT_APPROVAL on dr.ORIGINALDOCUMENTAPPROVALID equals oda.ORIGINALDOCUMENTAPPROVALID
                         join cc in _context.TBL_COLLATERAL_CUSTOMER on oda.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                         join atrail in _context.TBL_APPROVAL_TRAIL on dr.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                         join c in _context.TBL_CUSTOMER on cc.CUSTOMERID equals c.CUSTOMERID
                         where dr.DELETED == false 
                         && (atrail.OPERATIONID == (int)OperationsEnum.SecurityRelease || atrail.OPERATIONID == (int)OperationsEnum.GuaranteeReleaseApproval)
                         && (c.CUSTOMERCODE.Contains(searchString)
                                || c.FIRSTNAME.ToLower().Contains(searchString)
                                || c.MIDDLENAME.ToLower().Contains(searchString)
                                || c.LASTNAME.ToLower().Contains(searchString))

                         select new OriginalDocumentReleaseViewModel
                         {
                             approvalStatus = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault().ToUpper(),
                             customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,
                             documentReferenceNumber = oda.REFERENCENUMBER,
                             docDateTimeCreated = dr.DATETIMECREATED,
                             createdByName = _context.TBL_STAFF.Where(o => o.STAFFID == dr.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.LASTNAME + " " + o.MIDDLENAME).FirstOrDefault(),
                             documentDescription = oda.DESCRIPTION,
                             originalDocumentApprovalId = oda.ORIGINALDOCUMENTAPPROVALID,
                             originalDocumentReleaseId = dr.ORIGINALDOCUMENTRELEASEID,
                             docSubmissionOperationId = dr.DOCSUBMISSIONOPERATIONID,
                             approvalDate = dr.APPROVALDATE,
                             dateRecieved = atrail.ARRIVALDATE,
                             collateralId = cc.COLLATERALCUSTOMERID,
                             customerId = c.CUSTOMERID,
                             collateralCode = cc.COLLATERALCODE,
                             collateralCustomerId = cc.COLLATERALCUSTOMERID,
                             operationId = atrail.OPERATIONID,
                             perfectionStatusId = dr.PERFECTIONSTATUSID,
                             litigationStatusId = dr.LITIGATIONSTATUSID,
                             isOnAmconList = dr.ISONAMCONLIST,
                             currentApprovalLevel = atrail.TOAPPROVALLEVELID != null ? ((atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && atrail.LOOPEDSTAFFID != null) ? _context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == atrail.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : _context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).LEVELNAME) : "N/A",
                             responsiblePerson = atrail.TOSTAFFID == null ? "N/A" : atrail.TBL_STAFF1.STAFFCODE + " - " + atrail.TBL_STAFF1.FIRSTNAME + " " + atrail.TBL_STAFF1.MIDDLENAME + " " + atrail.TBL_STAFF1.LASTNAME,
                         };

            var result = record.GroupBy(r => r.originalDocumentApprovalId)
                               .Select(r => r.FirstOrDefault()).ToList();
            return result;
        }

        public IEnumerable<OriginalDocumentReleaseViewModel> GetRejectedAndReferredSecurityRelease(int staffId)
        {
            //var ids = _general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.SecurityRelease).ToList();
            var initiator = _context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.SecurityRelease || o.OPERATIONID == (int)OperationsEnum.GuaranteeReleaseApproval).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault();

            var record = (from oda in _context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                          join dr in _context.TBL_ORIGINAL_DOCUMENT_RELEASE  on oda.ORIGINALDOCUMENTAPPROVALID equals dr.ORIGINALDOCUMENTAPPROVALID
                         //join l in _context.TBL_LOAN_APPLICATION on oda.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                         join cc in _context.TBL_COLLATERAL_CUSTOMER on oda.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                         join atrail in _context.TBL_APPROVAL_TRAIL on dr.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                         join c in _context.TBL_CUSTOMER on cc.CUSTOMERID equals c.CUSTOMERID
                         where dr.DELETED == false
                            && (atrail.OPERATIONID == (int)OperationsEnum.SecurityRelease || atrail.OPERATIONID == (int)OperationsEnum.GuaranteeReleaseApproval)
                            && atrail.TARGETID == dr.ORIGINALDOCUMENTAPPROVALID
                            && ((atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred
                            && atrail.LOOPEDSTAFFID == initiator) 
                                || atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved || atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                            && atrail.RESPONSESTAFFID == null
                          orderby atrail.APPROVALTRAILID descending
                          select new OriginalDocumentReleaseViewModel
                         {
                             approvalStatusId = atrail.APPROVALSTATUSID,
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
                             dateRecieved = atrail.ARRIVALDATE,
                             collateralCode = cc.COLLATERALCODE,
                             collateralCustomerId = cc.COLLATERALCUSTOMERID,
                             operationId = (int)OperationsEnum.SecurityRelease,
                             collateralId = cc.COLLATERALCUSTOMERID,
                             loopedStaffId = atrail.LOOPEDSTAFFID,
                             approvalTrailId = atrail.APPROVALTRAILID,
                          }).ToList();

            var result = record.GroupBy(r => r.originalDocumentApprovalId)
                               .Select(r => r.FirstOrDefault()).Where(first => (first.approvalStatusId == (short)ApprovalStatusEnum.Referred
                               && first.loopedStaffId == initiator) || first.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
                               .ToList();

            return result;
        }
        
        public bool reinitiateSecurityRelease(int id, int staffId, int companyId)
        {
            var output = false;

            var rejected = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Find(id);

            if(rejected != null)
            {

                var documentUploadIds = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(odr => odr.ORIGINALDOCUMENTAPPROVALID == rejected.ORIGINALDOCUMENTAPPROVALID
                                                                                    && odr.DELETED == true
                                                                                    && odr.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved)
                                                                          .Select(odr => odr.DOCUMENTUPLOADID).DefaultIfEmpty().ToList();

                var rejectedDocumentList = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(odr => odr.ORIGINALDOCUMENTAPPROVALID == rejected.ORIGINALDOCUMENTAPPROVALID
                                                                                            && odr.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved
                                                                                            && !documentUploadIds.Contains(rejected.DOCUMENTUPLOADID)).ToList(); 
                foreach(var rej in rejectedDocumentList)
                {
                    
                    rej.APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending;
                    rej.LASTUPDATEDBY = staffId;
                    rej.DATETIMEUPDATED = DateTime.Now;
                }

                var result = _context.SaveChanges() > 0;
                if (result)
                {
                    _workflow.StaffId = staffId;
                    _workflow.CompanyId = companyId;
                    _workflow.StatusId = (short)ApprovalStatusEnum.Processing;
                    _workflow.TargetId = rejected.ORIGINALDOCUMENTAPPROVALID;
                    _workflow.Comment = "Request for security release approval";
                    _workflow.OperationId = (int)OperationsEnum.SecurityRelease;
                    _workflow.DeferredExecution = true;
                    _workflow.ExternalInitialization = true;
                    _workflow.LogActivity();

                    if (_context.SaveChanges() > 0)
                    {
                        foreach (var model in rejectedDocumentList)
                        {
                            model.APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing;
                        }
                    }
                }
                output = _context.SaveChanges() > 0;
            }

            return output;
        }

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
            var record = entity.GroupBy(x => x.originalDocumentApprovalId).Select(x => x.FirstOrDefault()).Where(x => x.approvalStatusId == (short)ApprovalStatusEnum.Pending); 
            var recordReferred = entity.GroupBy(x => x.originalDocumentApprovalId).Select(x => x.FirstOrDefault()).Where(x => x.approvalStatusId == (short)ApprovalStatusEnum.Referred );

            if (recordReferred != null)
            {

                foreach (var x in recordReferred)
                {
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        _workflow.StaffId = x.createdBy;
                        _workflow.CompanyId = x.companyId;
                        _workflow.StatusId = (short)ApprovalStatusEnum.Processing;
                        _workflow.TargetId = x.originalDocumentApprovalId;
                        _workflow.Comment = "Update has been applied, Request for Security Release Approval";
                        _workflow.OperationId = (int)OperationsEnum.SecurityRelease;
                        _workflow.DeferredExecution = true;
                        _workflow.LogActivity();
                        try
                        {
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }

            if(record != null)
            {
                foreach (var x in record)
                {
                    var data = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(t => t.ORIGINALDOCUMENTAPPROVALID == x.originalDocumentApprovalId 
                                                                                && t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending).ToList();
                    _workflow.StaffId = x.createdBy;
                    _workflow.CompanyId = x.companyId;
                    _workflow.StatusId = (short)ApprovalStatusEnum.Processing;
                    _workflow.TargetId = x.originalDocumentApprovalId;
                    _workflow.Comment = "Request for security release approval";
                    _workflow.OperationId = (int)OperationsEnum.SecurityRelease;
                    _workflow.DeferredExecution = true;
                    _workflow.ExternalInitialization = true;
                    _workflow.LogActivity();

                    if (_context.SaveChanges() > 0)
                    {
                        foreach (var model in data)
                        {
                            model.APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing;
                        }
                    }
                }
            }

            return _context.SaveChanges() != 0;
        }

        public WorkflowResponse GoForGuaranteeApproval(IEnumerable<OriginalDocumentReleaseViewModel> entity)
        {
            var record = entity.GroupBy(x => x.originalDocumentApprovalId).Select(x => x.FirstOrDefault()).Where(x => x.approvalStatusId == (short)ApprovalStatusEnum.Pending);
            var recordReferred = entity.GroupBy(x => x.originalDocumentApprovalId).Select(x => x.FirstOrDefault()).Where(x => x.approvalStatusId == (short)ApprovalStatusEnum.Referred);

            if (recordReferred != null)
            {

                foreach (var x in recordReferred)
                {
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        _workflow.StaffId = x.createdBy;
                        _workflow.CompanyId = x.companyId;
                        _workflow.StatusId = (short)ApprovalStatusEnum.Processing;
                        _workflow.TargetId = x.originalDocumentApprovalId;
                        _workflow.Comment = "Update has been applied, Request for Guarantee Release Approval";
                        _workflow.OperationId = (int)OperationsEnum.GuaranteeReleaseApproval;
                        _workflow.DeferredExecution = true;
                        _workflow.LogActivity();
                        try
                        {
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }

            if (record != null)
            {
                foreach (var x in record)
                {
                    var data = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(t => t.ORIGINALDOCUMENTAPPROVALID == x.originalDocumentApprovalId
                                                                                && t.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending).ToList();
                    _workflow.StaffId = x.createdBy;
                    _workflow.CompanyId = x.companyId;
                    _workflow.StatusId = (short)ApprovalStatusEnum.Processing;
                    _workflow.TargetId = x.originalDocumentApprovalId;
                    _workflow.Comment = "Request for gurantee security release approval";
                    _workflow.OperationId = (int)OperationsEnum.GuaranteeReleaseApproval;
                    _workflow.DeferredExecution = true;
                    _workflow.ExternalInitialization = true;
                    _workflow.LogActivity();

                    if (_context.SaveChanges() > 0)
                    {
                        foreach (var model in data)
                        {
                            model.APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing;
                        }
                    }
                }
            }
            _context.SaveChanges();
            return _workflow.Response;
        }

        public WorkflowResponse SubmitApproval(OriginalDocumentReleaseViewModel model)
        {
            bool responce = false;

            using (var transaction = _context.Database.BeginTransaction())
            {
               _workflow.StaffId = model.createdBy;
               _workflow.CompanyId = model.companyId;
               _workflow.StatusId = model.approvalStatusId == (short)ApprovalStatusEnum.Approved ? (short)ApprovalStatusEnum.Processing : model.approvalStatusId;
               _workflow.TargetId = model.originalDocumentApprovalId;
               _workflow.Comment = model.comment;
                _workflow.OperationId = (int)model.operationId; //model.docSubmissionOperationId; //(int)OperationsEnum.SecurityRelease;
               _workflow.DeferredExecution = true;
               _workflow.LogActivity();

                try
                {
                    var documents = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(o => o.ORIGINALDOCUMENTAPPROVALID == model.originalDocumentApprovalId
                                                                                    && o.DELETED == false
                                                                                    && o.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing).ToList();

                    var staffRole = _staffRepo.GetStaffRoleByStaffId(model.staffId);
                    var userActivities = _authRepo.GetUserActivitiesByUser(model.userId);

                    if (userActivities.IndexOf("perfection approval") > -1) {
                        foreach (var item in documents) {
                            item.PERFECTIONSTATUSID = model.perfectionStatusId;
                        }
                    }

                    if (userActivities.IndexOf("litigation approval") > -1) {
                        foreach (var item in documents) {
                            item.LITIGATIONSTATUSID = model.litigationStatusId;
                        }
                    }

                    if (staffRole.staffRoleCode == "AMCON OFFICER") {
                        foreach (var item in documents) {
                            item.ISONAMCONLIST = model.isOnAmconList;
                            item.NUMBEROFTIMESAPPROVE = item.NUMBEROFTIMESAPPROVE + 1;
                        }
                    }

                    if (_workflow.NewState == (int)ApprovalState.Ended)
                    {

                        if (documents != null)
                        {
                            foreach (var x in documents)
                            {
                                x.APPROVALSTATUSID = model.approvalStatusId;
                                x.APPROVALDATE = _general.GetApplicationDate();

                                if(model.approvalStatusId == (short)ApprovalStatusEnum.Approved)
                                {
                                    x.DELETED = true;
                                    x.DELETEDBY = model.createdBy;
                                    x.DATETIMEDELETED = _general.GetApplicationDate();
                                }
                                
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

        public IEnumerable<DocumentUploadViewModel> GetReleasedDocUploadIds(int operationId, int targetId, int staffId)
        {
            
            var documentUploadIds = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(odr => odr.ORIGINALDOCUMENTAPPROVALID == targetId
                                                                                    && odr.DELETED == false
                                                                                    && odr.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing)
                                                                          .Select(odr => odr.DOCUMENTUPLOADID).ToList();

             var documents = _docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
               .Join(_docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
               , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) =>
                   new {
                       documentUploadId = up.DOCUMENTUPLOADID,
                       fileName = up.FILENAME,
                       fileExtension = up.FILEEXTENSION,
                       fileSize = up.FILESIZE,
                       fileSizeUnit = up.FILESIZEUNIT,
                       fileData = up.FILEDATA,
                       companyId = up.COMPANYID,
                       issueDate = up.ISSUEDATE,
                       expiryDate = up.EXPIRYDATE,
                       physicalFilenumber = up.PHYSICALFILENUMBER,
                       physicalLocation = up.PHYSICALLOCATION,
                       documentTypeId = up.DOCUMENTTYPEID,
                       documentTypeName = up.TBL_DOCUMENT_TYPE.DOCUMENTTYPENAME,
                       documentCategoryId = up.TBL_DOCUMENT_TYPE.DOCUMENTCATEGORYID,
                       documentCategoryName = up.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                       owner = us.CREATEDBY == staffId,
                       dateTimeCreated = us.DATETIMECREATED,
                       dateTimeUpdated = us.DATETIMEUPDATED,
                       createdBy = us.CREATEDBY.Value,
                   }
           ).AsEnumerable()
           .Where(x => documentUploadIds.Contains(x.documentUploadId))
           .Select(up => new DocumentUploadViewModel
           {
               documentUploadId = up.documentUploadId,
               fileName = up.fileName,
               fileExtension = up.fileExtension,
               fileSize = up.fileSize,
               fileSizeUnit = up.fileSizeUnit,
               fileData = up.fileData,
               companyId = up.companyId,
               issueDate = up.issueDate,
               expiryDate = up.expiryDate,
               physicalFilenumber = up.physicalFilenumber,
               physicalLocation = up.physicalLocation,
               documentTypeId = up.documentTypeId,
               documentTypeName = up.documentTypeName,
               documentCategoryId = up.documentCategoryId,
               documentCategoryName = up.documentCategoryName,
               owner = up.owner,
               dateTimeCreated = up.dateTimeCreated,
               dateTimeUpdated = up.dateTimeUpdated,
               createdBy = up.createdBy,
               uploadedBy = _context.TBL_STAFF.Where(s => s.STAFFID == up.createdBy && s.DELETED != true).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + "(" + s.STAFFCODE + ")").FirstOrDefault(),

           })
           .OrderBy(x => x.dateTimeCreated)
           .ThenBy(x => x.documentCategoryId)
           .ThenBy(x => x.documentTypeId)
           .ToList();

            return documents;
        }

        public IEnumerable<DocumentUploadViewModel> GetAvailableDocumentsForReleease(int operationId, int targetId, int staffId)
        {
            
            var documentUploadIds = _context.TBL_ORIGINAL_DOCUMENT_RELEASE.Where(odr => odr.ORIGINALDOCUMENTAPPROVALID == targetId
                                                                                    && odr.DELETED == true
                                                                                    && odr.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved)
                                                                          .Select(odr => odr.DOCUMENTUPLOADID).DefaultIfEmpty().ToList();


            var documents = _docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
               .Join(_docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
               , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) =>
                   new {
                       documentUploadId = up.DOCUMENTUPLOADID,
                       fileName = up.FILENAME,
                       fileExtension = up.FILEEXTENSION,
                       fileSize = up.FILESIZE,
                       fileSizeUnit = up.FILESIZEUNIT,
                       fileData = up.FILEDATA,
                       companyId = up.COMPANYID,
                       issueDate = up.ISSUEDATE,
                       expiryDate = up.EXPIRYDATE,
                       physicalFilenumber = up.PHYSICALFILENUMBER,
                       physicalLocation = up.PHYSICALLOCATION,
                       documentTypeId = up.DOCUMENTTYPEID,
                       documentTypeName = up.TBL_DOCUMENT_TYPE.DOCUMENTTYPENAME,
                       documentCategoryId = up.TBL_DOCUMENT_TYPE.DOCUMENTCATEGORYID,
                       documentCategoryName = up.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                       owner = us.CREATEDBY == staffId,
                       dateTimeCreated = us.DATETIMECREATED,
                       dateTimeUpdated = us.DATETIMEUPDATED,
                       createdBy = us.CREATEDBY.Value,
                   }
           ).AsEnumerable()
           .Where(x => !documentUploadIds.Contains(x.documentUploadId))
           .Select(up => new DocumentUploadViewModel
           {
               documentUploadId = up.documentUploadId,
               fileName = up.fileName,
               fileExtension = up.fileExtension,
               fileSize = up.fileSize,
               fileSizeUnit = up.fileSizeUnit,
               fileData = up.fileData,
               companyId = up.companyId,
               issueDate = up.issueDate,
               expiryDate = up.expiryDate,
               physicalFilenumber = up.physicalFilenumber,
               physicalLocation = up.physicalLocation,
               documentTypeId = up.documentTypeId,
               documentTypeName = up.documentTypeName,
               documentCategoryId = up.documentCategoryId,
               documentCategoryName = up.documentCategoryName,
               owner = up.owner,
               dateTimeCreated = up.dateTimeCreated,
               dateTimeUpdated = up.dateTimeUpdated,
               createdBy = up.createdBy,
               uploadedBy = _context.TBL_STAFF.Where(s => s.STAFFID == up.createdBy && s.DELETED != true).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + "(" + s.STAFFCODE + ")").FirstOrDefault(),

           })
           .OrderBy(x => x.dateTimeCreated)
           .ThenBy(x => x.documentCategoryId)
           .ThenBy(x => x.documentTypeId)
           .ToList();

            return documents;
        }
        

    }
}
