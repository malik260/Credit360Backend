using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public  class AuthourisedSignatoriesViewModel
    {
        public int signatoryId { get; set; }
        public string signatoryName { get; set; }
        public string signatoryLevel { get; set; }
        public string signatoryInitial { get; set; }
        
    }
}
