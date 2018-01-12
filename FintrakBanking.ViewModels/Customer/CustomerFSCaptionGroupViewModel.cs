using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.Customer
{
    public class CustomerFSCaptionGroupViewModel : GeneralEntity
    {
        public short fsCaptionGroupId { get; set; }
        public string fsCaptionGroupName { get; set; }
        public int position  { get; set; }
    }
}
