using System;
namespace FintrakBanking.ViewModels.Finance
{
    public class CasaLienViewModel: GeneralEntity
    {
        public string lienReferenceNumber { get; set; }
        public string productAccountNumber { get; set; }
        public string sourceReferenceNumber { get; set; }
        //public decimal lienCreditAmount { get; set; }
        //public decimal lienDebitAmount { get; set; }
        public string description { get; set; }
        //public short lienTypeId { get; set; }
        //public DateTime dateCreated { get; set; }
        //public int companyId { get; set; }
        public short branchId { get; set; }
        public int stateId { get; set; }
        //public int createdBy { get; set; }



    }
}