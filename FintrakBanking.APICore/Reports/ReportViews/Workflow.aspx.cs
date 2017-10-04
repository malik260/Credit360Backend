using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class Workflow : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                operationId.Text = Request.QueryString["operationId"];
                companyId.Text = Request.QueryString["companyId"];
                 
                ReportViewer.LocalReport.Refresh();
            }
        }
    }
}