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

    [RoutePrefix("api/v1/customer")]
    public class CustomerController : ApiControllerBase
    {
        private ICustomerRepository repo;

        public CustomerController(ICustomerRepository _repo)
        {
            this.repo = _repo;
        }


        [HttpPost]
        [Route("")]
        public HttpResponseMessage AddCustomer([FromBody]CustomerViewModels entity)
        {
            try
            {
                TokenDecryptionHelper token = null;

                entity.userBranchId = (short)token.GetBranchId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.AddCustomer(entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

        [HttpDelete]
        [Route("{customerId}")]
        public HttpResponseMessage DeleteCustomer(int customerId)
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
                    return Request.CreateResponse(HttpStatusCode.OK,
                          new { success = true, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpGet]
        [Route("{customerId}")]
        public HttpResponseMessage GetCustomer(int custormerId)
        {

            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var data = repo.GetCustomer(custormerId);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-by-branch")]
        public HttpResponseMessage GetCustomerByBranchId()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.GetCustomerByBranchId(token.GetBranchId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        //[HttpGet] [Route("customer-by-branch")]
        //public HttpResponseMessage GetCustomerByBranchId()
        //{
        //    try
        //    {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();

        //        var data = repo.GetCustomerByBranchId(token.GetBranchId);
        //        if (!data.Any())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        //    }
        //    catch (Exception e)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
        //    }

        //}

        [HttpGet]
        [Route("customer")]
        public HttpResponseMessage SearchCustomer(string search)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.CustomerSearch(token.GetCompanyId, search);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        [Route("customer-search")]
        public HttpResponseMessage SearchCustomer([FromBody] CustomerSearchItemViewModels search)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                var data = repo.CustomerSearch(token.GetCompanyId, search);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-by-company/{companyId}")]
        public HttpResponseMessage GetCustomerByCompanyId(int companyId)
        {
            try
            {
                var data = repo.GetCustomerByCompanyId(companyId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("customer-by-customer-type/{customertypeId}")]
        public HttpResponseMessage GetCustomerByType(int customertypeId)
        {
            try
            {
                var data = repo.GetCustomerByTypeId(customertypeId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }

        }

        [HttpGet]
        [Route("customertype")]
        public HttpResponseMessage GetCustomerType()
        {
            try
            {
                var data = repo.GetCustomerType();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        [Route("{customerId}")]
        public HttpResponseMessage UpdateCustomer(int customerId, CustomerViewModels entity)
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();
                entity.userBranchId = (short)token.GetBranchId;
                //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;

                var data = repo.UpdateCustomer(customerId, entity).IsCompleted;
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }

        }

        [HttpGet]
        [Route("sectors")]
        public HttpResponseMessage GetAllSectors()
        {
            try
            {
                var data = repo.GetCustomerSectors();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [Route("subsector/{subSectorId}/sectors")]
        public HttpResponseMessage GetAllSectorsBySubSectorId(short subSectorId)
        {
            try
            {
                var data = repo.GetCustomerSectorBySubSectorId(subSectorId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }
    }
}
