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
    public partial class ExpiredSelfLiquidatingLoans : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FinTrakBankingContext context = new FinTrakBankingContext();
                GeneralSetupRepository generalSetup = new GeneralSetupRepository(context);
                ReportParameter date = new ReportParameter("currentDate", generalSetup.GetApplicationDate().ToShortDateString());

                selfLiqLoanRv.LocalReport.SetParameters(new ReportParameter[] { date });
                selfLiqLoanRv.LocalReport.Refresh();
            }
        }
    }
}