using System;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.ErrorLogger; 
using FintrakBanking.Interfaces.AppEmail; 
using FintrakBanking.Common;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.ErrorLogger
{
    public class ErrorLogRepository : IErrorLogRepository
    {
        FinTrakBankingContext context;
        IEmailRepository _emailRepo;
        public ErrorLogRepository() { }
        public ErrorLogRepository(FinTrakBankingContext _context,
                                   IEmailRepository emailRepo )
        {
            this.context = _context;
            this._emailRepo = emailRepo;
        }

        public async Task LogErrorAsync(Exception ex, string url, string username)
        {
            var errorMsg = ex.Message;
            if (ex.InnerException != null)
            {
                errorMsg += " " + ex.InnerException.Message;
            }

            var errorDetails = new TBL_ERRORLOG()
            {
                APIENDPOINT = url,
                ERRORMESSAGE = errorMsg,
                ERRORSOURCE = ex.Source,
                ERRORTYPE = ex.GetType().Name,
                ALLXML = errorMsg + " " + ex.StackTrace,
                USERNAME = username,
                ERRORPATH = "",
                TIMEUTC = DateTime.Now
            };
            this.context.TBL_ERRORLOG.Add(errorDetails);

            await context.SaveChangesAsync();

            //bool sendMail = bool.Parse(CommonHelpers.SendErrorMail);//  _config["AppConstants:sendErrorMail"]);
            //if (sendMail)
            //{
            //    var messageBody = $"<p>Fintrak Credit Management API Error:</p><p>{errorMsg}<br/>{errorDetails.AllXml}</p><p>API Url: {errorDetails.APIEndpoint}</p>";
            //    this._emailRepo.sendMail("corebankingteam@fintraksoftware.com", "corebankingteam@fintraksoftware.com", "", "", "Error Log", messageBody);
            //}
        }

        public void LogError(Exception ex, string url, string username)
        {
            var errorMsg = ex.Message;
            if (ex.InnerException != null)
            {
                errorMsg += " " + ex.InnerException.Message;
            }
            
            var errorDetails = new TBL_ERRORLOG()
            {
                APIENDPOINT = url,
                ERRORMESSAGE = errorMsg,
                ERRORSOURCE = ex.Source,
                ERRORTYPE = ex.GetType().Name,
                ALLXML = errorMsg + " " + ex.StackTrace,
                USERNAME = username,
                ERRORPATH = "" ,
                TIMEUTC = DateTime.Now
            };
            this.context.TBL_ERRORLOG.Add(errorDetails);
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