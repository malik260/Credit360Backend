using System;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.ErrorLogger; 
using FintrakBanking.Interfaces.AppEmail; 
using FintrakBanking.Common;
using System.ComponentModel.Composition;

namespace FintrakBanking.Repositories.ErrorLogger
{
    public class ErrorLogRepository : IErrorLogRepository
    {
        FinTrakBankingContext context;
        IEmailRepository _emailRepo; 
        public ErrorLogRepository(FinTrakBankingContext _context,
                                   IEmailRepository emailRepo )
        {
            this.context = _context;
            this._emailRepo = emailRepo; 
        }

        public void LogError(Exception ex, string url, string username)
        {
            var errorMsg = ex.Message;
            if (ex.InnerException != null)
            {
                errorMsg += " " + ex.InnerException.Message;
            }

            
            var errorDetails = new tbl_ErrorLog()
            {
                APIEndpoint = url,
                ErrorMessage = errorMsg,
                ErrorSource = ex.Source,
                ErrorType = ex.GetType().Name,
                AllXml = errorMsg + " " + ex.StackTrace,
                Username = username,
                ErrorPath = "" ,
                TimeUtc = DateTime.Now
            };
            this.context.tbl_ErrorLog.Add(errorDetails);
            context.SaveChanges();

            //bool sendMail = bool.Parse(CommonHelpers.SendErrorMail);//  _config["AppConstants:sendErrorMail"]);
            //if (sendMail)
            //{
            //    var messageBody = $"<p>Fintrak Credit Management API Error:</p><p>{errorMsg}<br/>{errorDetails.AllXml}</p><p>API Url: {errorDetails.APIEndpoint}</p>";
            //    this._emailRepo.sendMail("corebankingteam@fintraksoftware.com", "corebankingteam@fintraksoftware.com", "", "", "Error Log", messageBody);
            //}
        }
    }
}