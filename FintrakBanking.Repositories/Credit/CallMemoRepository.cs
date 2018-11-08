using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
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

        public CallMemoRepository(FinTrakBankingContext context, IGeneralSetupRepository genSetup,
                                IAuditTrailRepository auditTrail)
        {
            _context = context;
            _genSetup = genSetup;
            _auditTrail = auditTrail;
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

                var JobRole = (from a in _context.TBL_STAFF where a.STAFFID == staffId select a.JOBTITLEID).FirstOrDefault();
                if (JobRole > 0)
                {
                    var memoLimit = (from b in _context.TBL_CALL_MEMO_LIMIT where b.JOBTITLEID == JobRole && b.CALLLIMITTYPEID == 1 select b).FirstOrDefault();
                    if (memoLimit != null)
                    {
                        allFilteredLoan = (from a in _context.TBL_LOAN_APPLICATION
                                           join b in _context.TBL_CUSTOMER on a.CUSTOMERID equals b.CUSTOMERID
                                           where a.APPLICATIONAMOUNT <= memoLimit.MAXIMUMAMOUNT && a.APPLICATIONAMOUNT >= memoLimit.MINIMUMAMOUNT && (a.APPLICATIONREFERENCENUMBER.Contains(searchQuery) ||
                                           b.CUSTOMERCODE.ToLower().Contains(searchQuery))
                                           select new CallMemoLoanSearchViewModel
                                           {
                                               loanApplicationId = a.LOANAPPLICATIONID,
                                               customerId = a.CUSTOMERID,
                                               customerName = b.CUSTOMERCODE + " - " + b.FIRSTNAME + " " + b.LASTNAME,
                                               loanReferenceNo = a.APPLICATIONREFERENCENUMBER,
                                               principalAmount = a.APPLICATIONAMOUNT
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
                        orderby a.CALLLIMITTYPEID
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
                            CallLimitTypeId = a.CALLLIMITTYPEID,
                            CallLimitTypeName = _context.TBL_CALL_MEMO_TYPE.FirstOrDefault(i => i.CALLLIMITTYPEID == a.CALLLIMITTYPEID).NAME
                        }).ToList();
            return data;
        }

        public List<CallLimitViewModel> GetCallLimitByTypeId(int limitId)
        {
            var data = (from a in _context.TBL_CALL_MEMO_LIMIT
                        where a.DELETED == false && a.CALLLIMITID == limitId
                        orderby a.CALLLIMITTYPEID
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
                            CallLimitTypeId = a.CALLLIMITTYPEID,
                            CallLimitTypeName = _context.TBL_CALL_MEMO_TYPE.FirstOrDefault(i => i.CALLLIMITTYPEID == a.CALLLIMITTYPEID).NAME
                        }).ToList();
            return data;
        }
        public bool isLimitExist(CallLimitViewModel model)
        {
            return _context.TBL_CALL_MEMO_LIMIT.Where(x => x.JOBTITLEID == model.JobTitleId && x.CALLLIMITTYPEID == model.CallLimitTypeId).Any();
        }
        public bool AddCallLimit(CallLimitViewModel model)
        {
            var data = new TBL_CALL_MEMO_LIMIT
            {
                MAXIMUMAMOUNT = model.MaximumAmount,
                MINIMUMAMOUNT = model.MinimumAmount,
                FREQUENCYID = model.FrequencyId,
                JOBTITLEID = model.JobTitleId,
                CALLLIMITTYPEID = model.CallLimitTypeId,
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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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
            data.CALLLIMITTYPEID = model.CallLimitTypeId;
            data.FREQUENCYID = model.FrequencyId;
            data.JOBTITLEID = model.JobTitleId;
            // Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitUpdated,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Updated tbl_Call_Limit : '{ data.CALLLIMITID }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }
        #endregion

        #region "Call Memo"
        public IEnumerable<CallMemoViewModel> GetAllCallMemo(int staffId)
        {
            var data = (from a in _context.TBL_CALL_MEMO
                        join b in _context.TBL_LOAN_APPLICATION on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                        where a.STAFFID == staffId
                        orderby a.CALLMEMOID
                        select new CallMemoViewModel
                        {
                            CallMemoId = a.CALLMEMOID,
                            LoanApplicationId = a.LOANAPPLICATIONID,
                            LoanReferenceNo = b.APPLICATIONREFERENCENUMBER,
                            StaffId = a.STAFFID,
                            CallMemoTypeId = a.CALLLIMITTYPEID,
                            CallMemoType = a.TBL_CALL_MEMO_TYPE.NAME,
                            CustomerName = _context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == b.CUSTOMERID).FIRSTNAME,
                            MemoDate = a.MEMODATE,
                            NextCallDate = a.NEXTCALLDATE,
                            Purpose = a.PURPOSE,
                            Discusion = a.DISCUSION,
                            Summary = a.SUMMARY,
                            Action = a.ACTION,
                            Recommendation = a.RECOMMENDATION,
                            createdBy = a.CREATEDBY,
                            dateTimeCreated = a.DATECREATED
                        }).ToList();
            return data;
        }
        public bool AddCallMemo(CallMemoViewModel model)
        {
            var data = new TBL_CALL_MEMO
            {
                LOANAPPLICATIONID = model.LoanApplicationId,
                STAFFID = model.StaffId,
                MEMODATE = model.MemoDate,
                NEXTCALLDATE = model.NextCallDate,
                PURPOSE = model.Purpose,
                CALLLIMITTYPEID = model.CallMemoTypeId,
                DISCUSION = model.Discusion,
                SUMMARY = model.Summary,
                ACTION = model.Action,
                RECOMMENDATION = model.Recommendation,
                CREATEDBY = model.createdBy,
                DATECREATED = _genSetup.GetApplicationDate()
            };
            _context.TBL_CALL_MEMO.Add(data);

            // Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitAdded,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Added Call Memo for: '{ data.PURPOSE }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool UpdateCallMemo(int limitId, CallMemoViewModel model)
        {
            var data = _context.TBL_CALL_MEMO.Find(limitId);
            if (data == null) return false;
            data.CALLLIMITTYPEID = model.CallMemoTypeId;
            data.LOANAPPLICATIONID = model.LoanApplicationId;
            data.STAFFID = model.StaffId;
            data.MEMODATE = model.MemoDate;
            data.NEXTCALLDATE = model.NextCallDate;
            data.PURPOSE = model.Purpose;
            data.DISCUSION = model.Discusion;
            data.SUMMARY = model.Summary;
            data.ACTION = model.Action;
            data.RECOMMENDATION = model.Recommendation;
            // Audit Section ---------------------------

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LimitUpdated,
                STAFFID = model.createdBy,
                BRANCHID = model.userBranchId,
                DETAIL = $"Updated tbl_Call_Limit for data with Id : '{ data.CALLMEMOID }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }
        #endregion
    }
}