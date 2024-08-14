using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Common.CustomException;
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
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.WorkFlow;

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
        [ClaimsAuthorization]
        [Route("checklist-definition")]
        public HttpResponseMessage AddChecklistDefinition([FromBody] ChecklistDefinitionViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.UserHostAddress;
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("checklist-definition/multiple")]
        public HttpResponseMessage AddMultipleChecklistDefinition([FromBody] List<ChecklistDefinitionViewModel> model)
        {
            try
            {
                foreach (var item in model)
                {
                    item.userBranchId = (short)token.GetBranchId;
                    item.userIPAddress = CommonHelpers.GetUserIP();
                    item.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                    item.createdBy = token.GetStaffId;
                    item.companyId = token.GetCompanyId;
                }

                var recordId = repo.AddMultipleChecklistDefinition(model);
                if (recordId)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
      new { success = true, result = recordId, message = "Checklist Definitions has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Checklist Definition not created" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
      new { success = false, message = $"There was an error creating these records {e.Message}" });
            }

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("checklist-definition/multiple-items")]
        public HttpResponseMessage AddMultipleChecklistDefinitionWithMultipleItems([FromBody] ChecklistDefinitionViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var recordId = repo.AddMultipleChecklistDefinitionWithMultipleItems(model);

                if (recordId)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = recordId, message = "Checklist Definitions have been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Checklist Definitions not created" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"There was an error creating these records {e.Message}" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }

        }
       
        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-item-simulation/")]
        public HttpResponseMessage GetChecklistItemSimulationDetails(int productId)
        {
            try
            {
                var data = repo.GetChecklistItemSimulationDetails(productId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-definition-checklisttype/")]
        public HttpResponseMessage GetChecklistDefinitionByApprovalLevelCheckListType(int operationId, int checklistTypeId, int? productId, int loanTargetId,int? customerId=null)
        {
            try
            {
                var data = repo.GetChecklistDefinitionByApprovalLevelCheckListType(token.GetStaffId, productId, loanTargetId, operationId, checklistTypeId, customerId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-definition-checklisttype-view/")]
        public HttpResponseMessage GetChecklistDefinitionByApprovalLevelCheckListTypeView(int operationId, int checklistTypeId, int? productId, int loanTargetId,int customerId)
        {
            try
            {
                var data = repo.GetChecklistDefinitionByApprovalLevelCheckListType(token.GetStaffId, productId, loanTargetId, operationId, checklistTypeId, customerId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-type")]
        public HttpResponseMessage GetAllChecklistType()
        {
            try
            {
                var data = repo.GetAllChecklistType();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-type-byapprovallevel")]
        public HttpResponseMessage GetChecklistTypeByApprovalLevel(int operationId, int productClassProcessId)
        {
            try
            {
                var data = repo.GetChecklistTypeByApprovalLevel(token.GetStaffId, token.GetCompanyId, operationId, productClassProcessId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-detail-valitation/")]
        public HttpResponseMessage GetChecklistByCheckListTypeAndTargetId(int targetId, int checklistTypeId, bool isCamChecklist, int? customerId=null)
        {
            try
            {
                var data = repo.GetChecklistByCheckListTypeAndTargetId(targetId, checklistTypeId, isCamChecklist, customerId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }

        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("checklist-type-mapping/{checklistTypeMappingId}")]
        public HttpResponseMessage DeleteChecklistTypeMapping(int checklistTypeMappingId)
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

                repo.DeleteChecklistTypeMapping(checklistTypeMappingId, user);

                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = true, result = checklistTypeMappingId, message = "record has been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-definition/{CheckListDefinitionId}")]
        public HttpResponseMessage GetAllChecklistDefinitionById(short CheckListDefinitionId)
        {
            try
            {
                var data = repo.GetAllChecklistDefinitionById(CheckListDefinitionId);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), count = data.Count() });

            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = $"There was an error fetching the records {ex.Message}"
                });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-definition/mapped/approval-level/{approvalLevelId}/product/{productId}")]
        public HttpResponseMessage GetAllChecklistDefinitionByApprovalLevelId(short approvalLevelId, short productId)
        {
            try
            {
                var data = repo.GetAllMappedChecklistDefinitionByApprovalLevelAndProduct(approvalLevelId, productId);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList(), count = data.Count() });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), count = data.Count() });

            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = $"There was an error fetching the records {ex.Message}"
                });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-definition/unmapped/approval-level/{approvalLevelId}/product/{productId}")]
        public HttpResponseMessage GetUnmappedChecklistDefintionToApprovalLevel(short approvalLevelId, short productId)
        {
            try
            {
                var data = repo.GetAllUnmappedChecklistItemsToApprovalLevelAndProduct(approvalLevelId, productId);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        success = true,
                        result = data.ToList(),
                        count = data.Count()
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    result = data.ToList(),
                    count = data.Count()
                });

            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = $"There was an error fetching the records {ex.Message}"
                });
            }

        }

        [HttpPut]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                 new { success = false, message = $"There was an error updating this record {e.Message}" });
            }

        }

        [HttpDelete]
        [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = ex.Message });
            }

        }
        #endregion

        #region Checklist Detail
        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-details-targetid/")]
        public HttpResponseMessage GetChecklistDetailsById(int targetId)
        {
            try
            {
                var data = repo.GetChecklistByTargetId(targetId);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = data.ToList(), count = data.Count() });

            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    success = false,
                    message = $"There was an error fetching the records {ex.Message}"
                });
            }

        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("checklist-detail")]
        public HttpResponseMessage AddChecklistDetail([FromBody] ChecklistDetailViewModel model)
        {
            try
            {
                string createUpdate = "";
                if (model.checklistId != 0 || model.checklistId > 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateChecklistDetailEntry(model.checkListDefinitionId, model.targetId))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                new { success = false, message = "This checklist item is checked already" });
                    }
                }
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.AddChecklistDetail(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("checklist-detail-multiple")]
        public HttpResponseMessage AddChecklistDetailMultiple([FromBody] List<ChecklistDetailViewModel> model)
        {
            try
            {
                if (model.Count <= 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "Please select a checklist to continue" });
                }
                var data = repo.AddMultipleChecklistDetails(model, token.GetStaffId, (short)token.GetBranchId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = true, result = data, message = "The Checklist has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = "There was an error creating this Checklist" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-detail/")]
        public HttpResponseMessage GetAllChecklistDetailByProductId(int targetId)
        {
            try
            {
                // var targetTypeId = (int)CheckListTargetTypeEnum.Loan;
                var data = repo.GetAllMappedChecklistDefinitionByProductId(targetId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-detail/target-type/{targetTypeId}/product/{productId}")]
        public HttpResponseMessage GetAllChecklistDetailByProductAndTargetType(int targetTypeId, int productId)
        {
            try
            {
                var data = repo.GetAllChecklistDetailByProductAndTargetId(targetTypeId, productId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-detail/checklist-definition/{checklistDefinitionId}")]
        public HttpResponseMessage GetAllChecklistDetailByChecklistDefinition(int checklistDefinitionId)
        {
            try
            {
                var data = repo.GetAllChecklistDetailByChecklistDefinitionId(checklistDefinitionId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-detail/{ChecklistId}")]
        public HttpResponseMessage GetAllChecklistById(int ChecklistId)
        {
            try
            {
                var data = repo.GetAllChecklistDetailById(ChecklistId);
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = true, result = data, count = 1 });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
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
             new { success = true, result = ChecklistId, message = "Record has been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = ex.Message });
            }

        }
        #endregion

        #region CheckList Items
        [HttpPost]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }



        [HttpPost]
        [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = $"Error: {e.Message}" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-item/{ChecklistId}")]
        public HttpResponseMessage GetAllChecklistItemById(int CheckListItemId)
        {
            try
            {
                var data = repo.GetAllChecklistItemById(CheckListItemId);
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = true, result = data, count = 1 });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }

        }

        [HttpPut]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = $"There was an error updating this record {e.Message}" });
            }

        }

        [HttpDelete]
        [ClaimsAuthorization]
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = ex.Message });
            }


        }
        #endregion

        #region CheckList Select List
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-response-type")]
        public HttpResponseMessage GetAllChecklistResponseType()
        {
            try
            {
                var data = repo.GetAllChecklistResponseType();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion

        #region Loan Application CheckList
        [HttpGet]
        [ClaimsAuthorization]
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
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("validate-checklist-details")]
        public HttpResponseMessage ValidateChecklistDetail([FromBody] List<ValidateChecklistDetailViewModel> model)
        {
            try
            {
                var data = repo.ValidateChecklistDetail(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, message = "The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "There was an error updating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }
        [HttpPut]
        [ClaimsAuthorization]
        [Route("validate-condition-precedence")]
        public HttpResponseMessage ValidateConditionPrecedentDetail([FromBody] ConditionPrecedentViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = CommonHelpers.GetUserIP();
                if (repo.ValidateChecklistForDefferalOrWaival(model))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "The request for deferral/waival of this item is still being processed. " });
                }
                var data = repo.ValidateConditionPrecedentDetail(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, message = "The record has been Validated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "There was an error validating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = $"There was an error validating this record {e.Message}" });
            }

        }
        #endregion

        #region Condition Precedence Checklist
        [HttpGet]
        [ClaimsAuthorization]
        [Route("condition-prededence-checklist")]
        public HttpResponseMessage GetConditionPrecedenceChecklist(int loanApplicationId, bool isAvailment)
        {
            try
            {
                var data = repo.GetConditionPrecedenceChecklist(loanApplicationId, isAvailment);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("condition-prededence-checklist-status")]
        public HttpResponseMessage GetConditionPrecedenceChecklistStatus(int loanApplicationId, bool isAvailment)
        {
            try
            {
                var data = repo.GetConditionPrecedenceChecklistStatus(loanApplicationId, isAvailment, token.GetStaffId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("validate-precedence-checklist-completed")]
        public HttpResponseMessage ValidatePrecedenceChecklistCompleted(int loanApplicationId)
        {
            try
            {
                var data = repo.ValidatePrecedenceChecklistCompleted(loanApplicationId);
                if (data == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });     
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lms-validate-precedence-checklist-completed/{applicationId}")]
        public HttpResponseMessage LMSValidatePrecedenceChecklistCompleted(int applicationId)
        {
            try
            {
                var data = repo.LMSValidatePrecedenceChecklistCompleted(applicationId);
                if (data == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-deferred-documents-awaiting-approval")]
        public HttpResponseMessage GetDeferralDocumentsAwaitingApproval()
        {
            try
            {
                var data = repo.GetDeferralDocumentsAwaitingApproval(token.GetStaffId, token.GetCompanyId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-deferred-extensions-awaiting-approval")]
        public HttpResponseMessage GetDeferralExtensionsAwaitingApproval()
        {
            try
            {
                var data = repo.GetDeferralExtensionsAwaitingApproval(token.GetStaffId, token.GetCompanyId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("submit-deferred-document-for-approval")]
        public HttpResponseMessage SubmitDeferralDocumentForApproval([FromBody] ConditionPrecedentViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.SubmitDeferralDocumentForApproval(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = repo.ResponseMessage(response, "DEFERRED DOCUMENT PROVISION") });
                //return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = res });
            }
            catch (SecureException ex)
            { 
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error saving this record. + {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("submit-deferred-extension-for-approval")]
        public HttpResponseMessage SubmitDeferralExtensionForApproval([FromBody] ConditionPrecedentViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var res = repo.SubmitDeferralExtensionForApproval(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = res });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error saving this record. Error - {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("deferred-checklist-awaiting-approval")]
        public HttpResponseMessage GetChecklistAwaitingApproval()
        {
            try
            {
                var data = repo.GetChecklistAwaitingApproval(token.GetStaffId, token.GetCompanyId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("deferred-checklist")]
        public HttpResponseMessage GetAllDeferralChecklist()
        {
            try
            {
                var data = repo.GetAllDeferralChecklist();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("get-deferral-approval-trail/targetId/{targetId}/operationId/{operationId}")]
        public HttpResponseMessage GetDeferralApprovalTrail(int targetId, int operationId)
        {
            var data = repo.GetDeferralApprovalTrail(targetId, operationId);
            if (data.Any())
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "No record Found" });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("validate-checklist/{applicationId}")]
        public HttpResponseMessage ValidateChecklist(int applicationId)
        {
                var data = repo.ValidateChecklist(applicationId);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An error occurred" });
           
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("deferred-checklist-byContionId/")]
        public HttpResponseMessage GetAllDeferralChecklist(int conditionId)
        {
            try
            {
                var data = repo.GetDeferralChecklistByConditionId(conditionId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("update-loan-condition-precedence-status")]
        public HttpResponseMessage UpdateLoanConditionPrecedenceStatus([FromBody] ConditionPrecedentViewModel model)
        {
            if (model.conditionId == 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = "Please select a checklist to continue" });
            }
            if (model.deferedDate < DateTime.Now)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = "Deferred date cannot be less than today's date" });
            }
            model.userBranchId = (short)token.GetBranchId;
            model.companyId = token.GetCompanyId;
            model.createdBy = token.GetStaffId;
            model.applicationUrl = HttpContext.Current.Request.Path;
            model.userIPAddress = CommonHelpers.GetUserIP();

            var data = repo.UpdateLoanConditionPrecedenceStatus(model);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = true, message = "The Checklist Status has been updated successfully" });
            }

            return Request.CreateResponse(HttpStatusCode.OK,
        new { success = false, message = "There was an error updating this Checklist" });

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("submit-loan-condition-precedence-status")]
        public HttpResponseMessage ForwardChecklistForApproval([FromBody] List<ConditionPrecedentViewModel> models)
        {
            foreach (var model in models)
            {
                if (model.conditionId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "Please select a checklist to continue" });
                }
                if (model.deferedDate < DateTime.Now)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "Deferred date cannot be less than today's date" });
                }
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = CommonHelpers.GetUserIP();
            }
            

            var data = repo.ForwardChecklistForApproval(models);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = true, message = "The Checklist Status has been submitted successfully " + models[0].responseMessage });
            }

            return Request.CreateResponse(HttpStatusCode.OK,
        new { success = false, message = "There was an error submitting this Checklist" });

        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("extend-checklist-deferral-date")]
        public HttpResponseMessage ExtendChecklistDeferralDate([FromBody] ConditionPrecedentViewModel model)
        {
            try
            {
                if (model.conditionId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "Please select a checklist to continue" });
                }

                if (repo.ValidateDeferralDateExpiration((int)model.conditionId))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "Extention terminated, deferral not expired" });
                }

                model.userBranchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = CommonHelpers.GetUserIP();

                var data = repo.ExtendChecklistDeferralDate(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = true, message = "The Deferral Date has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = "There was an error extending checklist deferral" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("update-provided-checklist")]
        public HttpResponseMessage UpdateProvidedChecklist([FromBody] ConditionPrecedentViewModel model)
        {
            try
            {
                if (model.conditionId == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = false, message = "Please select a checklist to continue" });
                }
                model.userBranchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = CommonHelpers.GetUserIP();

                var data = repo.UpdateProvidedChecklist(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = true, message = "The Deferral Date has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = "There was an error extending checklist deferral" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("checklist-approval")]
        public HttpResponseMessage GoForApproval([FromBody]ApprovalViewModel entity)
        {
            try
            {
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;
                //entity.operationId = (int)OperationsEnum.DefferedChecklistApproval;
                var data = repo.GoForApproval(entity);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = data.responseMessage });

                //if (data == 1)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK,
                //        new { success = true, message = "Record has been approved successfully" });
                //}
                //else if (data == 2)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK,
                //        new { success = true, message = "Record has been disapproved successfully." });
                //}
                //else
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK,
                //    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
                //}
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error submitting this record {e.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("delete-loan-condition-checkstatus/{conditionId}/{isLMSChecklist}")]
        public HttpResponseMessage DeleteLoanConditionPrecedenceStatus(int conditionId, bool isLMSChecklist)
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

            var data =    repo.DeleteLoanConditionPrecedenceStatus(conditionId, isLMSChecklist, user);
                if(data == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                              new { success = true, result = conditionId, message = "Record has been deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false,  message = "Error deleting record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
             new { success = false, message = ex.Message });
            }

        }
        #endregion

        #region Checklist Type Mapping
        [HttpGet]
        [ClaimsAuthorization]
        [Route("mapped-checklist-type")]
        public HttpResponseMessage GetAllChecklistTypeMapping()
        {
            try
            {
                var data = repo.GetAllChecklistTypeMapping();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("checklist-type-mapping")]
        public HttpResponseMessage AddChecklistTypeMapping([FromBody] CheckListTypeMappingViewModel model)
        {
            try
            {
                string createUpdate = "";
                if (model.checklistTypeMappingId != 0 || model.checklistTypeMappingId < 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateChecklistTypeMapping(model.checklistTypeId, model.approvalLevelId))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                     new { success = false, message = "This Checklist Type is already mapped with the selected Approval Level" });
                    }
                }

                model.userBranchId = (short)token.GetBranchId;
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.userIPAddress = CommonHelpers.GetUserIP();

                var data = repo.AddChecklistTypeMapping(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = true, message = $"The record has been {createUpdate} successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = $"There was an error {createUpdate} this record." });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }
        #endregion

        #region EGS Checklist
        [HttpGet]
        [ClaimsAuthorization]
        [Route("esg-type")]
        public HttpResponseMessage GetESGType()
        {
            try
            {
                var data = repo.GetESGType();
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("esg-class")]
        public HttpResponseMessage GetESGClass()
        {
            try
            {
                var data = repo.GetESGClass();
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("esg-categories")]
        public HttpResponseMessage GetESGCategory()
        {
            try
            {
                var data = repo.GetESGCategory();
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("esg-sub-categories")]
        public HttpResponseMessage GetESGSubCategory(int categoryId)
        {
            try
            {
                var data = repo.GetESGSubCategory(categoryId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("esg-checklist-definition")]
        public HttpResponseMessage GetESGChecklistDefinition()
        {
            try
            {
                var data = repo.GetESGChecklistDefinition();
                if (data.Count() > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("esg-checklist-definition/delete/{esgChecklistDefinitionId}")]
        public HttpResponseMessage DeleteESGChecklistDefinition(int esgChecklistDefinitionId)
        {
            try
            {
                var data = repo.DeleteESGChecklistDefinition(esgChecklistDefinitionId, token.GetStaffId);
                if (data == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("esg-checklist-status")]
        public HttpResponseMessage GetESGChecklistDefinition(int loanApplicationId)
        {
            try
            {
                var data = repo.GetESGChecklistStatus(loanApplicationId);
                if (data.Count() > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            { 
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("esg-checklist-calculate")]
        public HttpResponseMessage CalculateESGChecklistSummary([FromBody] List<ESGChecklistDetailViewModel> model)
        {
            try
            {
                var data = repo.CalculateESGChecklistSummary(model);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error calculating the summary {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("green-rating-calculate")]
        public HttpResponseMessage CalculateGreenRatingSummary([FromBody] List<ESGChecklistDetailViewModel> model)
        {
            try
            {
                var data = repo.CalculateGreenRatingSummary(model);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error calculating the summary {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("esg-checklist-detail")]
        public HttpResponseMessage GetESGChecklistDetail(int loanApplicationId)
        {
            try
            {
                var data = repo.GetESGChecklistDetail(loanApplicationId);
                if (data.Count() > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }


        #region GreenRating
        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-scores/{checkListTypeId}")]
        public HttpResponseMessage GetCheckListScores(int checkListTypeId)
        {
            try
            {
                var data = repo.GetCheckListScores(checkListTypeId);
                if (data.Count() > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-item/{checkListTypeId}")]
        public HttpResponseMessage GetAllChecklistItemBycheckListTypeId(int checkListTypeId)
        {
            try
            {
                var data = repo.GetAllChecklistItemBycheckListTypeId(checkListTypeId);
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("green-rating-definition")]
        public HttpResponseMessage GetGreenRatingDefinition()
        {
            try
            {
                var data = repo.GetGreenRatingDefinition();
                if (data.Count() > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("green-rating-definition")]
        public HttpResponseMessage AddGreenRatingChecklistDefinition([FromBody] List<ESGChecklistDefinitionViewModel> model)
        {
            try
            {
                foreach (var item in model)
                {
                    item.userBranchId = (short)token.GetBranchId;
                    item.userIPAddress = CommonHelpers.GetUserIP();
                    item.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                    item.createdBy = token.GetStaffId;
                    item.companyId = token.GetCompanyId;
                }

                var data = repo.AddGreenRatingDefinition(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error creating record" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error creating these records {e.Message}" });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("green-rating-status")]
        public HttpResponseMessage GetGreenRatingDefinition(int loanApplicationId)
        {
            try
            {
                var data = repo.GetGreenRatingStatus(loanApplicationId);
                if (data.Count() > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("green-rating-detail")]
        public HttpResponseMessage GetGreenRatingDetail(int loanApplicationId)
        {
            try
            {
                var data = repo.GetGreenRatingDetail(loanApplicationId);
                if (data.Count() > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("green-rating-detail")]
        public HttpResponseMessage AddGreenRatingDetail([FromBody] List<ESGChecklistDetailViewModel> model)
        {
            try
            {
                foreach (var item in model)
                {
                    item.userBranchId = (short)token.GetBranchId;
                    item.userIPAddress = CommonHelpers.GetUserIP();
                    item.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                    item.createdBy = token.GetStaffId;
                    item.companyId = token.GetCompanyId;
                }

                var data = repo.AddGreenRatingDetail(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been saved successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error creating record" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error creating these records {e.Message}" });
            }

        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("green-rating-summary")]
        public HttpResponseMessage AddGreenRatingSummary([FromBody] ESGChecklistSummaryViewModel model)
        {
            try
            {

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.AddGreenRatingSummary(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been added successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error creating record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error creating these records {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("green-rating-definition/delete/{esgChecklistDefinitionId}")]
        public HttpResponseMessage DeleteGreenRatingDefinition(int esgChecklistDefinitionId)
        {
            try
            {
                var data = repo.DeleteGreenRatingDefinition(esgChecklistDefinitionId, token.GetStaffId);
                if (data == true)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }
        #endregion GreenRating

        //[HttpGet]
        //[ClaimsAuthorization]
        //[Route("esg-checklist-detail")]
        //public HttpResponseMessage GetESGChecklistSummary(int loanApplicationId)
        //{
        //    try
        //    {
        //        var data = repo.GetESGChecklistDetail(loanApplicationId);
        //        if (data.Count() > 0)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = true, result = data });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //          new { success = false, message = "No Record Found" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //      new { success = false, message = $"There was an error fetching this record {ex.Message}" });
        //    }
        //}

        [HttpGet]
        [ClaimsAuthorization]
        [Route("checklist-facility-detail")]
        public HttpResponseMessage GetAllFacilityDetails(int loanApplicationId)
        {
            try
            {
                var data = repo.GetAllFacilityDetails(loanApplicationId, token.GetCompanyId);
                if (data.Count() > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = "No Record Found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = $"There was an error fetching this record {ex.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("esg-category")]
        public HttpResponseMessage AddESGCategory([FromBody] ESGChecklistDefinitionViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.AddESGCategory(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error creating record" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error creating these records {e.Message}" });
            }

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("esg-subcategory")]
        public HttpResponseMessage AddESGSubCategory([FromBody] ESGChecklistDefinitionViewModel model)
        {
            try
            {
                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = CommonHelpers.GetUserIP();
                    model.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                var data = repo.AddESGSubCategory(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error creating record" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error creating these records {e.Message}" });
            }

        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("esg-category")]
        public HttpResponseMessage UpdateESGCategory([FromBody] ESGChecklistDefinitionViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.UpdateESGCategory(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error updating record" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error updating these records {e.Message}" });
            }

        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("esg-subcategory")]
        public HttpResponseMessage UpdateESGSubCategory([FromBody] ESGChecklistDefinitionViewModel model)
        {
            try
            {
                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = CommonHelpers.GetUserIP();
                    model.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                var data = repo.UpdateESGSubCategory(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been updating successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error updating record" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error updating these records {e.Message}" });
            }

        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("esg-category/{esgCategoryId}")]
        public HttpResponseMessage DeleteESGCategory(int esgCategoryId)
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

                var data = repo.DeleteESGCategory(esgCategoryId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error deleting record" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error deleting these records {e.Message}" });
            }

        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("esg-subcategory/{esgSubcategoryId}")]
        public HttpResponseMessage DeleteESGSubcategory(int esgSubcategoryId)
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

                var data = repo.DeleteESGSubcategory(esgSubcategoryId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been deleted successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error deleting record" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error deleting these records {e.Message}" });
            }

        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("esg-checklist-definition")]
        public HttpResponseMessage AddESGChecklistDefinition([FromBody] List<ESGChecklistDefinitionViewModel> model)
        {
            try
            {
                foreach (var item in model)
                {
                    item.userBranchId = (short)token.GetBranchId;
                    item.userIPAddress = CommonHelpers.GetUserIP();
                    item.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                    item.createdBy = token.GetStaffId;
                    item.companyId = token.GetCompanyId;
                }

                var data = repo.AddESGChecklistDefinition(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error creating record" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error creating these records {e.Message}" });
            }

        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("esg-checklist-detail")]
        public HttpResponseMessage AddESGChecklistDetail([FromBody] List<ESGChecklistDetailViewModel> model)
        {
            try
            {
                foreach (var item in model)
                {
                    item.userBranchId = (short)token.GetBranchId;
                    item.userIPAddress = CommonHelpers.GetUserIP();
                    item.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                    item.createdBy = token.GetStaffId;
                    item.companyId = token.GetCompanyId;
                }

                var data = repo.AddESGChecklistDetail(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been saved successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error creating record" });
            }

            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error creating these records {e.Message}" });
            }

        }
        [HttpPost]
        [ClaimsAuthorization]
        [Route("esg-checklist-summary")]
        public HttpResponseMessage AddESGChecklistSummary([FromBody] ESGChecklistSummaryViewModel model)
        {
            try
            {

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.UserHostAddress;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
             
                var data = repo.AddESGChecklistSummary(model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Record has been added successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = "Error creating record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error creating these records {e.Message}" });
            }
        }
        #endregion


        [HttpGet]
        [ClaimsAuthorization]
        [Route("regulatory-checklist-automapping")]
        public HttpResponseMessage RegulatoryChecklistAutomapping(int customerId, int targetId)
        {
            try
            {
                ChecklistDetailViewModel model = new ChecklistDetailViewModel();
                model.targetId = targetId;
                string createUpdate = "";
                if (model.checklistId != 0 || model.checklistId > 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateChecklistDetailEntry(model.checkListDefinitionId, model.targetId))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                new { success = false, message = "This checklist item is checked already" });
                    }
                }
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = CommonHelpers.GetUserIP();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var data = repo.RegulatoryChecklistAutomapping(customerId, model);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                 new { success = true, result = data, message = $"The record has been {createUpdate} successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
            new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        #region LMS Condition Precedence Checklist
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lms-condition-prededence-checklist")]
        public HttpResponseMessage GetLMSConditionPrecedenceChecklist(int loanReviewApplicationId)
        {
            try
            {
                var data = repo.GetLMSConditionPrecedenceChecklist(loanReviewApplicationId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpGet]
        [ClaimsAuthorization]
        [Route("lms-condition-prededence-checklist-status")]
        public HttpResponseMessage GetLMSConditionPrecedenceChecklistStatus(int loanReviewApplicationId)
        {
            try
            {
                var data = repo.GetLMSConditionPrecedenceChecklistStatus(loanReviewApplicationId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("populate-loanapplication-checklist")]
        public HttpResponseMessage PopulateLoanApplicationChecklist(CheckListTargetTypeViewModel model)
        {
            //try
            //{   
                var data = repo.PopulateLoanApplicationChecklist(model.loanApplicationId,token.GetStaffId,token.GetCompanyId,model.productClassProcessId);

                if (data == false)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error populating checklist" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            //}
            //catch (SecureException e)
            //{
            //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            //}
        }
        #endregion
        
    }
}