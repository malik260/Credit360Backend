using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class SectorViewModel: GeneralEntity
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
}
