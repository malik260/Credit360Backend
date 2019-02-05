using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Admin
{
    public class APIErrorLog : IAPIErrorLog
    {
        private readonly FinTrakBankingContext _context;
        public APIErrorLog(FinTrakBankingContext context)
        {
            _context = context;
        }
        public List<APILogViewModel> GetAPILog(DateTime startDate, DateTime endDate, string searchInfo)
        {
            var  apiLogList = from x in _context.TBL_CUSTOM_API_LOGS
                             where DbFunctions.TruncateTime(x.REQUESTDATETIME) >= DbFunctions.TruncateTime(startDate) 
                             && DbFunctions.TruncateTime(x.REQUESTDATETIME) <= DbFunctions.TruncateTime(endDate)
                             && (x.APIURL.ToLower().Contains(searchInfo.ToLower()) || x.REFERENCENUMBER.ToLower().Contains(searchInfo.ToLower())
                             || x.REQUESTMESSAGE.ToLower().Contains(searchInfo.ToLower())
                             || x.RESPONSEMESSAGE.ToLower().Contains(searchInfo.ToLower())
                             || searchInfo == "" || searchInfo == null)
                             orderby x.APILOGID descending
                             select new APILogViewModel
                             {
                                 apiUrl = x.APIURL,
                                 referenceNumber = x.REFERENCENUMBER,
                                 requestDateTime = x.REQUESTDATETIME,
                                 requestMessage = x.REQUESTMESSAGE,
                                 responseDateTime = x.RESPONSEDATETIME,
                                 responseMessage = x.RESPONSEMESSAGE,


                             };
            return  apiLogList.ToList();
        }

        public List<ErroLogViewModel> GetErrorLog(DateTime startDate, DateTime endDate)
        {
            var errorLog = from x in _context.TBL_ERRORLOG
                           where DbFunctions.TruncateTime( x.TIMEUTC) >= DbFunctions.TruncateTime(startDate) 
                           && DbFunctions.TruncateTime(x.TIMEUTC) <= DbFunctions.TruncateTime(endDate)
                           orderby x.ERRORLOGID descending
                           select new ErroLogViewModel
                           {
                               allXml = x.ALLXML,
                               apendEndPoint = x.APIENDPOINT,
                               errorMessage =x.ERRORMESSAGE,
                               errorPath =x.ERRORPATH,
                               errorSource = x.ERRORSOURCE,
                               errorType = x.ERRORTYPE,
                               statusCode =x.STATUSCODE,
                               username = x.USERNAME,
                               utc  = x.TIMEUTC
                           };
            return errorLog.ToList();
        }
    }
}
