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
            decimal CallLimit = 0;
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchQuery.Trim()))
            {
                var JobRole = (from a in _context.tbl_Staff where a.StaffId == staffId select a.JobTitleId).FirstOrDefault();
                if (JobRole > 0)
                {
                    CallLimit = (from b in _context.tbl_Call_Memo_Limit where b.JobTitleId == JobRole && b.CallLimitTypeId == 1 select b.MaximumAmount).FirstOrDefault();
                }
                allFilteredLoan = (from a in _context.tbl_Loan_Application
                                   join b in _context.tbl_Customer on a.CustomerId equals b.CustomerId
                                   where a.PrincipalAmount <= CallLimit && (a.ApplicationReferenceNumber.Contains(searchQuery) ||
                                   b.CustomerCode.ToLower().Contains(searchQuery))
                                   select new CallMemoLoanSearchViewModel
                                   {
                                       loanApplicationId = a.LoanApplicationId,
                                       customerId = a.CustomerId,
                                       customerName = b.CustomerCode + " - " + b.FirstName + " " + b.LastName,
                                       loanReferenceNo = a.ApplicationReferenceNumber,
                                       principalAmount = a.PrincipalAmount
                                   }).Take(10).AsQueryable();
            }

            return allFilteredLoan;
        }
        #region "Call Limit"

        public IEnumerable<CallMemoTypeViewModel> GetCallLimitType()
        {
            var data = (from a in _context.tbl_Call_Memo_Type
                        orderby a.Name
                        select new CallMemoTypeViewModel
                        {
                            CallLimitTypeId = a.CallLimitTypeId,
                            Name = a.Name
                        }).ToList();
            return data;
        }
        public IEnumerable<CallLimitViewModel> GetAllCallLimit(int companyId)
        {
            var data = (from a in _context.tbl_Call_Memo_Limit
                        where a.Deleted == false && a.CompanyId == companyId
                        orderby a.CallLimitTypeId
                        select new CallLimitViewModel
                        {
                            MaximumAmount = a.MaximumAmount,
                            MinimumAmount = a.MinimumAmount,
                            CallLimitId = a.CallLimitId,
                            companyId = a.CompanyId,
                            FrequencyId = a.FrequencyId,
                            FrequencyName = a.tbl_Frequency_Type.Mode,
                            JobTitleId = a.JobTitleId,
                            JobTitleName = _context.tbl_Staff_JobTitle.FirstOrDefault(d => d.JobTitleId == a.JobTitleId).JobTitleName,
                            CallLimitTypeId = a.CallLimitTypeId,
                            CallLimitTypeName = _context.tbl_Call_Memo_Type.FirstOrDefault(i=>i.CallLimitTypeId == a.CallLimitTypeId).Name
                        }).ToList();
            return data;
        }

        public List<CallLimitViewModel> GetCallLimitByTypeId(int limitId)
        {
            var data = (from a in _context.tbl_Call_Memo_Limit
                        where a.Deleted == false && a.CallLimitId == limitId
                        orderby a.CallLimitTypeId
                        select new CallLimitViewModel
                        {
                            MaximumAmount = a.MaximumAmount,
                            MinimumAmount = a.MinimumAmount,
                            CallLimitId = a.CallLimitId,
                            companyId = a.CompanyId,
                            FrequencyId = a.FrequencyId,
                            FrequencyName = a.tbl_Frequency_Type.Mode,
                            JobTitleId = a.JobTitleId,
                            JobTitleName = _context.tbl_Staff_JobTitle.FirstOrDefault(d => d.JobTitleId == a.JobTitleId).JobTitleName,
                            CallLimitTypeId = a.CallLimitTypeId,
                            CallLimitTypeName = _context.tbl_Call_Memo_Type.FirstOrDefault(i => i.CallLimitTypeId == a.CallLimitTypeId).Name
                        }).ToList();
            return data;
        }
        public bool isLimitExist(CallLimitViewModel model)
        {
            return _context.tbl_Call_Memo_Limit.Where(x => x.JobTitleId == model.JobTitleId && x.CallLimitTypeId == model.CallLimitTypeId).Any();
        }
        public bool AddCallLimit(CallLimitViewModel model)
        {
            var data = new tbl_Call_Memo_Limit
            {
                MaximumAmount = model.MaximumAmount,
                MinimumAmount = model.MinimumAmount,
                FrequencyId = model.FrequencyId,
                JobTitleId = model.JobTitleId,
                CallLimitTypeId = model.CallLimitTypeId,
                CompanyId = model.companyId,
                CreatedBy = model.createdBy,
                DateTimeCreated = _genSetup.GetApplicationDate()
            };

            _context.tbl_Call_Memo_Limit.Add(data);

            // Audit Section ---------------------------

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitAdded,
                StaffId = model.createdBy,
                BranchId = model.userBranchId,
                Detail = $"Added tbl_Call_Limit '{ data.CallLimitId }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool UpdateCallLimit(int limitId, CallLimitViewModel model)
        {
            var data = _context.tbl_Call_Memo_Limit.Find(limitId);
            if (data == null) return false;

            data.MaximumAmount = model.MaximumAmount;
            data.MinimumAmount = model.MinimumAmount;
            data.CallLimitTypeId = model.CallLimitTypeId;
            data.FrequencyId = model.FrequencyId;
            data.JobTitleId = model.JobTitleId;
            // Audit Section ---------------------------

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitUpdated,
                StaffId = model.createdBy,
                BranchId = model.userBranchId,
                Detail = $"Updated tbl_Call_Limit : '{ data.CallLimitId }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool DeleteCallLimit(int limitId, UserInfo user)
        {
            var data = _context.tbl_Call_Memo_Limit.Find(limitId);
            if (data != null)
            {
                data.Deleted = true;
                data.DeletedBy = user.staffId;
                data.DateTimeDeleted = _genSetup.GetApplicationDate();
            }

            // Audit Section ---------------------------
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitDeleted,
                StaffId = user.staffId,
                BranchId = (short)user.BranchId,
                Detail = $"Deleted tbl_Call_Limit with Id : '{ limitId }' ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }
        #endregion

        #region "Call Memo"
        public IEnumerable<CallMemoViewModel> GetAllCallMemo(int staffId)
        {
            var data = (from a in _context.tbl_Call_Memo
                        join b in _context.tbl_Loan_Application on a.LoanApplicationId equals b.LoanApplicationId
                        where a.StaffId == staffId
                        orderby a.CallMemoId
                        select new CallMemoViewModel
                        {
                            CallMemoId = a.CallMemoId,
                            LoanApplicationId = a.LoanApplicationId,
                            LoanReferenceNo = b.ApplicationReferenceNumber,
                            StaffId = a.StaffId,
                            CallMemoTypeId = a.CallLimitTypeId,
                            CallMemoType = a.tbl_Call_Memo_Type.Name,
                            CustomerName = _context.tbl_Customer.FirstOrDefault(x=>x.CustomerId == b.CustomerId).FirstName,
                            MemoDate = a.MemoDate,
                            NextCallDate = a.NextCallDate,
                            Purpose = a.Purpose,
                            Discusion = a.Discusion,
                            Summary = a.Summary,
                            Action = a.Action,
                            Recommendation = a.Recommendation,
                            createdBy = a.CreatedBy,
                            dateTimeCreated = a.DateCreated
                        }).ToList();
            return data;
        }
        public bool AddCallMemo(CallMemoViewModel model)
        {
            var data = new tbl_Call_Memo
            {
                LoanApplicationId = model.LoanApplicationId,
                StaffId = model.StaffId,
                MemoDate = model.MemoDate,
                NextCallDate = model.NextCallDate,
                Purpose = model.Purpose,
                CallLimitTypeId = model.CallMemoTypeId,
                Discusion = model.Discusion,
                Summary = model.Summary,
                Action = model.Action,
                Recommendation = model.Recommendation,
                CreatedBy = model.createdBy,
                DateCreated = _genSetup.GetApplicationDate()
            };
            _context.tbl_Call_Memo.Add(data);

            // Audit Section ---------------------------

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitAdded,
                StaffId = model.createdBy,
                BranchId = model.userBranchId,
                Detail = $"Added Call Memo for: '{ data.Purpose }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }

        public bool UpdateCallMemo(int limitId, CallMemoViewModel model)
        {
            var data = _context.tbl_Call_Memo.Find(limitId);
            if (data == null) return false;
            data.CallLimitTypeId = model.CallMemoTypeId;
            data.LoanApplicationId = model.LoanApplicationId;
            data.StaffId = model.StaffId;
            data.MemoDate = model.MemoDate;
            data.NextCallDate = model.NextCallDate;
            data.Purpose = model.Purpose;
            data.Discusion = model.Discusion;
            data.Summary = model.Summary;
            data.Action = model.Action;
            data.Recommendation = model.Recommendation;
            // Audit Section ---------------------------

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LimitUpdated,
                StaffId = model.createdBy,
                BranchId = model.userBranchId,
                Detail = $"Updated tbl_Call_Limit for data with Id : '{ data.CallMemoId }' ",
                IPAddress = model.userIPAddress,
                Url = model.applicationUrl,
                ApplicationDate = _genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            _auditTrail.AddAuditTrail(audit);

            //end of Audit section -----------------------
            return _context.SaveChanges() != 0;
        }
        #endregion
    }
}
