using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.SupportUtility
{
   public class SupportUtilityViewModel: GeneralEntity
    {
        public int supportIssueTypeId { get; set; }
        public string description { get; set; }
        public string tag { get; set; }
    }
}
