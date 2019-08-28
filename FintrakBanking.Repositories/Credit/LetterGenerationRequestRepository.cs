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

        public IEnumerable<LetterGenerationRequestViewModel> GetLetterGenerationRequests()
        {
            return context.TBL_LETTER_GENERATION_REQUEST.Where(x => x.DELETED == false
                                    && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                                    && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                                    && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.ApplicationRejected
                                    && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LetterGenerationRequestInProgress
                                    && x.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.LetterGenerationRequestCompleted)
                .Select(x => new LetterGenerationRequestViewModel
                {
                    requestId = x.LETTERGENERATIONREQUESTID,
                    customerId = x.CUSTOMERID,
                    requestDate = x.REQUESTDATE,
                    requestType = x.REQUESTTYPE,
                    asAtDate = x.ASATDATE,
                    comment = x.COMMENTS,
                    customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.LASTNAME,
                    dateTimeCreated = x.DATETIMECREATED
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
                             dateTimeCreated = (DateTime)a.DATEACTEDON
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
                requestDate = entity.REQUESTDATE,
                requestType = entity.REQUESTTYPE,
                asAtDate = entity.ASATDATE,
                comment = entity.COMMENTS,
                customerName = entity.TBL_CUSTOMER.FIRSTNAME + entity.TBL_CUSTOMER.LASTNAME
            };
        }

        public LetterGenerationRequestViewModel AddLetterGenerationRequest(LetterGenerationRequestViewModel model)
        {
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);
            var entity = new TBL_LETTER_GENERATION_REQUEST
            {
                CUSTOMERID = model.customerId,
                REQUESTDATE = model.requestDate,
                REQUESTTYPE = model.requestType,
                ASATDATE = model.asAtDate,
                COMMENTS = model.comment,
                REQUESTREF = referenceNumber,
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
            model.requestId = context.TBL_LETTER_GENERATION_REQUEST.Where(r => r.REQUESTREF == referenceNumber).FirstOrDefault().LETTERGENERATIONREQUESTID;
            return model;
        }

        public LetterGenerationRequestViewModel UpdateLetterGenerationRequest(LetterGenerationRequestViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_LETTER_GENERATION_REQUEST.Find(id);
            entity.LETTERGENERATIONREQUESTID = model.requestId;
            entity.CUSTOMERID = model.customerId;
            entity.REQUESTDATE = model.requestDate;
            entity.REQUESTTYPE = model.requestType;
            entity.ASATDATE = model.asAtDate;
            entity.COMMENTS = model.comment;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

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
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();

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


        public List<CamsolDocumentViewModel> GetCamsolLoansByCustomerCode(string customerName, string customerCode)
        {
            var data = from O in context.TBL_LOAN_CAMSOL
                       join C in context.TBL_LOAN_CAMSOL_TYPE on O.CAMSOLTYPEID equals C.CAMSOLTYPEID
                       where O.CUSTOMERNAME.Contains(customerName) || O.CUSTOMERCODE == customerCode
                       orderby O.LOAN_CAMSOLID descending
                       select new CamsolDocumentViewModel
                       {
                           customerCode = O.CUSTOMERCODE,
                           customerName = O.CUSTOMERNAME,
                           accountNumber = O.ACCOUNTNUMBER,
                           balance = O.BALANCE,
                           camsolTypeName = C.CAMSOLTYPENAME
                       };
            return data.ToList();
        }

        public string GetCamsolLoanDocument(int typeId)
        {
            if (typeId == 1) {
                return GetLetterOfIndebtedness();
            }
            else if (typeId == 2) {
                return GetLetterOfNonIndebtedness();
            }
            else {
                return GetAuditorEnquiryHtml(new List<CamsolDocumentViewModel>());
            }
        }

        public string GetLetterOfIndebtedness()
        {
            var reference = "ABP/ROG/OA/BO/03/2016/0061";
            var date = DateTime.Now;
            var address = "";
            var fullName = "";
            var accountNumber = "";
            var debtAmount = 1000;
            var salutation = $"<p><b>{reference}</b></p> <p><b>{date}.</b></p> <p><b>{fullName},</b> <br/> {address} </p> <p><b>Dear Sir/Ma,</b></p>";

            string result = $"<p><b>{reference}</b></p> " +
                $"<p><b>{date}.</b></p> " +
                $"<p><b>{fullName},</b> <br/> {address} </p> " +
                $"<p><b>Dear Sir/Ma,</b></p> " +
                $"<p><b>LETTER OF INDEBTEDNESS – {fullName} - {accountNumber}</b></p> " +
                $"<p>We hereby confirm that <b>{fullName}</b>, with account number {accountNumber} is indebted to our Bank as at {date}, to the tune of N{debtAmount}.</p> " +
                $"<p>Please note that interest will continue to accrue on the above amount on a daily basis until the facility is fully liquidated.</p> " +
                $"<p><b>This report is given in strict confidence and without liability on the part of Access Bank Plc or any of its staff or agent.</b></p> " +
                $"<p>Thank you.</p> " +
                $"<p>Yours faithfully,</p> <p><b>For:</b> ACCESS BANK PLC</p> " +
                $"<p></p><p><b>AUTHORISED SIGNATORY <br/> EMMANUELLA OGHOR <br/> ASSISTANT BRANCH MANAGER</b></p> " +
                $"<p></p><p><b>AUTHORISED SIGNATORY <br/> IKECHUKWU ONYEMEM <br/> BRANCH MANAGER</b></p>";

            return result;
        }

        public string GetLetterOfNonIndebtedness()
        {
            var reference = "ABP/ROG/OA/BO/03/2016/0061";
            var date = DateTime.Now;
            var address = "";
            var fullName = "";
            var accountNumber = "";
            var salutation = $"<p><b>{reference}</b></p> <p><b>{date}.</b></p> <p><b>{fullName},</b> <br/> {address} </p> <p><b>Dear Sir/Ma,</b></p>";

            string result = $"<p><b>{reference}</b></p> " +
                $"<p><b>{date}.</b></p> " +
                $"<p><b>{fullName},</b> <br/> {address} </p> " +
                $"<p><b>Dear Sir/Ma,</b></p> " +
                $"<p><b>LETTER OF INDEBTEDNESS – {fullName} - {accountNumber}</b></p> " +
                $"<p>We hereby confirm that {fullName}, is not indebted to our Bank as at {date}.</p> " +
                $"<p><b>Please note that this report is given in strict confidence and without liability on the part of Access Bank Plc or any of its staff or agent.</b></p> " +
                $"<p>Thank you.</p> " +
                $"<p>Yours faithfully,</p> <p><b>For:</b> ACCESS BANK PLC</p> " +
                $"<p></p><p><b>AUTHORISED SIGNATORY <br/> EMMANUELLA OGHOR <br/> ASSISTANT BRANCH MANAGER</b></p> " +
                $"<p></p><p><b>AUTHORISED SIGNATORY <br/> IKECHUKWU ONYEMEM <br/> BRANCH MANAGER</b></p>";

            return result;
        }

        public string GetAuditorEnquiryHtml(List<CamsolDocumentViewModel> list)
        {
            var n = 0;
            var result = String.Empty;
            result = result + $@"
                <table border=1 width=900 cellpadding=15 cellspacing=0>
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
                        <td>{item.applicationUrl}</td>'
                        <td>{item.balance}</td>
                        <td>{item.branchId}</td>
                        <td>{item.camsolTypeName}</td>
                        <td>{item.company}</td>
                    </tr>";
            }

            result = result + $"</table>";
            return result;
        }
    }
}

           // kernel.Bind<ILetterGenerationRequestRepository>().To<LetterGenerationRequestRepository>();
           // LetterGenerationRequestAdded = ???, LetterGenerationRequestUpdated = ???, LetterGenerationRequestDeleted = ???,
