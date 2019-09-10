using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Interfaces.Media;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")] // TODO: modify!
    public class ProductDocumentMapingController : ApiControllerBase
    {
        private IDocumentCategoryRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ProductDocumentMapingController(IDocumentCategoryRepository _repo)
        {
            this.repo = _repo;
        }
          
        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-category")]
        public HttpResponseMessage GetDocumentCategorys()
        {
            IEnumerable<DocumentCategoryViewModel> response = repo.GetDocumentCategorys();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("document-category/{id}")]
        public HttpResponseMessage GetDocumentCategory(int id)
        {
            DocumentCategoryViewModel response = repo.GetDocumentCategory(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("document-category")]
        public HttpResponseMessage AddDocumentCategory([FromBody] DocumentCategoryViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddDocumentCategory(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("document-category/{id}")]
        public HttpResponseMessage UpdateDocumentCategory([FromBody] DocumentCategoryViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateDocumentCategory(model,id,user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, message = "The record has been updated successfully", count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("document-category/{id}")]
        public HttpResponseMessage DeleteDocumentCategory(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteDocumentCategory(id,user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

    }
}
