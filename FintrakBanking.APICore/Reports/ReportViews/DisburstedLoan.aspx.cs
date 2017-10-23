using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class DisburstedLoan : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //startDate = 08 - May - 17 & endDate = 

                 startDate .Text  = "25-May-17";// Request.QueryString["startDate"];
                endDate.Text = "25-Oct-17";// Request.QueryString["endDdate"];
                 companyId.Text = "1";// Request.QueryString["companyId"];
                ReportViewer.LocalReport.Refresh();
            }
            }
    }
}