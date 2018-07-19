using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IAuthorizationRepository
    {
        Task<bool> AddGroup(GroupModel groupModel);

        Task<bool> DeleteGroup(short groupId);

        Task<bool> UpdateGroup(short groupId, GroupViewModel groupModel);

        IEnumerable<GroupViewModel> GetGroups();

        IEnumerable<ActivityViewModel> GetActivities();

        IEnumerable<Object> GetActivitiesByGroupId(int grpId);

        Task<bool> AddActivitiesToGroup(GroupViewModel model);

        //bool AddActivitiesToGroup(GroupViewModel model);
    }
}