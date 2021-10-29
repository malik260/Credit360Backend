using FintrakBanking.Common.CustomException;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.ThridPartyIntegration;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using FinTrakBanking.ThirdPartyIntegration.Finacle;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinTrakBanking.ThirdPartyIntegration.HeadOfficeToSub
{
    public class HeadOfficeToSubIntegration : IHeadOfficeToSubIntegration
    {
        private FinTrakBankingContext _context;
        private string API_KEY, API_URL = string.Empty;
        private IEnumerable<TBL_API_URL> APIUrlConfig;
        private TransactionPosting transaction;
        public HeadOfficeToSubIntegration(FinTrakBankingContext context, TransactionPosting transaction)
        {
            this._context = context;
            this.transaction = transaction;
            var configdata = context.TBL_SETUP_COMPANY.FirstOrDefault();
            APIUrlConfig = context.TBL_API_URL;
            API_KEY = configdata.APIKEY;
            API_URL = configdata.APIURL;
        }

        private void getAPIURLSettings(string typeName = null)
        {
            var apiConfig = APIUrlConfig.Where(x => x.TYPENAME.ToLower() == typeName.ToLower()).FirstOrDefault();
            if (apiConfig != null)
            {
                API_URL = apiConfig.URL.Trim();
                API_KEY = apiConfig.APIKEY;
            }
            if (apiConfig == null)
            {
                apiConfig = APIUrlConfig.Where(x => x.TYPENAME.ToUpper() == "DEFAULT").FirstOrDefault();
                API_URL = apiConfig.URL.Trim();
                API_KEY = apiConfig.APIKEY;
            }
        }

        public PostingResult PostFacilityApprovalToSubnputs(ForwardViewModel model)
        {
            {
                ResponseMessage result = null;
                Task.Run(async () => result = await transaction.ApprovalPostingToSubOffice(model)).GetAwaiter().GetResult();

                if (result.APIResponse != null)
                {
                    if (result.APIResponse.responseCode == "00")
                    {
                        string str = result.APIResponse.webRequestStatus;
                        return new PostingResult { posted = true, responseCode = result.APIResponse.responseCode };
                    }
                    else
                    {
                        throw new ConditionNotMetException("API call error - Response Code:" + result.APIResponse.responseCode + ". Response Message:" + result.APIResponse.message); //message result.APIResponse.webRequestStatus
                    }
                }
                else
                {
                    throw new APIErrorException("API call Error - Kindly contact the administrator.");
                }

            }
        }



    }

}

