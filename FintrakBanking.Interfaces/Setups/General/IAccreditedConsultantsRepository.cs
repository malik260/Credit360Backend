using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
   public interface IAccreditedConsultantsRepository
    {
        //Solicitor
        IEnumerable<AccreditedConsultantsViewModel> GetSearchedAgent(string search);
        IEnumerable<AccreditedConsultantsViewModel> GetAccreditedConsultants(int companyId);
        IEnumerable<AccreditedConsultantsViewModel> GetAccreditedStateConsultants(int getCompanyId);
        IEnumerable<AccreditedConsultantsViewModel> GetAccreditedStateConsultantsByStateId(int companyId, int stateId);
        IEnumerable<AccreditedConsultantTypeViewModel> GetAccreditedConsultantType();
        IEnumerable<AccreditedConsultantsViewModel> GetAccreditedConsultants(int companyId, int accreditedConsultantId);
        bool AddConsultantType(AccreditedConsultantTypeViewModel entity);

        Task<AccreditedConsultantsViewModel> AddAccreditedConsultants(AccreditedConsultantsViewModel entity);
        Task<bool> UpdateAccreditedConsultants(AccreditedConsultantsViewModel entity, int id);
        Task<bool> DeleteAccreditedConsultantStates(int id);
        IEnumerable<AccreditedConsultantsViewModel> GetAccreditedSolicitorsAwaitingApprovals(int staffId, int companyId, int accreditedConsultantId);
        int GoForApproval(ApprovalViewModel entity);

        ////Principal
        IEnumerable<AccreditedPrincipalsViewModel> GetAccreditedPrincipals(int companyId);
        Task<bool> AddAccreditedPrincipals(AccreditedPrincipalsViewModel entity);
        Task<bool> UpdateAccreditedPrincipals(AccreditedPrincipalsViewModel entity, int id);
        bool AddLoanConsultant(LoanConsultantViewModel entity);
        bool EditLoanConsultant(int id, LoanConsultantViewModel entity);
        bool RemoveLoanConsultant(int id, UserInfo user);
        List<LoanConsultantViewModel> GetLoanConsultant(int applicationId);

        ////Recovery Agent
        //IEnumerable<AccreditedRecoveryAgentViewModel> GetAccreditedRecoveryAgent(int companyId);
        //Task<bool> AddAccreditedRecoveryAgents(AccreditedRecoveryAgentViewModel entity);
        //Task<bool> UpdateAccreditedRecoveryAgent(AccreditedRecoveryAgentViewModel entity, int id);

        ////Auditor
        //IEnumerable<AccreditedAuditorsViewModel> GetAccreditedAuditors(int companyId);
        //Task<bool> AddAccreditedAuditors(AccreditedAuditorsViewModel entity);
        //Task<bool> UpdateAccreditedAuditors(AccreditedAuditorsViewModel entity, int id);
    }
}
