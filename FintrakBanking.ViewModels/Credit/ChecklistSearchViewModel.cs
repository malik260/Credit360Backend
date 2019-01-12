using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class ChecklistSearchViewModel
    {
        public int checkListDefinitionId { get; set; }
        public int checkListStatusId { get; set; }
        public int targetId { get; set; }
        public bool isproductbased { get; set; }
        public int? customerId { get; set; }
        public int? checkListItemId { get; set; }
        public int? checkListTypeId { get; set; }
        public DateTime? checklistDate { get; set; }
       
    }
}
