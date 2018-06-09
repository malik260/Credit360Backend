using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.Credit
{
    public class FSAccountCreationRepository : IFSAccountCreationRepository
    {
        private FinTrakBankingStagingContext context;

        public FSAccountCreationRepository(FinTrakBankingStagingContext _context)
        {
            context = _context;
        }
        public List<FSAccountCreationViewModel> GetAllFreeCode1()
        {
            var freecode1 = (from a in context.STG_FREECODE1
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE1,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllFreeCode4()
        {
            var freecode1 = (from a in context.STG_FREECODE4
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE4,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllFreeCode5()
        {
            var freecode1 = (from a in context.STG_FREECODE5
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE5,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllFreeCode6()
        {
            var freecode1 = (from a in context.STG_FREECODE6
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE6,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllFreeCode7()
        {
            var freecode1 = (from a in context.STG_FREECODE7
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE7,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllFreeCode8()
        {
            var freecode1 = (from a in context.STG_FREECODE8
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE8,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllFreeCode9()
        {
            var freecode1 = (from a in context.STG_FREECODE9
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE9,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllFreeCode10()
        {
            var freecode1 = (from a in context.STG_FREECODE10
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.FREECODE10,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllModeOfAdvance()
        {
            var freecode1 = (from a in context.STG_MODE_OF_ADV
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.MODE_OF_ADVANCE,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllNatureOfAdvance()
        {
            var freecode1 = (from a in context.STG_NAT_OF_ADV
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.NATURE_OF_ADVANCE,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllOccupationCode()
        {
            var freecode1 = (from a in context.STG_OCCUPATION_CODE
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.OCCUPATION_CODE,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllPurposeOfAdvance()
        {
            var freecode1 = (from a in context.STG_PURPOSE_OF_ADV
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.PURPOSE_OF_ADVANCE,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1; 
        }
        public List<FSAccountCreationViewModel> GetAllSanctionAuthority()
        {
            var freecode1 = (from a in context.STG_SANCTION_AUTH
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.SANCTION_AUTHORITY,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1; 
        }
        public List<FSAccountCreationViewModel> GetAllSanctionLevel()
        {
            var freecode1 = (from a in context.STG_SANCTION_LEVEL
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.SANCTION_LEVEL,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllSubSector()
        {
            var freecode1 = (from a in context.STG_SUB_SECTOR
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.SUB_SECTOR,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public List<FSAccountCreationViewModel> GetAllSector()
        {
            var freecode1 = (from a in context.STG_SECTOR_CODE
                             where a.DEL_FLG == "N"
                             select new FSAccountCreationViewModel
                             {
                                 fx_Code = a.SECTOR_CODE,
                                 ref_Desc = a.REF_DESC,
                                 del_Flg = a.DEL_FLG,
                                 bank_Id = a.BANK_ID
                             }).ToList();
            return freecode1;
        }
        public FSAccountCreationListViewModel GetListOfFSCode()
        {
            var list = new FSAccountCreationListViewModel();
            list.freeCode1 = GetAllFreeCode1();
            list.freeCode4 = GetAllFreeCode4();
            list.freeCode5 = GetAllFreeCode5();
            list.freeCode6 = GetAllFreeCode6();
            list.freeCode7 = GetAllFreeCode7();
            list.freeCode8 = GetAllFreeCode8();
            list.freeCode9 = GetAllFreeCode9();
            list.freeCode10 = GetAllFreeCode10();
            list.modeOfAdvance = GetAllModeOfAdvance();
            list.natureOfAdvance = GetAllNatureOfAdvance();
            list.sectorCode = GetAllSector();
            list.sub_sector = GetAllSubSector();
            list.sanctionAuthority = GetAllSanctionAuthority();
            list.sanctionLevel = GetAllSanctionLevel();
            list.occupationCode = GetAllOccupationCode();
            list.purposeOfAdvance = GetAllPurposeOfAdvance();
            return list;
        }
    }
}
