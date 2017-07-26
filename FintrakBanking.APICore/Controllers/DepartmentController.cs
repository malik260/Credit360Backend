using FintrakBanking.APICore.core;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    //[EnableCors("AllDomain")]
    [RoutePrefix("api/v1/setup")]
    public class DepartmentController : ApiControllerBase
    {
        private IDepartmentRepository repo;

        public DepartmentController(IDepartmentRepository _repo)
        {
            repo = _repo;
        }

        [HttpPost]
        [Route("department")]
        public HttpResponseMessage AddDepartment(HttpRequestMessage request, DepartmentViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                if (entity == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "empty record" }));
                }

                try
                {
                    var department = repo.AddDepartment(entity);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = department }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpPut]
        [Route("department")]
        public HttpResponseMessage DeleteDepartment(HttpRequestMessage request, int departmentId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                var account = repo.GetDepartment(departmentId);
                if (account == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                }

                try
                {
                    var depart = repo.DeleteDepartment(departmentId);

                    response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = departmentId, message = "account has been deleted successfully" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpGet]
        [Route("department")]
        public HttpResponseMessage GetAllDepartment(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                var Message = string.Empty;
                try
                {
                    var department = repo.GetAllDepartment().ToList();
                    if (department.Any())
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                                    Ok(new { success = true, result = department, count = department.Count }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                                Ok(new { success = false, message = "No department found" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, Message = e.Message }));
                }
                return response;
            });
        }

        [HttpPost]
        [Route("department/{departmentId}")]
        public HttpResponseMessage GetDepartment(HttpRequestMessage request, int departmentId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                var account = repo.GetDepartment(departmentId);
                if (account == null)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                }

                try
                {
                    var depart = repo.GetDepartment(departmentId);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = depart }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }
                return response;
            });
        }

        [HttpPost]
        [Route("department/{departmentId}")]
        public HttpResponseMessage UpdateDepartment(HttpRequestMessage request, int departmentId, DepartmentViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var data = repo.UpdateDepartment(departmentId, entity);
                    if (data)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = data }));
                    }

                    response = request.CreateResponse(HttpStatusCode.OK,
                                        Ok(new { success = false, message = $"There was an error updating this group {response}" }));
                }
                catch (Exception e)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                                        Ok(new { success = false, message = $"There was an error updating this group {e.Message}" }));
                }
                return response;
            });
        }
    }
}