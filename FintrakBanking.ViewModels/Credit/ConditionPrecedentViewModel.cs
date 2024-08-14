using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class ConditionPrecedentViewModel : GeneralEntity
    {
        public int? sectorId { get; set; }
        public int? subSectorId { get; set; }

        public int conditionId { get; set; }
        public string condition { get; set; }
        public bool isExternal { get; set; }
        public int customerId { get; set; }
        public string loanApplicationReferenceNumber { get; set; }
        public bool corporate { get; set; }
        public bool retail { get; set; }
        public short? productId { get; set; }
        public int loanApplicationId { get; set; }
        public string staffName { get; set; }
        public bool isSubsequent { get; set; }
        public int loanApplicationDetailId { get; set; }
        public short responseTypeId { get; set; }
        public short? checkListStatusId { get; set; }
        public bool? checkListValidated { get; set; }
        public int approvalTrailId { get; set; }
        public int? loopedStaffId { get; set; }
        public short? approvalStatusId { get; set; }
        public string approvalStatus { get; set; }
        public bool isAvailment { get; set; }
        public DateTime? deferedDate { get; set; }
        public int? deferedDays { get; set; }
        public string reason { get; set; }
        public string status { get; set; }
        public int? timelineId { get; set; }
        public bool? validationStatus { get; set; }
        public string product { get; set; }
        public int loanConditionId { get; set; }
        public bool isLMSChecklist { get; set; }
        public int checklistDifinitionId { get; set; }
        public int? operationId { get; set; }
        public bool? isCheckListSpecific { get; set; }
        public string comment { get; set; }
        public bool? excludeLegal { get; set; }
        public bool isDocument { get; set; }
        public string responseMessage { get; set; }
    }

    public class TransactionDynamicsViewModel : GeneralEntity
    {
        public int? dynamicsId { get; set; }
        public string dynamics { get; set; }
        public short productId { get; set; }
        public int loanApplicationId { get; set; }
        public string staffName { get; set; }
        public int loanApplicationDetailId { get; set; }
        public short? responseTypeId { get; set; }
        public short? checkListStatusId1 { get; set; }
        public short? checkListStatusId2 { get; set; }
        public short? approvalStatusId { get; set; }
        public bool isAvailment { get; set; }
        public DateTime? deferedDate { get; set; }
        public string reason { get; set; }
        public string status { get; set; }
        public string productName { get; set; }
        public int loanDynamicsId { get; set; }
        public double SN { get; set; }
        public int? position { get; set; }
        public bool? isExternal { get; set; }
        public int? operationId { get; set; }
        public bool? isCheckListSpecific { get; set; }
    }

    public class ComplianceTimelineViewModel : GeneralEntity
    {
        public int timelineId { get; set; }
        public string timeline { get; set; }
        public int? duration { get; set; }

    }

    public class SelectedIdsViewModel : GeneralEntity
    {
        public int id { get; set; }
        public List<int> selectedIds { get; set; }
        public int detailId { get; set; }
    }

    public class AdditionalCommentViewModel : GeneralEntity
    {
        public int id { get; set; }
        public int callerId { get; set; }
        public int applicationId { get; set; }
        public string additionalComment { get; set; }
        public bool owner { get; set; }
    }

    public class SuggestedConditionsViewModel : GeneralEntity
    {
        public int suggestionid { get; set; }
        public int loanApplicationDetailId { get; set; }
        public int suggestionTypeId { get; set; }
        public string description { get; set; }
        //public int applicationId { get; set; }
    }
    
}

