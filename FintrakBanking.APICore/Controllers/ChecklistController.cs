using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Setups;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using System.Web;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Common.Enum;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class ChecklistController : ApiControllerBase
    {
        private IChecklistRepository repo;
        private ILoanApplicationRepository loanApplicationRepo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public ChecklistController(IChecklistRepository _repo, ILoanApplicationRepository _loanApplicationRepo)
        {
            repo = _repo;
            loanApplicationRepo = _loanApplicationRepo;
        }

        #region Checklist Definition
        [HttpPost]
        [Route("checklist-definition")]
        public HttpResponseMessage AddChecklistDefinition([FromBody] ChecklistDefinitionViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.AddChecklistDefinition(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

        [HttpPost]
        [Route("checklist-definition/multiple")]
        public HttpResponseMessage AddMultipleChecklistDefinition([FromBody] List<ChecklistDefinitionViewModel> model)
        {
            try
            {
                var recordId = repo.AddMultipleChecklistDefinition(model);
                if (recordId)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
      new { success = true, result = recordId, message = "Checklist Definitions has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Checklist Definition not created" });
            }

            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
      new { success = false, message = $"There was an error creating these records {e.Message}" });
            }

        }

        [HttpPost]
        [Route("checklist-definition/multiple-items")]
        public HttpResponseMessage AddMultipleChecklistDefinitionWithMultipleItems([FromBody] ChecklistDefinitionViewModel model)
        {
            try
            {
                var recordId = repo.AddMultipleChecklistDefinitionWithMultipleItems(model);
                if (recordId)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = recordId, message = "Checklist Definitions have been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Checklist Definitions not created" });
            }

            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"There was an error creating these records {e.Message}" });
            }

        }

        [HttpGet]
        [Route("checklist-definition")]
        public HttpResponseMessage GetAllChecklistDefinition()
        {
            try
            {
                var data = repo.GetAllChecklistDefinition();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }

        }

        [HttpGet]
        [Route("checklist-definition/{CheckListDefinitionId}")]
        public HttpResponseMessage GetAllChecklistDefinitionById(short CheckListDefinitionId)
        {
            try
            {
                var data = repo.GetAllChecklistDefinitionById(CheckListDefinitionId);
                return Request.CreateResponse(HttpStatusCode.OK,
           new { success = true, result = data, count = 1 });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
           new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }

        }

        [HttpPut]
        [Route("checklist-definition/{CheckListDefinitionId}")]
        public HttpResponseMessage UpdateChecklistDefinition(short CheckListDefinitionId, [FromBody] ChecklistDefinitionViewModel model)
        {

            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.UpdateChecklistDefinition(CheckListDefinitionId, model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, message = "The record has been updated successfully" });

                }
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = $"There was an error updating this record {e.Message}" });
            }

        }

        [HttpDelete]
        [Route("checklist-definition/{CheckListDefinitionId}")]
        public HttpResponseMessage DeleteChecklistDefinition(short CheckListDefinitionId)
        {

            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = CommonHelpers.GetUserIP()
                };

                repo.DeleteChecklistDefinition(CheckListDefinitionId, user);

                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = true, result = CheckListDefinitionId, message = "record has been deleted successfully" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = ex.Message });
            }

        }
        #endregion

        #region Checklist Detail
        [HttpPost]
        [Route("checklist-detail")]
        public HttpResponseMessage AddChecklistDetail([FromBody] ChecklistDetailViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.AddChecklistDetail(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }
        [HttpPost]
        [Route("checklist-detail-multiple")]
        public HttpResponseMessage AddChecklistDetailMultiple([FromBody] List<ChecklistDetailViewModel> model)
        {
            try
            {
                if(model.Count <= 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "Please select all checklist to continue" });
                }
                var data = repo.AddMultipleChecklistDetails(model, token.GetStaffId, (short)token.GetBranchId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }
        [HttpGet]
        [Route("checklist-detail")]
        public HttpResponseMessage GetAllChecklistDetail()
        {
            try
            {
                var data = repo.GetAllChecklistDetail();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("checklist-detail/")]
        public HttpResponseMessage GetAllChecklistDetailByProductId(int targetId)
        {
            try
            {
               // var targetTypeId = (int)CheckListTargetTypeEnum.Loan;
                var data = repo.GetAllChecklistDefinitionByProductId(targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("checklist-detail/{ChecklistId}")]
        public HttpResponseMessage GetAllChecklistById(int ChecklistId)
        {
            try
            {
                var data = repo.GetAllChecklistDetailById(ChecklistId);
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = 1 });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut]
        [Route("checklist-detail/{ChecklistId}")]
        public HttpResponseMessage UpdateChecklistDetail(int ChecklistId, [FromBody] ChecklistDetailViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.UpdateChecklistDetail(ChecklistId, model);

                if (data)
                {

                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, message = "The record has been updated successfully" });

                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete]
        [Route("checklist-detail/{ChecklistId}")]
        public HttpResponseMessage DeleteLoanChecklist(int ChecklistId)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = CommonHelpers.GetUserIP()
                };

                repo.DeleteChecklistDetail(ChecklistId, user);

                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = true, result = ChecklistId, message = "record has been deleted successfully" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = ex.Message });
            }

        }
        #endregion

        #region CheckList Items
        [HttpPost]
        [Route("checklist-item")]
        public HttpResponseMessage AddChecklistItem([FromBody] ChecklistItemViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.AddChecklistItem(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
             new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }



        [HttpPost]
        [Route("checklist-item/multiple")]
        public HttpResponseMessage AddMultipleChecklistItem([FromBody] List<ChecklistItemViewModel> model)
        {
            try
            {
                var recordId = repo.AddMultipleChecklistItem(model);
                if (recordId)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
              new { success = true, result = recordId, message = "Checklist items has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Checklist items not created" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("checklist-item")]
        public HttpResponseMessage GetAllChecklistItem()
        {
            try
            {
                var data = repo.GetAllChecklistItem();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"Error: {e.Message}" });
            }

        }

        [HttpGet]
        [Route("checklist-item/{ChecklistId}")]
        public HttpResponseMessage GetAllChecklistItemById(int CheckListItemId)
        {
            try
            {
                var data = repo.GetAllChecklistItemById(CheckListItemId);
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = true, result = data, count = 1 });
            }
            catch (Exception ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }

        }

        [HttpPut]
        [Route("checklist-item/{CheckListItemId}")]
        public HttpResponseMessage UpdateChecklistItem(int CheckListItemId, [FromBody] ChecklistItemViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.UpdateChecklistItem(CheckListItemId, model);

                if (data)
                {

                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, message = "The record has been updated successfully" });

                }
                return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = $"There was an error updating this record {e.Message}" });
            }

        }

        [HttpDelete]
        [Route("checklist-item/{CheckListItemId}")]
        public HttpResponseMessage DeleteChecklistItem(int CheckListItemId)
        {
            try
            {

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = CommonHelpers.GetUserIP()
                };

                repo.DeleteChecklistItem(CheckListItemId, user);

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = true, result = CheckListItemId, message = "record has been deleted successfully" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = ex.Message });
            }


        }
        #endregion

        #region CheckList Select List
        [HttpGet]
        [Route("checklist-status")]
        public HttpResponseMessage GetAllChecklistStatus()
        {
            try
            {
                var data = repo.GetAllChecklistStatus();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("checklist-target-type")]
        public HttpResponseMessage GetAllChecklistTargetType()
        {
            try
            {
                var data = repo.GetAllChecklistTargetType();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion

        #region Loan Application CheckList
        [HttpGet]
        [Route("loan-application-checklist")]
        public HttpResponseMessage GetLoanApplicationsAwaitingCheckList()
        {
            try
            {
                var data = loanApplicationRepo.GetLoanApplicationsAwaitingCheckList(token.GetCompanyId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion
    }
}