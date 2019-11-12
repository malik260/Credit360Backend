using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Approval
{
    public class ApprovalReliefViewModel : GeneralEntity
    {
        public int reliefId { get; set; }
        public int relievedStaffId { get; set; }
        public int reliefStaffId { get; set; }
        public string staffName { get; set; }
        public string reliefStaffName { get; set; }
        public string reliefReason { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public bool isActive { get; set; }
        public string approvedBy { get; set; }
    }
}
