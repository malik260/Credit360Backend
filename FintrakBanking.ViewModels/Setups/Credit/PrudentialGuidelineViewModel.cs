using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
  public  class PrudentialGuidelineViewModel : GeneralEntity
    {
        public int prudentialGuidelineId { get; set; }

        public string statusName { get; set; }

        public string classification { get; set; }

        public int? internalMinimun { get; set; }

        public int? internalMaximun { get; set; }

        public int? externalMinimun { get; set; }

        public int? externalMaximun { get; set; }

        public string naration { get; set; }
    }
}
