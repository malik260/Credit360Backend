using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General 
{
   public class PublicHolidayViewModel : GeneralEntity
    {
        public int PublicHolidayId { get; set; }

        public int CountryId { get; set; }

        public DateTime Date { get; set; }

        public string CountryName { get; set; }

        public string Description { get; set; }
    }
}
