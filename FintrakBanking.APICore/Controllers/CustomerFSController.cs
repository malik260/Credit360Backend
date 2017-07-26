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
        public HttpResponseMessage AddCustomerFSCaptionGroup(HttpRequestMessage request, [FromBody] CustomerFSCaptionGroupViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record has been created successfully" }));
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

        [HttpGet]
        [Route("customer-fs-caption-group")]
        public HttpResponseMessage GetCustomerFSCaptionGroup(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsGroupRepo.GetCustomerFSCaptionGroup(token.GetCompanyId);
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

        [HttpGet]
        [Route("customer-fs-caption-group/{fsCaptionGroupId}")]
        public HttpResponseMessage GetCustomerFSCaptionGroupById(HttpRequestMessage request, short fsCaptionGroupId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = fsGroupRepo.GetCustomerFSCaptionGroupById(fsCaptionGroupId);
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
        [Route("customer-fs-caption-group/{fsCaptionGroupId}")]
        public HttpResponseMessage UpdateCustomerFSCaptionGroup(HttpRequestMessage request, short fsCaptionGroupId, [FromBody] CustomerFSCaptionGroupViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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

                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record has been updated successfully" }));

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
        #endregion

        #region Customer FS Caption
        [HttpPost]
        [Route("customer-fs-caption")]
        public HttpResponseMessage AddCustomerFSCaption(HttpRequestMessage request, [FromBody] CustomerFSCaptionViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record has been created successfully" }));
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

        [HttpGet]
        [Route("customer-fs-caption/group/{fsCaptionGroupId}")]
        public HttpResponseMessage GetCustomerFSCaption(HttpRequestMessage request, short fsCaptionGroupId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsCaptionRepo.GetCustomerFSCaption(fsCaptionGroupId);
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

        [HttpGet]
        [Route("customer-fs-caption/{fsCaptionId}")]
        public HttpResponseMessage GetCustomerFSCaptionById(HttpRequestMessage request, short fsCaptionId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = fsCaptionRepo.GetCustomerFSCaptionById(fsCaptionId);
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


        //[HttpGet("customer-fs-caption/unmapped/{fsCaptionGroupId}/customer/{customerId}/date/{fsDate}")]
        [HttpGet]
        [Route("customer-fs-caption/unmapped")]
        public HttpResponseMessage GetUnmappedCustomerFSCaption(HttpRequestMessage request, short fsCaptionGroupId, int customerId, DateTime fsDate)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsCaptionRepo.GetUnmappedCustomerFSCaption(fsCaptionGroupId, customerId, fsDate);
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

        [HttpPut]
        [Route("customer-fs-caption/{fsCaptionId}")]
        public HttpResponseMessage UpdateCustomerFSCaption(HttpRequestMessage request, int fsCaptionId, [FromBody] CustomerFSCaptionViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record has been updated successfully" }));
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
        #endregion

        #region Customer FS Caption Detail
        [HttpPost]
        [Route("customer-fs-caption-detail")]
        public HttpResponseMessage AddCustomerFSCaptionDetail(HttpRequestMessage request, [FromBody] CustomerFSCaptionDetailViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record has been created successfully" }));
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

        [HttpPost]
        [Route("customer-fs-caption-detail/multiple")]
        public HttpResponseMessage AddMultipleCustomerFSCaptionDetail(HttpRequestMessage request, [FromBody] List<CustomerFSCaptionDetailViewModel> entities)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record(s) has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "There was an error creating these record(s)" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"There was an error creating these record(s) {e.Message}" }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("customer-fs-caption-detail/customer/{customerId}")]
        public HttpResponseMessage GetCustomerFSCaptionDetail(HttpRequestMessage request, int customerId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsDetailRepo.GetCustomerFSCaptionDetail(customerId);
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

        [HttpGet]
        [Route("customer-fs-caption-detail/{fsDetailId}")]
        public HttpResponseMessage GetCustomerFSCaptionById(HttpRequestMessage request, int fsDetailId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = fsDetailRepo.GetCustomerFSCaptionDetailById(fsDetailId);
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
        [Route("customer-fs-caption-detail/{fsDetailId}")]
        public HttpResponseMessage UpdateCustomerFSCaptionDetail(HttpRequestMessage request, int fsDetailId, [FromBody] CustomerFSCaptionDetailViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record has been updated successfully" }));
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

        [HttpDelete]
        [Route("customer-fs-caption-detail/{fsdetailId}")]
        public HttpResponseMessage DeleteCustomerFSCaptionDetail(HttpRequestMessage request, int fsdetailId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = fsdetailId, message = "record has been deleted successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpDelete]
        [Route("customer-fs-caption-detail/multiple/{fsdetailIds}")]
        public HttpResponseMessage DeleteMultileCustomerFSCaptionDetail(HttpRequestMessage request, List<int> fsdetailIds)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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

                    fsDetailRepo.DeleteMultileCustomerFSCaptionDetail(fsdetailIds, user);

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = 1, message = "record(s) has been deleted successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        #endregion

        #region Customer FS Ratio Caption
        [HttpPost]
        [Route("customer-fs-ratio-caption")]
        public HttpResponseMessage AddFSRatioCaption(HttpRequestMessage request, [FromBody] CustomerFSRatioCaptionViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record has been created successfully" }));
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

        [HttpGet]
        [Route("customer-fs-ratio-caption")]
        public HttpResponseMessage GetFSRatioCaption(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsRepo.GetFSRatioCaption(token.GetCompanyId);
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

        [HttpGet]
        [Route("customer-fs-ratio-caption/{RatioCaptionId}")]
        public HttpResponseMessage GetFSRatioCaptionById(HttpRequestMessage request, short ratioCaptionId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = fsRepo.GetFSRatioCaptionById(ratioCaptionId);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data, count = 1 }));
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
        [Route("customer-fs-ratio-caption/{RatioCaptionId}")]
        public HttpResponseMessage UpdateFSRatioCaption(HttpRequestMessage request, short ratioCaptionId, [FromBody] CustomerFSRatioCaptionViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record has been updated successfully" }));

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

        [HttpDelete]
        [Route("customer-fs-ratio-caption/{RatioCaptionId}")]
        public HttpResponseMessage DeleteFSRatioCaption(HttpRequestMessage request, short ratioCaptionId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = ratioCaptionId, message = "record has been deleted successfully" }));
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

        #region FS Ratio Detail
        [HttpPost]
        [Route("customer-fs-ratio-detail")]
        public HttpResponseMessage AddFSRatioDetail(HttpRequestMessage request, [FromBody] CustomerFSRatioDetailViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record has been created successfully" }));
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

        [HttpPost]
        [Route("customer-fs-ratio-detail/multiple")]
        public HttpResponseMessage AddMultipleFSRatioDetail(HttpRequestMessage request, [FromBody] List<CustomerFSRatioDetailViewModel> models)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record(s) has been created successfully" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "There was an error creating these record(s)" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"There was an error creating these record(s) {e.Message}" }));
                }

                return response;

            });
        }

        [HttpGet]
        [Route("customer-fs-ratio-detail/ratio-caption/{ratioCaptionId}/caption-group/{fsCaptionGroupId}")]
        public HttpResponseMessage GetFSRatioDetail(HttpRequestMessage request, short ratioCaptionId, short fsCaptionGroupId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsRepo.GetFSRatioDetail(ratioCaptionId, fsCaptionGroupId, token.GetCompanyId);
                    if (!data.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = response, count = data.Count() }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = $"Error: {e.Message}" }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("customer-fs-ratio-detail/{ratioDetailId}")]
        public HttpResponseMessage GetFSRatioDetailById(HttpRequestMessage request, int ratioDetailId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = fsRepo.GetFSRatioDetailById(ratioDetailId);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data, count = 1 }));
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
        [Route("customer-fs-ratio-detail/{ratioDetailId}")]
        public HttpResponseMessage UpdateFSRatioDetail(HttpRequestMessage request, int ratioDetailId, [FromBody] CustomerFSRatioDetailViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = data, message = "The record has been updated successfully" }));
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

        [HttpDelete]
        [Route("customer-fs-ratio-detail/{ratioDetailId}")]
        public HttpResponseMessage DeleteFSRatioDetail(HttpRequestMessage request, int ratioDetailId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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

                    fsRepo.DeleteFSRatioDetail(ratioDetailId, user);

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = ratioDetailId, message = "record has been deleted successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }

                return response;

            });
        }

        [HttpDelete]
        [Route("customer-fs-ratio-detail/multiple/{ratioDetailId}")]
        public HttpResponseMessage DeleteMultileFSRatioDetail(HttpRequestMessage request, List<int> ratioDetailIds)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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

                    fsRepo.DeleteMultipleFSRatioDetail(ratioDetailIds, user);

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = 1, message = "record(s) has been deleted successfully" }));
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
        [Route("customer-fs-ratio-detail/divisor-type")]
        public HttpResponseMessage GetAllDivisorType(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsRepo.GetAllDivisorType();
                    if (!data.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = response, count = data.Count() }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("customer-fs-ratio-detail/value-type")]
        public HttpResponseMessage GetAllValueType(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();

                    var data = fsRepo.GetAllValueType();
                    if (!data.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = response, count = data.Count() }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
                }

                return response;
            });
        }
        #endregion
    }
}