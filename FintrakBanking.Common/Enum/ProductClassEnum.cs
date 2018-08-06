using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
   public enum ProductClassEnum
    {
        Corporate = 1,
        Commercial = 2,
        Retail = 3,
        Individual = 4,
        CashBackedOnly = 5,
        InvoiceDiscountingFacility = 6,
        FirstEdu = 7,
        FirstTrader = 8,
        ImportFinance = 9,
        BondAndGuarantees = 10
    }
    public enum ProductGroupEnum
    {
        LoansAndAdvances = 1,
        CurrentAndSavings = 2,
        MoneyMarket = 3,
        CapitalMarket = 4,
        RealEstate = 5,
        Lease = 6,
        ForexMarket = 7,
        CustomBond = 10,
        RetentionBond = 11
    }

    public enum DefaultProductEnum
    {
        CASA = 8
    
    }
}
