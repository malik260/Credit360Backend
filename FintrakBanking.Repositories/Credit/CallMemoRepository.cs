using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class CallMemoRepository : ICallMemoRepository
    {
        private readonly FinTrakBankingContext _context;
        private readonly IGeneralSetupRepository _genSetup;
        private readonly IAuditTrailRepository _auditTrail;
        private readonly IMemorandumRepository _memorandum;
        private readonly string support = ConfigurationManager.AppSettings["SupportEmailAddr"];
        private readonly IWorkflow _workflow;
       
        public CallMemoRepository(FinTrakBankingContext context, IGeneralSetupRepository genSetup,
                                  IAuditTrailRepository auditTrail, IWorkflow workflow, IMemorandumRepository memorandum)
        {
            _context = context;
            _genSetup = genSetup;
            _auditTrail = auditTrail;
            _workflow = workflow;
            _memorandum = memorandum;
        }
        public IQueryable<CallMemoLoanSearchViewModel> SearchForCallMemoLoan(int staffId, string searchQuery)
        {
            IQueryable<CallMemoLoanSearchViewModel> allFilteredLoan = null;
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {

                //var JobRole = (from a in _context.TBL_STAFF where a.STAFFID == staffId select a.JOBTITLEID).FirstOrDefault();
                var JobRole = (from a in _context.TBL_STAFF where a.STAFFID == staffId select a.STAFFROLEID).FirstOrDefault();

                if (JobRole > 0)
                {
                    var memoLimit = (from b in _context.TBL_CALL_MEMO_LIMIT where b.JOBTITLEID == JobRole select b).FirstOrDefault();
                    if (memoLimit != null)
                    {
                        allFilteredLoan = (from a in _context.TBL_CUSTOMER
                                           //join b in _context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                           //where a.APPLICATIONAMOUNT <= memoLimit.MAXIMUMAMOUNT && a.APPLICATIONAMOUNT >= memoLimit.MINIMUMAMOUNT && (a.APPLICATIONREFERENCENUMBER.Contains(searchQuery) ||
                                           where a.CUSTOMERCODE.ToLower().Contains(searchQuery) || a.FIRSTNAME.ToLower().StartsWith(searchQuery) || a.LASTNAME.StartsWith(searchQuery)
                                           select new CallMemoLoanSearchViewModel
                                           {
                                               //loanApplicationId = a.LOANAPPLICATIONID,
                                               customerId = a.CUSTOMERID,
                                               customerName = a.CUSTOMERCODE + " - " + a.FIRSTNAME + " " + a.LASTNAME,
                                               //loanReferenceNo = a.APPLICATIONREFERENCENUMBER,
                                               //principalAmount = a.APPLICATIONAMOUNT,
                                               customerCode = a.CUSTOMERCODE,
                                               email = a.EMAILADDRESS,
                                               //callDate = b.da
                                               //nextCallDate = b.ne
                                               operationId = (int)OperationsEnum.CallMemo
                                           }).Take(10).AsQueryable();

                    }
                    else
                    {
                        var jobName = _context.TBL_STAFF_JOBTITLE.Where(x => x.JOBTITLEID == JobRole).FirstOrDefault();
                        throw new ConditionNotMetException("Kindly Setup Call Memo Limit For Job Title : '" + jobName.JOBTITLENAME + "' For This Staff");
                    }
                }
            }

            return allFilteredLoan;
        }
        #region "Call Limit"

        public IEnumerable<CallMemoTypeViewModel> GetCallLimitType()
        {
            var data = (from a in _context.TBL_CALL_MEMO_TYPE
                        orderby a.NAME
                        select new CallMemoTypeViewModel
                        {
                            CallLimitTypeId = a.CALLLIMITTYPEID,
                            Name = a.NAME
                        }).ToList();
            return data;
        }
        public IEnumerable<CallLimitViewModel> GetAllCallLimit(int companyId)
        {
            var data = (from a in _context.TBL_CALL_MEMO_LIMIT
                        where a.DELETED == false && a.COMPANYID == companyId
                        select new CallLimitViewModel
                        {
                            MaximumAmount = a.MAXIMUMAMOUNT,
                            MinimumAmount = a.MINIMUMAMOUNT,
                            CallLimitId = a.CALLLIMITID,
                            companyId = a.COMPANYID,
                            FrequencyId = a.FREQUENCYID,
                            FrequencyName = a.TBL_FREQUENCY_TYPE.MODE,
                            JobTitleId = a.JOBTITLEID,
                            JobTitleName = _context.TBL_STAFF_ROLE.FirstOrDefault(d => d.STAFFROLEID == a.JOBTITLEID).STAFFROLENAME,
                           // CallLimitTypeName = _context.TBL_CALL_MEMO_TYPE.FirstOrDefault(i => i.CALLLIMITTYPEID == a.CALLLIMITTYPEID).NAME
                        }).ToList();
            return data;
        }

        public List<CallLimitViewModel> GetCallLimitByTypeId(int limitId)
        {
            var data = (from a in _context.TBL_CALL_MEMO_LIMIT
                        where a.DELETED == false && a.CALLLIMITID == limitId
                        //orderby a.CALLLIMITTYPEID
                        select new CallLimitViewModel
                        {
                            MaximumAmount = a.MAXIMUMAMOUNT,
                            MinimumAmount = a.MINIMUMAMOUNT,
                            CallLimitId = a.CALLLIMITID,
                            companyId = a.COMPANYID,
                            FrequencyId = a.FREQUENCYID,
                            FrequencyName = a.TBL_FREQUENCY_TYPE.MODE,
                            JobTitleId = a.JOBTITLEID,
                            JobTitleName = _context.TBL_STAFF_JOBTITLE.FirstOrDefault(d => d.JOBTITLEID == a.JOBTITLEID).JOBTITLENAME,
                           // CallLimitTypeId = a.CALLLIMITTYPEID,
                           // CallLimitTypeName = _context.TBL_CALL_MEMO_TYPE.FirstOrDefault(i => i.CALLLIMITTYPEID == a.CALLLIMITTYPEID).NAME
                        }).ToList();
            return data;
        }
        public bool isLimitExist(CallLimitViewModel model)
        {
            return _context.TBL_CALL_MEMO_LIMIT.Where(x => x.JOBTITLEID == model.JobTitleId).Any();
        }
        public bool AddCallLimit(CallLimitViewModel model)
        {
            var data = new TBL_CALL_MEMO_LIMIT
            {
                MAXIMUMAMOUNT = model.MaximumAmount,
                MINIMUMAMOUNT = model.MinimumAmount,
                FREQUENCYID = model.FrequencyId,
                JOBTITLEID = model.JobTitleId,
                //CALLLIMITTYPEID = model.CallLimitTypeId,
                COMPANYID = model.companyId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = _genSetup.GetApplicationDate()
            };

            _context.TBL_CALL_MEMO_LIMIT.Add(data);

            // Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitAdded,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Added tbl_Call_Limit '{ data.CALLLIMITID }' ",        
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),             
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool UpdateCallLimit(int limitId, CallLimitViewModel model)
        {
            var data = _context.TBL_CALL_MEMO_LIMIT.Find(limitId);
            if (data == null) return false;

            data.MAXIMUMAMOUNT = model.MaximumAmount;
            data.MINIMUMAMOUNT = model.MinimumAmount;
            //data.CALLLIMITTYPEID = model.CallLimitTypeId;
            data.FREQUENCYID = model.FrequencyId;
            data.JOBTITLEID = model.JobTitleId;
            // Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitUpdated,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Updated tbl_Call_Limit : '{ data.CALLLIMITID }' ",               
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),              
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool DeleteCallLimit(int limitId, UserInfo user)
        {
            var data = _context.TBL_CALL_MEMO_LIMIT.Find(limitId);
            if (data != null)
            {
                data.DELETED = true;
                data.DELETEDBY = user.staffId;
                data.DATETIMEDELETED = _genSetup.GetApplicationDate();
            }

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitDeleted,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Deleted tbl_Call_Limit with Id : '{ limitId }' ",   
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),             
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }
        #endregion

        #region "Call Memo"
        public IEnumerable<CallMemoViewModel> GetCustomerCallMemo(int staffId, int customerId)
        {
            var data = (from a in _context.TBL_CALL_MEMO
                        join c in _context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                        where a.STAFFID == staffId && a.CUSTOMERID == customerId && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending
                        orderby a.CALLMEMOID
                        select new CallMemoViewModel
                        {
                            callMemoId = a.CALLMEMOID,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            //LoanReferenceNo = b.APPLICATIONREFERENCENUMBER,
                            staffId = a.STAFFID,
                            participants = a.PARTICIPANTS,
                            location = a.LOCATION,
                            customerName = c.FIRSTNAME + " " + c.LASTNAME,
                            customerId = c.CUSTOMERID,
                            memoDate = a.MEMODATE,
                            nextCallDate = a.NEXTCALLDATE,
                            purpose = a.PURPOSE,
                            discusion = a.DISCUSION,
                            cc = a.CC,
                            recentUpdate = a.RECENTUPDATE,
                            action = a.ACTION,
                            background = a.BACKGROUND,
                            createdBy = a.CREATEDBY,
                            dateTimeCreated = a.DATECREATED,
                            approvalStatus = _context.TBL_APPROVAL_STATUS.Where(O => O.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(O => O.APPROVALSTATUSNAME).FirstOrDefault(),
                            operationId = (int) OperationsEnum.CallMemo
                        }).ToList();
            return data;
        }

        public IEnumerable<ApprovalTrailCallMemoViewModel> GetAppraisalMemorandumTrailCallMemo(int operationId)
        {
            var data = (from a in _context.TBL_APPROVAL_GROUP_MAPPING
                        join b in _context.TBL_APPROVAL_GROUP on a.GROUPID equals b.GROUPID
                        join c in _context.TBL_APPROVAL_LEVEL on b.GROUPID equals c.GROUPID
                        where a.OPERATIONID == 150
                        select new ApprovalTrailCallMemoViewModel
                        {
                            levelName = c.LEVELNAME,
                            approvalLevelId = c.APPROVALLEVELID,
                        })?.ToList();

            return data;
        }

        public IEnumerable<CallMemoViewModel> GetCustomerApprovedCallMemo(int staffId, int customerId)
        {
            var data = (from a in _context.TBL_CALL_MEMO
                        join c in _context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                        where a.STAFFID == staffId && a.CUSTOMERID == customerId && a.APPROVALSTATUSID == (int) ApprovalStatusEnum.Approved
                        orderby a.CALLMEMOID
                        select new CallMemoViewModel
                        {
                            callMemoId = a.CALLMEMOID,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            staffId = a.STAFFID,
                            participants = a.PARTICIPANTS,
                            location = a.LOCATION,
                            customerName = c.FIRSTNAME + " " + c.LASTNAME,
                            customerId = c.CUSTOMERID,
                            memoDate = a.MEMODATE,
                            nextCallDate = a.NEXTCALLDATE,
                            purpose = a.PURPOSE,
                            discusion = a.DISCUSION,
                            cc = a.CC,
                            recentUpdate = a.RECENTUPDATE,
                            action = a.ACTION,
                            background = a.BACKGROUND,
                            createdBy = a.CREATEDBY,
                            dateTimeCreated = a.DATECREATED,
                            approvalStatus = _context.TBL_APPROVAL_STATUS.Where(O => O.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(O => O.APPROVALSTATUSNAME).FirstOrDefault(),
                            operationId = (int)OperationsEnum.CallMemo
                        }).ToList();
            return data;
        }

        public IEnumerable<CallMemoViewModel> SearchCallMemo(int staffId, CallMemoViewModel model)
        {
            var staffs = from s in _context.TBL_STAFF select s;
            var initiator = _context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.CallMemo).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault();
            //&& a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
            var customer = model.customerName;
            var firstQuery = (from a in _context.TBL_CALL_MEMO
                        join c in _context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                        where (c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME).ToLower().Contains(customer.ToLower())
                        && a.MEMODATE >= model.startDate && a.MEMODATE <= model.endDate
                        orderby a.CALLMEMOID
                        select new CallMemoViewModel
                        {
                            callMemoId = a.CALLMEMOID,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            staffId = a.STAFFID,
                            participants = a.PARTICIPANTS,
                            location = a.LOCATION,
                            customerName = c.FIRSTNAME + " " + c.LASTNAME,
                            customerId = c.CUSTOMERID,
                            memoDate = a.MEMODATE,
                            nextCallDate = a.NEXTCALLDATE,
                            purpose = a.PURPOSE,
                            discusion = a.DISCUSION,
                            cc = a.CC,
                            recentUpdate = a.RECENTUPDATE,
                            action = a.ACTION,
                            background = a.BACKGROUND,
                            createdBy = a.CREATEDBY,
                            dateTimeCreated = a.DATECREATED,
                            approvalStatus = _context.TBL_APPROVAL_STATUS.Where(O => O.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(O => O.APPROVALSTATUSNAME).FirstOrDefault(),
                            operationId = (int)OperationsEnum.CallMemo
                        }).ToList();
            var secondQuery = (from x in _context.TBL_CALL_MEMO
                               join trail in _context.TBL_APPROVAL_TRAIL on x.CALLMEMOID equals trail.TARGETID
                               join c in _context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                               where trail.OPERATIONID == (short)OperationsEnum.CallMemo
                                    && trail.TARGETID == x.CALLMEMOID
                               orderby trail.APPROVALTRAILID descending
                               select new CallMemoViewModel
                               {
                                   callMemoId = x.CALLMEMOID,
                                   loanApplicationId = x.LOANAPPLICATIONID,
                                   participants = x.PARTICIPANTS,
                                   location = x.LOCATION,
                                   customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                   customerId = c.CUSTOMERID,
                                   memoDate = x.MEMODATE,
                                   nextCallDate = x.NEXTCALLDATE,
                                   purpose = x.PURPOSE,
                                   discusion = x.DISCUSION,
                                   action = x.ACTION,
                                   cc = x.CC,
                                   background = x.BACKGROUND,
                                   recentUpdate = x.RECENTUPDATE,
                                   createdBy = x.CREATEDBY,
                                   dateTimeCreated = x.DATECREATED,
                                   operationId = (int)OperationsEnum.CallMemo,
                                   loopedStaffId = trail.LOOPEDSTAFFID,
                                   approvalStatusId = trail.APPROVALSTATUSID,
                                   approvalTrailId = trail.APPROVALTRAILID,
                                   toApprovalLevelName = trail.TOAPPROVALLEVELID == null ? "N/A" : _context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == trail.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                   fromApprovalLevelName = trail.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == trail.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : _context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == trail.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                   approvalStatusName = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == trail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                               }).GroupBy(l => l.callMemoId).Select(l => l.OrderByDescending(t => t.approvalTrailId).FirstOrDefault())
                                        .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                        || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                        && l.loopedStaffId == staffId)).ToList();

            var data = firstQuery.Union(secondQuery).ToList();
            return data;
        }

        public IEnumerable<CallMemoViewModel> GetAllCallMemo(int staffId)
        {
            var staffs = from s in _context.TBL_STAFF select s;
            var initiator = _context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.CallMemo).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault();

                var firstQuery = (from a in _context.TBL_CALL_MEMO
                                  join c in _context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                                  where a.STAFFID == staffId && (a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                                  orderby a.CALLMEMOID
                                  select new CallMemoViewModel
                                  {
                                      callMemoId = a.CALLMEMOID,
                                      loanApplicationId = a.LOANAPPLICATIONID,
                                      participants = a.PARTICIPANTS,
                                      location = a.LOCATION,
                                      approvalStatusId = a.APPROVALSTATUSID,
                                      customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                      customerId = c.CUSTOMERID,
                                      memoDate = a.MEMODATE,
                                      nextCallDate = a.NEXTCALLDATE,
                                      purpose = a.PURPOSE,
                                      discusion = a.DISCUSION,
                                      action = a.ACTION,
                                      cc = a.CC,
                                      background = a.BACKGROUND,
                                      recentUpdate = a.RECENTUPDATE,
                                      createdBy = a.CREATEDBY,
                                      dateTimeCreated = a.DATECREATED,
                                      approvalStatusName = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == a.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                                      operationId = (int)OperationsEnum.CallMemo
                                  }).OrderByDescending(a => a.callMemoId).ToList();

                var secondQuery = (from x in _context.TBL_CALL_MEMO
                                   join trail in _context.TBL_APPROVAL_TRAIL on x.CALLMEMOID equals trail.TARGETID
                                   join c in _context.TBL_CUSTOMER on x.CUSTOMERID equals c.CUSTOMERID
                                   where trail.OPERATIONID == (short)OperationsEnum.CallMemo
                                        && trail.TARGETID == x.CALLMEMOID
                                   orderby trail.APPROVALTRAILID descending
                                   select new CallMemoViewModel
                                   {
                                       callMemoId = x.CALLMEMOID,
                                       loanApplicationId = x.LOANAPPLICATIONID,
                                       participants = x.PARTICIPANTS,
                                       location = x.LOCATION,
                                       customerName = c.FIRSTNAME + " " + c.LASTNAME,
                                       customerId = c.CUSTOMERID,
                                       memoDate = x.MEMODATE,
                                       nextCallDate = x.NEXTCALLDATE,
                                       purpose = x.PURPOSE,
                                       discusion = x.DISCUSION,
                                       action = x.ACTION,
                                       cc = x.CC,
                                       background = x.BACKGROUND,
                                       recentUpdate = x.RECENTUPDATE,
                                       createdBy = x.CREATEDBY,
                                       dateTimeCreated = x.DATECREATED,
                                       operationId = (int)OperationsEnum.CallMemo,
                                       loopedStaffId = trail.LOOPEDSTAFFID,
                                       approvalStatusId = trail.APPROVALSTATUSID,
                                       approvalTrailId = trail.APPROVALTRAILID,
                                       toApprovalLevelName = trail.TOAPPROVALLEVELID == null ? "N/A" : _context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == trail.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                       fromApprovalLevelName = trail.FROMAPPROVALLEVELID == null ? staffs.FirstOrDefault(r => r.STAFFID == trail.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : _context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == trail.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                                       approvalStatusName = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == trail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                                   }).GroupBy(l => l.callMemoId).Select(l => l.OrderByDescending(t => t.approvalTrailId).FirstOrDefault())
                                        .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                        || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                        && l.loopedStaffId == staffId)).ToList();

            var data = firstQuery.Union(secondQuery).ToList();
            return data;
        }

        public int AddCallMemo(CallMemoViewModel model)
        {
            var data = new TBL_CALL_MEMO
            {
                LOANAPPLICATIONID = model.loanApplicationId,
                STAFFID = model.staffId,
                MEMODATE = model.memoDate,
                NEXTCALLDATE = model.nextCallDate,
                PURPOSE = model.purpose,
                PARTICIPANTS = model.participants,
                BACKGROUND = model.background,
                LOCATION = model.location,
                DISCUSION = model.discusion,
                CALLTIME = model.callTime,
                CC = model.cc,
                NEXTCALLTIME = model.nextCallTime,
                ACTION = model.action,
                RECENTUPDATE = model.recentUpdate,
                APPROVALLEVELID = model.approvalLevelId,
                CREATEDBY = model.createdBy,
                CUSTOMERID = model.customerId,
                OPERATIONID = (int) OperationsEnum.CallMemo,
                APPROVALSTATUSID = (int) ApprovalStatusEnum.Pending,
                DATECREATED = _genSetup.GetApplicationDate()
            };

            var res = _context.TBL_CALL_MEMO.Add(data);
            
            // Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitAdded,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Added Call Memo for: '{ data.PURPOSE }' by {model.staffId}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()

               
            };

            _auditTrail.AddAuditTrail(audit);
            _context.SaveChanges();

            //end of Audit section -----------------------
            return res.CALLMEMOID;
        }

        public CallMemoViewModel GetCallMemoById(int callMemoID)
        {
            var data = (from a in _context.TBL_CALL_MEMO
                        //join b in _context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        join c in _context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                        where a.CALLMEMOID == callMemoID && a.APPROVALSTATUSID == (int) ApprovalStatusEnum.Pending
                        orderby a.CALLMEMOID
                        select new CallMemoViewModel
                        {
                            callMemoId = a.CALLMEMOID,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            //LoanReferenceNo = b.APPLICATIONREFERENCENUMBER,
                            staffId = a.STAFFID,
                            participants = a.PARTICIPANTS,
                            location = a.LOCATION,
                            customerName = c.FIRSTNAME + " " + c.LASTNAME,
                            customerId = c.CUSTOMERID,
                            memoDate = a.MEMODATE,
                            nextCallDate = a.NEXTCALLDATE,
                            purpose = a.PURPOSE,
                            discusion = a.DISCUSION,
                            cc = a.CC,
                            action = a.ACTION,
                            background = a.BACKGROUND,
                            recentUpdate = a.RECENTUPDATE,
                            createdBy = a.CREATEDBY,
                            dateTimeCreated = a.DATECREATED,
                            operationId = (int)OperationsEnum.CallMemo
                        }).FirstOrDefault();

            return data;
        }

        public bool UpdateCallMemo(int limitId, CallMemoViewModel model)
        {
            var data = _context.TBL_CALL_MEMO.Find(limitId);
            if (data == null) return false;
            data.PARTICIPANTS = model.participants;
            data.LOANAPPLICATIONID = model.loanApplicationId;
            data.STAFFID = model.createdBy;
            data.MEMODATE = model.memoDate;
            data.NEXTCALLDATE = model.nextCallDate;
            data.PURPOSE = model.purpose;
            data.DISCUSION = model.discusion;
            data.ACTION = model.action;
            data.LOCATION = model.location;
            data.BACKGROUND = model.background;
            data.RECENTUPDATE = model.recentUpdate;
            data.CC = model.cc;
            // Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitUpdated,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Updated tbl_Call_memo for data with Id : '{ data.CALLMEMOID }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool GoForCallMemoApproval(CallMemoViewModel entity)
        {
            var callMemos = _context.TBL_CALL_MEMO.Where(O => O.CALLMEMOID == entity.callMemoId 
                                                         && O.APPROVALSTATUSID == (int) ApprovalStatusEnum.Pending).Select(O => O).ToList();

            try
            {
                foreach (var callMemo in callMemos)
                {
                    callMemo.APPROVALSTATUSID = (int) ApprovalStatusEnum.Processing;
                }

                if (callMemos != null)
                {
                    _workflow.StaffId = entity.createdBy;
                    _workflow.CompanyId = entity.companyId;
                    _workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    _workflow.TargetId = entity.callMemoId;
                    _workflow.Comment = "Request for call memo approval";
                    _workflow.OperationId = (int)OperationsEnum.CallMemo;
                    _workflow.DeferredExecution = true;
                    _workflow.ExternalInitialization = true;
                    _workflow.LogActivity();

                    var message = new TBL_MESSAGE_LOG();
                    if (_workflow.NewState == (int)ApprovalState.Ended) 
                    {
                        if (_workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                        {
                            var memo = _context.TBL_CALL_MEMO.Find(entity.callMemoId);
                            var accountOfficer = _context.TBL_STAFF.Where(s => s.STAFFID == memo.CREATEDBY).FirstOrDefault();
                            var emailList = GetBusinessUsersEmails(accountOfficer.MISCODE) +";"+memo.CC;
                            var subject = $"Call Memo Approved Notification";
                            var messageBody = $"Dear All,<br/> Call Memo with purpose " + memo.PURPOSE + " has been approved.<br/> Kindly see details below.";
                                messageBody = messageBody + " " + _memorandum.GetCallMemoMarkup(entity.callMemoId);

                            message = new TBL_MESSAGE_LOG 
                            {
                                TOADDRESS = emailList,
                                MESSAGESUBJECT = subject,
                                MESSAGEBODY = messageBody,
                                MESSAGESTATUSID = (short)MessageStatusEnum.Pending,
                                MESSAGETYPEID = (short)MessageTypeEnum.Email,
                                FROMADDRESS = this.support,
                                DATETIMERECEIVED = DateTime.Now,
                                SENDONDATETIME = DateTime.Now,
                                TARGETID = entity.callMemoId,
                                OPERATIONID = (int)OperationsEnum.CallMemo,
                        };
                            _context.TBL_MESSAGE_LOG.Add(message);
                            _context.SaveChanges();

                        }
                    }


                }
            }
            catch (Exception ex) { }

            return _context.SaveChanges() != 0;
        }

        private string GetBusinessUsersEmails(string accountOfficerMIsCode)
        {
            string emailList = "";

            var accountOfficer = _context.TBL_STAFF.Where(x => x.MISCODE.ToLower() == accountOfficerMIsCode.ToLower()).FirstOrDefault();
            if (accountOfficer != null)
            {
                emailList = accountOfficer.EMAIL;
                if (accountOfficer.SUPERVISOR_STAFFID != null)
                {
                    var relationshipManager = _context.TBL_STAFF.Where(x => x.STAFFID == accountOfficer.SUPERVISOR_STAFFID).FirstOrDefault();
                    if (relationshipManager != null)
                    {
                        emailList = emailList + ";" + relationshipManager.EMAIL;
                        if (relationshipManager.SUPERVISOR_STAFFID != null)
                        {
                            var zonalHead = _context.TBL_STAFF.Where(x => x.STAFFID == relationshipManager.SUPERVISOR_STAFFID).FirstOrDefault();
                            if (zonalHead != null)
                            {
                                emailList = emailList + ";" + zonalHead.EMAIL;

                                var groupHead = _context.TBL_STAFF.Where(x => x.STAFFID == zonalHead.SUPERVISOR_STAFFID).FirstOrDefault();

                                if (groupHead != null)
                                {
                                    emailList = emailList + ";" + groupHead.EMAIL;
                                }
                            }
                        }
                    }
                }

            }

            return emailList;
        }
        public bool SubmitApproval(CallMemoViewModel model)
        {
            bool response = false;

            using (var transaction = _context.Database.BeginTransaction())
            {
                _workflow.StaffId = model.createdBy;
                _workflow.CompanyId = model.companyId;
                _workflow.StatusId = model.approvalStatusId == 3 ? (int) ApprovalStatusEnum.Disapproved : (int) ApprovalStatusEnum.Processing;
                _workflow.TargetId = model.callMemoId;
                _workflow.Comment = model.comment;
                _workflow.OperationId = (int) OperationsEnum.CallMemo;
                _workflow.DeferredExecution = true;
                _workflow.LogActivity();

                try
                {
                    if (_workflow.NewState == (int) ApprovalState.Ended)
                    {
                        var callMemos = _context.TBL_CALL_MEMO.Where(O => O.CALLMEMOID == model.callMemoId 
                                                && O.APPROVALSTATUSID == (int) ApprovalStatusEnum.Processing).Select(O => O).ToList();

                        foreach (var callMemo in callMemos)
                        {
                            callMemo.APPROVALSTATUSID = (int) ApprovalStatusEnum.Approved;
                        }
                    }

                    response = _context.SaveChanges() > 0;
                    transaction.Commit();
                    return response;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public IEnumerable<CallMemoViewModel> GetCallMemoWaitingForApproval(int staffId)
        {
            var ids = _genSetup.GetStaffApprovalLevelIds(staffId, (int) OperationsEnum.CallMemo).ToList();
            var staffs = _genSetup.GetStaffRlieved(staffId);
            var staffss = from s in _context.TBL_STAFF select s;
            var data = (from a in _context.TBL_CALL_MEMO
                        join c in _context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID
                        join atrail in _context.TBL_APPROVAL_TRAIL on a.CALLMEMOID equals atrail.TARGETID
                        where atrail.APPROVALSTATUSID == (int) ApprovalStatusEnum.Processing 
                        && atrail.RESPONSESTAFFID == null
                        && ids.Contains((int) atrail.TOAPPROVALLEVELID)
                        && (atrail.TOSTAFFID == null || staffs.Contains((int)atrail.TOSTAFFID))
                        && atrail.OPERATIONID == (int) OperationsEnum.CallMemo
                        orderby a.CALLMEMOID
                        select new CallMemoViewModel
                        {
                            callMemoId = a.CALLMEMOID,
                            loanApplicationId = a.LOANAPPLICATIONID,
                            staffId = a.STAFFID,
                            participants = a.PARTICIPANTS,
                            location = a.LOCATION,
                            customerName = c.FIRSTNAME + " " + c.LASTNAME,
                            customerId = c.CUSTOMERID,
                            memoDate = a.MEMODATE,
                            nextCallDate = a.NEXTCALLDATE,
                            purpose = a.PURPOSE,
                            discusion = a.DISCUSION,
                            action = a.ACTION,
                            background = a.BACKGROUND,
                            recentUpdate = a.RECENTUPDATE,
                            createdBy = a.CREATEDBY,
                            dateTimeCreated = a.DATECREATED,
                            operationId = (int)OperationsEnum.CallMemo,
                            toApprovalLevelName = atrail.TOAPPROVALLEVELID == null ? "N/A" : _context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                            fromApprovalLevelName = atrail.FROMAPPROVALLEVELID == null ? staffss.FirstOrDefault(r => r.STAFFID == atrail.REQUESTSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : _context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == atrail.FROMAPPROVALLEVELID).Select(a => a.LEVELNAME).FirstOrDefault(),
                            approvalStatusName = _context.TBL_APPROVAL_STATUS.Where(o => o.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(o => o.APPROVALSTATUSNAME).FirstOrDefault(),
                        }).ToList();
            return data;
        }
        #endregion
    }
}