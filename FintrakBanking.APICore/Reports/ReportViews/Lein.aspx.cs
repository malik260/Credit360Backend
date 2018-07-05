using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class LoanCASAaccountWithLein : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                companyId.Text = Request.QueryString["companyId"];
                startDate.Text = Request.QueryString["startDate"];
                endDate.Text = Request.QueryString["endDate"];
                searchParamemter.Text = Request.QueryString["searchParamemter"];
                ReportViewer.LocalReport.Refresh();
            }
        }
    }
}