 
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class PostedFinancialTransactions : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ReportParameter data = new ReportParameter("selectedDate", DateTime.Now.ToString());
                StartDate.Text = Request.QueryString["startDate"];
                EndDate.Text = Request.QueryString["endDate"];
                PostedByStaffId.Text = Request.QueryString["staffId"];
                CompanyId.Text = Request.QueryString["companyId"];
                BranchId.Text = Request.QueryString["branchId"];
                ExcludeSystem.Text = Request.QueryString["excludeSystem"];

                ReportParameter sDate = new ReportParameter("StartDate", StartDate.Text);
                ReportParameter eDate = new ReportParameter("EndDate", EndDate.Text);

                ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate, eDate });
                ReportViewer.LocalReport.Refresh();

            }
        }
    }
}