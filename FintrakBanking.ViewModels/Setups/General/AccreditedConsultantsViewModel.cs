using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class AccreditedConsultantStateViewModel
    {
        public int accreditedConsultantStateCoveredID { get; set; }
        public int stateId { get; set; }
        public string stateName { get; set; }
        public int accreditedConsultantId { get; set; }
    }
    public class AccreditedConsultantsViewModel : GeneralEntity
    {
        public AccreditedConsultantsViewModel()
        {
            accreditedConsultantStates = new List<AccreditedConsultantStateViewModel>();
        }
        public int accreditedConsultantId { get; set; }
        public string registrationNumber { get; set; }
        public string name { get; set; }
        public string firmName { get; set; }
        public int? accreditedConsultantTypeId { get; set; }
        public short? cityId { get; set; }
        public string accountNumber { get; set; }
        public string solicitorBVN { get; set; }
        public short? countryId { get; set; }
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public string address { get; set; }
        public string coreCompetence { get; set; }
        public List<AccreditedConsultantStateViewModel> accreditedConsultantStates { get; set; }
    }
    public class AccreditedPrincipalsViewModel : GeneralEntity
    {
        public short principalsId { get; set; }
        public string principalsRegNumber { get; set; }
        public string name { get; set; }
        public short? cityId { get; set; }
        public string accountNumber { get; set; }
        public string principalsBVN { get; set; }
        public short? countryId { get; set; }
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public string address { get; set; }
    }
    public class AccreditedRecoveryAgentViewModel : GeneralEntity
    {
        public short recoveryAgentsId { get; set; }
        public string recoveryAgentsLicenceNumber { get; set; }
        public string name { get; set; }
        public short? cityId { get; set; }
        public string accountNumber { get; set; }
        public string agentBVN { get; set; }
        public short? countryId { get; set; }
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public string address { get; set; }
    }
    public class AccreditedAuditorsViewModel : GeneralEntity
    {
        public short auditorsId { get; set; }
        public string auditorsLicenceNumber { get; set; }
        public string name { get; set; }
        public short? cityId { get; set; }
        public short? countryId { get; set; }
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public string address { get; set; }
    }
    public class AccreditedConsultantTypeViewModel
    {
        public int accreditedConsultantTypeId { get; set; }
        public string name { get; set; }
    }
}
