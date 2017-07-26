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
        public DateTime SystemDateTime { get; set; }        
        public int createdBy { get; set; }
    }

}
