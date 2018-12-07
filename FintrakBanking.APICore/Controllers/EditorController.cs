using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Mvc;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class EditorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpOptions]
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase upload, string CKEditorFuncNum, string CKEditor, string langCode)
        {
            var file = upload;
            string path = String.Empty;
            string newFileName = String.Empty;

            if (file != null && file.ContentLength > 0)
            {
                string fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
                if (fileExtension == ".jpg"
                    || fileExtension == ".jpeg"
                    || fileExtension == ".png"
                    )
                {
                    var fileName = System.IO.Path.GetFileName(file.FileName);
                    newFileName = this.UniqueFileName(fileName);
                    //var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/Editor/Uploads"), fileName);
                    path = System.IO.Path.Combine(Server.MapPath("~/Content/Images/Editor/Uploads"), newFileName); // + guid
                    file.SaveAs(path);
                }
            }

            ViewBag.FileName = newFileName;
            ViewBag.FuncName = CKEditorFuncNum;
            ViewBag.Message = "Image was saved correctly";
            // Response.AddHeader("Access-Control-Allow-Origin", "*");

            var domain = Request.Url.AbsoluteUri.Replace(Request.Url.AbsolutePath, String.Empty);
            var src = domain + "/Content/Images/Editor/Uploads/" + newFileName;

            return Json(new { uploaded = 1, fileName = file.FileName, url = src });
        }

        private string UniqueFileName(string actualfilename)
        {
            var guid = Guid.NewGuid();
            actualfilename = actualfilename.ToLower();
            return guid.ToString() + "-" + actualfilename.Replace(" ", "-");
        }

    }
}
