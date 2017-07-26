using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Business;
using FintrakBanking.ViewModels.Setups.General;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/setup")]
    public class StaffController : BaseController
    {
        TokenDecryptionHelper token = null;
        private IStaffRepository repo;
        IErrorLogRepository errorLogger;
        public StaffController(IStaffRepository _repo,
                                IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            this.errorLogger = _errorLogger;
        }

        [HttpGet("staff")]
        public IActionResult GetStaffInfo()
        {
            try
            {
                
                this.token = new TokenDecryptionHelper(this.HttpContext);
                var staffinfo = repo.GetAllStaff().Where(x=>x.companyId==token.GetCompanyId).ToList();

                if (staffinfo == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("staff/approvals/temp")]
        public IActionResult GetStaffAwaitingApproval()
        {
            try
            {
                this.token = new TokenDecryptionHelper(this.HttpContext);
                var staffinfo = repo.GetStaffAwaitingApprovals(token.GetStaffId, token.GetCompanyId);

                if (staffinfo == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("staff/approvals/temp/{staffId}")]
        public IActionResult GetTempStaffDetailsById(int staffId)
        {
            try
            {
                this.token = new TokenDecryptionHelper(this.HttpContext);
                var staffinfo = repo.GetTempStaffDetail(staffId);

                if (staffinfo == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("staff/approvals/{staffCode}")]
        public IActionResult GetStaffDetailsById(string staffCode)
        {
            try
            {
                this.token = new TokenDecryptionHelper(this.HttpContext);
                var staffinfo = repo.GetStaffDetail(staffCode, token.GetCompanyId);

                if (staffinfo == null)
                {
                    return Ok(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("staff/approvals")]
        public IActionResult GetStaffDetails()
        {
            try
            {
                this.token = new TokenDecryptionHelper(this.HttpContext);
                var staffinfo =  repo.GetStaffDetails(token.GetCompanyId);

                if (staffinfo.ToList() == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("staff/names")]
        public IActionResult GetStaff()
        {
            try
            {
                this.token = new TokenDecryptionHelper(this.HttpContext);
                var staffinfo = repo.GetStaffNames();

                if (staffinfo == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("approval-status")]
        public IActionResult GetApprovalStatus()
        {
            try
            {
                this.token = new TokenDecryptionHelper(this.HttpContext);
                var staffinfo = repo.GetApprovalStatus();

                if (staffinfo == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("staff/{staffId}")]
        public IActionResult GetStaffInfoById(int staffId)
        {
            try
            {
                this.token = new TokenDecryptionHelper(this.HttpContext);
                var staffInfo = repo.GetStaffById(staffId);
                return Ok(staffInfo);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = true, message = ex.Message });
            }
        }

        [HttpPost("staff")]
        public IActionResult AddTempStaff([FromBody] StaffInfoViewModel model)
        {
            try
            {

                if (repo.IsStaffCodeAlreadyExist(model.StaffCode))
                {
                    return Ok(new { success = false, message = $"A staff with {model.StaffCode} already exist" });
                }
                if (repo.IsStaffExist(model.StaffCode))
                {
                    return Ok(new { success = false, message = $"A staff with {model.StaffCode} already exist waiting for approval" });
                }
                               
                token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId =  (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                

                var username = token.GetUsername;
                var staffId = token.GetStaffId;
                var companyId = token.GetCompanyId; //etc

                //We can now use staffId extracted from the token as the created by
                //We ca also get companyId too

                model.createdBy  = staffId; ///This staff Id was gotten from the token
                

                var staff = repo.AddTempStaff(model);

                if (staff)
                {
                    return Ok(new { success = true, result = staff, message = "Staff has been created successfully, now waiting for approval" });
                }
                else
                    return Ok(new { success = false, message = "staff not created" });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("staff/{staffid}")]
        public IActionResult UpdateStaffInfo(int staffid, [FromBody] StaffInfoViewModel model)
        {
            try
            {

                token = new TokenDecryptionHelper(this.HttpContext);
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = Request.Path.Value;
                model.createdBy = token.GetStaffId;


                var staff = repo.UpdateStaff(staffid, model);
                if (staff)
                {
                    return Ok(new { success = true, result = staff, message = "Staff has been updated successfully, now waiting for approval" });
                }
                else
                    return Ok(new { success = false, message = "staff not created" });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("staff/{staffId}")]
        public IActionResult DeleteStaffInfo(int staffId)
        {
            
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = Request.Path.Value,
                    userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };
                var staff = repo.DeleteStaff(staffId, user);
                if (staff)
                {
                    return Ok(new { success = true, result = staff, message = "staff has been created successfully" });
                }
                else
                    return Ok(new { success = false, message = "staff not created" });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("staffbybranch/{branchId}")]
        public IActionResult GetStaffInfoByBranchId(int branchId)
        {
            try
            {
             
                var staffInfo = repo.GetAllStaff()
                 .SingleOrDefault(c => c.BranchId == branchId);
                if (staffInfo == null)
                {
                    return NotFound(new { success = false, message = "No record found" });
                }
                return Ok(new { success = true, result = staffInfo });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("staff/approval")]
        public IActionResult GoForApproval([FromBody]ApprovalViewModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);

                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId =  token.GetStaffId;
                entity.applicationUrl = Request.Path.Value;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                
                var response = repo.GoForApproval(entity);

                if (response)
                {
                    return Ok(new { success = true, message = "Staff record has been approved successfully" });
                }
                else
                    return Ok(new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (System.Exception ex)
            {
                this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
                return Ok(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("staff/search/{queryString}")]
        public IActionResult SearchStaff(string queryString)
        {
            try
            {
                token = new TokenDecryptionHelper(this.HttpContext);
                var data = repo.SearchStaff(queryString, token.GetCompanyId);
                return Ok(new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Ok(new { success = false, message = ex.Message });
            }
        }


    }



}