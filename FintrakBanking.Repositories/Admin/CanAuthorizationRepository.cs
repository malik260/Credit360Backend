using FintrakBanking.Interfaces.Admin;
using System;
using System.Collections.Generic;
using System.Text;
using FintrakBanking.ViewModels.Admin;
using FintrakBanking.Entities.Models; 
using System.Linq;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.Admin
{ 
    public class CanAuthorizationRepository : ICanAuthorizationRepository
    {
        private FinTrakBankingContext context;
         


        public CanAuthorizationRepository(FinTrakBankingContext _context )
        {
            this.context = _context; 
        }
        public bool CanPerformActionOnResource(int userId, int activityId, UserActions action)
        {
            IList<CanGroupViewModel> activities = this.LoadUserResources(userId);

            switch (action)
            {
                case UserActions.Approve:
                    return activities.Any(x => x.ActivityId == activityId && x.CanApprove == true);
                case UserActions.Delete:
                    return activities.Any(x => x.ActivityId == activityId && x.CanDelete == true);
                case UserActions.Update:
                    return activities.Any(x => x.ActivityId == activityId && x.CanEdit == true);
                case UserActions.View:
                    return activities.Any(x => x.ActivityId == activityId && x.CanView == true);
                case UserActions.Add:
                    return activities.Any(x => x.ActivityId == activityId && x.CanAdd == true);
                default:
                    return false;
            }
        }


        private IList<CanGroupViewModel> LoadUserResources(int userId)
        {
            var userGroups = (from ug in context.tbl_Profile_UserGroup
                              join u in context.tbl_Profile_Group on ug.GroupId equals u.GroupId
                              where ug.UserId == userId
                              select ug.GroupId).ToList();

            IList<CanGroupViewModel> groupsActivities = null;
            IList<CanGroupViewModel> additionalActivities = null;
            IList<CanGroupViewModel> fullActivities = null;

            if (userGroups.Any())
            {
                groupsActivities = context.tbl_Profile_Group_Activity.Where(x => userGroups.Contains(x.GroupId))
                                        .Select(x => new CanGroupViewModel
                                        {
                                            ActivityId = x.ActivityId,
                                            CanAdd = x.CanAdd.Value,
                                            CanApprove = x.CanApprove.Value,
                                            CanDelete = x.CanDelete.Value,
                                            CanEdit = x.CanEdit.Value,
                                            CanView = x.CanView.Value

                                        }).ToList();


                additionalActivities = context.tbl_Profile_AdditionalActivity.Where(x => x.UserId == userId)
                                        .Select(x => new CanGroupViewModel
                                        {
                                            ActivityId = x.ActivityId,
                                            CanAdd = x.CanAdd,
                                            CanApprove = x.CanApprove,
                                            CanDelete = x.CanDelete,
                                            CanEdit = x.CanEdit,
                                            CanView = x.CanView

                                        }).ToList();

                if (groupsActivities.Any())
                {
                    fullActivities = groupsActivities.Concat(additionalActivities).ToList();
                }

            }
            return fullActivities;
        }
    }
}
