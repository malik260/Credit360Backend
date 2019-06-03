using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Risk
{
    public class CreditOfficerRiskViewModel : GeneralEntity
    {
        public int creditOfficerRiskId { get; set; }

    }

    public class MatrixGrid
    {
        public int id { get; set; }
        public string description { get; set; }
        public string rating { get; set; }
    }
}