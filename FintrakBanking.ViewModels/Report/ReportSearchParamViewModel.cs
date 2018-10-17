using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Report
{
    public class ReportSearchParamViewModel
    {
        public int branchId { get; set; }
        public string param { get; set; }
    }

    public class DropdownParam
    {
        public int valueId { get; set; }
        public string valueName { get; set; }

    }
}
