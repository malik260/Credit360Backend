using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General; 
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IAuthorizationRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class AuthorizationRepository : IAuthorizationRepository
    { 
        private FinTrakBankingContext context;

        public AuthorizationRepository( FinTrakBankingContext _context)
        { 
            this.context = _context;
        }

        public async Task<bool> AddGroup(GroupModel groupModel)
        {
            var newGroup = new tbl_Profile_Group()
            {
                GroupName = groupModel.groupName,
                CreatedBy = groupModel.createdBy,
                DateTimeCreated = DateTime.UtcNow
            };

              context.tbl_Profile_Group.Add(newGroup);

            var response = await context.SaveChangesAsync();

            return response > 0;
        }

        public Task<bool> DeleteGroup(short groupId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateGroup(short groupId, GroupViewModel groupModel)
        {
            var targetGroup = context.tbl_Profile_Group.Find(groupId);

            targetGroup.GroupName = groupModel.groupName;
            targetGroup.DateTimeUpdated = DateTime.UtcNow;
            targetGroup.LastUpdatedBy = groupModel.createdBy;

            var response = await context.SaveChangesAsync();
            return response != 0;
        }

        public IEnumerable<GroupViewModel> GetGroups()
        {
            IEnumerable<GroupViewModel> result = null;

            result = context.tbl_Profile_Group.Select(g => new GroupViewModel()
            {
                groupId = g.GroupId,
                groupName = g.GroupName,
                Activities = g.tbl_Profile_Group_Activity.Select(x => new ActivityViewModel()
                {
                    activityId = x.ActivityId,
                    activityName = x.tbl_Profile_Activity.ActivityName
                }).ToList()
            });

            return result;
        }

        public IEnumerable<ActivityViewModel> GetActivities()
        {
            return context.tbl_Profile_Activity.Select(x => new ActivityViewModel()
            {
                activityId = x.ActivityId,
                activityName = x.ActivityName
            });
        }

        public async Task<bool> AddActivitiesToGroup(GroupViewModel model)
        {
            if (model.Activities.Any())
            {
                var existingActivities = context.tbl_Profile_Group_Activity.Where(x => x.GroupId == model.groupId).ToList();
                bool isExist = existingActivities.Count() > 0;
                if (existingActivities.Count > 0)
                {
                    foreach (var item in existingActivities)
                    {
                        context.tbl_Profile_Group_Activity.Remove(item);
                    }
                }

                foreach (var activity in model.Activities)
                {
                    var newActivity = new tbl_Profile_Group_Activity()
                    {
                        ActivityId = activity.activityId,
                        GroupId = model.groupId,
                        CreatedBy = model.createdBy,
                        DateTimeCreated = DateTime.UtcNow
                    };
                    if (isExist)
                    {
                        newActivity.DateTimeUpdated = DateTime.UtcNow;
                    }

                    context.tbl_Profile_Group_Activity.Add(newActivity);
                }
            }
            var response = await context.SaveChangesAsync();

            return response > 0;
        }

        public IEnumerable<Object> GetActivitiesByGroupId(int grpId)
        {
            return context.tbl_Profile_Group_Activity.Where(a => a.GroupId == grpId)
                .Select(x => new
                {
                    activityId = x.ActivityId,
                    groupId = x.GroupId
                });
        }
    }
}