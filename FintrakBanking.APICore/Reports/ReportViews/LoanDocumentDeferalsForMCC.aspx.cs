using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class LoanDocumentDeferalsForMCC : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                startDate.Text = Request.QueryString["startDate"];
                companyId.Text = Request.QueryString["companyId"];
                branchCode.Text = Request.QueryString["branchCode"];

                ReportParameter sDate = new ReportParameter("startDate", startDate.Text);

                ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate });
                ReportViewer.LocalReport.Refresh();
            }
        }
    }
}