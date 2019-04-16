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
    }
}
