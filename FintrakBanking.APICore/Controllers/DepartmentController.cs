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
        public HttpResponseMessage AddDepartment(  DepartmentViewModel entity)
        { 
                if (entity == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,  new { success = false, message = "empty record" });
                }

                try
                {
                    var department = repo.AddDepartment(entity);
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = department });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                } 
        }

        [HttpPut]
        [Route("department")]
        public HttpResponseMessage DeleteDepartment(  int departmentId)
        { 
                var account = repo.GetDepartment(departmentId);
                if (account == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                try
                {
                    var depart = repo.DeleteDepartment(departmentId);

                    return Request.CreateResponse(HttpStatusCode.OK,
                            new { success = true, result = departmentId, message = "account has been deleted successfully" });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
                } 
        }

        [HttpGet]
        [Route("department")]
        public HttpResponseMessage GetAllDepartment( )
        { 
                var Message = string.Empty;
                try
                {
                    var department = repo.GetAllDepartment().ToList();
                    if (department.Any())
                    {
                        return Request.CreateResponse(HttpStatusCode.OK,
                                     new { success = true, result = department, count = department.Count });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK,
                               new { success = false, message = "No department found" });
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
                } 
        }

        [HttpPost]
        [Route("department/{departmentId}")]
        public HttpResponseMessage GetDepartment(  int departmentId)
        { 
                var account = repo.GetDepartment(departmentId);
                if (account == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }

                try
                {
                    var depart = repo.GetDepartment(departmentId);
                    return Request.CreateResponse(HttpStatusCode.OK,  new { success = true, result = depart });
                }
                catch (System.Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,  new { success = false, message = ex.Message });
                } 
        }

        [HttpPost]
        [Route("department/{departmentId}")]
        public HttpResponseMessage UpdateDepartment(  int departmentId, DepartmentViewModel entity)
        {  try
                {
                    var data = repo.UpdateDepartment(departmentId, entity);
                    if (data)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = false, message = $"There was an error updating this group {data}" });
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                        new { success = false, message = $"There was an error updating this group {e.Message}" });
                } 
        }
    }
}