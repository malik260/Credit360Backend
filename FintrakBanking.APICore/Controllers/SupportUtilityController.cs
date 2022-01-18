using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels.SupportUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/support-utility")]
    public class SupportUtilityController: ApiControllerBase
    {
        private ISupportUtilityRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public SupportUtilityController(ISupportUtilityRepository _repo)
        {
            this.repo = _repo;
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-support-issue-type/{supportIssueTypeId}")]
        public HttpResponseMessage GetSupportIssueType(int supportIssueTypeId)
        {
            SupportUtilityViewModel response = repo.GetSupportIssueType(supportIssueTypeId);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-support-issue-type")]
        public HttpResponseMessage GetDocumentTypes()
        {
            IEnumerable<SupportUtilityViewModel> response = repo.GetAllSupportIssueType();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-customer-issue-by-param")]
        public HttpResponseMessage GetCustomersIssuesByParams(string searchParam, short? IssueTypeId)
        {
            IEnumerable<CustomerViewModels> response = repo.GetCustomersIssuesByParams(searchParam, IssueTypeId) ;
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("view-approval-trail/{searchString}")]
        public HttpResponseMessage GetApprovalTrail(string searchString)
        {
            var response = repo.GetApprovalTrail(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Approval trail for " + searchString, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("view-single-trail/{approvalTrailId}")]
        public HttpResponseMessage GetSingleTrail(int approvalTrailId)
        {
            var response = repo.GetSingleTrail(approvalTrailId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Approval trail for " + approvalTrailId, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("view-unique-operations/{searchString}")]
        public HttpResponseMessage GetUniqueOperations(string searchString)
        {
            var response = repo.GetDistinctOperations(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Approval trail for " + searchString, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("view-expected-workflow/{searchString}")]
        public HttpResponseMessage GetExpectedWorkFlow(int searchString)
        {
            var response = repo.GetExpectedWorkFlow(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Approval trail for " + searchString, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-business-rule/{approvalLevelId}")]
        public HttpResponseMessage GetBusinessRule( int approvalLevelId)
        {
            var response = repo.GetBusinessRule(approvalLevelId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Approval trail for " ,  result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-staff-record/{searchString}")]
        public HttpResponseMessage GetStaff(string searchString)
        {
            var response = repo.GetStaff(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Staff record for " + searchString, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-temp-staff-record/{searchString}")]
        public HttpResponseMessage GetStaffCompairRecord(string searchString)
        {
            var response = repo.GetStaffCompairRecord(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Temp Staff record for " + searchString, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-customer-record/{searchString}")]
        public HttpResponseMessage GetCustomer(string searchString)
        {
            var response = repo.GetSingleCustomerGeneralInfo(searchString);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "customer record for " + searchString, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-temp-customer-record/{customerId}")]
        public HttpResponseMessage GetTempCustomerRecord(int customerId)
        {
            var response = repo.GetTempCustomerRecord(customerId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "customer record for " + customerId, result = response });

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-casa-customer-record/{customerId}")]
        public HttpResponseMessage GetCasaCustomerRecord(int customerId)
        {
            var response = repo.GetCasaCustomerRecord(customerId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "customer record for " + customerId, result = response });

        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("update-customer-record/{customerId}")]
        public HttpResponseMessage UpdateCustomerRecord(int customerId, CustomerViewModels entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                entity.customerSensitivityLevelId = 1;

                var data = repo.UpdateCustomerRecord(customerId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "The record has been updated successfully." });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error updating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = ce.Message });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this record {e.Message}" });
            }

        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("update-staff-record/{staffId}/{staffCode}")]
        public HttpResponseMessage UpdateStaffRecord( int staffId, string staffCode, StaffInfoViewModel staffModel)
        {
            try
            {
                staffModel.userBranchId = (short)token.GetBranchId;
                staffModel.companyId = (short)token.GetCompanyId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                staffModel.applicationUrl = HttpContext.Current.Request.Path;
                staffModel.createdBy = token.GetStaffId;
                staffModel.customerSensitivityLevelId = 1;

                var data = repo.UpdateStaffRecord( staffId, staffCode, staffModel);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "The record has been updated successfully." });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error updating this record" });
            }
            catch (ConditionNotMetException ce)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                  new { success = false, message = ce.Message });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this record {e.Message}" });
            }

        }
    }
}
