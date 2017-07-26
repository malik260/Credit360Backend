using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Net;
using System.Web;
using FintrakBanking.APICore.core;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/credit")]
    public class CollateralController : ApiControllerBase
    {
        private ICollateralCustomerRepository repo;
        IErrorLogRepository errorLogger;
        public CollateralController(ICollateralCustomerRepository _repo, IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            errorLogger = _errorLogger;
        }

        #region Collateral 
        [HttpPost] [Route("collateral-customer")]
        public HttpResponseMessage AddCollateral([FromBody] CollateralViewModel entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                //entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.companyId = token.GetCompanyId;

                var data = repo.AddCollateral(entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpPut][Route("collateral-customer/{collateralCustomerId}")]
        //public HttpResponseMessage UpdateCustomCollateral( int collateralCustomerId, [FromBody] CollateralCustomer entity)
        //{
        //        try
        //        {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();
        //            entity.lastUpdatedBy = token.GetStaffId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.userBranchId = (short)token.GetBranchId;
        //            //entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

        //            var data = repo.UpdateCollateralCustomer(collateralCustomerId, entity).IsCompleted;
        //            if (!data)
        //            {
        //            return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = "No record found" });
        //            }
        //        return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data });
        //        }
        //        catch (System.Exception ex)
        //        {
        //        return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
        //        }

        //}

        //[HttpDelete] [Route("collateral-customer/{collateralCustomerId}")]
        //public HttpResponseMessage DeleteCollateralCustomer(int collateralCustomerId)
        //{
        //        try
        //        {
        //            TokenDecryptionHelper token = new TokenDecryptionHelper();

        //            UserInfo user = new UserInfo()
        //            {
        //                BranchId = token.GetBranchId,
        //                companyId = token.GetCompanyId,
        //                staffId = token.GetStaffId,
        //                applicationUrl = HttpContext.Current.Request.Path,
        //                //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
        //            };

        //            var data = repo.DeleteCollateralCustomer(collateralCustomerId, user).IsCompleted;
        //            if (data)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "Created successfully" });
        //            }

        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
        //        }
        //        catch (Exception ex)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //        }
        //    }
        //}

        //[HttpGet][Route("collateral-customer/customer/{customerId}")]
        //public HttpResponseMessage GetCollateralCustomer(int customerId)
        //{
        //    try
        //    {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();

        //        var data = repo.GetCollateralCustomerByCustomerId(customerId, token.GetCompanyId);
        //        if (data == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        #endregion Collateral 

        #region Seniority Of Claims
        [HttpGet] [Route("collateral-seniority-of-claims")]
        public HttpResponseMessage GetCollateralSeciorityOfClaims()
        {
            try {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetCollateralSeniorityOfClaims();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion Seniority Of Claims


        //#region Listing Functions
        //[HttpGet][Route("collateral-value-base-type")]
        //public HttpResponseMessage GetCollateralValueBaseType()
        //{
        //try
        //      {
        //            TokenDecryptionHelper token = new TokenDecryptionHelper(this.HttpContext);

        //            var data = repo.GetCollateralValueBaseType();
        //            if (data == null)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //            }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //        }
        //        catch (System.Exception ex)
        //        {
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //        }
        //}

        //[HttpGet][Route("collateral-valuers")]
        //public HttpResponseMessage GetCollateralValuers()
        //{
        //try
        //        {
        //            TokenDecryptionHelper token =  new TokenDecryptionHelper();

        //            var data = repo.GetCollateralValuers(token.GetCompanyId);
        //            if (data == null)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //        }
        //        catch (System.Exception ex)
        //        {
        //             return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //        }

        //}

        //[HttpGet][Route("collateral-valuer-type")]
        //public HttpResponseMessage GetCollateralValuerType()
        //{
        //try
        //{
        //    TokenDecryptionHelper token = new TokenDecryptionHelper();

        //    var data = repo.GetCollateralValuerType();

        //    if (data == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //    }
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //}
        //catch (System.Exception ex)
        //{
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //}
        //}

        //[HttpGet("collateral-policy/customer/{collateralCustomerId}")]
        //public HttpResponseMessage GetCollateralCustomerPolicyByCollateralCustomerId(short collateralCustomerId)
        //{
        //    try
        //    {
        //        token = new TokenDecryptionHelper(this.HttpContext);

        //        var response = repo.GetCollateralCustomerPolicyByCollateralCustomerId(collateralCustomerId);
        //        if (response == null)
        //        {
        //            return Ok(new { success = false, message = "No record found" });
        //        }
        //        return Ok(new { success = true, result = response });
        //    }
        //    catch (System.Exception ex)
        //    {
        //        this.errorLogger.LogError(ex, this.Request.Path.Value, token.GetUsername);
        //        return Ok(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpGet][Route("collateral-sub-type/{collateralSubTypeId}")]
        //public HttpResponseMessage GetCollateralSubTypeById(short collateralSubTypeId)
        //{
        //try
        //{
        //     TokenDecryptionHelper token =  new TokenDecryptionHelper();

        //     var data = repo.GetCollateralSubTypeById(collateralSubTypeId);
        //     if (data == null)
        //     {
        //           return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //     }
        //     return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //}
        //catch (System.Exception ex)
        //{
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //}
        //}


        //[HttpGet][Route("collateral-sub-type/collateral-type/{collateralTypeId}")]
        //public HttpResponseMessage GetCollateralSubTypeByCollateralTypeId(short collateralTypeId)
        //{
        //        try
        //        {
        //            TokenDecryptionHelper token = new TokenDecryptionHelper();

        //    var data = repo.GetCollateralSubTypeByCollateralTypeId(collateralTypeId);
        //            if (data == null)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //            }
        //             return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
        //        }
        //        catch (System.Exception ex)
        //        {
        //    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //        }
        //}
        //#endregion End of Listing Functions
    }
}
