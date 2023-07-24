using FintrakBanking.ViewModels.Setups.General;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IMisInfoRepository
    {
        IEnumerable<MisInfoViewModel> GetAllMisInfo();

        IEnumerable<MisInfoViewModel> GetMisInfoByCompanyId(int coyId);

        MisInfoViewModel GetMisInfoById(int misInfoId);

        Task<bool> AddMisInfo(MisInfoViewModel entity);

        Task<bool> UpdateMisInfo(int misinfoid, MisInfoViewModel entity);

        Task<bool> DeleteMisInfo(int misInfoId);

        IEnumerable<MisTypeViewModel> GetAllMisType();

        MisTypeViewModel GetMisTypeById(int misInfoId);

        Task<bool> AddMisType(MisTypeViewModel entity);

        Task<bool> UpdateMisType(int mistypeid, MisTypeViewModel entity);

        Task<bool> DeleteMisType(int misInfoId);
    }
}