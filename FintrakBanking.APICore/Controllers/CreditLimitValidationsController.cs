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

      [HttpGet] [ClaimsAuthorization]  
        [Route("customer-eligibility/{customerCode}")]
        public HttpResponseMessage ValidateCustomerEligibility(string customerCode)
        {
            try
            {
                var data = repo.ValidateCustomerEligibility(customerCode);
                if(data != null)
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
            try
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("validatenpl/sector/{subSectorId}")]
        public HttpResponseMessage ValidateNPLBySector(int subSectorId)
        {
            try
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
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
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
        [Route("obligor-limit")]
        public HttpResponseMessage GetAllObligorLimit()
        {
            try
            {
                var response = repo.GetAllObligorLimit();

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }
        }
        [HttpPost]
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

        [HttpGet]
        [ClaimsAuthorization]
        [Route("validate-application-customer-rating/{applicationId}")]
        public HttpResponseMessage ValidateApplicationCustomerRating(int applicationId)
        {
            CreditLimitValidationsModel data = repo.ValidateApplicationCustomerRating(applicationId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        }

        #endregion
    }
} 