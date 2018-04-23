using FintrakBanking.ViewModels.Setups.General;
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
        IEnumerable<AccreditedConsultantsViewModel> GetAccreditedStateConsultantsByStateId(int companyId, int stateId);
        IEnumerable<AccreditedConsultantTypeViewModel> GetAccreditedConsultantType();
        IEnumerable<AccreditedConsultantsViewModel> GetAccreditedConsultants(int companyId);
        Task<bool> AddAccreditedConsultants(AccreditedConsultantsViewModel entity);
        Task<bool> UpdateAccreditedConsultants(AccreditedConsultantsViewModel entity, int id);
        Task<bool> DeleteAccreditedConsultantStates(int id);
        ////Principal
        //IEnumerable<AccreditedPrincipalsViewModel> GetAccreditedPrincipals(int companyId);
        //Task<bool> AddAccreditedPrincipals(AccreditedPrincipalsViewModel entity);
        //Task<bool> UpdateAccreditedPrincipals(AccreditedPrincipalsViewModel entity, int id);

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
