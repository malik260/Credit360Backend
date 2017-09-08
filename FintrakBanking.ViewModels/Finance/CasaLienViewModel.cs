using System;
namespace FintrakBanking.ViewModels.Finance
{
    public class CasaLienViewModel: GeneralEntity
    {
        public string lienReferenceNumber { get; set; }
        public string productAccountNumber { get; set; }
        public string sourceReferenceNumber { get; set; }
        public string certificateOfOccupancy { get; set; }
        public string description { get; set; }
        public short branchId { get; set; }
        public int stateId { get; set; }



    }
}