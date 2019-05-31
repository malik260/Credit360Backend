using FintrakBanking.ReportObjects.ReportingObjects;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FintrakBanking.APICore.Reports.ReportViews
{
    public partial class OutPutTemplate : System.Web.UI.Page
    {
        public OutPutTemplate()
        {

        }

        Warning[] warnings;
        string[] streamIds;
        string contentType;
        string encoding;
        string extension;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                
            }


            
        }

        void GenerateOutPutDocument() {

            var dateInfo = Request.QueryString["startDate"];
            DateTime startDate = DateTime.ParseExact(Request.QueryString["startDate"], "dd-MM-yyyy", null);
            DateTime endDate = DateTime.ParseExact(Request.QueryString["endDate"], "dd-MM-yyyy", null);
            int auditTypeId = Int32.Parse(Request.QueryString["auditTypeId"]);
            string username = Request.QueryString["username"];
            string inputDateInfo = Request.QueryString["key1"];
            string inputHashValue = Request.QueryString["key2"];

            Audit audit = new Audit();
            var data = audit.GetAuditTrailByParam(startDate, endDate, username, auditTypeId);

            this.ReportViewer.LocalReport.DataSources.Clear();
            ReportDataSource reportDataSource = new ReportDataSource();
            reportDataSource.Value = data;
            reportDataSource.Name = "Audit";

            ReportViewer.ProcessingMode = ProcessingMode.Local;
            ReportViewer.LocalReport.ReportPath = Server.MapPath("~/Reports/Report/Error.rdlc");
            ReportViewer.LocalReport.DataSources.Clear();
            ReportViewer.LocalReport.DataSources.Add(reportDataSource);

            //Export the RDLC Report to Byte Array.
            byte[] bytes = ReportViewer.LocalReport.Render("PDF", null, out contentType, out encoding, out extension, out streamIds, out warnings);

            //Download the RDLC Report in Word, Excel, PDF and Image formats.
            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = contentType;
            Response.AppendHeader("Content-Disposition", "attachment; filename=RDLC." + extension);
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
        }
    }
}