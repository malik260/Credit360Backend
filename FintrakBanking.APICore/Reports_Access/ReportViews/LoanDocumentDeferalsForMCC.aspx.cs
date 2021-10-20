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
               // DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
                companyId.Text = Request.QueryString["companyId"];
                branchCode.Text = Request.QueryString["branchCode"];

                ReportParameter sDate = new ReportParameter("startDate", startDate.Text);

                string exportOption = "PDF";
                RenderingExtension extension = ReportViewer.LocalReport.ListRenderingExtensions().ToList().Find(x => x.Name.Equals(exportOption, StringComparison.CurrentCultureIgnoreCase));
                if (extension != null)
                {
                    System.Reflection.FieldInfo fieldInfo = extension.GetType().GetField("m_isVisible", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    fieldInfo.SetValue(extension, false);
                }

                ReportViewer.LocalReport.SetParameters(new ReportParameter[] { sDate });
                ReportViewer.LocalReport.Refresh();
            }
        }
    }
}