using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setup")]
    public class DepartmentController : ApiControllerBase
    {
        private IDepartmentRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public DepartmentController(IDepartmentRepository _repo)
        {
            repo = _repo;
        }

        [HttpPost]
        [Route("department")]
        public HttpResponseMessage AddDepartment(DepartmentViewModel entity)
        {
            if (entity == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Empty Record" });
            }

            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                var department = repo.AddDepartment(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = department, message = "The record has been created successfully" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("department")]
        public HttpResponseMessage DeleteDepartment(int departmentId)
        {
            var account = repo.GetDepartment(departmentId);
            if (account == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            try
            {
                var depart = repo.DeleteDepartment(departmentId);

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = departmentId, message = "account has been deleted successfully" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("department")]
        public HttpResponseMessage GetAllDepartment()
        {
            var Message = string.Empty;
            try
            {
                var department = repo.GetAllDepartment().ToList();
                if (department.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = department, count = department.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No department found" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }

        [HttpGet]
        [Route("department/units{departmentId}")]
        public HttpResponseMessage GetAllDepartmentUnits(short departmentId)
        {
            var Message = string.Empty;
            try
            {
                var departmentUnits = repo.GetAllDepartmentUnits(departmentId).ToList();
                if (departmentUnits.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = departmentUnits, count = departmentUnits.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No department found" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }

        [HttpGet]
        [Route("department/units")]
        public HttpResponseMessage GetAllUnits()
        {
            var Message = string.Empty;
            try
            {
                var units = repo.GetAllUnits().ToList();
                if (units.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = units, count = units.Count });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No department found" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, Message = e.Message });
            }
        }

        [HttpGet]
        [Route("department-staff/")]
        public HttpResponseMessage SearchForDepartmentStaff(string searchQuery, int departmentId)
        {
            try
            {
                var data = repo.SearchForDepartmentStaff(token.GetCompanyId, searchQuery);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("department-staff/{departmentId}/")]
        public HttpResponseMessage SearchForDepartmentbyStaffId(string searchQuery, int departmentId)
        {
            try
            {
                var data = repo.SearchForDepartmentStaff(token.GetCompanyId, searchQuery, departmentId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                      new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("department/search")]
        public HttpResponseMessage SearchDepartment(string q, string t)
        {
            try
            {

                var data = repo.SearchDepartment(int.Parse(t), token.GetCompanyId, q).ToList();
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

        [HttpPost]
        [Route("department/{departmentId}")]
        public HttpResponseMessage GetDepartment(int departmentId)
        {
            var account = repo.GetDepartment(departmentId);
            if (account == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            }

            try
            {
                var depart = repo.GetDepartment(departmentId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = depart });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

     

        [HttpPut]
        [Route("department/{departmentId}")]
        public HttpResponseMessage UpdateDepartment(int departmentId, DepartmentViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                var data = repo.UpdateDepartment(departmentId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {data}" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {e.Message}" });
            }
        }

        [HttpPut]
        [Route("unit/{unitId}")]
        public HttpResponseMessage UpdateUnit(int unitId, DepartmentViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = token.GetStaffId;
                var data = repo.UpdateDepartment(unitId, entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {data}" });
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error updating this group {e.Message}" });
            }
        }

        [HttpGet, Route("department/{id}/staff")]
        public HttpResponseMessage GetAllDepartmentStaff(int id)
        {
            try
            {
                var staff = repo.GetAllDepartmentStaff(id).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staff, count = staff.Count });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet, Route("department/user-department")]
        public HttpResponseMessage GetUserDepartment()
        {
            try
            {
                var data = repo.GetStaffDepartment(token.GetStaffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }
    }
}