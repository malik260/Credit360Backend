using FintrakBanking.Interfaces.Customer;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Customer;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Customer
{
    [Export(typeof(ILoanCovenantRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoanCovenantRepository : ILoanCovenantRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        //private int customerId;
        //int status = 0;

        public LoanCovenantRepository(IAuditTrailRepository _auditTrail,
                                    IGeneralSetupRepository _genSetup,
                                    FinTrakBankingContext _context)
        {
            this.context = _context;
            auditTrail = _auditTrail;
            this.genSetup = _genSetup;
        }

        #region LoanCovenantDetail
        public async Task<int> AddMultipleLoanCovenantDetail(List<LoanCovenantDetailViewModel> covenantModel)
        {
            if (covenantModel.Count <= 0)
                return -1;

            foreach (LoanCovenantDetailViewModel entity in covenantModel)
            {
                await AddLoanCovenantDetail(entity);
            }

            return 1;

        }
        public async  Task<bool> AddLoanCovenantDetail(LoanCovenantDetailViewModel entity)
        {
            var convenant = new tbl_Loan_Covenant_Detail
            {
                CompanyId = entity.companyId,
                CovenantAmount = entity.covenantAmount,
                CovenantDate = entity.covenantDate,
                CovenantDetail = entity.covenantDetail,
                CovenantTypeId = entity.covenantTypeId,
                CreatedBy = entity.createdBy,
                DateTimeCreated = this.genSetup.GetApplicationDate().Date,
                FrequencyTypeId = entity.frequencyTypeId,
                LoanId = entity.loanId
            };
            context.tbl_Loan_Covenant_Detail.Add(convenant);
            var loanRef = context.tbl_Loan.SingleOrDefault(c => c.LoanId == entity.loanId).LoanReferenceNumber;
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanCovenantDetailAdd,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Added loan convent to loan ref: { loanRef } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return await context.SaveChangesAsync() != 0;
        }

        public async  Task<bool> DeleteLoanCovenantDetail(int loanCovenantDetailId, UserInfo user)
        {
            var convenant = context.tbl_Loan_Covenant_Detail.Find(loanCovenantDetailId);
            convenant.Deleted = true;
            convenant.DeletedBy = user.staffId;
            convenant.DateTimeDeleted = this.genSetup.GetApplicationDate().Date; 

            var loanRef = context.tbl_Loan.SingleOrDefault(c => c.LoanId == loanCovenantDetailId).LoanReferenceNumber;
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanCovenantDetailDelete,
                StaffId = user.createdBy,
                BranchId = (short)user.BranchId,
                Detail = $"Delete loan convent to loan ref: { loanRef } ",
                IPAddress = user.userIPAddress,
                Url = user.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return await context.SaveChangesAsync() != 0;
        }

        private IEnumerable<LoanCovenantDetailViewModel>  LoanCovenantDetail(int companyId)
        {
           return context.tbl_Loan_Covenant_Detail.Where(c => c.CompanyId == companyId).Select(c => new LoanCovenantDetailViewModel
            {
                covenantAmount = c.CovenantAmount,
                companyId = c.CompanyId,
                covenantDate = c.CovenantDate,
                covenantDetail = c.CovenantDetail,
                covenantTypeId = c.CovenantTypeId,
                covenantTypeName = c.tbl_Loan_Covenant_Type.CovenantTypeName,
                frequencyTypeId = c.FrequencyTypeId,
                frequencyTypeName = c.tbl_Frequency_Type.Mode,
                loanCovenantDetailId = c.LoanCovenantDetailId,
                loanId = c.LoanId,
                loanRef = c.tbl_Loan.LoanReferenceNumber,
                productName = c.tbl_Loan.tbl_Product.ProductName
            });
        }

        //public IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetail(int companyId)
        //{
        //    return LoanCovenantDetail(companyId);
        //}

        public IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetailByCovenantType(int covenantTypeId, int companyId)
        {
            return LoanCovenantDetail(companyId).Where(c => c.covenantTypeId == covenantTypeId);
        }

        public IEnumerable<LoanCovenantDetailViewModel> GetLoanCovenantDetailByloanId(int loanId, int companyId)
        {
            return LoanCovenantDetail(companyId).Where(c => c.loanId == loanId);
        }
        
        public async  Task<bool> UpdateLoanCovenantDetail(int id ,LoanCovenantDetailViewModel entity)
        {
            var convenant = context.tbl_Loan_Covenant_Detail.Find(id);

            convenant.CompanyId = entity.companyId;
            convenant.CovenantAmount = entity.covenantAmount;
            convenant.CovenantDate = entity.covenantDate;
            convenant.CovenantDetail = entity.covenantDetail;
            convenant.CovenantTypeId = entity.covenantTypeId;
            convenant.CreatedBy = entity.createdBy;
            convenant.DateTimeUpdated = this.genSetup.GetApplicationDate().Date;
            convenant.FrequencyTypeId = entity.frequencyTypeId;
            convenant.LoanId = entity.loanId;

            var loanRef = context.tbl_Loan.SingleOrDefault(c => c.LoanId == entity.loanId).LoanReferenceNumber;
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanCovenantDetailUpdate,
                StaffId = entity.lastUpdatedBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated loan convent to loan ref: { loanRef } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            return await context.SaveChangesAsync() != 0;
        }
        
        public IEnumerable<LoanCovenantTypeViewModel> GetLoanCovenantDetailById(int covenantDetailId, int companyId)
        {
            return LoanCovenantType(companyId).Where(c => c.covenantTypeId == covenantDetailId);
        }
        #endregion LoanCovenantDetail

        #region Loan Covenant Type
        public async  Task<bool> AddLoanCovenantType(LoanCovenantTypeViewModel entity)
        {
            var convenant = new tbl_Loan_Covenant_Type
            {
                CompanyId = entity.companyId,
                CovenantTypeName = entity.covenantTypeName,
                RequireAmount = entity.requireAmount,
                RequireFrequency = entity.requireFrequency 
            };
            context.tbl_Loan_Covenant_Type.Add(convenant);
        
            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanCovenantTypeAdd ,
                StaffId = entity.createdBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Defined loan convent type: { entity.covenantTypeName } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            return await context.SaveChangesAsync() != 0;
        }
        
        public async Task<bool> UpdateLoanCovenantType(short id, LoanCovenantTypeViewModel entity)
        {
            var convenant = context.tbl_Loan_Covenant_Type.Find(id);
            convenant.CompanyId = entity.companyId;
            convenant.CovenantTypeName = entity.covenantTypeName;
            convenant.RequireAmount = entity.requireAmount;
            convenant.RequireFrequency = entity.requireFrequency;

            var audit = new tbl_Audit
            {
                AuditTypeId = (short)AuditTypeEnum.LoanCovenantTypeAdd,
                StaffId = entity.lastUpdatedBy,
                BranchId = (short)entity.userBranchId,
                Detail = $"Updated loan convent type: { entity.covenantTypeName } ",
                IPAddress = entity.userIPAddress,
                Url = entity.applicationUrl,
                ApplicationDate = genSetup.GetApplicationDate(),
                SystemDateTime = DateTime.Now
            };

            //this.auditTrail.AddAuditTrail(audit);
            return await context.SaveChangesAsync() != 0;
        }

        IEnumerable<LoanCovenantTypeViewModel>  LoanCovenantType(int companyId)
        {
            return context.tbl_Loan_Covenant_Type.Where(c => c.CompanyId == companyId).Select(c => new LoanCovenantTypeViewModel
            {
                companyId = c.CompanyId,
                covenantTypeId = c.CovenantTypeId,
                covenantTypeName = c.CovenantTypeName,
                requireAmount = c.RequireAmount,
                requireFrequency = c.RequireFrequency
            });
        }

        public IEnumerable<LoanCovenantTypeViewModel> GetLoanCovenantType(int companyId)
        {
            return LoanCovenantType(companyId);
        }

        #endregion Loan Covenant Type
    }
}
