using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class LoanDocumentDefferal : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                startDate.Text = Request.QueryString["startDate"];
                endDate.Text = Request.QueryString["endDate"];
                companyId.Text = Request.QueryString["companyId"];
                branchId.Text = Request.QueryString["branchId"];

                ReportParameter sDate = new ReportParameter("startDate", startDate.Text);
                ReportParameter eDate = new ReportParameter("endDate", endDate.Text);

                ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate, eDate });
                ReportViewer.LocalReport.Refresh();
            }
        }
    }
}