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
using FintrakBanking.ViewModels.WorkFlow;

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
            var staffs = general.GetStaffRlieved(staffId);

            //var data = new List<OriginalDocumentApprovalViewModel>();
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.OriginalDocumentApproval).ToList();

            var initiator = context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault();

            var dataCollateral = (from x in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                    join o in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals o.COLLATERALCUSTOMERID
                    join c in context.TBL_CUSTOMER on o.CUSTOMERID equals c.CUSTOMERID
                    join atrail in context.TBL_APPROVAL_TRAIL on x.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                    where x.DELETED == false 
                    && (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Finishing)
                     && atrail.RESPONSESTAFFID == null
                      && (ids.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.LOOPEDSTAFFID == null)
                      && (atrail.TOSTAFFID == null || staffs.Contains((int)atrail.TOSTAFFID))
                     && atrail.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval

                    select new OriginalDocumentApprovalViewModel
                    {
                        originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                        loanApplicationId = x.LOANAPPLICATIONID,
                        description = x.DESCRIPTION,
                        collateralCode = o.COLLATERALCODE,
                        collateralType = context.TBL_COLLATERAL_TYPE.Where(a => a.COLLATERALTYPEID == o.COLLATERALTYPEID).Select(o => o.COLLATERALTYPENAME).FirstOrDefault(),
                        collateralTypeId = o.COLLATERALTYPEID,
                        approvalStatusId = (short)x.APPROVALSTATUSID,
                        applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                        referenceNumber = x.REFERENCENUMBER,
                        dateTimeCreated = x.DATETIMECREATED,
                        systemArrivalDateTime = atrail.SYSTEMARRIVALDATETIME,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        customerId = c.CUSTOMERID,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        operationId = atrail.OPERATIONID,
                        approvalDate = x.APPROVALDATE,
                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == c.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        atInitiator = staffId == context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval && o.TARGETID == x.ORIGINALDOCUMENTAPPROVALID).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault(),
                        createdBy = x.CREATEDBY,
                        collateralCustomerId = x.COLLATERALCUSTOMERID,
                        isOriginalTitleDocument = x.ISORIGINALTITLEDOCUMENT,
                        isOriginalTitleDocumentString = x.ISORIGINALTITLEDOCUMENT ? "Yes" : "No",

                        //applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                        //customerId = x.COLLATERALCUSTOMERID,
                        //operationId = (int)OperationsEnum.OriginalDocumentApproval,

                    }).ToList();

            var dataFacility = (from x in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                                join o in context.TBL_LOAN_APPLICATION_DETAIL on x.COLLATERALCUSTOMERID equals o.LOANAPPLICATIONDETAILID
                                join c in context.TBL_CUSTOMER on o.CUSTOMERID equals c.CUSTOMERID
                                join atrail in context.TBL_APPROVAL_TRAIL on x.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                                where x.DELETED == false
                                && (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Finishing)
                                && atrail.RESPONSESTAFFID == null
                                && (ids.Contains((int)atrail.TOAPPROVALLEVELID) && atrail.LOOPEDSTAFFID == null)
                                && (atrail.TOSTAFFID == null || staffs.Contains((int)atrail.TOSTAFFID))
                                && atrail.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval

                                select new OriginalDocumentApprovalViewModel
                                {
                                  businessUnitId = "n/a",
                                  interestRate = o.PROPOSEDINTERESTRATE,
                                  applicationAmount = o.PROPOSEDAMOUNT,
                                  productName = context.TBL_PRODUCT.Where(p => p.PRODUCTID == o.PROPOSEDPRODUCTID).Select(p => p.PRODUCTNAME).FirstOrDefault(),
                                  originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                                  loanApplicationId = x.LOANAPPLICATIONID,
                                  description = x.DESCRIPTION,
                                  collateralCode = "",
                                  collateralType = "",
                                  collateralTypeId = 0,
                                  approvalStatusId = (short)x.APPROVALSTATUSID,
                                  applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                                  referenceNumber = x.REFERENCENUMBER,
                                  dateTimeCreated = x.DATETIMECREATED,
                                  systemArrivalDateTime = atrail.SYSTEMARRIVALDATETIME,
                                  approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                                  customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                  customerCode = c.CUSTOMERCODE,
                                  customerId = c.CUSTOMERID,
                                  branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                                  operationId = atrail.OPERATIONID,
                                  approvalDate = x.APPROVALDATE,
                                  relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == c.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                                  atInitiator = staffId == context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval && o.TARGETID == x.ORIGINALDOCUMENTAPPROVALID).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault(),
                                  createdBy = x.CREATEDBY,
                                  collateralCustomerId = x.COLLATERALCUSTOMERID,
                                  isOriginalTitleDocument = x.ISORIGINALTITLEDOCUMENT,
                                  isOriginalTitleDocumentString = x.ISORIGINALTITLEDOCUMENT ? "Yes" : "No",
                              }).ToList();

             var data = dataCollateral.Union(dataFacility);

            return data.OrderBy(o => o.originalDocumentApprovalId).ToList();
        }

        public IEnumerable<OriginalDocumentApprovalViewModel> GetOriginalDocumentSearch(string searchString)
        {
            searchString = searchString.ToLower().Trim();
            
           var dataCollateral = (from x in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                    join o in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals o.COLLATERALCUSTOMERID
                    join c in context.TBL_CUSTOMER on o.CUSTOMERID equals c.CUSTOMERID
                    join atrail in context.TBL_APPROVAL_TRAIL on x.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                    where x.DELETED == false 
                    && atrail.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval
                    && (c.CUSTOMERCODE == searchString
                    || x.REFERENCENUMBER.ToLower().Contains(searchString)
                    || c.FIRSTNAME.ToLower().Contains(searchString)
                    || c.MIDDLENAME.ToLower().Contains(searchString)
                    || c.LASTNAME.ToLower().Contains(searchString))

                    select new OriginalDocumentApprovalViewModel
                    {
                        approvalTrailId = atrail.APPROVALTRAILID,
                        originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                        loanApplicationId = x.LOANAPPLICATIONID,
                        description = x.DESCRIPTION,
                        collateralCode = o.COLLATERALCODE,
                        collateralType = context.TBL_COLLATERAL_TYPE.Where(a => a.COLLATERALTYPEID == o.COLLATERALTYPEID).Select(o => o.COLLATERALTYPENAME).FirstOrDefault(),
                        collateralTypeId = o.COLLATERALTYPEID,
                        approvalStatusId = (short)x.APPROVALSTATUSID,
                        applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                        referenceNumber = x.REFERENCENUMBER,
                        dateTimeCreated = x.DATETIMECREATED,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault().ToUpper(),
                        customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                        customerCode = c.CUSTOMERCODE,
                        customerId = c.CUSTOMERID,
                        branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                        operationId = atrail.OPERATIONID,
                        approvalDate = x.APPROVALDATE,
                        isOriginalTitleDocumentString = x.ISORIGINALTITLEDOCUMENT == true ? "Yes" : "No",
                        relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == c.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        createdBy = x.CREATEDBY,
                        createdByName = context.TBL_STAFF.Where(o => o.STAFFID == x.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                        collateralCustomerId = x.COLLATERALCUSTOMERID,
                        currentApprovalLevel = atrail.TOAPPROVALLEVELID != null ? ((atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && atrail.LOOPEDSTAFFID != null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == atrail.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).LEVELNAME) : "N/A",
                        responsiblePerson = atrail.TOSTAFFID == null ? "N/A" : atrail.TBL_STAFF1.STAFFCODE + " - " + atrail.TBL_STAFF1.FIRSTNAME + " " + atrail.TBL_STAFF1.MIDDLENAME + " " + atrail.TBL_STAFF1.LASTNAME,
                    }).ToList();

            var dataFacility = (from x in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                        join o in context.TBL_LOAN_APPLICATION_DETAIL on x.COLLATERALCUSTOMERID equals o.LOANAPPLICATIONDETAILID
                        join c in context.TBL_CUSTOMER on o.CUSTOMERID equals c.CUSTOMERID
                        join atrail in context.TBL_APPROVAL_TRAIL on x.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                        where x.DELETED == false
                        && atrail.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval
                        && (c.CUSTOMERCODE == searchString
                        || x.REFERENCENUMBER.ToLower().Contains(searchString)
                        || c.FIRSTNAME.ToLower().Contains(searchString)
                        || c.MIDDLENAME.ToLower().Contains(searchString)
                        || c.LASTNAME.ToLower().Contains(searchString))

                        select new OriginalDocumentApprovalViewModel
                        {
                            approvalTrailId = atrail.APPROVALTRAILID,
                            originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                            loanApplicationId = x.LOANAPPLICATIONID,
                            description = x.DESCRIPTION,
                            collateralCode = "",
                            collateralType = "",
                            collateralTypeId = 0,
                            approvalStatusId = (short)x.APPROVALSTATUSID,
                            applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                            referenceNumber = x.REFERENCENUMBER,
                            dateTimeCreated = x.DATETIMECREATED,
                            approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault().ToUpper(),
                            customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                            customerCode = c.CUSTOMERCODE,
                            customerId = c.CUSTOMERID,
                            branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                            operationId = atrail.OPERATIONID,
                            approvalDate = x.APPROVALDATE,
                            isOriginalTitleDocumentString = x.ISORIGINALTITLEDOCUMENT == true ? "Yes" : "No",
                            relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == c.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                            createdBy = x.CREATEDBY,
                            createdByName = context.TBL_STAFF.Where(o => o.STAFFID == x.CREATEDBY).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                            collateralCustomerId = x.COLLATERALCUSTOMERID,
                            currentApprovalLevel = atrail.TOAPPROVALLEVELID != null ? ((atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred && atrail.LOOPEDSTAFFID != null) ? context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == atrail.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).LEVELNAME) : "N/A",
                            responsiblePerson = atrail.TOSTAFFID == null ? "N/A" : atrail.TBL_STAFF1.STAFFCODE + " - " + atrail.TBL_STAFF1.FIRSTNAME + " " + atrail.TBL_STAFF1.MIDDLENAME + " " + atrail.TBL_STAFF1.LASTNAME,
                        }).ToList();

            var data = dataCollateral.Union(dataFacility);
            var result = data.GroupBy(r => r.originalDocumentApprovalId)
                               .Select(p => p.OrderByDescending(r => r.approvalTrailId).FirstOrDefault()).ToList();

            return result;
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
                    approvalStatusId = (short)entity.APPROVALSTATUSID,
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
                    approvalStatusId = (short)x.APPROVALSTATUSID,
                    collateralCustomerId = x.COLLATERALCUSTOMERID,

                    //applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                    customerId = x.COLLATERALCUSTOMERID,
                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                }).ToList();

            return entity;
        }

        public List<OriginalDocumentApprovalViewModel> GetApprovedOriginalDocumentByCollateralCustomerId(int id)
        {
            var entity = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Where(x => x.COLLATERALCUSTOMERID == id && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved && x.DELETED == false)
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
                    approvalStatusId = (short)x.APPROVALSTATUSID,
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
                                         select new OriginalDocumentApprovalViewModel {
                                             collateralCustomerId = cc.COLLATERALCUSTOMERID,
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



        public List<OriginalDocumentApprovalViewModel> GetReleaseDocumentByCustomerFacilityId(int id)
        {
            List<OriginalDocumentApprovalViewModel> data = new List<OriginalDocumentApprovalViewModel>();

            var collateralcustomerIds = (from oda in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                                         join ad in context.TBL_LOAN_APPLICATION_DETAIL on oda.COLLATERALCUSTOMERID equals ad.LOANAPPLICATIONDETAILID
                                         where ad.CUSTOMERID == id && oda.DELETED == false
                                         select new OriginalDocumentApprovalViewModel
                                         {
                                             collateralCustomerId = oda.COLLATERALCUSTOMERID,
                                             originalDocumentApprovalId = oda.ORIGINALDOCUMENTAPPROVALID
                                         }
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
                                        applicationReferenceNumber = oda.APPLICATIONREFERNECENUMBER,
                                        referenceNumber = oda.REFERENCENUMBER,
                                        arrivalDate = atrail.ARRIVALDATE,
                                        approvalDate = oda.APPROVALDATE,
                                        approvalStatusId = (short)oda.APPROVALSTATUSID,
                                        collateralCustomerId = oda.COLLATERALCUSTOMERID,
                                        approvalTrailId = atrail.APPROVALTRAILID,
                                        currentApprovalLevel = atrail.TOAPPROVALLEVELID != null ? context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).LEVELNAME : "n/a",
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
            var entity = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Where(x => x.ORIGINALDOCUMENTAPPROVALID == id && x.DELETED == false && (x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing))
                .Select(x => new OriginalDocumentApprovalViewModel
                {
                    originalDocumentApprovalId = x.ORIGINALDOCUMENTAPPROVALID,
                    
                    loanApplicationId = x.LOANAPPLICATIONID,
                    description = x.DESCRIPTION,
                    isOriginalTitleDocument = x.ISORIGINALTITLEDOCUMENT,
                    approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                    referenceNumber = x.REFERENCENUMBER,
                    dateTimeCreated = x.DATETIMECREATED,
                    approvalDate = x.APPROVALDATE,
                    approvalStatusId = (short)x.APPROVALSTATUSID,

                    applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                    customerId = x.COLLATERALCUSTOMERID,
                    operationId = (int)OperationsEnum.OriginalDocumentApproval,
                    collateralCustomerId = x.COLLATERALCUSTOMERID
                }).OrderBy(o=>o.originalDocumentApprovalId).ToList();

            return entity;
        }

        public List<OriginalDocumentApprovalViewModel> GetDocumentUploadList(int staffId)
         {
            var ids = general.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.OriginalDocumentApproval).ToList();
            var staffs = general.GetStaffRlieved(staffId);

            //var initiator = context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault();

            var model1 = (from oda in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                          join cc in context.TBL_COLLATERAL_CUSTOMER on oda.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                          join c in context.TBL_CUSTOMER on cc.CUSTOMERID equals c.CUSTOMERID
                          join atrail in context.TBL_APPROVAL_TRAIL on oda.ORIGINALDOCUMENTAPPROVALID equals atrail.TARGETID
                          where oda.DELETED == false
                              && atrail.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval
                              && atrail.TARGETID == oda.ORIGINALDOCUMENTAPPROVALID
                          orderby atrail.APPROVALTRAILID descending
                          select new OriginalDocumentApprovalViewModel
                          {
                              loopedStaffId = atrail.LOOPEDSTAFFID,
                              originalDocumentApprovalId = oda.ORIGINALDOCUMENTAPPROVALID,
                              description = oda.DESCRIPTION,
                              isOriginalTitleDocument = oda.ISORIGINALTITLEDOCUMENT,
                              loanApplicationId = oda.LOANAPPLICATIONID,
                              collateralType = context.TBL_COLLATERAL_TYPE.Where(a => a.COLLATERALTYPEID == cc.COLLATERALTYPEID).Select(o => o.COLLATERALTYPENAME).FirstOrDefault(),
                              collateralTypeId = cc.COLLATERALTYPEID,
                              customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                              collateralCode = cc.COLLATERALCODE,
                              referenceNumber = oda.REFERENCENUMBER,
                              applicationReferenceNumber = oda.APPLICATIONREFERNECENUMBER,
                              dateTimeCreated = oda.DATETIMECREATED,
                              collateralCustomerId = cc.COLLATERALCUSTOMERID,
                              customerId = c.CUSTOMERID,
                              businessUnit = c.BUSINESSUNTID,
                              branchName = context.TBL_BRANCH.Where(o => o.BRANCHID == c.BRANCHID).Select(o => o.BRANCHNAME).FirstOrDefault(),
                              operationId = (int)OperationsEnum.OriginalDocumentApproval,
                              approvalStatusId = atrail.APPROVALSTATUSID,
                              approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(s => s.APPROVALSTATUSNAME).FirstOrDefault(),
                              relationshipOfficerName = context.TBL_STAFF.Where(o => o.STAFFID == c.RELATIONSHIPOFFICERID).Select(o => o.FIRSTNAME + " " + o.MIDDLENAME + " " + o.LASTNAME).FirstOrDefault(),
                              atInitiator = staffId == context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.OriginalDocumentApproval && o.TARGETID == oda.ORIGINALDOCUMENTAPPROVALID).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault(),


                          }).ToList().GroupBy(x => x.originalDocumentApprovalId).Select(x => x.FirstOrDefault()).Where((x => (x.approvalStatusId == (short)ApprovalStatusEnum.Referred
                                              && staffs.Contains(x.loopedStaffId ?? 0)) || x.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)).ToList();
                                              //&& x.loopedStaffId == initiator) || x.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)).ToList();

            //var modelGroup = model1.GroupBy(x => x.originalDocumentApprovalId).Select(x => x.FirstOrDefault()).ToList();

            var model2 = (from oda in context.TBL_ORIGINAL_DOCUMENT_APPROVAL
                          join cc in context.TBL_COLLATERAL_CUSTOMER on oda.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                          join c in context.TBL_CUSTOMER on cc.CUSTOMERID equals c.CUSTOMERID
                          where oda.DELETED == false && oda.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending

                          select new OriginalDocumentApprovalViewModel
                          {
                              originalDocumentApprovalId = oda.ORIGINALDOCUMENTAPPROVALID,
                              description = oda.DESCRIPTION,
                              isOriginalTitleDocument = oda.ISORIGINALTITLEDOCUMENT,
                              customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                              collateralCode = cc.COLLATERALCODE,
                              referenceNumber = oda.REFERENCENUMBER,
                              dateTimeCreated = oda.DATETIMECREATED,
                              collateralCustomerId = cc.COLLATERALCUSTOMERID,
                              customerId = c.CUSTOMERID,
                              operationId = (int)OperationsEnum.OriginalDocumentApproval,
                              approvalStatusId = (short)oda.APPROVALSTATUSID,
                              approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == oda.APPROVALSTATUSID).Select(s => s.APPROVALSTATUSNAME).FirstOrDefault(),


                          }).OrderByDescending(o => o.originalDocumentApprovalId).ToList();

           

            var model = model2.Union(model1).ToList();

            return model;
        }

        public int updateOriginalDocumentApproval(OriginalDocumentApprovalViewModel model)
        {
            var entity = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Find(model.originalDocumentApprovalId);
            if(entity != null)
            {
                entity.DESCRIPTION = model.description;
                entity.ISORIGINALTITLEDOCUMENT = model.isOriginalTitleDocument;
                entity.DATETIMEUPDATED = general.GetApplicationDate();
                entity.LASTUPDATEDBY = model.createdBy;
                entity.APPROVALSTATUSID = model.approvalStatusId == (short)ApprovalStatusEnum.Referred ? (short)ApprovalStatusEnum.Processing : model.approvalStatusId;

                var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
                // Audit Section ---------------------------
                this.audit.AddAuditTrail(new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.OriginalDocumentApprovalUpdated,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"TBL_Original Document Approval '{entity.DESCRIPTION}' updated by {auditStaff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                });
                // Audit Section end ------------------------
            }

            if(context.SaveChanges() > 0) return entity.ORIGINALDOCUMENTAPPROVALID;

            return 0;

        }
        public int AddOriginalDocumentApproval(OriginalDocumentApprovalViewModel model)
        {
            var search = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Find(model.originalDocumentApprovalId);

            if (search != null)  return updateOriginalDocumentApproval(model);
            else
            {
                var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);

                var entity = new TBL_ORIGINAL_DOCUMENT_APPROVAL
                {
                    COLLATERALCUSTOMERID = model.collateralCustomerId,
                    DESCRIPTION = model.description,
                    ISORIGINALTITLEDOCUMENT = model.isOriginalTitleDocument,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                    DATETIMECREATED = general.GetApplicationDate(),
                    APPLICATIONREFERNECENUMBER = model.applicationReferenceNumber,
                    REFERENCENUMBER = referenceNumber,
                    LOANAPPLICATIONID = model.loanApplicationId,
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
        }

        public bool UpdateOriginalDocumentApproval(OriginalDocumentApprovalViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Find(id);
            entity.LOANAPPLICATIONID = model.loanApplicationId;
            entity.DESCRIPTION = model.description;
            entity.ISORIGINALTITLEDOCUMENT = model.isOriginalTitleDocument;
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

        public WorkflowResponse GoForApproval(OriginalDocumentApprovalViewModel entity , short? approvalStatusId)
        {

            var document = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Find(entity.originalDocumentApprovalId);
            if (document != null)
            {
                if(approvalStatusId != (short)ApprovalStatusEnum.Referred)
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
                    workflow.LevelBusinessRule = new LevelBusinessRule
                    {
                        excludeLevel = !document.ISORIGINALTITLEDOCUMENT
                    };
                    workflow.LogActivity();
                }

                else if (approvalStatusId == (short)ApprovalStatusEnum.Referred)
                {

                    workflow.StaffId = entity.createdBy;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = entity.approvalStatusId == 3 ? (int)ApprovalStatusEnum.Disapproved : (int)ApprovalStatusEnum.Processing;
                    workflow.TargetId = entity.originalDocumentApprovalId;
                    workflow.Comment = entity.comment;
                    workflow.OperationId = (int)OperationsEnum.OriginalDocumentApproval;
                    workflow.DeferredExecution = true;
                    workflow.LogActivity();
                }

            }

            var saved = context.SaveChanges() > 0;
            return workflow.Response;


        }

        public WorkflowResponse SubmitApproval(OriginalDocumentApprovalViewModel model)
        {
            bool responce = false;

            try
            {
                var document = context.TBL_ORIGINAL_DOCUMENT_APPROVAL.Where(o => o.ORIGINALDOCUMENTAPPROVALID == model.originalDocumentApprovalId).FirstOrDefault();
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = model.approvalStatusId == 3 ? (int)ApprovalStatusEnum.Disapproved : (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.originalDocumentApprovalId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.OriginalDocumentApproval;
                workflow.DeferredExecution = true;
                workflow.LevelBusinessRule = new LevelBusinessRule
                {
                    excludeLevel = !(document?.ISORIGINALTITLEDOCUMENT ?? false)
                };
                workflow.LogActivity();

                if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                {
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
                        isOriginalTitleDocument = x.ISORIGINALTITLEDOCUMENT,
                        approvalStatusName = context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == x.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        applicationReferenceNumber = x.APPLICATIONREFERNECENUMBER,
                        referenceNumber = x.REFERENCENUMBER,
                        dateTimeCreated = x.DATETIMECREATED,
                        customerName = l.LASTNAME + " " + l.FIRSTNAME + " " + l.MIDDLENAME,
                        customerCode = l.CUSTOMERCODE,
                        customerId = c.CUSTOMERID.Value,
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

