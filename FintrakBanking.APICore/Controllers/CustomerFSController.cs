using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using System.Web;

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/customers")]
    public class CustomerFSController : ApiControllerBase
    {
        private ICustomerFSCaptionGroupRepository fsGroupRepo;
        private ICustomerFSCaptionRepository fsCaptionRepo;
        private ICustomerFSCaptionDetailRepository fsDetailRepo;
        private ICustomerFSRatioRepository fsRepo;

        public CustomerFSController(ICustomerFSCaptionGroupRepository _fsGroupRepo,
                                    ICustomerFSCaptionRepository _fsCaptionRepo,
                                    ICustomerFSCaptionDetailRepository _fsDetailRepo,
                                    ICustomerFSRatioRepository _fsRepo)
        {
            this.fsGroupRepo = _fsGroupRepo;
            this.fsCaptionRepo = _fsCaptionRepo;
            this.fsDetailRepo = _fsDetailRepo;
            this.fsRepo = _fsRepo;
        }

        #region Customer FS Caption Group
        [HttpPost]
        [Route("customer-fs-caption-group")]
        public HttpResponseMessage AddCustomerFSCaptionGroup(  [FromBody] CustomerFSCaptionGroupViewModel entity)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    entity.userBranchId = (short)token.GetBranchId;
                    entity.userIPAddress = Request.RequestUri.Host;
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    entity.createdBy = token.GetStaffId;
                    entity.companyId = token.GetCompanyId;

                    var data = fsGroupRepo.AddCustomerFSCaptionGroup(entity);
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
        [Route("customer-fs-caption-group")]
        public HttpResponseMessage GetCustomerFSCaptionGroup( )
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsGroupRepo.GetCustomerFSCaptionGroup(token.GetCompanyId);
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
        [Route("customer-fs-caption-group/{fsCaptionGroupId}")]
        public HttpResponseMessage GetCustomerFSCaptionGroupById(  short fsCaptionGroupId)
        { 
                try
                {
                    var data = fsGroupRepo.GetCustomerFSCaptionGroupById(fsCaptionGroupId);
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, count = 1 });
                }
                catch (System.Exception ex)
                {

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error updating this record {ex.Message}" });
                }
             
        }


        [HttpPut]
        [Route("customer-fs-caption-group/{fsCaptionGroupId}")]
        public HttpResponseMessage UpdateCustomerFSCaptionGroup(  short fsCaptionGroupId, [FromBody] CustomerFSCaptionGroupViewModel entity)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    entity.userBranchId = (short)token.GetBranchId;
                    entity.userIPAddress = Request.RequestUri.Host;
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    entity.createdBy = token.GetStaffId;
                    entity.companyId = token.GetCompanyId;

                    var data = fsGroupRepo.UpdateCustomerFSCaptionGroup(fsCaptionGroupId, entity);

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
        #endregion

        #region Customer FS Caption
        [HttpPost]
        [Route("customer-fs-caption")]
        public HttpResponseMessage AddCustomerFSCaption(  [FromBody] CustomerFSCaptionViewModel entity)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    entity.userBranchId = (short)token.GetBranchId;
                    entity.userIPAddress = Request.RequestUri.Host;
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    entity.createdBy = token.GetStaffId;
                    entity.companyId = token.GetCompanyId;

                    var data = fsCaptionRepo.AddCustomerFSCaption(entity);
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
        [Route("customer-fs-caption/group/{fsCaptionGroupId}")]
        public HttpResponseMessage GetCustomerFSCaption(  short fsCaptionGroupId)
        {  try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsCaptionRepo.GetCustomerFSCaption(fsCaptionGroupId);
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
        [Route("customer-fs-caption/{fsCaptionId}")]
        public HttpResponseMessage GetCustomerFSCaptionById(  short fsCaptionId)
        {  try
                {
                    var data = fsCaptionRepo.GetCustomerFSCaptionById(fsCaptionId);
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, count = 1 });
                }
                catch (System.Exception ex)
                {

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error updating this record {ex.Message}" });
                }
                 
        }


        //[HttpGet("customer-fs-caption/unmapped/{fsCaptionGroupId}/customer/{customerId}/date/{fsDate}")]
        [HttpGet]
        [Route("customer-fs-caption/unmapped")]
        public HttpResponseMessage GetUnmappedCustomerFSCaption(  short fsCaptionGroupId, int customerId, DateTime fsDate)
        {   try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsCaptionRepo.GetUnmappedCustomerFSCaption(fsCaptionGroupId, customerId, fsDate);
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

        [HttpPut]
        [Route("customer-fs-caption/{fsCaptionId}")]
        public HttpResponseMessage UpdateCustomerFSCaption(  int fsCaptionId, [FromBody] CustomerFSCaptionViewModel entity)
        {  try
                {
                    var token = new TokenDecryptionHelper();

                    entity.userBranchId = (short)token.GetBranchId;
                    entity.userIPAddress = Request.RequestUri.Host;
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    entity.createdBy = token.GetStaffId;
                    entity.companyId = token.GetCompanyId;

                    var data = fsCaptionRepo.UpdateCustomerFSCaption(fsCaptionId, entity);

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
        #endregion

        #region Customer FS Caption Detail
        [HttpPost]
        [Route("customer-fs-caption-detail")]
        public HttpResponseMessage AddCustomerFSCaptionDetail(  [FromBody] CustomerFSCaptionDetailViewModel entity)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    entity.userBranchId = (short)token.GetBranchId;
                    entity.userIPAddress = Request.RequestUri.Host;
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    entity.createdBy = token.GetStaffId;
                    entity.companyId = token.GetCompanyId;

                    var data = fsDetailRepo.AddCustomerFSCaptionDetail(entity);
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
        [Route("customer-fs-caption-detail/multiple")]
        public HttpResponseMessage AddMultipleCustomerFSCaptionDetail(  [FromBody] List<CustomerFSCaptionDetailViewModel> entities)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    var userBranch = (short)token.GetBranchId;
                    var userIPAddress = Request.RequestUri.Host;
                    var applicationUrl = HttpContext.Current.Request.Path;
                    var createdBy = token.GetStaffId;

                    foreach (CustomerFSCaptionDetailViewModel entity in entities)
                    {
                        entity.userBranchId = userBranch;
                        entity.userIPAddress = userIPAddress;
                        entity.applicationUrl = applicationUrl;
                        entity.createdBy = createdBy;
                    }

                    var data = fsDetailRepo.AddMultipleCustomerFSCaptionDetail(entities);
                    if (data)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                            new { success = true, result = data, message = "The record(s) has been created successfully" });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "There was an error creating these record(s)" });
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error creating these record(s) {e.Message}" });
                } 
        }

        [HttpGet]
        [Route("customer-fs-caption-detail/customer/{customerId}")]
        public HttpResponseMessage GetCustomerFSCaptionDetail(  int customerId)
        { try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsDetailRepo.GetCustomerFSCaptionDetail(customerId);
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
        [Route("customer-fs-caption-detail/{fsDetailId}")]
        public HttpResponseMessage GetCustomerFSCaptionById(  int fsDetailId)
        { 
                try
                {
                    var data = fsDetailRepo.GetCustomerFSCaptionDetailById(fsDetailId);
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, count = 1 });
                }
                catch (System.Exception ex)
                {

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error updating this record {ex.Message}" });
                } 
        }


        [HttpPut]
        [Route("customer-fs-caption-detail/{fsDetailId}")]
        public HttpResponseMessage UpdateCustomerFSCaptionDetail(  int fsDetailId, [FromBody] CustomerFSCaptionDetailViewModel entity)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    entity.userBranchId = (short)token.GetBranchId;
                    entity.userIPAddress = Request.RequestUri.Host;
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    entity.createdBy = token.GetStaffId;
                    entity.companyId = token.GetCompanyId;

                    var data = fsDetailRepo.UpdateCustomerFSCaptionDetail(fsDetailId, entity);

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
        [Route("customer-fs-caption-detail/{fsdetailId}")]
        public HttpResponseMessage DeleteCustomerFSCaptionDetail(  int fsdetailId)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        applicationUrl = HttpContext.Current.Request.Path,
                        userIPAddress = Request.RequestUri.Host
                    };

                    fsDetailRepo.DeleteCustomerFSCaptionDetail(fsdetailId, user);

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = fsdetailId, message = "record has been deleted successfully" });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                } 
        }

        [HttpDelete]
        [Route("customer-fs-caption-detail/multiple/{fsdetailIds}")]
        public HttpResponseMessage DeleteMultileCustomerFSCaptionDetail(  List<int> fsdetailIds)
        { try
                {
                    var token = new TokenDecryptionHelper();

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        applicationUrl = HttpContext.Current.Request.Path,
                        userIPAddress = Request.RequestUri.Host
                    };

                    fsDetailRepo.DeleteMultileCustomerFSCaptionDetail(fsdetailIds, user);

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = 1, message = "record(s) has been deleted successfully" });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                } 
        }

        #endregion

        #region Customer FS Ratio Caption
        [HttpPost]
        [Route("customer-fs-ratio-caption")]
        public HttpResponseMessage AddFSRatioCaption(  [FromBody] CustomerFSRatioCaptionViewModel model)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = fsRepo.AddFSRatioCaption(model);
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
        [Route("customer-fs-ratio-caption")]
        public HttpResponseMessage GetFSRatioCaption( )
        {  try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsRepo.GetFSRatioCaption(token.GetCompanyId);
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
        [Route("customer-fs-ratio-caption/{RatioCaptionId}")]
        public HttpResponseMessage GetFSRatioCaptionById(  short ratioCaptionId)
        { 
                try
                {
                    var data = fsRepo.GetFSRatioCaptionById(ratioCaptionId);
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
                }
                catch (System.Exception ex)
                {

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error updating this record {ex.Message}" });
                } 
        }

        [HttpPut]
        [Route("customer-fs-ratio-caption/{RatioCaptionId}")]
        public HttpResponseMessage UpdateFSRatioCaption(  short ratioCaptionId, [FromBody] CustomerFSRatioCaptionViewModel model)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = fsRepo.UpdateFSRatioCaption(ratioCaptionId, model);

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
        [Route("customer-fs-ratio-caption/{RatioCaptionId}")]
        public HttpResponseMessage DeleteFSRatioCaption(  short ratioCaptionId)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        applicationUrl = HttpContext.Current.Request.Path,
                        userIPAddress = Request.RequestUri.Host
                    };

                    fsRepo.DeleteFSRatioCaption(ratioCaptionId, user);

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = ratioCaptionId, message = "record has been deleted successfully" });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = ex.Message });
                } 
        }

        #endregion

        #region FS Ratio Detail
        [HttpPost]
        [Route("customer-fs-ratio-detail")]
        public HttpResponseMessage AddFSRatioDetail(  [FromBody] CustomerFSRatioDetailViewModel model)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = fsRepo.AddFSRatioDetail(model);
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
        [Route("customer-fs-ratio-detail/multiple")]
        public HttpResponseMessage AddMultipleFSRatioDetail(  [FromBody] List<CustomerFSRatioDetailViewModel> models)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    var userBranch = (short)token.GetBranchId;
                    var userIPAddress = Request.RequestUri.Host;
                    var applicationUrl = HttpContext.Current.Request.Path;
                    var createdBy = token.GetStaffId;

                    foreach (CustomerFSRatioDetailViewModel model in models)
                    {
                        model.userBranchId = userBranch;
                        model.userIPAddress = userIPAddress;
                        model.applicationUrl = applicationUrl;
                        model.createdBy = createdBy;
                        model.companyId = token.GetCompanyId;
                    }

                    var data = fsRepo.AddMultipleFSRatioDetail(models);
                    if (data)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                            new { success = true, result = data, message = "The record(s) has been created successfully" });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "There was an error creating these record(s)" });
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error creating these record(s) {e.Message}" });
                } 
        }

        [HttpGet]
        [Route("customer-fs-ratio-detail/ratio-caption/{ratioCaptionId}/caption-group/{fsCaptionGroupId}")]
        public HttpResponseMessage GetFSRatioDetail(  short ratioCaptionId, short fsCaptionGroupId)
        { 
                try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsRepo.GetFSRatioDetail(ratioCaptionId, fsCaptionGroupId, token.GetCompanyId);
                    if (!data.Any())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data , count = data.Count() });
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
                }
             
        }

        [HttpGet]
        [Route("customer-fs-ratio-detail/{ratioDetailId}")]
        public HttpResponseMessage GetFSRatioDetailById(  int ratioDetailId)
        { 
                try
                {
                    var data = fsRepo.GetFSRatioDetailById(ratioDetailId);
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
                }
                catch (System.Exception ex)
                {

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"There was an error updating this record {ex.Message}" });
                } 
        }

        [HttpPut]
        [Route("customer-fs-ratio-detail/{ratioDetailId}")]
        public HttpResponseMessage UpdateFSRatioDetail(  int ratioDetailId, [FromBody] CustomerFSRatioDetailViewModel model)
        {  try
                {
                    var token = new TokenDecryptionHelper();

                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;
                    model.companyId = token.GetCompanyId;

                    var data = fsRepo.UpdateFSRatioDetail(ratioDetailId, model);

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
        [Route("customer-fs-ratio-detail/{ratioDetailId}")]
        public HttpResponseMessage DeleteFSRatioDetail(  int ratioDetailId)
        {   try
                {
                    var token = new TokenDecryptionHelper();

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        applicationUrl = HttpContext.Current.Request.Path,
                        userIPAddress = Request.RequestUri.Host
                    };

                    fsRepo.DeleteFSRatioDetail(ratioDetailId, user);

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = ratioDetailId, message = "record has been deleted successfully" });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = ex.Message });
                } 
        }

        [HttpDelete]
        [Route("customer-fs-ratio-detail/multiple/{ratioDetailId}")]
        public HttpResponseMessage DeleteMultileFSRatioDetail(  List<int> ratioDetailIds)
        { try
                {
                    var token = new TokenDecryptionHelper();

                    UserInfo user = new UserInfo()
                    {
                        BranchId = token.GetBranchId,
                        companyId = token.GetCompanyId,
                        staffId = token.GetStaffId,
                        applicationUrl = HttpContext.Current.Request.Path,
                        userIPAddress = Request.RequestUri.Host
                    };

                    fsRepo.DeleteMultipleFSRatioDetail(ratioDetailIds, user);

                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = 1, message = "record(s) has been deleted successfully" });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = ex.Message });
                } 
        }

        [HttpGet]
        [Route("customer-fs-ratio-detail/divisor-type")]
        public HttpResponseMessage GetAllDivisorType( )
        {   try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsRepo.GetAllDivisorType();
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
        [Route("customer-fs-ratio-detail/value-type")]
        public HttpResponseMessage GetAllValueType( )
        {  try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsRepo.GetAllValueType();
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
    }
}