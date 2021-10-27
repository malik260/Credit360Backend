using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class AlertTitleViewModel : GeneralEntity
    {
        public string levelGroupName { get; set; }
        public string levelCode { get; set; }
        public string templateTypeName { get; set; }

        public int alertTitleId { get; set; }
        public string title { get; set; }
        public string template { get; set; }
        public string businessOwner { get; set; }
        public string senderName { get; set; }
        public string senderEmail { get; set; }
        public string templateType { get; set; }
        public string defaultEmail { get; set; }
        public string bindingMethod { get; set; }
        public DateTime? lastSentDate { get; set; }
        public int? actionStatus { get; set; }
        public bool isActive { get; set; }
        public int? frequency { get; set; }
        public string alertTime { get; set; }
        public string timeFrom { get; set; }
        public string timeTo { get; set; }
        public int? alertScheduleId { get; set; }
        public int? frequencyId { get; set; }
        public int bindingMethodId { get; set; }
        public int? alertFrequencyId { get; set; }
        public string userActivities { get; set; }

        //public DateTime lastSentDate { get; set; }
        //public int source { get; set; }
    }
    public class AlertSetupViewModel : GeneralEntity
    {
        public string title { get; set; }
        public string levelGroupName { get; set; }
        public string levelCode { get; set; }
        public string operationName { get; set; }
        public string formular { get; set; }

        public int alertSetupId { get; set; }
        public int titleId { get; set; }
        public int levelGroupId { get; set; }
        public short frequencyId { get; set; }
        public short conditionId { get; set; }
    }


    public class LevelGroupMappingViewModel : GeneralEntity
    {
        public string levelGroupName { get; set; }
        public int alertLevelGroupMapId { get; set; }
        public int levelGroupId { get; set; }
        public string levelCode { get; set; }
    }

    public class AlertLevelGroupViewModel : GeneralEntity
    {
        public int alertLevelGroupId { get; set; }
        public string levelGroupName { get; set; }
        public string description { get; set; }
    }

    public class AlertLevelViewModel : GeneralEntity
    {
        public string levelGroupName { get; set; }
        public int alertStaffRoleId { get; set; }
        public int alertTitleId { get; set; }
        public int staffRoleId { get; set; }
        public string title { get; set; }
        public string staffRoleCode { get; set; }
        public string staffRoleName { get; set; }

        public int groupEmailId { get; set; }
        public string groupCode { get; set; }
        public string groupName { get; set; }
        public string groupEmail { get; set; }
    }

    public class AlertsViewModel : GeneralEntity
    {
        public AlertsViewModel()
        {
            receiverEmailList = new List<string>();
        }

        public string alertTitle { get; set; }
        public string frequencyMode { get; set; }
        public List<string> receiverEmailList { get; set; }
        public string template { get; set; }
        public bool canFire { get; set; }
        public int alertTitleId { get; set; }
        public string accountOfficerEmail { get; set; }
        public string operationMethod { get; set; }
    }

    public class AlertMisViewModel : GeneralEntity
    {
        public string profitCenterDefinitionCode { get; set; }
        public string profitCenterMisCode { get; set; }
        public int userMisId { get; set; }
        public string loginId { get; set; }
    }

    public class AlertFrequencyViewModel : GeneralEntity
    {
        public int alertFrequencyId { get; set; }
        public string frequencyMode { get; set; }
        public string description { get; set; }
    }

    public class AlertConditionViewModel : GeneralEntity
    {
        public string titleName { get; set; }
        public string operationName { get; set; }

        public int alertConditionId { get; set; }
        public string formular { get; set; }
        public DateTime lastRunDate { get; set; }
        public DateTime nextRunDate { get; set; }
        //public DateTime lastRunTime { get; set; }
        public short? operationId { get; set; }
        public string triggerSource { get; set; }
        public string type { get; set; }
        public short? alertInterval { get; set; }
        public int? title { get; set; }
        public string actionForTrigger { get; set; }

    }

    public class TblOperationsViewModel 
    {
        public int operationId { get; set; }
        public string operationName { get; set; }
    }

    public class GeneralTempateViewModel
    {
        public int templateId { get; set; }
        public string templateBody { get; set; }
    }

    public class AlertPlaceHoldersViewModel : GeneralEntity
    {
        public short placeHolderId { get; set; }
        public string placeHolder { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public int? alertTitleId { get; set; }
        public string partition { get; set; }
        public object title { get; set; }
    }

    public class AlertScheduleViewModel : GeneralEntity
    {
        public int alertScheduleId { get; set; }
        public int frequencyId { get; set; }
        public string alertTime { get; set; }
        public string timeFrom { get; set; }
        public string timeTo { get; set; }
        public int alertTitleId { get; set; }
    }

    public class AlertBindingMethodsViewModel : GeneralEntity
    {
        public int bindingMethodId { get; set; }
        public string methodName { get; set; }
        public string methodTitle { get; set; }
    }

}
