using System;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.ErrorLogger
{
    public interface IErrorLogRepository
    {

        void LogError(Exception ex, string url,string username);
        Task LogErrorAsync(Exception ex, string url, string username);

    }
}