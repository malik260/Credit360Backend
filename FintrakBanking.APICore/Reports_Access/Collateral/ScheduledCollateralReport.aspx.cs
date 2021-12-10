using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Entities.Models;
using FintrakBanking.Repositories.Setups.General;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.Collateral
{
    public partial class ScheduledCollateralReport : System.Web.UI.Page
    {
        
            private ApiControllerBase authCtrl = new ApiControllerBase();

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //FinTrakBankingContext context = new FinTrakBankingContext();
                //GeneralSetupRepository generalSetup = new GeneralSetupRepository(context);
                //ReportParameter date = new ReportParameter("currentDate", generalSetup.GetApplicationDate().ToShortDateString());

                //collPropRv.LocalReport.SetParameters(new ReportParameter[] { date });
                //collPropRv.LocalReport.Refresh();
            }

        }
    }
    
}