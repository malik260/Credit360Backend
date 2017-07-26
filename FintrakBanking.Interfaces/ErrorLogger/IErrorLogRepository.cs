using System;

namespace FintrakBanking.Interfaces.ErrorLogger
{
    public interface IErrorLogRepository
    {

        void LogError(Exception ex, string url,string username);

    }
}