//using FintrakBanking.Interfaces.CreditLimitValidations;
using System;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.CreditLimitValidations;

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
        //        catch (Exception ex)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
        //        }

        //}
        [HttpGet]
        [Route("blacklist/{customerCode}")]
        public HttpResponseMessage ValidateBlackList(string customerCode)
        {
            try
            {
                var data = repo.ValidateBlackList(customerCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("watchlist/{customerId}")]
        public HttpResponseMessage ValidateWatchList(int customerId)
        {
            try
            {
                var data = repo.ValidateWatchList(customerId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }


        [HttpGet]
        [Route("camsol/{customerId}")]
        public HttpResponseMessage ValidateCamsol(int customerId)
        {
            try
            {
                var data = repo.ValidateCamsol(customerId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }


        [HttpGet]
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
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = ex.Message });
                }
                  
        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }



        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }


        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }


        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }


        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }


        [HttpGet]
        [Route("validatecreditlimitnpl/RMBM/{relationshipofficerId}")]
        public HttpResponseMessage ValidateCreditLimitNPLByRMBM(short relationshipofficerId)
        {
            try
            {

                var data = repo.ValidateCreditLimitNPLByRMBM(relationshipofficerId);
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
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = ex.Message });
            }

        }

    }
} 