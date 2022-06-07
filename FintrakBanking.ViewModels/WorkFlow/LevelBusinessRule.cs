using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.WorkFlow
{
    public class LevelBusinessRule
    {
        public decimal? Amount { get; set; }
        public decimal? PepAmount { get; set; }
        public bool Pep { get; set; }
        public bool ProjectRelated { get; set; }
        public bool InsiderRelated { get; set; }
        public bool OnLending { get; set; }
        public bool InterventionFunds { get; set; }
        public bool OrrBasedApproval { get; set; }
        public bool WithInstruction { get; set; }
        public bool DomiciliationNotInPlace { get; set; }
        public bool isContingentFacility { get; set; }
        public bool isRevolvingFacility { get; set; }
        public bool isRenewal { get; set; }
        public bool esrm { get; set; }
        public int? tenor { get; set; }
        public bool excludeLevel { get; set; }
        public bool isAgricRelated { get; set; }
        public bool isSyndicated { get; set; }
    }
}
