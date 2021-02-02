using System;
using System.Collections.Generic;
using System.Linq;

using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Common;
using FintrakBanking.ViewModels.Setups.General;

namespace FintrakBanking.Repositories.Credit
{
    public class LetterGenerationRequestRepository : ILetterGenerationRequestRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;

        public LetterGenerationRequestRepository(
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

        public IEnumerable<AuthorisedSignatoryViewModel> GetLetterGenerationSignatory(int requestId)
        {
            var signatories = (from a in context.TBL_AUTHORISED_SIGNATORY
                               join b in context.TBL_OPERATION_SIGNATORY on a.SIGNATORYID equals b.SIGNATORYID
                               where
                               (
                               a.DELETED == false && b.DELETED == false
                               && b.TARGETID == requestId && b.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest
                               )
                               select new AuthorisedSignatoryViewModel
                               {
                                   signatoryId = a.SIGNATORYID,
                                   signatoryName = a.SIGNATORYNAME,
                                   signatoryInitials = a.SIGNATORYINITIALS,
                                   signatoryTitle = a.SIGNATORYTITLE,
                               }).ToList();
            return signatories;
        }

        public IEnumerable<LetterGenerationRequestViewModel> GetLetterGenerationRequests(int staffId)
        {
            var requestsInProgress = (from x in context.TBL_LETTER_GENERATION_REQUEST
                                      join t in context.TBL_APPROVAL_TRAIL on x.LETTERGENERATIONREQUESTID equals t.TARGETID where 
                                      (
                                      x.DELETED == false 
                                      && t.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest
                                      && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LetterGenerationRequestInProgress
                                      )
                                      select new LetterGenerationRequestViewModel()
                                      {
                                          requestId = x.LETTERGENERATIONREQUESTID,
                                          customerId = x.CUSTOMERID,
                                          customerCode = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == x.CUSTOMERID).FirstOrDefault().CUSTOMERCODE,
                                          requestDate = x.REQUESTDATE,
                                          requestType = x.REQUESTTYPE,
                                          asAtDate = x.ASATDATE,
                                          comment = x.COMMENTS,
                                          customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.LASTNAME,
                                          dateTimeCreated = x.DATETIMECREATED,
                                          approvalStatus = t.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                                          approvalStatusId = t.APPROVALSTATUSID,
                                          approvalTrailId = t.APPROVALTRAILID,
                                          loopedStaffId = (int)t.LOOPEDSTAFFID,
                                          requestRef = x.REQUESTREF,
                                          loanBalance = x.LOANBALANCE,
                                          letterGenerationsignatories = (from a in context.TBL_AUTHORISED_SIGNATORY
                                                                         join y in context.TBL_OPERATION_SIGNATORY on a.SIGNATORYID equals y.SIGNATORYID
                                                                         where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest && x.LETTERGENERATIONREQUESTID == y.TARGETID
                                                                         select new AuthorisedSignatoryViewModel()
                                                                         {
                                                                            signatoryId = y.SIGNATORYID,
                                                                            signatoryName = a.SIGNATORYNAME,
                                                                            signatoryInitials = a.SIGNATORYINITIALS,
                                                                            signatoryTitle = a.SIGNATORYTITLE,
                                                                         }).ToList(),
                                          letterGenerationCamsolList = (from a in context.TBL_LOAN_CAMSOL
                                                                    join y in context.TBL_OPERATION_CAMSOL_LIST on a.LOAN_CAMSOLID equals y.LOAN_CAMSOLID
                                                                    join z in context.TBL_LOAN_CAMSOL_TYPE on a.CAMSOLTYPEID equals z.CAMSOLTYPEID
                                                                    where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest && x.LETTERGENERATIONREQUESTID == y.TARGETID
                                                                    select new CamsolLoanDocumentViewModel()
                                                                    {
                                                                        camsolId = a.LOAN_CAMSOLID,
                                                                        customerCode = a.CUSTOMERCODE,
                                                                        customerName = a.CUSTOMERNAME,
                                                                        accountNumber = a.ACCOUNTNUMBER,
                                                                        balance = a.BALANCE,
                                                                        camsolTypeName = z.CAMSOLTYPENAME
                                                                    }).ToList(),
                                      }).GroupBy(l => l.requestId).Select(l => l.OrderByDescending(t => t.approvalTrailId).FirstOrDefault())
                                        .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                        || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                        && l.loopedStaffId == staffId)).ToList();

            var requestsNotStarted = (from x in context.TBL_LETTER_GENERATION_REQUEST
                                      where 
                                      (
                                      x.DELETED == false && x.APPLICATIONSTATUSID == null
                                      )
                                      select new LetterGenerationRequestViewModel()
                                      {
                                          requestId = x.LETTERGENERATIONREQUESTID,
                                          customerId = x.CUSTOMERID,
                                          customerCode = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == x.CUSTOMERID).FirstOrDefault().CUSTOMERCODE,
                                          requestDate = x.REQUESTDATE,
                                          requestType = x.REQUESTTYPE,
                                          asAtDate = x.ASATDATE,
                                          comment = x.COMMENTS,
                                          customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.LASTNAME,
                                          dateTimeCreated = x.DATETIMECREATED,
                                          requestRef = x.REQUESTREF,
                                          loanBalance = x.LOANBALANCE,
                                          letterGenerationsignatories = (from a in context.TBL_AUTHORISED_SIGNATORY
                                                                         join y in context.TBL_OPERATION_SIGNATORY on a.SIGNATORYID equals y.SIGNATORYID
                                                                         where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest && x.LETTERGENERATIONREQUESTID == y.TARGETID
                                                                         select new AuthorisedSignatoryViewModel()
                                                                         {
                                                                             signatoryId = y.SIGNATORYID,
                                                                             signatoryName = a.SIGNATORYNAME,
                                                                             signatoryInitials = a.SIGNATORYINITIALS,
                                                                             signatoryTitle = a.SIGNATORYTITLE,
                                                                         }).ToList(),
                                          letterGenerationCamsolList = (from a in context.TBL_LOAN_CAMSOL
                                                                    join y in context.TBL_OPERATION_CAMSOL_LIST on a.LOAN_CAMSOLID equals y.LOAN_CAMSOLID
                                                                    join z in context.TBL_LOAN_CAMSOL_TYPE on a.CAMSOLTYPEID equals z.CAMSOLTYPEID
                                                                    where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest && x.LETTERGENERATIONREQUESTID == y.TARGETID
                                                                    select new CamsolLoanDocumentViewModel()
                                                                    {
                                                                        camsolId = a.LOAN_CAMSOLID,
                                                                        customerCode = a.CUSTOMERCODE,
                                                                        customerName = a.CUSTOMERNAME,
                                                                        accountNumber = a.ACCOUNTNUMBER,
                                                                        balance = a.BALANCE,
                                                                        camsolTypeName = z.CAMSOLTYPENAME
                                                                    }).ToList(),
                                      }).ToList().OrderByDescending(r => r.dateTimeCreated);
            var requests = requestsNotStarted.Union(requestsInProgress);
            return requests;
        }

        public IEnumerable<LetterGenerationRequestViewModel> GetLetterGenerationCompleted()
        {
            return context.TBL_LETTER_GENERATION_REQUEST.Where(x => x.DELETED == false
                                    && x.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LetterGenerationRequestCompleted)
                .Select(x => new LetterGenerationRequestViewModel
                {
                    requestId = x.LETTERGENERATIONREQUESTID,
                    customerCode = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == x.CUSTOMERID).FirstOrDefault().CUSTOMERCODE,
                    customerId = x.CUSTOMERID,
                    loanBalance = x.LOANBALANCE,
                    requestDate = x.REQUESTDATE,
                    requestType = x.REQUESTTYPE,
                    asAtDate = x.ASATDATE,
                    comment = x.COMMENTS,
                    customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.LASTNAME,
                    dateTimeCreated = x.DATETIMECREATED,
                    requestRef = x.REQUESTREF,
                    letterGenerationsignatories = (from a in context.TBL_AUTHORISED_SIGNATORY
                                                   join y in context.TBL_OPERATION_SIGNATORY on a.SIGNATORYID equals y.SIGNATORYID
                                                   where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest && x.LETTERGENERATIONREQUESTID == y.TARGETID
                                                   select new AuthorisedSignatoryViewModel()
                                                   {
                                                       signatoryId = y.SIGNATORYID,
                                                       signatoryName = a.SIGNATORYNAME,
                                                       signatoryInitials = a.SIGNATORYINITIALS,
                                                       signatoryTitle = a.SIGNATORYTITLE,
                                                   }).ToList(),
                    letterGenerationCamsolList = (from a in context.TBL_LOAN_CAMSOL
                                              join y in context.TBL_OPERATION_CAMSOL_LIST on a.LOAN_CAMSOLID equals y.LOAN_CAMSOLID
                                              join z in context.TBL_LOAN_CAMSOL_TYPE on a.CAMSOLTYPEID equals z.CAMSOLTYPEID
                                              where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest && x.LETTERGENERATIONREQUESTID == y.TARGETID
                                              select new CamsolLoanDocumentViewModel()
                                              {
                                                  camsolId = a.LOAN_CAMSOLID,
                                                  customerCode = a.CUSTOMERCODE,
                                                  customerName = a.CUSTOMERNAME,
                                                  accountNumber = a.ACCOUNTNUMBER,
                                                  balance = a.BALANCE,
                                                  camsolTypeName = z.CAMSOLTYPENAME
                                              }).ToList(),
                })
                .ToList().OrderByDescending(r => r.dateTimeCreated);
        }

        public IEnumerable<LetterGenerationRequestViewModel> GetLetterGenerationRequestsForApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.LetterGenerationRequest;
            IQueryable<LetterGenerationRequestViewModel> applications = null;
            var levelIds = general.GetStaffApprovalLevelIds(staffId, operationId).ToList();

            //var querytest1 = (from a in context.TBL_LC_ISSUANCE
            //                  where
            //                    a.DELETED == false
            //                    && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LcShippingReleaseInProgress
            //                  select a).ToList();

            //var querytest2 = (from b in context.TBL_APPROVAL_TRAIL
            //                  where
            //                    (b.OPERATIONID == operationId)
            //                    && b.APPROVALSTATEID != (int)ApprovalState.Ended
            //                    && b.RESPONSESTAFFID == null
            //                    && levelIds.Contains((int)b.TOAPPROVALLEVELID)
            //                    && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
            //                  select b).ToList();
            // query
            var query = (from l in context.TBL_LETTER_GENERATION_REQUEST
                         where
                            (l.DELETED == false
                            //&& l.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LetterGenerationRequestInProgress
                            )
                         orderby l.DATEACTEDON
                         join b in context.TBL_APPROVAL_TRAIL on l.LETTERGENERATIONREQUESTID equals b.TARGETID
                         where
                            (
                            (b.OPERATIONID == operationId)
                            && b.APPROVALSTATEID != (int)ApprovalState.Ended
                            && b.RESPONSESTAFFID == null
                            && b.LOOPEDSTAFFID == null
                            && levelIds.Contains((int)b.TOAPPROVALLEVELID)
                            && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
                            )
                         select new LetterGenerationRequestViewModel()
                         {
                             requestId = l.LETTERGENERATIONREQUESTID,
                             requestDate = l.REQUESTDATE,
                             requestType = l.REQUESTTYPE,
                             approvalStatus = b.TBL_APPROVAL_STATUS.APPROVALSTATUSNAME,
                             asAtDate = l.ASATDATE,
                             comment = l.COMMENTS,
                             customerId = l.CUSTOMERID,
                             customerName = l.TBL_CUSTOMER.FIRSTNAME + l.TBL_CUSTOMER.LASTNAME,
                             lastComment = b.COMMENT,
                             currentApprovalStateId = b.APPROVALSTATEID,
                             currentApprovalLevelId = b.TOAPPROVALLEVELID,
                             currentApprovalLevel = b.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                             currentApprovalLevelTypeId = b.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                             approvalTrailId = b == null ? 0 : b.APPROVALTRAILID, // for inner sequence ordering
                             toStaffId = b.TOSTAFFID,
                             approvalStatusId = b.APPROVALSTATUSID,
                             applicationStatusId = l.APPLICATIONSTATUSID,
                             createdBy = (int)l.CREATEDBY,
                             operationId = operationId,
                             systemArrivalDateTime = b.SYSTEMARRIVALDATETIME,
                             dateTimeCreated = (DateTime)l.DATEACTEDON,
                             customerCode = l.TBL_CUSTOMER.CUSTOMERCODE,
                             requestRef = l.REQUESTREF,
                             loanBalance = l.LOANBALANCE,
                             letterGenerationsignatories = (from a in context.TBL_AUTHORISED_SIGNATORY
                                                            join y in context.TBL_OPERATION_SIGNATORY on a.SIGNATORYID equals y.SIGNATORYID
                                                            where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest && l.LETTERGENERATIONREQUESTID == y.TARGETID
                                                            select new AuthorisedSignatoryViewModel()
                                                            {
                                                                signatoryId = y.SIGNATORYID,
                                                                signatoryName = a.SIGNATORYNAME,
                                                                signatoryInitials = a.SIGNATORYINITIALS,
                                                                signatoryTitle = a.SIGNATORYTITLE,
                                                            }).ToList(),
                             letterGenerationCamsolList = (from a in context.TBL_LOAN_CAMSOL
                                                           join y in context.TBL_OPERATION_CAMSOL_LIST on a.LOAN_CAMSOLID equals y.LOAN_CAMSOLID
                                                           join z in context.TBL_LOAN_CAMSOL_TYPE on a.CAMSOLTYPEID equals z.CAMSOLTYPEID
                                                           where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest && l.LETTERGENERATIONREQUESTID == y.TARGETID
                                                           select new CamsolLoanDocumentViewModel()
                                                           {
                                                               camsolId = a.LOAN_CAMSOLID,
                                                               customerCode = a.CUSTOMERCODE,
                                                               customerName = a.CUSTOMERNAME,
                                                               accountNumber = a.ACCOUNTNUMBER,
                                                               balance = a.BALANCE,
                                                               camsolTypeName = z.CAMSOLTYPENAME
                                                           }).ToList(),
                             //accountNumber = context.TBL_CASA.Where(O => O.CUSTOMERID == a.CUSTOMERID).Select(O => O.OLDPRODUCTACCOUNTNUMBER1).FirstOrDefault(),
                         }).ToList();

            applications = query.AsQueryable()
                .Where(x => x.currentApprovalLevelTypeId != 2)
                .GroupBy(d => d.requestId)
                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault());

            return applications.ToList();
        }


        public LetterGenerationRequestViewModel GetLetterGenerationRequest(int id)
        {
            var entity = context.TBL_LETTER_GENERATION_REQUEST.FirstOrDefault(x => x.LETTERGENERATIONREQUESTID == id && x.DELETED == false);

            return new LetterGenerationRequestViewModel
            {
                requestId = entity.LETTERGENERATIONREQUESTID,
                customerId = entity.CUSTOMERID,
                customerCode = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == entity.CUSTOMERID).FirstOrDefault().CUSTOMERCODE,
                requestDate = entity.REQUESTDATE,
                requestType = entity.REQUESTTYPE,
                asAtDate = entity.ASATDATE,
                comment = entity.COMMENTS,
                customerName = entity.TBL_CUSTOMER.FIRSTNAME + entity.TBL_CUSTOMER.LASTNAME,
                requestRef = entity.REQUESTREF,
                loanBalance = entity.LOANBALANCE,
                letterGenerationsignatories = (from a in context.TBL_AUTHORISED_SIGNATORY
                                               join y in context.TBL_OPERATION_SIGNATORY on a.SIGNATORYID equals y.SIGNATORYID
                                               where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest && entity.LETTERGENERATIONREQUESTID == y.TARGETID
                                               select new AuthorisedSignatoryViewModel()
                                               {
                                                   signatoryId = y.SIGNATORYID,
                                                   signatoryName = a.SIGNATORYNAME,
                                                   signatoryInitials = a.SIGNATORYINITIALS,
                                                   signatoryTitle = a.SIGNATORYTITLE,
                                               }).ToList(),
                letterGenerationCamsolList = (from a in context.TBL_LOAN_CAMSOL
                                          join y in context.TBL_OPERATION_CAMSOL_LIST on a.LOAN_CAMSOLID equals y.LOAN_CAMSOLID
                                          join z in context.TBL_LOAN_CAMSOL_TYPE on a.CAMSOLTYPEID equals z.CAMSOLTYPEID
                                          where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest && entity.LETTERGENERATIONREQUESTID == y.TARGETID
                                          select new CamsolLoanDocumentViewModel()
                                          {
                                              camsolId = a.LOAN_CAMSOLID,
                                              customerCode = a.CUSTOMERCODE,
                                              customerName = a.CUSTOMERNAME,
                                              accountNumber = a.ACCOUNTNUMBER,
                                              balance = a.BALANCE,
                                              camsolTypeName = z.CAMSOLTYPENAME
                                          }).ToList(),
            };
        }

        public LetterGenerationRequestViewModel AddLetterGenerationRequest(LetterGenerationRequestViewModel model)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                //var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
                String referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
                var entity = new TBL_LETTER_GENERATION_REQUEST
                {
                    CUSTOMERID = model.customerId,
                    REQUESTDATE = model.requestDate,
                    REQUESTTYPE = model.requestType,
                    ASATDATE = model.asAtDate,
                    COMMENTS = model.comment,
                    REQUESTREF = referenceNumber,
                    LOANBALANCE = model.loanBalance,
                    // COMPANYID = model.companyId,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = DateTime.Now,
                };

                context.TBL_LETTER_GENERATION_REQUEST.Add(entity);



                var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
                // Audit Section ---------------------------
                var aud = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LetterGenerationRequestAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"TBL_LETTER_GENERATION_REQUEST '{entity.ToString()}' created by {auditStaff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };
                context.TBL_AUDIT.Add(aud);
                // Audit Section end ------------------------

                context.SaveChanges();
                var req = context.TBL_LETTER_GENERATION_REQUEST.Where(r => r.REQUESTREF == referenceNumber).FirstOrDefault();
                model.requestId = req.LETTERGENERATIONREQUESTID;
                var sig = new List<TBL_OPERATION_SIGNATORY>();
                var cam = new List<TBL_OPERATION_CAMSOL_LIST>();
                if (model.letterGenerationsignatories.Count() > 0)
                {
                    int n = 0;
                    foreach (var s in model.letterGenerationsignatories)
                    {
                        n++;
                        sig.Add(new TBL_OPERATION_SIGNATORY
                        {
                            TARGETID = model.requestId,
                            SIGNATORYID = s.signatoryId,
                            OPERATIONID = (int)OperationsEnum.LetterGenerationRequest,
                            POSITION = n,
                            DATETIMECREATED = general.GetApplicationDate(),
                            CREATEDBY = model.createdBy,
                        });
                    }
                    context.TBL_OPERATION_SIGNATORY.AddRange(sig);
                }

                if (model.letterGenerationCamsolList.Count() > 0)
                {
                    int n = 0;
                    foreach (var c in model.letterGenerationCamsolList)
                    {
                        n++;
                        cam.Add(new TBL_OPERATION_CAMSOL_LIST
                        {
                            TARGETID = model.requestId,
                            LOAN_CAMSOLID = c.camsolId,
                            OPERATIONID = (int)OperationsEnum.LetterGenerationRequest,
                            POSITION = n,
                            DATETIMECREATED = general.GetApplicationDate(),
                            CREATEDBY = model.createdBy,
                        });
                    }
                    context.TBL_OPERATION_CAMSOL_LIST.AddRange(cam);
                }

                referenceNumber = GenerateLetterGenRef(model.createdBy, sig, req);
                req.REQUESTREF = referenceNumber;
                model.requestRef = referenceNumber;
                context.SaveChanges();
                trans.Commit();
                return model;
            }
        }
            
            

        public string GenerateLetterGenRef(int requestId, List<TBL_OPERATION_SIGNATORY> signatories, TBL_LETTER_GENERATION_REQUEST request)
        {
            var reference = String.Empty;
            reference = $@"ABP/{context.TBL_STAFF.Find(requestId)?.TBL_PROFILE_BUSINESS_UNIT?.BUSINESSUNITINITIALS}";
            var sigs = signatories.OrderBy(s => s.POSITION);
            foreach(var s in sigs)
            {
                reference += $@"/{context.TBL_AUTHORISED_SIGNATORY.Find(s.SIGNATORYID)?.SIGNATORYINITIALS}";
            }
            var date = DateTime.Now;
            //var format = date.ToString("MM/dd/yy");
            var month = date.ToString("MM");
            var year = date.ToString("yy");
            reference += $@"/{month}/{year}/{request.LETTERGENERATIONREQUESTID}";

            return reference;
        }

        public LetterGenerationRequestViewModel UpdateLetterGenerationRequest(LetterGenerationRequestViewModel model, int id, UserInfo user)
        {
            var sigs = new List<TBL_OPERATION_SIGNATORY>();
            var cams = new List<TBL_OPERATION_CAMSOL_LIST>();
            int n = 0;
            var entity = this.context.TBL_LETTER_GENERATION_REQUEST.Find(id);
            var signatories = context.TBL_OPERATION_SIGNATORY.Where(s => s.DELETED == false && s.TARGETID == model.requestId && s.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest).ToList();
            var camsols = context.TBL_OPERATION_CAMSOL_LIST.Where(s => s.DELETED == false && s.TARGETID == model.requestId && s.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest).ToList();
            entity.LETTERGENERATIONREQUESTID = model.requestId;
            entity.CUSTOMERID = model.customerId;
            entity.REQUESTDATE = model.requestDate;
            entity.REQUESTTYPE = model.requestType;
            entity.ASATDATE = model.asAtDate;
            entity.COMMENTS = model.comment;
            entity.LOANBALANCE = model.loanBalance;
            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            foreach (var sig in signatories)
            {
                //if (!model.letterGenerationsignatories.Exists(s => s.signatoryId == sig.SIGNATORYID))
                //{
                //    context.TBL_OPERATION_SIGNATORY.Remove(sig);
                //}
                context.TBL_OPERATION_SIGNATORY.Remove(sig);
            }
            foreach (var s in model.letterGenerationsignatories)
            {
                n++;
                sigs.Add(new TBL_OPERATION_SIGNATORY
                {
                    TARGETID = model.requestId,
                    SIGNATORYID = s.signatoryId,
                    OPERATIONID = (int)OperationsEnum.LetterGenerationRequest,
                    POSITION = n,
                    DELETED = false,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = general.GetApplicationDate()
                });
                //if (!signatories.Exists(sig => sig.SIGNATORYID == s.signatoryId))
                //{
                //    sigs.Add(new TBL_OPERATION_SIGNATORY
                //    {
                //        TARGETID = model.requestId,
                //        SIGNATORYID = s.signatoryId,
                //        OPERATIONID = (int)OperationsEnum.LetterGenerationRequest
                //    });
                //}
            }
            if (sigs.Count() > 0)
            {
                context.TBL_OPERATION_SIGNATORY.AddRange(sigs);
            }

            foreach (var cam in camsols)
            {
                context.TBL_OPERATION_CAMSOL_LIST.Remove(cam);
            }
            foreach (var s in model.letterGenerationCamsolList)
            {
                n++;
                cams.Add(new TBL_OPERATION_CAMSOL_LIST
                {
                    TARGETID = model.requestId,
                    LOAN_CAMSOLID = s.camsolId,
                    OPERATIONID = (int)OperationsEnum.LetterGenerationRequest,
                    POSITION = n,
                    DELETED = false,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = general.GetApplicationDate()
                });
            }
            if (cams.Count() > 0)
            {
                context.TBL_OPERATION_CAMSOL_LIST.AddRange(cams);
            }

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LetterGenerationRequestUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_LETTER_GENERATION_REQUEST '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.LETTERGENERATIONREQUESTID
            });
            // Audit Section end ------------------------

            context.SaveChanges();
            return model;
        }

        public bool DeleteLetterGenerationRequest(int id, UserInfo user)
        {
            var entity = this.context.TBL_LETTER_GENERATION_REQUEST.Find(id);
            var signatories = context.TBL_OPERATION_SIGNATORY.Where(s => s.DELETED == false && s.TARGETID == entity.LETTERGENERATIONREQUESTID && s.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest).ToList();
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

            foreach(var sig in signatories)
            {
                var s = context.TBL_OPERATION_SIGNATORY.Find(sig.OPERATIONSIGNATORYID);
                s.DELETED = true;
            }

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LetterGenerationRequestDeleted,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_LETTER_GENERATION_REQUEST '{entity.ToString()}' was deleted by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                TARGETID = entity.LETTERGENERATIONREQUESTID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }


        public List<CamsolLoanDocumentViewModel> GetCamsolLoansByCustomerCode(string customerName, string customerCode)
        {
            string[] str = {" "};
            //string[] customerNames;
            var customerNames = customerName.Trim().ToLower().Split(str, StringSplitOptions.RemoveEmptyEntries);
            customerNames.ToList();
            //var test = context.TBL_LOAN_CAMSOL.Where(c => customerNames.Any(n => c.CUSTOMERNAME.Trim().ToLower().Contains(n))).ToList();
            //var test2 = context.TBL_LOAN_CAMSOL.Select(c => c.CUSTOMERNAME.Trim().ToLower()).ToList();
            //var contains = test2.Exists(n => n.Contains(customerName));
            var data = from O in context.TBL_LOAN_CAMSOL
                       join C in context.TBL_LOAN_CAMSOL_TYPE on O.CAMSOLTYPEID equals C.CAMSOLTYPEID
                       //let customerNames = customerName.Trim().ToLower().Split(str, StringSplitOptions.RemoveEmptyEntries)
                       where customerNames.Any(n => O.CUSTOMERNAME.Trim().ToLower().Contains(n)) || O.CUSTOMERCODE == customerCode || O.CUSTOMERNAME.Trim().ToLower() == customerName
                       orderby O.LOAN_CAMSOLID descending
                       select new CamsolLoanDocumentViewModel
                       {
                           camsolId = O.LOAN_CAMSOLID,
                           customerCode = O.CUSTOMERCODE,
                           customerName = O.CUSTOMERNAME,
                           accountNumber = O.ACCOUNTNUMBER,
                           balance = O.BALANCE,
                           camsolTypeName = C.CAMSOLTYPENAME
                       };
            return data.ToList();
        }

        public string GetCamsolLoanDocument(int typeId, LetterGenerationRequestViewModel model)
        {
            if (typeId == 1) {
                return GetLetterOfIndebtedness(model);
            }
            else if (typeId == 2) {
                return GetLetterOfNonIndebtedness(model);
            }
            else {
                var camsolList = (from c in context.TBL_OPERATION_CAMSOL_LIST
                                  join l in context.TBL_LOAN_CAMSOL on c.LOAN_CAMSOLID equals l.LOAN_CAMSOLID
                                  join t in context.TBL_LOAN_CAMSOL_TYPE on l.CAMSOLTYPEID equals t.CAMSOLTYPEID
                                  where c.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest && c.TARGETID == model.requestId
                                  select new CamsolLoanDocumentViewModel
                                  {
                                        accountNumber = l.ACCOUNTNUMBER,
                                        balance = (decimal)l.WRITTENOFFACCRUALAMOUNT,
                                        camsolTypeName = t.CAMSOLTYPENAME,
                                        customerCode = "5",
                                        customerName = l.CUSTOMERNAME,
                                        loanPrincipalAmount = l.PRINCIPAL
                                  }).ToList();
                return GetAuditorEnquiryHtml(camsolList);
            }
        }

        public string GetLetterOfIndebtedness(LetterGenerationRequestViewModel model)
        {
            if (model == null) {
                return "";
            }

            var camsol = context.TBL_LOAN_CAMSOL.Where(O => O.CUSTOMERNAME.Contains(model.customerName.ToUpper()) || model.customerName.ToUpper().Contains(O.CUSTOMERNAME)).FirstOrDefault();

            decimal debtAmount = 0;
            var reference = "<p style='font face:arial; size:12px'>ABP/ROG/OA/BO/03/2016/0061</p>";
            var asAtDate = model.asAtDate.ToString("dd MMM yyyy");
            var address = context.TBL_CUSTOMER_ADDRESS.Where(O => O.CUSTOMERID == model.customerId).FirstOrDefault()?.ADDRESS;
            var customerCode = model.customerCode;
            var fullName = model.customerName;
            //var accountNumber = model.accountNumber;
            var accountNumber = "0";
            debtAmount = model.loanBalance ?? 0;

            if (camsol != null) {
                //debtAmount = camsol.BALANCE;
                accountNumber = camsol.ACCOUNTNUMBER;
            }

            string result = $"<p style='font face:arial;size:12px;'><b>{model.requestRef}</b></p> " +
                $"<p style='font face:arial;size:12px;'><b>{asAtDate}.</b></p> " +
                $"<p style='font face:arial;size:12px;'><b>{fullName},</b> <br/> {address} </p> " +
                $"<p style='font face:arial;size:12px;'><b>Dear Sir/Ma,</b></p> " +
                $"<p style='font face:arial;size:12px;'><b>LETTER OF INDEBTEDNESS – {fullName} - {accountNumber}</b></p> " +
                $"<p style='font face:arial;size:12px;'>We hereby confirm that <b>{fullName}</b>, with account number {accountNumber} is indebted to our Bank as at {asAtDate}, to the tune of N {debtAmount.ToString("#,##")}.</p> " +
                $"<p style='font face:arial;size:12px;'>Please note that interest will continue to accrue on the above amount on a daily basis until the facility is fully liquidated.</p> " +
                $"<p style='font face:arial;size:12px;'><b>This report is given in strict confidence and without liability on the part of Access Bank Plc or any of its staff or agent.</b></p> " +
                $"<p style='font face:arial;size:12px;'>Thank you.</p> " +
                $"<p style='font face:arial;size:12px;'>Yours faithfully,</p> <p style='font face:arial;size:12px;'><b>For:</b> ACCESS BANK PLC</p> " +
                //$"<p></p><p><b>AUTHORISED SIGNATORY <br/> EMMANUELLA OGHOR <br/> ASSISTANT BRANCH MANAGER</b></p> " +
                //$"<p></p><p><b>AUTHORISED SIGNATORY <br/> IKECHUKWU ONYEMEM <br/> BRANCH MANAGER</b></p></font>";
                $"{GetLetterGenRequestSignatory(model.requestId)}</font>";

            return result;
        }

        public string GetLetterGenRequestSignatory(int requestId)
        {
            var result = String.Empty;
            var signatories = context.TBL_OPERATION_SIGNATORY.Where(s => s.DELETED == false && s.TARGETID == requestId && s.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest)
                .OrderBy(s => s.POSITION);
            foreach(var s in signatories)
            {
                result += $@"<p></p><p style='font face:arial;size:12px;'><b>AUTHORISED SIGNATORY <br/> {s.TBL_AUTHORISED_SIGNATORY.SIGNATORYNAME} <br/> {s.TBL_AUTHORISED_SIGNATORY.SIGNATORYTITLE}</b></p>";
            }
            return result;
        }

        public string GetLetterOfNonIndebtedness(LetterGenerationRequestViewModel model)
        {
            if (model == null) {
                return "";
            }

            var camsol = context.TBL_LOAN_CAMSOL.Where(O => O.CUSTOMERNAME.Contains(model.customerName.ToUpper()) || model.customerName.ToUpper().Contains(O.CUSTOMERNAME)).FirstOrDefault();

           // var reference = "<p style='font face:arial;size:12px;'>ABP/ROG/OA/BO/03/2016/0061</p>";
            var asAtDate = model.asAtDate.ToString("dd MMM yyyy");
            var address = context.TBL_CUSTOMER_ADDRESS.Where(O => O.CUSTOMERID == model.customerId).FirstOrDefault()?.ADDRESS;
            var fullName = model.customerName;
            var accountNumber = "0";

            if (camsol != null) {
                accountNumber = camsol.ACCOUNTNUMBER;
            }
            string result = $"<p style='font face:arial;size:12px;'><b>REF: {model.requestRef}</b></p> " +
                $"<p style='font face:arial;size:12px;'><b>{asAtDate}.</b></p> " +
                $"<p style='font face:arial;size:12px;'><b>{fullName},</b> <br/> {address}</p> " +
                $"<p style='font face:arial;size:12px;'><b>Dear Sir/Ma,</b></p> " +
                $"<p style='font face:arial;size:12px;'><b>LETTER OF NON-INDEBTEDNESS – {fullName} - {accountNumber}</b></p> " +
                $"<p style='font face:arial;size:12px;'>We hereby confirm that {fullName}, is not indebted to our Bank as at {asAtDate}.</p> " +
                $"<p style='font face:arial;size:12px;'><b>Please note that this report is given in strict confidence and without liability on the part of Access Bank Plc or any of its staff or agent.</b></p> " +
                $"<p style='font face:arial;size:12px;'>Thank you.</font></p> " +
                $"<p style='font face:arial;size:12px;'>Yours faithfully,</p> <p style='font face:arial;size:12px;'><b>For:</b> ACCESS BANK PLC</p> " +              
                //$"<p></p><p><b>AUTHORISED SIGNATORY <br/> EMMANUELLA OGHOR <br/> ASSISTANT BRANCH MANAGER</b></p> " +
                //$"<p></p><p><b>AUTHORISED SIGNATORY <br/> IKECHUKWU ONYEMEM <br/> BRANCH MANAGER</b></p></font>";
                $"{GetLetterGenRequestSignatory(model.requestId)}</font>";

            return result;
        }

        public string GetAuditorEnquiryHtml(List<CamsolLoanDocumentViewModel> list)
        {
            var n = 0;
            var result = String.Empty;
            result = result + $@"
                <table border=1 width=750 cellpadding=15 cellspacing=0 style='font face: arial; size:12px'>
                    <tr>
                        <th><b>Facility Type</b></th>
                        <th><b>Loan Amount (N)</b></th>
                        <th><b>Outstanding Amount (N)</b></th>
                        <th><b>Rate</b></th>
                        <th><b>Value Date</b></th>
                        <th><b>Maturity Date</b></th>
                    </tr>";

            foreach (var item in list)
            {
                n++;
                result = result + $@"
                    <tr>
                        <td>{item.accountNumber}</td>
                        <td>{item.loanPrincipalAmount}</td>'
                        <td>{item.balance}</td>
                        <td>{item.customerCode}</td>
                        <td>{item.camsolTypeName}</td>
                        <td>{item.customerName}</td>
                    </tr>";
            }

            result = result + $"</table>";
            return result;
        }

        public IEnumerable<LetterGenerationRequestViewModel> Search(string searchString)
        {

            var operationId = (int)OperationsEnum.LetterGenerationRequest;

            searchString = searchString.Trim().ToLower();



            var applications = (from lgr in context.TBL_LETTER_GENERATION_REQUEST
                                join c in context.TBL_CUSTOMER on lgr.CUSTOMERID equals c.CUSTOMERID
                                join atrail in context.TBL_APPROVAL_TRAIL on lgr.LETTERGENERATIONREQUESTID equals atrail.TARGETID
                                where atrail.OPERATIONID == operationId && lgr.DELETED == false
                                && atrail.TARGETID == lgr.LETTERGENERATIONREQUESTID
                                && (lgr.REQUESTREF == searchString
                                || c.FIRSTNAME.ToLower().Contains(searchString)
                                || c.LASTNAME.ToLower().Contains(searchString)
                                || c.MIDDLENAME.ToLower().Contains(searchString)
                                || lgr.REQUESTREF.ToLower().Contains(searchString)
                                )
                                select new LetterGenerationRequestViewModel
                                {
                                    requestRef = lgr.REQUESTREF,
                                    requestId = lgr.LETTERGENERATIONREQUESTID,
                                    approvalStatusId = lgr.APPLICATIONSTATUSID,
                                    arrivalDate = atrail.ARRIVALDATE,
                                    customerName = c.LASTNAME + " " + c.FIRSTNAME + " " + c.MIDDLENAME,
                                    customerCode = c.CUSTOMERCODE,
                                    approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(s => s.APPROVALSTATUSID == lgr.APPROVALSTATUSID).APPROVALSTATUSNAME,
                                    currentApprovalLevel = atrail.TOAPPROVALLEVELID != null ? context.TBL_APPROVAL_LEVEL.FirstOrDefault(s => s.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).LEVELNAME : "n/a",
                                    approvalTrailId = atrail.APPROVALTRAILID,
                                    operationId = (int)OperationsEnum.LetterGenerationRequest,
                                }).OrderByDescending(d => d.approvalTrailId).ToList();

            var applicationGrouped = applications.GroupBy(e => e.requestId).Select(e => e.FirstOrDefault()).ToList();

            return applicationGrouped;

        }

        
    }
}

           // kernel.Bind<ILetterGenerationRequestRepository>().To<LetterGenerationRequestRepository>();
           // LetterGenerationRequestAdded = ???, LetterGenerationRequestUpdated = ???, LetterGenerationRequestDeleted = ???,
