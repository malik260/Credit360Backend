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
        public IEnumerable<ChargeVeiwModel> GetAllCharges()
        {
            token = new TokenDecryptionHelper();
            return repo.GetAllCharges(token.GetCompanyId);
        }

        [HttpGet]
        [Route("charges/{id}")]
        public ChargeVeiwModel GetAllCharges(  int  id)
        {
            token = new TokenDecryptionHelper();
            return repo.GetAllCharges(token.GetCompanyId, id);
        }

        [HttpGet]
        [Route("charges/operation/{id}")]        
        public IEnumerable<ChargeVeiwModel> GetAllChargeByOperation( int id)
        {
            token = new TokenDecryptionHelper();
            return repo.GetAllChargeByOperation(token.GetCompanyId,  id);
        }

        public Task<bool> AddChargeRange([FromBody]ChargeRangeVeiwModel entity)
        {
            return repo.AddChargeRange(entity);
        }

        public Task<bool> DeleteChargeRange([FromBody]ChargeRangeVeiwModel entity)
        {
            return repo.DeleteChargeRange(entity);
        }
        public ChargeRangeVeiwModel GetAllChargeRanges(int companyId, int rangeId)
        {
            token = new TokenDecryptionHelper();
            return repo.GetAllChargeRanges(token.GetCompanyId , rangeId);
        }
        public IEnumerable<ChargeRangeVeiwModel> GetAllChargesRange( )
        {
            token = new TokenDecryptionHelper();
            return repo.GetAllChargesRange(token.GetCompanyId);
        }



        public Task<bool> UpdateCharge([FromBody]ChargeVeiwModel entity)
        {
            return repo.UpdateCharge(entity);
        }

        public Task<bool> UpdateChargeRange([FromBody]ChargeRangeVeiwModel entity)
        {
            return repo.UpdateChargeRange(entity);
        }


        public Task<bool> DeleteChargeValueSource([FromBody]ChargesValueSourceVeiwModel entity)
        {
            return repo.DeleteChargeValueSource(entity);
        }

        public IEnumerable<ChargesValueSourceVeiwModel> GetAllChargesValueSource()
        {
            token = new TokenDecryptionHelper();
            return repo.GetAllChargesValueSource(token.GetCompanyId);
        }

        public ChargesValueSourceVeiwModel GetChargesValueSourceById( int valueSourceId)
        {
            token = new TokenDecryptionHelper();
            return repo.GetChargesValueSourceById(token.GetCompanyId, valueSourceId);
        }
        public Task<bool> UpdateChargeValueSource([FromBody]ChargesValueSourceVeiwModel entity)
        {
            return repo.UpdateChargeValueSource(entity);
        }
        public Task<bool> AddChargeValueSource([FromBody]ChargesValueSourceVeiwModel entity)
        {
            return repo.AddChargeValueSource(entity);
        }
    }
}
