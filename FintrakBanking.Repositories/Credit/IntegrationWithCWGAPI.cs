using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FinTrakBanking.ThirdPartyIntegration.Finacle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.ThridPartyIntegration;

namespace FintrakBanking.Repositories.Credit
{
  public   class IntegrationWithCWGAPI : IIntegrationWithCWGAPI
    {
        private FinTrakBankingContext context;
        private TransactionPosting transaction;
        public IntegrationWithCWGAPI(FinTrakBankingContext context, TransactionPosting transaction)
        {
            this.context = context;
            this.transaction = transaction;
        }

        public OverdraftResponseViewModel OverDraftExtend(OverDraftExtendViewModel model)
        {
            ResponseMessage result = null;
            Task.Run(async () => result = await transaction.APIOverDraftExtend(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus == "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    return result.APIResponse;
                }
            }
            else
            {
                throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }

        }

        public OverdraftResponseViewModel OverDraftNormal(OverDraftNormalViewModel model)
        {
            ResponseMessage result = null;
            Task.Run(async () => result = await transaction.APIOverDraftNormal(model)).GetAwaiter().GetResult();
          
            if (result.Message.IsSuccessStatusCode)
            {
               if( result.APIResponse.webRequestStatus== "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    return result.APIResponse;
                }
            }
            else
            {                 
                throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
            
           
        }

        public OverdraftResponseViewModel OverDraftTopUp(OverDraftTopUpViewModel model)
        {
            ResponseMessage result = null;
            Task.Run(async () => result = await transaction.APIOverDraftTopUp(model)).GetAwaiter().GetResult();
            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus == "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    return result.APIResponse;
                }
            }
            else
            {
                throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
        }

        public OverdraftResponseViewModel TemporaryOverDraftNormal(TemporaryOverDraftViewModel model)
        {
            ResponseMessage result = null;
            Task.Run(async () => result = await transaction.APITemporaryOverDraftNormal(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus == "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    return result.APIResponse;
                }
            }
            else
            {
                throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
        }

        public OverdraftResponseViewModel TemporaryOverDraftRunning(TemporaryOverDraftViewModel model)
        {
            ResponseMessage result = null;
            Task.Run(async () => result = await transaction.APITemporaryOverDraftRunning(model)).GetAwaiter().GetResult();

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus == "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    return result.APIResponse;
                }
            }
            else
            {
                throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
        }

        public OverdraftResponseViewModel TemporaryOverDraftSingle(TemporaryOverDraftViewModel model)
        {
            ResponseMessage result = null;
            Task.Run(async () => result = await transaction.APITemporaryOverDraftSingle(model)).GetAwaiter().GetResult();            

            if (result.Message.IsSuccessStatusCode)
            {
                if (result.APIResponse.webRequestStatus == "FAILURE")
                {
                    throw new Exception(result.APIResponse.webRequestStatus);
                }
                else
                {
                    return result.APIResponse;
                }
            }
            else
            {

                throw new Exception(result.Message.StatusCode + "" + result.Message.ReasonPhrase);
            }
        }
    }
}
