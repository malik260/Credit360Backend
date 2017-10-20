using FintrakBanking.Entities.Models;
using FintrakBanking.Repositories.Setups.General;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.Credit.Monitoring
{
    public partial class SelfLiquidatingLoanExpiry : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FinTrakBankingContext context = new FinTrakBankingContext();
                GeneralSetupRepository genSetup = new GeneralSetupRepository(context);

                ReportParameter date = new ReportParameter("currentDate", genSetup.GetApplicationDate().ToShortDateString());

                selfLiqLoanExpRv.LocalReport.SetParameters(new ReportParameter[] { date });
                selfLiqLoanExpRv.LocalReport.Refresh();
            }
        }
    }
}