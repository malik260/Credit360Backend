using FintrakBanking.Interfaces.CRMS;
using FintrakBanking.ViewModels.CRMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.CRMS
{
    public class CRMSCodeBookRepository : ICRMSCodeBookRepository
    {
        public IQueryable<CRMSCodeBookViewModel> BusinessLine()
        {
            var businessLine = new List<CRMSCodeBookViewModel> {
                new CRMSCodeBookViewModel {
                    CRMSCodde = "40100",
                    description="AGRICULTURE",
                    fintrakId=0
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40200",
                    description="MINING AND QUARRYING",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40300",
                    description="MANUFACTURING",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40500",
                    description="PUBLIC UTILITIES",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40700",
                    description="GENERAL COMMERCE",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40800",
                    description="TRANSPORTATION AND  STORAGE",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40900",
                    description="FINANCE AND INSURANCE",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "41000",
                    description="GENERAL",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "41200",
                    description="GOVERNMENT",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "41300",
                    description="WATER SUPPLY; SEWERAGE, WASTE MANAGEMENT AND REMEDIATION ACTIVITIES",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "41400",
                    description="CONSTRUCTION",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "41500",
                    description="INFORMATION AND COMMUNICATION",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "41600",
                    description="PROFESSIONAL, SCIENTIFIC AND TECHNICAL ACTIVITIES",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "41700",
                    description="ADMINISTRATIVE AND SUPPORT SERVICE ACTIVITIES",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "41800",
                    description="EDUCATION",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "41900",
                    description="HUMAN HEALTH AND SOCIAL WORK ACTIVITIES",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "42000",
                    description="ARTS, ENTERTAINMENT AND RECREATION",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "42100",
                    description="ACTIVITIES OF EXTRATERRITORIAL ORGANIZATIONS AND BODIES",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "42200",
                    description="POWER AND ENERGY",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "42300",
                    description="CAPITAL MARKET",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "42400",
                    description="OIL AND GAS",
                    fintrakId=1
                },

            };
            return businessLine.AsQueryable();
        }
        public IQueryable<CRMSCodeBookViewModel> SubSector()
        {
            var businessLine = new List<CRMSCodeBookViewModel> {
                new CRMSCodeBookViewModel {
                    CRMSCodde = "40110",
                    description="Crop Production",
                    fintrakId=0
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40120",
                    description="Poultry and livestock ",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40130",
                    description="Fishing",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40140",
                    description="Plantation",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40150",
                    description="Agro Services",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40160",
                    description="Cash Crop",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40210",
                    description="Metal: Tin, Iron, etc",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40220",
                    description="Non-metal Quarrying",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40301",
                    description="Flourmills and Bakeries",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40302",
                    description="Food manufacturing",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40303",
                    description="Beverages",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40304",
                    description="Aluminium and allied products",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40305",
                    description="Basic Metal Products",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40306",
                    description="Breweries",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40307",
                    description="Building materials",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40308",
                    description="Cement",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40309",
                    description="Chemicals and Allied products",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40310",
                    description="Footwear",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40311",
                    description="Hides and Skin",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40312",
                    description="Household Equipment",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40313",
                    description="Pharmaceuticals",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40314",
                    description="Paints and Allied products",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40315",
                    description="Miscellaneous Manufacturing",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40316",
                    description="Paper and Paper products",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40317",
                    description="Printing and Publishing",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40318",
                    description="Personal care",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40319",
                    description="Plastics",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40320",
                    description="Rubber and Allied products",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40321",
                    description="Steel Rolling Mills",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40322",
                    description="Soft Drinks",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40323",
                    description="Cables and Mines",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40324",
                    description="Textiles and Apparel",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40325",
                    description="Tyre",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40326",
                    description="Conglomerate",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40510",
                    description="Residential Mortgage Loans",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40520",
                    description="Commercial Property",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40530",
                    description="Home Equity",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40540",
                    description="Real estate Construction/ Home Developers",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40550",
                    description="Real estate (Income-Producing)",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40560",
                    description="High-volatility Commercial real estate",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40610",
                    description="Utility (Public)",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40620",
                    description="Utility (Private)",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40710",
                    description="Automotive parts",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40720",
                    description="Domestic trade",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40730",
                    description="Automobile (Motor Vehicles)",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40740",
                    description="Food Processing",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40750",
                    description="Chemicals and Allied Products",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40760: Trading (Rice)",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "",
                    description="",
                    fintrakId=1
                },

            };
            return businessLine.AsQueryable();
        }

        public IQueryable< CRMSCodeBookViewModel> LoanType()
        {
            var loanType = new List<CRMSCodeBookViewModel> {
                new CRMSCodeBookViewModel {
                    CRMSCodde = "40030",
                    description="Personal Loan",
                    fintrakId=0
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40031",
                    description="Cash backed & asset backed Loans",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40032",
                    description="Advance Payment Guarantee/Bid Bond/Performance Bond/Bank Gttee",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40033",
                    description="Cheque Discounting/Drawing Against Uncleared Cheques",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40034",
                    description="Credit Card",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40035",
                    description="Export Finance",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40036",
                    description="Import Finance Facility (IFF)",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40037",
                    description="Indemnity",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40038",
                    description="Invoice Discounting/LPO Finance",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40039",
                    description="Mortgage Loan",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40040",
                    description="Leases (Operating, Finance, Wet, Equipment etc etc)",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40041",
                    description="Overdraft & Working Capital",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40042",
                    description="Project/Contract Finance",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40043",
                    description="Share Purchase Loan",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40044",
                    description="Term Loan",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40045",
                    description="Asset/Stock Financing",
                    fintrakId=1
                }, new CRMSCodeBookViewModel {
                    CRMSCodde = "40046",
                    description="Foreign Currency Loan",
                    fintrakId=1
                },new CRMSCodeBookViewModel {
                    CRMSCodde = "40047",
                    description="Bankers Acceptance/Commercial Paper (NOT FOR SYNDICATION)",
                    fintrakId=1
                },

            };
            return loanType.AsQueryable();


        }
    }
}
