using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class AlertTitleViewModel : GeneralEntity
    {
        public int alertTitleId { get; set; }
        public string title { get; set; }
        public string template { get; set; }      

    }
    public class AlertSetupViewModel : GeneralEntity
    {
        public int alertSetupId { get; set; }
        public int titleId { get; set; }
        public int levelGroupMappingId { get; set; }
        public short frequencyId { get; set; }
        public short conditionId { get; set; }
    }


    public class LevelGroupMappingViewModel : GeneralEntity
    {
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
        public string levelGroupName;

        public int alertLevelId { get; set; }
        public string emailList { get; set; }
        public string levelCode { get; set; }
        public short levelGroupId { get; set; }
    }

    public class AlertsViewModel : GeneralEntity
    {
        public string alertTitle { get; set; }
        public string frequencyMode { get; set; }
        public List<string> receiverEmailList { get; set; }
        public string template { get; set; }
        public bool canFire { get; set; }
        public int alertTitleId { get; set; }
    }

    public class AlertMisViewModel 
    {
        public string profitCenterDefinitionCode { get; set; }
        public string profitCenterMisCode { get; set; }
        public int userMisId { get; set; }
        public string loginId { get; set; }
    }


}
