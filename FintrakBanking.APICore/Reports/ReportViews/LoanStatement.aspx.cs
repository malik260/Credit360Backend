using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class LoanStatement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                companyId.Text = Request.QueryString["companyId"]; //"1",
               
                loanId.Text = Request.QueryString["loanId"]; //"252"; 

                ReportViewer.LocalReport.Refresh();
               
            }
        }
    }
}