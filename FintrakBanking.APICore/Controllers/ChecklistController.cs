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

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class ChecklistController : ApiControllerBase
    {
        private IChecklistRepository repo;

        public ChecklistController(IChecklistRepository _repo)
        {
            this.repo = _repo;
        }

        #region Checklist Definition
        [HttpPost] [Route("checklist-definition")]
        public HttpResponseMessage AddChecklistDefinition(HttpRequestMessage request, [FromBody] ChecklistDefinitionViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null; // new TokenDecryptionHelper(this.HttpContext);

                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //model.applicationUrl = Request.Path.Value;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repo.AddChecklistDefinition(model);
                    if (data)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                  Created("", new { success = true, result = data, message = "The record has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                  Ok(new { success = false, message = "There was an error creating this record" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                  Ok(new { success = false, message = $"There was an error creating this record {e.Message}" }));
                }
                return response;
            });
        }

        [HttpPost] [Route("checklist-definition/multiple")]
        public HttpResponseMessage AddMultipleChecklistDefinition(HttpRequestMessage request, [FromBody] List<ChecklistDefinitionViewModel> model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    // new TokenDecryptionHelper(this.HttpContext);
                    var recordId = repo.AddMultipleChecklistDefinition(model);
                    if (recordId)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
           Ok(new { success = true, result = recordId, message = "Checklist Definitions has been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
           Ok(new { success = false, message = "Checklist Definition not created" }));
                }

                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
           Ok(new { success = false, message = $"There was an error creating these records {e.Message}" }));
                }
                return response;
            });
        }

        [HttpPost] [Route("checklist-definition/multiple-items")]
        public HttpResponseMessage AddMultipleChecklistDefinitionWithMultipleItems(HttpRequestMessage request, [FromBody] ChecklistDefinitionViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    var recordId = repo.AddMultipleChecklistDefinitionWithMultipleItems(model);
                    if (recordId)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = true, result = recordId, message = "Checklist Definitions have been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = false, message = "Checklist Definitions not created" }));
                }

                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = false, message = $"There was an error creating these records {e.Message}" }));
                }
                return response;
            });
        }

        [HttpGet] [Route("checklist-definition")]
        public HttpResponseMessage GetAllChecklistDefinition(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    var data = repo.GetAllChecklistDefinition();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = true, result = data, count = data.Count() }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
            });
        }

        [HttpGet] [Route("checklist-definition/{CheckListDefinitionId}")]
        public HttpResponseMessage GetAllChecklistDefinitionById(HttpRequestMessage request, short CheckListDefinitionId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllChecklistDefinitionById(CheckListDefinitionId);
                    response = request.CreateResponse(HttpStatusCode.OK,
                Ok(new { success = true, result = data, count = 1 }));
                }
                catch (System.Exception ex)
                {

                    response = request.CreateResponse(HttpStatusCode.OK,
                Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" }));
                }
                return response;
            });

        }

        [HttpPut]
        [Route("checklist-definition/{CheckListDefinitionId}")]
        public HttpResponseMessage UpdateChecklistDefinition(HttpRequestMessage request, short CheckListDefinitionId, [FromBody] ChecklistDefinitionViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //model.applicationUrl = Request.Path.Value;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repo.UpdateChecklistDefinition(CheckListDefinitionId, model);

                    if (data)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                        Created("", new { success = true, result = data, message = "The record has been updated successfully" }));

                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = false, message = "There was an error updating this record" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                      Ok(new { success = false, message = $"There was an error updating this record {e.Message}" }));
                }
                return response;
            });
        }

        [HttpDelete] [Route("checklist-definition/{CheckListDefinitionId}")]
        public HttpResponseMessage DeleteChecklistDefinition(HttpRequestMessage request, short CheckListDefinitionId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        // applicationUrl = Request.Path.Value,
                        // userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    };

                    repo.DeleteChecklistDefinition(CheckListDefinitionId, user);

                    response = request.CreateResponse(HttpStatusCode.OK,
                  Ok(new { success = true, result = CheckListDefinitionId, message = "record has been deleted successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                  Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }
        #endregion

        #region Checklist Detail
        [HttpPost] [Route("checklist-detail")]
        public HttpResponseMessage AddChecklistDetail(HttpRequestMessage request, [FromBody] ChecklistDetailViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //model.applicationUrl = Request.Path.Value;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repo.AddChecklistDetail(model);
                    if (data)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                    Created("", new { success = true, result = data, message = "The record has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                 Ok(new { success = false, message = "There was an error creating this record" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                 Ok(new { success = false, message = $"There was an error creating this record {e.Message}" }));
                }

                return response;
            });
        }

        [HttpGet] [Route("checklist-detail")]
        public HttpResponseMessage GetAllChecklistDetail(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {

                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    var data = repo.GetAllChecklistDetail();
                    if (!data.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = true, result = data, count = data.Count() }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
            });
        }

        [HttpGet] [Route("checklist-detail/{ChecklistId}")]
        public HttpResponseMessage GetAllChecklistById(HttpRequestMessage request, int ChecklistId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllChecklistDetailById(ChecklistId);
                    response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = true, result = data, count = 1 }));
                }
                catch (System.Exception ex)
                {

                    response = request.CreateResponse(HttpStatusCode.OK,
                    Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" }));
                }
                return response;
            });
        }

        [HttpPut] [Route("checklist-detail/{ChecklistId}")]
        public HttpResponseMessage UpdateChecklistDetail(HttpRequestMessage request, int ChecklistId, [FromBody] ChecklistDetailViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //model.applicationUrl = Request.Path.Value;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repo.UpdateChecklistDetail(ChecklistId, model);

                    if (data)
                    {

                        response = request.CreateResponse(HttpStatusCode.OK,
                        Created("", new { success = true, result = data, message = "The record has been updated successfully" }));

                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "There was an error updating this record" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = $"There was an error updating this record {e.Message}" }));
                }
                return response;
            });
        }

        [HttpDelete] [Route("checklist-detail/{ChecklistId}")]
        public HttpResponseMessage DeleteLoanChecklist(HttpRequestMessage request, int ChecklistId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        //applicationUrl = Request.Path.Value,
                        //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    };

                    repo.DeleteChecklistDetail(ChecklistId, user);

                    response = request.CreateResponse(HttpStatusCode.OK,
                  Ok(new { success = true, result = ChecklistId, message = "record has been deleted successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                  Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }
        #endregion

        #region CheckList Items
        [HttpPost] [Route("checklist-item")]
        public HttpResponseMessage AddChecklistItem(HttpRequestMessage request, [FromBody] ChecklistItemViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //model.applicationUrl = Request.Path.Value;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repo.AddChecklistItem(model);
                    if (data)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                 Created("", new { success = true, result = data, message = "The record has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                 Ok(new { success = false, message = "There was an error creating this record" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                 Ok(new { success = false, message = $"There was an error creating this record {e.Message}" }));
                }
                return response;
            });
        }

        [HttpPost] [Route("checklist-item/multiple")]
        public HttpResponseMessage AddMultipleChecklistItem(HttpRequestMessage request, [FromBody] List<ChecklistItemViewModel> model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    var recordId = repo.AddMultipleChecklistItem(model);
                    if (recordId)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                   Ok(new { success = true, result = recordId, message = "Checklist items has been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                   Ok(new { success = false, message = "Checklist items not created" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                   Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }


        [HttpGet]
        [Route("checklist-item")]
        public HttpResponseMessage GetAllChecklistItem(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    var data = repo.GetAllChecklistItem();
                    if (!data.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = true, result = data, count = data.Count() }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                       Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
        });
        }

        [HttpGet][Route("checklist-item/{ChecklistId}")]
        public HttpResponseMessage GetAllChecklistItemById(HttpRequestMessage request,int CheckListItemId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllChecklistItemById(CheckListItemId);
                    response = request.CreateResponse(HttpStatusCode.OK,
                   Ok(new { success = true, result = data, count = 1 }));
                }
                catch (System.Exception ex)
                {

                    response = request.CreateResponse(HttpStatusCode.OK,
                   Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" }));
                }
                return response;
            });
        }

        [HttpPut][Route("checklist-item/{CheckListItemId}")]
        public HttpResponseMessage UpdateChecklistItem(HttpRequestMessage request,int CheckListItemId, [FromBody] ChecklistItemViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {

                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);
                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    //model.applicationUrl = Request.Path.Value;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = repo.UpdateChecklistItem(CheckListItemId, model);

                    if (data)
                    {

                        response = request.CreateResponse(HttpStatusCode.OK,
                      Created("", new { success = true, result = data, message = "The record has been updated successfully" }));

                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                      Ok(new { success = false, message = "There was an error updating this record" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                      Ok(new { success = false, message = $"There was an error updating this record {e.Message}" }));
                }
                return response;
            });
        }

        [HttpDelete] [Route("checklist-item/{CheckListItemId}")]
        public HttpResponseMessage DeleteChecklistItem(HttpRequestMessage request, int CheckListItemId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        //applicationUrl = Request.Path.Value,
                        //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                    };

                    repo.DeleteChecklistItem(CheckListItemId, user);

                    response = request.CreateResponse(HttpStatusCode.OK,
                   Ok(new { success = true, result = CheckListItemId, message = "record has been deleted successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                   Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });

        }
        #endregion

        #region CheckList Select List
        [HttpGet]
        [Route("checklist-status")]
        public HttpResponseMessage GetAllChecklistStatus(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    var data = repo.GetAllChecklistStatus();
                    if (!data.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                     Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                     Ok(new { success = true, result = data, count = data.Count() }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                     Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
            });
        }

        [HttpGet] [Route("checklist-target-type")]
        public HttpResponseMessage GetAllChecklistTargetType(HttpRequestMessage request)
        { HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    TokenDecryptionHelper token = null;// new TokenDecryptionHelper(this.HttpContext);

                    var data = repo.GetAllChecklistTargetType();
                    if (!data.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                     Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                     Ok(new { success = true, result = data, count = data.Count() }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                     Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
            }); }
        #endregion
    }
}