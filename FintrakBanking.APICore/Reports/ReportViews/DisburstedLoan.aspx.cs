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

                hdf_startDate .Value  = Request.QueryString["startDate"];
                hdf_endDdate.Value = Request.QueryString["endDdate"];
                hdf_companyId .Value = Request.QueryString["companyId"];
                ReportViewer.LocalReport.Refresh();
            }
            }
    }
}