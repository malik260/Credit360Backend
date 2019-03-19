using FintrakBanking.Common.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Finance
{
 
    public class RefreshStagingMonitoringModel : GeneralEntity
    {
        public string status { get; set; }
        public int count { get; set; }
    }

}
