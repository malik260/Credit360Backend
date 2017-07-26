using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class FeeController : ApiControllerBase
    {
        private IFeeRepository repo;

        public FeeController(IFeeRepository _repo)
        {
            this.repo = _repo;
        }

        #region Product Fee
        [HttpGet][Route("fee")]
        public HttpResponseMessage GetAllFee()
        {
            try
            {
                var data = repo.GetAllFee();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet][Route("fee/{feeId}")]
        public HttpResponseMessage GetFeeById( int feeId)
        {
            try
            {
                var data = repo.GetFeeViewModel(feeId);
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

        [HttpPost][Route("fee")]
        public HttpResponseMessage AddFee([FromBody] FeeViewModel model)
        {
                try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                model.userBranchId = (short)token.GetBranchId;
                //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;

                var data = repo.AddFee(model);
                if (data >= 1)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "product fee has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "product fee not created" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut][Route("fee/{feeId}")]
        public HttpResponseMessage UpdateFee( int feeId, [FromBody] FeeViewModel model)
        {
            if (model == null)
                Request.CreateResponse(HttpStatusCode.OK, BadRequest());

                try
            {
                    TokenDecryptionHelper token = null;

                    model.userBranchId = (short)token.GetBranchId;
                    //model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;

                    var data = repo.GetFeeViewModel(feeId);
                    if (data == null)
                    {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                    }
                    else
                    {
                        repo.UpdateFee(feeId, model);
                    }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = model.feeId, message = "product fee has been updated successfully" });
                }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion Product Fee

        [HttpGet][Route("fee/account-category")]
        public HttpResponseMessage GetFeeAccountCategory()
        {
            try
            {
                var data = repo.GetFeeAccountCategory();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet][Route("fee/fee-type")]
        public HttpResponseMessage GetFeeType()
        {
                try
            {
                var data = repo.GetFeeType();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet][Route("fee/fee-interval")]
        public HttpResponseMessage GetFeeInterval()
        {
                try
            {
                var data = repo.GetFeeInterval();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet][Route("fee/fee-target")]
        public HttpResponseMessage GetFeeTarget(HttpRequestMessage request)
        {
                try
            {
                var data = repo.GetFeeTarget();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}