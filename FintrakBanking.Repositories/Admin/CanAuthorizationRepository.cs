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
            IList<CanGroupViewModel> activities = this.loadUserResources(userId);

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


        private IList<CanGroupViewModel> loadUserResources(int userId)
        {
            var userGroups = (from ug in context.TBL_PROFILE_USERGROUP
                              join u in context.TBL_PROFILE_GROUP on ug.GROUPID equals u.GROUPID
                              where ug.USERID == userId
                              select ug.GROUPID).ToList();

            IList<CanGroupViewModel> groupsActivities = null;
            IList<CanGroupViewModel> additionalActivities = null;
            IList<CanGroupViewModel> fullActivities = null;

            if (userGroups.Any())
            {
                groupsActivities = context.TBL_PROFILE_GROUP_ACTIVITY.Where(x => userGroups.Contains(x.GROUPID))
                                        .Select(x => new CanGroupViewModel
                                        {
                                            ActivityId = x.ACTIVITYID,
                                            CanAdd = x.CANADD.Value,
                                            CanApprove = x.CANAPPROVE.Value,
                                            CanDelete = x.CANDELETE.Value,
                                            CanEdit = x.CANEDIT.Value,
                                            CanView = x.CANVIEW.Value

                                        }).ToList();


                additionalActivities = context.TBL_PROFILE_ADDITIONALACTIVITY.Where(x => x.USERID == userId)
                                        .Select(x => new CanGroupViewModel
                                        {
                                            ActivityId = x.ACTIVITYID,
                                            CanAdd = x.CANADD,
                                            CanApprove = x.CANAPPROVE,
                                            CanDelete = x.CANDELETE,
                                            CanEdit = x.CANEDIT,
                                            CanView = x.CANVIEW

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
