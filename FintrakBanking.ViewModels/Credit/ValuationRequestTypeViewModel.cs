using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
    public class ValuationRequestTypeViewModel : GeneralEntity
    {
        public int valuationRequestTypeId { get; set; }

        public string valuationRequestType { get; set; }
    }
}
