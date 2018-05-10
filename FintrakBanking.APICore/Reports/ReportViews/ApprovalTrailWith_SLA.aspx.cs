using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class ApprovalTrailWith_SLA : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                operationId.Text = Request.QueryString["operationId"];
                targetId.Text = Request.QueryString["loanApplicationId"];
                companyId.Text = Request.QueryString["companyId"];
                staffId.Text = Request.QueryString["staffId"];

                ReportViewer.LocalReport.Refresh();

            }
        }
    }
}