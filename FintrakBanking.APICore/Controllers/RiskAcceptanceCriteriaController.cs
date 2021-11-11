using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Risk;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.Risk;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/rac")] 
    public class RiskAcceptanceCriteriaController : ApiControllerBase
    {
        private IRiskAcceptanceCriteriaRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public RiskAcceptanceCriteriaController(IRiskAcceptanceCriteriaRepository _repo)
        {
            this.repo = _repo;
        }

        #region risk-acceptance-criteria

        [HttpPost]
        [ClaimsAuthorization]
        [Route("risk-acceptance-criteria-input")]
        public HttpResponseMessage GetRiskAcceptanceCriteriaByProduct(RiskAcceptanceCriteriaViewModel model)
        {
            RiskAcceptanceCriteriaViewModel response = repo.GetRiskAcceptanceCriteriaByProduct(model);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response?.categories?.Count() });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("risk-acceptance-criteria/product/{productId}/target/{targetId}")]
        public HttpResponseMessage GetRiskAcceptanceCriteriaByProductAndTarget(int productId, int targetId)
        {
            RiskAcceptanceCriteriaViewModel response = repo.GetRiskAcceptanceCriteriaByProductAndTarget(productId, targetId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.categories.Count() });
        }

       
        

        #endregion

        #region RacCategory 
        [HttpGet]
        [ClaimsAuthorization]
        [Route("category")]
        public HttpResponseMessage GetRacCategorys()
        {
            IEnumerable<RacCategoryViewModel> response = repo.GetRacCategorys();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("category/{id}")]
        public HttpResponseMessage GetRacCategory(int id)
        {
            RacCategoryViewModel response = repo.GetRacCategory(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("category-type-id/{id}")]
        public HttpResponseMessage GetRacCategoryType(int id)
        {
            IEnumerable<RacCategoryViewModel> response = repo.GetRacCategoryType(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("category")]
        public HttpResponseMessage AddRacCategory([FromBody] RacCategoryViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddRacCategory(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("category/{id}")]
        public HttpResponseMessage UpdateRacCategory([FromBody] RacCategoryViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateRacCategory(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("category/{id}")]
        public HttpResponseMessage DeleteRacCategory(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteRacCategory(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region RacDefinition
        [HttpGet]
        [ClaimsAuthorization]
        [Route("definition")]
        public HttpResponseMessage GetRacDefinitions()
        {
            IEnumerable<RacDefinitionViewModel> response = repo.GetRacDefinitions();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("definition/{id}")]
        public HttpResponseMessage GetRacDefinition(int id)
        {
            RacDefinitionViewModel response = repo.GetRacDefinition(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("definition")]
        public HttpResponseMessage AddRacDefinition([FromBody] RacDefinitionViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddRacDefinition(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("definition/{id}")]
        public HttpResponseMessage UpdateRacDefinition([FromBody] RacDefinitionViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateRacDefinition(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("definition/{id}")]
        public HttpResponseMessage DeleteRacDefinition(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteRacDefinition(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region RacInputType
        [HttpGet]
        [ClaimsAuthorization]
        [Route("input-type")]
        public HttpResponseMessage GetRacInputTypes()
        {
            IEnumerable<RacInputTypeViewModel> response = repo.GetRacInputTypes();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("input-type/{id}")]
        public HttpResponseMessage GetRacInputType(int id)
        {
            RacInputTypeViewModel response = repo.GetRacInputType(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("input-type")]
        public HttpResponseMessage AddRacInputType([FromBody] RacInputTypeViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddRacInputType(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("input-type/{id}")]
        public HttpResponseMessage UpdateRacInputType([FromBody] RacInputTypeViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateRacInputType(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("input-type/{id}")]
        public HttpResponseMessage DeleteRacInputType(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteRacInputType(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region Details
        [HttpGet]
        [ClaimsAuthorization]
        [Route("detail")]
        public HttpResponseMessage GetRacDetails()
        {
            IEnumerable<RacDetailViewModel> response = repo.GetRacDetails();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("detail/{id}")]
        public HttpResponseMessage GetRacDetail(int id)
        {
            RacDetailViewModel response = repo.GetRacDetail(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("detail")]
        public HttpResponseMessage AddRacDetail([FromBody] RacDetailViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddRacDetail(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("detail/{id}")]
        public HttpResponseMessage UpdateRacDetail([FromBody] RacDetailViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateRacDetail(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("detail/{id}")]
        public HttpResponseMessage DeleteRacDetail(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteRacDetail(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region RacItem
        [HttpGet]
        [ClaimsAuthorization]
        [Route("item")]
        public HttpResponseMessage GetRacItems()
        {
            IEnumerable<RacItemViewModel> response = repo.GetRacItems();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("item/{id}")]
        public HttpResponseMessage GetRacItem(int id)
        {
            RacItemViewModel response = repo.GetRacItem(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("item-search")]
        public HttpResponseMessage GetRacItemSearch(string searchQuery)
        {
           IEnumerable< RacItemViewModel> response = repo.GetRacItem(searchQuery);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("approval-level")]
        public HttpResponseMessage GetApprovalLevel()
        {
            IEnumerable<ApprovalLevelViewModel> response = repo.GetApprovalLevel(token.GetCompanyId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("item")]
        public HttpResponseMessage AddRacItem([FromBody] RacItemViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddRacItem(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("item/{id}")]
        public HttpResponseMessage UpdateRacItem([FromBody] RacItemViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateRacItem(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("item/{id}")]
        public HttpResponseMessage DeleteRacItem(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteRacItem(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region RacOptions
        [HttpGet]
        [ClaimsAuthorization]
        [Route("option")]
        public HttpResponseMessage GetRacOptions()
        {
            IEnumerable<RacOptionViewModel> response = repo.GetRacOptions();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("option/{id}")]
        public HttpResponseMessage GetRacOption(int id)
        {
            RacOptionViewModel response = repo.GetRacOption(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("option")]
        public HttpResponseMessage AddRacOption([FromBody] RacOptionViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddRacOption(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("option/{id}")]
        public HttpResponseMessage UpdateRacOption([FromBody] RacOptionViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateRacOption(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("option/{id}")]
        public HttpResponseMessage DeleteRacOption(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteRacOption(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region
        [HttpGet]
        [ClaimsAuthorization]
        [Route("option-item")]
        public HttpResponseMessage GetRacOptionItems()
        {
            IEnumerable<RacOptionItemViewModel> response = repo.GetRacOptionItems();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("option-item/{id}")]
        public HttpResponseMessage GetRacOptionItem(int id)
        {
            RacOptionItemViewModel response = repo.GetRacOptionItem(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("option-item")]
        public HttpResponseMessage AddRacOptionItem([FromBody] RacOptionItemViewModel model)
        {
            model.userBranchId = (short)token.GetBranchId;
            model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.createdBy = token.GetStaffId;
            model.companyId = token.GetCompanyId;
            var response = repo.AddRacOptionItem(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("option-item/{id}")]
        public HttpResponseMessage UpdateRacOptionItem([FromBody] RacOptionItemViewModel model, int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.UpdateRacOptionItem(model, id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("option-item/{id}")]
        public HttpResponseMessage DeleteRacOptionItem(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteRacOptionItem(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region racCategoryType
        [HttpGet]
        [ClaimsAuthorization]
        [Route("category-type")]
        public HttpResponseMessage GetRacCategoryType()
        {
            var response = repo.GetAllRacCategoryType();
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("category-type/{id}")]
        public HttpResponseMessage GetRacCategoryTypeById(int id)
        {
            var response = repo.GetRacCategoryTypeById(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("category-type")]
        public HttpResponseMessage AddRacCategoryType([FromBody] RacCategoryTypeViewModel model)
        {
            bool response = repo.AddRacCategoryType(model);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("category-type/{id}")]
        public HttpResponseMessage UpdateRacCategoryType([FromBody] RacCategoryTypeViewModel model, int id)
        {
            bool response = repo.UpdateRacCategoryTypeById(model, id);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("category-type/{id}")]
        public HttpResponseMessage DeleteRacCategoryType(int id)
        {
            bool response = repo.DeleteRacCategoryTypeById(id);
            if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        }


        #endregion

        [HttpGet]
        [ClaimsAuthorization]
        [Route("conditional-operator")]
        public HttpResponseMessage GetConditionalOperators()
        {
            IEnumerable<ConditionalOperatorViewModel> response = repo.GetConditionalOperators();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("defined-function")]
        public HttpResponseMessage GetDefinedFunctions()
        {
            IEnumerable<DefinedFunctionViewModel> response = repo.GetDefinedFunctions();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }
        




        [HttpGet]
        [ClaimsAuthorization]
        [Route("defined-category-type/{productId}/productId")]
        public HttpResponseMessage GetRacCategoryTypes(int productId)
        {
            IEnumerable<RacCategoryViewModel> response = repo.GetRacCategoryTypes(productId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("saved-rac-for-a-loan")]
        public HttpResponseMessage GetSavedRiskAcceptanceCriteria(RiskAcceptanceCriteriaViewModel model)
        {
            RiskAcceptanceCriteriaViewModel response = repo.GetSavedRiskAcceptanceCriteria(model.productId, model.loanApplicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.categories.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("exit-rac-category-type/{productId}/productId/{racCategoryTypeId}/racCategoryTypeId")]
        public HttpResponseMessage RacCategoryTypeExist(int productId, int racCategoryTypeId)
        {
            bool response = repo.RacCategoryTypeExist(productId, racCategoryTypeId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response});
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("rac-details/{targetId}")]
        public HttpResponseMessage GetRacDetails([FromUri] int targetId)
        {
           List<RacCategoryTypeViewModel> response = repo.GetRacDetails(targetId); 
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response });
        }

    }
}
