using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Admin;
using Microsoft.Reporting.WebForms;

namespace FintrakBanking.APICore.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult FintrakReport()
        {
            var data = new List<AuditViewModel>();
            using (FinTrakBankingContext context = new FinTrakBankingContext())
            {
                DateTime startDate = Convert.ToDateTime("03/01/2018");
                DateTime endDate = Convert.ToDateTime("03/30/2018");
                data = (from _audit in context.TBL_AUDIT
                           join atype in context.TBL_AUDIT_TYPE on _audit.AUDITTYPEID equals atype.AUDITTYPEID
                           join st in context.TBL_STAFF on _audit.STAFFID equals st.STAFFID
                           join u in context.TBL_PROFILE_USER on st.STAFFID equals u.STAFFID
                           join b in context.TBL_BRANCH on _audit.BRANCHID equals b.BRANCHID
                           where (_audit.SYSTEMDATETIME >= startDate && _audit.SYSTEMDATETIME <= endDate)
                           select new AuditViewModel
                           {
                               auditId = _audit.AUDITID,
                               applicationDate = _audit.APPLICATIONDATE,
                               auditType = atype.AUDITTYPENAME,
                               details = _audit.DETAIL,
                               firstName = st.FIRSTNAME,
                               lastName = st.LASTNAME,
                               systemDate = _audit.SYSTEMDATETIME,
                               username = u.USERNAME,
                               url = _audit.URL,
                               branchName = b.BRANCHNAME,
                               ipAddress = _audit.IPADDRESS
                           }).ToList();

            }

            ReportViewer reportViewer = new ReportViewer();
            reportViewer.ProcessingMode = ProcessingMode.Local;

            reportViewer.LocalReport.ReportPath = Request.MapPath(Request.ApplicationPath) + @"Report File MVC\AuditTrail.rdlc";
             reportViewer.LocalReport.DataSources.Add(new ReportDataSource("DataSet1", data));

            ViewBag.ReportViewer = reportViewer;

            return View();
        }

        // GET: Report/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Report/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Report/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Report/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Report/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Report/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Report/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
