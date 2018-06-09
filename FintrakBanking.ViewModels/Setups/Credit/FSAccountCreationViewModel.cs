using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class FSAccountCreationViewModel
    {
        public string fx_Code { get; set; }
        public string ref_Desc { get; set; }
        public string del_Flg { get; set; }
        public string bank_Id { get; set; }
    }
    public class FSAccountCreationListViewModel
    {
        public List<FSAccountCreationViewModel> freeCode1 { get; set; }
        public List<FSAccountCreationViewModel> freeCode4 { get; set; }
        public List<FSAccountCreationViewModel> freeCode5 { get; set; }
        public List<FSAccountCreationViewModel> freeCode6 { get; set; }
        public List<FSAccountCreationViewModel> freeCode7 { get; set; }
        public List<FSAccountCreationViewModel> freeCode8 { get; set; }
        public List<FSAccountCreationViewModel> freeCode9 { get; set; }
        public List<FSAccountCreationViewModel> freeCode10 { get; set; }
        public List<FSAccountCreationViewModel> sanctionLevel { get; set; }
        public List<FSAccountCreationViewModel> sanctionAuthority { get; set; }
        public List<FSAccountCreationViewModel> sectorCode { get; set; }
        public List<FSAccountCreationViewModel> sub_sector { get; set; }
        public List<FSAccountCreationViewModel> purposeOfAdvance { get; set; }
        public List<FSAccountCreationViewModel> occupationCode { get; set; }
        public List<FSAccountCreationViewModel> natureOfAdvance { get; set; }
        public List<FSAccountCreationViewModel> modeOfAdvance { get; set; }
    }
}
