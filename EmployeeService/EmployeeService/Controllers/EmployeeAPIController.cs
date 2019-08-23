using EmployeeDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace EmployeeService.Controllers
{
    [RoutePrefix("Api/EmployeeRecord")]
    public class EmployeeAPIController : ApiController
    {
        EmployeeDBEntities entities = new EmployeeDBEntities();

        [HttpGet]
        [Route("AllEmployeeDetails")]
        public HttpResponseMessage Get(string gender = "ALL")
        {
            try
            {

                switch (gender.ToLower())
                {
                    case "all":
                        return Request.CreateResponse(HttpStatusCode.OK, entities.EmployeeDetails.ToList());
                    case "0":
                        return Request.CreateResponse(HttpStatusCode.OK, entities.EmployeeDetails.Where(e => e.Gender.ToLower() == "0").ToList());
                    case "1":
                        return Request.CreateResponse(HttpStatusCode.OK, entities.EmployeeDetails.Where(e => e.Gender.ToLower() == "1").ToList());
                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Value for gender my be All, Male or Female " + gender + " is invalid");

                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }
        }

        [HttpGet]
        [Route("GetEmployeeDetailsById/{employeeId}")]
        public HttpResponseMessage GetEmaployeeById(string employeeId)
        {

            int ID = Convert.ToInt32(employeeId);
            try
            {

                var entity = entities.EmployeeDetails.FirstOrDefault(e => e.EmpId == ID);
                if (entity != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, entity);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Employee with id = " + ID.ToString() + " not found");
                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }


        }

        [HttpPost]
        [Route("InsertEmployeeDetails")]
        public HttpResponseMessage PostEmaployee([FromBody] EmployeeDetail data)
        {
            try
            {
                entities.EmployeeDetails.Add(data);
                entities.SaveChanges();

                var message = Request.CreateResponse(HttpStatusCode.Created, data);
                message.Headers.Location = new Uri(Request.RequestUri + data.EmpId.ToString());
                return message;

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }


        }

        [HttpPut]
        [Route("UpdateEmployeeDetails")]
        public HttpResponseMessage PutEmaployeeMaster([FromBody] EmployeeDetail employee)
        {
            try
            {
                int id = employee.EmpId;

                var objEmp = entities.EmployeeDetails.FirstOrDefault(e => e.EmpId == id);
                if (objEmp != null)
                {
                    objEmp.EmpName = employee.EmpName;
                    objEmp.Address = employee.Address;
                    objEmp.EmailId = employee.EmailId;
                    objEmp.DateOfBirth = employee.DateOfBirth;
                    objEmp.Gender = employee.Gender;
                    objEmp.PinCode = employee.PinCode;
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, objEmp);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Employee with id = " + id.ToString() + " not found to update");

                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex);
            }

        }

        [HttpDelete]
        [Route("DeleteEmployeeDetails")]
        public HttpResponseMessage DeleteEmaployeeDelete(int id)
        {
            try
            {

                var entity = entities.EmployeeDetails.FirstOrDefault(e => e.EmpId == id);
                if (entity != null)
                {
                    entities.EmployeeDetails.Remove(entity);
                    entities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, entity);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Employee with id = " + id.ToString() + " not found to delete");
                }

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadGateway, ex);

            }


        }
    }
}
