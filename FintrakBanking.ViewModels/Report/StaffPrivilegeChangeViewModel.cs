using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Report
{
    public class StaffPrivilegeChangeViewModel
    {
        public string previousStaffRoleName { get; set; }
        public string currentStaffRoleName { get; set; }

        public string staffCreatedByName { get; set; }

    

        public string tempStaffCode { get; set; }

        
        public DateTime dateTimeCreated { get; set; }
     

        public string staffFullName { get; set; }
      
    }
}
