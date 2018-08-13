using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Admin;

namespace FintrakBanking.Interfaces
{
  public   interface IProfileSetupRepository
  {
      ProfileSettingViewModel UpdateProfileConfiguration(ProfileSettingViewModel entity);
      ProfileSettingViewModel GetProfileConfiguration();
        ProfileSettingViewModel GetProfileSettings();

    }
}
