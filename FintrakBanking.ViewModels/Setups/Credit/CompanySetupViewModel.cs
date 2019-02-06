using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class CompanySetupViewModel //: GeneralEntity
    {
        public bool requireCreditBureauModule { get; set; }
        public short creditBureauSearchTypeId { get; set; }
        public short collateralSearchTypeId { get; set; }
    }
}
