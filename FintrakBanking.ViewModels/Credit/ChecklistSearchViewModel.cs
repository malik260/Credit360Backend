using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Credit
{
   public class ChecklistSearchViewModel
    {
        public int definitionId { get; set; }
        public int statusId { get; set; }
        public int detailId { get; set; }
        public bool isProductBased { get; set; }
        public int? customerId { get; set; }
        public int? checkListItemId { get; set; }
        public int? checkListTypeId { get; set; }
        public DateTime? checklistDate { get; set; }
       
    }
}
