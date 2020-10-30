using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class CollateralSwapViewModel : GeneralEntity
    {
        public int collateralSwapId { get; set; }
        public int operationId { get; set; }
        public int loanCollateralMappingId { get; set; }
        public int loanAppCollateralId { get; set; }
        public int oldCollateralId { get; set; }
        public int newCollateralId { get; set; }
        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int? currentApprovalLevelId { get; set; }
        public string currentApprovalLevel { get; set; }
        public string fromLevel { get; set; }
        public string responsiblePerson { get; set; }
        public string productName { get; set; }
        public int? customerId { get; set; }
        public string swapRef { get; set; }
        public string applicationReferenceNumber { get; set; }
        public string oldCollateralCode { get; set; }
        public string newCollateralCode { get; set; }
        public int? approvalStatusId { get; set; }
        public int? collateralSwapStatusId { get; set; }
        public string collateralSwapStatus { get; set; }
        public decimal coverage { get; set; }
        public string customerName { get; set; }
        public int approvalTrailId { get; set; }
        public int? loopedStaffId { get; set; }
        public int forwardAction { get; set; }
        public string approvalStatus { get; set; }
        public string comment { get; set; }
        public short vote { get; set; }
        public int? toStaffId { get; set; }
        public int toRoleId { get; set; }
        public bool isBusiness { get; set; }
    }

    public class ExceptionalLoanViewModel : GeneralEntity
    {
       
        public int loanApplicationId { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int operationId { get; set; }

        public int? customerId { get; set; }
        
        public int? approvalStatusId { get; set; }
        
        public int forwardAction { get; set; }
        public string approvalStatus { get; set; }
        public string comment { get; set; }
        public short vote { get; set; }
    }
}
