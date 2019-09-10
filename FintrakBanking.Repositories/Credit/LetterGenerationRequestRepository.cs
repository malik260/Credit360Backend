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
                                          letterGenerationsignatories = (from x in context.TBL_LETTER_GENERATION_REQUEST
                                                                        join y in context.TBL_OPERATION_SIGNATORY on x.LETTERGENERATIONREQUESTID equals y.TARGETID
                                                                        where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest
                                                                        select new OperationSignatoryViewModel()
                                                                        {
                                                                            operationSignatoryId = y.OPERATIONSIGNATORYID,
                                                                            targetId = y.TARGETID,
                                                                            signatoryId = y.SIGNATORYID,
                                                                            operationId = y.OPERATIONID
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
                                          letterGenerationsignatories = (from l in context.TBL_LETTER_GENERATION_REQUEST
                                                                         join y in context.TBL_OPERATION_SIGNATORY on l.LETTERGENERATIONREQUESTID equals y.TARGETID
                                                                         where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest
                                                                         select new OperationSignatoryViewModel()
                                                                         {
                                                                             operationSignatoryId = y.OPERATIONSIGNATORYID,
                                                                             targetId = y.TARGETID,
                                                                             signatoryId = y.SIGNATORYID,
                                                                             operationId = y.OPERATIONID
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
                    letterGenerationsignatories = (from l in context.TBL_LETTER_GENERATION_REQUEST
                                                   join y in context.TBL_OPERATION_SIGNATORY on l.LETTERGENERATIONREQUESTID equals y.TARGETID
                                                   where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest
                                                   select new OperationSignatoryViewModel()
                                                   {
                                                       operationSignatoryId = y.OPERATIONSIGNATORYID,
                                                       targetId = y.TARGETID,
                                                       signatoryId = y.SIGNATORYID,
                                                       operationId = y.OPERATIONID
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
            var query = (from a in context.TBL_LETTER_GENERATION_REQUEST
                         where
                            (a.DELETED == false
                            && a.APPLICATIONSTATUSID == (int)LoanApplicationStatusEnum.LetterGenerationRequestInProgress)
                         orderby a.DATEACTEDON
                         join b in context.TBL_APPROVAL_TRAIL on a.LETTERGENERATIONREQUESTID equals b.TARGETID
                         where
                            (
                            (b.OPERATIONID == operationId)
                            && b.APPROVALSTATEID != (int)ApprovalState.Ended
                            && b.RESPONSESTAFFID == null
                            && levelIds.Contains((int)b.TOAPPROVALLEVELID)
                            && (b.TOSTAFFID == null || b.TOSTAFFID == staffId)
                            )
                         select new LetterGenerationRequestViewModel()
                         {
                             requestId = a.LETTERGENERATIONREQUESTID,
                             requestDate = a.REQUESTDATE,
                             requestType = a.REQUESTTYPE,
                             asAtDate = a.ASATDATE,
                             comment = a.COMMENTS,
                             customerId = a.CUSTOMERID,
                             customerName = a.TBL_CUSTOMER.FIRSTNAME + a.TBL_CUSTOMER.LASTNAME,
                             lastComment = b.COMMENT,
                             currentApprovalStateId = b.APPROVALSTATEID,
                             currentApprovalLevelId = b.TOAPPROVALLEVELID,
                             currentApprovalLevel = b.TBL_APPROVAL_LEVEL1.LEVELNAME, // pls note! tbl_Approval_Level1<---1
                             currentApprovalLevelTypeId = b.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // pls note! tbl_Approval_Level1<---1
                             approvalTrailId = b == null ? 0 : b.APPROVALTRAILID, // for inner sequence ordering
                             toStaffId = b.TOSTAFFID,
                             approvalStatusId = (short)a.APPROVALSTATUSID,
                             applicationStatusId = a.APPLICATIONSTATUSID,
                             createdBy = (int)a.CREATEDBY,
                             operationId = operationId,
                             dateTimeCreated = (DateTime)a.DATEACTEDON,
                             customerCode = a.TBL_CUSTOMER.CUSTOMERCODE,
                             requestRef = a.REQUESTREF,
                             loanBalance = a.LOANBALANCE,
                             letterGenerationsignatories = (from l in context.TBL_LETTER_GENERATION_REQUEST
                                                            join y in context.TBL_OPERATION_SIGNATORY on l.LETTERGENERATIONREQUESTID equals y.TARGETID
                                                            where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest
                                                            select new OperationSignatoryViewModel()
                                                            {
                                                                operationSignatoryId = y.OPERATIONSIGNATORYID,
                                                                targetId = y.TARGETID,
                                                                signatoryId = y.SIGNATORYID,
                                                                operationId = y.OPERATIONID
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
                letterGenerationsignatories = (from l in context.TBL_LETTER_GENERATION_REQUEST
                                               join y in context.TBL_OPERATION_SIGNATORY on l.LETTERGENERATIONREQUESTID equals y.TARGETID
                                               where y.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest
                                               select new OperationSignatoryViewModel()
                                               {
                                                   operationSignatoryId = y.OPERATIONSIGNATORYID,
                                                   targetId = y.TARGETID,
                                                   signatoryId = y.SIGNATORYID,
                                                   operationId = y.OPERATIONID
                                               }).ToList(),
            };
        }

        public LetterGenerationRequestViewModel AddLetterGenerationRequest(LetterGenerationRequestViewModel model)
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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            context.TBL_AUDIT.Add(aud);
            // Audit Section end ------------------------

            context.SaveChanges();
            var req = context.TBL_LETTER_GENERATION_REQUEST.Where(r => r.REQUESTREF == referenceNumber).FirstOrDefault();
            model.requestId = req.LETTERGENERATIONREQUESTID;
            var sig = new List<TBL_OPERATION_SIGNATORY>();
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
                    });
                }
                context.TBL_OPERATION_SIGNATORY.AddRange(sig);
            }
            referenceNumber = GenerateLetterGenRef(model.createdBy, sig, req);
            req.REQUESTREF = referenceNumber;
            model.requestRef = referenceNumber;
            context.SaveChanges();
            return model;
        }

        public string GenerateLetterGenRef(int requestId, List<TBL_OPERATION_SIGNATORY> signatories, TBL_LETTER_GENERATION_REQUEST request)
        {
            var reference = String.Empty;
            reference = $@"ABP/{context.TBL_STAFF.Find(requestId).TBL_PROFILE_BUSINESS_UNIT.BUSINESSUNITINITIALS}";
            var sigs = signatories.OrderBy(s => s.POSITION);
            foreach(var s in sigs)
            {
                reference += $@"/{context.TBL_AUTHORISED_SIGNATORY.Find(s.SIGNATORYID).SIGNATORYINITIALS}";
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
            int n = 0;
            var entity = this.context.TBL_LETTER_GENERATION_REQUEST.Find(id);
            var signatories = context.TBL_OPERATION_SIGNATORY.Where(s => s.DELETED == false && s.TARGETID == model.requestId && s.OPERATIONID == (int)OperationsEnum.LetterGenerationRequest).ToList();
            entity.LETTERGENERATIONREQUESTID = model.requestId;
            entity.CUSTOMERID = model.customerId;
            entity.REQUESTDATE = model.requestDate;
            entity.REQUESTTYPE = model.requestType;
            entity.ASATDATE = model.asAtDate;
            entity.COMMENTS = model.comment;

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

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LetterGenerationRequestUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_LETTER_GENERATION_REQUEST '{entity.ToString()}' was updated by {auditStaff}",
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.LETTERGENERATIONREQUESTID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }


        public List<CamsolLoanDocumentViewModel> GetCamsolLoansByCustomerCode(string customerName, string customerCode)
        {
            var data = from O in context.TBL_LOAN_CAMSOL
                       join C in context.TBL_LOAN_CAMSOL_TYPE on O.CAMSOLTYPEID equals C.CAMSOLTYPEID
                       where O.CUSTOMERNAME.Contains(customerName.ToUpper()) || O.CUSTOMERCODE == customerCode
                       orderby O.LOAN_CAMSOLID descending
                       select new CamsolLoanDocumentViewModel
                       {
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
                return GetAuditorEnquiryHtml(new List<CamsolLoanDocumentViewModel>());
            }
        }

        public string GetLetterOfIndebtedness(LetterGenerationRequestViewModel model)
        {
            if (model == null) {
                return "";
            }

            var camsol = context.TBL_LOAN_CAMSOL.Where(O => O.CUSTOMERNAME.Contains(model.customerName.ToUpper()) || model.customerName.ToUpper().Contains(O.CUSTOMERNAME)).FirstOrDefault();

            decimal debtAmount = 0;
            var reference = "ABP/ROG/OA/BO/03/2016/0061";
            var asAtDate = model.asAtDate.ToString("dd MMM yyyy");
            var address = context.TBL_CUSTOMER_ADDRESS.Where(O => O.CUSTOMERID == model.customerId).FirstOrDefault()?.ADDRESS;
            var customerCode = model.customerCode;
            var fullName = model.customerName;
            //var accountNumber = model.accountNumber;
            var accountNumber = "0";
            debtAmount = model.loanBalance.Value;

            if (camsol != null) {
                //debtAmount = camsol.BALANCE;
                accountNumber = camsol.ACCOUNTNUMBER;
            }

            string result = $"<font face=Arial><p><b>REF: {model.requestRef}</b></p> " +
                $"<p><b>{asAtDate}.</b></p> " +
                $"<p><b>{fullName},</b> <br/> {address} </p> " +
                $"<p><b>Dear Sir/Ma,</b></p> " +
                $"<p><b>LETTER OF INDEBTEDNESS – {fullName} - {accountNumber}</b></p> " +
                $"<p>We hereby confirm that <b>{fullName}</b>, with account number {accountNumber} is indebted to our Bank as at {asAtDate}, to the tune of N {debtAmount.ToString("#,##")}.</p> " +
                $"<p>Please note that interest will continue to accrue on the above amount on a daily basis until the facility is fully liquidated.</p> " +
                $"<p><b>This report is given in strict confidence and without liability on the part of Access Bank Plc or any of its staff or agent.</b></p> " +
                $"<p>Thank you.</p> " +
                $"<p>Yours faithfully,</p> <p><b>For:</b> ACCESS BANK PLC</p> " +
                $"<p></p><p><b>AUTHORISED SIGNATORY <br/> EMMANUELLA OGHOR <br/> ASSISTANT BRANCH MANAGER</b></p> " +
                $"<p></p><p><b>AUTHORISED SIGNATORY <br/> IKECHUKWU ONYEMEM <br/> BRANCH MANAGER</b></p></font>";

            return result;
        }

        public string GetLetterOfNonIndebtedness(LetterGenerationRequestViewModel model)
        {
            if (model == null) {
                return "";
            }

            var camsol = context.TBL_LOAN_CAMSOL.Where(O => O.CUSTOMERNAME.Contains(model.customerName.ToUpper()) || model.customerName.ToUpper().Contains(O.CUSTOMERNAME)).FirstOrDefault();

            var reference = "ABP/ROG/OA/BO/03/2016/0061";
            var asAtDate = model.asAtDate.ToString("dd MMM yyyy");
            var address = context.TBL_CUSTOMER_ADDRESS.Where(O => O.CUSTOMERID == model.customerId).FirstOrDefault()?.ADDRESS;
            var fullName = model.customerName;
            var accountNumber = "0";

            if (camsol != null) {
                accountNumber = camsol.ACCOUNTNUMBER;
            }

            string result = $"<font face=Arial><p><b>REF: {model.requestRef}</b></p> " +
                $"<p><b>{asAtDate}.</b></p> " +
                $"<p><b>{fullName},</b> <br/> {address} </p> " +
                $"<p><b>Dear Sir/Ma,</b></p> " +
                $"<p><b>LETTER OF INDEBTEDNESS – {fullName} - {accountNumber}</b></p> " +
                $"<p>We hereby confirm that {fullName}, is not indebted to our Bank as at {asAtDate}.</p> " +
                $"<p><b>Please note that this report is given in strict confidence and without liability on the part of Access Bank Plc or any of its staff or agent.</b></p> " +
                $"<p>Thank you.</p> " +
                $"<p>Yours faithfully,</p> <p><b>For:</b> ACCESS BANK PLC</p> " +
                $"<p></p><p><b>AUTHORISED SIGNATORY <br/> EMMANUELLA OGHOR <br/> ASSISTANT BRANCH MANAGER</b></p> " +
                $"<p></p><p><b>AUTHORISED SIGNATORY <br/> IKECHUKWU ONYEMEM <br/> BRANCH MANAGER</b></p></font>";

            return result;
        }

        public string GetAuditorEnquiryHtml(List<CamsolLoanDocumentViewModel> list)
        {
            var n = 0;
            var result = String.Empty;
            result = result + $@"
                <table border=1 width=750 cellpadding=15 cellspacing=0>
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
                        <td>{item.balance}</td>'
                        <td>{item.camsolTypeName}</td>
                        <td>{item.customerCode}</td>
                        <td>{item.customerName}</td>
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
                                || c.MIDDLENAME.ToLower().Contains(searchString))
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
