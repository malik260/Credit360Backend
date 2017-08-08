using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.ViewModels.Credit;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using System.Linq;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.ViewModels.Business;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Credit
{ 
    public class CreditAssessmentMemoRepository : ICreditAssessmentMemoRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkFlowRepository workFlow;
        private IApprovalLevelStaffRepository level;
        private ILoanApplicationRepository loan;
        public CreditAssessmentMemoRepository(FinTrakBankingContext _context, IAuditTrailRepository _auditTrail,
           IGeneralSetupRepository _genSetup, IWorkFlowRepository _workFlow, ILoanApplicationRepository _loan, IApprovalLevelStaffRepository _level)
        {
            context = _context;
            auditTrail = _auditTrail;
            genSetup = _genSetup;
            workFlow = _workFlow;
            loan = _loan;
            level = _level;
        }

        public async Task<bool> AddCreditAssessmentMemo(int operationId, CreditAssessmentMemoViewModel entity)
        {
            bool  result ;
            var request = new tbl_Credit_Assessment_Memorandum
            {
                CAMDocumentation = entity.camdocumentation,
                CAMRef = entity.camref,
                CreatedBy = entity.createdBy,
                DateTimeCreated = entity.dateTimeCreated,
                IsSubmitted = true,
                IsProccessed = false
            };
            context.tbl_Credit_Assessment_Memorandum.Add(request);
            result = await context.SaveChangesAsync() > 0;
            workFlow.LogForApproval(new ViewModels.Business.ApprovalViewModel()
            {
                applicationUrl = entity.applicationUrl,
                approvalStatusId = (int)ApprovalStatusEnum.Pending,
                BranchId = entity.userBranchId,
                companyId = entity.companyId,  
                createdBy = entity.createdBy,
                staffId = entity.createdBy,
                targetId = request.AccessmentMemorandumId,
                userIPAddress = entity.userIPAddress,
                 operationId = operationId,
                SystemDateTime = DateTime.Now
            });
            return result;  
        }

        //public IEnumerable<LoanApplicationViewModel> GetRequestForCreditAssessmentMemo(int companyId, int branchId)
        //{
        //    return loan.GetAllLoanApplications(companyId)
        //        .Where(c => c.branchId == branchId && c.approvalStatusId == (int)ApprovalStatusEnum.Pending);
        //}
        /// <summary>
        /// This is use to get cam request further proccessing.
        /// </summary>
        /// <param name="companyId"> </param>
        /// <param name="branchId"></param>
        /// <param name="staffId"></param>
        /// <param name="operationId">request or operation type</param>
        /// <returns></returns>
        public IQueryable<LoanApplicationViewModel> GetRequestOnCreditAssessmentMemo(int companyId, int branchId, int staffId, int operationId)
        {
            int staffApprovalLevelId = level.GetAllApprovalLevelStaffByStaffId(staffId, companyId, operationId).staffLevelId;
            return (from c in context.tbl_Approval_Trail
                    join d in loan.GetAllLoanApplications(companyId) on c.TargetId equals d.loanApplicationId
                    join e in context.tbl_Credit_Assessment_Memorandum on d.loanApplicationId equals e.LoanApplicationId
                    where c.OperationId == operationId && e.IsSubmitted == true && e.IsProccessed == false && d.branchId == branchId
                    && c.ToApprovalLevelId == staffApprovalLevelId
                    select d).OrderBy(c=> c.applicationDate);
        }

        public bool SubmitRequestForProcessing(ApprovalViewModel entity)
        {
            bool result = false;
            var wf = workFlow.GoForApproval(entity).Result;
            if (wf.Item1)
            {
                result = wf.Item1;
            }
            return result;
        }

        public async Task<bool> AddAssessmentTempates(AssessmentTemplatesViewModel entity)
        {
            var template = new tbl_Credit_Template
            {
                CreditTemplate = entity.creditTemplate,
                CompanyId = entity.companyId,
                 TemplateTitle = entity.templateTitle ,
                CreatedBy = entity.createdBy,
                ProductClassId = entity.productClassId ,
                ApprovalLevelId = entity.approvalLevelId,
                DateTimeCreated = genSetup.GetApplicationDate()
            };
            context.tbl_Credit_Template.Add(template);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAssessmentTempates(AssessmentTemplatesViewModel entity)
        {
            var template = context.tbl_Credit_Template.Where(c => c.CreditTemplateId == entity.creditTemplateId).FirstOrDefault();
            {
                template.CreditTemplate = entity.creditTemplate;
                template.CompanyId = entity.companyId;
                template.TemplateTitle = entity.templateTitle;
                template.ProductClassId = entity.productClassId;
                template.LastUpdatedBy  = entity.createdBy;
                template.ApprovalLevelId = entity.approvalLevelId;
                template.DateTimeUpdated  = genSetup.GetApplicationDate ();
            };
        
            return await context.SaveChangesAsync() > 0;
        }
        
        public IEnumerable<AssessmentTemplatesViewModel> GetAssessmentTempates(int approvalLevelId, int productId, int companyId)
        {
            var template = context.tbl_Credit_Template.Where(c => c.CompanyId == companyId
            && c.ProductClassId == productId
            && c.ApprovalLevelId == approvalLevelId)
                .Select(c => new AssessmentTemplatesViewModel()
                {
                    creditTemplateId = c.CreditTemplateId,
                    creditTemplate = c.CreditTemplate,
                    companyId = c.CompanyId,
                    productClassId = c.ProductClassId,
                    templateTitle = c.TemplateTitle,
                    createdBy = c.CreatedBy,
                    approvalLevelId = (int)c.ApprovalLevelId,
                    dateTimeCreated = c.DateTimeCreated
                }).ToList();
            return template;
        }

        
    }
}
