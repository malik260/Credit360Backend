//using FintrakBanking.Interfaces.CreditLimitValidations;
using System;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.CreditLimitValidations;
using System.Web;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.CreditLimitValidations;
using FintrakBanking.Common.CustomException;
using FintrakBanking.ViewModels.Setups.General;
using FintrakBanking.ViewModels;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/credit/limitvalidations")]
    public class CreditLimitValidationsController : ApiControllerBase
    {
        private ICreditLimitValidationsRepository repo;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public CreditLimitValidationsController(ICreditLimitValidationsRepository _repo)
        {
            this.repo = _repo;
        }



        //[HttpGet]
        //[Route("blacklist/{customerId}")]
        //public HttpResponseMessage ValidateBlackList(int customerId)
        //{ 
        //        try
        //        {
        //            var data = repo.ValidateBlackList(customerId);
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //        }
        //        catch (SecureException ex)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
        //        }

        //}
      [HttpGet] [ClaimsAuthorization]  
        [Route("blacklist/{customerCode}")]
        public HttpResponseMessage ValidateBlackList(string customerCode)
        {
            try
            {
                var data = repo.ValidateBlackList(customerCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("customer-eligibility/{customerCode}")]
        public HttpResponseMessage ValidateCustomerEligibility(string customerCode)
        {
            try
            {
                var data = repo.ValidateCustomerEligibility(customerCode);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record Found" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("get-customer-eligibility/{customerCode}")]
        public HttpResponseMessage GetCustomerEligibility(string customerCode)
        {
            var data = repo.GetCustomerEligibility(customerCode);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        //public CustomerEligibility GetCustomerEligibility(string customerCode)

        [HttpGet] [ClaimsAuthorization]  
        [Route("watchlist/{customerId}")]
        public HttpResponseMessage ValidateWatchList(int customerId)
        {
            try
            {
                var data = repo.ValidateWatchList(customerId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }


      //[HttpGet] [ClaimsAuthorization]  
      //  [Route("camsol/{customerId}")]
      //  public HttpResponseMessage ValidateCamsol(int customerId)
      //  {
      //      try
      //      {
      //          var data = repo.ValidateCamsol(customerId);
      //          return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
      //      }
      //      catch (SecureException ex)
      //      {
      //          return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
      //      }

      //  }


      [HttpGet] [ClaimsAuthorization]  
        [Route("validateamount/branch")]
        public HttpResponseMessage  ValidateAmountByBranch( )
        { 
                try
                {
            
                var data = repo.ValidateAmountByBranch((short)token.GetBranchId);
                    if (data != null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = true, message = "No record found" });
                    }
                }
                catch (SecureException ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = ex.Message });
                }
                  
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("validatenpl/branch")]
        public HttpResponseMessage ValidateNPLByBranch()
        {
            try
            {

                var data = repo.ValidateNPLByBranch((short)token.GetBranchId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }



      [HttpGet] [ClaimsAuthorization]  
        [Route("validateamount/segment/{segmentId}")]
        public HttpResponseMessage ValidateAmountBySegment(short segmentId)
        {
            try
            {

                var data = repo.ValidateAmountBySegment(segmentId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("validatenpl/segment/{segmentId}")]
        public HttpResponseMessage ValidateNPLBySegment(short segmentId)
        {
            try
            {

                var data = repo.ValidateNPLBySegment(segmentId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }


        [HttpGet] [ClaimsAuthorization]  
        [Route("validateamount/sector/{subSectorId}")]
        public HttpResponseMessage ValidateAmountBySector(int subSectorId)
        {
            
                var data = repo.ValidateAmountBySector(subSectorId);
                if (data != null)
                {
                    
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("validateamountfacility/sector/{sectorId}")]
        public HttpResponseMessage ValidateAmountFacilityBySector(int sectorId)
        {
                var data = repo.ValidateAmountFacilityBySector(sectorId);
                if (data != null)
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("validatenpl/sector/{subSectorId}")]
        public HttpResponseMessage ValidateNPLBySector(int subSectorId)
        {
            
                var data = repo.ValidateNPLBySector(subSectorId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("validateamount/customer/{customerId}")]
        public HttpResponseMessage ValidateAmountByCustomer(int customerId)
        {
            try
            {

                var data = repo.ValidateAmountByCustomer(customerId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("validatenpl/customer/{customerId}")]
        public HttpResponseMessage ValidateNPLByCustomer(int customerId)
        {
            try
            {

                var data = repo.ValidateNPLByCustomer(customerId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("validateamount/customergroup/{customerId}")]
        public HttpResponseMessage ValidateAmountByCustomerGroup(int customergroupId)
        {
            try
            {

                var data = repo.ValidateAmountByCustomerGroup(customergroupId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("validatenpl/customergroup/{customerId}")]
        public HttpResponseMessage ValidateNPLByCustomerGroup(int customergroupId)
        {
            try
            {

                var data = repo.ValidateNPLByCustomerGroup(customergroupId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("validatecreditlimitnpl/RMBM/{relationshipofficerId}")]
        public HttpResponseMessage ValidateCreditLimitByRMBM(short relationshipofficerId)
        {
            try
            {

                var data = repo.ValidateCreditLimitByRMBM(relationshipofficerId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, message = "No record found" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }
        #region Obligor Limit 
        [HttpGet]
        [ClaimsAuthorization]
        [Route("obligor-limit")]
        public HttpResponseMessage GetAllObligorLimit()
        {
            
            var response = repo.GetAllObligorLimit();
            if (response != null) { 
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }else
            
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("obligor-limit")]
        public HttpResponseMessage AddUpdateObligorLimit([FromBody] ObligorLimitViewModel entity)
        {
            try
            {
                string createUpdate = "";
                if (entity.riskRatingId != 0 || entity.riskRatingId > 0)
                {
                    createUpdate = "updated";
                }
                else
                {
                    createUpdate = "created";
                    if (repo.ValidateRiskRating(entity.riskRating))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                               new { success = false, message = "Risk Rating with same Name or Code already exist." });
                    }
                }
                entity.userBranchId = (short)token.GetBranchId;

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddUpdateRiskRating(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Changes Saved Successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Saved Changes not Successfull" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error saving this record {e.Message}" });
            }
        }

        [HttpPost]
        [Route("obligor-limit/{riskRatingId}")]
        public HttpResponseMessage DeleteObligorLimit([FromBody] ObligorLimitViewModel entity)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.BranchId = (short)token.GetBranchId;
                user.companyId = (short)token.GetCompanyId;
                user.applicationUrl = HttpContext.Current.Request.Path;
                user.staffId = token.GetStaffId;

                var data = repo.DeleteRiskRating(entity.riskRatingId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Changes deleted Successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"deleted Changes not Successfull" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error deleting this record {e.Message}" });
            }
        }

        [HttpPost]
        [Route("update-customer-rating")]
        public HttpResponseMessage UpdateCustomerRating([FromBody] ObligorLimitViewModel entity)
        {
            try
            {
                bool data = repo.UpdateCustomerRating(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        // cam

        [HttpPost]
        [Route("update-application-customer-rating")]
        public HttpResponseMessage UpdateApplicationCustomerRating([FromBody] ObligorLimitViewModel entity)
        {
            try
            {
                bool data = repo.UpdateApplicationCustomerRating(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("validate-application-customer-rating")]
        public HttpResponseMessage ValidateApplicationCustomerRating([FromBody] ObligorLimitViewModel entity)
        {
            CreditLimitValidationsModel data = repo.ValidateApplicationCustomerRating(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        #endregion

        [HttpPost]
        [ClaimsAuthorization]
        [Route("total-exposure-limit")]
        public HttpResponseMessage GetTotalExposureLimit([FromBody] ExposureLimitRequestModel entity)
        {
            entity.companyId = token.GetCompanyId;
            TotalExposureLimit data = repo.GetTotalExposureLimit(entity);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("total-exposure-limit/reference/{reference}")]
        public HttpResponseMessage GetTotalExposureLimitReference(string reference)
        {
            TotalExposureLimit data = repo.GetTotalExposureLimitReference(reference, token.GetCompanyId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }


        #region currency & group limits
        [HttpGet]
        [Route("currency-limit")]
        public HttpResponseMessage GetAllCurrencyLimit()
        {
            var response = repo.GetAllCurrencyLimit();
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("currency-limit")]
        public HttpResponseMessage AddCurrencyLimit([FromBody] CurrencyLimitViewModel entity)
        {
            
                entity.userBranchId = (short)token.GetBranchId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = (short)token.GetCompanyId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCurrencyLimits(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Record Saved Successfully" });
                }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("currency-limit/{currencyLimitId}")]
        public HttpResponseMessage UpdateCurrencyLimit(int currencyLimitId, [FromBody] CurrencyLimitViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;
            entity.currencyLimitId = currencyLimitId;

            var data = repo.UpdateCurrencyLimits(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }else
            return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("currency-limit/{currencyLimitId}")]
        public HttpResponseMessage DeleteCurrencyLimit(int currencyLimitId)
        {
                UserInfo user = new UserInfo();
                user.BranchId = (short)token.GetBranchId;
                user.companyId = (short)token.GetCompanyId;
                user.applicationUrl = HttpContext.Current.Request.Path;
                user.staffId = token.GetStaffId;

                var data = repo.DeleteCurrencyLimit(currencyLimitId, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "Record deleted Successfully" });
                }else
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"deleted Record not Successfull" });
        }

        [HttpGet]
        [Route("group-limit")]
        public HttpResponseMessage GetAllGroupLimit()
        {
            var response = repo.GetAllGroupLimit();
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("group-limit")]
        public HttpResponseMessage AddGroupLimit([FromBody] GroupLimitViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;

            var data = repo.AddGroupLimits(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("group-limit/{groupLimitId}")]
        public HttpResponseMessage UpdateGroupLimit(int groupLimitId, [FromBody] GroupLimitViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;
            entity.groupLimitId = groupLimitId;

            var data = repo.UpdateGroupLimits(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("group-limit/{groupLimitId}")]
        public HttpResponseMessage DeleteGroupLimit(int groupLimitId)
        {
            UserInfo user = new UserInfo();
            user.BranchId = (short)token.GetBranchId;
            user.companyId = (short)token.GetCompanyId;
            user.applicationUrl = HttpContext.Current.Request.Path;
            user.staffId = token.GetStaffId;

            var data = repo.DeleteGroupLimit(groupLimitId, user);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record deleted Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"deleted Record not Successfull" });
        }
        #endregion


        [HttpGet]
        [Route("project-risk-rating-categories")]
        public HttpResponseMessage GetAllProjectRiskRatingCategories()
        {
            var response = repo.getAllProjectRiskRatingCategories();
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }

        [HttpGet]
        [Route("all-criteria-list")]
        public HttpResponseMessage GetAllCriteriaList()
        {
            var response = repo.getAllCriteriaList();
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }

        [HttpPost]
        [Route("add-contractor-criteria")]
        public HttpResponseMessage AddContractorCriteria([FromBody] ContractorCriteriaViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;

            var data = repo.AddContractorCriteria(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpPost]
        [Route("add-ibl-checklist")]
        public HttpResponseMessage AddIBLChecklist([FromBody] IBLChecklistViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;

            var data = repo.AddIBLChecklist(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpPost]
        [Route("add-contractor-criteria-option")]
        public HttpResponseMessage AddContractorCriteriaOption([FromBody] ContractorCriteriaOptionViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;

            var data = repo.AddContractorCriteriaOption(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }
        [HttpPost]
        [Route("add-ibl-checklist-option")]
        public HttpResponseMessage AddIBLChecklistOption([FromBody] IBLChecklistOptionViewModel entity)
        {

            

            var data = repo.AddIBLChecklistOption(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpPut]
        [Route("update-contractor-criteria/{criteriaId}")]
        public HttpResponseMessage UpdateContractorCriteria(int criteriaId, [FromBody] ContractorCriteriaViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;
            entity.criteriaId = criteriaId;

            var data = repo.UpdateContractorCriteria(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }
        [HttpPut]
        [Route("update-ibl-checklist/{iblChecklistId}")]
        public HttpResponseMessage UpdateIBLChecklist(int iblChecklistId, [FromBody] IBLChecklistViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;
            entity.iblChecklistId = iblChecklistId;

            var data = repo.UpdateIBLChecklist(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpPut]
        [Route("update-contractor-criteria-option/{optionId}")]
        public HttpResponseMessage UpdateContractorCriteriaOption(int optionId, [FromBody] ContractorCriteriaOptionViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;
            entity.optionId = optionId;

            var data = repo.UpdateContractorCriteriaOption(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }
        [HttpPut]
        [Route("update-ibl-checklist-option/{optionId}")]
        public HttpResponseMessage UpdateIBLChecklistOption(int optionId, [FromBody] IBLChecklistOptionViewModel entity)
        {

          
            entity.optionId = optionId;

            var data = repo.UpdateIBLChecklistOption(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpPost]
        [Route("add-project-risk-criteria")]
        public HttpResponseMessage AddProjectRiskCriteria([FromBody] ProjectRiskRatingCriteriaViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;

            var data = repo.AddProjectRiskCriteria(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpPut]
        [Route("update-project-risk-criteria/{projectRiskRatingCriteriaId}")]
        public HttpResponseMessage UpdateProjectRiskCriteria(int projectRiskRatingCriteriaId, [FromBody] ProjectRiskRatingCriteriaViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;
            entity.projectRiskRatingCriteriaId = projectRiskRatingCriteriaId;

            var data = repo.UpdateProjectRiskCriteria(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpPost]
        [Route("add-project-risk-category")]
        public HttpResponseMessage AddProjectRiskCategory([FromBody] ProjectRiskRatingCategoryViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;

            var data = repo.AddProjectRiskCategory(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpPut]
        [Route("update-project-risk-category/{categoryId}")]
        public HttpResponseMessage UpdateProjectRiskCategory(int categoryId, [FromBody] ProjectRiskRatingCategoryViewModel entity)
        {

            entity.userBranchId = (short)token.GetBranchId;
            entity.userBranchId = (short)token.GetBranchId;
            entity.companyId = (short)token.GetCompanyId;
            entity.applicationUrl = HttpContext.Current.Request.Path;
            entity.createdBy = token.GetStaffId;
            entity.categoryId = categoryId;

            var data = repo.UpdateProjectRiskCategory(entity);
            if (data)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = data, message = "Record Saved Successfully" });
            }
            else
                return Request.CreateResponse(HttpStatusCode.OK,
               new { success = false, message = $"Saved Record not Successfull" });
        }

        [HttpGet]
        [Route("contractor-criteria")]
        public HttpResponseMessage GetAllContractorCriteria()
        {
            var response = repo.getAllContractorCriteria();
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }

        [HttpGet]
        [Route("ibl-checklist")]
        public HttpResponseMessage GetAllIBLChecklist()
        {
            var response = repo.getAllIBLChecklist();
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }

        [HttpGet]
        [Route("contractor-criteria-option")]
        public HttpResponseMessage GetAllContractorCriteriaOption()
        {
            var response = repo.getAllContractorCriteriaOption();
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }
        [HttpGet]
        [Route("ibl-checlist-option")]
        public HttpResponseMessage GetAllIBLChecklistOption()
        {
            var response = repo.getAllIBLCheclistOption();
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }

        [HttpGet]
        [Route("project-risk-rating-criteria")]
        public HttpResponseMessage GetAllProjectRiskCriteria()
        {
            var response = repo.getAllProjectRiskRatingCriteria();
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }

        [HttpGet]
        [Route("contractor-tiering/{loanApplicationId}/{customerId}")]
        public HttpResponseMessage GetContractorTieringByApplication(int loanApplicationId, int customerId)
        {
            var response = repo.getContractorTieringByApplication(loanApplicationId, customerId);
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }
        [HttpGet]
        [Route("ibl-checklist/{loanApplicationId}/{customerId}")]
        public HttpResponseMessage GetIBLChecklistDetailByApplication(int loanApplicationId, int customerId)
        {
            var response = repo.getIBLChecklistDetailByApplication(loanApplicationId, customerId);
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }

        [HttpGet]
        [Route("contractor-tiering-computation/{loanApplicationId}/{customerId}")]
        public HttpResponseMessage GettContractorTieringByApplicationAndCustomer(int loanApplicationId, int customerId)
        {
            var response = repo.getContractorTieringByApplicationAndCustomer(loanApplicationId, customerId);
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }
        [HttpGet]
        [Route("ibl-checklist-detail/{loanApplicationId}/{customerId}")]
        public HttpResponseMessage GettIBLChecklistDetailByApplicationAndCustomer(int loanApplicationId, int customerId)
        {
            var response = repo.getIBLChecklistDetailByApplicationAndCustomer(loanApplicationId, customerId);
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }


        [HttpGet]
        [Route("contractor-tiering-update/{contractorTieringId}")]
        public HttpResponseMessage GetContractorTieringForEdit(int contractorTieringId)
        {
            var response = repo.getContractorTieringForEdit(contractorTieringId);
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }
        [HttpGet]
        [Route("ibl-checklist-detail-update/{iblChecklistDetailId}")]
        public HttpResponseMessage getIBLChecklistDetailForEdit(int iblChecklistDetailId)
        {
            var response = repo.getIBLChecklistDetailForEdit(iblChecklistDetailId);
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }


        [HttpGet]
        [Route("all-project-risk-rating-criteria")]
        public HttpResponseMessage GetAllProjectRiskRatingByCategories()
        {
            var response = repo.getAllProjectRiskRatingByCategories();
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }


        [HttpGet]
        [Route("project-risk-rating/{loanApplicationId}/{loanApplicationDetailId}/{loanBookingRequestId}")]
        public HttpResponseMessage GetContractorTieringByApplication(int loanApplicationId, int loanApplicationDetailId, int loanBookingRequestId)
        {
            var response = repo.getProjectRiskRatingByApplicationDetailId(loanApplicationId, loanApplicationDetailId, loanBookingRequestId);
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }

        [HttpGet]
        [Route("project-risk-rating-computation/{loanApplicationId}/{loanApplicationDetailId}")]
        public HttpResponseMessage GetProjectRiskRatingByApplicationAndApplicationDetailId(int loanApplicationId, int loanApplicationDetailId)
        {
            var response = repo.getProjectRiskRatingByApplicationAndApplicationDetailId(loanApplicationId, loanApplicationDetailId);
            if (response != null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            else

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No records found" });
        }

    }
} 