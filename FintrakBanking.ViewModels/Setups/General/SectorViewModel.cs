using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class SectorViewModel : GeneralEntity
    {
        public short? subSectorId { get; set; }
        public short sectorId { get; set; }
        public string sectorName { get; set; }
        public string sectorCode { get; set; }
        public decimal? sectorLimit { get; set; }
        public bool allowOverride { get; set; }
    }

    public class SectorLimitViewModel : GeneralEntity
    {
        public short sectorId { get; set; }
        public string sectorName { get; set; }
        public string sectorCode { get; set; }
        public decimal sectorLimit { get; set; }
        public decimal sectorUsage { get; set; }
        public decimal sectorBalance { get; set; }
        public bool allowOverride { get; set; }
    }

    public class ExposureLimitRequestModel
    {
        public int companyId { get; set; } // required!
        public int? staffId { get; set; }
        public int? customerId { get; set; }
        public int? customerGroupId { get; set; }
        public int? branchId { get; set; }
        public int? sectorId { get; set; }
        public int? applicationId { get; set; }
    }

    public class RequestExposureLimit
    {
        public RequestExposureLimit()
        {
            SectorLimit = 0;
            SectorMaximumExposure = 0;
        }

        public string productCustomerName { get; set; }

        public decimal? ObligorLimit { get; set; }
        public decimal? ObligorExposure { get; set; }
        public decimal? ObligorMaximumExposure { get; set; }

        public decimal? SectorLimit { get; set; }
        public decimal? SectorExposure { get; set; }
        public decimal? SectorMaximumExposure { get; set; }

        public string SectorLimitString
        {
            get
            {
                if (SectorMaximumExposure == 0 || SectorMaximumExposure == null) return "No limit";
                return string.Format("{0:#,0.00}", SectorLimit);
            }
        }

        public string ObligorLimitString
        {
            get
            {
                if (ObligorMaximumExposure == 0 || ObligorMaximumExposure == null) return "No limit";
                return string.Format("{0:#,0.00}", ObligorLimit);
            }
        }
    }

    public class TotalExposureLimit
    {
        public decimal? AccountOfficerNPLLimit { get; set; }
        public decimal? AccountOfficerNPLExposure { get; set; }
        public decimal? AccountOfficerMaximumNPLExposure { get; set; }

        public decimal? BranchNPLLimit { get; set; }
        public decimal? BranchNPLExposure { get; set; }
        public decimal? BranchMaximumNPLExposure { get; set; }

        //public decimal? ObligorLimit { get; set; } 
        //public decimal? ObligorExposure { get; set; }
        //public decimal? ObligorMaximumExposure { get; set; }
        //public decimal? SectorLimit { get; set; }
        //public decimal? SectorExposure { get; set; }
        //public decimal? SectorMaximumExposure { get; set; }

        public decimal? InitiatedLoansBalance { get; set; }
        public decimal? UndisbursedApprovedLoansBalance { get; set; }

        public List<RequestExposureLimit> RequestExposureLimits { get; set; }

        //public string SectorLimitString
        //{
        //    get
        //    {
        //        if (SectorMaximumExposure == 0 || SectorMaximumExposure == null) return "No limit";
        //        return string.Format("{0:#,0.00}", SectorLimit);
        //    }
        //}

        public string AccountOfficerNPLLimitString
        {
            get
            {
                if (AccountOfficerMaximumNPLExposure == 0 || AccountOfficerMaximumNPLExposure == null) return "No limit";
                return string.Format("{0:#,0.00}", AccountOfficerNPLLimit);
            }
        }

        public string BranchNPLLimitString
        {
            get
            {
                if (BranchMaximumNPLExposure == 0 || BranchMaximumNPLExposure == null) return "No limit";
                return string.Format("{0:#,0.00}", BranchNPLLimit);
            }
        }

        //public string ObligorLimitString
        //{
        //    get
        //    {
        //        if (ObligorMaximumExposure == 0 || ObligorMaximumExposure == null) return "No limit";
        //        return string.Format("{0:#,0.00}", ObligorLimit);
        //    }
        //}

       

    }
    public class GlobalSectorViewModel : GeneralEntity
    {
        public int id { get; set; }
        public DateTime? date { get; set; }
        public string cbnSector { get; set; }
        public string cbnSectorId { get; set; }
        public decimal? totalExposureLcy { get; set; }
        public decimal? percentageExposures { get; set; }
        public decimal? percentageSectorLimit { get; set; }
    }
    
}
