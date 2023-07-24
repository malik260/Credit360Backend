using FintrakBanking.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Admin
{
    public interface ICanAuthorizationRepository
    {
        bool CanPerformActionOnResource(int userId, int activityId, UserActions action);
    }
}
