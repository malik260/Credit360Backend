using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class FXAccountCreationViewModel
    {
        public string fx_Code { get; set; }
        public string ref_Desc { get; set; }
        // public string del_Flg { get; set; }
        // public string bank_Id { get; set; }
    }
    public class FXAccountCreationListViewModel
    {
        public List<FXAccountCreationViewModel> freeCode1 { get; set; }
        public List<FXAccountCreationViewModel> freeCode4 { get; set; }
        public List<FXAccountCreationViewModel> freeCode5 { get; set; }
        public List<FXAccountCreationViewModel> freeCode6 { get; set; }
        public List<FXAccountCreationViewModel> freeCode7 { get; set; }
        public List<FXAccountCreationViewModel> freeCode8 { get; set; }
        public List<FXAccountCreationViewModel> freeCode9 { get; set; }
        public List<FXAccountCreationViewModel> freeCode10 { get; set; }

        public List<FXAccountCreationViewModel> borrowerCategoryCode { get; set; }
        public List<FXAccountCreationViewModel> occupationCode { get; set; }

        public List<FXAccountCreationViewModel> sectorCode { get; set; }
        public List<FXAccountCreationViewModel> sub_sector { get; set; }

        public List<FXAccountCreationViewModel> natureOfAdvance { get; set; }
        public List<FXAccountCreationViewModel> modeOfAdvance { get; set; }
        public List<FXAccountCreationViewModel> advanceType { get; set; }
        public List<FXAccountCreationViewModel> purposeOfAdvance { get; set; }

        public List<FXAccountCreationViewModel> currencyCode { get; set; }
        public List<FXAccountCreationViewModel> schemeCode { get; set; }
        public List<FXAccountCreationViewModel> sol_Ids { get; set; }
        public List<FXAccountCreationViewModel> glSubHeadCode { get; set; }
        

    }
    public class CustomerFXAccountViewModel
    {
        public string functionCode { get; set;}
        public string solId { get; set; }
        public string channel { get; set; }
        public string currencyCode { get; set; }
        public string customerCode { get; set; }
        public string schemeCode { get; set; }
        public string generalLedgerSubHeadCode { get; set; }
        public string sectorCode { get; set; }
        public string subSectorCode { get; set; }
        public string accountOccupationCode { get; set; }
        public string borrowerCategoryCode { get; set; }
        public string purposeOfAdavance { get; set; }
        public string natureOfAdavance { get; set; }
        public string modeOfAdavance { get; set; }
        public string typeOfAdavance { get; set; }
        public string freeCodeOne { get; set; }
        public string freeCodeFour { get; set; }
        public string freeCodeFive { get; set; }
        public string freeCodeSix { get; set; }
        public string freeCodeSeven { get; set; }
        public string freeCodeEight { get; set; }
        public string freeCodeNine { get; set; }
        public string freeCodeTen { get; set; }
    }
}
