using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class StaffAccountHistoryViewModel : GeneralEntity
    {
        public short productTypeId { get; set; }

        public int targetId { get; set; }

        public int staffAccountHistoryId { get; set; }

        public int currentRMStaffId { get; set; }

        public DateTime startDate { get; set; }

        public DateTime endDate { get; set; }

        public int newRMStaffId { get; set; }

        public string reasonForChange { get; set; }

        public short approvalStatusId { get; set; }

        public string newRMStaffName { get; set; }

        public string currentRMStaffName { get; set; }


    }

    
 

    
}
