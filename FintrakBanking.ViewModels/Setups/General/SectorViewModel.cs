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

    }
}
