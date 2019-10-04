using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class AlertViewModel : GeneralEntity
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
        public int frequencyId { get; set; }
        public int conditionId { get; set; }
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
        public int alertLevelId { get; set; }
        public string emailList { get; set; }
        public string levelCode { get; set; }
    }
}
