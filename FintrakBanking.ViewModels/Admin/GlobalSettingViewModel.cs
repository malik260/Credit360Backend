using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Admin
{
    public class GlobalSettingViewModel : GeneralEntity
    {
        public short applicationSetupId { get; set; }

        public string reportPath { get; set; }

        public bool useActiveDirectory { get; set; }

        public string activeDirectoryDomainName { get; set; }

        public string activeDirectoryUserName { get; set; }

        public string activeDirectoryUserPassword { get; set; }

        public bool requireAdUser { get; set; }

        public bool useThirdPArtyIntegration { get; set; }

        public bool useTwoFactorAuthentication { get; set; }

        public int maxFileUploadSize { get; set; }

        public string applicationURL { get; set; }

        public string supportEmail { get; set; }

    }
   
}
