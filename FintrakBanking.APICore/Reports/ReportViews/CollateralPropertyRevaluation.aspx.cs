using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Entities.Models;
using FintrakBanking.Repositories.Setups.General;
using Microsoft.Reporting.WebForms;
using System;

namespace FintrakBanking.APICore.Reports.Credit.Monitoring
{
    public partial class CollateralPropertyRevaluation : System.Web.UI.Page
    {
        private ApiControllerBase authCtrl = new ApiControllerBase();

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                startDate.Text = Request.QueryString["startDate"];
                endDate.Text = Request.QueryString["endDate"];
                collPropRv.LocalReport.Refresh();
            }

        }

    }

}