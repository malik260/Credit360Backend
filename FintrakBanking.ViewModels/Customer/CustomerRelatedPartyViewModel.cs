using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerRelatedPartyViewModel : GeneralEntity
    {
        public int relatedPartyId { get; set; }
        public int companyDirectorId { get; set; }
        public int customerId { get; set; }
        public string relationshipType { get; set; }
        public string directorName { get; set; }
        public string customerName { get; set; }
    }

   
}
