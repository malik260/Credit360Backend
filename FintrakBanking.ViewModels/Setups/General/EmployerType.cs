using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
    public class EmployerType
    {
        public int employerTypeId { get; set; }
        public string employerTypeName { get; set; }
    }

    public class EmployerSubType
    {
        public int employerSubTypeId { get; set; }
        public string employerSubTypeName { get; set; }
        public string employerTypeName { get; set; }
        public short employerTypeId { get; set; }
    }
}
