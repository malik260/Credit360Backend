using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.Repositories;
using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.ViewModels.Business;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors("AllDomain")]
    [Route("api/v1/credit")]
    public class LoanApplicationController : BaseController
    {
        private ILoanApplicationRepository repoApply;

        public LoanApplicationController(ILoanApplicationRepository _repoApply)
        {
            this.repoApply = _repoApply;
        }

        #region Loan Application
        [HttpGet("loan-application")]
        public IActionResult GetAllLoanApplications()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repoApply.GetAllLoanApplications(token.GetCompanyId);
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

        [HttpGet("loan-application/{id}")]
        public IActionResult GetLoanApplicationById(int id)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repoApply.GetLoanApplicationById(id,token.GetCompanyId);
                if (response!= null)
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("loan-product-class")]
        public IActionResult GetProductClass()
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repoApply.GetProductClass();
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

        
        [HttpGet("loan-application/search/{searchCriteria}")]
        public IActionResult FindLoan(string searchCriteria)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                var response = repoApply.FindLoanApplication(searchCriteria, token.GetCompanyId);

                return Ok(new { success = true, result = response, count = 1 });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        //[HttpDelete("loan-application/{aid}/approval-status/{id}")]
        //public IActionResult UpdateApprovalStatus(int aid, ApprovalStatusEnum id)
        //{
        //    try
        //    {
        //        var token = new TokenDecryptionHelper(this.HttpContext);

        //        UserInfo user = new UserInfo()
        //        {
        //            BranchId = token.GetBranchId,
        //            companyId = token.GetCompanyId,
        //            staffId = token.GetStaffId,
        //            applicationUrl = Request.Path.Value,
        //            userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
        //        };

        //        repoApply.UpdateApprovalStatus(aid,id, user);

        //        return Ok(new { success = true, result = id, message = "record has been deleted successfully" });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}


        [HttpPost("loan/application")]
        public async Task<IActionResult> LoanBooking([FromBody] LoanApplicationViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);

                entity.userBranchId = (short)token.GetBranchId;
                entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = Request.Path.Value;
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.branchId = (short)token.GetBranchId;

                entity.misCode = "001";
                entity.teamMiscode = "004";

                var response = await repoApply.CreateLoanApplication(entity);
                if (response)
                {
                    return Ok(new { success = true, message = "Operation completed successfully" });
                }

                return Ok(new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet("loan/application/pending")]
        public IActionResult GetAllPendingLoanApplications([FromQuery] int page, [FromQuery] int itemsPerPage)
        {
            try
            {
                var token = new TokenDecryptionHelper(this.HttpContext);
                var response = repoApply.GetAllLoanApplications(token.GetCompanyId).Where(x => x.approvalStatusId == (int)ApprovalStatusEnum.Pending).ToList();
                int totalItems = response.Count();
                response = response.OrderBy(x=>x.applicationDate).Skip(page).Take(itemsPerPage).ToList();
                if (!response.Any())
                {
                    return Ok(new { success = false, message = "No record found" });
                }

                return Ok(new { success = true, result = response, totalItems = totalItems });
            }
            catch (Exception e)
            {
                return Ok(new { success = false, message = $"Error: {e.Message}" });
            }
        }

        #endregion


    }
}