using FintrakBanking.ViewModels.WorkFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class StaffAccountHistoryViewModel : GeneralEntity
    {
        public short accountTypeId { get; set; }

        public string  productType { get; set; }

        public int targetId { get; set; }

        public int staffAccountHistoryId { get; set; }

        public int currentRMStaffId { get; set; }

        public DateTime startDate { get; set; }

        public DateTime endDate { get; set; }

        public int newRMStaffId { get; set; }

        public string reasonForChange { get; set; }
        public string loanReferneceNumber { get; set; }

        public short approvalStatusId { get; set; }

        public string newRMStaffName { get; set; }

        public string currentRMStaffName { get; set; }


        public string field1 { get; set; }
                      
        public string field2 { get; set; }
                   
        public string field3 { get; set; }
                 
        public string field4 { get; set; }
                      
        public string field5 { get; set; }
                    
        public string field6 { get; set; }
              
        public string field7 { get; set; }
                 
        public string field8 { get; set; }
                      
        public string field9 { get; set; }
                    
        public string field10 { get; set; }
    }

    public class StaffMISHistoryViewModel : LoanViewModel
    {
         

        public string productType { get; set; }

        public int targetId { get; set; }

        public int staffAccountHistoryId { get; set; }

        public int currentRMStaffId { get; set; }

        public DateTime startDate { get; set; }

        public DateTime endDate { get; set; }

        public int newRMStaffId { get; set; }

        public string reasonForChange { get; set; } 

        public string newRMStaffName { get; set; }

        public string currentRMStaffName { get; set; }



    }

    public class ReasignedAccountApprovalViewModel : ApprovalViewModel
    {
        public short loanSystemTypeId { get; set; }
        public int loanId { get; set; }
        public int newRMStaffId { get; set; }
        public int staffAccountHistoryId { get; set; }


    }

  

}
