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
        public string del_Flg { get; set; }
        public string bank_Id { get; set; }
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
        public List<FXAccountCreationViewModel> sanctionLevel { get; set; }
        public List<FXAccountCreationViewModel> sanctionAuthority { get; set; }
        public List<FXAccountCreationViewModel> sectorCode { get; set; }
        public List<FXAccountCreationViewModel> sub_sector { get; set; }
        public List<FXAccountCreationViewModel> purposeOfAdvance { get; set; }
        public List<FXAccountCreationViewModel> occupationCode { get; set; }
        public List<FXAccountCreationViewModel> natureOfAdvance { get; set; }
        public List<FXAccountCreationViewModel> modeOfAdvance { get; set; }
    }
}
