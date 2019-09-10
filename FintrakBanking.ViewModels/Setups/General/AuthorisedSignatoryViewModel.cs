using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public  class AuthorisedSignatoryViewModel:GeneralEntity
    {
        public int signatoryId { get; set; }
        public string signatoryName { get; set; }
        public string signatoryTitle { get; set; }
        public string signatoryInitials { get; set; }
    }
}
