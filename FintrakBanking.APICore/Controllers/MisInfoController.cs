using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using FintrakBanking.APICore.core;
using System.Web;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    // [EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setup")]
    public class MisInfoController : ApiControllerBase
    {
        private IMisInfoRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        public MisInfoController(IMisInfoRepository _repo)
        {
            repo = _repo;
        }

        #region MisInfo

         [HttpPost] [ClaimsAuthorization][Route("misInfo")]
        public async Task<HttpResponseMessage> AddMisInfo(MisInfoViewModel entity)
        {
            try
            {
                var misInfo = await repo.AddMisInfo(entity);
                if (misInfo)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "mis not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete] [ClaimsAuthorization][Route("misinfo/{misInfoId}")]
        public async Task<HttpResponseMessage> DeleteMisInfo(int misInfoId)
        {
            try
            {
                var misInfo = await repo.DeleteMisInfo(misInfoId);
                if (misInfo)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "mis not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  [Route("misinfo")]
        public HttpResponseMessage GetAllMisInfo()
        {
            try
            {
                var misInfo = repo.GetAllMisInfo();
                if (misInfo != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "mis not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


      [HttpGet] [ClaimsAuthorization]  [Route("misinfo/company")]
        public HttpResponseMessage GetMisInfoByCoyId()
        {
            
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var misInfo = repo.GetMisInfoByCompanyId(token.GetCompanyId);
                if (misInfo != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = misInfo });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "mis not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  [Route("misinfo/{misInfoId}")]
        public HttpResponseMessage GetMisInfoById(int misInfoId)
        {
            try
            {
                var misInfo = repo.GetMisInfoById(misInfoId);
                if (misInfo != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "mis not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization][Route("misinfo/{misinfoid}")]
        public async Task<HttpResponseMessage> UpdateMisInfo(int misinfoid, [FromBody] MisInfoViewModel entity)
        {
            try
            {
                var misInfo = await repo.UpdateMisInfo(misinfoid, entity);
                if (misInfo)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "mis not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion MisInfo

        #region MisType

         [HttpPost] [ClaimsAuthorization][Route("mistype")]
        public async Task<HttpResponseMessage> AddMisTypeAsync(MisTypeViewModel entity)
        {
            try
            {
                var misInfo = await repo.AddMisType(entity);
                if (misInfo)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = misInfo, message = "mis has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "mis not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete] [ClaimsAuthorization][Route("mistype/{mistypeid}")]
        public async Task<HttpResponseMessage> DeleteMisTypeAsync(int misInfoId)
        {
            try
            {
                var misType = await repo.DeleteMisType(misInfoId);
                if (misType)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = misType, message = "mis has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "mis not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  [Route("mistype")]
        public HttpResponseMessage GetAllMisType()
        {
            try
            {
                var misType = repo.GetAllMisType(); ;
                if (misType != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = misType, message = "mis has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "mis not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  [Route("mistype/{mistypeid}")]
        public HttpResponseMessage GetMisTypeById(int misInfoId)
        {
            try
            {
                var misType = repo.GetMisTypeById(misInfoId);
                if (misType != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = misType, message = "mis has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "mis not created" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       [HttpPut] [ClaimsAuthorization][Route("mistype/{mistypeid}")]
        public async Task<HttpResponseMessage> UpdateMisType(int mistypeid, [FromBody] MisTypeViewModel entity)
        {
            if (entity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Bad Request" });
            }

            var account = await repo.UpdateMisType(mistypeid, entity);
            if (account)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            try
            {
                await repo.UpdateMisType(mistypeid, entity);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "account has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion MisType
    }
}