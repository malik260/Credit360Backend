using FintrakBanking.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Validation;
using FintrakBanking.Common.CustomException;


namespace FintrakBanking.Repositories.Validetion
{
   public  class OverDraftValidation : IOverDraftValidation
    {
        private FinTrakBankingContext context;

        private IGeneralSetupRepository generalSetup;

        public OverDraftValidation( FinTrakBankingContext context,
          IGeneralSetupRepository generalSetup)
        {
            this.context = context;
            this.generalSetup = generalSetup;
        }

        public bool ODTopupValidation(int loanId, DateTime topupDate, decimal odLimit)
        { 
          var data =  context.TBL_LOAN_REVOLVING.Find(loanId);
             
            if (data.LOANSTATUSID == (int)LoanStatusEnum.Inactive && data.MATURITYDATE.Date > topupDate.Date)
            {
                if(data.MATURITYDATE.Date < topupDate.Date)
                {
                    throw new SecureException("The tenor for the top-up amount is not expected to exceed the expiry date of the current limit");
                }
                return true; 
            }
            else
            {
                throw new SecureException("Limit has experied or is inactive");
            }          
        }
    }
}
