using System;
namespace FintrakBanking.ViewModels.CASA
{
    public class CasaLienViewModel: GeneralEntity
    {
        public int lienId { get; set; }

        public string lienReferenceNumber { get; set; }
        public string productAccountNumber { get; set; }
        public string sourceReferenceNumber { get; set; }
        public string certificateOfOccupancy { get; set; }
        public string description { get; set; }
        public short branchId { get; set; }
        public decimal lienAmount { get; set; }
        public short lienTypeId { get; set; }                
        public int stateId { get; set; }
        public string branchName { get; set; }
        public string lienTypeName { get; set; }
        public string customerName { get; set; }
        public DateTime dateLienRemoved { get; set; }

        public string currencyCode { get; set; }

        public bool isTermDeposit { get; set; }
        //public string lienReferenceNumber { get; set; }
        //public string productAccountNumber { get; set; }
        //public string sourceReferenceNumber { get; set; }
        //public string certificateOfOccupancy { get; set; }
        //public string description { get; set; }
        //public short branchId { get; set; }


    }

}

