using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class CrmsRegulatoryViewModel : GeneralEntity
    {
        public int regulatoryId { get; set; }
        public int crmsTypeId { get; set; }
        public int? customerTypeId { get; set; }
        public string code { get; set; }
        public string description { get; set; }
    }
    public class CrmsRegulatoryTypeViewModel : GeneralEntity
    {
        public int crmsTypeId { get; set; }
        public string description { get; set; }
    }
}
