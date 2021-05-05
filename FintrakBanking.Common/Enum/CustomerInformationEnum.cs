using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
    public enum CustomerInformationEnum
    {
    }
    public enum CustomerAddressTypeEnum
    {
        Permanent = 1,
        Residential = 2,
        Office = 3,
        Corporate = 4
    }
    public enum CustomerTypeEnum
    {
        Individual = 1,
        Corporate = 2
    }
    public enum CorporateCustomerTypeEnum
    {
        SME = 1,
        SmallCorporate = 2
    }
}
