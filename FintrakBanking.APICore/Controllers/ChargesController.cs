using FintrakBanking.Interfaces.Setups.Finance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.ViewModels.Setups.Finance;
using System.Threading.Tasks;
using FintrakBanking.APICore.JWTAuth;
using System.Web;
using FintrakBanking.Interfaces.ErrorLogger;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class ChargesController : ApiController
    {
        TokenDecryptionHelper token = null;
        private readonly IChargeRepository repo; IErrorLogRepository errorLogger;
        public ChargesController(IChargeRepository _repo, IErrorLogRepository _errorLogger) {
            this.repo = _repo;
            errorLogger = _errorLogger;
        }
      
        #region Charges
        [HttpPost]
        [Route("charges")]
        public HttpResponseMessage AddCharge([FromBody]ChargeVeiwModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                // model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;
                var data = repo.AddCharge(entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpDelete]
        [Route("charges")]
        public HttpResponseMessage DeleteCharge(ChargeVeiwModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                // model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;
                var result = repo.DeleteCharge(entity).IsCompleted;
                if (result)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = result, message = "Branch has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
              new { success = false, message = "There was an error saving this record" });

            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("charges")]
        public HttpResponseMessage GetAllCharges()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetAllCharges(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("charges/{id}")]
        public HttpResponseMessage  GetAllCharges(  int  id)
        { 
            try
            {
                //
                var token = new TokenDecryptionHelper();
                var data = repo.GetAllCharges(token.GetCompanyId, id);
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

        [HttpPut]
        [Route("charges")]
        public HttpResponseMessage UpdateCharge([FromBody]ChargeVeiwModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                // entity.userIPAddress =  //Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                var data = repo.UpdateCharge(entity);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been Update successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error Update this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error Update this record {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("charges/operation/{id}")]        
        public HttpResponseMessage  GetAllChargeByOperation( int id)
        {            
            try
            {
                //
                var token = new TokenDecryptionHelper();
                var data = repo.GetAllChargeByOperation(token.GetCompanyId, id);
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
        #endregion Charge End

        #region charge range
        [HttpPost]
        [Route("chargerange")]
        public HttpResponseMessage AddChargeRange([FromBody]ChargeRangeVeiwModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                // model.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;
                var data = repo.AddChargeRange(entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.Message}" });
            }
        }

        [HttpDelete]
        [Route("chargerange")]
        public Task<bool> DeleteChargeRange([FromBody]ChargeRangeVeiwModel entity)
        {
            return repo.DeleteChargeRange(entity);
        }

        [HttpGet]
        [Route("chargerrange/range/{id}")]
        public HttpResponseMessage GetAllChargeRanges( int id)
        {
            try
            { 
                var token = new TokenDecryptionHelper();
                var data = repo.GetAllChargeRanges(token.GetCompanyId, id);
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

        [HttpGet]
        [Route("chargerrange")]
        public HttpResponseMessage GetAllChargesRange( )
        { 
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetAllChargesRange(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("chargerange")]
        public HttpResponseMessage UpdateChargeRange([FromBody]ChargeRangeVeiwModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                // entity.userIPAddress =  //Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                var data = repo.UpdateChargeRange(entity);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been Update successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error Update this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error Update this record {ex.Message}" });
            }
        }
        #endregion Charges Value Source
        
        #region charge value source
        [HttpDelete]
        [Route("chargevaluesource")]
        public HttpResponseMessage DeleteChargeValueSource([FromBody]ChargesValueSourceVeiwModel entity)
        {
            return repo.DeleteChargeValueSource(entity);
        }

        [HttpGet]
        [Route("chargevaluesource/all")]
        public  HttpResponseMessage GetAllChargesValueSource()
        {
             
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetAllChargesValueSource(token.GetCompanyId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("chargevaluesource/{id}")]
        public HttpResponseMessage  GetChargesValueSourceById( int valueSourceId)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.GetChargesValueSourceById(token.GetCompanyId, valueSourceId);
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

        [HttpPut]
        [Route("chargevaluesource")]
        public HttpResponseMessage UpdateChargeValueSource([FromBody]ChargesValueSourceVeiwModel entity)
        { 
            try
            {
                token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                // entity.userIPAddress =  //Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                var data = repo.UpdateChargeValueSource(entity);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been Update successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error Update this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error Update this record {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("chargevaluesource")]
        public HttpResponseMessage AddChargeValueSource([FromBody]ChargesValueSourceVeiwModel entity)
        {
            try
            {
                token = new TokenDecryptionHelper();

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                // entity.userIPAddress =  //Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                var data = repo.AddChargeValueSource(entity);

                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been Update successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error Update this record" });
            }
            catch (Exception ex)
            {
                this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error Update this record {ex.Message}" });
            }
        }
        #endregion Charge Range
    }
}
