using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class Blacklist : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            startDate.Text = Request.QueryString["startDate"];
            endDate.Text = Request.QueryString["endDate"];
            customerCode.Text = Request.QueryString["customerCode"];
        }
    }
}