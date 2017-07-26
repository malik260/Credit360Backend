using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/customers")]
    public class CustomerController : ApiControllerBase
    {
        private ICustomerRepository repo;

        public CustomerController(ICustomerRepository _repo)
        {
            this.repo = _repo;
        }


        [HttpPost]
        [Route("customer")]
        public HttpResponseMessage AddCustomer(HttpRequestMessage request, [FromBody]CustomerViewModels entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
            {
                    TokenDecryptionHelper token = null;

                    entity.userBranchId = (short)token.GetBranchId;
                    //entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                    entity.applicationUrl = HttpContext.Current.Request.Path;
                    entity.createdBy = token.GetStaffId;

                var data =  repo.AddCustomer(entity).IsCompleted;
                if (data)
                {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Created("", new { success = true, result = data, message = "The record has been created successfully" }));
                }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "There was an error creating this record" }));
            }
            catch (Exception e)
            {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"There was an error creating this record {e.Message}" }));
            }
                return response;
            });
        }

        [HttpDelete]
        [Route("customer/{customerId}")]
        public HttpResponseMessage DeleteCustomer(HttpRequestMessage request, int customerId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
            {
                TokenDecryptionHelper token = null;
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    //userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
                };

                var data = repo.DeleteCustomer(customerId, user).IsCompleted;
                if (data)
                {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Created("", new { success = true, message = "The record has been created successfully" }));
                }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = "There was an error creating this record" }));
            }
            catch (Exception e)
            {

                    response = request.CreateResponse(HttpStatusCode.OK, 
                        Ok(new { success = false, message = $"There was an error creating this record {e.Message}" }));
            }
                return response;
            });
        }

        [HttpGet]
        [Route("customer/{customerId}")]
        public HttpResponseMessage GetCustomer(HttpRequestMessage request, int custormerId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
            {
                    TokenDecryptionHelper token = new TokenDecryptionHelper();

                    var data = repo.GetCustomer(custormerId);
                if (data != null)
                {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data }));
            }
            catch (Exception e)
            {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
            }
                return response;
            });
        }

        [HttpGet]
        [Route("customer-by-branch/{branchId}")]
        public HttpResponseMessage GetCustomerByBranchId(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
            {
                    TokenDecryptionHelper token = new TokenDecryptionHelper();
                    var data = repo.GetCustomerByBranchId(token.GetCompanyId);
                if (!data.Any())
                {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data, count = data.Count() }));
            }
            catch (Exception e)
            {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
            }
                return response;
            });
        }

        [HttpGet][Route("customer-by-branch")]
        public HttpResponseMessage GetCustomerByBranchId()
        {
                try
            {
                    TokenDecryptionHelper token = new TokenDecryptionHelper();

                    var data = repo.GetCustomerByBranchId(token.GetBranchId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = $"Error: {e.Message}" });
            }

        }

        [HttpGet]
        [Route("customer")]
        public HttpResponseMessage SearchCustomer(HttpRequestMessage request, string search)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
            {
                    TokenDecryptionHelper token = new TokenDecryptionHelper();
                    var data = repo.CustomerSearch(token.GetCompanyId, search);
                if (!data.Any())
                {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data, count = data.Count() }));
            }
            catch (Exception e)
            {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
            }
                return response;
            });
        }

        [HttpPost]
        [Route("customer-search")]
        public HttpResponseMessage SearchCustomer(HttpRequestMessage request, [FromBody] CustomerSearchItemViewModels search)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                
                try
                {
                    TokenDecryptionHelper token = new TokenDecryptionHelper();
                    var data = repo.CustomerSearch(token.GetCompanyId, search);
                    if (!data.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data, count = data.Count() }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("customer-by-company/{companyId}")]
        public HttpResponseMessage GetCustomerByCompanyId(HttpRequestMessage request, int companyId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
            {
                var data = repo.GetCustomerByCompanyId(companyId);
                if (!data.Any())
                {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data, count = data.Count() }));
            }
            catch (Exception e)
            {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
            }
                return response;
            });
        }

        [HttpGet]
        [Route("customer-by-customer-type/{customertypeId}")]
        public HttpResponseMessage GetCustomerByType(HttpRequestMessage request, int customertypeId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
            {
                var data = repo.GetCustomerByTypeId(customertypeId);
                if (!data.Any())
                {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                }

                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data, count = data.Count() }));
            }
            catch (Exception e)
            {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
            }
                return response;
            });
        }

        [HttpGet]
        [Route("customertype")]
        public HttpResponseMessage GetCustomerType(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
            {
                var data = repo.GetCustomerType();
                if (!data.Any())
                {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                }
                    response = request.CreateResponse(HttpStatusCode.OK, 
                        Ok(new { success = true, result = data, count = data.Count() }));
            }
            catch (Exception e)
            {
                    response = request.CreateResponse(HttpStatusCode.OK, 
                        Ok(new { success = false, message = $"Error: {e.Message}" }));
            }
                return response;
        });
        }

        [HttpPut]
        [Route("customer/{customerId}")]
        public HttpResponseMessage UpdateCustomer(HttpRequestMessage request, int customerId, CustomerViewModels entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                //entity.userIPAddress = Request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.UpdateCustomer(customerId, entity).IsCompleted;
                if (data)
                {
                        response = request.CreateResponse(HttpStatusCode.OK, 
                            Created("", new { success = true, result = data, message = "The record has been created successfully" }));
                }

                    response = request.CreateResponse(HttpStatusCode.OK, 
                        Ok(new { success = false, message = "There was an error creating this record" }));
            }
            catch (Exception e)
            {
                    response = request.CreateResponse(HttpStatusCode.OK, 
                        Ok(new { success = false, message = $"There was an error creating this record {e.Message}" }));
            }
                return response;
            });
        }
    }
}
