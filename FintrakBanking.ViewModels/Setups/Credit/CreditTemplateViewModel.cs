using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class CreditTemplateViewModel : GeneralEntity
    {
        public int creditTemplateId { get; set; }
        public string templateTitle { get; set; }
        public string templateDocument { get; set; }
        public int approvalLevelId { get; set; }
        public short productClassId { get; set; }
    }
}
