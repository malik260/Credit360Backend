using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Common.Enum
{
   public enum ProductClassEnum
    {
        InvoiceDiscountingFacility = 6,
        BondAndGuarantees = 10,
        ImportFinanceFacilities = 41,
        EmergingBusiness = 33,
        Creditcards = 25,
        Corporate = 1,
    	Commercial = 2,
	    Individual = 4,
    	CashCollaterized = 5, 
    	PublicSector = 11,
    	PrivateBanking = 21,
    	ImportFinanceFacility = 41,
	    AutoLoans = 22,
    	PersonalLoan  = 23,
	    FacilityUpgradeSupportScheme = 24,
	    MortgageLoan = 26,
	    MHSS = 30,
	    WPOWER =  31,
	    MPOWER = 32,
	    AdvanceForSchoolFees = 39,
        DigitalLoans = 3,
        All4one = 44,
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
