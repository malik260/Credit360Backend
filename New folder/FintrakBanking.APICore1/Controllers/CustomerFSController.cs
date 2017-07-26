using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FintrakBanking.Interfaces.Customer;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/customers")]
    public class CustomerFSController : BaseController
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
        [HttpPost("customer-fs-caption-group")]
        public IActionResult AddCustomerFSCaptionGroup([FromBody] CustomerFSCaptionGroupViewModel entity)
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;

                var response = fsGroupRepo.AddCustomerFSCaptionGroup(entity);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }        

        [HttpGet("customer-fs-caption-group")]
        public IActionResult GetCustomerFSCaptionGroup()
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = fsGroupRepo.GetCustomerFSCaptionGroup(token.GetCompanyId);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("customer-fs-caption-group/{fsCaptionGroupId}")]
        public IActionResult GetCustomerFSCaptionGroupById(short fsCaptionGroupId)
        {
            try
            {
                var response = fsGroupRepo.GetCustomerFSCaptionGroupById(fsCaptionGroupId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }


        [HttpPut("customer-fs-caption-group/{fsCaptionGroupId}")]
        public IActionResult UpdateCustomerFSCaptionGroup(short fsCaptionGroupId, [FromBody] CustomerFSCaptionGroupViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;

                var response = fsGroupRepo.UpdateCustomerFSCaptionGroup(fsCaptionGroupId, entity);

                if (response)
                {

                    return Created("", new { success = true, result = response, message = "The record has been updated successfully" });

                }
                return Ok(new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }
        #endregion

        #region Customer FS Caption
        [HttpPost("customer-fs-caption")]
        public IActionResult AddCustomerFSCaption([FromBody] CustomerFSCaptionViewModel entity)
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;

                var response = fsCaptionRepo.AddCustomerFSCaption(entity);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet("customer-fs-caption/group/{fsCaptionGroupId}")]
        public IActionResult GetCustomerFSCaption(short fsCaptionGroupId)
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = fsCaptionRepo.GetCustomerFSCaption(fsCaptionGroupId);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("customer-fs-caption/{fsCaptionId}")]
        public IActionResult GetCustomerFSCaptionById(short fsCaptionId)
        {
            try
            {
                var response = fsCaptionRepo.GetCustomerFSCaptionById(fsCaptionId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }


        //[HttpGet("customer-fs-caption/unmapped/{fsCaptionGroupId}/customer/{customerId}/date/{fsDate}")]
        [HttpGet("customer-fs-caption/unmapped")]
        public IActionResult GetUnmappedCustomerFSCaption(short fsCaptionGroupId, int customerId, DateTime fsDate)
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = fsCaptionRepo.GetUnmappedCustomerFSCaption(fsCaptionGroupId, customerId, fsDate);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPut("customer-fs-caption/{fsCaptionId}")]
        public IActionResult UpdateCustomerFSCaption(int fsCaptionId, [FromBody] CustomerFSCaptionViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;

                var response = fsCaptionRepo.UpdateCustomerFSCaption(fsCaptionId, entity);

                if (response)
                {
                    return Created("", new { success = true, result = response, message = "The record has been updated successfully" });
                }
                return Ok(new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }
        #endregion

        #region Customer FS Caption Detail
        [HttpPost("customer-fs-caption-detail")]
        public IActionResult AddCustomerFSCaptionDetail([FromBody] CustomerFSCaptionDetailViewModel entity)
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;

                var response = fsDetailRepo.AddCustomerFSCaptionDetail(entity);
                if (response)
                {
                    return Ok( new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost("customer-fs-caption-detail/multiple")]
        public IActionResult AddMultipleCustomerFSCaptionDetail([FromBody] List<CustomerFSCaptionDetailViewModel> entities)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var userBranch = (short)token.GetBranchId;
                var userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                var applicationUrl = Request.Path.Value;
                var createdBy = token.GetStaffId;

                foreach (CustomerFSCaptionDetailViewModel entity in entities)
                {
                    entity.userBranchId = userBranch;
                    entity.userIPAddress = userIPAddress;
                    entity.applicationUrl = applicationUrl;
                    entity.createdBy = createdBy;
                }

                var response = fsDetailRepo.AddMultipleCustomerFSCaptionDetail(entities);
                if (response)
                {
                    return Ok(new { success = true, result = response, message = "The record(s) has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating these record(s)" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating these record(s) {e.Message}" });
            }
        }

        [HttpGet("customer-fs-caption-detail/customer/{customerId}")]
        public IActionResult GetCustomerFSCaptionDetail(int customerId)
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = fsDetailRepo.GetCustomerFSCaptionDetail(customerId);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("customer-fs-caption-detail/{fsDetailId}")]
        public IActionResult GetCustomerFSCaptionById(int fsDetailId)
        {
            try
            {
                var response = fsDetailRepo.GetCustomerFSCaptionDetailById(fsDetailId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }


        [HttpPut("customer-fs-caption-detail/{fsDetailId}")]
        public IActionResult UpdateCustomerFSCaptionDetail(int fsDetailId, [FromBody] CustomerFSCaptionDetailViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;

                var response = fsDetailRepo.UpdateCustomerFSCaptionDetail(fsDetailId, entity);

                if (response)
                {
                    return Ok( new { success = true, result = response, message = "The record has been updated successfully" });
                }
                return Ok(new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete("customer-fs-caption-detail/{fsdetailId}")]
        public IActionResult DeleteCustomerFSCaptionDetail(int fsdetailId)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                fsDetailRepo.DeleteCustomerFSCaptionDetail(fsdetailId, user);

                return Ok(new { success = true, result = fsdetailId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("customer-fs-caption-detail/multiple/{fsdetailIds}")]
        public IActionResult DeleteMultileCustomerFSCaptionDetail(List<int> fsdetailIds)
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                fsDetailRepo.DeleteMultileCustomerFSCaptionDetail(fsdetailIds, user);

                return Ok(new { success = true, result = 1, message = "record(s) has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Customer FS Ratio Caption
        [HttpPost("customer-fs-ratio-caption")]
        public IActionResult AddFSRatioCaption([FromBody] CustomerFSRatioCaptionViewModel model)
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = fsRepo.AddFSRatioCaption(model);
                if (response)
                {
                    return Created("", new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet("customer-fs-ratio-caption")]
        public IActionResult GetFSRatioCaption()
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = fsRepo.GetFSRatioCaption(token.GetCompanyId);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("customer-fs-ratio-caption/{RatioCaptionId}")]
        public IActionResult GetFSRatioCaptionById(short ratioCaptionId)
        {
            try
            {
                var response = fsRepo.GetFSRatioCaptionById(ratioCaptionId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut("customer-fs-ratio-caption/{RatioCaptionId}")]
        public IActionResult UpdateFSRatioCaption(short ratioCaptionId, [FromBody] CustomerFSRatioCaptionViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = fsRepo.UpdateFSRatioCaption(ratioCaptionId, model);

                if (response)
                {

                    return Created("", new { success = true, result = response, message = "The record has been updated successfully" });

                }
                return Ok(new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete("customer-fs-ratio-caption/{RatioCaptionId}")]
        public IActionResult DeleteFSRatioCaption(short ratioCaptionId)
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                fsRepo.DeleteFSRatioCaption(ratioCaptionId, user);

                return Ok(new { success = true, result = ratioCaptionId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region FS Ratio Detail
        [HttpPost("customer-fs-ratio-detail")]
        public IActionResult AddFSRatioDetail([FromBody] CustomerFSRatioDetailViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = fsRepo.AddFSRatioDetail(model);
                if (response)
                {
                    return Ok(new { success = true, result = response, message = "The record has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPost("customer-fs-ratio-detail/multiple")]
        public IActionResult AddMultipleFSRatioDetail([FromBody] List<CustomerFSRatioDetailViewModel> models)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var userBranch = (short)token.GetBranchId;
                var userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                var applicationUrl = Request.Path.Value;
                var createdBy = token.GetStaffId;

                foreach (CustomerFSRatioDetailViewModel model in models)
                {
                    model.userBranchId = userBranch;
                    model.userIPAddress = userIPAddress;
                    model.applicationUrl = applicationUrl;
                    model.createdBy = createdBy;
                    model.companyId = token.GetCompanyId;
                }

                var response = fsRepo.AddMultipleFSRatioDetail(models);
                if (response)
                {
                    return Ok(new { success = true, result = response, message = "The record(s) has been created successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating these record(s)" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating these record(s) {e.Message}" });
            }
        }

        [HttpGet("customer-fs-ratio-detail/ratio-caption/{ratioCaptionId}/caption-group/{fsCaptionGroupId}")]
        public IActionResult GetFSRatioDetail(short ratioCaptionId, short fsCaptionGroupId)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = fsRepo.GetFSRatioDetail(ratioCaptionId, fsCaptionGroupId, token.GetCompanyId);
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("customer-fs-ratio-detail/{ratioDetailId}")]
        public IActionResult GetFSRatioDetailById(int ratioDetailId)
        {
            try
            {
                var response = fsRepo.GetFSRatioDetailById(ratioDetailId);
                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (System.Exception ex)
            {

                return Ok(new { success = false, message = $"There was an error updating this record {ex.Message}" });
            }
        }

        [HttpPut("customer-fs-ratio-detail/{ratioDetailId}")]
        public IActionResult UpdateFSRatioDetail(int ratioDetailId, [FromBody] CustomerFSRatioDetailViewModel model)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = fsRepo.UpdateFSRatioDetail(ratioDetailId, model);

                if (response)
                {
                    return Ok(new { success = true, result = response, message = "The record has been updated successfully" });
                }
                return Ok(new { success = false, message = "There was an error updating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error updating this record {e.Message}" });
            }
        }

        [HttpDelete("customer-fs-ratio-detail/{ratioDetailId}")]
        public IActionResult DeleteFSRatioDetail(int ratioDetailId)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                fsRepo.DeleteFSRatioDetail(ratioDetailId, user);

                return Ok(new { success = true, result = ratioDetailId, message = "record has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("customer-fs-ratio-detail/multiple/{ratioDetailId}")]
        public IActionResult DeleteMultileFSRatioDetail(List<int> ratioDetailIds)
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                fsRepo.DeleteMultipleFSRatioDetail(ratioDetailIds, user);

                return Ok(new { success = true, result = 1, message = "record(s) has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("customer-fs-ratio-detail/divisor-type")]
        public IActionResult GetAllDivisorType()
        {

            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = fsRepo.GetAllDivisorType();
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("customer-fs-ratio-detail/value-type")]
        public IActionResult GetAllValueType()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = fsRepo.GetAllValueType();
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, count = response.Count() });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }
        #endregion
    }
}