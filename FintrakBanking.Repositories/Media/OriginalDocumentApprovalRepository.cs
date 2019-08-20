using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Media;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Media;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Media
{
    public class OriginalDocumentApprovalRepository : IOriginalDocumentApprovalRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;


        public OriginalDocumentApprovalRepository(
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow
            )
        {
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
        }

        public IEnumerable<OriginalDocumentApprovalViewModel> GetOriginalDocumentApprovals(int staffId)
        {
            var data = new List<OriginalDocumentApprovalViewModel>();
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.OriginalDocumentApproval).ToList();

            var initiator = context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault();

            data = (from x in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                   join o in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals o.COLLATERALCUSTOMERID
                    join c in context.TBL_CUSTOMER on o.CUSTOMERID equals c.CUSTOMERID
                    join atrail in context.TBL_APPROVAL_TRAIL on x.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                    where x.DELETED == false && (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred)
                     && atrail.RESPONSESTAFFID == null
                      && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                     && atrail.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval

                    select new OriginalDocumentApprovalViewModel
                    {
                        originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                        loanApplicationId = x.LOANAPPLICATIONID,
                        description = x.DESCRIPTION,
                        collateralCode = o.COLLATERALCODE,
                        collateralType = context.TBL_COLLATERAL_TYPE.Where(a=>a.COLLATERALTYPEID==o.COLLATERALTYPEID).Select(o=>o.COLLATERALTYPENAME).FirstOrDefault(),
                        collateralTypeId = o.COLLATERALTYPEID,
                        approvalStatusId = x.APPROVALSTATUSID,
                        applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                        referenceNumber = x.REFERENCENUMBER,
                        dateTimeCreated = x.DATETIMECREATED,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        customerId = c.CUSTOMERID,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        operationId = atrail.OPERATIONID,
                        approvalDate = x.APPROVALDATE,
                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == c.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        atInitiator = staffId== context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval && o.TARGETID==x.ORIGINALDOCUMENTAPPROVALID).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault(),
                        createdBy = x.CREATEDBY,
                        collateralCustomerId = x.COLLATERALCUSTOMERID,

                        //applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                        //customerId = x.COLLATERALCUSTOMERID,
                        //operationId = (int)OperationsEnum.OriginalDocumentApproval,

                    }).OrderBy(o=>o.originalDocumentApprovalId)
               .ToList();
            return data;
        }

        public OriginalDocumentApprovalViewModel GetOriginalDocumentApproval(int id)
        {
           return (from entity in  context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                  join o in context.TBL_COLLATERAL_CUSTOMER on entity.COLLATERALCUSTOMERID equals o.COLLATERALCUSTOMERID
                  where entity.ORIGINALDOCUMENTAPPROVALID == id && entity.DELETED == false
                  select new OriginalDocumentApprovalViewModel
                  {
                    originalDocumentApprovalId = entity.ORIGINALDOCUMENTAPPROVALID,
                    loanApplicationId = entity.LOANAPPLICATIONID,
                    description = entity.DESCRIPTION,
                    collateralCode = o.COLLATERALCODE,
                    collateralType = context.TBL_COLLATERAL_TYPE.Where(a => a.COLLATERALTYPEID == o.COLLATERALTYPEID).Select(o => o.COLLATERALTYPENAME).FirstOrDefault(),
                    collateralTypeId = o.COLLATERALTYPEID,
                    approvalStatusId = entity.APPROVALSTATUSID,
                    applicationReferenceNumber = entity.APPLICATIONREFERNECENUMBER,
                    referenceNumber = entity.REFERENCENUMBER,
                    dateTimeCreated = entity.DATETIMECREATED,
                    approvalDate = entity.APPROVALDATE,
                    collateralCustomerId = entity.COLLATERALCUSTOMERID,

                    //applicationReferenceNumber = entity.APPLICATIONREFERNECENUMBER,
                    customerId = entity.COLLATERALCUSTOMERID,
                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                  }).FirstOrDefault();
        }

        public List<OriginalDocumentApprovalViewModel> GetOriginalDocumentByCollateralCustomerId(int id)
        {
            var entity = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Where(x => x.COLLATERALCUSTOMERID == id && x.DELETED == false)
                .Select(x => new OriginalDocumentApprovalViewModel
                {
                    originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                    loanApplicationId = x.LOANAPPLICATIONID,
                    description = x.DESCRIPTION,
                    approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                    applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                    referenceNumber = x.REFERENCENUMBER,
                    dateTimeCreated = x.DATETIMECREATED,
                    approvalDate = x.APPROVALDATE,
                    approvalStatusId = x.APPROVALSTATUSID,
                    collateralCustomerId = x.COLLATERALCUSTOMERID,

                    //applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                    customerId = x.COLLATERALCUSTOMERID,
                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                }).ToList();

            return entity;
        }

        public List<OriginalDocumentApprovalViewModel> GetReleaseDocumentByCollateralCustomerId(int id)
        {
            List<OriginalDocumentApprovalViewModel> data = new List<OriginalDocumentApprovalViewModel>();

            var collateralcustomerIds = (from oda in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                                         join cc in context.TBL_COLLATERAL_CUSTOMER on oda.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                                         where cc.CUSTOMERID == id
                                         select new OriginalDocumentApprovalViewModel { collateralCustomerId = cc.COLLATERALCUSTOMERID,
                                                                                        originalDocumentApprovalId = oda.ORIGINALDOCUMENTAPPROVALID }
                                       ).ToList();

            if (collateralcustomerIds != null)
            {
                foreach (var ccId in collateralcustomerIds)
                {
                    var entities = (from oda in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                                    join atrail in context.TBL_APPROVAL_TRAIL on oda.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                                    where oda.COLLATERALCUSTOMERID == ccId.collateralCustomerId && oda.ORIGINALDOCUMENTAPPROVALID == ccId.originalDocumentApprovalId
                                    && oda.DELETED == false && atrail.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval
                                    select new OriginalDocumentApprovalViewModel
                                    {
                                      originalDocumentApprovalId = oda.ORIGINALDOCUMENTAPPROVALID,
                                      loanApplicationId = oda.LOANAPPLICATIONID,
                                      description = oda.DESCRIPTION,
                                      approvalStatusName = context.TBL_APPROVAL_STATUS.FirstOrDefault(o => o.APPROVALSTATUSID == oda.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                      //applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                                      //referenceNumber = x.REFERENCENUMBER,
                                      arrivalDate = atrail.ARRIVALDATE,
                                      //dateTimeCreated = x.DATETIMECREATED,
                                      approvalDate = oda.APPROVALDATE,
                                      approvalStatusId = (short)oda.APPROVALSTATUSID,
                                      collateralCustomerId = oda.COLLATERALCUSTOMERID,
                                      //approvedPerson = atrail.RELIEVEDSTAFFID == null ? "n/a" : context.TBL_STAFF.Where(x => x.STAFFID == atrail.RESPONSESTAFFID).Select(s => s.STAFFCODE).FirstOrDefault(),
                                      //responsiblePerson = atrail.RESPONSESTAFFID == null ? "n/a" : context.TBL_STAFF.Where(x => x.STAFFID == atrail.TOSTAFFID).Select( s => s.STAFFCODE).FirstOrDefault(),
                                      //responsiblePerson = atrail.TOSTAFFID == null ? "n/a" : atrail.TBL_STAFF1.STAFFCODE + " - " + atrail.TBL_STAFF1.FIRSTNAME + " " + atrail.TBL_STAFF1.MIDDLENAME + " " + atrail.TBL_STAFF1.LASTNAME,
                                      approvalTrailId = atrail.APPROVALTRAILID,
                                      currentApprovalLevel = atrail.TOAPPROVALLEVELID != null ? context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).LEVELNAME : "n/a",
                                      //customerId = x.COLLATERALCUSTOMERID,
                                      operationId = (int)OperationsEnum.OriginalDocumentApproval,
                                  }).OrderByDescending(e => e.approvalTrailId).ToList();

                    var entity = entities.FirstOrDefault();
                    
                    if (entity != null)
                    {
                        data.Add(entity);
                    }
                }
            }

            return data.OrderByDescending(d => d.approvalTrailId).ToList();
        }

        public List<OriginalDocumentApprovalViewModel> GetOriginalDocument(int id)
        {
            var entity = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Where(x => x.ORIGINALDOCUMENTAPPROVALID == id && x.DELETED == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending)
                .Select(x => new OriginalDocumentApprovalViewModel
                {
                    originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                    
                    loanApplicationId = x.LOANAPPLICATIONID,
                    description = x.DESCRIPTION,
                    approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                    referenceNumber = x.REFERENCENUMBER,
                    dateTimeCreated = x.DATETIMECREATED,
                    approvalDate = x.APPROVALDATE,
                    approvalStatusId = x.APPROVALSTATUSID,

                    applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                    customerId = x.COLLATERALCUSTOMERID,
                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                    collateralCustomerId = x.COLLATERALCUSTOMERID
                }).OrderBy(o=>o.originalDocumentApprovalId).ToList();

            return entity;
        }
        public int AddOriginalDocumentApproval(OriginalDocumentApprovalViewModel model)
        {
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);

            var entity = new TBL_ORIGINAL_DOCUMENT_APPROVAL
            {
                COLLATERALCUSTOMERID = model.collateralCustomerId,
                DESCRIPTION = model.description,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                DATETIMECREATED = general.GetApplicationDate(),
                APPLICATIONREFERNECENUMBER = model.applicationReferenceNumber,
                REFERENCENUMBER = referenceNumber,
                DELETED = false,
                CREATEDBY = model.createdBy,


            };

            context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.OriginalDocumentApprovalAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_Original Document Approval '{entity.DESCRIPTION}' created by {auditStaff}",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
            // Audit Section end ------------------------
            context.SaveChanges();

            return entity.ORIGINALDOCUMENTAPPROVALID;
        }

        public bool UpdateOriginalDocumentApproval(OriginalDocumentApprovalViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Find(id);
            entity.LOANAPPLICATIONID = model.loanApplicationId;
            entity.DESCRIPTION = model.description;
            entity.APPROVALSTATUSID = model.approvalStatusId;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.OriginalDocumentApprovalUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Original Document Approval '{entity.DESCRIPTION}' was updated by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ORIGINALDOCUMENTAPPROVALID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteOriginalDocumentApproval(int id, UserInfo user)
        {
            var entity = this.context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.OriginalDocumentApprovalDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Original Document Approval '{entity.DESCRIPTION}' was deleted by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.ORIGINALDOCUMENTAPPROVALID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<LoanApplicationViewModel> Search(string searchString)
        {
            return (from x in context.TBL_LOAN_APPLICATION
                    join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                    join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                    where x.APPLICATIONREFERENCENUMBER == searchString
                 || c.FIRSTNAME.ToLower().Contains(searchString.Trim())
                 || c.LASTNAME.ToLower().Contains(searchString.Trim())
                 || c.MIDDLENAME.ToLower().Contains(searchString.Trim())

                    select new LoanApplicationViewModel
                    {
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        applicationReferenceNumber = x.APPLICATIONREFERENCENUMBER,
                        loanApplicationId = x.LOANAPPLICATIONID,
                        customerId = c.CUSTOMERID,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        applicationDate = x.APPLICATIONDATE,
                        applicationAmount = x.APPLICATIONAMOUNT,
                        interestRate = x.INTERESTRATE,
                        productName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == a.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),
                        relationshipOfficerId = x.RELATIONSHIPOFFICERID,
                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        relationshipManagerId = x.RELATIONSHIPMANAGERID,
                        relationshipManagerName = context.TBL_STAFF.Where(o => o.STAFFID == x.RELATIONSHIPMANAGERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        operationId = (int)OperationsEnum.OriginalDocumentApproval
                    }).ToList();
        }

        public bool GoForApproval(OriginalDocumentApprovalViewModel entity)
        {
            var document = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Find(entity.originalDocumentApprovalId);
            if (document != null)
            {
                document.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = entity.originalDocumentApprovalId;
                workflow.Comment = "Request for Original document submission approval";
                workflow.OperationId = (int)OperationsEnum.OriginalDocumentApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

            return context.SaveChanges() != 0;


        }

        public WorkflowResponse SubmitApproval(OriginalDocumentApprovalViewModel model)
        {
            bool responce = false;

            try
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = model.approvalStatusId == 3 ? (int)ApprovalStatusEnum.Disapproved : (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.originalDocumentApprovalId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.OriginalDocumentApproval;
                workflow.DeferredExecution = true;
                workflow.LogActivity();

                if (workflow.NewState == (int)ApprovalState.Ended)
                {
                    var document = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Where(o => o.ORIGINALDOCUMENTAPPROVALID == model.originalDocumentApprovalId).FirstOrDefault();
                    if (document != null)
                    {
                        document.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                        document.APPROVALDATE = general.GetApplicationDate();
                    }

                }

                responce = context.SaveChanges() > 0;

                return workflow.Response;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public IEnumerable<OriginalDocumentApprovalViewModel> SearchForApprovedOriginalDocument(string searchString)
        {
            return (from x in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                    join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    join l in context.TBL_CUSTOMER on c.CUSTOMERID equals l.CUSTOMERID
                    where x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved && c.COLLATERALCODE == searchString
                 || l.FIRSTNAME.ToLower().Contains(searchString.Trim())
                 || l.LASTNAME.ToLower().Contains(searchString.Trim())
                 || l.MIDDLENAME.ToLower().Contains(searchString.Trim())
                    select new OriginalDocumentApprovalViewModel
                    {
                        originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                        loanApplicationId = x.LOANAPPLICATIONID,
                        description = x.DESCRIPTION,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                        referenceNumber = x.REFERENCENUMBER,
                        dateTimeCreated = x.DATETIMECREATED,
                        customerName = l.LASTNAME + " " + l.FIRSTNAME + " " + l.MIDDLENAME,
                        customerCode = l.CUSTOMERCODE,
                        customerId = c.CUSTOMERID,
                        collateralValue = c.COLLATERALVALUE,
                        collateralType = context.TBL_COLLATERAL_TYPE.Where(o=>o.COLLATERALTYPEID==c.COLLATERALTYPEID).Select(o=>o.COLLATERALTYPENAME).FirstOrDefault(),
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == l.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == l.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        operationId = (int)OperationsEnum.OriginalDocumentApproval
                    }).ToList(); //applicationReferenceNumber
                                 //exposureValue = context.TBL_LOAN_COLLATERAL_MAPPING.Where(O => O.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).

        }
    }
}

