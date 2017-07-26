using FintrakBanking.APICore.core;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [Route("api/v1/setups")]
    public class GeneralSetupController : ApiControllerBase
    {
        private IGeneralSetupRepository repo;

        public GeneralSetupController(IGeneralSetupRepository _repo)
        {
            this.repo = _repo;
        }

        #region General Setups

        [HttpGet]
        [Route("calculate-maturity-date/effective-date/{effectiveDate}/tenor-mode/{tenorModeId}/tenor/{tenor}")]
        public HttpResponseMessage GetMaturityDate(HttpRequestMessage request, DateTime effectiveDate, short tenorModeId, int tenor)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    //var token = new TokenDecryptionHelper(this.HttpContext);
                    var data = repo.CalculateMaturityDate(effectiveDate, (TenorModeEnum)tenorModeId, tenor);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data, count = 1 }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("tenor-mode")]
        public HttpResponseMessage GetAllTenorMode(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllTenorMode();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("currency")]
        public HttpResponseMessage GetAllCurrency(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllCurrency();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("customer-type")]
        public HttpResponseMessage GetAllCustomerType(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllCustomerType();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }


        [HttpGet]
        [Route("deal-classification-type")]
        public HttpResponseMessage GetAllDealClassificationType(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllDealClassificationType();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("application-date")]
        public HttpResponseMessage GetApplicaionDate(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetApplicaionDate();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("day-count")]
        public HttpResponseMessage GetAllDayCount(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllDayCount();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));//Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("fee-amortisation-type")]
        public HttpResponseMessage GetAllFeeAmortisationType(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllFeeAmortisationType();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("deal-types")]
        public HttpResponseMessage GetAllDealTypes(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {

                try
                {
                    var data = repo.GetAllDealTypes();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("fs-types")]
        public HttpResponseMessage GetAllFSTypes(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllFSTypes();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("frequency-types")]
        public HttpResponseMessage GetAllFrequencyTypes(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllFrequencyTypes();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("operation-types")]
        public HttpResponseMessage GetAllOperationTypes(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllOperationTypes();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                    }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("operation")]
        public HttpResponseMessage GetAllOperations(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetAllOperations();
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() })); //Ok(accounts);
                        }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("operation/{operationTypeId}")]
        public HttpResponseMessage GetOperations(HttpRequestMessage request, short operationTypeId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.GetOperations(operationTypeId);
                    if (data == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));  //Ok(accounts);
                            }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        //[HttpGet]
        //[Route("casa/account-status")]
        //public HttpResponseMessage GetCasaAccountStatus(HttpRequestMessage request)
        //{
        //    HttpResponseMessage response = null;
        //    return GetHttpResponse(request, () =>
        //    {
        //        try
        //        {
        //            var data = repo.GetCasaAccountStatus();
        //            if (data == null)
        //            {
        //                response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
        //            }
        //            response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data.ToList() }));
        //        }
        //        catch (System.Exception ex)
        //        {
        //            response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
        //        }
        //        return response;
        //    });
        //}




        //[HttpGet("productgroups", Name = "GroupGet")]
        //public IActionResult GetProductGroup()
        //{
        //    try
        //    {
        //        var productGroups = repo.GetAllProductGroup();
        //        return Ok(productGroups);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return BadRequest(new { error = true, message = ex.Message });
        //    }
        //}

        //[HttpPost("productgroup/add")]
        //public IActionResult SaveProductGroup([FromBody]ProductGroupViewModel model)
        //{
        //    try
        //    {
        //        if (repo.SaveProductGroup(model))
        //        {
        //            return Created("", model);
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        return BadRequest();
        //    }

        //    return BadRequest();
        //}

        #endregion General Setups
    }
}