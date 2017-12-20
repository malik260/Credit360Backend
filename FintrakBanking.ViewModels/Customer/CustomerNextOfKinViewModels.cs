using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Customer
{
   public class CustomerNextOfKinViewModels: GeneralEntity
    {
       public int nextOfKinId { get; set; }
        public int customerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phoneNumber { get; set; }
        public DateTime? dateOfBirth { get; set; }
        public string gender { get; set; }
        public string relationship { get; set; }
        public string email { get; set; }
        public string address { get; set; }
        public string nearestLandmark { get; set; }
        public int? stateId { get; set; }
        public int? cityId { get; set; }
        public bool? active { get; set; } 
   
    }
}
