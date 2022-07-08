using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.WorkFlow
{
    public class BusinessRuleViewModel : GeneralEntity
    {
        public int levelBusinessRuleId { get; set; }
        public decimal? minimumAmount { get; set; }
        public decimal? maximumAmount { get; set; }
        public decimal? pepAmount { get; set; }
        public bool pep { get; set; }
        public bool projectRelated { get; set; }
        public bool insiderRelated { get; set; }
        public bool onLending { get; set; }
        public bool interventionFunds { get; set; }
        public bool orrBasedApproval { get; set; }
        public bool withoutInstruction { get; set; }
        public bool domiciliationNotInPlace { get; set; }
        public string description { get; set; }
        public bool isForContingentFacility { get; set; }
        public bool isForRevolvingFacility { get; set; }
        public bool exemptContingentFacility { get; set; }
        public bool exemptRevolvingFacility { get; set; }
        public bool isForRenewal { get; set; }
        public bool exemptRenewal { get; set; }
        public bool esrm { get; set; }
        public int? tenor { get; set; }
        public bool excludeLevel { get; set; }
        public bool isAgricRelated { get; set; }
        public bool isSyndicated { get; set; }

    }
}
