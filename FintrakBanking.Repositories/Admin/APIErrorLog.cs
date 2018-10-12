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
        public async Task<List<APILogViewModel>> GetAPILog(DateTime startDate, DateTime endDate)
        {
            var  apiLogList = from x in _context.TBL_CUSTOM_API_LOGS
                             where x.REQUESTDATETIME >= startDate && x.REQUESTDATETIME <= endDate
                             select new APILogViewModel
                             {
                                 apiUrl = x.APIURL,
                                 referenceNumber = x.REFERENCENUMBER,
                                 requestDateTime = x.REQUESTDATETIME,
                                 requestMessage = x.RESPONSEMESSAGE,
                                 responseDateTime = x.RESPONSEDATETIME,
                                 responseMessage = x.RESPONSEMESSAGE

                             };
            return await apiLogList.ToListAsync();
        }

        public async Task<List<ErroLogViewModel>> GetErrorLog(DateTime startDate, DateTime endDate)
        {
            var errorLog = from x in _context.TBL_ERRORLOG
                           where x.TIMEUTC >= startDate && x.TIMEUTC <= endDate
                           select new ErroLogViewModel
                           {
                               allXml = x.ALLXML,
                               apendEndPoint = x.APIENDPOINT,
                               errorMessage =x.ERRORMESSAGE,
                               errorPath =x.ERRORPATH,
                               errorSource = x.ERRORSOURCE,
                               errorType = x.ERRORTYPE,
                               statusCode =x.STATUSCODE,
                               username = x.USERNAME
                           };
            return await errorLog.ToListAsync();
        }
    }
}
