using FintrakBanking.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Admin
{
    public interface IAPIErrorLog
    {
        Task<List<ErroLogViewModel>> GetErrorLog(DateTime startDate, DateTime endDate);
        Task<List<APILogViewModel>> GetAPILog(DateTime startDate, DateTime endDate);
    }
}
