using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class GroupViewModel
    {
        public GroupViewModel()
        {
            Activities = new List<ActivityViewModel>();
        }

        public short groupId { get; set; }
        public string groupName { get; set; }
        public int createdBy { get; set; }

        public List<ActivityViewModel> Activities { get; set; }
    }

    public class GroupModel
    {
        public short groupId { get; set; }
        public string groupName { get; set; }
        public int createdBy { get; set; }
       
    }
}