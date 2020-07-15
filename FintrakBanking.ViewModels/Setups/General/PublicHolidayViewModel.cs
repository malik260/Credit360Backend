using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General 
{
   public class PublicHolidayViewModel : GeneralEntity
    {
        public int publicHolidayId { get; set; }

        public new int countryId { get; set; }

        public DateTime date { get; set; }

        public string countryName { get; set; }

        public string description { get; set; }

        public bool isActive { get; set; }
    }
}
