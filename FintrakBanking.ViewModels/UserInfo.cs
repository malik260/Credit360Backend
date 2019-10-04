using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels
{
    public class UserInfo
    {
        public int companyId { get; set; }
        public int BranchId { get; set; }
        public int staffId { get; set; }
        public string applicationUrl { get; set; }
        public string userIPAddress { get; set; }
        public DateTime? SystemDateTime { get; set; }        
        public int createdBy { get; set; }
        public string userName { get; set; }
        public string passCode { get; set; }
        public List<string> activities { get; set; }
        public string companyName { get; set; }
        public string branchName { get; set; }
        public string staffName { get; set; }
        public DateTime? applicationDate { get; set; }
        public SessionStatusInfo sessionStatusInfo { get; set; }
        public DateTime? lastLoginDate { get; set; }
        public string staffRole { get; set; }
        public int staffRoleId { get; set; }
        public string businessUnitName { get; set; }
        public int corrMatrixId { get; set; }
        public string corrMatrixDescription { get; set; }
    }

}
